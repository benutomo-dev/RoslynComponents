using Benutomo;
using System;

#pragma warning disable CS0414

namespace SourceGeneratorDebug_StandardPatterns.AutomaticDisposeImpl
{
    [AutomaticDisposeImpl(Mode = AutomaticDisposeImplMode.Explicit)]
    partial class DependencyMember : IDisposable, IAsyncDisposable
    {
        [EnableAutomaticDispose(nameof(_fieldDisposableDependencyField), nameof(_fieldDisposableDependencyProperty))]
        private IDisposable? fieldDisposable = null;

        [EnableAutomaticDispose(nameof(_asyncFieldDisposableDependencyField), nameof(_asyncFieldDisposableDependencyProperty))]
        private IAsyncDisposable? asyncFieldDisposable = null;
    }

    partial class DependencyMember
    {
        // 他のメンバのdependencyMembersに指定されているので属性付与が無くともエラーとならない
        private IDisposable? _fieldDisposableDependencyField = null;

        // 他のメンバのdependencyMembersに指定されているので属性付与が無くともエラーとならない
        private IDisposable? _fieldDisposableDependencyProperty { get; set; }

        // 他のメンバのdependencyMembersに指定されているので属性付与が無くともエラーとならない
        private IDisposable? _asyncFieldDisposableDependencyField = null;

        // 他のメンバのdependencyMembersに指定されているので属性付与が無くともエラーとならない
        private IDisposable? _asyncFieldDisposableDependencyProperty { get; set; }
    }
}
