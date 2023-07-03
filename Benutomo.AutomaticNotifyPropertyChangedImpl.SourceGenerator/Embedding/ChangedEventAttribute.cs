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
    public class ChangedEventAttribute : Attribute
    {
        public ChangedEventAttribute() { }

        public ChangedEventAttribute(NotificationAccessibility accessibility) { }

        public ChangedEventAttribute(ExplicitInterfaceImplementation enableExplicitInterfaceImplementation) { }

        public ChangedEventAttribute(NotificationAccessibility accessibility, ExplicitInterfaceImplementation enableExplicitInterfaceImplementation) { }
    }
}