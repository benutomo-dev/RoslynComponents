using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator
{
    [Generator(LanguageNames.CSharp)]
    public partial class SourceGenerator : IIncrementalGenerator
    {
#if DEBUG
        static StreamWriter _streamWriter;
        static SourceGenerator()
        {
            Directory.CreateDirectory(@"c:\var\log\StaticSource.SourceGenerator");
            var proc = Process.GetCurrentProcess();
            _streamWriter = new StreamWriter($@"c:\var\log\StaticSource.SourceGenerator\{DateTime.Now:yyyyMMddHHmmss}_{proc.Id}.txt");
            _streamWriter.WriteLine(proc);
        }

        [Conditional("DEBUG")]
        static void WriteLogLine(string line)
        {
            lock (_streamWriter)
            {
                _streamWriter.WriteLine(line);
                _streamWriter.Flush();
            }
        }
#else
        [Conditional("DEBUG")]
        static void WriteLogLine(string line)
        {
        }
#endif

        static Regex NamespaceRegex = new Regex(@"\bnamespace\s+([a-zA-Z0-9_.]+)\s*[;{\r\n]", RegexOptions.Compiled);
        static Regex TypeRegex = new Regex(@"\b(?:class|struct|enum)\s+([a-zA-Z0-9_]+)", RegexOptions.Compiled);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            WriteLogLine("Begin Initialize");

            // Where句を使用しない。
            // https://github.com/dotnet/roslyn/issues/57991
            // 今は、Where句を使用するとSource GeneratorがVSでインクリメンタルに実行されたときに
            // 対象のコードの状態や編集内容などによって突然内部状態が壊れて機能しなくなる問題がおきる。

            context.RegisterSourceOutput(context.AdditionalTextsProvider.Collect().Combine(context.AnalyzerConfigOptionsProvider), GenerateMethod);

            WriteLogLine("End Initialize");
        }

        void GenerateMethod(SourceProductionContext context, (ImmutableArray<AdditionalText> AdditionalTexts, AnalyzerConfigOptionsProvider AnalyzerConfigOptionsProvider) args)
        {
            var sourceBuildInputs = args.AdditionalTexts
                .Select(additionalText =>
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    var options = args.AnalyzerConfigOptionsProvider.GetOptions(additionalText);

                    if (!options.TryGetValue("build_metadata.AdditionalFiles.StaticSource", out var staticSource))
                    {
                        return null!;
                    }

                    if (!bool.TryParse(staticSource, out var isStaticSource) || !isStaticSource)
                    {
                        return null!;
                    }

                    if (!options.TryGetValue("build_metadata.AdditionalFiles.UseSelfBuilding", out var useSelfBuildingValue))
                    {
                        useSelfBuildingValue = null;
                    }

                    if (!bool.TryParse(useSelfBuildingValue, out var useSelfBuilding))
                    {
                        useSelfBuilding = false;
                    }

                    var sourceText = additionalText.GetText(context.CancellationToken);

                    if (sourceText is null)
                    {
                        return null!;
                    }

                    var text = sourceText.ToString();

                    var namespaceMatch = NamespaceRegex.Match(text);

                    if (!namespaceMatch.Success)
                    {
                        return null!;
                    }

                    var typeMatch = TypeRegex.Match(text, namespaceMatch.Index + namespaceMatch.Length);

                    if (!typeMatch.Success)
                    {
                        return null!;
                    }

                    return new MethodSourceBuildInputs(sourceText, namespaceMatch.Groups[1].Value, typeMatch.Groups[1].Value, useSelfBuilding);
                })
                .Where(v => v is not null)
                .ToArray();

            WriteLogLine($"Begin Generate StaticSource");

            try
            {
                Span<char> initialBuffer = stackalloc char[80000];

                using var sourceBuilder = new MethodSourceBuilder(context, sourceBuildInputs, initialBuffer);

                sourceBuilder.Build();

                context.AddSource("StaticSources.cs", sourceBuilder.SourceText);

                foreach (var selfUseSource in sourceBuildInputs.Where(v => v.UseSelfBuilding))
                {
                    context.AddSource($"{selfUseSource.Namespace}.{selfUseSource.TypeName}.cs", selfUseSource.SourceText.ToString());
                }

                WriteLogLine($"End Generate StaticSource");
            }
            catch (OperationCanceledException)
            {
                WriteLogLine($"Canceled Generate StaticSource");
                throw;
            }
            catch (Exception ex)
            {
                WriteLogLine($"Exception in Generate StaticSource");
                WriteLogLine(ex.ToString());
                throw;
            }
        }

    }
}
