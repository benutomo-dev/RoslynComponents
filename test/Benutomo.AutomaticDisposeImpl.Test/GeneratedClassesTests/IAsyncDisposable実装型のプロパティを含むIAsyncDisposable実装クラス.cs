﻿using Benutomo.AutomaticDisposeImpl.Test.TestUtils;
using FluentAssertions;
using Moq;
using Xunit;

namespace Benutomo.AutomaticDisposeImpl.Test.GeneratedClassesTests
{
    public partial class IAsyncDisposable実装型のプロパティを含むIAsyncDisposable実装クラス
    {
        [AutomaticDisposeImpl]
        partial class NullPropertyClass : IAsyncDisposable, IDisposable/*AutomaticDisposeImplによるIAsyncDisposableの自動実装ではIDisposableも必須*/
        {
            [EnableAutomaticDispose]
            internal IAutomaticImplSupportedAsyncDisposable? disposable { get; set; } = null; // SG0003警告が発生しないこと
        }

        [AutomaticDisposeImpl]
        partial class ExclusivityTestBaseClass : IAsyncDisposable, IDisposable/*AutomaticDisposeImplによるIAsyncDisposableの自動実装ではIDisposableも必須*/
        {
            [EnableAutomaticDispose]
            internal ImplicitAsyncDisposableImplementClass baseDisposable { get; set; } = new(); // SG0003警告が発生しないこと

            int baseImplReleaseUnmanagedResourceCallCount;
            int baseImplSyncDisposeCallCount;
            int baseImplAsyncDisposeCallCount;
            int baseImplTotalDisposeCallCount;

            public int BaseImplReleaseUnmanagedResourceCallCount => Thread.VolatileRead(ref baseImplReleaseUnmanagedResourceCallCount);
            public int BaseImplSyncDisposeCallCount => Thread.VolatileRead(ref baseImplSyncDisposeCallCount);
            public int BaseImplAsyncDisposeCallCount => Thread.VolatileRead(ref baseImplAsyncDisposeCallCount);
            public int BaseImplTotalDisposeCallCount => Thread.VolatileRead(ref baseImplTotalDisposeCallCount);

            [UnmanagedResourceReleaseMethod]
            void SelfImplFinalize()
            {
                Interlocked.Increment(ref baseImplReleaseUnmanagedResourceCallCount);
            }

            [ManagedObjectDisposeMethod]
            void SelfImplSyncDispose()
            {
                Interlocked.Increment(ref baseImplSyncDisposeCallCount);
                Interlocked.Increment(ref baseImplTotalDisposeCallCount);
            }

            [ManagedObjectAsyncDisposeMethod]
            ValueTask SelfImplDisposeAsync()
            {
                Interlocked.Increment(ref baseImplAsyncDisposeCallCount);
                Interlocked.Increment(ref baseImplTotalDisposeCallCount);
                return default;
            }
        }

        [AutomaticDisposeImpl]
        partial class ExclusivityTestClass : ExclusivityTestBaseClass
        {
            [EnableAutomaticDispose]
            internal ImplicitAsyncDisposableImplementClass selfDisposable { get; set; } = new(); // SG0003警告が発生しないこと

            int selfImplReleaseUnmanagedResourceCallCount;
            int selfImplSyncDisposeCallCount;
            int selfImplAsyncDisposeCallCount;
            int selfImplTotalDisposeCallCount;

            public int SelfImplReleaseUnmanagedResourceCallCount => Thread.VolatileRead(ref selfImplReleaseUnmanagedResourceCallCount);
            public int SelfImplSyncDisposeCallCount => Thread.VolatileRead(ref selfImplSyncDisposeCallCount);
            public int SelfImplAsyncDisposeCallCount => Thread.VolatileRead(ref selfImplAsyncDisposeCallCount);
            public int SelfImplTotalDisposeCallCount => Thread.VolatileRead(ref selfImplTotalDisposeCallCount);

            [UnmanagedResourceReleaseMethod]
            void SelfImplFinalize()
            {
                Interlocked.Increment(ref selfImplReleaseUnmanagedResourceCallCount);
            }

            [ManagedObjectDisposeMethod]
            void SelfImplSyncDispose()
            {
                Interlocked.Increment(ref selfImplSyncDisposeCallCount);
                Interlocked.Increment(ref selfImplTotalDisposeCallCount);
            }

            [ManagedObjectAsyncDisposeMethod]
            ValueTask SelfImplDisposeAsync()
            {
                Interlocked.Increment(ref selfImplAsyncDisposeCallCount);
                Interlocked.Increment(ref selfImplTotalDisposeCallCount);
                return default;
            }
        }

        [AutomaticDisposeImpl]
        partial class GetonlyPropertyClass : IAsyncDisposable, IDisposable/*AutomaticDisposeImplによるIAsyncDisposableの自動実装ではIDisposableも必須*/
        {
            [EnableAutomaticDispose]
            internal IAutomaticImplSupportedAsyncDisposable disposable { get; } // SG0003警告が発生しないこと

            public GetonlyPropertyClass(IAutomaticImplSupportedAsyncDisposable disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class GenericTypePropertyClass<T> : IAsyncDisposable, IDisposable/*AutomaticDisposeImplによるIAsyncDisposableの自動実装ではIDisposableも必須*/ where T : IDisposable, IAsyncDisposable
        {
            [EnableAutomaticDispose]
            internal T disposable { get; } // SG0003警告が発生しないこと

            public GenericTypePropertyClass(T disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class InterfacePropertyClass : IAsyncDisposable, IDisposable/*AutomaticDisposeImplによるIAsyncDisposableの自動実装ではIDisposableも必須*/
        {
            [EnableAutomaticDispose]
            internal IAutomaticImplSupportedAsyncDisposable disposable { get; set; } // SG0003警告が発生しないこと

            public InterfacePropertyClass(IAutomaticImplSupportedAsyncDisposable disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class ImplicitAsyncDisposableImplementClassPropertyClass : IAsyncDisposable, IDisposable/*AutomaticDisposeImplによるIAsyncDisposableの自動実装ではIDisposableも必須*/
        {
            [EnableAutomaticDispose]
            internal ImplicitAsyncDisposableImplementClass disposable { get; set; } = new(); // SG0003警告が発生しないこと
        }

        [AutomaticDisposeImpl]
        partial class ExplicitAsyncDisposableImplemetnClassPropertyClass : IAsyncDisposable, IDisposable/*AutomaticDisposeImplによるIAsyncDisposableの自動実装ではIDisposableも必須*/
        {
            [EnableAutomaticDispose]
            internal ExplicitAsyncDisposableImplemetnClass disposable { get; set; } = new(); // SG0003警告が発生しないこと
        }

        [Fact]
        public void プロパティがnull値となっている場合でもDisposeで例外は発生しない()
        {
            var testeeObject = new NullPropertyClass();
            testeeObject.Dispose();
        }

        [Fact]
        public async Task プロパティがnull値となっている場合でもDisposeAsyncで例外は発生しない()
        {
            var testeeObject = new NullPropertyClass();
            await testeeObject.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public void DisposeとDisposeAsyncが複数スレッドから同時に呼び出されても最初の呼び出しのみが排他的に有効となる()
        {
            const int millisecondsTimeout = 1000;

            var testIterationCount = Math.Min(10, Environment.ProcessorCount * 3);

            for (int i = 0; i < testIterationCount; i++)
            {
                // 念のため、同時実行数を変更しながら複数回反復してチェックする
                var disposeCallThreadCount = 1 + i;


                using var disposeStartEvent = new ManualResetEventSlim(false);
                using var disposeEnteredEvent = new ManualResetEventSlim(false);
                using var disposeBlockEvent = new ManualResetEventSlim(false);

                var testeeObject = new ExclusivityTestClass();

                var unsetCountAtBaseDisposableMemberDisposeCalling = 0;

                testeeObject.baseDisposable.OnDispose = () =>
                {
                    if (!disposeBlockEvent.IsSet)
                    {
                        // 親クラスのメンバーの破棄は派生クラスのメンバーの破棄よりも後に行われるため
                        // 正しい実装において、このタイミングでdisposeBlockEvent.IsSetが偽となることはないはず。
                        Interlocked.Increment(ref unsetCountAtBaseDisposableMemberDisposeCalling);
                    }
                };

                testeeObject.baseDisposable.OnDisposeAsync = () =>
                {
                    if (!disposeBlockEvent.IsSet)
                    {
                        // 親クラスのメンバーの破棄は派生クラスのメンバーの破棄よりも後に行われるため
                        // 正しい実装において、このタイミングでdisposeBlockEvent.IsSetが偽となることはないはず。
                        Interlocked.Increment(ref unsetCountAtBaseDisposableMemberDisposeCalling);
                    }
                    return default(ValueTask);
                };

                testeeObject.selfDisposable.OnDispose = () =>
                {
                    disposeEnteredEvent.Set();
                    disposeBlockEvent.Wait(millisecondsTimeout);
                };

                testeeObject.selfDisposable.OnDisposeAsync = () =>
                {
                    disposeEnteredEvent.Set();
                    disposeBlockEvent.Wait(millisecondsTimeout);
                    return default(ValueTask);
                };

                var threads = Enumerable.Range(0, disposeCallThreadCount).Select(id => new Thread(_ =>
                {
                    disposeStartEvent.Wait(millisecondsTimeout);

                    if ((id % 2) == 0)
                    {
                        testeeObject.Dispose();
                    }
                    else
                    {
                        testeeObject.DisposeAsync().GetAwaiter().GetResult();
                    }

                })).ToArray();

                foreach (var thread in threads)
                {
                    thread.Start();
                }

                testeeObject.BaseImplTotalDisposeCallCount.Should().Be(0, "まだdisposeStartEventがセットされていないので、Disposeは呼ばれていないはず。");
                testeeObject.SelfImplTotalDisposeCallCount.Should().Be(0, "まだdisposeStartEventがセットされていないので、Disposeは呼ばれていないはず。");

                testeeObject.baseDisposable.ManagedContextTotalDisposeCount.Should().Be(0, "まだdisposeStartEventがセットされていないので、Disposeは呼ばれていないはず。");
                testeeObject.selfDisposable.ManagedContextTotalDisposeCount.Should().Be(0, "まだdisposeStartEventがセットされていないので、Disposeは呼ばれていないはず。");

                testeeObject.IsDisposed.Should().BeFalse("まだdisposeStartEventがセットされていないので、Disposeは呼ばれていないはず。");

                disposeStartEvent.Set();
                // 複数スレッドが同時にDisposeを開始

                disposeEnteredEvent.Wait(millisecondsTimeout);
                // いずれかのスレッドからの呼び出しがメンバのDisposeに到達

                testeeObject.BaseImplTotalDisposeCallCount.Should().Be(0, "まだメンバのDisposeの中で破棄の進行がブロックされているのでカウントは変化しないはず。");
                testeeObject.SelfImplTotalDisposeCallCount.Should().Be(0, "まだメンバのDisposeの中で破棄の進行がブロックされているのでカウントは変化しないはず。");

                testeeObject.selfDisposable.ManagedContextTotalDisposeCount.Should().Be(0, "まだDisposeの入口でブロックされているのでカウントは変化しないはず。");
                testeeObject.IsDisposed.Should().BeTrue("内部で既に排他的なDisposeが開始された時点でDispose完了前に真となるはず。");

                disposeBlockEvent.Set();
                // メンバのDisposeのブロックを解除

                foreach (var thread in threads)
                {
                    thread.Join(millisecondsTimeout);
                }

                testeeObject.BaseImplTotalDisposeCallCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていても実装者のDisposeは最初の１回に対してのみ排他的に呼び出されるはず。");
                testeeObject.SelfImplTotalDisposeCallCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていても実装者のDisposeは最初の１回に対してのみ排他的に呼び出されるはず。");

                testeeObject.BaseImplReleaseUnmanagedResourceCallCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていても実装者のReleaseUnmanagedResourceは最初の１回に対してのみ排他的に呼び出されるはず。");
                testeeObject.SelfImplReleaseUnmanagedResourceCallCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていても実装者のReleaseUnmanagedResourceは最初の１回に対してのみ排他的に呼び出されるはず。");

                testeeObject.baseDisposable.ManagedContextTotalDisposeCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていてもメンバのDisposeは最初の１回に対してのみ排他的に呼び出されるはず。");
                testeeObject.selfDisposable.ManagedContextTotalDisposeCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていてもメンバのDisposeは最初の１回に対してのみ排他的に呼び出されるはず。");

                testeeObject.baseDisposable.UnmanagedContextDisposeCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていてもメンバのDisposeは最初の１回に対してのみ排他的に呼び出されるはず。");
                testeeObject.selfDisposable.UnmanagedContextDisposeCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていてもメンバのDisposeは最初の１回に対してのみ排他的に呼び出されるはず。");

                unsetCountAtBaseDisposableMemberDisposeCalling.Should().Be(0, "親クラスのメンバのDisposeが派生クラスのメンバのDisposeより先に呼び出されていなければ0のはず。");
            }
        }

        [Fact]
        public async Task readonlyプロパティに対する自動実装によるDispose()
        {
            // IAsyncDisposableを実装している型のメンバを自動破棄するためにはメンバがIDisposableも実装している必要があるので
            // テストでは両方のインターフェイスを合成したIAutomaticImplSupportedAsyncDisposableを使用

            var disposableMock = new Mock<IAutomaticImplSupportedAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());
            disposableMock.Setup(v => v.DisposeAsync()).Returns(default(ValueTask));

            var testeeObject = new GetonlyPropertyClass(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Disposeの呼び出しが伝搬していない。");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "Disposeの呼び出しがDisposeAsyncに伝搬してしまっている。");

            testeeObject.IsDisposed.Should().BeTrue("Disposeの完了後は真でなければならない。");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");
        }


        [Fact]
        public async Task readonlyプロパティに対する自動実装によるDisposeAsync()
        {
            // IAsyncDisposableを実装している型のメンバを自動破棄するためにはメンバがIDisposableも実装している必要があるので
            // テストでは両方のインターフェイスを合成したIAutomaticImplSupportedAsyncDisposableを使用

            var disposableMock = new Mock<IAutomaticImplSupportedAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());
            disposableMock.Setup(v => v.DisposeAsync()).Returns(default(ValueTask));

            var testeeObject = new GetonlyPropertyClass(disposableMock.Object);

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "Disposeの呼び出しがDisposeAsyncに伝搬してしまっている。");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "Disposeの呼び出しが伝搬していない。");

            testeeObject.IsDisposed.Should().BeTrue("Disposeの完了後は真でなければならない。");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");
        }

        [Fact]
        public async Task ジェネリック型プロパティに対する自動実装によるDispose()
        {
            // IAsyncDisposableを実装している型のメンバを自動破棄するためにはメンバがIDisposableも実装している必要があるので
            // テストでは両方のインターフェイスを合成したIAutomaticImplSupportedAsyncDisposableを使用

            var disposableMock = new Mock<IAutomaticImplSupportedAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());
            disposableMock.Setup(v => v.DisposeAsync()).Returns(default(ValueTask));

            var testeeObject = new GenericTypePropertyClass<IAutomaticImplSupportedAsyncDisposable>(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Disposeの呼び出しが伝搬していない。");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "Disposeの呼び出しがDisposeAsyncに伝搬してしまっている。");

            testeeObject.IsDisposed.Should().BeTrue("Disposeの完了後は真でなければならない。");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");
        }

        [Fact]
        public async Task ジェネリック型プロパティに対する自動実装によるDisposeAsync()
        {
            // IAsyncDisposableを実装している型のメンバを自動破棄するためにはメンバがIDisposableも実装している必要があるので
            // テストでは両方のインターフェイスを合成したIAutomaticImplSupportedAsyncDisposableを使用

            var disposableMock = new Mock<IAutomaticImplSupportedAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());
            disposableMock.Setup(v => v.DisposeAsync()).Returns(default(ValueTask));

            var testeeObject = new GenericTypePropertyClass<IAutomaticImplSupportedAsyncDisposable>(disposableMock.Object);

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "Disposeの呼び出しがDisposeAsyncに伝搬してしまっている。");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "Disposeの呼び出しが伝搬していない。");

            testeeObject.IsDisposed.Should().BeTrue("Disposeの完了後は真でなければならない。");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");
        }

        [Fact]
        public async Task IAsyncDisposable型のプロパティに対する自動実装によるDispose()
        {
            // IAsyncDisposableを実装している型のメンバを自動破棄するためにはメンバがIDisposableも実装している必要があるので
            // テストでは両方のインターフェイスを合成したIAutomaticImplSupportedAsyncDisposableを使用

            var disposableMock = new Mock<IAutomaticImplSupportedAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());
            disposableMock.Setup(v => v.DisposeAsync()).Returns(default(ValueTask));

            var testeeObject = new InterfacePropertyClass(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Disposeの呼び出しが伝搬していない。");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "Disposeの呼び出しがDisposeAsyncに伝搬してしまっている。");

            testeeObject.IsDisposed.Should().BeTrue("Disposeの完了後は真でなければならない。");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");
        }

        [Fact]
        public async Task IAsyncDisposable型のプロパティに対する自動実装によるDisposeAsync()
        {
            // IAsyncDisposableを実装している型のメンバを自動破棄するためにはメンバがIDisposableも実装している必要があるので
            // テストでは両方のインターフェイスを合成したIAutomaticImplSupportedAsyncDisposableを使用

            var disposableMock = new Mock<IAutomaticImplSupportedAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());
            disposableMock.Setup(v => v.DisposeAsync()).Returns(default(ValueTask));

            var testeeObject = new InterfacePropertyClass(disposableMock.Object);

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "Disposeの呼び出しがDisposeAsyncに伝搬してしまっている。");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "Disposeの呼び出しが伝搬していない。");

            testeeObject.IsDisposed.Should().BeTrue("Disposeの完了後は真でなければならない。");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "２回目以降の破棄の呼び出しが伝搬してしまっている。");
        }

        [Fact]
        public async Task IAsyncDisposableを直接実装している型のプロパティに対する自動実装によるDispose()
        {
            var testeeObject = new ImplicitAsyncDisposableImplementClassPropertyClass();

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "最初のDisposeの呼び出しは伝搬されなければならない。");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(0, "Disposeの呼び出しがDisposeAsyncに伝搬してしまっている。");

            testeeObject.IsDisposed.Should().BeTrue("Disposeの完了後は真でなければならない。");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(0, "２回目以降の破棄の呼び出しが伝搬してしまっている。");

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(0, "２回目以降の破棄の呼び出しが伝搬してしまっている。");
        }

        [Fact]
        public async Task IAsyncDisposableを直接実装している型のプロパティに対する自動実装によるDisposeAsync()
        {
            var testeeObject = new ImplicitAsyncDisposableImplementClassPropertyClass();

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(0, "DisposeAsyncの呼び出しがDisposeに伝搬してしまっている。");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(1, "最初のDisposeAsyncの呼び出しは伝搬されなければならない。");

            testeeObject.IsDisposed.Should().BeTrue("Disposeの完了後は真でなければならない。");

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(0, "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(1, "２回目以降の破棄の呼び出しが伝搬してしまっている。");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(0, "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(1, "２回目以降の破棄の呼び出しが伝搬してしまっている。");
        }

        [Fact]
        public async Task IAsyncDisposableを直接明示的に実装している型のプロパティに対する自動実装によるDispose()
        {
            var testeeObject = new ExplicitAsyncDisposableImplemetnClassPropertyClass();

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "最初のDisposeの呼び出しは伝搬されなければならない。");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(0, "Disposeの呼び出しがDisposeAsyncに伝搬してしまっている。");

            testeeObject.IsDisposed.Should().BeTrue("Disposeの完了後は真でなければならない。");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(0, "２回目以降の破棄の呼び出しが伝搬してしまっている。");

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(0, "２回目以降の破棄の呼び出しが伝搬してしまっている。");
        }

        [Fact]
        public async Task IAsyncDisposableを直接明示的に実装している型のプロパティに対する自動実装によるDisposeAsync()
        {
            var testeeObject = new ExplicitAsyncDisposableImplemetnClassPropertyClass();

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(0, "DisposeAsyncの呼び出しがDisposeに伝搬してしまっている。");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(1, "最初のDisposeAsyncの呼び出しは伝搬されなければならない。");

            testeeObject.IsDisposed.Should().BeTrue("Disposeの完了後は真でなければならない。");

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(0, "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(1, "２回目以降の破棄の呼び出しが伝搬してしまっている。");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(0, "２回目以降の破棄の呼び出しが伝搬してしまっている。");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(1, "２回目以降の破棄の呼び出しが伝搬してしまっている。");
        }
    }
}
