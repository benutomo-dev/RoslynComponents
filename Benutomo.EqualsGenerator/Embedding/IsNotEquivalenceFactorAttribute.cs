namespace Benutomo.EqualsGenerator.Embedding
{
    /// <summary>
    /// Equalsメソッドの自動実装で等価性を判定するメンバーから除外する。
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
    
    public class IsNotEquivalenceFactorAttribute : Attribute
    {
    }
}

