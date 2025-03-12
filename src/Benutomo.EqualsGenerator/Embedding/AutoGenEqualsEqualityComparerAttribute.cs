#pragma warning disable CA1018 // 属性を AttributeUsageAttribute に設定します
#pragma warning disable IDE0060 // 未使用のパラメーターを削除します

namespace Benutomo.EqualsGenerator.Embedding
{
    /// <summary>
    /// Equalsメソッドの自動実装でこの属性を付与したメンバの等価性判定に使用する<see cref="IEqualityComparer{T}"。/>
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
            @"[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]",
            @"[Conditional(""CompileTimeOnly"")]",
        ])]
    public sealed class AutoGenEqualsEqualityComparerAttribute : Attribute
    {
        public AutoGenEqualsEqualityComparerAttribute(string nameofIEqualityComparerMember, bool useDefaultEqualityComparerIfNull = false) { }
    }
}

