using Benutomo.CancellationAnalyzer.Embedding;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Benutomo.CancellationAnalyzer
{
    internal record UsingSymbols(
        INamedTypeSymbol CancellationToken,
        INamedTypeSymbol UncancelableAttribute,
        INamedTypeSymbol DisableArgumentCancellationTokenCheckAttribute,
        INamedTypeSymbol Cancellation,
        INamedTypeSymbol? TaskCompletionSourceTResult,
        INamedTypeSymbol? TaskCompletionSource
        );

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// 引数のCancellatoinTokenを引き渡し可能なオーバーロードが存在
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_CT0001 = new DiagnosticDescriptor(
            "CT0001",
            "引数のCancellatoinTokenを引き渡し可能なオーバーロードが存在",
            "呼び出しメソッドには引数のCancellationTokenを引き渡すことが可能なオーバーロードが存在します。",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// パラメータのCancellationTokenが未使用
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_CT0002 = new DiagnosticDescriptor(
            "CT0002",
            "引数のCancellationTokenが未使用",
            "呼び出しメソッドに引数のCancellationTokenが影響していません。",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// 引数にCancellationTokenの追加を推奨
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_CT0003 = new DiagnosticDescriptor(
            "CT0003",
            "引数にCancellationTokenの追加を推奨",
            "内部で呼び出すメソッドにCancellationTokenを受けることが出来るオーバーロードを呼び出すためにこのメソッドの引数にもCancellationTokenを追加することを推奨します。",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// CancellationTokenを引数に含むメソッドにUncancelable属性が付与されている
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_CT0004 = new DiagnosticDescriptor(
            "CT0004",
            "CancellationTokenを引数に含むメソッドにUncancelable属性が付与されている",
            "CancellationTokenを引数に含むキャンセル可能なメソッドにUncancelable属性が付与されています。",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// キャンセル禁止区間で明示的なキャンセルの影響を受ける可能性がある呼び出しがされている
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_CT0005 = new DiagnosticDescriptor(
            "CT0005",
            "キャンセル禁止区間で明示的なキャンセルの影響を受ける可能性がある呼び出しがされている",
            "キャンセル禁止区間でdefaultまはたCancellationToken.None以外のCancellationToken値を引数に渡した呼び出しが行われています。",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// <see cref="Cancellation.Uncancelable"/>がusing文以外で使用されている
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_CT0006 = new DiagnosticDescriptor(
            "CT0006",
            $"{nameof(Cancellation.Uncancelable)}がusing文以外で使用されている",
            $"{nameof(Cancellation)}.{nameof(Cancellation.Uncancelable)}はusing文の括弧内の式以外で使用することはできません。using({nameof(Cancellation)}.{nameof(Cancellation.Uncancelable)})のような記述のみが可能です。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// <see cref="Cancellation.DisableArgumentCancellationTokenCheck"/>がusing文以外で使用されている
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_CT0007 = new DiagnosticDescriptor(
            "CT0007",
            $"{nameof(Cancellation.DisableArgumentCancellationTokenCheck)}がusing文以外で使用されている",
            $"{nameof(Cancellation)}.{nameof(Cancellation.DisableArgumentCancellationTokenCheck)}はusing文の括弧内の式以外で使用することはできません。using({nameof(Cancellation)}.{nameof(Cancellation.DisableArgumentCancellationTokenCheck)})のような記述のみが可能です。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            s_diagnosticDescriptor_CT0001,
            s_diagnosticDescriptor_CT0002,
            s_diagnosticDescriptor_CT0003,
            s_diagnosticDescriptor_CT0004,
            s_diagnosticDescriptor_CT0005,
            s_diagnosticDescriptor_CT0006,
            s_diagnosticDescriptor_CT0007
            );

#if DEBUG
        [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1026:同時実行を有効にします", Justification = "<保留中>")]
#endif
        public override void Initialize(AnalysisContext context)
        {
#if !DEBUG
            context.EnableConcurrentExecution();
#endif
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSemanticModelAction(SemanticModelAction);
        }

        public void SemanticModelAction(SemanticModelAnalysisContext context)
        {
            var cancellationTokenSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Threading.CancellationToken");

            if (cancellationTokenSymbol is null)
                throw new InvalidOperationException("System.Threading.CancellationTokenが見つかりません。");

            var uncancelableAttributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(StaticSourceAttribute.GetFullyQualifiedMetadataName<UncancelableAttribute>());

            if (uncancelableAttributeSymbol is null)
                throw new InvalidOperationException($"{StaticSourceAttribute.GetFullyQualifiedMetadataName<UncancelableAttribute>()}が見つかりません。");

            var disableArgumentCancellationTokenCheckAttribute = context.SemanticModel.Compilation.GetTypeByMetadataName(StaticSourceAttribute.GetFullyQualifiedMetadataName<DisableArgumentCancellationTokenCheckAttribute>());

            if (disableArgumentCancellationTokenCheckAttribute is null)
                throw new InvalidOperationException($"{StaticSourceAttribute.GetFullyQualifiedMetadataName<DisableArgumentCancellationTokenCheckAttribute>()}が見つかりません。");

            var cancellationSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(StaticSourceAttribute.GetFullyQualifiedMetadataName(typeof(Cancellation)));

            if (cancellationSymbol is null)
                throw new InvalidOperationException($"{StaticSourceAttribute.GetFullyQualifiedMetadataName(typeof(Cancellation))}が見つかりません。");

            var taskCompletionSourceTResultSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.TaskCompletionSource`1");
            var taskCompletionSourceSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.TaskCompletionSource");

            var usingSymbols = new UsingSymbols(
                CancellationToken: cancellationTokenSymbol,
                UncancelableAttribute: uncancelableAttributeSymbol,
                DisableArgumentCancellationTokenCheckAttribute: disableArgumentCancellationTokenCheckAttribute,
                Cancellation: cancellationSymbol,
                TaskCompletionSourceTResult: taskCompletionSourceTResultSymbol,
                TaskCompletionSource: taskCompletionSourceSymbol
            );

            var methodDeclarationQuery = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var methodDeclaration in methodDeclarationQuery)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var declaredSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);

                if (declaredSymbol is not IMethodSymbol declatationMethodSymbol)
                {
                    continue;
                }

                bool isUncancelableAttributedMethod = false;
                bool isAllowIgnoreArgumentCancellationTokenAttributedMethod = false;

                foreach (var attribute in declatationMethodSymbol.GetAttributes())
                {
                    if (!isUncancelableAttributedMethod && SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, uncancelableAttributeSymbol))
                    {
                        isUncancelableAttributedMethod = true;
                    }

                    if (!isAllowIgnoreArgumentCancellationTokenAttributedMethod && SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, disableArgumentCancellationTokenCheckAttribute))
                    {
                        isAllowIgnoreArgumentCancellationTokenAttributedMethod = true;
                    }
                }
                
                var existsCancellationTokenSymbolInParameters = declatationMethodSymbol.Parameters
                    .Where(parameter => SymbolEqualityComparer.Default.Equals(parameter.Type, cancellationTokenSymbol))
                    .Any();

                if (isUncancelableAttributedMethod)
                {
                    if (existsCancellationTokenSymbolInParameters)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_CT0004, methodDeclaration.Identifier.GetLocation()));
                    }

                    // メソッド全体をキャンセル禁止区間として分析
                    ReportCancelingProhibitionDiagnosticRecursiveRoot(context, usingSymbols, methodDeclaration);
                }
                else
                {
                    var isCancellationChecklessMethod = isAllowIgnoreArgumentCancellationTokenAttributedMethod || (!declatationMethodSymbol.IsAsync && !existsCancellationTokenSymbolInParameters);

                    if (isCancellationChecklessMethod)
                    {
                        // メソッド全体をCancellationTokenの引数渡しチェック不要区間として分析
                        ReportCancellationChecklessContextDiagnosticRecursiveRoot(context, usingSymbols, methodDeclaration);
                    }
                    else
                    {
                        // メソッド全体をCancellationTokenの引数渡し込みのチェック込みの区間として分析
                        ReportCancellationCheckContextDiagnosticRecursiveRoot(context, usingSymbols, methodDeclaration, out var isReportedCT0001OrCT0002);

                        // 継承等と無関係にクラス独自に定義されているCancellationTokenを引数に持たないメソッドに対して、
                        // CancellationTokenの引数渡しに関する警告が実際に発生した場合は、そのメソッドの引数に
                        // CancellationTokenを追加することを推奨する(warningで(笑))
                        if (true
                            && isReportedCT0001OrCT0002
                            && !existsCancellationTokenSymbolInParameters
                            && !declatationMethodSymbol.IsOverride                             // オーバーライドしているメソッドのシグネチャは親クラスの定義で決定されるのでCT0003対象外にする
                            && !declatationMethodSymbol.ExplicitInterfaceImplementations.Any() // 明示的実装をしているメソッドのシグネチャは実装インターフェイスの定義で決定されるのでCT0003対象外にする
                            )
                        {
                            // MEMO:
                            // あるメソッドが特定のインターフェイスを実装している物であるか否かを直接調べる方法が見つからなかった。
                            // 逆にインターフェイスのメンバに対応する実装クラスのメンバを得る方法(FindImplementationForInterfaceMember)はあったので、
                            // 全ての実装インターフェイスについて目的のメソッドと同等のシグネチャのメソッドを絞り込んだうえで、
                            // FindImplementationForInterfaceMemberにかけて目的のメソッドと一致するメソッドがないかを調べる方法をとった。

                            var isInterfaceImplementationMethod = declatationMethodSymbol.ContainingType.AllInterfaces
                                .SelectMany(interfaceSymbol =>
                                    interfaceSymbol.GetMembers().OfType<IMethodSymbol>().Where(candidateMethodSymbol => IsSameSignatureMethods(declatationMethodSymbol, candidateMethodSymbol))
                                )
                                .Select(interfaceMemberMethodSymbol => declatationMethodSymbol.ContainingType.FindImplementationForInterfaceMember(interfaceMemberMethodSymbol))
                                .Where(implementationMethodSymbol => SymbolEqualityComparer.Default.Equals(declatationMethodSymbol, implementationMethodSymbol))
                                .Any();

                            if (!isInterfaceImplementationMethod) // インターフェイスの実装に対応しているメソッドのシグネチャは実装インターフェイスの定義で決定されるのでCT0003対象外にする
                            {
                                context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_CT0003, methodDeclaration.Identifier.GetLocation()));
                            }

                            static bool IsSameSignatureMethods(IMethodSymbol a, IMethodSymbol b)
                            {
                                if (a.Arity != b.Arity) return false;
                                if (a.Parameters.Length != b.Parameters.Length) return false;
                                if (a.Name != b.Name) return false;
                                if (!SymbolEqualityComparer.Default.Equals(a.ReturnType, b.ReturnType)) return false;
                                if (!a.Parameters.Select(v => v.Type).SequenceEqual(b.Parameters.Select(v => v.Type), SymbolEqualityComparer.Default)) return false;

                                return true;
                            }
                        }
                    }
                }
            }


            var identifyNameQuery = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken).DescendantNodes().OfType<IdentifierNameSyntax>();

            foreach (var identifyName in identifyNameQuery)
            {
                if (IsDirectMemberAccessInExpressionOfUsingStatement(identifyName))
                {
                    // using文の式の中にあるのならば、UncancelableSectionか否かに関わらず問題ない
                    continue;
                }

                var symbolInfo = context.SemanticModel.GetSymbolInfo(identifyName, context.CancellationToken);

                if (symbolInfo.Symbol is not IPropertySymbol property)
                {
                    // プロパティでないならば、UncancelableSectionではない
                    continue;
                }

                if (!SymbolEqualityComparer.Default.Equals(property.ContainingType, usingSymbols.Cancellation))
                {
                    // 同名の別の型のプロパティ
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_CT0006, identifyName.GetLocation()));
            }
        }

        bool IsDirectMemberAccessInExpressionOfUsingStatement(SyntaxNode syntaxNode)
        {
            if (syntaxNode.Parent is MemberAccessExpressionSyntax)
            {
                return IsDirectMemberAccessInExpressionOfUsingStatement(syntaxNode.Parent);
            }
            else if (syntaxNode.Parent is UsingStatementSyntax usingStatementSyntax)
            {
                return usingStatementSyntax.Expression == syntaxNode;
            }
            else
            {
                return false;
            }
        }

        public void ReportCancellationCheckContextDiagnosticRecursiveRoot(SemanticModelAnalysisContext context, UsingSymbols usingSymbols, SyntaxNode syntaxNode, out bool isReportedCT0001OrCT0002)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (syntaxNode is InvocationExpressionSyntax invocationExpressionSyntax)
            {
                ReportCancellationDiagnosticIfNeeds(context, usingSymbols, invocationExpressionSyntax, out isReportedCT0001OrCT0002);
            }
            else if (syntaxNode is UsingStatementSyntax usingStatementSyntax)
            {
                if (IsUncancelableUsingStatementBlock(context, usingSymbols, usingStatementSyntax))
                {
                    // これ以下のノードはキャンセル禁止区間として探索する

                    isReportedCT0001OrCT0002 = false;
                    ReportCancelingProhibitionDiagnosticRecursiveRoot(context, usingSymbols, usingStatementSyntax.Statement);
                    return;
                }
                else if (IsDisableArgumentCancellationTokenCheckUsingStatementBlock(context, usingSymbols, usingStatementSyntax))
                {
                    // これ以下のノードはCancellationTokenの引数渡しチェック不要区間として探索する

                    isReportedCT0001OrCT0002 = false;
                    ReportCancellationChecklessContextDiagnosticRecursiveRoot(context, usingSymbols, usingStatementSyntax.Statement);
                    return;
                }
            }

            isReportedCT0001OrCT0002 = false;
            foreach (var child in syntaxNode.ChildNodes())
            {
                ReportCancellationCheckContextDiagnosticRecursiveRoot(context, usingSymbols, child, out var _isReportedCT0001OrCT0002);
                isReportedCT0001OrCT0002 = isReportedCT0001OrCT0002 || _isReportedCT0001OrCT0002;
            }
            return;
        }

        public void ReportCancellationChecklessContextDiagnosticRecursiveRoot(SemanticModelAnalysisContext context, UsingSymbols usingSymbols, SyntaxNode syntaxNode)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (syntaxNode is UsingStatementSyntax usingStatementSyntax)
            {
                if (IsUncancelableUsingStatementBlock(context, usingSymbols, usingStatementSyntax))
                {
                    // これ以下のノードはキャンセル禁止区間として探索する

                    ReportCancelingProhibitionDiagnosticRecursiveRoot(context, usingSymbols, usingStatementSyntax.Statement);
                    return;
                }
            }

            foreach (var child in syntaxNode.ChildNodes())
            {
                ReportCancellationChecklessContextDiagnosticRecursiveRoot(context, usingSymbols, child);
            }
        }

        public void ReportCancelingProhibitionDiagnosticRecursiveRoot(SemanticModelAnalysisContext context, UsingSymbols usingSymbols, SyntaxNode syntaxNode)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (syntaxNode is InvocationExpressionSyntax invocationExpressionSyntax)
            {
                ReportCancelingProhibitionDiagnosticIfNeeds(context, usingSymbols, invocationExpressionSyntax);
            }
            else if (syntaxNode is UsingStatementSyntax usingStatementSyntax)
            {
                if (IsDisableArgumentCancellationTokenCheckUsingStatementBlock(context, usingSymbols, usingStatementSyntax))
                {
                    // これ以下のノードはCancellationTokenの引数渡しチェック不要区間として探索する
                    ReportCancellationChecklessContextDiagnosticRecursiveRoot(context, usingSymbols, usingStatementSyntax.Statement);
                    return;
                }
            }

            foreach (var child in syntaxNode.ChildNodes())
            {
                ReportCancelingProhibitionDiagnosticRecursiveRoot(context, usingSymbols, child);
            }
        }

        public void ReportCancelingProhibitionDiagnosticIfNeeds(SemanticModelAnalysisContext context, UsingSymbols usingSymbols, InvocationExpressionSyntax invocationExpressionSyntax)
        {
            foreach (var arg in invocationExpressionSyntax.ArgumentList.Arguments)
            {
                if (arg.Expression is LiteralExpressionSyntax)
                {
                    // 無関係な定数
                    continue;
                }

                if (arg.Expression is DefaultExpressionSyntax)
                {
                    // default
                    continue;
                }

                if (IsCancellationTokenNone(arg.Expression))
                {
                    // CancellationToken.None
                    continue;
                }

                var typeInfo = context.SemanticModel.GetTypeInfo(arg.Expression, context.CancellationToken);

                if (SymbolEqualityComparer.Default.Equals(typeInfo.Type, usingSymbols.CancellationToken))
                {
                    // defaultまたはCancellationToken.None以外のCancellationTokenが使用されている

                    context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_CT0005, arg.GetLocation()));
                }
            }
        }

        public void ReportCancellationDiagnosticIfNeeds(SemanticModelAnalysisContext context, UsingSymbols usingSymbols, InvocationExpressionSyntax invocationExpressionSyntax, out bool isReportedCT0001OrCT0002)
        {
            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax.Expression, context.CancellationToken);

            if (symbolInfo.Symbol is not IMethodSymbol invokedMethodSymbol)
            {
                isReportedCT0001OrCT0002 = false;
                return;
            }

            INamespaceOrTypeSymbol overloadSymbolsLookupContainer;
            Location reportLocation;

            if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
            {
                var typeInfo = context.SemanticModel.GetTypeInfo(memberAccessExpressionSyntax.Expression, context.CancellationToken);

                if (typeInfo.Type is null)
                {
                    isReportedCT0001OrCT0002 = false;
                    return;
                }

                overloadSymbolsLookupContainer = typeInfo.Type;
                reportLocation = memberAccessExpressionSyntax.Name.GetLocation();
            }
            else
            {
                overloadSymbolsLookupContainer = invokedMethodSymbol.ContainingType;
                reportLocation = invocationExpressionSyntax.Expression.GetLocation();
            }

            if (invokedMethodSymbol.Parameters.Any(parameter => SymbolEqualityComparer.Default.Equals(parameter.Type, usingSymbols.CancellationToken)))
            {
                bool hasExpressionDerivedFromParameterToken = false;

                for (int i = 0; i < invokedMethodSymbol.Parameters.Length; i++)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    if (!SymbolEqualityComparer.Default.Equals(invokedMethodSymbol.Parameters[i].Type, usingSymbols.CancellationToken))
                    {
                        continue;
                    }

                    if (i < invocationExpressionSyntax.ArgumentList.Arguments.Count
                        && IsExpressionDerivedFromParameterToken(invocationExpressionSyntax.ArgumentList.Arguments[i].Expression, ref context)
                        )
                    {
                        hasExpressionDerivedFromParameterToken = true;
                        break;
                    }
                }

                if (!hasExpressionDerivedFromParameterToken)
                {
                    isReportedCT0001OrCT0002 = true;
                    context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_CT0002, reportLocation));
                    return;
                }
            }
            else
            {
                var isCT0001IgnoredClassMethod = false
                    || (usingSymbols.TaskCompletionSource is not null && SymbolEqualityComparer.Default.Equals(usingSymbols.TaskCompletionSource, invokedMethodSymbol.ContainingType))
                    || (usingSymbols.TaskCompletionSourceTResult is not null
                            && invokedMethodSymbol.ContainingType.IsGenericType
                            && SymbolEqualityComparer.Default.Equals(usingSymbols.TaskCompletionSourceTResult, invokedMethodSymbol.ContainingType.ConstructedFrom))
                    ;

                if (!isCT0001IgnoredClassMethod)
                {
                    var overloadMethodSymbols = context.SemanticModel.LookupSymbols(invocationExpressionSyntax.Expression.GetLocation().SourceSpan.Start, overloadSymbolsLookupContainer, invokedMethodSymbol.Name, includeReducedExtensionMethods: true);

                    if (overloadMethodSymbols.OfType<IMethodSymbol>().Any(candidateMethodSymbol => IsCancellableVersionMethod(candidateMethodSymbol, usingSymbols.CancellationToken, invokedMethodSymbol)))
                    {
                        isReportedCT0001OrCT0002 = true;
                        context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_CT0001, reportLocation));
                        return;
                    }
                }
            }

            isReportedCT0001OrCT0002 = false;
            return;
        }


        private static bool IsCancellationTokenNone(SyntaxNode node)
        {
            var memberAccessExpressionSyntax = node as MemberAccessExpressionSyntax;

            if (memberAccessExpressionSyntax is null) return false;

            if (memberAccessExpressionSyntax.Name.Identifier.ValueText != nameof(CancellationToken.None)) return false;

            var classNameIdentifierNameSyntax 
                = memberAccessExpressionSyntax.Expression as IdentifierNameSyntax
                ?? (memberAccessExpressionSyntax.Expression as MemberAccessExpressionSyntax)?.Name;

            if (classNameIdentifierNameSyntax?.Identifier.ValueText != nameof(CancellationToken)) return false;

            // MEMO 軽量化のため、using文を利用してCancellationTokenを別名にしている場合は考慮しない

            return true;
        }

        private static bool IsUncancelableUsingStatementBlock(SemanticModelAnalysisContext context, UsingSymbols usingSymbols, UsingStatementSyntax usingStatementSyntax)
        {
            var memberAccessExpressionSyntax = usingStatementSyntax.Expression as MemberAccessExpressionSyntax;

            var isUncancelable = memberAccessExpressionSyntax?.Name.Identifier.ValueText == nameof(Cancellation.Uncancelable);

            return isUncancelable;
        }

        private static bool IsDisableArgumentCancellationTokenCheckUsingStatementBlock(SemanticModelAnalysisContext context, UsingSymbols usingSymbols, UsingStatementSyntax usingStatementSyntax)
        {
            var memberAccessExpressionSyntax = usingStatementSyntax.Expression as MemberAccessExpressionSyntax;

            var isUncancelable = memberAccessExpressionSyntax?.Name.Identifier.ValueText == nameof(Cancellation.DisableArgumentCancellationTokenCheck);

            return isUncancelable;
        }

        private static bool IsCancellableVersionMethod(IMethodSymbol candidateMethodSymbol, INamedTypeSymbol? cancellationTokenSymbol, IMethodSymbol methodSymbol)
        {
            if (candidateMethodSymbol.Parameters.Length != methodSymbol.Parameters.Length + 1) return false;

            if (!SymbolEqualityComparer.Default.Equals(candidateMethodSymbol.ReturnType, methodSymbol.ReturnType)) return false;

            if (!SymbolEqualityComparer.Default.Equals(candidateMethodSymbol.Parameters.Last().Type, cancellationTokenSymbol))
            {
                return false;
            }

            for (int i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                if (!SymbolEqualityComparer.Default.Equals(candidateMethodSymbol.Parameters[i].Type, methodSymbol.Parameters[i].Type))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsExpressionDerivedFromParameterToken(ExpressionSyntax expressionSyntax, ref SemanticModelAnalysisContext context)
        {
            var dataFlow = context.SemanticModel.AnalyzeDataFlow(expressionSyntax);

            if (dataFlow.DataFlowsIn.Concat(dataFlow.CapturedInside).Any(refereceSymbol => refereceSymbol is IParameterSymbol))
            {
                return true;
            }

            foreach (var refereceSymbol in dataFlow.DataFlowsIn.Concat(dataFlow.CapturedInside))
            {
                foreach (var declaringSyntaxReference in refereceSymbol.DeclaringSyntaxReferences)
                {
                    if (declaringSyntaxReference.GetSyntax(context.CancellationToken) is not VariableDeclaratorSyntax variableDeclaratorSyntax)
                    {
                        continue;
                    }

                    if (variableDeclaratorSyntax.Initializer is null)
                    {
                        continue;
                    }

                    if (IsExpressionDerivedFromParameterToken(variableDeclaratorSyntax.Initializer.Value, ref context))
                    {
                        return true;
                    }
                }
            }


            return false;
        }
    }
}
