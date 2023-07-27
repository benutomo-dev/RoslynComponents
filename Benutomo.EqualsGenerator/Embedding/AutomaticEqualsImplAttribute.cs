namespace Benutomo.EqualsGenerator.Embedding
{
    /// <summary>
    /// 指定したクラスに<see cref="IEquatable{T}"/>のメンバを自動実装する。
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
            @"[AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]",
            @"[Conditional(""CompileTimeOnly"")]",
        })]
    public class AutomaticEqualsImplAttribute : Attribute
    {
        public AutomaticEqualsImplAttribute(AutomaticEqualsImplOptions options = AutomaticEqualsImplOptions.None) { }
    }
}
