using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;

namespace Benutomo.SourceGeneratorCommons;

internal static partial class PropertySymbolExtensions
{
    internal static bool IsAutoImplimenedProperty(this IPropertySymbol propertySymbol, CancellationToken cancellationToken)
    {
        if (propertySymbol.IsIndexer)
        {
            ;
        }

        foreach (var syntaxReference in propertySymbol.DeclaringSyntaxReferences)
        {
            var syntax = syntaxReference.GetSyntax(cancellationToken);

            if (syntax is PropertyDeclarationSyntax propertyDeclarationSyntax)
            {
                if (propertyDeclarationSyntax.ExpressionBody is not null)
                {
                    return false;
                }

                if (propertyDeclarationSyntax.AccessorList is null)
                {
                    continue;
                }

                foreach (var accesorDeclarationSyntax in propertyDeclarationSyntax.AccessorList.Accessors)
                {
                    if (accesorDeclarationSyntax.Body is not null)
                    {
                        return false;
                    }

                    if (accesorDeclarationSyntax.ExpressionBody is not null)
                    {
                        return false;
                    }
                }
            }
            else if (syntax is IndexerDeclarationSyntax indexerDeclarationSyntax)
            {
                if (indexerDeclarationSyntax.ExpressionBody is not null)
                {
                    return false;
                }

                if (indexerDeclarationSyntax.AccessorList is null)
                {
                    continue;
                }

                foreach (var accesorDeclarationSyntax in indexerDeclarationSyntax.AccessorList.Accessors)
                {
                    if (accesorDeclarationSyntax.Body is not null)
                    {
                        return false;
                    }

                    if (accesorDeclarationSyntax.ExpressionBody is not null)
                    {
                        return false;
                    }
                }
            }
            else
            {
                Debug.Fail(null);
                return false;
            }
        }

        return true;
    }
}
