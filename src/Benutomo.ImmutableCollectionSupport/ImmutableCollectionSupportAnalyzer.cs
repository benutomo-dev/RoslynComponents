using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Benutomo.ImmutableCollectionSupport;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ImmutableCollectionSupportAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// ImmutableCollectionの操作結果が破棄されている恐れがあります。
    /// </summary>
    internal static DiagnosticDescriptor s_diagnosticDescriptor_ImmutableCollection0001 = new DiagnosticDescriptor(
        "ImmutableCollection0001",
        "ImmutableCollectionの操作結果の破棄",
        "ImmutableCollectionの操作結果が破棄されている恐れがあります。",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// ImmutableArrayの非object型への暗黙的なボックス化が発生しています。
    /// </summary>
    internal static DiagnosticDescriptor s_diagnosticDescriptor_ImmutableCollection0002 = new DiagnosticDescriptor(
        "ImmutableCollection0002",
        "ImmutableArray<T>の非object型への暗黙的なボックス化",
        "ImmutableArray<T>の非object型への暗黙的なボックス化が発生しています。",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// {0}メソッドにはImmutableArray&lt;T&gt;を直接受け取ることが可能なオーバーロードが存在しています。BoxlessAsReadOnlyList()は不要です。
    /// </summary>
    internal static DiagnosticDescriptor s_diagnosticDescriptor_ImmutableCollection0003 = new DiagnosticDescriptor(
        "ImmutableCollection0003",
        $"不要な{IncrementalGenerator.BoxlessAsReadOnlyListMethodName}()の呼び出し",
        $"{{0}}メソッドにはImmutableArray<T>を直接受け取ることが可能なオーバーロードが存在しています。{IncrementalGenerator.BoxlessAsReadOnlyListMethodName}()は不要です。",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);


    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        s_diagnosticDescriptor_ImmutableCollection0001,
        s_diagnosticDescriptor_ImmutableCollection0002,
        s_diagnosticDescriptor_ImmutableCollection0003
        );

#if DEBUG
    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1026:同時実行を有効にします", Justification = "デバック時は同時実行を有効化しない")]
#endif
    public override void Initialize(AnalysisContext context)
    {
#if !DEBUG
        context.EnableConcurrentExecution();
#endif
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterOperationAction(DetectX, OperationKind.Invocation);

        context.RegisterOperationAction(DetectImplicitBoxingForImmutableArray, OperationKind.Conversion);

        context.RegisterSyntaxNodeAction(DetectDiscardOfCollectionOperationResult, SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void DetectX(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation invocationOperation)
        {
            return;
        }

        if (invocationOperation.SemanticModel is null)
        {
            return;
        }

        if (invocationOperation.TargetMethod is not { Name: IncrementalGenerator.BoxlessAsReadOnlyListMethodName, IsExtensionMethod: true, Parameters.Length: 1 })
        {
            return;
        }

        if (invocationOperation.Arguments[0].Value.Type is not INamedTypeSymbol instanceType)
        {
            return;
        }

        var immutableArrayTypeSymbol = context.Compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableArray`1");

        if (immutableArrayTypeSymbol is null)
        {
            return;
        }

        var unboundGenericInstanceTypeSymbol = instanceType.ConstructUnboundGenericType();

        var unboundGenericImmutableArrayTypeSymbol = immutableArrayTypeSymbol.ConstructUnboundGenericType();

        if (!SymbolEqualityComparer.Default.Equals(unboundGenericInstanceTypeSymbol, unboundGenericImmutableArrayTypeSymbol))
        {
            return;
        }

        IInvocationOperation parentInvocationOperation;
        {
            var parent = invocationOperation.Parent;
            while (true)
            {
                switch (parent)
                {
                    case IInvocationOperation _parentInvocationOperation:
                        parentInvocationOperation = _parentInvocationOperation;
                        break;
                    case IConversionOperation:
                    case IArgumentOperation:
                        parent = parent.Parent;
                        continue;
                    default:
                        return;
                }
                break;
            }

        }

        var reboundGenericImmutableArrayTypeSymbol = immutableArrayTypeSymbol.Construct(instanceType.TypeArguments[0]);

        var immutableArrayExtensionMethodSymbols = invocationOperation.SemanticModel
            .LookupExtensionMethods(invocationOperation.Syntax.SpanStart, name: parentInvocationOperation.TargetMethod.Name, receiverType: reboundGenericImmutableArrayTypeSymbol, context.CancellationToken);

        bool hasImmutableArrayVersionMethod = false;
        foreach (var extensionMethodSymbol in immutableArrayExtensionMethodSymbols)
        {
            if (extensionMethodSymbol.Parameters.Length < invocationOperation.Arguments.Length)
            {
                continue;
            }

            //if (false
            //    || extensionMethodSymbol.Parameters[0].Type is not INamedTypeSymbol extensionMethodRecieverType
            //    || !(extensionMethodRecieverType.IsGenericType && SymbolEqualityComparer.Default.Equals(extensionMethodRecieverType.ConstructUnboundGenericType(), unboundGenericImmutableArrayTypeSymbol))
            //    )
            //{
            //    continue;
            //}

            if (SymbolEqualityComparer.Default.Equals(invocationOperation.TargetMethod, extensionMethodSymbol))
            {
                continue;
            }

            bool canCallSignature = true;

            // 第１引数(拡張メソッドの擬似this)以外で現在のメソッドに明示的に渡されている引数は同じ全て位置の引数にそのまま代入可能であること
            for (int i = 1; i < parentInvocationOperation.Arguments.Length; i++)
            {
                var argValueType = parentInvocationOperation.Arguments[i].Value switch
                {
                    IConversionOperation { IsImplicit: true } conversionOperation => conversionOperation.Operand.Type,
                    { }  argValue => argValue.Type
                };

                if (!argValueType.IsAssignableTo(extensionMethodSymbol.Parameters[i].Type, context.Compilation))
                {
                    canCallSignature = false;
                    break;
                }
            }

            // 代わりのメソッドに余分な引数を受け取る余地がある場合は全て省略可能なオプションであること
            for (int i = parentInvocationOperation.Arguments.Length; i < extensionMethodSymbol.Parameters.Length; i++)
            {
                if (!extensionMethodSymbol.Parameters[i].IsOptional)
                {
                    canCallSignature = false;
                    break;
                }
            }

            if (canCallSignature)
            {
                hasImmutableArrayVersionMethod = true;
                break;
            }
        }

        if (!hasImmutableArrayVersionMethod)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(s_diagnosticDescriptor_ImmutableCollection0003, invocationOperation.Syntax.GetLocation(), parentInvocationOperation.TargetMethod.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static void DetectImplicitBoxingForImmutableArray(OperationAnalysisContext context)
    {
        if (context.Operation is not IConversionOperation conversionOperation)
        {
            return;
        }

        if (conversionOperation.Conversion.IsUserDefined)
        {
            return;
        }

        if (conversionOperation.Syntax is CastExpressionSyntax)
        {
            // 明示的なキャストは除外
            return;
        }


        if (conversionOperation.Operand.Type is not INamedTypeSymbol valueType)
        {
            return;
        }

        if (!(conversionOperation.Type is { } targetType))
        {
            return;
        }


        if (!valueType.IsGenericType)
        {
            return;
        }

        var unboundGenericValueType = valueType.ConstructUnboundGenericType();

        var unboundGenericImmutableArrayTypeSymbol = context.Compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableArray`1")?.ConstructUnboundGenericType();

        if (!SymbolEqualityComparer.Default.Equals(unboundGenericValueType, unboundGenericImmutableArrayTypeSymbol))
        {
            return;
        }

        if (targetType.IsValueType || targetType.SpecialType == SpecialType.System_Object)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(s_diagnosticDescriptor_ImmutableCollection0002, context.Operation.Syntax.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static void DetectDiscardOfCollectionOperationResult(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        {
            return;
        }

        if (memberAccessExpressionSyntax.Parent is not InvocationExpressionSyntax invocationExpressionSyntax)
        {
            return;
        }

        var methodSymbolInfo = context.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax, context.CancellationToken);

        if (methodSymbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (methodSymbol.ReturnsVoid)
        {
            return;
        }

        var expressionSymbolInfo = context.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Expression, context.CancellationToken);

        if (expressionSymbolInfo.Symbol.GetTypeOfExpressionSymbol() is not { } expressionTypeSymbol)
        {
            return;
        }

        if (!SymbolEqualityComparer.Default.Equals(methodSymbol.ReturnType, expressionTypeSymbol))
        {
            return;
        }

        var typeSymbol = methodSymbol.ContainingType;

        var immutableArrayTypeSymbol = context.Compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableArray`1");

        if (immutableArrayTypeSymbol is null)
        {
            return;
        }

        var isImmutableCollectionType = false
            || SymbolEqualityComparer.Default.Equals(typeSymbol.ContainingNamespace, immutableArrayTypeSymbol.ContainingNamespace)
            || (!typeSymbol.AllInterfaces.IsEmpty && typeSymbol.AllInterfaces.Any(interfaceTypeSymbol => SymbolEqualityComparer.Default.Equals(interfaceTypeSymbol.ContainingNamespace)))
            ;

        if (!isImmutableCollectionType)
        {
            // ImmutableCollectionではないのでこのアナライザの対象外
            return;
        }

        if (invocationExpressionSyntax.Parent is AssignmentExpressionSyntax)
        {
            // 以下の様にメソッドの結果を左辺に代入しているならばOK
            // b = a.Add(x);
            return;
        }

        if (invocationExpressionSyntax.Parent is EqualsValueClauseSyntax)
        {
            // 以下の様にメソッドの結果を変数定義に仕様しているならばOK
            // var b = a.Add(x);
            return;
        }

        if (invocationExpressionSyntax.Parent is ArgumentSyntax)
        {
            // 以下の様にメソッドの結果を引数にしているならばOK
            // M(a.Add(x));
            return;
        }

        if (invocationExpressionSyntax.Parent is ReturnStatementSyntax)
        {
            // 以下の様にメソッドの結果をメソッドの返り値にしているならばOK
            // return a.Add(x);
            return;
        }

        if (isValidArrowExpressionPattern(context, invocationExpressionSyntax.Parent))
        {
            // 以下の様にメソッドの結果をメソッドの返り値にしているならばOK?
            // ImmutableArray<int> M() => a.Add(x);
            return;
        }

        if (isValidMethodChainPattern(context, expressionTypeSymbol, invocationExpressionSyntax.Parent))
        {
            // 以下の様にメソッドの結果を元にメソッドチェインにしているならばOK
            // return a.Add(x)  // メソッドチェインの対象なのでOK
            //         .Add(y); // returnの対象なのでOK
            return;
        }

        var diagnostic = Diagnostic.Create(s_diagnosticDescriptor_ImmutableCollection0001, invocationExpressionSyntax.GetLocation());
        context.ReportDiagnostic(diagnostic);


        static bool isValidMethodChainPattern(SyntaxNodeAnalysisContext context, ITypeSymbol expressionTypeSymbol, SyntaxNode? invocationExpressionSyntaxParent)
        {
            if (invocationExpressionSyntaxParent is not MemberAccessExpressionSyntax memberAccessExpression)
            {
                return false;
            }

            if (memberAccessExpression.Parent is not InvocationExpressionSyntax invocationExpressionSyntax)
            {
                return false;
            }

            var chainSymbolInfo = context.SemanticModel.GetSymbolInfo(memberAccessExpression, context.CancellationToken);

            if (chainSymbolInfo.Symbol is not IMethodSymbol chainMethodSymbol)
            {
                return false;
            }

            if (chainMethodSymbol.ReturnsVoid)
            {
                return false;
            }

            if (!SymbolEqualityComparer.Default.Equals(expressionTypeSymbol, chainMethodSymbol.ReturnType))
            {
                return false;
            }

            return true;
        }


        static bool isValidArrowExpressionPattern(SyntaxNodeAnalysisContext context, SyntaxNode? invocationExpressionSyntaxParent)
        {
            if (invocationExpressionSyntaxParent is not ArrowExpressionClauseSyntax arrowExpressionClauseSyntax)
            {
                return false;
            }

            if (arrowExpressionClauseSyntax.Parent is not MethodDeclarationSyntax methodDeclarationSyntax)
            {
                return false;
            }

            var containsMethodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax);

            if (containsMethodSymbol is null)
            {
                return false;
            }

            if (containsMethodSymbol.ReturnsVoid)
            {
                return false;
            }

            return true;
        }
    }
}
