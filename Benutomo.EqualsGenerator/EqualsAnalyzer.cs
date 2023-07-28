using Benutomo.SourceGeneratorCommons;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Benutomo.EqualsGenerator
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EqualsAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Equalsのオーバーライドは自身の型のEqualsを呼び出すように実装してください。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_EqualsGenerator0001 = new DiagnosticDescriptor(
            "EqualsGenerator0001",
            "Equalsのオーバーライドが自身の型のEqualsを呼び出していない",
            "Equalsのオーバーライドは自身の型のEqualsを呼び出すように実装してください。",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// {0}は等価性の判定要因から除外されたメンバーです。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_EqualsGenerator0002 = new DiagnosticDescriptor(
            "EqualsGenerator0002",
            "等価性判定の要因から除外されたメンバーを使用",
            "{0}は等価性の判定要因から除外されたメンバーです。",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// 等価性判定の要因となるメンバー({0})が使用されていません。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_EqualsGenerator0003 = new DiagnosticDescriptor(
            "EqualsGenerator0003",
            "等価性判定の要因となるメンバーを未使用",
            "等価性判定の要因となるメンバー({0})が使用されていません。",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            s_diagnosticDescriptor_EqualsGenerator0001,
            s_diagnosticDescriptor_EqualsGenerator0002,
            s_diagnosticDescriptor_EqualsGenerator0003
            );

#if DEBUG
        [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1026:同時実行を有効にします", Justification = "<保留中>")]
#endif
        public override void Initialize(AnalysisContext context)
        {
#if !DEBUG
            context.EnableConcurrentExecution();
#endif
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);


            context.RegisterSyntaxNodeAction(AnalyzeEqualityMethodsImpl, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeEqualityMethodsImpl(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not MethodDeclarationSyntax methodDeclarationSyntax)
            {
                return;
            }

            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
            if (methodSymbol is null || methodSymbol.IsGenericMethod)
            {
                return;
            }

            var methodBody = (SyntaxNode?)methodDeclarationSyntax.Body ?? methodDeclarationSyntax.ExpressionBody;
            if (methodBody is null)
            {
                return;
            }

            var usingSymbols = new UsingSymbols(context.Compilation);

            if (methodSymbol.Name == nameof(Equals) && methodSymbol.Parameters.Length == 1)
            {
                var isCallOtherEqualsImpl = methodBody.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Select(symbol => context.SemanticModel.GetSymbolInfo(symbol.Expression).Symbol as IMethodSymbol)
                    .Where(invokationMethodSymbol => invokationMethodSymbol is not null
                        && SymbolEqualityComparer.Default.Equals(invokationMethodSymbol.ContainingType, methodSymbol.ContainingType)
                        && invokationMethodSymbol.IsAttributedBy(usingSymbols.EqualsImplAttribute)
                    )
                    .Any();

                if (!isCallOtherEqualsImpl)
                {
                    analyzeEqualsMethod(context, usingSymbols, methodDeclarationSyntax, methodSymbol, methodBody);
                }
            }
            else if (methodSymbol.IsAttributedBy(usingSymbols.EqualsImplAttribute))
            {
                analyzeEqualsMethod(context, usingSymbols, methodDeclarationSyntax, methodSymbol, methodBody);
            }

            if (methodSymbol.Name == nameof(GetHashCode) && methodSymbol.Parameters.Length == 0)
            {
                var isCallOtherGetHashCodeImpl = methodBody.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Select(syntax => context.SemanticModel.GetSymbolInfo(syntax.Expression).Symbol as IMethodSymbol)
                    .Where(invokationMethodSymbol => invokationMethodSymbol is not null
                        && SymbolEqualityComparer.Default.Equals(invokationMethodSymbol.ContainingType, methodSymbol.ContainingType)
                        && invokationMethodSymbol.IsAttributedBy(usingSymbols.GetHashCodeImplAttribute)
                    )
                    .Any();

                if (!isCallOtherGetHashCodeImpl)
                {
                    analyzeGetHashCodeMethod(context, usingSymbols, methodDeclarationSyntax, methodSymbol, methodBody);
                }
            }
            else if (methodSymbol.IsAttributedBy(usingSymbols.GetHashCodeImplAttribute))
            {
                analyzeGetHashCodeMethod(context, usingSymbols, methodDeclarationSyntax, methodSymbol, methodBody);
            }


            static void validateEqualityFactorMembersAccessing(SyntaxNodeAnalysisContext context, UsingSymbols usingSymbols, IMethodSymbol methodSymbol, SyntaxNode methodBody, bool isEqualsMethod, out IEnumerable<string> unusedEqualityFactorMembers, out IEnumerable<IdentifierNameSyntax> usedNotEqualityFactorMembers)
            {
                var readAccessMembersLookup = methodBody.DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Select(syntax => (syntax, symbol: context.SemanticModel.GetSymbolInfo(syntax).Symbol!))
                    .Where(v => v.symbol is not null)
                    .Where(v => v.syntax.Parent is not AssignmentExpressionSyntax assignmentExpression || assignmentExpression.Left != v.syntax)
                    .Where(v => v.symbol is IFieldSymbol or IPropertySymbol)
                    .Where(v => SymbolEqualityComparer.Default.Equals(v.symbol.ContainingType, methodSymbol.ContainingType))
                    .Select(v => (name: v.symbol.Name, v.syntax))
                    .ToLookup(v => v.name);

                var readAccessMembers = new HashSet<string>(readAccessMembersLookup.Select(v => v.Key));

                var equalityFacterMembersQuery = EqualsHelper.EnumerateValidMembers(methodSymbol.ContainingType, usingSymbols, context.SemanticModel, context.CancellationToken)
                    .Where(v => isEqualsMethod ? !v.isHashCodeCache : true)
                    .Select(v => v.symbol.Name);

                var equalityFacterMembers = new HashSet<string>(equalityFacterMembersQuery);

                if (readAccessMembers.SetEquals(equalityFacterMembers))
                {
                    unusedEqualityFactorMembers = Array.Empty<string>();
                    usedNotEqualityFactorMembers = Array.Empty<IdentifierNameSyntax>();
                }
                else
                {
                    var intersection = new HashSet<string>(readAccessMembers, readAccessMembers.Comparer);
                    intersection.IntersectWith(equalityFacterMembers);

                    equalityFacterMembers.ExceptWith(intersection);
                    readAccessMembers.ExceptWith(intersection);

                    unusedEqualityFactorMembers = equalityFacterMembers;
                    usedNotEqualityFactorMembers = readAccessMembers
                        .SelectMany(v => readAccessMembersLookup[v])
                        .Select(v => v.syntax)
                        .ToArray();
                }
            }

            static void analyzeEqualsMethod(SyntaxNodeAnalysisContext context, UsingSymbols usingSymbols, MethodDeclarationSyntax methodDeclarationSyntax, IMethodSymbol methodSymbol, SyntaxNode methodBody)
            {
                if (SymbolEqualityComparer.Default.Equals(methodSymbol.Parameters[0].Type, methodSymbol.ContainingSymbol))
                {
                    validateEqualityFactorMembersAccessing(context, usingSymbols, methodSymbol, methodBody, isEqualsMethod: true, out var unusedEqualityFactorMembers, out var usedNotEqualityFactorMembers);

                    Debug.Assert(unusedEqualityFactorMembers is ICollection<string>);
                    Debug.Assert(usedNotEqualityFactorMembers is ICollection<IdentifierNameSyntax>);

                    foreach (var usedNotEqualityFactorMember in usedNotEqualityFactorMembers)
                    {
                        var diagnostic = Diagnostic.Create(s_diagnosticDescriptor_EqualsGenerator0002, usedNotEqualityFactorMember.GetLocation(), usedNotEqualityFactorMember.Identifier.ValueText);
                        context.ReportDiagnostic(diagnostic);
                    }

                    foreach (var unusedEqualityFactorMember in unusedEqualityFactorMembers)
                    {
                        var diagnostic = Diagnostic.Create(s_diagnosticDescriptor_EqualsGenerator0003, methodDeclarationSyntax.Identifier.GetLocation(), unusedEqualityFactorMember);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
                else
                {
                    var isCallOtherEquals = methodBody.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Where(v => v.Expression is IdentifierNameSyntax { Identifier.ValueText: nameof(Equals) } or MemberAccessExpressionSyntax { Name.Identifier.ValueText: nameof(Equals) })
                        .Select(v => context.SemanticModel.GetSymbolInfo(v.Expression).Symbol as IMethodSymbol)
                        .Where(v => v is not null
                            && SymbolEqualityComparer.Default.Equals(v.ContainingType, methodSymbol.ContainingType)
                            && v.Parameters.Length == 1
                            && SymbolEqualityComparer.Default.Equals(v.Parameters[0].Type, methodSymbol.ContainingType)
                        )
                        .Any();

                    if (!isCallOtherEquals)
                    {
                        var diagnostic = Diagnostic.Create(s_diagnosticDescriptor_EqualsGenerator0001, methodDeclarationSyntax.Identifier.GetLocation());
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }

            static void analyzeGetHashCodeMethod(SyntaxNodeAnalysisContext context, UsingSymbols usingSymbols, MethodDeclarationSyntax methodDeclarationSyntax, IMethodSymbol methodSymbol, SyntaxNode methodBody)
            {
                validateEqualityFactorMembersAccessing(context, usingSymbols, methodSymbol, methodBody, isEqualsMethod: false, out var unusedEqualityFactorMembers, out var usedNotEqualityFactorMembers);

                Debug.Assert(unusedEqualityFactorMembers is ICollection<string>);
                Debug.Assert(usedNotEqualityFactorMembers is ICollection<IdentifierNameSyntax>);

                foreach (var usedNotEqualityFactorMember in usedNotEqualityFactorMembers)
                {
                    var diagnostic = Diagnostic.Create(s_diagnosticDescriptor_EqualsGenerator0002, usedNotEqualityFactorMember.GetLocation(), usedNotEqualityFactorMember.Identifier.ValueText);
                    context.ReportDiagnostic(diagnostic);
                }

                foreach (var unusedEqualityFactorMember in unusedEqualityFactorMembers)
                {
                    var diagnostic = Diagnostic.Create(s_diagnosticDescriptor_EqualsGenerator0003, methodDeclarationSyntax.Identifier.GetLocation(), unusedEqualityFactorMember);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
