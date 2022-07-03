using Benutomo;
using System;
using System.IO;

// VisualStudioの警告表示を目視確認をするときだけコメントアウトする
#pragma warning disable SG0003

namespace SourceGeneratorDebug_StandardPatterns.AutomaticDisposeImpl
{
    namespace Sample
    {
        [AutomaticDisposeImpl]
        partial class MissingImplementIAsyncDisposableInterface : IDisposable
        {

            [EnableAutomaticDispose]
            Stream? _streamFieald = null;


            [EnableAutomaticDispose]
            Stream? StreamProperty { get; set; }
        }
    }
}
