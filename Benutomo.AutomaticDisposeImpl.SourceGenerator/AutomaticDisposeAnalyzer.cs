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
        /// AutomaticDisposeImpl属性を付与していないクラスのメンバに対してAutomaticDisposeImplMode属性を付与することはできません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0005 = new DiagnosticDescriptor(
            "SG0005",
            "AutomaticDisposeImplMode属性はAutomaticDisposeImpl属性が付与されているクラスのメンバに対してのみ付与可能",
            "AutomaticDisposeImpl属性を付与していないクラスのメンバに対してAutomaticDisposeImplMode属性を付与することはできません。",
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

            var systemTaskSymbol = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");

            var systemValueTaskSymbol = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");

            var attributeData = namedTypeSymbol.GetAttributes().SingleOrDefault(v => AutomaticDisposeGenerator.IsAutomaticDisposeImplAttribute(v.AttributeClass));
            if (attributeData is null)
            {
                AnalyzeNonAutomaticDisposeImplClass();
            }
            else
            {
                AnalyzeAutomaticDisposeImplClass();
            }


            return;

            void AnalyzeNonAutomaticDisposeImplClass()
            {
                foreach (var member in namedTypeSymbol.GetMembers())
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    if (member is IPropertySymbol || member is IFieldSymbol)
                    {
                        foreach (var attribute in member.GetAttributes())
                        {
                            context.CancellationToken.ThrowIfCancellationRequested();

                            if (AutomaticDisposeGenerator.IsAutomaticDisposeImplModeAttribute(attribute.AttributeClass))
                            {
                                foreach (var location in member.Locations)
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0005, location));
                                }
                            }
                        }
                    }
                    else if (member is IMethodSymbol)
                    {
                        foreach (var attribute in member.GetAttributes())
                        {
                            context.CancellationToken.ThrowIfCancellationRequested();

                            if (AutomaticDisposeGenerator.IsUnmanagedResourceReleaseMethodAttribute(attribute.AttributeClass))
                            {
                                foreach (var location in member.Locations)
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0006, location));
                                }
                            }
                            else if (AutomaticDisposeGenerator.IsManagedObjectDisposeMethodAttribute(attribute.AttributeClass))
                            {
                                foreach (var location in member.Locations)
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0008, location));
                                }
                            }
                            else if (AutomaticDisposeGenerator.IsManagedObjectAsyncDisposeMethodAttribute(attribute.AttributeClass))
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

            void AnalyzeAutomaticDisposeImplClass()
            {
                var classDeclarationSyntaxes = EnumerateAllDeclarationSyntaxes(namedTypeSymbol, context.CancellationToken).ToArray();

                foreach (var nonParcialDeclaration in classDeclarationSyntaxes.Where(IsNotParcialDeclaration))
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0001, nonParcialDeclaration.Identifier.GetLocation()));
                }

                AutomaticDisposeContextChecker automaticDisposeContextChecker = new AutomaticDisposeContextChecker(attributeData);

                var isAssignableToIDisposable = AutomaticDisposeGenerator.IsAssignableToIDisposable(namedTypeSymbol);
                var isAssignableToIAsyncDisposable = AutomaticDisposeGenerator.IsAssignableToIAsyncDisposable(namedTypeSymbol);

                List<ISymbol> unmanagedResourceReleaseMethodAttributeedMembers = new();
                List<ISymbol> managedObjectDisposeMethodAttributeedMembers = new();
                List<ISymbol> managedObjectAsyncDisposeMethodAttributeedMembers = new();

                foreach (var member in namedTypeSymbol.GetMembers())
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    var isAutomaticDisposeImplModeAttributedMember = AutomaticDisposeGenerator.IsAutomaticDisposeImplModeAttributedMember(member);
                    var isUnmanagedResourceReleaseMethodAttributeedMember = AutomaticDisposeGenerator.IsUnmanagedResourceReleaseMethodAttributedMember(member);
                    var isManagedObjectDisposeMethodAttributeedMember = AutomaticDisposeGenerator.IsManagedObjectDisposeMethodAttributedMember(member);
                    var isManagedObjectAsyncDisposeMethodAttributeedMember = AutomaticDisposeGenerator.IsManagedObjectAsyncDisposeMethodAttributedMember(member);

                    if (isUnmanagedResourceReleaseMethodAttributeedMember)
                    {
                        unmanagedResourceReleaseMethodAttributeedMembers.Add(member);
                    }

                    if (isManagedObjectDisposeMethodAttributeedMember)
                    {
                        managedObjectDisposeMethodAttributeedMembers.Add(member);

                        if (isAssignableToIDisposable)
                        {
                            if (!IsValidMethodForManagedObjectDisposeMethodAttribute(member))
                            {
                                foreach (var location in member.Locations)
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0010, location));
                                }
                            }

                            static bool IsValidMethodForManagedObjectDisposeMethodAttribute(ISymbol? member)
                            {
                                if (member is not IMethodSymbol methodSymbol) return false;

                                if (methodSymbol.ReturnType.SpecialType != SpecialType.System_Void) return false;

                                if (methodSymbol.IsGenericMethod) return false;

                                if (!methodSymbol.Parameters.IsEmpty) return false;

                                return true;
                            }
                        }
                        else
                        {
                            foreach (var location in member.Locations)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0007, location));
                            }
                        }
                    }

                    if (isManagedObjectAsyncDisposeMethodAttributeedMember)
                    {
                        managedObjectAsyncDisposeMethodAttributeedMembers.Add(member);

                        if (isAssignableToIAsyncDisposable)
                        {
                            if (!IsValidMethodForManagedObjectAsyncDisposeMethodAttribute(member, systemTaskSymbol, systemValueTaskSymbol))
                            {
                                foreach (var location in member.Locations)
                                {
                                    context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0014, location));
                                }
                            }

                            static bool IsValidMethodForManagedObjectAsyncDisposeMethodAttribute(ISymbol? member, INamedTypeSymbol? systemTaskSymbol, INamedTypeSymbol? systemValueTaskSymbol)
                            {
                                if (member is not IMethodSymbol methodSymbol) return false;

                                if (true
                                    && !SymbolEqualityComparer.Default.Equals(methodSymbol.ReturnType, systemTaskSymbol)
                                    && !SymbolEqualityComparer.Default.Equals(methodSymbol.ReturnType, systemValueTaskSymbol)
                                    )
                                {
                                    return false;
                                }

                                if (methodSymbol.IsGenericMethod) return false;

                                if (!methodSymbol.Parameters.IsEmpty) return false;

                                return true;
                            }
                        }
                        else
                        {
                            foreach (var location in member.Locations)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0011, location));
                            }
                        }
                    }


                    if (isAssignableToIAsyncDisposable)
                    {
                    }
                    else
                    {
                        if (isAssignableToIDisposable)
                        {
                            // 自動実装対象クラスがIDisposableのみを実装

                            if (member.IsImplicitlyDeclared) continue;
                            if (member.IsStatic) continue;


                            if (member is IFieldSymbol fieldSymbol)
                            {
                                if (automaticDisposeContextChecker.IsDisabledModeField(fieldSymbol))
                                {
                                    continue;
                                }

                                if (AutomaticDisposeGenerator.IsAssignableToIAsyncDisposable(fieldSymbol.Type))
                                {
                                    if (AutomaticDisposeGenerator.IsAssignableToIDisposable(fieldSymbol.Type))
                                    {
                                        foreach (var location in fieldSymbol.Locations)
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

                                if (AutomaticDisposeGenerator.IsAssignableToIAsyncDisposable(propertySymbol.Type))
                                {
                                    if (AutomaticDisposeGenerator.IsAssignableToIDisposable(propertySymbol.Type))
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

                if (isAssignableToIDisposable && isAssignableToIAsyncDisposable)
                {
                    // 自動実装対象として指定されたクラスがIDisposableとIAsyncDisposableの両方を実装

                    if (managedObjectAsyncDisposeMethodAttributeedMembers.Count > 0 && managedObjectDisposeMethodAttributeedMembers.Count == 0)
                    {
                        // ユーザ実装の破棄が非同期側にしか存在していない

                        if (TryGetAttributeAttachedClassDeclarationSyntax(namedTypeSymbol, classDeclarationSyntaxes, out var classDeclarationSyntax, context.CancellationToken))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0015, classDeclarationSyntax.Identifier.GetLocation(), managedObjectAsyncDisposeMethodAttributeedMembers[0].Name));
                        }
                        else
                        {
                            context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0015, namedTypeSymbol.Locations[0], managedObjectAsyncDisposeMethodAttributeedMembers[0].Name));
                        }
                    }
                }

                if (!isAssignableToIAsyncDisposable && !isAssignableToIDisposable)
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
