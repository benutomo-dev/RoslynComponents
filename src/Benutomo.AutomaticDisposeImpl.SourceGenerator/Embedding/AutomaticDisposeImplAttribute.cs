#pragma warning disable CA1018 // 属性を AttributeUsageAttribute に設定します

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator.Embedding
{
    /// <summary>
    /// 指定したクラスに破棄(<see cref=""System.IDisposable"" />,<see cref=""System.IAsyncDisposable"" />)をサポートするメンバを破棄する<see cref=""System.IDisposable.Dispose"" />メソッドおよび<see cref=""System.IAsyncDisposable.DisposeAsync"" />メソッド(当該クラスに<see cref=""System.IAsyncDisposable"" />インターフェイスが含まれている場合のみ)を自動実装する。
    /// </summary>
    [StaticSource("Benutomo",
        Usings = ["using System;"],
        Directives = [
            "#pragma warning disable CS0436",
            "#nullable enable",
        ],
        Attributes = [
            @"[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]",
            @"[AttributeUsage(AttributeTargets.Class)]",
        ])]
    public sealed class AutomaticDisposeImplAttribute : Attribute
    {
        /// <summary>
        /// 自動破棄実装の既定動作を設定する。
        /// </summary>
        public AutomaticDisposeImplMode Mode { get; set; }
    }
}