namespace Benutomo.EqualsGenerator.Embedding
{
    /// <summary>
    /// <see cref="IEquatable{T}"/>の実装のオプション
    /// </summary>
    [StaticSource("Benutomo",
        Usings = [
            "using System;",
        ],
        Directives = [
            "#pragma warning disable CS0436",
            "#nullable enable",
        ],
        Attributes = [
            @"[Flags]",
        ])]
    [Flags]
    public enum AutomaticEqualsImplOptions
    {
        None         = 0b0000,
        WithOperator = 0b0001,
    }
}