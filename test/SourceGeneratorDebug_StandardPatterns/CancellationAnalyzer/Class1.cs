using Benutomo;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SourceGeneratorDebug_StandardPatterns.CancellationAnalyzer
{
    class DummyDispossable : IDisposable
    {
        public static DummyDispossable Create() => new DummyDispossable();
        public static DummyDispossable Create(CancellationToken cancellationToken) => new DummyDispossable();

        public static Task<DummyDispossable> CreateAsync() => Task.FromResult(new DummyDispossable());
        public static Task<DummyDispossable> CreateAsync(CancellationToken cancellationToken) => Task.FromResult(new DummyDispossable());

        public void Dispose() { }
    }

    internal class Class1
    {
        public void NothingCancellationTokenMethod()
        {
            var stream = new FileStream("", FileMode.Open);

            stream.WriteAsync([], 0, 0);

            using (DummyDispossable.Create())
            {
                DummyDispossable.Create();
            }

            using (Cancellation.Uncancelable)
            {
                var cancellationToken = CancellationToken.None;

                // CT0005
                stream.WriteAsync([], 0, 0, cancellationToken);
            }
        }

        [Uncancelable]
        public async Task UncancellableAsyncMethod()
        {
            var cancellationToken = CancellationToken.None;

            var stream = new FileStream("", FileMode.Open);

            await stream.WriteAsync([], 0, 0);

            using (DummyDispossable.Create())
            {
                DummyDispossable.Create();
            }

            using (await DummyDispossable.CreateAsync())
            {
                await DummyDispossable.CreateAsync();
            }

            // CT0005
            await stream.WriteAsync([], 0, 0, cancellationToken);

            // CT0005
            using (DummyDispossable.Create(cancellationToken))
            {
                // CT0005
                DummyDispossable.Create(cancellationToken);
            }

            // CT0005
            using (await DummyDispossable.CreateAsync(cancellationToken))
            {
                // CT0005
                await DummyDispossable.CreateAsync(cancellationToken);
            }

            using (Cancellation.Uncancelable)
            {
                // CT0005
                await stream.WriteAsync([], 0, 0, cancellationToken);

                // CT0005
                using (DummyDispossable.Create(cancellationToken))
                {
                    // CT0005
                    DummyDispossable.Create(cancellationToken);
                }

                // CT0005
                using (await DummyDispossable.CreateAsync(cancellationToken))
                {
                    // CT0005
                    await DummyDispossable.CreateAsync(cancellationToken);
                }
            }

            Func<CancellationToken, Task> cancelableLambda = async cancellationToken =>
            {
                // NO CT0005
                await cancelableFunc(cancellationToken);

                using (Cancellation.Uncancelable)
                {
                    // CT0005
                    await cancelableFunc(cancellationToken);

                    await cancelableFunc(default);

                    await uncancelableFunc();
                }
            };

            async Task cancelableFunc(CancellationToken cancellationToken)
            {
                // NO CT0005
                await cancelableFunc(cancellationToken);

                using (Cancellation.Uncancelable)
                {
                    // CT0005
                    await cancelableFunc(cancellationToken);

                    await cancelableFunc(default);

                    await uncancelableFunc();
                }
            };

            Func<Task> uncancelableLambda = [Uncancelable] async () =>
            {
                // NO CT0005
                await cancelableFunc(cancellationToken);

                using (Cancellation.Uncancelable)
                {
                    // CT0005
                    await cancelableFunc(cancellationToken);

                    await cancelableFunc(default);

                    await uncancelableFunc();
                }
            };

            [Uncancelable]
            async Task uncancelableFunc()
            {
                // CT0005
                await cancelableFunc(cancellationToken);

                using (Cancellation.Uncancelable)
                {
                    // CT0005
                    await cancelableFunc(cancellationToken);

                    await cancelableFunc(default);

                    await uncancelableFunc();
                }
            };
        }

        [Uncancelable]
        public void UncancellableMethod(CancellationToken cancellationToken) // CT0004
        {
            var stream = new FileStream("", FileMode.Open);

            stream.WriteAsync([], 0, 0);

            using (DummyDispossable.Create())
            {
                DummyDispossable.Create();
            }

            // CT0005
            stream.WriteAsync([], 0, 0, cancellationToken);

            // CT0005
            using (DummyDispossable.Create(cancellationToken))
            {
                // CT0005
                DummyDispossable.Create(cancellationToken);
            }

            using (Cancellation.Uncancelable)
            {
                // CT0005
                stream.WriteAsync([], 0, 0, cancellationToken);
            }
        }

        [DisableArgumentCancellationTokenCheck]
        public async Task IgnoreArgumentCancellationTokenAsyncMethod()
        {
            var stream = new FileStream("", FileMode.Open);

            await stream.WriteAsync([], 0, 0);

            using (DummyDispossable.Create())
            {
                DummyDispossable.Create();
            }

            using (await DummyDispossable.CreateAsync())
            {
                await DummyDispossable.CreateAsync();
            }

            using (Cancellation.Uncancelable)
            {
                var cancellationToken = CancellationToken.None;

                // CT0005
                await stream.WriteAsync([], 0, 0, cancellationToken);

                // CT0005
                using (DummyDispossable.Create(cancellationToken))
                {
                    // CT0005
                    DummyDispossable.Create(cancellationToken);
                }

                // CT0005
                using (await DummyDispossable.CreateAsync(cancellationToken))
                {
                    // CT0005
                    await DummyDispossable.CreateAsync(cancellationToken);
                }
            }
        }

        [DisableArgumentCancellationTokenCheck]
        public void IgnoreArgumentCancellationTokenMethod(CancellationToken cancellationToken) // CT0004
        {
            var stream = new FileStream("", FileMode.Open);

            stream.WriteAsync([], 0, 0);

            using (DummyDispossable.Create())
            {
                DummyDispossable.Create();
            }

            using (Cancellation.Uncancelable)
            {
                // CT0005
                stream.WriteAsync([], 0, 0, cancellationToken);
            }
        }

        public void CancellableMethod(CancellationToken cancellationToken)
        {
            var stream = new FileStream("", FileMode.Open);

            // CT0001
            stream.WriteAsync([], 0, 0);

            // CT0001
            using (DummyDispossable.Create())
            {
                // CT0001
                DummyDispossable.Create();
            }

            using (Cancellation.Uncancelable)
            {
                // CT0005
                stream.WriteAsync([], 0, 0, cancellationToken);
            }
        }

        public async Task NothingCancellationTokenAsyncMethod() // CT0003
        {
            var stream = new FileStream("", FileMode.Open);

            // CT0001
            await stream.WriteAsync([], 0, 0);

            // CT0001
            using (DummyDispossable.Create())
            {
                // CT0001
                DummyDispossable.Create();
            }

            // CT0001
            using (await DummyDispossable.CreateAsync())
            {
                // CT0001
                await DummyDispossable.CreateAsync();
            }

            // CT0001
            using (await DummyDispossable.CreateAsync().ConfigureAwait(false))
            {
                // CT0001
                await DummyDispossable.CreateAsync().ConfigureAwait(false);
            }

            using (Cancellation.Uncancelable)
            {
                var cancellationToken = CancellationToken.None;

                // CT0005
                await stream.WriteAsync([], 0, 0, cancellationToken);

                // CT0005
                using (DummyDispossable.Create(cancellationToken))
                {
                    // CT0005
                    DummyDispossable.Create(cancellationToken);
                }

                // CT0005
                using (await DummyDispossable.CreateAsync(cancellationToken))
                {
                    // CT0005
                    await DummyDispossable.CreateAsync(cancellationToken);
                }
            }
        }

        public async Task CancellableAsyncMethod(CancellationToken cancellationToken)
        {
            var stream = new FileStream("", FileMode.Open);

            // CT0001
            await stream.WriteAsync([], 0, 0);

            // CT0001
            using (DummyDispossable.Create())
            {
                // CT0001
                DummyDispossable.Create();
            }

            // CT0001
            using (await DummyDispossable.CreateAsync())
            {
                // CT0001
                await DummyDispossable.CreateAsync();
            }

            // CT0001
            using (await DummyDispossable.CreateAsync().ConfigureAwait(false))
            {
                // CT0001
                await DummyDispossable.CreateAsync().ConfigureAwait(false);
            }

            using (Cancellation.Uncancelable)
            {
                // CT0005
                await stream.WriteAsync([], 0, 0, cancellationToken);

                // CT0005
                using (DummyDispossable.Create(cancellationToken))
                {
                    // CT0005
                    DummyDispossable.Create(cancellationToken);
                }

                // CT0005
                using (await DummyDispossable.CreateAsync(cancellationToken))
                {
                    // CT0005
                    await DummyDispossable.CreateAsync(cancellationToken);
                }
            }
        }
    }
}
