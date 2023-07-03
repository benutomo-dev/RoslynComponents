using Benutomo.AutomaticDisposeImpl.Test.TestUtils;
using FluentAssertions;
using Moq;
using Xunit;

// VisualStudioの警告表示を目視確認をするときだけコメントアウトする
#pragma warning disable SG0003

namespace Benutomo.AutomaticDisposeImpl.Test.GeneretedClassesTests
{
    public partial class IAsyncDisposable実装型のフィールドを含むIDisposable実装クラス
    {
        [AutomaticDisposeImpl]
        partial class NullFieldClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal IAutomaticImplSupportedAsyncDisposable? disposable = null; // SG0003警告が発生すること(確認する場合はソース先頭のpragmaをコメントアウト)
        }

        [AutomaticDisposeImpl]
        partial class ExclusivityTestBaseClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal ImplicitAsyncDisposableImplementClass baseDisposable = new(); // SG0003警告が発生すること(確認する場合はソース先頭のpragmaをコメントアウト)

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

#pragma warning disable SG0011 // ManagedObjectDisposeMethod属性はIDisposableインターフェースが実装されていないクラスのメソッドに付与されています
            [ManagedObjectAsyncDisposeMethod]
            ValueTask SelfImplDisposeAsync()
            {
                Interlocked.Increment(ref baseImplAsyncDisposeCallCount);
                Interlocked.Increment(ref baseImplTotalDisposeCallCount);
                return default;
            }
#pragma warning restore SG0011 // ManagedObjectDisposeMethod属性はIDisposableインターフェースが実装されていないクラスのメソッドに付与されています
        }

        [AutomaticDisposeImpl]
        partial class ExclusivityTestClass : ExclusivityTestBaseClass
        {
            [EnableAutomaticDispose]
            internal ImplicitAsyncDisposableImplementClass selfDisposable = new(); // SG0003警告が発生すること(確認する場合はソース先頭のpragmaをコメントアウト)

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

#pragma warning disable SG0011 // ManagedObjectDisposeMethod属性はIDisposableインターフェースが実装されていないクラスのメソッドに付与されています
            [ManagedObjectAsyncDisposeMethod]
            ValueTask SelfImplDisposeAsync()
            {
                Interlocked.Increment(ref selfImplAsyncDisposeCallCount);
                Interlocked.Increment(ref selfImplTotalDisposeCallCount);
                return default;
            }
#pragma warning restore SG0011 // ManagedObjectDisposeMethod属性はIDisposableインターフェースが実装されていないクラスのメソッドに付与されています
        }

        [AutomaticDisposeImpl]
        partial class ReadonlyFieldClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal readonly IAutomaticImplSupportedAsyncDisposable disposable; // SG0003警告が発生すること(確認する場合はソース先頭のpragmaをコメントアウト)

            public ReadonlyFieldClass(IAutomaticImplSupportedAsyncDisposable disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class GenericTypeFieldClass<T> : IDisposable where T : IDisposable, IAsyncDisposable
        {
            [EnableAutomaticDispose]
            internal T disposable; // SG0003警告が発生すること(確認する場合はソース先頭のpragmaをコメントアウト)

            public GenericTypeFieldClass(T disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class InterfaceFieldClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal IAutomaticImplSupportedAsyncDisposable disposable; // SG0003警告が発生すること(確認する場合はソース先頭のpragmaをコメントアウト)

            public InterfaceFieldClass(IAutomaticImplSupportedAsyncDisposable disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class ImplicitAsyncDisposableImplementClassFieldClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal ImplicitAsyncDisposableImplementClass disposable = new(); // SG0003警告が発生すること(確認する場合はソース先頭のpragmaをコメントアウト)
        }

        [AutomaticDisposeImpl]
        partial class ExplicitAsyncDisposableImplemetnClassFieldClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal ExplicitAsyncDisposableImplemetnClass disposable = new(); // SG0003警告が発生すること(確認する場合はソース先頭のpragmaをコメントアウト)
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

                testeeObject.BaseImplSyncDisposeCallCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていても実装者のDisposeは最初の１回に対してのみ排他的に呼び出されるはず。");
                testeeObject.SelfImplSyncDisposeCallCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていても実装者のDisposeは最初の１回に対してのみ排他的に呼び出されるはず。");

                testeeObject.BaseImplAsyncDisposeCallCount.Should().Be(0, "DisposeAsyncには伝搬しないはず。");
                testeeObject.SelfImplAsyncDisposeCallCount.Should().Be(0, "DisposeAsyncには伝搬しないはず。");

                testeeObject.BaseImplReleaseUnmanagedResourceCallCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていても実装者のReleaseUnmanagedResourceは最初の１回に対してのみ排他的に呼び出されるはず。");
                testeeObject.SelfImplReleaseUnmanagedResourceCallCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていても実装者のReleaseUnmanagedResourceは最初の１回に対してのみ排他的に呼び出されるはず。");

                testeeObject.baseDisposable.ManagedContextSyncDisposeCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていてもメンバのDisposeは最初の１回に対してのみ排他的に呼び出されるはず。");
                testeeObject.selfDisposable.ManagedContextSyncDisposeCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていてもメンバのDisposeは最初の１回に対してのみ排他的に呼び出されるはず。");

                testeeObject.baseDisposable.ManagedContextAsyncDisposeCount.Should().Be(0, "DisposeAsyncには伝搬しないはず。");
                testeeObject.selfDisposable.ManagedContextAsyncDisposeCount.Should().Be(0, "DisposeAsyncには伝搬しないはず。");

                testeeObject.baseDisposable.UnmanagedContextDisposeCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていてもメンバのDisposeは最初の１回に対してのみ排他的に呼び出されるはず。");
                testeeObject.selfDisposable.UnmanagedContextDisposeCount.Should().Be(1, "複数スレッドから同時にDisposeが呼び出されていてもメンバのDisposeは最初の１回に対してのみ排他的に呼び出されるはず。");

                unsetCountAtBaseDisposableMemberDisposeCalling.Should().Be(0, "親クラスのメンバのDisposeが派生クラスのメンバのDisposeより先に呼び出されていなければ0のはず。");
            }
        }

        [Fact]
        public void readonlyフィールドに対する自動実装()
        {
            // IAsyncDisposableを実装している型のメンバを自動破棄するためにはメンバがIDisposableも実装している必要があるので
            // テストでは両方のインターフェイスを合成したIAutomaticImplSupportedAsyncDisposableを使用

            var disposableMock = new Mock<IAutomaticImplSupportedAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());

            var testeeObject = new ReadonlyFieldClass(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Disposeの呼び出しが伝搬していない。");

            testeeObject.IsDisposed.Should().BeTrue("Disposeの完了後は真でなければならない。");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "２回目以降のDisposeの呼び出しが伝搬してしまっている。");
        }

        [Fact]
        public void ジェネリック型フィールドに対する自動実装()
        {
            // IAsyncDisposableを実装している型のメンバを自動破棄するためにはメンバがIDisposableも実装している必要があるので
            // テストでは両方のインターフェイスを合成したIAutomaticImplSupportedAsyncDisposableを使用

            var disposableMock = new Mock<IAutomaticImplSupportedAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());

            var testeeObject = new GenericTypeFieldClass<IAutomaticImplSupportedAsyncDisposable>(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Disposeの呼び出しが伝搬していない。");

            testeeObject.IsDisposed.Should().BeTrue("Disposeの完了後は真でなければならない。");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "２回目以降のDisposeの呼び出しが伝搬してしまっている。");
        }

        [Fact]
        public void IAsyncDisposable型のフィールドに対する自動実装()
        {
            // IAsyncDisposableを実装している型のメンバを自動破棄するためにはメンバがIDisposableも実装している必要があるので
            // テストでは両方のインターフェイスを合成したIAutomaticImplSupportedAsyncDisposableを使用

            var disposableMock = new Mock<IAutomaticImplSupportedAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());

            var testeeObject = new InterfaceFieldClass(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Disposeの呼び出しが伝搬していない。");

            testeeObject.IsDisposed.Should().BeTrue("Disposeの完了後は真でなければならない。");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "２回目以降のDisposeの呼び出しが伝搬してしまっている。");
        }

        [Fact]
        public void IAsyncDisposableを直接実装している型のフィールドに対する自動実装()
        {
            var testeeObject = new ImplicitAsyncDisposableImplementClassFieldClass();

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "最初のDisposeの呼び出しは伝搬されなければならない。");

            testeeObject.IsDisposed.Should().BeTrue("Disposeの完了後は真でなければならない。");

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "２回目以降のDisposeの呼び出しが伝搬してはいけない。");
        }

        [Fact]
        public void IAsyncDisposableを直接明示的に実装している型のフィールドに対する自動実装()
        {
            var testeeObject = new ExplicitAsyncDisposableImplemetnClassFieldClass();

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "最初のDisposeの呼び出しは伝搬されなければならない。");

            testeeObject.IsDisposed.Should().BeTrue("Disposeの完了後は真でなければならない。");

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "２回目以降のDisposeの呼び出しが伝搬してはいけない。");
        }
    }
}
