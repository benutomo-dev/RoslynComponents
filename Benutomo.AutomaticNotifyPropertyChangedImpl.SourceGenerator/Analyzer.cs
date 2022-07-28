using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// AutomaticDisposeImpl属性を付与した型の定義はpartialである必要があります。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0001 = new DiagnosticDescriptor(
            "AutoNotif0001",
            "メンバのプロパティにEnableNotificationSupportAttribute属性を付与した型にはpartialキーワードが必要",
            "メンバのプロパティにEnableNotificationSupportAttribute属性を付与した型の定義はpartialである必要があります。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);


        /// <summary>
        /// 変更通知付きプロパティが自動実装したフィールドはコンストラクタ以外でアクセスすべきではありません。値の読み書きはプロパティから行って下さい。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0002 = new DiagnosticDescriptor(
            "AutoNotif0002",
            "変更通知付きプロパティが自動実装したフィールドにコンストラクタでアクセス",
            "\"{0}\"フィールドは変更通知の自動実装プロパティがプロパティ値を格納する内部フィールドです。コンストラクタ以外でこのフィールドに直接アクセスすべきではありません。値の読み書きは\"{1}\"プロパティから行って下さい。",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);


        /// <summary>
        /// 変更通知付きプロパティが自動実装したフィールドはコンストラクタ以外でアクセスすべきではありません。値の読み書きはプロパティから行って下さい。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0003 = new DiagnosticDescriptor(
            "AutoNotif0003",
            "変更通知付きプロパティのgetterは自動実装したGetメソッドの使用が必須",
            "変更通知付きプロパティのgetterは\"{0}()\"メソッドを使用して実装される必要があります。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);


        /// <summary>
        /// 変更通知付きプロパティが自動実装したフィールドはコンストラクタ以外でアクセスすべきではありません。値の読み書きはプロパティから行って下さい。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0004 = new DiagnosticDescriptor(
            "AutoNotif0004",
            "変更通知付きプロパティのsetterは自動実装したSetメソッドの使用が必須",
            "変更通知付きプロパティのsetterは\"{0}(value[, ...])\"メソッドを使用して実装される必要があります。",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);


        /// <summary>
        /// 変更通知付きプロパティが自動実装したフィールドはコンストラクタ以外でアクセスすべきではありません。値の読み書きはプロパティから行って下さい。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_SG0005 = new DiagnosticDescriptor(
            "AutoNotif0005",
            "変更の遅延通知用Setメソッドの戻り値はusingで受けることが必須",
            "このメソッドは戻り値のDisposeが呼び出されたタイミングでChanged系のイベントが発行されるため、戻り値をusingで受け取る必要があります。",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            s_diagnosticDescriptor_SG0001,
            s_diagnosticDescriptor_SG0002,
            s_diagnosticDescriptor_SG0003,
            s_diagnosticDescriptor_SG0004,
            s_diagnosticDescriptor_SG0005
            );

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(AnalyzeClassDeclarationSyntaxNode, SyntaxKind.ClassDeclaration);

            context.RegisterSyntaxNodeAction(AnalyzeInternalFiledReferenceSyntaxNode, SyntaxKind.IdentifierName);
            context.RegisterSyntaxNodeAction(AnalyzeMustDisposeSetterMethodCallSyntaxNode, SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(context => AnalyzeFieldAccessorSyntaxNode(context, isSetter: false), SyntaxKind.GetAccessorDeclaration);
            context.RegisterSyntaxNodeAction(context => AnalyzeFieldAccessorSyntaxNode(context, isSetter: true), SyntaxKind.SetAccessorDeclaration);
        }

        private static void AnalyzeClassDeclarationSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not ClassDeclarationSyntax classDeclarationSyntax) return;

            if (classDeclarationSyntax.Modifiers.Any(modifier => modifier.Text == "partial")) return;

            INamedTypeSymbol? enableNotificationSupportAttributeSymbol = null;

            var propertyDeclarationSyntaxes = classDeclarationSyntax.Members.OfType<PropertyDeclarationSyntax>();

            foreach (var propertyDeclarationSyntax in propertyDeclarationSyntaxes)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (propertyDeclarationSyntax.AttributeLists.Count == 0)
                {
                    continue;
                }

                enableNotificationSupportAttributeSymbol ??= context.Compilation.GetTypeByMetadataName(StaticSources.EnableNotificationSupportAttributeFullyQualifiedMetadataName);
                if (enableNotificationSupportAttributeSymbol is null)
                {
                    return;
                }

                var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax, context.CancellationToken);

                if (propertySymbol.IsAttributedBy(enableNotificationSupportAttributeSymbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0001, classDeclarationSyntax.Identifier.GetLocation()));
                    return;
                }
            }
        }

        private static void AnalyzeInternalFiledReferenceSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not IdentifierNameSyntax identifierNameSyntax) return;

            if (!identifierNameSyntax.Identifier.Text.StartsWith("__")) return;

            foreach (var ancestor in identifierNameSyntax.Ancestors())
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (ancestor is InvocationExpressionSyntax invocationExpressionSyntax)
                {
                    if (invocationExpressionSyntax.Expression is IdentifierNameSyntax invocationIdentifierName && invocationIdentifierName.Identifier.Text == "nameof")
                    {
                        // nameofのパラメータとしては使用しても良い
                        return;
                    }
                }
                else if (ancestor is ConstructorDeclarationSyntax)
                {
                    // コンストラクタ内では初期化の為などに使用しても良い
                    return;
                }
                else if (ancestor is MethodDeclarationSyntax methodDeclarationSyntax)
                {
                    var autoGeneratedInternalAccessorMethodAttributeSymbol = context.Compilation.GetTypeByMetadataName(StaticSources.AutoGeneratedInternalAccessorMethodAttributeFullyQualifiedMetadataName);

                    if (autoGeneratedInternalAccessorMethodAttributeSymbol is null)
                    {
                        break;
                    }

                    var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax, context.CancellationToken);

                    if (methodSymbol is null)
                    {
                        break;
                    }

                    if (methodSymbol.IsAttributedBy(autoGeneratedInternalAccessorMethodAttributeSymbol))
                    {
                        // ソースジェネレータが生成したメンバ内では使用しても良い
                        return;
                    }
                }
                else if (ancestor is MemberDeclarationSyntax)
                {
                    break;
                }
            }

            // 呼び出されるメソッドが自動生成されたフィールドであることを確認して、診断結果を報告

            var symbolInfo = context.SemanticModel.GetSymbolInfo(identifierNameSyntax, context.CancellationToken);

            if (symbolInfo.Symbol is not IFieldSymbol fieldSymbol) return;

            var autoGeneratedInternalFieldAttributeSymbol = context.Compilation.GetTypeByMetadataName(StaticSources.AutoGeneratedInternalFieldAttributeFullyQualifiedMetadataName);

            if (autoGeneratedInternalFieldAttributeSymbol is null) return;

            var autoGeneratedInternalFieldAttribute = fieldSymbol.GetAttributes().FirstOrDefault(v => v.AttributeClass.IsSameSymbolTo(autoGeneratedInternalFieldAttributeSymbol));

            if (autoGeneratedInternalFieldAttribute is null) return;

            context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0002, identifierNameSyntax.GetLocation(), identifierNameSyntax.Identifier.Text, autoGeneratedInternalFieldAttribute.ConstructorArguments[0].Value));
        }

        private static void AnalyzeMustDisposeSetterMethodCallSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not InvocationExpressionSyntax invocationExpressionSyntax) return;

            if (invocationExpressionSyntax.Expression is not IdentifierNameSyntax identifierNameSyntax) return;

            if (!identifierNameSyntax.Identifier.Text.StartsWith("_")) return;

            if (!identifierNameSyntax.Identifier.Text.EndsWith(MethodSourceBuildInputs.DefferedNotificationMethodSuffix)) return;

            foreach (var ancestor in invocationExpressionSyntax.Ancestors())
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (ancestor is LocalDeclarationStatementSyntax localDeclarationStatementSyntax)
                {
                    if (localDeclarationStatementSyntax.UsingKeyword.Value is not null)
                    {
                        // usingキーワードつきのローカル変数の定義で使用されているのでOK
                        return;
                    }

                    break;
                }
                else if (ancestor is UsingStatementSyntax usingStatementSyntax)
                {
                    // using文で使用されているのでOK
                    return;
                }
                else if (ancestor is InvocationExpressionSyntax)
                {
                    break;
                }
                else if (ancestor is MemberDeclarationSyntax)
                {
                    break;
                }
                else if (ancestor is BlockSyntax)
                {
                    break;
                }
            }

            // 呼び出されるメソッドが自動生成されたDisposeメソッドを持つオブジェクトを返すsetterであることを確認して、診断結果を報告

            var symbolInfo = context.SemanticModel.GetSymbolInfo(identifierNameSyntax, context.CancellationToken);

            if (symbolInfo.Symbol is not IMethodSymbol methodSymbol) return;

            var autoGeneratedInternalAccessorMethodAttributeSymbol = context.Compilation.GetTypeByMetadataName(StaticSources.AutoGeneratedInternalAccessorMethodAttributeFullyQualifiedMetadataName);

            if (autoGeneratedInternalAccessorMethodAttributeSymbol is null) return;

            var autoGeneratedInternalAccessorMethodAttribute = methodSymbol.GetAttributes().FirstOrDefault(v => v.AttributeClass.IsSameSymbolTo(autoGeneratedInternalAccessorMethodAttributeSymbol));

            if (autoGeneratedInternalAccessorMethodAttribute is null) return;

            if (!methodSymbol.ReturnType.GetMembers().Any(v => v is IMethodSymbol methodSymbol && methodSymbol.Name == "Dispose")) return;

            context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0005, invocationExpressionSyntax.GetLocation()));
        }

        private static void AnalyzeFieldAccessorSyntaxNode(SyntaxNodeAnalysisContext context, bool isSetter)
        {
            if (context.Node is not AccessorDeclarationSyntax accessorDeclarationSyntax) return;

            var propertyDeclarationSyntax = accessorDeclarationSyntax.FirstAncestorOrSelf<PropertyDeclarationSyntax>();

            if (propertyDeclarationSyntax is null) return;

            var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax, context.CancellationToken);

            if (propertySymbol is null) return;

            var enableNotificationSupportAttributeSymbol = context.Compilation.GetTypeByMetadataName(StaticSources.EnableNotificationSupportAttributeFullyQualifiedMetadataName);

            if (enableNotificationSupportAttributeSymbol is null) return;

            var attributeData = propertySymbol.GetAttributes().FirstOrDefault(v => v.AttributeClass.IsSameSymbolTo(enableNotificationSupportAttributeSymbol));

            if (attributeData is null) return;

            var isEventArgsOnly = attributeData.NamedArguments.Where(v => v.Key == "EventArgsOnly").Select(v => (bool)(v.Value.Value ?? false)).FirstOrDefault();

            if (isEventArgsOnly) return;

            string methodIdentifierName;

            if (propertySymbol.ExplicitInterfaceImplementations.Length > 0)
            {
                methodIdentifierName = $"_{propertySymbol.ExplicitInterfaceImplementations[0].ContainingType.Name}_{propertySymbol.ExplicitInterfaceImplementations[0].Name}";
            }
            else
            {
                methodIdentifierName = $"_{propertySymbol.Name}";
            }

            // get/setアクセサ内に必須のメソッド呼び出しが含まれている事をチェックする
            foreach (var invocationExpressionSyntax in accessorDeclarationSyntax.DescendantNodes(node => node is not InvocationExpressionSyntax).OfType<InvocationExpressionSyntax>())
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (invocationExpressionSyntax.Expression is IdentifierNameSyntax identifierName)
                {
                    bool isMatchedExpectedMetohdSignature = false;
                    if (isSetter)
                    {
                        if (invocationExpressionSyntax.ArgumentList.Arguments.Count > 0
                            && (identifierName.Identifier.Text == methodIdentifierName || identifierName.Identifier.Text == $"{methodIdentifierName}{MethodSourceBuildInputs.DefferedNotificationMethodSuffix}"))
                        {
                            isMatchedExpectedMetohdSignature = true;
                        }
                    }
                    else
                    {
                        if (invocationExpressionSyntax.ArgumentList.Arguments.Count == 0 && identifierName.Identifier.Text == methodIdentifierName)
                        {
                            isMatchedExpectedMetohdSignature = true;
                        }
                    }

                    if (isMatchedExpectedMetohdSignature)
                    {
                        // メソッドがプロパティを定義するクラス自身のメンバメソッドであることを確認

                        var methodSymbolInfo = context.SemanticModel.GetSymbolInfo(identifierName, context.CancellationToken);

                        if (methodSymbolInfo.Symbol is not null && methodSymbolInfo.Symbol.ContainingSymbol.IsSameSymbolTo(propertySymbol.ContainingSymbol))
                        {
                            return;
                        }
                    }
                }
            }

            if (isSetter)
            {
                context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0004, accessorDeclarationSyntax.GetLocation(), methodIdentifierName));
            }
            else
            {
                context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_SG0003, accessorDeclarationSyntax.GetLocation(), methodIdentifierName));
            }
        }
    }
}
