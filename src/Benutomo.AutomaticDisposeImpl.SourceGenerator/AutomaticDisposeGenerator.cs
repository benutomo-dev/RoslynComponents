using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public partial class AutomaticDisposeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        StaticSources.StaticSource.Register(context);

        var usingSymbols = context.CompilationProvider
            .Select((compilation, cancellationToken) =>
            {
                return UsingSymbols.CreateFrom(compilation);
            });

        var csDeclarationProvider = context.CreateCsDeclarationProvider();

        var anotatedClasses = context.SyntaxProvider
            .CreateSyntaxProvider(Predicate, Transform)
            .Where(v => v is not null)
            .Combine(usingSymbols)
            .Select((v, _) => (namedTypeSymbol: v.Left, usingSymbols: v.Right))
            .Combine(csDeclarationProvider)
            .Select((v, _) => (v.Left.namedTypeSymbol, v.Left.usingSymbols, csDeclarationProvider: v.Right))
            .Select(PostTransform)
            .Where(v => v is not null);

        context.RegisterSourceOutput(anotatedClasses, Generate);
    }

    bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
    {
        return node is ClassDeclarationSyntax
        {
            AttributeLists.Count: > 0
        };
    }

    INamedTypeSymbol? Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        try
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

            if (!classDeclarationSyntax.Modifiers.Any(modifier => modifier.ValueText == "partial"))
            {
                return null;
            }

            var namedTypeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, cancellationToken) as INamedTypeSymbol;

            return namedTypeSymbol;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
    }

    MethodSourceBuilderInputs? PostTransform((INamedTypeSymbol? namedTypeSymbol, UsingSymbols usingSymbols, CsDeclarationProvider csDeclarationProvider) v, CancellationToken ct)
    {
        var (namedTypeSymbol, usingSymbols, csDeclarationProvider) = v;

        if (namedTypeSymbol is null) return null;

        try
        {
            var automaticDisposeImplAttributeData = namedTypeSymbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass.IsSameSymbolTo(usingSymbols.AutomaticDisposeImplAttribute));
            if (automaticDisposeImplAttributeData is null)
            {
                return null;
            }

            if (!namedTypeSymbol.IsAssignableTo(usingSymbols.IDisposable, csDeclarationProvider.Compilation) && (usingSymbols.IAsyncDisposable is null || !namedTypeSymbol.IsAssignableTo(usingSymbols.IAsyncDisposable, csDeclarationProvider.Compilation)))
            {
                return null;
            }

            var result = new MethodSourceBuilderInputs(namedTypeSymbol, usingSymbols, csDeclarationProvider, automaticDisposeImplAttributeData);

            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }

    void Generate(SourceProductionContext context, MethodSourceBuilderInputs? sourceBuildInputs)
    {
        if (sourceBuildInputs is null) return;

        try
        {
            Span<char> initialBuffer = stackalloc char[80000];

            using var sourceBuilder = new MethodSourceBuilder(context, sourceBuildInputs);

            sourceBuilder.Build();

            sourceBuilder.Commit();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
