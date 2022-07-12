#pragma warning disable CS0436
#nullable enable

namespace Benutomo
{
    /// <summary>
    /// Todo
    /// </summary>
    [global::System.AttributeUsage(global::System.AttributeTargets.Property)]
    internal class ChangedObservableAttribute : global::System.Attribute
    {
        public ChangedObservableAttribute() { }

        public ChangedObservableAttribute(NotificationAccessibility Accessibility) { }
    }
}