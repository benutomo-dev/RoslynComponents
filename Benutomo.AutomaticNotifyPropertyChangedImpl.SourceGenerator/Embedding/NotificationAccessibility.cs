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
        ])]
    public enum NotificationAccessibility : int
    {
        Public,
        Protected,
        Internal,
        ProtectedInternal,
        PrivateProtected,
        Private,
    }
}