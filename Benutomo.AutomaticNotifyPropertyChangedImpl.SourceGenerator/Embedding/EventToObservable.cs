namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator.Embedding
{
    [StaticSource("Benutomo.Internal",
        Usings = new[] {
            "using System;",
            "using System.Threading;",
        },
        Directives = new[] {
            "#pragma warning disable CS0436",
            "#nullable enable",
        },
        Attributes = new[] {
            @"[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]",
        })]
    public sealed class EventToObservable<T> : IObservable<T>
    {
        Action<EventHandler> _addHandler;
        Action<EventHandler> _removeHandler;
        Func<T> _valueGetter;
        bool _pushValueAtSubscribed;

        public EventToObservable(Action<EventHandler> addHandler, Action<EventHandler> removeHandler, Func<T> valueGetter, bool pushValueAtSubscribed)
        {
            _addHandler = addHandler ?? throw new ArgumentNullException(nameof(addHandler));
            _removeHandler = removeHandler ?? throw new ArgumentNullException(nameof(removeHandler));
            _valueGetter = valueGetter ?? throw new ArgumentNullException(nameof(valueGetter));
            _pushValueAtSubscribed = pushValueAtSubscribed;
        }

        public IDisposable Subscribe(IObserver<T> observer) => new Proxy(_addHandler, _removeHandler, _valueGetter, observer, _pushValueAtSubscribed);

        private class Proxy : IDisposable
        {
            Action<EventHandler>? _removeHandler;
            Func<T> _valueGetter;
            IObserver<T>? _observer;

            public Proxy(Action<EventHandler> addHandler, Action<EventHandler> removeHandler, Func<T> valueGetter, IObserver<T> observer, bool pushValueAtSubscribed)
            {
                addHandler(EventHandler);
                _removeHandler = removeHandler;
                _valueGetter = valueGetter;
                _observer = observer;

                if (pushValueAtSubscribed)
                {
                    PushValue();
                }
            }

            public void Dispose() => Close(exception: null);

            void PushValue()
            {
                if (Volatile.Read(ref _observer) is { } observer)
                {
                    // まだOnComplete/OnErrorの呼び出しに至っていない

                    T value = default!;

                    try
                    {
                        value = _valueGetter();
                    }
                    catch (Exception exception)
                    {
                        Close(exception);
                        return;
                    }

                    if (Volatile.Read(ref _removeHandler) is not null)
                    {
                        // _valueGetterの間にCloseが呼び出されていない(厳密にOnComplete/OnError後のOnNextを防止できる排他ではないがほぼ問題ないはず)

                        observer.OnNext(value);
                    }
                }
            }

            void Close(Exception? exception)
            {
                var removeHandler = Interlocked.Exchange(ref _removeHandler, null);
                removeHandler?.Invoke(EventHandler);

                var observer = Interlocked.Exchange(ref _observer, null);

                if (exception is null)
                {
                    observer?.OnCompleted();
                }
                else
                {
                    observer?.OnError(exception);
                }
            }

            void EventHandler(object? source, EventArgs args) => PushValue();
        }
    }
}