using Moq;
using Xunit;

namespace Benutomo.AutomaticDisposeImpl.Test.GeneretedClassesTests
{
    public partial class 暗黙的な場合の自動破棄の実装
    {

        [AutomaticDisposeImpl(Mode = AutomaticDisposeImplMode.Implicit)]
        partial class ImplicitAutoDisposeNoneMemberAttributeClass : IDisposable
        {
            // 指定なし
            internal IDisposable? disposable;

            public ImplicitAutoDisposeNoneMemberAttributeClass(IDisposable? disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl(Mode = AutomaticDisposeImplMode.Implicit)]
        partial class ImplicitAutoDisposeEnableAttributeClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal IDisposable? disposable;

            public ImplicitAutoDisposeEnableAttributeClass(IDisposable? disposable)
            {
                this.disposable = disposable;
            }
        }


        [AutomaticDisposeImpl(Mode = AutomaticDisposeImplMode.Implicit)]
        partial class ImplicitAutoDisposeDisableAttributeClass : IDisposable
        {
            [DisableAutomaticDispose]
            internal IDisposable? disposable;

            public ImplicitAutoDisposeDisableAttributeClass(IDisposable? disposable)
            {
                this.disposable = disposable;
            }
        }


        [AutomaticDisposeImpl(Mode = AutomaticDisposeImplMode.Implicit)]
        partial class AsyncImplicitAutoDisposeNoneMemberAttributeClass : IAsyncDisposable
        {
            // 指定なし
            internal IAsyncDisposable? disposable;

            public AsyncImplicitAutoDisposeNoneMemberAttributeClass(IAsyncDisposable? disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl(Mode = AutomaticDisposeImplMode.Implicit)]
        partial class AsyncImplicitAutoDisposeEnableAttributeClass : IAsyncDisposable
        {
            [EnableAutomaticDispose]
            internal IAsyncDisposable? disposable;

            public AsyncImplicitAutoDisposeEnableAttributeClass(IAsyncDisposable? disposable)
            {
                this.disposable = disposable;
            }
        }


        [AutomaticDisposeImpl(Mode = AutomaticDisposeImplMode.Implicit)]
        partial class AsyncImplicitAutoDisposeDisableAttributeClass : IAsyncDisposable
        {
            [DisableAutomaticDispose]
            internal IAsyncDisposable? disposable;

            public AsyncImplicitAutoDisposeDisableAttributeClass(IAsyncDisposable? disposable)
            {
                this.disposable = disposable;
            }
        }


        [Fact]
        public void Implicitモード指定で無属性のIDisposableメンバの破棄()
        {
            var disposableMock = new Mock<IDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());

            var testeeObject = new ImplicitAutoDisposeNoneMemberAttributeClass(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Disposeの呼び出しが伝搬していない。");
        }


        [Fact]
        public void Implicitモード指定でEnableAutomaticDispose属性のIDisposableメンバの破棄()
        {
            var disposableMock = new Mock<IDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());

            var testeeObject = new ImplicitAutoDisposeEnableAttributeClass(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Disposeの呼び出しが伝搬していない。");
        }


        [Fact]
        public void Implicitモード指定でDisableAutomaticDispose属性のIDisposableメンバの破棄()
        {
            var disposableMock = new Mock<IDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());

            var testeeObject = new ImplicitAutoDisposeDisableAttributeClass(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "Disposeの呼び出しが伝搬してしまっている。");
        }


        [Fact]
        public async Task Implicitモード指定で無属性のIAsyncDisposableメンバの破棄()
        {
            var disposableMock = new Mock<IAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.DisposeAsync()).Returns(default(ValueTask));

            var testeeObject = new AsyncImplicitAutoDisposeNoneMemberAttributeClass(disposableMock.Object);

            await testeeObject.DisposeAsync();
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "DisposeAsyncの呼び出しが伝搬していない。");
        }


        [Fact]
        public async Task Implicitモード指定でEnableAutomaticDispose属性のIAsyncDisposableメンバの破棄()
        {
            var disposableMock = new Mock<IAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.DisposeAsync()).Returns(default(ValueTask));

            var testeeObject = new AsyncImplicitAutoDisposeEnableAttributeClass(disposableMock.Object);

            await testeeObject.DisposeAsync();
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "DisposeAsyncの呼び出しが伝搬していない。");
        }


        [Fact]
        public async Task Implicitモード指定でDisableAutomaticDispose属性のIAsyncDisposableメンバの破棄()
        {
            var disposableMock = new Mock<IAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.DisposeAsync()).Returns(default(ValueTask));

            var testeeObject = new AsyncImplicitAutoDisposeDisableAttributeClass(disposableMock.Object);

            await testeeObject.DisposeAsync();
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "DisposeAsyncの呼び出しが伝搬してしまっている。");
        }
    }
}
