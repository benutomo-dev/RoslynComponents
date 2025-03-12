#pragma warning disable CA1018 // 属性を AttributeUsageAttribute に設定します
#pragma warning disable IDE0060 // 未使用のパラメーターを削除します

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
            "#pragma warning disable IDE0060",
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
