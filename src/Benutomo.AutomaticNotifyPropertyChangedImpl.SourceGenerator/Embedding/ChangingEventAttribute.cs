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
    public sealed class ChangingEventAttribute : Attribute
    {
        public ChangingEventAttribute() { }

        public ChangingEventAttribute(NotificationAccessibility accessibility) { }

        public ChangingEventAttribute(ExplicitInterfaceImplementation enableExplicitInterfaceImplementation) { }

        public ChangingEventAttribute(NotificationAccessibility accessibility, ExplicitInterfaceImplementation enableExplicitInterfaceImplementation) { }
    }
}