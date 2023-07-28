using Microsoft.CodeAnalysis;

namespace Benutomo.SourceGeneratorCommons;

internal static partial class SymbolExtensions
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
}
