using Benutomo;
using System;
using System.IO;

// VisualStudioの警告表示を目視確認をするときだけコメントアウトする
#pragma warning disable SG0003

namespace SourceGeneratorDebug_StandardPatterns
{
    namespace Sample
    {
        [AutomaticDisposeImpl]
        partial class MissingImplementIAsyncDisposableInterface : IDisposable
        {
            Stream _streamFieald = null;

            Stream StreamProperty { get; set; }
        }
    }
}
