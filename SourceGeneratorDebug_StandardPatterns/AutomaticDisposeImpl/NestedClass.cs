﻿using Benutomo;
using System;

namespace SourceGeneratorDebug_StandardPatterns.AutomaticDisposeImpl
{
    internal partial class OuterClass
    {
        [AutomaticDisposeImpl]
        partial class InnerClass : IDisposable, IAsyncDisposable
        {
            [EnableAutomaticDispose]
            private IDisposable? disposable = null;

            [EnableAutomaticDispose]
            private IAsyncDisposable? asyncDisposable = null;
        }

        [AutomaticDisposeImpl]
        partial class InnerClass<T> : IDisposable, IAsyncDisposable
        {
            [EnableAutomaticDispose]
            private IDisposable? disposable = null;

            [EnableAutomaticDispose]
            private IAsyncDisposable? asyncDisposable = null;
        }

    }

    internal partial class OuterClass<T>
    {
        [AutomaticDisposeImpl]
        partial class InnerClass : IDisposable, IAsyncDisposable
        {
            [EnableAutomaticDispose]
            private IDisposable? disposable = null;

            [EnableAutomaticDispose]
            private IAsyncDisposable? asyncDisposable = null;
        }

        [AutomaticDisposeImpl]
        partial class InnerClass<U> : IDisposable, IAsyncDisposable
        {
            [EnableAutomaticDispose]
            private IDisposable? disposable = null;

            [EnableAutomaticDispose]
            private IAsyncDisposable? asyncDisposable = null;
        }

    }
}
