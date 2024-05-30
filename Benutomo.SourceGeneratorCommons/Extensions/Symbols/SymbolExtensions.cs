using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Benutomo.SourceGeneratorCommons;

internal static partial class SymbolExtensions
{
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

    internal static ITypeSymbol? GetTypeOfExpressionSymbol(this ISymbol? symbol)
    {
        return symbol switch
        {
            IMethodSymbol method => method.ReturnType,
            IPropertySymbol property => property.Type,
            IFieldSymbol field => field.Type,
            ILocalSymbol local => local.Type,
            IParameterSymbol parameter => parameter.Type,
            _ => null,
        };
    }
}
