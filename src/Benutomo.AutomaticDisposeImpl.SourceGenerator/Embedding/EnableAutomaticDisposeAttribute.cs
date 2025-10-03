#pragma warning disable CA1018 // 属性を AttributeUsageAttribute に設定します
#pragma warning disable IDE0060 // 未使用のパラメーターを削除します

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator.Embedding;

/// <summary>
/// このオブジェクトの破棄と同時に自動的に<see cref=""System.IDisposable.Dispose"" />メソッドまたは<see cref=""System.IAsyncDisposable.DisposeAsync"" />メソッドを呼び出します。
/// </summary>
[StaticSource("Benutomo",
    Usings = ["using System;"],
    Directives = [
        "#pragma warning disable CS0436",
        "#pragma warning disable IDE0060",
        "#nullable enable",
    ],
    Attributes = [
        @"[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]",
        @"[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]",
    ])]
public sealed class EnableAutomaticDisposeAttribute : Attribute
{
    public EnableAutomaticDisposeAttribute() { }

    /// <summary>
    /// このオブジェクトの破棄と同時に自動的に<see cref=""System.IDisposable.Dispose"" />メソッドまたは<see cref=""System.IAsyncDisposable.DisposeAsync"" />メソッドを呼び出します。
    /// </summary>
    /// <param name=""linkedMembers"">このメンバの破棄に連動して破棄されるメンバ(ここで列挙されたメンバはEnable/DisableAutomaticDispose属性を省略可能)</param>
    public EnableAutomaticDisposeAttribute(params string[] dependencyMembers) { }
}