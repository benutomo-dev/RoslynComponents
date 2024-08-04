﻿namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator.Embedding
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
    public sealed class ChangingObservableAttribute : Attribute
    {
        public ChangingObservableAttribute() { }

        public ChangingObservableAttribute(NotificationAccessibility Accessibility) { }
    }
}