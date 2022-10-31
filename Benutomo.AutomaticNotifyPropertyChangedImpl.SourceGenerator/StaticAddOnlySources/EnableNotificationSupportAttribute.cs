#pragma warning disable CS0436
#nullable enable

namespace Benutomo
{
    /// <summary>
    /// Todo
    /// </summary>
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [global::System.AttributeUsage(global::System.AttributeTargets.Property)]
    public class EnableNotificationSupportAttribute : global::System.Attribute
    {
        public bool EventArgsOnly { get; set; } = false;
    }
}