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
        private readonly NotificationAccessibility _accessibility;
        private readonly ExplicitInterfaceImplementation _enableExplicitInterfaceImplementation;

        public ChangingEventAttribute() { }

        public ChangingEventAttribute(NotificationAccessibility accessibility)
        {
            _accessibility = accessibility;
        }

        public ChangingEventAttribute(ExplicitInterfaceImplementation enableExplicitInterfaceImplementation)
        {
            _enableExplicitInterfaceImplementation = enableExplicitInterfaceImplementation;
        }

        public ChangingEventAttribute(NotificationAccessibility accessibility, ExplicitInterfaceImplementation enableExplicitInterfaceImplementation)
        {
            _accessibility = accessibility;
            _enableExplicitInterfaceImplementation = enableExplicitInterfaceImplementation;
        }
    }
}