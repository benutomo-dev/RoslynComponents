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
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0002 = new DiagnosticDescriptor("SG0002", "AutomaticDisposeImpl属性を付与する型にはIDisposableまたはIAsyncDisposableインターフェイスが必要", "AutomaticDisposeImpl属性を付与した型はIDisposableとIAsyncDisposableの少なくともどちらか一方を実装していなくてはなりません。", "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);

        /// <summary>
        /// {0}(メンバ名)はIAsyncDisposableを実装していますが、{1}(メンバを含んでいる型名)にIAsyncDisposableが実装されていないため、常に同期メソッドのDisposeによって破棄されます。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0003 = new DiagnosticDescriptor("SG0003", "メンバの非同期破棄メソッドを利用するためにはIAsyncDisposableインターフェイスが必要", "{0}はIAsyncDisposableを実装しているため、DisposeAsync()メソッドによる非同期破棄が可能ですが、{1}にIAsyncDisposableが実装されていないため常にDispose()による同期的な破棄がされます。{1}の実装インターフェイスにIAsyncDisposableを追加して非同期破棄をサポートしてください。", "Usage", DiagnosticSeverity.Warning, isEnabledByDefault: true);

        /// <summary>
        /// {0}(メンバ名)はIAsyncDisposableを実装していますが、IDisposableを実装していません。{1}(メンバを含んでいる型名)にはIDisposableのみが実装されているため、このメンバは自動破棄の対象とはなりません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0004 = new DiagnosticDescriptor("SG0004", "非同期破棄のみをサポートするメンバはIDiposableのみを実装するクラスの自動破棄対象外", "{0}はIAsyncDisposableを実装していますが、IDisposableを実装していません。{1}にはIDisposableのみが実装されているため、このメンバは自動破棄の対象とはなりません。", "Usage", DiagnosticSeverity.Warning, isEnabledByDefault: true);

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
            s_diagnosticDescriptor_SG0004,
            s_diagnosticDescriptor_SG9998,
            s_diagnosticDescriptor_SG9999
            );

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);

            //context.RegisterSemanticModelAction(AnalyzeSemanticModel);
        }

        //private static void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
        //{
        //}

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

            AutomaticDisposeContextChecker automaticDisposeContextChecker = new AutomaticDisposeContextChecker(attributeData);

            if (!AutomaticDisposeGenerator.IsAssignableTypeSymbolToIAsyncDisposable(namedTypeSymbol))
            {
                if (AutomaticDisposeGenerator.IsAssignableTypeSymbolToIDisposable(namedTypeSymbol))
                {
                    // 自動実装対象クラスがIDisposableのみを実装

                    foreach (var member in namedTypeSymbol.GetMembers().Where(v => !v.IsImplicitlyDeclared && !v.IsStatic))
                    {
                        context.CancellationToken.ThrowIfCancellationRequested();

                        if (member is IFieldSymbol fieldSymbol)
                        {
                            if (automaticDisposeContextChecker.IsDisabledModeField(fieldSymbol))
                            {
                                continue;
                            }

                            if (AutomaticDisposeGenerator.IsAssignableTypeSymbolToIAsyncDisposable(fieldSymbol.Type))
                            {
                                if (AutomaticDisposeGenerator.IsAssignableTypeSymbolToIDisposable(fieldSymbol.Type))
                                {
                                    foreach(var location in fieldSymbol.Locations)
                                    {
                                        context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0003, location, fieldSymbol.Name, namedTypeSymbol.Name));
                                    }
                                }
                                else
                                {
                                    foreach (var location in fieldSymbol.Locations)
                                    {
                                        context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0004, location, fieldSymbol.Name, namedTypeSymbol.Name));
                                    }
                                }
                            }
                        }
                        else if (member is IPropertySymbol propertySymbol)
                        {
                            if (automaticDisposeContextChecker.IsDisabledModeProperty(propertySymbol))
                            {
                                continue;
                            }

                            if (AutomaticDisposeGenerator.IsAssignableTypeSymbolToIAsyncDisposable(propertySymbol.Type))
                            {
                                if (AutomaticDisposeGenerator.IsAssignableTypeSymbolToIDisposable(propertySymbol.Type))
                                {
                                    foreach (var location in propertySymbol.Locations)
                                    {
                                        context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0003, location, propertySymbol.Name, namedTypeSymbol.Name));
                                    }
                                }
                                else
                                {
                                    foreach (var location in propertySymbol.Locations)
                                    {
                                        context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0004, location, propertySymbol.Name, namedTypeSymbol.Name));
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // 自動実装対象として指定されたクラスがIDisposableもIAsyncDisposableも実装していない

                    if (TryGetAttributeAttachedClassDeclarationSyntax(namedTypeSymbol, classDeclarationSyntaxes, out var classDeclarationSyntax, context.CancellationToken))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0002, classDeclarationSyntax.Identifier.GetLocation()));
                    }
                    else
                    {
                        context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0002, namedTypeSymbol.Locations[0]));
                    }
                }
            }

            return;


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
