using Benutomo;
using System;
using System.IO;
using System.Threading.Tasks;

class AsyncOnlyDisposable : IAsyncDisposable
{
    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}

namespace SourceGeneratorDebug_StandardPatterns
{
    [AutomaticDisposeImpl]
    public partial class AsyncDisposableOnlyComposition1 : IDisposable
    {
        [EnableAutomaticDispose]
        AsyncOnlyDisposable? _asyncOnlyDisposableFieald;

        [EnableAutomaticDispose]
        readonly AsyncOnlyDisposable? _asyncOnlyDisposableReadonlyFieald;

        static AsyncOnlyDisposable? s_asyncOnlyDisposableFieald = null;

        static readonly AsyncOnlyDisposable? s_asyncOnlyDisposableReadonlyFieald = null;


        [EnableAutomaticDispose]
        AsyncOnlyDisposable? _asyncOnlyDisposableProperty { get; set; }

        [EnableAutomaticDispose]
        AsyncOnlyDisposable? _asyncOnlyDisposableGetonlyProperty { get; }

        static AsyncOnlyDisposable? s_asyncOnlyDisposableProperty { get; set; }

        static AsyncOnlyDisposable? s_asyncOnlyDisposableGetonlyProperty { get; }

        internal AsyncDisposableOnlyComposition1(AsyncOnlyDisposable asyncOnlyDisposableFieald, AsyncOnlyDisposable asyncOnlyDisposableReadonlyFieald, AsyncOnlyDisposable asyncOnlyDisposableProperty, AsyncOnlyDisposable asyncOnlyDisposableGetonlyProperty)
        {
            _asyncOnlyDisposableFieald = asyncOnlyDisposableFieald ?? s_asyncOnlyDisposableFieald ?? s_asyncOnlyDisposableReadonlyFieald;
            _asyncOnlyDisposableReadonlyFieald = asyncOnlyDisposableReadonlyFieald;
            _asyncOnlyDisposableProperty = asyncOnlyDisposableProperty;
            _asyncOnlyDisposableGetonlyProperty = asyncOnlyDisposableGetonlyProperty;
        }
    }
}
 
namespace SourceGeneratorDebug_StandardPatterns.Nest
{
    [AutomaticDisposeImpl]
    public partial class AsyncDisposableOnlyComposition2 : IDisposable, IAsyncDisposable
    {
        [EnableAutomaticDispose]
        AsyncOnlyDisposable? _asyncOnlyDisposableFieald;

        [EnableAutomaticDispose]
        readonly AsyncOnlyDisposable? _asyncOnlyDisposableReadonlyFieald;


        static AsyncOnlyDisposable? s_asyncOnlyDisposableFieald = null;

        static readonly AsyncOnlyDisposable? s_asyncOnlyDisposableReadonlyFieald = null;


        [EnableAutomaticDispose]
        AsyncOnlyDisposable? _asyncOnlyDisposableProperty { get; set; }

        [EnableAutomaticDispose]
        AsyncOnlyDisposable? _asyncOnlyDisposableGetonlyProperty { get; }

        static AsyncOnlyDisposable? s_asyncOnlyDisposableProperty { get; set; }

        static AsyncOnlyDisposable? s_asyncOnlyDisposableGetonlyProperty { get; }

        internal AsyncDisposableOnlyComposition2(AsyncOnlyDisposable asyncOnlyDisposableFieald, AsyncOnlyDisposable asyncOnlyDisposableReadonlyFieald, AsyncOnlyDisposable asyncOnlyDisposableProperty, AsyncOnlyDisposable asyncOnlyDisposableGetonlyProperty)
        {
            _asyncOnlyDisposableFieald = asyncOnlyDisposableFieald ?? s_asyncOnlyDisposableFieald ?? s_asyncOnlyDisposableReadonlyFieald;
            _asyncOnlyDisposableReadonlyFieald = asyncOnlyDisposableReadonlyFieald;
            _asyncOnlyDisposableProperty = asyncOnlyDisposableProperty;
            _asyncOnlyDisposableGetonlyProperty = asyncOnlyDisposableGetonlyProperty;
        }
    }
}

namespace SourceGeneratorDebug_StandardPatterns
{
    namespace Nest
    {
        [AutomaticDisposeImpl]
        public partial class AsyncDisposableOnlyComposition3 : IAsyncDisposable
        {

            [EnableAutomaticDispose]
            AsyncOnlyDisposable? _asyncOnlyDisposableFieald;

            [EnableAutomaticDispose]
            readonly AsyncOnlyDisposable? _asyncOnlyDisposableReadonlyFieald;

            static AsyncOnlyDisposable? s_asyncOnlyDisposableFieald = null;

            static readonly AsyncOnlyDisposable? s_asyncOnlyDisposableReadonlyFieald = null;


            [EnableAutomaticDispose]
            AsyncOnlyDisposable? _asyncOnlyDisposableProperty { get; set; }

            [EnableAutomaticDispose]
            AsyncOnlyDisposable? _asyncOnlyDisposableGetonlyProperty { get; }

            static AsyncOnlyDisposable? s_asyncOnlyDisposableProperty { get; set; }

            static AsyncOnlyDisposable? s_asyncOnlyDisposableGetonlyProperty { get; }

            internal AsyncDisposableOnlyComposition3(AsyncOnlyDisposable asyncOnlyDisposableFieald, AsyncOnlyDisposable asyncOnlyDisposableReadonlyFieald, AsyncOnlyDisposable asyncOnlyDisposableProperty, AsyncOnlyDisposable asyncOnlyDisposableGetonlyProperty)
            {
                _asyncOnlyDisposableFieald = asyncOnlyDisposableFieald ?? s_asyncOnlyDisposableFieald ?? s_asyncOnlyDisposableReadonlyFieald;
                _asyncOnlyDisposableReadonlyFieald = asyncOnlyDisposableReadonlyFieald;
                _asyncOnlyDisposableProperty = asyncOnlyDisposableProperty;
                _asyncOnlyDisposableGetonlyProperty = asyncOnlyDisposableGetonlyProperty;
            }
        }
    }
}
