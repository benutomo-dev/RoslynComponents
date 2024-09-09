#pragma warning disable CA1018 // 属性を AttributeUsageAttribute に設定します
#pragma warning disable IDE0060 // 未使用のパラメーターを削除します

namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator.Embedding
{
    /// <summary>
    /// Todo
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
            @"[AttributeUsage(AttributeTargets.Property)]",
        ])]
    public sealed class ChangedObservableAttribute : Attribute
    {
        public ChangedObservableAttribute() { }

        public ChangedObservableAttribute(NotificationAccessibility accessibility) { }
    }
}