using Benutomo.AutomaticDisposeImpl.SourceGenerator.Embedding;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AutomaticDisposeAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// AutomaticDisposeImpl属性を付与した型の定義はpartialである必要があります。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0001 = new DiagnosticDescriptor(
            "SG0001",
            "AutomaticDisposeImpl属性を付与する型にはpartialキーワードが必要",
            "AutomaticDisposeImpl属性を付与した型の定義はpartialである必要があります。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// AutomaticDisposeImpl属性を付与した型はIDisposableとIAsyncDisposableの少なくともどちらか一方を実装していなくてはなりません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0002 = new DiagnosticDescriptor(
            "SG0002",
            "AutomaticDisposeImpl属性を付与する型にはIDisposableまたはIAsyncDisposableインターフェイスが必要",
            "AutomaticDisposeImpl属性を付与した型はIDisposableとIAsyncDisposableの少なくともどちらか一方を実装していなくてはなりません。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// {0}(メンバ名)はIAsyncDisposableを実装していますが、{1}(メンバを含んでいる型名)にIAsyncDisposableが実装されていないため、常に同期メソッドのDisposeによって破棄されます。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0003 = new DiagnosticDescriptor(
            "SG0003",
            "メンバの非同期破棄メソッドを利用するためにはIAsyncDisposableインターフェイスが必要",
            "{0}はIAsyncDisposableを実装しているため、DisposeAsync()メソッドによる非同期破棄が可能ですが、{1}にIAsyncDisposableが実装されていないため常にDispose()による同期的な破棄がされます。{1}の実装インターフェイスにIAsyncDisposableを追加して非同期破棄をサポートしてください。",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// {0}(メンバ名)はIAsyncDisposableを実装していますが、IDisposableを実装していません。{1}(メンバを含んでいる型名)にはIDisposableのみが実装されているため、このメンバは自動破棄の対象とはなりません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0004 = new DiagnosticDescriptor(
            "SG0004",
            "非同期破棄のみをサポートするメンバはIDiposableのみを実装するクラスの自動破棄対象外",
            "{0}はIAsyncDisposableを実装していますが、IDisposableを実装していません。{1}にはIDisposableのみが実装されているため、このメンバは自動破棄の対象とはなりません。",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
            );

        /// <summary>
        /// AutomaticDisposeImpl属性を付与していないクラスのメンバに対してEnableAutomaticDispose属性を付与することはできません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0005 = new DiagnosticDescriptor(
            "SG0005",
            "EnableAutomaticDispose属性はAutomaticDisposeImpl属性が付与されているクラスのメンバに対してのみ付与可能",
            "AutomaticDisposeImpl属性を付与していないクラスのメンバに対してEnableAutomaticDispose属性を付与することはできません。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// AutomaticDisposeImpl属性を付与していないクラスのメンバに対してDisableAutomaticDispose属性を付与することはできません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0017 = new DiagnosticDescriptor(
            "SG0017",
            "DisableAutomaticDispose属性はAutomaticDisposeImpl属性が付与されているクラスのメンバに対してのみ付与可能",
            "AutomaticDisposeImpl属性を付与していないクラスのメンバに対してDisableAutomaticDispose属性を付与することはできません。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// EnableAutomaticDispose属性とDisableAutomaticDispose属性を同一メンバに同時に付与することはできません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0018 = new DiagnosticDescriptor(
            "SG0018",
            "EnableAutomaticDispose属性とDisableAutomaticDispose属性は同一メンバに対してどちらか一方のみ付与可能",
            "EnableAutomaticDispose属性とDisableAutomaticDispose属性を同一メンバに同時に付与することはできません。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// このメンバはEnableAutomaticDispose属性またはDisableAutomaticDispose属性を付与して自動的な破棄の対象とするか否かを明示する必要があります。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0019 = new DiagnosticDescriptor(
            "SG0019",
            "自動破棄実装が明示的であるクラスではEnableAutomaticDispose属性またはDisableAutomaticDispose属性が必須",
            "このメンバはEnableAutomaticDispose属性またはDisableAutomaticDispose属性を付与して自動的な破棄の対象とするか否かを明示する必要があります。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// IDisposableとIAysncDisposableの少なくともどちらも実装されていない型のメンバにEnableAutomaticDispose属性を付与することはできません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0020 = new DiagnosticDescriptor(
            "SG0020",
            "IDisposableとIAysncDisposableのどちらも実装していない型のメンバにEnableAutomaticDispose属性は付与できない",
            "IDisposableとIAysncDisposableの少なくともどちらも実装されていない型のメンバにEnableAutomaticDispose属性を付与することはできません。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// IDisposableとIAysncDisposableの少なくともどちらも実装されていない型のメンバにDisableAutomaticDispose属性を付与することはできません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0021 = new DiagnosticDescriptor(
            "SG0021",
            "IDisposableとIAysncDisposableのどちらも実装していない型のメンバにDisableAutomaticDispose属性は付与できない",
            "IDisposableとIAysncDisposableの少なくともどちらも実装されていない型のメンバにDisableAutomaticDispose属性を付与することはできません。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// staticメンバにEnableAutomaticDispose属性を付与することはできません。staticメンバは常に自動破棄の対象外です。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0022 = new DiagnosticDescriptor(
            "SG0022",
            "staticメンバにEnableAutomaticDispose属性は付与できない",
            "staticメンバにEnableAutomaticDispose属性を付与することはできません。staticメンバは常に自動破棄の対象外です。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// staticメンバにDisableAutomaticDispose属性を付与することはできません。staticメンバは常に自動破棄の対象外です。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0023 = new DiagnosticDescriptor(
            "SG0023",
            "staticメンバにDisableAutomaticDispose属性は付与できない",
            "staticメンバにDisableAutomaticDispose属性を付与することはできません。staticメンバは常に自動破棄の対象外です。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// AutomaticDisposeImpl属性を付与していないクラスのメソッドに対してUnmanagedResourceReleaseMethod属性を付与することはできません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0006 = new DiagnosticDescriptor(
            "SG0006",
            "UnmanagedResourceReleaseMethod属性はAutomaticDisposeImpl属性が付与されているクラスのメソッドに対してのみ付与可能",
            "AutomaticDisposeImpl属性を付与していないクラスのメソッドに対してUnmanagedResourceReleaseMethod属性を付与することはできません。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// UnmanagedResourceReleaseMethod属性を一つのクラス内で複数のメソッドに付与することは出来ません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0016 = new DiagnosticDescriptor(
            "SG0016",
            "UnmanagedResourceReleaseMethod属性を一つのクラス内で複数のメソッドに付与することは出来ません",
            "UnmanagedResourceReleaseMethod属性を一つのクラス内で複数のメソッドに付与することは出来ません。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// IDisposableを実装していないクラスでManagedObjectDisposeMethod属性を使用することは出来ません。ManagedObjectDisposeMethod属性を付与したメソッドは、IDisposable.Dispose()の呼び出しと対応します。このメソッドはIAsyncDisposable.DisposeAsync()の自動実装コードからは呼び出されません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0007 = new DiagnosticDescriptor(
            "SG0007",
            "ManagedObjectDisposeMethod属性がIDisposableインターフェースを実装していないクラスで使用されています。",
            "IDisposableを実装していないクラスでManagedObjectDisposeMethod属性を使用することは出来ません。ManagedObjectDisposeMethod属性を付与したメソッドは、IDisposable.Dispose()の呼び出しと対応します。このメソッドはIAsyncDisposable.DisposeAsync()の自動実装コードからは呼び出されません。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// AutomaticDisposeImpl属性を付与していないクラスのメソッドに対してManagedObjectDisposeMethod属性を付与することはできません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0008 = new DiagnosticDescriptor(
            "SG0008",
            "ManagedObjectDisposeMethod属性がAutomaticDisposeImpl属性を付与されていないクラスのメソッドに付与されています",
            "AutomaticDisposeImpl属性を付与していないクラスのメソッドに対してManagedObjectDisposeMethod属性を付与することはできません。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// ManagedObjectDisposeMethod属性を一つのクラス内で複数のメソッドに付与することは出来ません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0009 = new DiagnosticDescriptor(
            "SG0009",
            "ManagedObjectDisposeMethod属性を一つのクラス内で複数のメソッドに付与することは出来ません",
            "ManagedObjectDisposeMethod属性を一つのクラス内で複数のメソッドに付与することは出来ません。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// ManagedObjectDisposeMethod属性を付与するメソッドは戻り値がvoidで引数が存在しないインスタンスメソッドである必要があります。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0010 = new DiagnosticDescriptor(
            "SG0010",
            "不適当なシグネチャのメソッドにManagedObjectDisposeMethod属性が付与されています",
            "ManagedObjectDisposeMethod属性を付与するメソッドは戻り値がvoidで引数が存在しないインスタンスメソッドである必要があります。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// IAsyncDisposableを実装していないクラスでManagedObjectAsyncDisposeMethod属性を使用することは出来ません。ManagedObjectAsyncDisposeMethod属性を付与したメソッドは、IAsyncDisposable.DisposeAsync()の呼び出しと対応します。このメソッドはIDisposable.Dispose()の自動実装コードからは呼び出されません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0011 = new DiagnosticDescriptor(
            "SG0011",
            "ManagedObjectAsyncDisposeMethod属性がIAsyncDisposableインターフェースを実装していないクラスのメソッドに付与されています",
            "IAsyncDisposableを実装していないクラスでManagedObjectAsyncDisposeMethod属性を使用することは出来ません。ManagedObjectAsyncDisposeMethod属性を付与したメソッドは、IAsyncDisposable.DisposeAsync()の呼び出しと対応します。このメソッドはIDisposable.Dispose()の自動実装コードからは呼び出されません。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// AutomaticDisposeImpl属性を付与していないクラスのメソッドに対してManagedObjectAsyncDisposeMethod属性を付与することはできません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0012 = new DiagnosticDescriptor(
            "SG0012",
            "ManagedObjectAsyncDisposeMethod属性がAutomaticDisposeImpl属性を付与されていないクラスのメソッドに付与されています",
            "AutomaticDisposeImpl属性を付与していないクラスのメソッドに対してManagedObjectAsyncDisposeMethod属性を付与することはできません。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// ManagedObjectAsyncDisposeMethod属性を一つのクラス内で複数のメソッドに付与することは出来ません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0013 = new DiagnosticDescriptor(
            "SG0013",
            "ManagedObjectAsyncDisposeMethod属性を一つのクラス内で複数のメソッドに付与することは出来ません",
            "ManagedObjectAsyncDisposeMethod属性を一つのクラス内で複数のメソッドに付与することは出来ません。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// ManagedObjectAsyncDisposeMethod属性を付与するメソッドは戻り値の型がSystem.Threading.TaskまたはSystem.Threading.Tasks.ValueTaskで引数が存在しないインスタンスメソッドである必要があります。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0014 = new DiagnosticDescriptor(
            "SG0014",
            "不適当なシグネチャのメソッドにManagedObjectAsyncDisposeMethod属性が付与されています",
            "ManagedObjectAsyncDisposeMethod属性を付与するメソッドは戻り値の型がSystem.Threading.TaskまたはSystem.Threading.Tasks.ValueTaskで引数が存在しないインスタンスメソッドである必要があります。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// {0}(メソッド名)はDisposeAsync()による非同期的処理で破棄された時にのみ呼び出され、Dispose()による同期的処理で破棄された場合には呼び出されません。ManagedObjectDisposeMethod属性を付与するメソッドを追加し、同期的に破棄される場合と非同期的に破棄される場合の両方に対応して下さい。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0015 = new DiagnosticDescriptor(
            "SG0015",
            "非同期的に破棄された場合の処理と対に定義すべき同期的に破棄された場合の処理が未実装",
            "{0}はDisposeAsync()による非同期的処理で破棄された時にのみ呼び出され、Dispose()による同期的処理で破棄された場合には呼び出されません。ManagedObjectDisposeMethod属性を付与するメソッドを追加し、同期的に破棄される場合と非同期的に破棄される場合の両方に対応して下さい。",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// 不明な内部異常によって{0}に対するDisposeの自動実装に失敗しました。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG9998 = new DiagnosticDescriptor(
            "SG9998",
            "ソース生成の失敗",
            "不明な内部異常によって{0}に対するDisposeの自動実装が失敗しました。",
            "Execution",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// ソースジェネレータが不明な内部異常によって停止しました。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG9999 = new DiagnosticDescriptor(
            "SG9999",
            "異常終了",
            $"AutomaticDisposeGeneratorが不明な内部異常によって停止しました。",
            "Execution",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            s_diagnosticDescriptor_SG0001,
            s_diagnosticDescriptor_SG0002,
            s_diagnosticDescriptor_SG0003,
            s_diagnosticDescriptor_SG0004,
            s_diagnosticDescriptor_SG0005,
            s_diagnosticDescriptor_SG0006,
            s_diagnosticDescriptor_SG0007,
            s_diagnosticDescriptor_SG0008,
            s_diagnosticDescriptor_SG0009,
            s_diagnosticDescriptor_SG0010,
            s_diagnosticDescriptor_SG0011,
            s_diagnosticDescriptor_SG0012,
            s_diagnosticDescriptor_SG0013,
            s_diagnosticDescriptor_SG0014,
            s_diagnosticDescriptor_SG0015,
            s_diagnosticDescriptor_SG0016,
            s_diagnosticDescriptor_SG0017,
            s_diagnosticDescriptor_SG0018,
            s_diagnosticDescriptor_SG0019,
            s_diagnosticDescriptor_SG0020,
            s_diagnosticDescriptor_SG0021,
            s_diagnosticDescriptor_SG0022,
            s_diagnosticDescriptor_SG0023,
            s_diagnosticDescriptor_SG9998,
            s_diagnosticDescriptor_SG9999
            );

#if DEBUG
        [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1026:同時実行を有効にします", Justification = "<保留中>")]
#endif
        public override void Initialize(AnalysisContext context)
        {
#if !DEBUG
            context.EnableConcurrentExecution();
#endif
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

            var usingSymbols = UsingSymbols.CreateFrom(context.Compilation);

            var attributeData = namedTypeSymbol.GetAttributes().SingleOrDefault(v => v.AttributeClass.IsSameSymbolTo(usingSymbols.AutomaticDisposeImplAttribute));
            if (attributeData is null)
            {
                AnalyzeNonAutomaticDisposeImplClass(ref context, namedTypeSymbol, usingSymbols);
            }
            else
            {
                AnalyzeAutomaticDisposeImplClass(ref context, namedTypeSymbol, usingSymbols, attributeData);
            }
        }

        private static void AnalyzeNonAutomaticDisposeImplClass(ref SymbolAnalysisContext context, INamedTypeSymbol namedTypeSymbol, UsingSymbols usingSymbols)
        {
            foreach (var member in namedTypeSymbol.GetMembers())
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (member is IPropertySymbol || member is IFieldSymbol)
                {
                    foreach (var attribute in member.GetAttributes())
                    {
                        context.CancellationToken.ThrowIfCancellationRequested();

                        if (attribute.AttributeClass.IsSameSymbolTo(usingSymbols.EnableAutomaticDisposeAttribute))
                        {
                            foreach (var location in member.Locations)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0005, location));
                            }
                        }

                        if (attribute.AttributeClass.IsSameSymbolTo(usingSymbols.DisableAutomaticDisposeAttribute))
                        {
                            foreach (var location in member.Locations)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0017, location));
                            }
                        }
                    }
                }
                else if (member is IMethodSymbol)
                {
                    foreach (var attribute in member.GetAttributes())
                    {
                        context.CancellationToken.ThrowIfCancellationRequested();

                        if (attribute.AttributeClass.IsSameSymbolTo(usingSymbols.UnmanagedResourceReleaseMethodAttribute))
                        {
                            foreach (var location in member.Locations)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0006, location));
                            }
                        }
                        else if (attribute.AttributeClass.IsSameSymbolTo(usingSymbols.ManagedObjectDisposeMethodAttribute))
                        {
                            foreach (var location in member.Locations)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0008, location));
                            }
                        }
                        else if (attribute.AttributeClass.IsSameSymbolTo(usingSymbols.ManagedObjectAsyncDisposeMethodAttribute))
                        {
                            foreach (var location in member.Locations)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0012, location));
                            }
                        }
                    }
                }
            }
        }

        private static void AnalyzeAutomaticDisposeImplClass(ref SymbolAnalysisContext context, INamedTypeSymbol namedTypeSymbol, UsingSymbols usingSymbols, AttributeData attributeData)
        {
            var classDeclarationSyntaxes = enumerateAllDeclarationSyntaxes(namedTypeSymbol, context.CancellationToken).ToArray();

            foreach (var nonParcialDeclaration in classDeclarationSyntaxes.Where(IsNotParcialDeclaration))
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0001, nonParcialDeclaration.Identifier.GetLocation()));
            }

            AutomaticDisposeContextChecker automaticDisposeContextChecker = new AutomaticDisposeContextChecker(attributeData, usingSymbols);

            var isAssignableToIDisposable = namedTypeSymbol.IsAssignableTo(usingSymbols.IDisposable);
            var isAssignableToIAsyncDisposable = usingSymbols.IAsyncDisposable is not null && namedTypeSymbol.IsAssignableTo(usingSymbols.IAsyncDisposable);

            List<ISymbol> unmanagedResourceReleaseMethodAttributeedMembers = [];
            List<ISymbol> managedObjectDisposeMethodAttributeedMembers = [];
            List<ISymbol> managedObjectAsyncDisposeMethodAttributeedMembers = [];


            HashSet<string> dependencyMemberRegisteredSet = [];

            foreach (var member in namedTypeSymbol.GetMembers())
            {
                var dependencyMembers = member.GetAttributes()
                    .Where(v => v.AttributeClass.IsSameSymbolTo(usingSymbols.EnableAutomaticDisposeAttribute))
                    .Where(v => v.ConstructorArguments.Length == 1 && v.ConstructorArguments[0].Kind == TypedConstantKind.Array)
                    .SelectMany(v => v.ConstructorArguments[0].Values.Select(v => v.Value as string))
                    .Where(v => v is not null)
                    .Select(v => v!);

                foreach (var dependencyMember in dependencyMembers)
                {
                    dependencyMemberRegisteredSet.Add(dependencyMember);
                }
            }

            foreach (var member in namedTypeSymbol.GetMembers())
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                AutomaticDisposeImplClassMemberReporter reporter;
                reporter.context = context;
                reporter.usingSymbols = usingSymbols;
                reporter.namedTypeSymbol = namedTypeSymbol;
                reporter.automaticDisposeContextChecker = automaticDisposeContextChecker;
                reporter.member = member;
                reporter.dependencyMemberRegisteredSet = dependencyMemberRegisteredSet;
                reporter.isAssignableToIDisposable = isAssignableToIDisposable;
                reporter.isAssignableToIAsyncDisposable = isAssignableToIAsyncDisposable;
                reporter.isEnableAutomaticDisposeAttributedMember           = member.IsAttributedBy(usingSymbols.EnableAutomaticDisposeAttribute);
                reporter.isDisableAutomaticDisposeAttributedMember          = member.IsAttributedBy(usingSymbols.DisableAutomaticDisposeAttribute);
                reporter.isUnmanagedResourceReleaseMethodAttributeedMember  = member.IsAttributedBy(usingSymbols.UnmanagedResourceReleaseMethodAttribute);
                reporter.isManagedObjectDisposeMethodAttributeedMember      = member.IsAttributedBy(usingSymbols.ManagedObjectDisposeMethodAttribute);
                reporter.isManagedObjectAsyncDisposeMethodAttributeedMember = member.IsAttributedBy(usingSymbols.ManagedObjectAsyncDisposeMethodAttribute);

                if (reporter.isUnmanagedResourceReleaseMethodAttributeedMember)
                {
                    unmanagedResourceReleaseMethodAttributeedMembers.Add(member);
                }

                if (reporter.isManagedObjectDisposeMethodAttributeedMember)
                {
                    managedObjectDisposeMethodAttributeedMembers.Add(member);
                }

                if (reporter.isManagedObjectAsyncDisposeMethodAttributeedMember)
                {
                    managedObjectAsyncDisposeMethodAttributeedMembers.Add(member);
                }

                reporter.DoReport();
            }

            if (unmanagedResourceReleaseMethodAttributeedMembers.Count > 1)
            {
                foreach (var member in unmanagedResourceReleaseMethodAttributeedMembers)
                {
                    foreach (var location in member.Locations)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0016, location));
                    }
                }
            }

            if (managedObjectDisposeMethodAttributeedMembers.Count > 1)
            {
                foreach (var member in managedObjectDisposeMethodAttributeedMembers)
                {
                    foreach (var location in member.Locations)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0009, location));
                    }
                }
            }

            if (managedObjectAsyncDisposeMethodAttributeedMembers.Count > 1)
            {
                foreach (var member in managedObjectAsyncDisposeMethodAttributeedMembers)
                {
                    foreach (var location in member.Locations)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0013, location));
                    }
                }
            }

            if (isAssignableToIAsyncDisposable && isAssignableToIDisposable && managedObjectAsyncDisposeMethodAttributeedMembers.Count > 0 && managedObjectDisposeMethodAttributeedMembers.Count == 0)
            {
                if (TryGetAttributeAttachedClassDeclarationSyntax(classDeclarationSyntaxes, out var classDeclarationSyntax, context.CancellationToken))
                {
                    context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0015, classDeclarationSyntax.Identifier.GetLocation(), managedObjectAsyncDisposeMethodAttributeedMembers[0].Name));
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0015, namedTypeSymbol.Locations[0], managedObjectAsyncDisposeMethodAttributeedMembers[0].Name));
                }
            }

            if (!isAssignableToIAsyncDisposable && !isAssignableToIDisposable)
            {
                // 自動実装対象として指定されたクラスがIDisposableもIAsyncDisposableも実装していない

                if (TryGetAttributeAttachedClassDeclarationSyntax(classDeclarationSyntaxes, out var classDeclarationSyntax, context.CancellationToken))
                {
                    context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0002, classDeclarationSyntax.Identifier.GetLocation()));
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0002, namedTypeSymbol.Locations[0]));
                }
            }

            return;

            static IEnumerable<ClassDeclarationSyntax> enumerateAllDeclarationSyntaxes(INamedTypeSymbol namedTypeSymbol, CancellationToken cancellationToken)
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

        static bool TryGetAttributeAttachedClassDeclarationSyntax(IEnumerable<ClassDeclarationSyntax> classDeclarationSyntaxes, out ClassDeclarationSyntax classDeclarationSyntax, CancellationToken cancellationToken)
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

            if (name.EndsWith(StaticSourceAttribute.GetAttributeName<AutomaticDisposeImplAttribute>())) return true;
            if (name.EndsWith(nameof(AutomaticDisposeImplAttribute))) return true;

            return false;
        }
    }
}
