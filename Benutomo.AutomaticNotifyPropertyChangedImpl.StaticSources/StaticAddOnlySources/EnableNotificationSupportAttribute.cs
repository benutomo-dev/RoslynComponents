#pragma warning disable CS0436
#nullable enable

namespace Benutomo
{
    /// <summary>
    /// Todo
    /// </summary>
    [global::System.AttributeUsage(global::System.AttributeTargets.Property)]
    internal class EnableNotificationSupportAttribute : global::System.Attribute
    {
        public bool EventArgsOnly { get; set; } = false;
    }
}