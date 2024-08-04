namespace Benutomo.EqualsGenerator.Embedding
{
    /// <summary>
    /// Equalsメソッドの自動実装でこの属性を付与したメンバの等価性判定に使用する<see cref="IEqualityComparer{T}"。/>
    /// </summary>
    [StaticSource("Benutomo",
        Usings = new[] {
            "using System;",
            "using System.Diagnostics;",
        },
        Directives = new[] {
            "#pragma warning disable CS0436",
            "#nullable enable",
        },
        Attributes = new[] {
            @"[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]",
            @"[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]",
            @"[Conditional(""CompileTimeOnly"")]",
        })]
    public sealed class AutoGenEqualsEqualityComparerAttribute : Attribute
    {
        public AutoGenEqualsEqualityComparerAttribute(string nameofIEqualityComparerMember) { }
    }
}

