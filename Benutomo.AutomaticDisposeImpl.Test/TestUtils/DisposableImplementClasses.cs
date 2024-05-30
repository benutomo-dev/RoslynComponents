namespace Benutomo.AutomaticDisposeImpl.Test.TestUtils
{
    class ExplicitDisposableImplemetnClass : IDisposable
    {
        public Action? OnFinalize { get; set; }

        public Action? OnDispose { get; set; }
        public Action<bool>? OnDisposeCore { get; set; }

        private int _managedContextDisposeCount;
        private int _unmanagedContextDisposeCount;

        public int ManagedContextDisposeCount => _managedContextDisposeCount;
        public int UnmanagedContextDisposeCount => _unmanagedContextDisposeCount;

        protected virtual void Dispose(bool disposing)
        {
            OnDisposeCore?.Invoke(disposing);

            if (disposing)
            {
                Interlocked.Increment(ref _managedContextDisposeCount);
            }

            Interlocked.Increment(ref _unmanagedContextDisposeCount);
        }

        ~ExplicitDisposableImplemetnClass()
        {
            OnFinalize?.Invoke();

            Dispose(disposing: false);
        }

        void IDisposable.Dispose()
        {
            OnDispose?.Invoke();

            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    class ImplicitDisposableImplementClass : IDisposable
    {
        public Action? OnFinalize { get; set; }

        public Action? OnDispose { get; set; }
        public Action<bool>? OnDisposeCore { get; set; }

        private int _managedContextDisposeCount;
        private int _unmanagedContextDisposeCount;

        public int ManagedContextDisposeCount => _managedContextDisposeCount;
        public int UnmanagedContextDisposeCount => _unmanagedContextDisposeCount;

        protected virtual void Dispose(bool disposing)
        {
            OnDisposeCore?.Invoke(disposing);

            if (disposing)
            {
                Interlocked.Increment(ref _managedContextDisposeCount);
            }

            Interlocked.Increment(ref _unmanagedContextDisposeCount);
        }

        ~ImplicitDisposableImplementClass()
        {
            OnFinalize?.Invoke();

            Dispose(disposing: false);
        }

        public void Dispose()
        {
            OnDispose?.Invoke();

            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    class ExplicitAsyncDisposableImplemetnClass : IDisposable, IAsyncDisposable
    {
        public Action? OnFinalize { get; set; }

        public Action? OnDispose { get; set; }
        public Action<bool>? OnDisposeCore { get; set; }

        public Func<ValueTask>? OnDisposeAsync { get; set; }
        public Func<ValueTask>? OnDisposeAsyncCore { get; set; }

        private int _unmanagedContextDisposeCount;

        private int _managedContextAsyncDisposeCount;
        private int _managedContextSyncDisposeCount;

        public int ManagedContextTotalDisposeCount => _managedContextSyncDisposeCount + _managedContextAsyncDisposeCount;
        public int UnmanagedContextDisposeCount => _unmanagedContextDisposeCount;

        public int ManagedContextSyncDisposeCount => _managedContextSyncDisposeCount;
        public int ManagedContextAsyncDisposeCount => _managedContextAsyncDisposeCount;

        protected virtual void Dispose(bool disposing)
        {
            OnDisposeCore?.Invoke(disposing);

            if (disposing)
            {
                Interlocked.Increment(ref _managedContextSyncDisposeCount);
            }

            Interlocked.Increment(ref _unmanagedContextDisposeCount);
        }

        ~ExplicitAsyncDisposableImplemetnClass()
        {
            OnFinalize?.Invoke();

            Dispose(disposing: false);
        }

        void IDisposable.Dispose()
        {
            OnDispose?.Invoke();

            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected async virtual ValueTask DisposeAsyncCore()
        {
            if (OnDisposeAsyncCore is { } action)
            {
                await action();
            }

            Interlocked.Increment(ref _managedContextAsyncDisposeCount);
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (OnDisposeAsync is { } action)
            {
                await action();
            }

            await DisposeAsyncCore().ConfigureAwait(false);

            Dispose(disposing: false);

            GC.SuppressFinalize(this);
        }
    }

    class ImplicitAsyncDisposableImplementClass : IDisposable, IAsyncDisposable
    {
        public Action? OnFinalize { get; set; }

        public Action? OnDispose { get; set; }
        public Action<bool>? OnDisposeCore { get; set; }

        public Func<ValueTask>? OnDisposeAsync { get; set; }
        public Func<ValueTask>? OnDisposeAsyncCore { get; set; }

        private int _unmanagedContextDisposeCount;

        private int _managedContextAsyncDisposeCount;
        private int _managedContextSyncDisposeCount;

        public int ManagedContextTotalDisposeCount => _managedContextSyncDisposeCount + _managedContextAsyncDisposeCount;
        public int UnmanagedContextDisposeCount => _unmanagedContextDisposeCount;

        public int ManagedContextSyncDisposeCount => _managedContextSyncDisposeCount;
        public int ManagedContextAsyncDisposeCount => _managedContextAsyncDisposeCount;

        protected virtual void Dispose(bool disposing)
        {
            OnDisposeCore?.Invoke(disposing);

            if (disposing)
            {
                Interlocked.Increment(ref _managedContextSyncDisposeCount);
            }

            Interlocked.Increment(ref _unmanagedContextDisposeCount);
        }

        ~ImplicitAsyncDisposableImplementClass()
        {
            OnFinalize?.Invoke();

            Dispose(disposing: false);
        }

        public void Dispose()
        {
            OnDispose?.Invoke();

            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected async virtual ValueTask DisposeAsyncCore()
        {
            if (OnDisposeAsyncCore is { } action)
            {
                await action();
            }

            Interlocked.Increment(ref _managedContextAsyncDisposeCount);
            return;
        }

        public async ValueTask DisposeAsync()
        {
            if (OnDisposeAsync is { } action)
            {
                await action();
            }

            await DisposeAsyncCore().ConfigureAwait(false);

            Dispose(disposing: false);

            GC.SuppressFinalize(this);
        }
    }
}
