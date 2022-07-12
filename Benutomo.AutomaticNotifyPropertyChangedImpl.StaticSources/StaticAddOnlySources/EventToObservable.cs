#pragma warning disable CS0436
#nullable enable

namespace Benutomo.Internal
{
    internal class EventToObservable<T> : global::System.IObservable<T>
    {
        global::System.Action<global::System.EventHandler> _addHandler;
        global::System.Action<global::System.EventHandler> _removeHandler;
        global::System.Func<T> _valueGetter;
        bool _pushValueAtSubscribed;

        public EventToObservable(global::System.Action<global::System.EventHandler> addHandler, global::System.Action<global::System.EventHandler> removeHandler, global::System.Func<T> valueGetter, bool pushValueAtSubscribed)
        {
            _addHandler = addHandler ?? throw new global::System.ArgumentNullException(nameof(addHandler));
            _removeHandler = removeHandler ?? throw new global::System.ArgumentNullException(nameof(removeHandler));
            _valueGetter = valueGetter ?? throw new global::System.ArgumentNullException(nameof(valueGetter));
            _pushValueAtSubscribed = pushValueAtSubscribed;
        }

        public global::System.IDisposable Subscribe(global::System.IObserver<T> observer) => new Proxy(_addHandler, _removeHandler, _valueGetter, observer, _pushValueAtSubscribed);

        private class Proxy : global::System.IDisposable
        {
            global::System.Action<global::System.EventHandler>? _removeHandler;
            global::System.Func<T> _valueGetter;
            global::System.IObserver<T>? _observer;

            public Proxy(global::System.Action<global::System.EventHandler> addHandler, global::System.Action<global::System.EventHandler> removeHandler, global::System.Func<T> valueGetter, global::System.IObserver<T> observer, bool pushValueAtSubscribed)
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
                if (global::System.Threading.Volatile.Read(ref _observer) is { } observer)
                {
                    // まだOnComplete/OnErrorの呼び出しに至っていない

                    T value = default!;

                    try
                    {
                        value = _valueGetter();
                    }
                    catch (global::System.Exception exception)
                    {
                        Close(exception);
                        return;
                    }

                    if (global::System.Threading.Volatile.Read(ref _removeHandler) is not null)
                    {
                        // _valueGetterの間にCloseが呼び出されていない(厳密にOnComplete/OnError後のOnNextを防止できる排他ではないがほぼ問題ないはず)

                        observer.OnNext(value);
                    }
                }
            }

            void Close(global::System.Exception? exception)
            {
                var removeHandler = global::System.Threading.Interlocked.Exchange(ref _removeHandler, null);
                removeHandler?.Invoke(EventHandler);

                var observer = global::System.Threading.Interlocked.Exchange(ref _observer, null);

                if (exception is null)
                {
                    observer?.OnCompleted();
                }
                else
                {
                    observer?.OnError(exception);
                }
            }

            void EventHandler(object? source, global::System.EventArgs args) => PushValue();
        }
    }
}