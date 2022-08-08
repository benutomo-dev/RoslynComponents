using Microsoft.CodeAnalysis;

namespace Benutomo.CancellationAnalyzer
{
    [Generator(LanguageNames.CSharp)]
    public class SourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(PostInitializationOutput);
        }

        public void PostInitializationOutput(IncrementalGeneratorPostInitializationContext context)
        {
            foreach (var source in StaticSources.Sources)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                context.AddSource(source.HintName, source.Source);
            }
        }
    }
}