#pragma warning disable CA1018 // 属性を AttributeUsageAttribute に設定します

namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator.Embedding
{
    /// <summary>
    /// Todo
    /// </summary>
    [StaticSource("Benutomo",
        Usings = ["using System;"],
        Directives = [
            "#pragma warning disable CS0436",
            "#nullable enable",
        ],
        Attributes = [
            @"[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]",
            @"[AttributeUsage(AttributeTargets.Property)]",
        ])]
    public sealed class EnableNotificationSupportAttribute : Attribute
    {
        public bool EventArgsOnly { get; set; }
    }
}