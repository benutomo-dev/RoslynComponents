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
    public sealed class ChangedEventAttribute : Attribute
    {
        private readonly NotificationAccessibility _accessibility;
        private readonly ExplicitInterfaceImplementation _enableExplicitInterfaceImplementation;

        public ChangedEventAttribute() { }

        public ChangedEventAttribute(NotificationAccessibility accessibility)
        {
            _accessibility = accessibility;
        }

        public ChangedEventAttribute(ExplicitInterfaceImplementation enableExplicitInterfaceImplementation)
        {
            _enableExplicitInterfaceImplementation = enableExplicitInterfaceImplementation;
        }

        public ChangedEventAttribute(NotificationAccessibility accessibility, ExplicitInterfaceImplementation enableExplicitInterfaceImplementation)
        {
            _accessibility = accessibility;
            _enableExplicitInterfaceImplementation = enableExplicitInterfaceImplementation;
        }
    }
}