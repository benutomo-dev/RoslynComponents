using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Benutomo.SourceGeneratorCommons;

internal static partial class SymbolExtensions
{
    private enum TypeNameEmitMode
    {
        SimpleTypeName,
        ReflectionFullTypeName,
        SourceEmbeddingFullTypeName,
        Cref,
    }

    internal static void AppendTypeName(this StringBuilder stringBuilder, ITypeSymbol typeSymbol) => AppendTypeNameCore(new Appender(stringBuilder), typeSymbol, mode: TypeNameEmitMode.SimpleTypeName);

    internal static void AppendTypeName(this SourceBuilderEx sourceBuilder, ITypeSymbol typeSymbol) => AppendTypeNameCore(new Appender(sourceBuilder), typeSymbol, mode: TypeNameEmitMode.SimpleTypeName);

    internal static void AppendFullTypeName(this StringBuilder stringBuilder, ITypeSymbol typeSymbol) => AppendTypeNameCore(new Appender(stringBuilder), typeSymbol, mode: TypeNameEmitMode.ReflectionFullTypeName);

    internal static void AppendFullTypeName(this SourceBuilderEx sourceBuilder, ITypeSymbol typeSymbol) => AppendTypeNameCore(new Appender(sourceBuilder), typeSymbol, mode: TypeNameEmitMode.ReflectionFullTypeName);

    internal static void AppendFullTypeNameWithNamespaceAlias(this StringBuilder stringBuilder, ITypeSymbol typeSymbol) => AppendTypeNameCore(new Appender(stringBuilder), typeSymbol, mode: TypeNameEmitMode.SourceEmbeddingFullTypeName);

    internal static void AppendFullTypeNameWithNamespaceAlias(this SourceBuilderEx sourceBuilder, ITypeSymbol typeSymbol) => AppendTypeNameCore(new Appender(sourceBuilder), typeSymbol, mode: TypeNameEmitMode.SourceEmbeddingFullTypeName);

    private static void AppendTypeNameCore(Appender appender, ITypeSymbol typeSymbol, TypeNameEmitMode mode)
    {
        if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            AppendTypeNameCore(appender, arrayTypeSymbol.ElementType, mode);
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
            if (mode != TypeNameEmitMode.SimpleTypeName)
            {
                if (typeSymbol.ContainingType is null)
                {
                    if (mode == TypeNameEmitMode.SourceEmbeddingFullTypeName)
                    {
                        appender.Append("global::");
                    }
                    AppendFullNamespace(appender, typeSymbol.ContainingNamespace);
                }
                else
                {
                    AppendTypeNameCore(appender, typeSymbol.ContainingType, mode);
                }
                appender.Append(".");
            }

            appender.Append(typeSymbol.Name);

            if (typeSymbol is INamedTypeSymbol namedTypeSymbol && !namedTypeSymbol.TypeArguments.IsDefaultOrEmpty)
            {
                var typeArguments = namedTypeSymbol.TypeArguments;

                appender.Append(mode == TypeNameEmitMode.Cref ? "{" : "<");

                for (int i = 0; i < typeArguments.Length - 1; i++)
                {
                    AppendTypeNameCore(appender, typeArguments[i], mode);
                    appender.Append(", ");
                }
                AppendTypeNameCore(appender, typeArguments[typeArguments.Length - 1], mode);

                appender.Append(mode == TypeNameEmitMode.Cref ? "}" : ">");
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


    internal static void AppendCref(this StringBuilder stringBuilder, IMethodSymbol methodSymbol) => AppendCref(new Appender(stringBuilder), methodSymbol);

    internal static void AppendCref(this SourceBuilderEx sourceBuilder, IMethodSymbol methodSymbol) => AppendCref(new Appender(sourceBuilder), methodSymbol);

    private static void AppendCref(Appender appender, IMethodSymbol methodSymbol)
    {
        AppendTypeNameCore(appender, methodSymbol.ContainingType, mode: TypeNameEmitMode.Cref);
        appender.Append(".");
        appender.Append(methodSymbol.Name);
        if (methodSymbol.IsGenericMethod)
        {
            appender.Append("{");
            appender.Append(methodSymbol.TypeParameters[0].Name);
            for (int i = 1; i < methodSymbol.TypeParameters.Length; i++)
            {
                appender.Append(", ");
                appender.Append(methodSymbol.TypeParameters[i].Name);
            }
            appender.Append("}");
        }
        appender.Append("(");
        for (int i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            if (i > 0)
            {
                appender.Append(", ");
            }
            switch (methodSymbol.Parameters[i].RefKind)
            {
                case RefKind.Ref:
                    appender.Append("ref ");
                    break;
                case RefKind.In:
                    appender.Append("in ");
                    break;
                case RefKind.Out:
                    appender.Append("out ");
                    break;
            }
            AppendTypeNameCore(appender, methodSymbol.Parameters[i].Type, mode: TypeNameEmitMode.Cref);
        }
        appender.Append(")");
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
