using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Benutomo.SourceGeneratorCommons
{
    internal static class SymbolHelper
    {
        internal static bool IsAssignableTo(this IFieldSymbol? fieldSymbol, ITypeSymbol assignTargetTypeSymbol) => IsAssignableTo(fieldSymbol?.Type, assignTargetTypeSymbol);

        internal static bool IsAssignableTo(this IPropertySymbol? propertySymbol, ITypeSymbol assignTargetTypeSymbol) => IsAssignableTo(propertySymbol?.Type, assignTargetTypeSymbol);

        internal static bool IsAssignableTo(this ILocalSymbol? localSymbol, ITypeSymbol assignTargetTypeSymbol) => IsAssignableTo(localSymbol?.Type, assignTargetTypeSymbol);

        internal static bool IsAssignableTo(this ITypeSymbol? typeSymbol, ITypeSymbol assignTargetTypeSymbol)
        {
            if (typeSymbol is null) return false;

            var comparer = SymbolEqualityComparer.Default;

            if (comparer.Equals(typeSymbol, assignTargetTypeSymbol)) return true;
            
            if (typeSymbol is ITypeParameterSymbol typeParameterSymbol)
            {
                // ジェネリック型の型パラメータの場合は型パラメータの制約を再帰的に確認

                foreach (var constraintType in typeParameterSymbol.ConstraintTypes)
                {
                    if (IsAssignableTo(constraintType, assignTargetTypeSymbol))
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (assignTargetTypeSymbol.TypeKind == TypeKind.Interface)
                {
                    foreach (var interfaceType in typeSymbol.AllInterfaces)
                    {
                        if (comparer.Equals(interfaceType, assignTargetTypeSymbol))
                        {
                            return true;
                        }
                    }
                }
                else if (assignTargetTypeSymbol.TypeKind == TypeKind.Class)
                {
                    if (!assignTargetTypeSymbol.IsSealed && typeSymbol.BaseType is not null)
                    {
                        if (IsAssignableTo(typeSymbol.BaseType, assignTargetTypeSymbol))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        internal static bool IsSameSymbolTo(this ISymbol? self, ISymbol? otherSymbol) => SymbolEqualityComparer.Default.Equals(self, otherSymbol);

        internal static bool IsAttributedBy(this ISymbol? symbol, ITypeSymbol attibuteTypeSymbol)
        {
            if (symbol is null) return false;

            Debug.Assert(attibuteTypeSymbol?.BaseType?.Name == "Attribute");

            foreach (var attributeData in symbol.GetAttributes())
            {
                if (attributeData.AttributeClass is not null && SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, attibuteTypeSymbol))
                {
                    return true;
                }
            }

            return false;
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

        internal static void AppendFullTypeName(this StringBuilder typeNameBuilder, ITypeSymbol typeSymbol)
        {
            if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                AppendFullTypeName(typeNameBuilder, arrayTypeSymbol.ElementType);
                typeNameBuilder.Append("[");
                for (var i = 1; i < arrayTypeSymbol.Rank; i++)
                {
                    typeNameBuilder.Append(",");
                }
                typeNameBuilder.Append("]");
            }
            else if (typeSymbol is ITypeParameterSymbol typeParameterSymbol)
            {
                typeNameBuilder.Append(typeParameterSymbol.Name);

                if (typeParameterSymbol.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    typeNameBuilder.Append("?");
                }
            }
            else
            {
                if (typeSymbol.ContainingType is null)
                {
                    typeNameBuilder.Append("global::");
                    AppendFullNamespace(typeNameBuilder, typeSymbol.ContainingNamespace);
                }
                else
                {
                    AppendFullTypeName(typeNameBuilder, typeSymbol.ContainingType);
                }
                typeNameBuilder.Append(".");

                typeNameBuilder.Append(typeSymbol.Name);

                if (typeSymbol is INamedTypeSymbol namedTypeSymbol && !namedTypeSymbol.TypeArguments.IsDefaultOrEmpty)
                {
                    var typeArguments = namedTypeSymbol.TypeArguments;

                    typeNameBuilder.Append("<");

                    for (int i = 0; i < typeArguments.Length - 1; i++)
                    {
                        AppendFullTypeName(typeNameBuilder, typeArguments[i]);
                        typeNameBuilder.Append(", ");
                    }
                    AppendFullTypeName(typeNameBuilder, typeArguments[typeArguments.Length - 1]);

                    typeNameBuilder.Append(">");
                }

                if (typeSymbol.IsReferenceType && typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    typeNameBuilder.Append("?");
                }
            }
        }

        internal static void AppendFullNamespace(this StringBuilder namespaceNameBuilder, INamespaceSymbol namespaceSymbol)
        {
            if (namespaceSymbol.ContainingNamespace is not null && !namespaceSymbol.ContainingNamespace.IsGlobalNamespace)
            {
                AppendFullNamespace(namespaceNameBuilder, namespaceSymbol.ContainingNamespace);
                namespaceNameBuilder.Append(".");
            }

            namespaceNameBuilder.Append(namespaceSymbol.Name);
        }

        internal static TypeDefinitionInfo BuildTypeDefinitionInfo(this ITypeSymbol typeSymbol)
        {
            ITypeContainer container;

            if (typeSymbol.ContainingType is null)
            {
                var namespaceBuilder = new StringBuilder();
                AppendFullNamespace(namespaceBuilder, typeSymbol.ContainingNamespace);

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
    }
}
