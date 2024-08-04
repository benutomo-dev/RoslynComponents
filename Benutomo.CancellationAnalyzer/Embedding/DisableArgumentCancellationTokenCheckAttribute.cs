namespace Benutomo.CancellationAnalyzer.Embedding
{
    [StaticSource("Benutomo",
        Usings = ["using System;"],
        Attributes = [
            @"[AttributeUsage(AttributeTargets.Method, Inherited = true)]",
            @"[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]",
        ])]
    public sealed class DisableArgumentCancellationTokenCheckAttribute : Attribute
    {
    }
}
