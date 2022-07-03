using Benutomo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceGeneratorDebug_StandardPatterns.AutomaticDisposeImpl
{
    internal partial class OuterClass
    {
        [AutomaticDisposeImpl]
        partial class InnerClass : IDisposable, IAsyncDisposable
        {
            [EnableAutomaticDispose]
            private IDisposable? disposable;

            [EnableAutomaticDispose]
            private IAsyncDisposable? asyncDisposable;
        }

        [AutomaticDisposeImpl]
        partial class InnerClass<T> : IDisposable, IAsyncDisposable
        {
            [EnableAutomaticDispose]
            private IDisposable? disposable;

            [EnableAutomaticDispose]
            private IAsyncDisposable? asyncDisposable;
        }

    }

    internal partial class OuterClass<T>
    {
        [AutomaticDisposeImpl]
        partial class InnerClass : IDisposable, IAsyncDisposable
        {
            [EnableAutomaticDispose]
            private IDisposable? disposable;

            [EnableAutomaticDispose]
            private IAsyncDisposable? asyncDisposable;
        }

        [AutomaticDisposeImpl]
        partial class InnerClass<U> : IDisposable, IAsyncDisposable
        {
            [EnableAutomaticDispose]
            private IDisposable? disposable;

            [EnableAutomaticDispose]
            private IAsyncDisposable? asyncDisposable;
        }

    }
}
