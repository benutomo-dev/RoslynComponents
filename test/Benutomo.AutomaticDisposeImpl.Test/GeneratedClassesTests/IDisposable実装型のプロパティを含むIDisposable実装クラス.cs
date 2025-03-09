﻿using Benutomo.AutomaticDisposeImpl.Test.TestUtils;
using FluentAssertions;
using Moq;
using Xunit;

namespace Benutomo.AutomaticDisposeImpl.Test.GeneratedClassesTests
{
    public partial class IDisposable実装型のプロパティを含むIDisposable実装クラス
    {
        [AutomaticDisposeImpl]
        partial class NullPropertyClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal IDisposable? disposable { get; set; } = null;
        }

        [AutomaticDisposeImpl]
        partial class ExclusivityTestBaseClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal ImplicitDisposableImplementClass baseDisposable { get; set; } = new();

            int baseImplReleaseUnmanagedResourceCallCount;
            int baseImplDisposeCallCount;

            public int BaseImplReleaseUnmanagedResourceCallCount => Thread.VolatileRead(ref baseImplReleaseUnmanagedResourceCallCount);
            public int BaseImplDisposeCallCount => Thread.VolatileRead(ref baseImplDisposeCallCount);

            [UnmanagedResourceReleaseMethod]
            void SelfImplFinalize()
            {
                Interlocked.Increment(ref baseImplReleaseUnmanagedResourceCallCount);
            }

            [ManagedObjectDisposeMethod]
            void SelfImplSyncDispose()
            {
                Interlocked.Increment(ref baseImplDisposeCallCount);
            }
        }

        [AutomaticDisposeImpl]
        partial class ExclusivityTestClass : ExclusivityTestBaseClass
        {
            [EnableAutomaticDispose]
            internal ImplicitDisposableImplementClass selfDisposable { get; set; } = new();

            int selfImplReleaseUnmanagedResourceCallCount;
            int selfImplDisposeCallCount;

            public int SelfImplReleaseUnmanagedResourceCallCount => Thread.VolatileRead(ref selfImplReleaseUnmanagedResourceCallCount);
            public int SelfImplDisposeCallCount => Thread.VolatileRead(ref selfImplDisposeCallCount);

            [UnmanagedResourceReleaseMethod]
            void SelfImplFinalize()
            {
                Interlocked.Increment(ref selfImplReleaseUnmanagedResourceCallCount);
            }

            [ManagedObjectDisposeMethod]
            void SelfImplSyncDispose()
            {
                Interlocked.Increment(ref selfImplDisposeCallCount);
            }
        }

        [AutomaticDisposeImpl]
        partial class GetonlyPropertyClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal IDisposable disposable { get; }

            public GetonlyPropertyClass(IDisposable disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class GenericTypePropertyClass<T> : IDisposable where T : IDisposable
        {
            [EnableAutomaticDispose]
            internal T disposable { get; }

            public GenericTypePropertyClass(T disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class InterfacePropertyClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal IDisposable disposable { get; set; }

            public InterfacePropertyClass(IDisposable disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class ImplicitDisposableImplementClassPropertyClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal ImplicitDisposableImplementClass disposable { get; set; } = new();
        }

        [AutomaticDisposeImpl]
        partial class ExplicitDisposableImplemetnClassPropertyClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal ExplicitDisposableImplemetnClass disposable { get; set; } = new();
        }

        [Fact]
        public void プロパティがnull値となっている場合でもDisposeで例外は発生しない()
        {
            var testeeObject = new NullPropertyClass();
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
        public void getonlyプロパティに対する自動実装()
        {
            var disposableMock = new Mock<IDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());

            var testeeObject = new GetonlyPropertyClass(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Disposeの呼び出しが伝搬していない。");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "２回目以降のDisposeの呼び出しが伝搬してしまっている。");
        }

        [Fact]
        public void ジェネリック型プロパティに対する自動実装()
        {
            var disposableMock = new Mock<IDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());

            var testeeObject = new GenericTypePropertyClass<IDisposable>(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Disposeの呼び出しが伝搬していない。");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "２回目以降のDisposeの呼び出しが伝搬してしまっている。");
        }

        [Fact]
        public void IDisposable型のプロパティに対する自動実装()
        {
            var disposableMock = new Mock<IDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());

            var testeeObject = new InterfacePropertyClass(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Disposeの呼び出しが伝搬していない。");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "２回目以降のDisposeの呼び出しが伝搬してしまっている。");
        }

        [Fact]
        public void IDisposableを直接実装している型のプロパティに対する自動実装()
        {
            var testeeObject = new ImplicitDisposableImplementClassPropertyClass();

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextDisposeCount.Should().Be(1, "最初のDisposeの呼び出しは伝搬されなければならない。");

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextDisposeCount.Should().Be(1, "２回目以降のDisposeの呼び出しが伝搬してはいけない。");
        }

        [Fact]
        public void IDisposableを直接明示的に実装している型のプロパティに対する自動実装()
        {
            var testeeObject = new ExplicitDisposableImplemetnClassPropertyClass();

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextDisposeCount.Should().Be(1, "最初のDisposeの呼び出しは伝搬されなければならない。");

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextDisposeCount.Should().Be(1, "２回目以降のDisposeの呼び出しが伝搬してはいけない。");
        }
    }
}
