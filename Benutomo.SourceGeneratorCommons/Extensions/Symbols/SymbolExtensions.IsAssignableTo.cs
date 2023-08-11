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
            if (assignTargetTypeSymbol.TypeKind == TypeKind.TypeParameter)
            {
                // TODO: 本当は代入先の型制約に代入元の型制約が包含される場合にtrue
                return false;
            }
            else
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
        }
        else
        {
            if (assignTargetTypeSymbol.TypeKind == TypeKind.Interface)
            {
                foreach (var interfaceType in typeSymbol.AllInterfaces)
                {
                    if (IsAssignableTo(interfaceType, assignTargetTypeSymbol))
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
            else if (assignTargetTypeSymbol is ITypeParameterSymbol assignTargetTypeParameterSymbol)
            {
                // ジェネリック型の型パラメータの場合は型パラメータの制約を確認

                if (true
                    && (!assignTargetTypeParameterSymbol.HasReferenceTypeConstraint || !typeSymbol.IsValueType)
                    && (!assignTargetTypeParameterSymbol.HasValueTypeConstraint || typeSymbol.IsValueType)
                    && (!assignTargetTypeParameterSymbol.HasUnmanagedTypeConstraint || typeSymbol.IsUnmanagedType)
                    && (!assignTargetTypeParameterSymbol.HasConstructorConstraint || typeSymbol.GetMembers().Any(v => v is IMethodSymbol { MethodKind: MethodKind.Constructor, Parameters.Length: 0 }))
                    && assignTargetTypeParameterSymbol.ConstraintTypes.All(constraintType => IsAssignableTo(constraintType, assignTargetTypeSymbol))
                    )
                {
                    return true;
                }
            }
        }

        if (true
            && assignTargetTypeSymbol is INamedTypeSymbol { IsGenericType: true } assignTargetGenericTypeSymbol
            && assignTargetGenericTypeSymbol.TypeArguments.Any(v => v.TypeKind == TypeKind.TypeParameter)
            && typeSymbol is INamedTypeSymbol { IsGenericType: true } genericTypeSymbol
            && SymbolEqualityComparer.Default.Equals(genericTypeSymbol.ConstructUnboundGenericType(), assignTargetGenericTypeSymbol.ConstructUnboundGenericType())
            )
        {
            for (int i = 0; i < genericTypeSymbol.TypeArguments.Length; i++)
            {
                if (!IsAssignableTo(genericTypeSymbol.TypeArguments[i], assignTargetGenericTypeSymbol.TypeArguments[i]))
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }
}
