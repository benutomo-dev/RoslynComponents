using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AutomaticDisposeAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// AutomaticDisposeImpl属性を付与した型の定義はpartialである必要があります。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0001 = new DiagnosticDescriptor("SG0001", "AutomaticDisposeImpl属性を付与する型にはpartialキーワードが必要", "AutomaticDisposeImpl属性を付与した型の定義はpartialである必要があります。", "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);

        /// <summary>
        /// AutomaticDisposeImpl属性を付与した型はIDisposableを実装していなくてはなりません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0002 = new DiagnosticDescriptor("SG0002", "AutomaticDisposeImpl属性を付与する型にはIDisposableインターフェイスが必要", "AutomaticDisposeImpl属性を付与した型はIDisposableを実装していなくてはなりません。", "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);

        /// <summary>
        /// {0}(メンバ名)はIAsyncDisposableを実装していますが、{1}(メンバを含んでいる型名)にIAsyncDisposableが実装されていないため、常に同期メソッドのDisposeによって破棄されます。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0003 = new DiagnosticDescriptor("SG0003", "メンバの非同期破棄メソッドを利用するためにはIAsyncDisposableインターフェイスが必要", "{0}はIAsyncDisposableを実装していますが、{1}にIAsyncDisposableが実装されていないため、常に同期メソッドのDisposeによって破棄されます。", "Usage", DiagnosticSeverity.Warning, isEnabledByDefault: true);

        /// <summary>
        /// 不明な内部異常によって{0}に対するDisposeの自動実装に失敗しました。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG9998 = new DiagnosticDescriptor("SG9998", "ソース生成の失敗", "不明な内部異常によって{0}に対するDisposeの自動実装が失敗しました。", "Execution", DiagnosticSeverity.Error, isEnabledByDefault: true);

        /// <summary>
        /// ソースジェネレータが不明な内部異常によって停止しました。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG9999 = new DiagnosticDescriptor("SG9999", "異常終了", $"AutomaticDisposeGeneratorが不明な内部異常によって停止しました。", "Execution", DiagnosticSeverity.Error, isEnabledByDefault: true);


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            s_diagnosticDescriptor_SG0001,
            s_diagnosticDescriptor_SG0002,
            s_diagnosticDescriptor_SG0003,
            s_diagnosticDescriptor_SG9998,
            s_diagnosticDescriptor_SG9999
            );

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);

            context.RegisterSemanticModelAction(AnalyzeSemanticModel);
        }

        private static void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
        {
            var automaticDisposeImplAttributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(AutomaticDisposeGenerator.AutomaticDisposeImplAttributeFullyQualifiedMetadataName);

            var automaticDisposeImplModeAttributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(AutomaticDisposeGenerator.AutomaticDisposeImplModeAttributeFullyQualifiedMetadataName);

            var automaticDisposeImplDesignations = context.SemanticModel.SyntaxTree.GetRoot()
                                                                                   .DescendantNodes()
                                                                                   .TakeWhile(_ => !context.CancellationToken.IsCancellationRequested)
                                                                                   .OfType<ClassDeclarationSyntax>()
                                                                                   .Select(classDeclarationSyntax => context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax))
                                                                                   .OfType<INamedTypeSymbol>()
                                                                                   .Where(symbol => AutomaticDisposeGenerator.IsAssignableTypeSymbolToIDisposable(symbol) && !AutomaticDisposeGenerator.IsAssignableTypeSymbolToIAsyncDisposable(symbol))
                                                                                   .Select(symbol => (symbol, attributeData: symbol.GetAttributes().SingleOrDefault(attrData => AutomaticDisposeGenerator.IsAutomaticDisposeImplAnnotationTypeSymbol(attrData.AttributeClass))!))
                                                                                   .Where(v => v.attributeData is not null)
                                                                                   .ToArray();

            foreach (var automaticDisposeImplDesignation in automaticDisposeImplDesignations)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                AutomaticDisposeContextChecker automaticDisposeContextChecker = new AutomaticDisposeContextChecker(automaticDisposeImplModeAttributeSymbol, automaticDisposeImplDesignation.attributeData);

                var memberAndLocationPairs = automaticDisposeImplDesignation.symbol.GetMembers()
                                                                                   .Select(member => (member, location: member.Locations.FirstOrDefault(location => location.SourceTree == context.SemanticModel.SyntaxTree)!))
                                                                                   .Where(v => v.location is not null);

                foreach (var memberAndLocationPair in memberAndLocationPairs)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    var member = memberAndLocationPair.member;
                    var location = memberAndLocationPair.location;

                    if (member.IsImplicitlyDeclared || member.IsStatic)
                    {
                        continue;
                    }

                    if (member is IFieldSymbol fieldSymbol)
                    {
                        if (!AutomaticDisposeGenerator.IsAssignableTypeSymbolToIAsyncDisposable(fieldSymbol.Type))
                        {
                            continue;
                        }

                        if (automaticDisposeContextChecker.IsDisabledModeField(fieldSymbol))
                        {
                            continue;
                        }

                        context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0003, location, fieldSymbol.Name, automaticDisposeImplDesignation.symbol.Name));
                    }
                    else if (member is IPropertySymbol propertySymbol)
                    {
                        if (!AutomaticDisposeGenerator.IsAssignableTypeSymbolToIAsyncDisposable(propertySymbol.Type))
                        {
                            continue;
                        }

                        if (automaticDisposeContextChecker.IsDisabledModeProperty(propertySymbol))
                        {
                            continue;
                        }

                        context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0003, location, propertySymbol.Name, automaticDisposeImplDesignation.symbol.Name));
                    }
                }
            }
        }

        private static void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol namedTypeSymbol)
            {
                return;
            }

            var automaticDisposeImplAttributeSymbol = context.Compilation.GetTypeByMetadataName(AutomaticDisposeGenerator.AutomaticDisposeImplAttributeFullyQualifiedMetadataName);
            if (automaticDisposeImplAttributeSymbol is null)
            {
                return;
            }

            var attributeData = namedTypeSymbol.GetAttributes().SingleOrDefault(v => SymbolEqualityComparer.Default.Equals(v.AttributeClass, automaticDisposeImplAttributeSymbol));
            if (attributeData is null)
            {
                return;
            }

            var classDeclarationSyntaxes = EnumerateAllDeclarationSyntaxes(namedTypeSymbol, context.CancellationToken).ToArray();

            foreach (var nonParcialDeclaration in classDeclarationSyntaxes.Where(IsNotParcialDeclaration))
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0001, nonParcialDeclaration.Identifier.GetLocation()));
            }

            if (!AutomaticDisposeGenerator.IsAssignableTypeSymbolToIDisposable(namedTypeSymbol))
            {
                if (TryGetAttributeAttachedClassDeclarationSyntax(namedTypeSymbol, classDeclarationSyntaxes, out var classDeclarationSyntax, context.CancellationToken))
                {
                    context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0002, classDeclarationSyntax.Identifier.GetLocation()));
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0002, namedTypeSymbol.Locations[0]));
                }
            }

            static IEnumerable<ClassDeclarationSyntax> EnumerateAllDeclarationSyntaxes(INamedTypeSymbol namedTypeSymbol, CancellationToken cancellationToken)
            {
                foreach (var location in namedTypeSymbol.Locations)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (location.SourceTree is not { } syntaxTree || !syntaxTree.TryGetRoot(out var root))
                    {
                        continue;
                    }

                    if (root.FindNode(location.SourceSpan) is not ClassDeclarationSyntax classDeclarationSyntax)
                    {
                        continue;
                    }

                    yield return classDeclarationSyntax;
                }
            }
        }

        static bool IsNotParcialDeclaration(ClassDeclarationSyntax classDeclarationSyntax) => classDeclarationSyntax.Modifiers.All(modifier => modifier.Text != "partial");

        static bool TryGetAttributeAttachedClassDeclarationSyntax(INamedTypeSymbol namedTypeSymbol, IEnumerable<ClassDeclarationSyntax> classDeclarationSyntaxes, out ClassDeclarationSyntax classDeclarationSyntax, CancellationToken cancellationToken)
        {
            foreach (var candidateClassDeclarationSyntax in classDeclarationSyntaxes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (candidateClassDeclarationSyntax.AttributeLists.SelectMany(list => list.Attributes).Any(MaybeAutomaticDisposeImplAttributeSyntax))
                {
                    classDeclarationSyntax = candidateClassDeclarationSyntax;
                    return true;
                }
            }

            classDeclarationSyntax = default!;
            return false;
        }

        static bool MaybeAutomaticDisposeImplAttributeSyntax(AttributeSyntax attributeSyntax)
        {
            var name = attributeSyntax.Name.ToString();

            if (name.EndsWith(AutomaticDisposeGenerator.AutomaticDisposeImplAttributeCoreName)) return true;
            if (name.EndsWith(AutomaticDisposeGenerator.AutomaticDisposeImplAttributeName)) return true;

            return false;
        }
    }
}
