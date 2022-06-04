using Benutomo;
using System;


namespace SourceGeneratorDebug_StandardPatterns
{
    [AutomaticDisposeImpl(Mode = AutomaticDisposeImplMode.Explicit)]
    partial class DependencyMember : IDisposable, IAsyncDisposable
    {
        [EnableAutomaticDispose(nameof(_fieldDisposableDependencyField), nameof(_fieldDisposableDependencyProperty))]
        private IDisposable? fieldDisposable;

        [EnableAutomaticDispose(nameof(_asyncFieldDisposableDependencyField), nameof(_asyncFieldDisposableDependencyProperty))]
        private IAsyncDisposable? asyncFieldDisposable;
    }

    partial class DependencyMember
    {
        // 他のメンバのdependencyMembersに指定されているので属性付与が無くともエラーとならない
        private IDisposable? _fieldDisposableDependencyField;

        // 他のメンバのdependencyMembersに指定されているので属性付与が無くともエラーとならない
        private IDisposable? _fieldDisposableDependencyProperty { get; set; }

        // 他のメンバのdependencyMembersに指定されているので属性付与が無くともエラーとならない
        private IDisposable? _asyncFieldDisposableDependencyField;

        // 他のメンバのdependencyMembersに指定されているので属性付与が無くともエラーとならない
        private IDisposable? _asyncFieldDisposableDependencyProperty { get; set; }
    }
}
