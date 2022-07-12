#pragma warning disable CS0436
#nullable enable

namespace Benutomo
{
    /// <summary>
    /// Todo
    /// </summary>
    [global::System.AttributeUsage(global::System.AttributeTargets.Property)]
    internal class ChangingEventAttribute : global::System.Attribute
    {
        public ChangingEventAttribute() { }

        public ChangingEventAttribute(NotificationAccessibility accessibility) { }

        public ChangingEventAttribute(ExplicitInterfaceImplementation enableExplicitInterfaceImplementation) { }

        public ChangingEventAttribute(NotificationAccessibility accessibility, ExplicitInterfaceImplementation enableExplicitInterfaceImplementation) { }
    }
}