using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator
{
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

            // Where句を使用しない。
            // https://github.com/dotnet/roslyn/issues/57991
            // 今は、Where句を使用するとSource GeneratorがVSでインクリメンタルに実行されたときに
            // 対象のコードの状態や編集内容などによって突然内部状態が壊れて機能しなくなる問題がおきる。

            var anotatedClasses = context.SyntaxProvider
                .CreateSyntaxProvider(Predicate, Transform)
                //.Where(v => v is not null)
                .Combine(usingSymbols)
                .Select(PostTransform)
                ;//.Where(v => v is not null);

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

        MethodSourceBuilderInputs? PostTransform((INamedTypeSymbol? Left, UsingSymbols Right) v, CancellationToken ct)
        {
            var namedTypeSymbol = v.Left;
            var usingSymbols = v.Right;

            if (namedTypeSymbol is null) return null;

            try
            {
                var automaticDisposeImplAttributeData = namedTypeSymbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass.IsSameSymbolTo(usingSymbols.AutomaticDisposeImplAttribute));
                if (automaticDisposeImplAttributeData is null)
                {
                    return null;
                }

                if (!namedTypeSymbol.IsAssignableTo(usingSymbols.IDisposable) && (usingSymbols.IAsyncDisposable is null || !namedTypeSymbol.IsAssignableTo(usingSymbols.IAsyncDisposable)))
                {
                    return null;
                }

                var result = new MethodSourceBuilderInputs(namedTypeSymbol, usingSymbols, automaticDisposeImplAttributeData);

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

                using var sourceBuilder = new MethodSourceBuilder(context, sourceBuildInputs, initialBuffer);

                sourceBuilder.Build();

                context.AddSource(sourceBuilder.HintName, sourceBuilder.SourceText);
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
}
