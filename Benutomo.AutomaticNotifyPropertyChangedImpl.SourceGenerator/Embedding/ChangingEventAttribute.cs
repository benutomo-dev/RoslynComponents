namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator.Embedding
{
    /// <summary>
    /// Todo
    /// </summary>
    [StaticSource("Benutomo",
        Usings = new[] { "using System;" },
        Directives = new[] {
            "#pragma warning disable CS0436",
            "#nullable enable",
        },
        Attributes = new[] {
            @"[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]",
            @"[AttributeUsage(AttributeTargets.Property)]",
        })]
    public sealed class ChangingEventAttribute : Attribute
    {
        public ChangingEventAttribute() { }

        public ChangingEventAttribute(NotificationAccessibility accessibility) { }

        public ChangingEventAttribute(ExplicitInterfaceImplementation enableExplicitInterfaceImplementation) { }

        public ChangingEventAttribute(NotificationAccessibility accessibility, ExplicitInterfaceImplementation enableExplicitInterfaceImplementation) { }
    }
}