using Benutomo.AutomaticDisposeImpl.Test.TestUtils;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading;
using Xunit;

namespace Benutomo.AutomaticDisposeImpl.Test
{
    public partial class IDisposable実装型のフィールドを含むIDisposable実装クラス
    {
        [AutomaticDisposeImpl]
        partial class NullFieldClass : IDisposable
        {
            internal IDisposable disposable = null;
        }

        [AutomaticDisposeImpl(ReleaseUnmanagedResourcesMethod = nameof(SelfImplFinalize), SelfDisposeMethod = nameof(SelfImplSyncDispose))]
        partial class ExclusivityTestBaseClass : IDisposable
        {
            internal ImplicitDisposableImplementClass baseDisposable = new();

            int baseImplReleaseUnmanagedResourceCallCount;
            int baseImplDisposeCallCount;

            public int BaseImplReleaseUnmanagedResourceCallCount => Thread.VolatileRead(ref baseImplReleaseUnmanagedResourceCallCount);
            public int BaseImplDisposeCallCount => Thread.VolatileRead(ref baseImplDisposeCallCount);

            void SelfImplFinalize()
            {
                Interlocked.Increment(ref baseImplReleaseUnmanagedResourceCallCount);
            }

            void SelfImplSyncDispose()
            {
                Interlocked.Increment(ref baseImplDisposeCallCount);
            }
        }

        [AutomaticDisposeImpl(ReleaseUnmanagedResourcesMethod = nameof(SelfImplFinalize), SelfDisposeMethod = nameof(SelfImplSyncDispose))]
        partial class ExclusivityTestClass : ExclusivityTestBaseClass
        {
            internal ImplicitDisposableImplementClass selfDisposable = new();

            int selfImplReleaseUnmanagedResourceCallCount;
            int selfImplDisposeCallCount;

            public int SelfImplReleaseUnmanagedResourceCallCount => Thread.VolatileRead(ref selfImplReleaseUnmanagedResourceCallCount);
            public int SelfImplDisposeCallCount => Thread.VolatileRead(ref selfImplDisposeCallCount);

            void SelfImplFinalize()
            {
                Interlocked.Increment(ref selfImplReleaseUnmanagedResourceCallCount);
            }

            void SelfImplSyncDispose()
            {
                Interlocked.Increment(ref selfImplDisposeCallCount);
            }
        }

        [AutomaticDisposeImpl]
        partial class ReadonlyFieldClass : IDisposable
        {
            internal readonly IDisposable disposable;

            public ReadonlyFieldClass(IDisposable disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class GenericTypeFieldClass<T> : IDisposable where T : IDisposable
        {
            internal T disposable;

            public GenericTypeFieldClass(T disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class InterfaceFieldClass : IDisposable
        {
            internal IDisposable disposable;

            public InterfaceFieldClass(IDisposable disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class ImplicitDisposableImplementClassFieldClass : IDisposable
        {
            internal ImplicitDisposableImplementClass disposable = new();
        }

        [AutomaticDisposeImpl]
        partial class ExplicitDisposableImplemetnClassFieldClass : IDisposable
        {
            internal ExplicitDisposableImplemetnClass disposable = new();
        }

        [Fact]
        public void フィールドがnull値となっている場合でもDisposeで例外は発生しない()
        {
            var testeeObject = new NullFieldClass();
            testeeObject.Dispose();
        }

        [Fact]
        public void Disposeが複数スレッドから同時に呼び出されても最初の呼び出しのみが排他的に有効となる()
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

                testeeObject.selfDisposable.OnDispose = () =>
                {
                    disposeEnteredEvent.Set();
                    disposeBlockEvent.Wait(millisecondsTimeout);
                };

                var threads = Enumerable.Range(0, disposeCallThreadCount).Select(id => new Thread(_ =>
                {
                    disposeStartEvent.Wait(millisecondsTimeout);
                    testeeObject.Dispose();
                })).ToArray();

                foreach (var thread in threads)
                {
                    thread.Start();
                }

                testeeObject.BaseImplDisposeCallCount.Should().Be(0, "まだdisposeStartEventがセットされていないので、Disposeは呼ばれていないはず。");
                testeeObject.SelfImplDisposeCallCount.Should().Be(0, "まだdisposeStartEventがセットされていないので、Disposeは呼ばれていないはず。");

                testeeObject.baseDisposable.ManagedContextDisposeCount.Should().Be(0, "まだdisposeStartEventがセットされていないので、Disposeは呼ばれていないはず。");
                testeeObject.selfDisposable.ManagedContextDisposeCount.Should().Be(0, "まだdisposeStartEventがセットされていないので、Disposeは呼ばれていないはず。");

                testeeObject.IsDisposed.Should().BeFalse("まだdisposeStartEventがセットされていないので、Disposeは呼ばれていないはず。");

                disposeStartEvent.Set();
                // 複数スレッドが同時にDisposeを開始

                disposeEnteredEvent.Wait(millisecondsTimeout);
                // いずれかのスレッドからの呼び出しがメンバのDisposeに到達

                testeeObject.BaseImplDisposeCallCount.Should().Be(0, "まだメンバのDisposeの中で破棄の進行がブロックされているのでカウントは変化しないはず。");
                testeeObject.SelfImplDisposeCallCount.Should().Be(0, "まだメンバのDisposeの中で破棄の進行がブロックされているのでカウントは変化しないはず。");

                testeeObject.selfDisposable.ManagedContextDisposeCount.Should().Be(0, "まだDisposeの入口でブロックされているのでカウントは変化しないはず。");
                testeeObject.IsDisposed.Should().BeTrue("内部で既に排他的なDisposeが開始された時点でDispose完了前に真となるはず。");

                disposeBlockEvent.Set();
                // メンバのDisposeのブロックを解除

                foreach (var thread in threads)
                {
                    thread.Join(millisecondsTimeout);
                }

                testeeObject.BaseImplDisposeCallCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていても実装者のDisposeは最初の１回に対してのみ排他的に呼び出されるはず。");
                testeeObject.SelfImplDisposeCallCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていても実装者のDisposeは最初の１回に対してのみ排他的に呼び出されるはず。");

                testeeObject.BaseImplReleaseUnmanagedResourceCallCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていても実装者のReleaseUnmanagedResourceは最初の１回に対してのみ排他的に呼び出されるはず。");
                testeeObject.SelfImplReleaseUnmanagedResourceCallCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていても実装者のReleaseUnmanagedResourceは最初の１回に対してのみ排他的に呼び出されるはず。");

                testeeObject.baseDisposable.ManagedContextDisposeCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていてもメンバのDisposeは最初の１回に対してのみ排他的に呼び出されるはず。");
                testeeObject.selfDisposable.ManagedContextDisposeCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていてもメンバのDisposeは最初の１回に対してのみ排他的に呼び出されるはず。");

                testeeObject.baseDisposable.UnmanagedContextDisposeCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていてもメンバのDisposeは最初の１回に対してのみ排他的に呼び出されるはず。");
                testeeObject.selfDisposable.UnmanagedContextDisposeCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていてもメンバのDisposeは最初の１回に対してのみ排他的に呼び出されるはず。");

                unsetCountAtBaseDisposableMemberDisposeCalling.Should().Be(0, "親クラスのメンバのDisposeが派生クラスのメンバのDisposeより先に呼び出されていなければ0のはず。");
            }
        }

        [Fact]
        public void readonlyフィールドに対する自動実装()
        {
            var disposableMock = new Mock<IDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());

            var testeeObject = new ReadonlyFieldClass(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Disposeの呼び出しが伝搬していない。");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "２回目以降のDisposeの呼び出しが伝搬してしまっている。");
        }

        [Fact]
        public void ジェネリック型フィールドに対する自動実装()
        {
            var disposableMock = new Mock<IDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());

            var testeeObject = new GenericTypeFieldClass<IDisposable>(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Disposeの呼び出しが伝搬していない。");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "２回目以降のDisposeの呼び出しが伝搬してしまっている。");
        }

        [Fact]
        public void IDisposable型のフィールドに対する自動実装()
        {
            var disposableMock = new Mock<IDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());

            var testeeObject = new InterfaceFieldClass(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Disposeの呼び出しが伝搬していない。");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "２回目以降のDisposeの呼び出しが伝搬してしまっている。");
        }

        [Fact]
        public void IDisposableを直接実装している型のフィールドに対する自動実装()
        {
            var testeeObject = new ImplicitDisposableImplementClassFieldClass();

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextDisposeCount.Should().Be(1, "最初のDisposeの呼び出しは伝搬されなければならない。");

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextDisposeCount.Should().Be(1, "２回目以降のDisposeの呼び出しが伝搬してはいけない。");
        }

        [Fact]
        public void IDisposableを直接明示的に実装している型のフィールドに対する自動実装()
        {
            var testeeObject = new ExplicitDisposableImplemetnClassFieldClass();

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextDisposeCount.Should().Be(1, "最初のDisposeの呼び出しは伝搬されなければならない。");

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextDisposeCount.Should().Be(1, "２回目以降のDisposeの呼び出しが伝搬してはいけない。");
        }
    }
}
