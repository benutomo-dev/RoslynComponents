using Microsoft.CodeAnalysis;
using System.Diagnostics;

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
    }
}
