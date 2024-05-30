using Microsoft.CodeAnalysis;

namespace Benutomo.CancellationAnalyzer
{
    [Generator(LanguageNames.CSharp)]
    public class SourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            StaticSources.StaticSource.Register(context);
        }
    }
}