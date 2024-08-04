namespace Benutomo.EqualsGenerator.Embedding
{
    /// <summary>
    /// 指定したクラスに<see cref="IEquatable{T}"/>のメンバを自動実装する。
    /// </summary>
    [StaticSource("Benutomo",
        Usings = [
            "using System;",
            "using System.Diagnostics;",
        ],
        Directives = [
            "#pragma warning disable CS0436",
            "#nullable enable",
        ],
        Attributes = [
            @"[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]",
            @"[AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]",
            @"[Conditional(""CompileTimeOnly"")]",
        ])]
    public sealed class AutomaticEqualsImplAttribute : Attribute
    {
        public AutomaticEqualsImplAttribute(AutomaticEqualsImplOptions options = AutomaticEqualsImplOptions.None) { }
    }
}
