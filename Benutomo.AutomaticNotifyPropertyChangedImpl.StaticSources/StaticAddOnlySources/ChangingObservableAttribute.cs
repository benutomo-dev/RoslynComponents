#pragma warning disable CS0436
#nullable enable

namespace Benutomo
{
    /// <summary>
    /// Todo
    /// </summary>
    [global::System.AttributeUsage(global::System.AttributeTargets.Property)]
    internal class ChangingObservableAttribute : global::System.Attribute
    {
        public ChangingObservableAttribute() { }

        public ChangingObservableAttribute(NotificationAccessibility Accessibility) { }
    }
}