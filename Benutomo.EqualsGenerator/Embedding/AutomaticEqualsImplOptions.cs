namespace Benutomo.EqualsGenerator.Embedding
{
    /// <summary>
    /// <see cref="IEquatable{T}"/>の実装のオプション
    /// </summary>
    [StaticSource("Benutomo",
        Usings = new[] {
            "using System;",
        },
        Directives = new[] {
            "#pragma warning disable CS0436",
            "#nullable enable",
        },
        Attributes = new[] {
            @"[Flags]",
        })]
    [Flags]
    public enum AutomaticEqualsImplOptions
    {
        None         = 0b0000,
        WithOperator = 0b0001,
    }
}