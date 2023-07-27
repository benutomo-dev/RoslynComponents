namespace Benutomo.EqualsGenerator.Embedding
{
    /// <summary>
    /// Equalsメソッドの自動実装でこの属性を付与したメンバが指定した他メンバの等価性を代表するメンバであることを示す。
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
            @"[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]",
            @"[Conditional(""CompileTimeOnly"")]",
        })]
    public class RepresentingEquivalenceForAttribute : Attribute
    {
        public RepresentingEquivalenceForAttribute(params string[] members)
        { }
    }
}

