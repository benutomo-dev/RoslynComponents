#pragma warning disable CS0436
#nullable enable

namespace Benutomo
{
    /// <summary>
    /// Todo
    /// </summary>
    [global::System.AttributeUsage(global::System.AttributeTargets.Property)]
    internal class ChangedEventAttribute : global::System.Attribute
    {
        public ChangedEventAttribute() { }

        public ChangedEventAttribute(NotificationAccessibility accessibility) { }

        public ChangedEventAttribute(ExplicitInterfaceImplementation enableExplicitInterfaceImplementation) { }

        public ChangedEventAttribute(NotificationAccessibility accessibility, ExplicitInterfaceImplementation enableExplicitInterfaceImplementation) { }
    }
}