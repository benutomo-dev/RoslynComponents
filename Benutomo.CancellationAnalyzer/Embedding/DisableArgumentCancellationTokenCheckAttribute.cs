namespace Benutomo.CancellationAnalyzer.Embedding
{
    [StaticSource("Benutomo",
        Usings = new[] { "using System;" },
        Attributes = new[] {
            @"[AttributeUsage(AttributeTargets.Method, Inherited = true)]",
            @"[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]",
        })]
    public sealed class DisableArgumentCancellationTokenCheckAttribute : Attribute
    {
    }
}
