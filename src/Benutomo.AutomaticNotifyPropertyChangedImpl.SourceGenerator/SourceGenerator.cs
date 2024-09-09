using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator
{
    [Generator(LanguageNames.CSharp)]
    public partial class SourceGenerator : IIncrementalGenerator
    {
#if DEBUG
        static StreamWriter s_streamWriter;
        static SourceGenerator()
        {
            Directory.CreateDirectory(@"c:\var\log\AutomaticNotifyPropertyChangedImpl");
            var proc = Process.GetCurrentProcess();
            s_streamWriter = new StreamWriter($@"c:\var\log\AutomaticNotifyPropertyChangedImpl\{DateTime.Now:yyyyMMddHHmmss}_{proc.Id}.txt");
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

            var enableNotificationSupportAttributeSymbol = context.CompilationProvider
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

            var anotatedProperties = context.SyntaxProvider.CreateSyntaxProvider(IsAttributeAttachedPropertyDeclarationSystax, ToPropertySymbol);

            var methodSourceBuildInputArgs = anotatedProperties
                .Combine(enableNotificationSupportAttributeSymbol)
                .Select(ToMethodSourceBuildInputArgs);

            context.RegisterSourceOutput(methodSourceBuildInputArgs, GenerateMethod);

            var propertyNameInputArgs = methodSourceBuildInputArgs
                .Collect()
                .SelectMany(toPropertyInputArgs);

            context.RegisterSourceOutput(propertyNameInputArgs, GenerateEventArg);

            WriteLogLine("End Initialize");

            IEnumerable<EventArgSourceBuilderInputs> toPropertyInputArgs(ImmutableArray<MethodSourceBuildInputs?> sourceBuildInputs, CancellationToken cancellationToken)
            {
                foreach (var propertiesInClass in sourceBuildInputs.Where(v => !cancellationToken.IsCancellationRequested && v is not null).ToLookup(v => v!.ContainingTypeInfo))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var properties = Enumerable.Empty<(string, PropertyEventArgClass)>()
                        .Concat(
                            propertiesInClass
                                .Where(v => v?.EnabledNotifyPropertyChanged == true)
                                .SelectMany(v => v!.PropertyEventArgNames.Select(name => (name, PropertyEventArgClass.Changed)))
                        )
                        .Concat(
                            propertiesInClass
                                .Where(v => v?.EnabledNotifyPropertyChanging == true)
                                .SelectMany(v => v!.PropertyEventArgNames.Select(name => (name, PropertyEventArgClass.Changing)))
                        )
                        .Distinct()
                        .ToImmutableArray();

                    yield return new (propertiesInClass.Key, properties);
                }
            }
        }

        bool IsAttributeAttachedPropertyDeclarationSystax(SyntaxNode node, CancellationToken cancellationToken)
        {
            //WriteLogLine("Predicate");

            return node is PropertyDeclarationSyntax
            {
                AttributeLists.Count: > 0
            };
        }

        IPropertySymbol? ToPropertySymbol(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            WriteLogLine("Begin Transform");
            try
            {
                var propertyDeclarationSyntax = (PropertyDeclarationSyntax)context.Node;

                var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax, cancellationToken) as IPropertySymbol;

                WriteLogLine($"End Transform ({propertySymbol?.ContainingType?.Name}.{propertySymbol?.Name})");

                return propertySymbol;
            }
            catch (OperationCanceledException)
            {
                WriteLogLine($"Canceled Transform");
                throw;
            }
        }

        MethodSourceBuildInputs? ToMethodSourceBuildInputArgs((IPropertySymbol? Left, UsingSymbols Right) v, CancellationToken ct)
        {
            var propertySymbol = v.Left;
            var usingSymbols = v.Right;

            if (propertySymbol is null) return null;

            WriteLogLine($"Begin PostTransform ({propertySymbol.ContainingType?.Name}.{propertySymbol.Name})");

            try
            {
                var enableNotificationSupportAttributeData = propertySymbol.GetAttributes().FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, usingSymbols.EnableNotificationSupportAttribute));
                if (enableNotificationSupportAttributeData is null)
                {
                    return null;
                }

                var result = new MethodSourceBuildInputs(propertySymbol, usingSymbols, enableNotificationSupportAttributeData);

                WriteLogLine($"End PostTransform ({propertySymbol.ContainingType?.Name}.{propertySymbol.Name})");

                return result;
            }
            catch (OperationCanceledException)
            {
                WriteLogLine($"Canceled PostTransform ({propertySymbol.ContainingType?.Name}.{propertySymbol.Name})");
                throw;
            }
            catch (Exception ex)
            {
                WriteLogLine($"Exception PostTransform ({propertySymbol.ContainingType?.Name}.{propertySymbol.Name})");
                WriteLogLine(ex.ToString());
                throw;
            }
        }

        void GenerateMethod(SourceProductionContext context, MethodSourceBuildInputs? sourceBuildInputs)
        {
            if (sourceBuildInputs is null) return;

            if (sourceBuildInputs.IsEventArgsOnly) return;


            WriteLogLine($"Begin Generate ({sourceBuildInputs.ContainingTypeInfo.Name}.{sourceBuildInputs.InternalPropertyName})");

            try
            {
                Span<char> initialBuffer = stackalloc char[80000];

                using var sourceBuilder = new MethodSourceBuilder(context, sourceBuildInputs, initialBuffer);

                sourceBuilder.Build();

                context.AddSource(sourceBuilder.HintName, sourceBuilder.SourceText);

                WriteLogLine($"End Generate ({sourceBuildInputs.ContainingTypeInfo.Name}.{sourceBuildInputs.InternalPropertyName}) => {sourceBuilder.HintName}");
            }
            catch (OperationCanceledException)
            {
                WriteLogLine($"Canceled Generate ({sourceBuildInputs.ContainingTypeInfo.Name}.{sourceBuildInputs.InternalPropertyName})");
                throw;
            }
            catch (Exception ex)
            {
                WriteLogLine($"Exception in Generate ({sourceBuildInputs.ContainingTypeInfo.Name}.{sourceBuildInputs.InternalPropertyName})");
                WriteLogLine(ex.ToString());
                throw;
            }
        }

        void GenerateEventArg(SourceProductionContext context, EventArgSourceBuilderInputs args)
        {
            WriteLogLine($"Begin Generate {args.ContainingTypeInfo.Name} EventArgs");

            try
            {
                Span<char> initialBuffer = stackalloc char[80000];

                using var sourceBuilder = new EventArgSourceBuilder(context, args, initialBuffer);

                sourceBuilder.Build();

                context.AddSource(sourceBuilder.HintName, sourceBuilder.SourceText);

                WriteLogLine($"End Generate {args.ContainingTypeInfo.Name} EventArgs => {sourceBuilder.HintName}");
            }
            catch (OperationCanceledException)
            {
                WriteLogLine($"Canceled Generate {args.ContainingTypeInfo.Name} EventArgs");
                throw;
            }
            catch (Exception ex)
            {
                WriteLogLine($"Exception in Generate {args.ContainingTypeInfo.Name} EventArgs");
                WriteLogLine(ex.ToString());
                throw;
            }
        }
    }
}
