namespace Benutomo.CancellationAnalyzer.Embedding
{
    [StaticSource("Benutomo",
        Usings = ["using System;"],
        Attributes = [
            @"[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]",
        ])]
    internal static class Cancellation
    {
        public static _UncancelableSection Uncancelable => _UncancelableSection.Defualt;

        public sealed class _UncancelableSection : IDisposable
        {
            internal static _UncancelableSection Defualt { get; } = new _UncancelableSection();

            public void Dispose() { }
        }

        public static _DisableArgumentCancellationTokenCheckSection DisableArgumentCancellationTokenCheck => _DisableArgumentCancellationTokenCheckSection.Defualt;

        public sealed class _DisableArgumentCancellationTokenCheckSection : IDisposable
        {
            internal static _DisableArgumentCancellationTokenCheckSection Defualt { get; } = new _DisableArgumentCancellationTokenCheckSection();

            public void Dispose() { }
        }
    }
}
