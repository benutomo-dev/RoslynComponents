using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Benutomo.SourceGeneratorCommons;

internal static partial class TypeSymbolExtensions
{
    internal static bool IsInterlockedExchangable(this ITypeSymbol typeSymbol)
    {
        return typeSymbol switch
        {
            { IsReferenceType: true } => true,
            { SpecialType: SpecialType.System_Int32 } => true,
            { SpecialType: SpecialType.System_Int64 } => true,
            { SpecialType: SpecialType.System_IntPtr } => true,
            { SpecialType: SpecialType.System_Single } => true,
            { SpecialType: SpecialType.System_Double } => true,
            _ => false,
        };
    }

    internal static TypeDefinitionInfo BuildTypeDefinitionInfo(this ITypeSymbol typeSymbol)
    {
        ITypeContainer container;

        if (typeSymbol.ContainingType is null)
        {
            var namespaceBuilder = new StringBuilder();
            SymbolExtensions.AppendFullNamespace(namespaceBuilder, typeSymbol.ContainingNamespace);

            container = new NameSpaceInfo(namespaceBuilder.ToString());
        }
        else
        {
            container = BuildTypeDefinitionInfo(typeSymbol.ContainingType);
        }

        ImmutableArray<string> genericTypeArgs = ImmutableArray<string>.Empty;

        if (typeSymbol is INamedTypeSymbol namedTypeSymbol && !namedTypeSymbol.TypeArguments.IsDefaultOrEmpty)
        {
            var builder = ImmutableArray.CreateBuilder<string>(namedTypeSymbol.TypeArguments.Length);

            for (int i = 0; i < namedTypeSymbol.TypeArguments.Length; i++)
            {
                builder.Add(namedTypeSymbol.TypeArguments[i].Name);
            }

            genericTypeArgs = builder.MoveToImmutable();
        }

        return new TypeDefinitionInfo(container, typeSymbol.Name, typeSymbol.IsValueType, typeSymbol.NullableAnnotation == NullableAnnotation.Annotated, genericTypeArgs);
    }

    internal static bool IsXSymbolImpl(ITypeSymbol? typeSymbol, string ns1, string typeName)
    {
        Debug.Assert(!ns1.Contains("."));
        Debug.Assert(!typeName.Contains("."));

        if (typeSymbol is null) return false;

        if (typeSymbol.Name != typeName) return false;

        var containingNamespaceSymbol = typeSymbol.ContainingNamespace;

        if (containingNamespaceSymbol is null) return false;

        if (containingNamespaceSymbol.Name != ns1) return false;

        if (containingNamespaceSymbol.ContainingNamespace is null) return false;

        if (!containingNamespaceSymbol.ContainingNamespace.IsGlobalNamespace) return false;

        return true;
    }


    internal static bool IsAssignableToIXImpl(ITypeSymbol? typeSymbol, Func<ITypeSymbol, bool> isXTypeFunc)
    {
        if (typeSymbol is null) return false;

        if (isXTypeFunc(typeSymbol)) return true;

        if (typeSymbol.AllInterfaces.Any((Func<INamedTypeSymbol, bool>)isXTypeFunc)) return true;

        // ジェネリック型の型パラメータの場合は型パラメータの制約を再帰的に確認
        if (typeSymbol is ITypeParameterSymbol typeParameterSymbol && typeParameterSymbol.ConstraintTypes.Any(constraintType => IsAssignableToIXImpl(constraintType, isXTypeFunc)))
        {
            return true;
        }

        return false;
    }

    internal static bool IsXAttributedMemberImpl(ISymbol? symbol, Func<INamedTypeSymbol, bool> isXAttributeSymbol)
    {
        if (symbol is null) return false;

        foreach (var attributeData in symbol.GetAttributes())
        {
            if (attributeData.AttributeClass is not null && isXAttributeSymbol(attributeData.AttributeClass))
            {
                return true;
            }
        }

        return false;
    }

}
