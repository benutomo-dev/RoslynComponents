using Benutomo.EqualsGenerator.Embedding;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Benutomo.EqualsGenerator;

internal sealed record class ProviderOutput(
    INamedTypeSymbol TargetSymbol,
    AttributeData Attribute,
    SemanticModel SemanticModel,
    CsDeclarationProvider CsDeclarationProvider,
    UsingSymbols UsingSymbols
    );

internal sealed record class EmitInput(
    CsTypeDeclaration TargetType,
    EquatableArray<CsTypeReference> EqualsOverrideTargetTypes,
    EquatableArray<TargetMember> TargetMembers,
    AutomaticEqualsImplOptions AutomaticEqualsImplOptions,
    string IncludeUsingStatements
    );

internal sealed record class TargetMember(
    ISymbol symbol,
    ITypeSymbol type,
    bool isHashCodeCache,
    string equalityComparer,
    string? nullFallbackEqualityComparer,
    string? warningComment);

[Generator(LanguageNames.CSharp)]
public partial class IncrementalGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        StaticSources.StaticSource.Register(context);

        var compilationLevelContextProvider = context.CreateCsDeclarationProvider()
            .Select((csDeclarationProvider, _) => (csDeclarationProvider, usingSymbols: new UsingSymbols(csDeclarationProvider.Compilation)));

        var anotatedClasses = context.SyntaxProvider
            .ForAttributeWithMetadataName(StaticSourceAttribute.GetFullyQualifiedMetadataName<AutomaticEqualsImplAttribute>(), Predicate, Transform)
            .Where(v => v.targetSymbol is not null)
            .Combine(compilationLevelContextProvider)
            .Select((v, _) => new ProviderOutput(v.Left.targetSymbol!, v.Left.attribute, v.Left.semanticModel, v.Right.csDeclarationProvider, v.Right.usingSymbols))
            .Select(ToEmitInput);

        context.RegisterSourceOutput(anotatedClasses, Emit);
    }

    private static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
    {
        return node is TypeDeclarationSyntax
        {
            AttributeLists.Count: > 0
        };
    }

    private static (INamedTypeSymbol? targetSymbol, AttributeData attribute, SemanticModel semanticModel) Transform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var typeDeclarationSyntax = (TypeDeclarationSyntax)context.TargetNode;

        if (!typeDeclarationSyntax.Modifiers.Any(modifier => modifier.ValueText == "partial"))
        {
            return default;
        }

        var namedTypeSymbol = context.SemanticModel.GetDeclaredSymbol(typeDeclarationSyntax, cancellationToken) as INamedTypeSymbol;

        Debug.Assert(context.Attributes.Length == 1);

        return (namedTypeSymbol, context.Attributes[0], context.SemanticModel);
    }

    EmitInput ToEmitInput(ProviderOutput providerOutput, CancellationToken cancellationToken)
    {
        var (targetSymbol, attribute, semanticModel, csDeclarationProvider, usingSymbols) = providerOutput;

        DebugSGen.AssertIsNotNull(targetSymbol);

        var targetMembersBuilder = ImmutableArray.CreateBuilder<TargetMember>();

        foreach (var member in EqualsHelper.EnumerateValidMembers(targetSymbol, usingSymbols, semanticModel, cancellationToken))
        {
            var equalityComparerAttribute = member.symbol.GetAttributes()
                .FirstOrDefault(v => SymbolEqualityComparer.Default.Equals(v.AttributeClass, usingSymbols.EqualityComparerAttribute));

            string equalityComparer;
            string? nullFallbackEqualityComparer = null;
            string? warningComment = null;

            if (equalityComparerAttribute is not null)
            {
                equalityComparer = "0 /* The type of IEqualityComparer<T> could not be identified. */";

                var equalityComparerAttributeSyntaxReference = equalityComparerAttribute.ApplicationSyntaxReference;

                var syntaxNode = equalityComparerAttributeSyntaxReference?.GetSyntax(cancellationToken);

                if (syntaxNode is AttributeSyntax attributeSyntax
                    && attributeSyntax.ArgumentList is { Arguments.Count: >= 1 } attributreArgListSyntax
                    && attributreArgListSyntax.Arguments[0].Expression is InvocationExpressionSyntax { ArgumentList.Arguments.Count: >= 1 } invocationExpressionSyntax
                    && invocationExpressionSyntax.Expression is IdentifierNameSyntax { Identifier.ValueText: "nameof" }
                    )
                {
                    var nameofTargetExpression = invocationExpressionSyntax.ArgumentList.Arguments[0].Expression;

                    var semanticModelForReferenceSyntaxNode = semanticModel.Compilation.GetSemanticModel(syntaxNode.SyntaxTree);

                    var symbolInfo = semanticModelForReferenceSyntaxNode.GetSymbolInfo(nameofTargetExpression, cancellationToken);

                    if (symbolInfo.Symbol is not null)
                    {
                        equalityComparer = symbolInfo.Symbol.ToString();
                    }
                }

                if (equalityComparerAttribute.ConstructorArguments.Length >= 2)
                {
                    if (equalityComparerAttribute.AttributeConstructor!.Parameters[1].Name != "useDefaultEqualityComparerIfNull")
                    {
                        DebugSGen.Fail($"{usingSymbols.EqualityComparerAttribute.Name}のコンストラクタパラメータが想定外");
                        warningComment = @$"// {usingSymbols.EqualityComparerAttribute.Name}のコンストラクタパラメータが想定外";
                    }

                    if (equalityComparerAttribute.ConstructorArguments[1].Value is true)
                    {
                        nullFallbackEqualityComparer = getDefaultEqualityComparer(member.type);
                    }
                }
            }
            else
            {
                equalityComparer = getDefaultEqualityComparer(member.type);
            }

            targetMembersBuilder.Add(new(member.symbol, member.type, member.isHashCodeCache, equalityComparer, nullFallbackEqualityComparer, warningComment));
        }

        var equalsOverrideTargetTypes = enumerateBaseTypeEqualsMethodTypes(targetSymbol.BaseType)
            .Distinct<ITypeSymbol>(SymbolEqualityComparer.Default)
            .Select(baseTypeEqualsType =>
            {
                if (baseTypeEqualsType.IsReferenceType)
                    baseTypeEqualsType = baseTypeEqualsType.WithNullableAnnotation(NullableAnnotation.Annotated);

                return csDeclarationProvider.GetTypeReference(baseTypeEqualsType);
            })
            .ToImmutableArray();

        var options = (AutomaticEqualsImplOptions)(int)attribute.ConstructorArguments[0].Value!;

        var includeUsingStatements = semanticModel.SyntaxTree.GetCompilationUnitRoot(cancellationToken).Usings.ToString();


        return new EmitInput(
            csDeclarationProvider.GetTypeDeclaration(targetSymbol),
            equalsOverrideTargetTypes,
            targetMembersBuilder.ToImmutableArray(),
            options,
            includeUsingStatements);



        static string getDefaultEqualityComparer(ITypeSymbol type)
        {
            var equalityComparerBuilder = new StringBuilder(256);

            equalityComparerBuilder.Append("System.Collections.Generic.EqualityComparer<");
            equalityComparerBuilder.AppendFullTypeNameWithNamespaceAlias(type);
            equalityComparerBuilder.Append(">.Default");

            return equalityComparerBuilder.ToString();
        }

        static IEnumerable<ITypeSymbol> enumerateBaseTypeEqualsMethodTypes(INamedTypeSymbol? baseType)
        {
            while (baseType is not null)
            {
                foreach (var member in baseType.GetMembers(nameof(Equals)))
                {
                    if (member is not IMethodSymbol methodSymbol) continue;
                    if (methodSymbol.ReturnType.SpecialType != SpecialType.System_Boolean) continue;
                    if (methodSymbol.Parameters.Length == 1) continue;

                    yield return methodSymbol.Parameters[0].Type;
                }

                baseType = baseType.BaseType;
            }
        }
    }

    private void Emit(SourceProductionContext context, EmitInput emitInput)
    {
        using var builder = new SourceBuilder(context, $"{emitInput.TargetType.MakeStandardHintName()}.cs");

        var internalSelfNullableTypeName = $"{emitInput.TargetType.NameWithGenericParams}{(emitInput.TargetType.IsValueType ? "" : "?")}";

#if !DEBUG
        builder.AppendLine(SourceBuilder.AutoGeneratedComment);
#endif

        builder.AppendLine(emitInput.IncludeUsingStatements);

        builder.AppendLine("#nullable enable");
        builder.AppendLine("#pragma warning disable CS0612,CS0618,CS0619");
        builder.AppendLine("#pragma warning disable CS0436");

        using (builder.BeginTypeDefinitionBlock(emitInput.TargetType, TypeDefinitionBlockOptions.Simple with { TypeDeclarationLineTail = $" // This is generated by EqualsGenerator." }))
        {
            var members = emitInput.TargetMembers.Values;

            foreach (var targetMemberWarningComment in members.Select(v => v.warningComment).Where(v => v is not null).Select(v => v!))
                builder.AppendLine(targetMemberWarningComment);

            foreach (var baseTypeEqualsType in emitInput.EqualsOverrideTargetTypes.Values)
            {
                using (builder.BeginBlock($@"public override bool Equals({baseTypeEqualsType.ToString()} other)"))
                {
                    builder.PutIndentSpace();
                    builder.Append("return other is ");
                    builder.Append(emitInput.TargetType.NameWithGenericParams);
                    builder.Append(" _other && this.Equals(_other);");
                    builder.AppendLine();
                }
            }

            var hashCodeCacheField = members.FirstOrDefault(v => v.isHashCodeCache);

            using (builder.BeginBlock($@"public override int GetHashCode()"))
            {
                SourceBuilder._BlockEndDisposable block = default;
                try
                {
                    if (hashCodeCacheField?.symbol is not null)
                    {
                        block = builder.BeginBlock($@"if ({hashCodeCacheField.symbol.Name} == 0)");
                    }

                    builder.PutIndentSpace();
                    builder.AppendLine($@"var hashCode = new System.HashCode();");

                    if (!emitInput.TargetType.IsValueType && emitInput.TargetType is CsClass { BaseType: not null })
                    {
                        builder.PutIndentSpace();
                        builder.AppendLine("hashCode.Add(base.GetHashCode());");
                    }

                    foreach (var member in members.Where(v => !v.isHashCodeCache))
                    {
                        builder.PutIndentSpace();
                        builder.Append("hashCode.Add(this.");
                        builder.Append(member.symbol.Name);
                        builder.Append(", ");
                        if (member.nullFallbackEqualityComparer is not null)
                        {
                            builder.Append(member.symbol.Name);
                            builder.Append(" is null ? ");
                            builder.Append(member.nullFallbackEqualityComparer);
                            builder.Append(" : ");
                            builder.Append(member.equalityComparer);
                        }
                        else
                        {
                            builder.Append(member.equalityComparer);
                        }
                        builder.Append(");");
                        builder.AppendLine();
                    }

                    if (hashCodeCacheField?.symbol is not null)
                    {
                        builder.PutIndentSpace();
                        builder.AppendLine($@"this.{hashCodeCacheField.symbol.Name} = hashCode.ToHashCode();");
                        builder.PutIndentSpace();
                        builder.AppendLine($@"if (this.{hashCodeCacheField.symbol.Name} == 0) this.{hashCodeCacheField.symbol.Name} = 1;");
                    }
                }
                finally
                {
                    block.Dispose();
                }

                if (hashCodeCacheField?.symbol is not null)
                {
                    builder.PutIndentSpace();
                    builder.AppendLine($@"return this.{hashCodeCacheField.symbol.Name};");
                }
                else
                {
                    builder.PutIndentSpace();
                    builder.AppendLine($@"return hashCode.ToHashCode();");
                }
            }

            using (builder.BeginBlock($@"public {(emitInput.TargetType.CanInherit ? "virtual" : "")} bool Equals({internalSelfNullableTypeName} other)"))
            {
                if (!emitInput.TargetType.IsValueType)
                {
                    builder.PutIndentSpace();
                    builder.AppendLine($@"if (other is null) return false;");

                    builder.PutIndentSpace();
                    builder.AppendLine($@"if (object.ReferenceEquals(this, other)) return true;");
                }

                builder.PutIndentSpace();
                builder.AppendLine($@"return true");

                if (hashCodeCacheField?.symbol is not null)
                {
                    builder.PutIndentSpace();
                    builder.AppendLine($@"  && this.GetHashCode() == other.GetHashCode()");
                }

                if (!emitInput.TargetType.IsValueType && emitInput.TargetType is CsClass { BaseType: not null })
                {
                    builder.PutIndentSpace();
                    builder.AppendLine("  && base.Equals(other)");
                }

                foreach (var member in members.Where(v => !v.isHashCodeCache))
                {
                    builder.PutIndentSpace();
                    builder.Append("  && ");
                    if (member.nullFallbackEqualityComparer is not null)
                    {
                        builder.Append("(");
                        builder.Append(member.symbol.Name);
                        builder.Append(" is null ? ");
                        builder.Append(member.nullFallbackEqualityComparer);
                        builder.Append(".Equals(this.");
                        builder.Append(member.symbol.Name);
                        builder.Append(", other.");
                        builder.Append(member.symbol.Name);
                        builder.Append(") : ");
                        builder.Append(member.equalityComparer);
                        builder.Append(".Equals(this.");
                        builder.Append(member.symbol.Name);
                        builder.Append(", other.");
                        builder.Append(member.symbol.Name);
                        builder.Append("))");
                        builder.AppendLine();
                    }
                    else
                    {
                        builder.Append(member.equalityComparer);
                        builder.Append(".Equals(this.");
                        builder.Append(member.symbol.Name);
                        builder.Append(", other.");
                        builder.Append(member.symbol.Name);
                        builder.Append(")");
                        builder.AppendLine();
                    }
                }

                builder.PutIndentSpace();
                builder.AppendLine("  ;");
            }

            if (emitInput.AutomaticEqualsImplOptions.HasFlag(AutomaticEqualsImplOptions.WithOperator))
            {
                using (builder.BeginBlock($@"public static bool operator ==({internalSelfNullableTypeName} left, {internalSelfNullableTypeName} right)"))
                {
                    if (emitInput.TargetType.IsValueType)
                    {
                        builder.PutIndentSpace();
                        builder.AppendLine($@"return left.Equals(right);");
                    }
                    else
                    {
                        using (builder.BeginBlock("if (left is null)"))
                        {
                            using (builder.BeginBlock("if (right is null)"))
                            {
                                builder.PutIndentSpace();
                                builder.AppendLine($@"return true;");
                            }
                            using (builder.BeginBlock("else"))
                            {
                                builder.PutIndentSpace();
                                builder.AppendLine($@"return false;");
                            }
                        }
                        using (builder.BeginBlock("else"))
                        {
                            builder.PutIndentSpace();
                            builder.AppendLine($@"return left.Equals(right);");
                        }
                    }
                }

                using (builder.BeginBlock($@"public static bool operator !=({internalSelfNullableTypeName} left, {internalSelfNullableTypeName} right)"))
                {
                    builder.PutIndentSpace();
                    builder.AppendLine($@"return !(left == right);");
                }
            }
        }

        builder.Commit();
    }
}
