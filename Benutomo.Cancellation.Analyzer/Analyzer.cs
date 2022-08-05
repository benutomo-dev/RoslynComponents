using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Benutomo.Cancellation.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// CancellatoinTokenを受け付け可能なオーバーロードが存在
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_CT0001 = new DiagnosticDescriptor(
            "CT0001",
            "CancellatoinTokenを受け付け可能なオーバーロードが存在",
            "呼び出しメソッドには末尾の引数にCancellationTokenが追加されている以外は同一のシグネチャのオーバーロードが存在します。",
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


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            s_diagnosticDescriptor_CT0001,
            s_diagnosticDescriptor_CT0002,
            s_diagnosticDescriptor_CT0003,
            s_diagnosticDescriptor_CT0004
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSemanticModelAction(SemanticModelAction);
        }

        public void SemanticModelAction(SemanticModelAnalysisContext context)
        {
            var cancellationTokenSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Threading.CancellationToken");

            if (cancellationTokenSymbol is null) return;

            var uncancelableAttributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(StaticSources.UncancelableAttributeFullyQualifiedMetadataName);

            if (uncancelableAttributeSymbol is null) return;

            var taskCompletionSourceTResultSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.TaskCompletionSource`1");
            var taskCompletionSourceSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.TaskCompletionSource");

            var methodDeclarationQuery = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var methodDeclaration in methodDeclarationQuery)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var declaredSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);

                if (declaredSymbol is not IMethodSymbol declatationMethodSymbol)
                {
                    continue;
                }

                var isUncancelableAttributedMethod = declatationMethodSymbol.GetAttributes()
                    .Where(v => SymbolEqualityComparer.Default.Equals(v.AttributeClass, uncancelableAttributeSymbol))
                    .Any();

                var existsCancellationTokenSymbolInParameters = declatationMethodSymbol.Parameters
                    .Where(parameter => SymbolEqualityComparer.Default.Equals(parameter.Type, cancellationTokenSymbol))
                    .Any();

                if (isUncancelableAttributedMethod && existsCancellationTokenSymbolInParameters)
                {
                    context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_CT0004, methodDeclaration.Identifier.GetLocation()));
                }

                var isCancellationChecklessMethod = isUncancelableAttributedMethod || (!declatationMethodSymbol.IsAsync && !existsCancellationTokenSymbolInParameters);

                if (isCancellationChecklessMethod)
                {
                    continue;
                }

                List<ISymbol> negativeSymbols = new List<ISymbol>();

                var invocationExpressionQuery = methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>();

                bool needCancellationTokenArg = false;

                foreach (var invocationExpressionSyntax in invocationExpressionQuery)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    var symbolInfo = context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax.Expression, context.CancellationToken);

                    if (symbolInfo.Symbol is not IMethodSymbol invokedMethodSymbol) continue;

                    INamespaceOrTypeSymbol overloadSymbolsLookupContainer;
                    Location reportLocation;

                    if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
                    {
                        var typeInfo = context.SemanticModel.GetTypeInfo(memberAccessExpressionSyntax.Expression, context.CancellationToken);

                        if (typeInfo.Type is null) continue;

                        overloadSymbolsLookupContainer = typeInfo.Type;
                        reportLocation = memberAccessExpressionSyntax.Name.GetLocation();
                    }
                    else
                    {
                        overloadSymbolsLookupContainer = invokedMethodSymbol.ContainingType;
                        reportLocation = invocationExpressionSyntax.Expression.GetLocation();
                    }

                    if (invokedMethodSymbol.Parameters.Any(parameter => SymbolEqualityComparer.Default.Equals(parameter.Type, cancellationTokenSymbol)))
                    {
                        bool hasExpressionDerivedFromParameterToken = false;

                        for (int i = 0; i < invokedMethodSymbol.Parameters.Length; i++)
                        {
                            context.CancellationToken.ThrowIfCancellationRequested();

                            if (!SymbolEqualityComparer.Default.Equals(invokedMethodSymbol.Parameters[i].Type, cancellationTokenSymbol))
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
                            needCancellationTokenArg = true;
                            context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_CT0002, reportLocation));
                        }
                    }
                    else
                    {
                        var isCT0001IgnoredClassMethod = false
                            || (taskCompletionSourceSymbol is not null && SymbolEqualityComparer.Default.Equals(taskCompletionSourceSymbol, invokedMethodSymbol.ContainingType))
                            || (taskCompletionSourceTResultSymbol is not null
                                    && invokedMethodSymbol.ContainingType.IsGenericType
                                    && SymbolEqualityComparer.Default.Equals(taskCompletionSourceTResultSymbol, invokedMethodSymbol.ContainingType.ConstructedFrom))
                            ;

                        if (!isCT0001IgnoredClassMethod)
                        {
                            var overloadMethodSymbols = context.SemanticModel.LookupSymbols(invocationExpressionSyntax.Expression.GetLocation().SourceSpan.Start, overloadSymbolsLookupContainer, invokedMethodSymbol.Name, includeReducedExtensionMethods: true);

                            if (overloadMethodSymbols.OfType<IMethodSymbol>().Any(candidateMethodSymbol => IsCancellableVersionMethod(candidateMethodSymbol, cancellationTokenSymbol, invokedMethodSymbol)))
                            {
                                needCancellationTokenArg = true;
                                context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_CT0001, reportLocation));
                            }
                        }
                    }
                }


                if (true
                    && needCancellationTokenArg
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
