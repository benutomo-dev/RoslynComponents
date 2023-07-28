using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Benutomo.SourceGeneratorCommons;

internal static partial class PropertySymbolExtensions
{
    internal static bool IsAutoImplimenedProperty(this IPropertySymbol propertySymbol, CancellationToken cancellationToken)
    {
        var isAutoImplementedProperty = propertySymbol.DeclaringSyntaxReferences
            .Select(v => v.GetSyntax(cancellationToken))
            .OfType<PropertyDeclarationSyntax>()
            .All(v => v.ExpressionBody is null && (v.AccessorList?.Accessors.All(v => v.ExpressionBody is null && v.Body is null) ?? true));

        return isAutoImplementedProperty;
    }
}
