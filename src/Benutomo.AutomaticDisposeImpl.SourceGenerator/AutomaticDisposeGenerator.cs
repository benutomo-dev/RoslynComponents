using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator
{
    [Generator(LanguageNames.CSharp)]
    public partial class AutomaticDisposeGenerator : IIncrementalGenerator
    {
#if DEBUG
        static StreamWriter s_streamWriter;
        static AutomaticDisposeGenerator()
        {
            Directory.CreateDirectory(@"c:\var\log\AutomaticDisposeGenerator");
            var proc = Process.GetCurrentProcess();
            s_streamWriter = new StreamWriter($@"c:\var\log\AutomaticDisposeGenerator\{DateTime.Now:yyyyMMddHHmmss}_{proc.Id}.txt");
            s_streamWriter.WriteLine(proc);
        }

        [Conditional("DEBUG")]
        static void WriteLogLine(string line)
        {
            lock (s_streamWriter)
            {
                s_streamWriter.WriteLine(line);
                s_streamWriter.Flush();
            }
        }
#else
        [Conditional("DEBUG")]
        static void WriteLogLine(string line)
        {
        }
#endif

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            WriteLogLine("Begin Initialize");

            StaticSources.StaticSource.Register(context);

            var usingSymbols = context.CompilationProvider
                .Select((compilation, cancellationToken) =>
                {
                    WriteLogLine("Begin GetTypeByMetadataName");

                    try
                    {
                        return UsingSymbols.CreateFrom(compilation);
                    }
                    catch (OperationCanceledException)
                    {
                        WriteLogLine("Canceled GetTypeByMetadataName");
                        throw;
                    }
                    catch (Exception ex)
                    {
                        WriteLogLine("Exception GetTypeByMetadataName");
                        WriteLogLine(ex.ToString());
                        throw;
                    }
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

            WriteLogLine("End Initialize");
        }

        bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
        {
            //WriteLogLine("Predicate");

            return node is ClassDeclarationSyntax
            {
                AttributeLists.Count: > 0
            };
        }

        INamedTypeSymbol? Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            WriteLogLine("Begin Transform");
            try
            {
                var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

                if (!classDeclarationSyntax.Modifiers.Any(modifier => modifier.ValueText == "partial"))
                {
                    return null;
                }

                var namedTypeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, cancellationToken) as INamedTypeSymbol;

                WriteLogLine($"End Transform ({namedTypeSymbol?.ContainingType?.Name}.{namedTypeSymbol?.Name})");

                return namedTypeSymbol;
            }
            catch (OperationCanceledException)
            {
                WriteLogLine($"Canceled Transform");
                throw;
            }
        }

        MethodSourceBuilderInputs? PostTransform((INamedTypeSymbol? Left, UsingSymbols Right) v, CancellationToken ct)
        {
            var namedTypeSymbol = v.Left;
            var usingSymbols = v.Right;

            if (namedTypeSymbol is null) return null;

            WriteLogLine($"Begin PostTransform ({namedTypeSymbol.Name})");

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

                WriteLogLine($"End PostTransform ({namedTypeSymbol.Name})");

                return result;
            }
            catch (OperationCanceledException)
            {
                WriteLogLine($"Canceled PostTransform ({namedTypeSymbol.Name})");
                throw;
            }
            catch (Exception ex)
            {
                WriteLogLine($"Exception PostTransform ({namedTypeSymbol.Name})");
                WriteLogLine(ex.ToString());
                throw;
            }
        }

        void Generate(SourceProductionContext context, MethodSourceBuilderInputs? sourceBuildInputs)
        {
            if (sourceBuildInputs is null) return;

            WriteLogLine($"Begin Generate ({sourceBuildInputs.TargetTypeInfo.Name})");

            try
            {
                Span<char> initialBuffer = stackalloc char[80000];

                using var sourceBuilder = new MethodSourceBuilder(context, sourceBuildInputs, initialBuffer);

                sourceBuilder.Build();

                context.AddSource(sourceBuilder.HintName, sourceBuilder.SourceText);

                WriteLogLine($"End Generate ({sourceBuildInputs.TargetTypeInfo.Name}) => {sourceBuilder.HintName}");
            }
            catch (OperationCanceledException)
            {
                WriteLogLine($"Canceled Generate ({sourceBuildInputs.TargetTypeInfo.Name})");
                throw;
            }
            catch (Exception ex)
            {
                WriteLogLine($"Exception in Generate ({sourceBuildInputs.TargetTypeInfo.Name})");
                WriteLogLine(ex.ToString());
                throw;
            }
        }
    }
}
