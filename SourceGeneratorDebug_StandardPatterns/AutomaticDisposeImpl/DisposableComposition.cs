using Benutomo;
using System;
using System.Threading.Tasks;

public class Disposable : IDisposable, IAsyncDisposable
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}

namespace SourceGeneratorDebug_StandardPatterns.AutomaticDisposeImpl
{
    [AutomaticDisposeImpl]
    public partial class DisposableyComposition1 : IDisposable
    {

        [EnableAutomaticDispose]
        Disposable? _disposableFieald;

        [EnableAutomaticDispose]
        readonly Disposable? _disposableReadonlyFieald;

        static Disposable? s_disposableFieald = null;

        static readonly Disposable? s_disposableReadonlyFieald = null;


        [EnableAutomaticDispose]
        Disposable? _disposableProperty { get; set; }

        [EnableAutomaticDispose]
        Disposable? _disposableGetonlyProperty { get; }

        static Disposable? s_disposableProperty { get; set; }

        static Disposable? s_disposableGetonlyProperty { get; }

        public DisposableyComposition1(Disposable disposableFieald, Disposable disposableReadonlyFieald, Disposable disposableProperty, Disposable disposableGetonlyProperty)
        {
            _disposableFieald = disposableFieald ?? s_disposableFieald ?? s_disposableReadonlyFieald;
            _disposableReadonlyFieald = disposableReadonlyFieald;
            _disposableProperty = disposableProperty;
            _disposableGetonlyProperty = disposableGetonlyProperty;
        }
    }
}
 
namespace SourceGeneratorDebug_StandardPatterns.AutomaticDisposeImpl.Nest
{
    [AutomaticDisposeImpl]
    public partial class DisposableyComposition2 : IDisposable, IAsyncDisposable
    {

        [EnableAutomaticDispose]
        Disposable? _disposableFieald;

        [EnableAutomaticDispose]
        readonly Disposable? _disposableReadonlyFieald;

        static Disposable? s_disposableFieald = null;

        static readonly Disposable? s_disposableReadonlyFieald = null;


        [EnableAutomaticDispose]
        Disposable? _disposableProperty { get; set; }

        [EnableAutomaticDispose]
        Disposable? _disposableGetonlyProperty { get; }

        static Disposable? s_disposableProperty { get; set; }

        static Disposable? s_disposableGetonlyProperty { get; }

        public DisposableyComposition2(Disposable disposableFieald, Disposable disposableReadonlyFieald, Disposable disposableProperty, Disposable disposableGetonlyProperty)
        {
            _disposableFieald = disposableFieald ?? s_disposableFieald ?? s_disposableReadonlyFieald;
            _disposableReadonlyFieald = disposableReadonlyFieald;
            _disposableProperty = disposableProperty;
            _disposableGetonlyProperty = disposableGetonlyProperty;
        }
    }
}

namespace SourceGeneratorDebug_StandardPatterns.AutomaticDisposeImpl
{
    namespace Nest
    {
        [AutomaticDisposeImpl]
        public partial class DisposableyComposition3 : IAsyncDisposable
        {

            [EnableAutomaticDispose]
            Disposable? _disposableFieald;

            [EnableAutomaticDispose]
            readonly Disposable? _disposableReadonlyFieald;

            static Disposable? s_disposableFieald = null;

            static readonly Disposable? s_disposableReadonlyFieald = null;


            [EnableAutomaticDispose]
            Disposable? _disposableProperty { get; set; }

            [EnableAutomaticDispose]
            Disposable? _disposableGetonlyProperty { get; }

            static Disposable? s_disposableProperty { get; set; }

            static Disposable? s_disposableGetonlyProperty { get; }

            public DisposableyComposition3(Disposable disposableFieald, Disposable disposableReadonlyFieald, Disposable disposableProperty, Disposable disposableGetonlyProperty)
            {
                _disposableFieald = disposableFieald ?? s_disposableFieald ?? s_disposableReadonlyFieald;
                _disposableReadonlyFieald = disposableReadonlyFieald;
                _disposableProperty = disposableProperty;
                _disposableGetonlyProperty = disposableGetonlyProperty;
            }
        }
    }
}
