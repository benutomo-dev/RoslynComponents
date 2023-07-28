using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Benutomo.SourceGeneratorCommons;

internal static partial class SymbolExtensions
{
    internal static void AppendTypeName(this StringBuilder stringBuilder, ITypeSymbol typeSymbol) => AppendTypeNameCore(new Appender(stringBuilder), typeSymbol, includeContainerNames: false);

    internal static void AppendTypeName(this SourceBuilderEx sourceBuilder, ITypeSymbol typeSymbol) => AppendTypeNameCore(new Appender(sourceBuilder), typeSymbol, includeContainerNames: false);


    internal static void AppendFullTypeName(this StringBuilder stringBuilder, ITypeSymbol typeSymbol) => AppendTypeNameCore(new Appender(stringBuilder), typeSymbol, includeContainerNames: true);

    internal static void AppendFullTypeName(this SourceBuilderEx sourceBuilder, ITypeSymbol typeSymbol) => AppendTypeNameCore(new Appender(sourceBuilder), typeSymbol, includeContainerNames: true);

    private static void AppendTypeNameCore(Appender appender, ITypeSymbol typeSymbol, bool includeContainerNames)
    {
        if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            AppendTypeNameCore(appender, arrayTypeSymbol.ElementType, includeContainerNames);
            appender.Append("[");
            for (var i = 1; i < arrayTypeSymbol.Rank; i++)
            {
                appender.Append(",");
            }
            appender.Append("]");
        }
        else if (typeSymbol is ITypeParameterSymbol typeParameterSymbol)
        {
            appender.Append(typeParameterSymbol.Name);

            if (typeParameterSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                appender.Append("?");
            }
        }
        else
        {
            if (includeContainerNames)
            {
                if (typeSymbol.ContainingType is null)
                {
                    appender.Append("global::");
                    AppendFullNamespace(appender, typeSymbol.ContainingNamespace);
                }
                else
                {
                    AppendTypeNameCore(appender, typeSymbol.ContainingType, includeContainerNames);
                }
                appender.Append(".");
            }

            appender.Append(typeSymbol.Name);

            if (typeSymbol is INamedTypeSymbol namedTypeSymbol && !namedTypeSymbol.TypeArguments.IsDefaultOrEmpty)
            {
                var typeArguments = namedTypeSymbol.TypeArguments;

                appender.Append("<");

                for (int i = 0; i < typeArguments.Length - 1; i++)
                {
                    AppendTypeNameCore(appender, typeArguments[i], includeContainerNames);
                    appender.Append(", ");
                }
                AppendTypeNameCore(appender, typeArguments[typeArguments.Length - 1], includeContainerNames);

                appender.Append(">");
            }

            if (typeSymbol.IsReferenceType && typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                appender.Append("?");
            }
        }
    }

    internal static void AppendFullNamespace(this StringBuilder stringBuilder, INamespaceSymbol namespaceSymbol) => AppendFullNamespace(new Appender(stringBuilder), namespaceSymbol);

    internal static void AppendFullNamespace(this SourceBuilderEx sourceBuilder, INamespaceSymbol namespaceSymbol) => AppendFullNamespace(new Appender(sourceBuilder), namespaceSymbol);

    private static void AppendFullNamespace(Appender appender, INamespaceSymbol namespaceSymbol)
    {
        if (namespaceSymbol.ContainingNamespace is not null && !namespaceSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            AppendFullNamespace(appender, namespaceSymbol.ContainingNamespace);
            appender.Append(".");
        }

        appender.Append(namespaceSymbol.Name);
    }

    struct Appender
    {
        object _instance;

        public Appender(StringBuilder stringBuilder) { _instance = stringBuilder; }

        public Appender(SourceBuilderEx sourceBuilderEx) { _instance = sourceBuilderEx; }

        public void Append(string value)
        {
            if (_instance is StringBuilder stringBuilder)
            {
                stringBuilder.Append(value);
            }
            else if (_instance is SourceBuilderEx sourceBuilder)
            {
                sourceBuilder.Append(value);
            }
            else
            {
                Throw();
            }

            static void Throw() => throw new InvalidOperationException();
        }
    }
}
