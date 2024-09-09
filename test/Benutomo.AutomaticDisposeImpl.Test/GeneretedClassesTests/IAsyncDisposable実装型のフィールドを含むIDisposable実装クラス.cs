using Benutomo.AutomaticDisposeImpl.Test.TestUtils;
using FluentAssertions;
using Moq;
using Xunit;

// VisualStudio�̌x���\����ڎ��m�F������Ƃ������R�����g�A�E�g����
#pragma warning disable SG0003

namespace Benutomo.AutomaticDisposeImpl.Test.GeneretedClassesTests
{
    public partial class IAsyncDisposable�����^�̃t�B�[���h���܂�IDisposable�����N���X
    {
        [AutomaticDisposeImpl]
        partial class NullFieldClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal IAutomaticImplSupportedAsyncDisposable? disposable = null; // SG0003�x�����������邱��(�m�F����ꍇ�̓\�[�X�擪��pragma���R�����g�A�E�g)
        }

        [AutomaticDisposeImpl]
        partial class ExclusivityTestBaseClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal ImplicitAsyncDisposableImplementClass baseDisposable = new(); // SG0003�x�����������邱��(�m�F����ꍇ�̓\�[�X�擪��pragma���R�����g�A�E�g)

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

#pragma warning disable SG0011 // ManagedObjectDisposeMethod������IDisposable�C���^�[�t�F�[�X����������Ă��Ȃ��N���X�̃��\�b�h�ɕt�^����Ă��܂�
            [ManagedObjectAsyncDisposeMethod]
            ValueTask SelfImplDisposeAsync()
            {
                Interlocked.Increment(ref baseImplAsyncDisposeCallCount);
                Interlocked.Increment(ref baseImplTotalDisposeCallCount);
                return default;
            }
#pragma warning restore SG0011 // ManagedObjectDisposeMethod������IDisposable�C���^�[�t�F�[�X����������Ă��Ȃ��N���X�̃��\�b�h�ɕt�^����Ă��܂�
        }

        [AutomaticDisposeImpl]
        partial class ExclusivityTestClass : ExclusivityTestBaseClass
        {
            [EnableAutomaticDispose]
            internal ImplicitAsyncDisposableImplementClass selfDisposable = new(); // SG0003�x�����������邱��(�m�F����ꍇ�̓\�[�X�擪��pragma���R�����g�A�E�g)

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

#pragma warning disable SG0011 // ManagedObjectDisposeMethod������IDisposable�C���^�[�t�F�[�X����������Ă��Ȃ��N���X�̃��\�b�h�ɕt�^����Ă��܂�
            [ManagedObjectAsyncDisposeMethod]
            ValueTask SelfImplDisposeAsync()
            {
                Interlocked.Increment(ref selfImplAsyncDisposeCallCount);
                Interlocked.Increment(ref selfImplTotalDisposeCallCount);
                return default;
            }
#pragma warning restore SG0011 // ManagedObjectDisposeMethod������IDisposable�C���^�[�t�F�[�X����������Ă��Ȃ��N���X�̃��\�b�h�ɕt�^����Ă��܂�
        }

        [AutomaticDisposeImpl]
        partial class ReadonlyFieldClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal readonly IAutomaticImplSupportedAsyncDisposable disposable; // SG0003�x�����������邱��(�m�F����ꍇ�̓\�[�X�擪��pragma���R�����g�A�E�g)

            public ReadonlyFieldClass(IAutomaticImplSupportedAsyncDisposable disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class GenericTypeFieldClass<T> : IDisposable where T : IDisposable, IAsyncDisposable
        {
            [EnableAutomaticDispose]
            internal T disposable; // SG0003�x�����������邱��(�m�F����ꍇ�̓\�[�X�擪��pragma���R�����g�A�E�g)

            public GenericTypeFieldClass(T disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class InterfaceFieldClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal IAutomaticImplSupportedAsyncDisposable disposable; // SG0003�x�����������邱��(�m�F����ꍇ�̓\�[�X�擪��pragma���R�����g�A�E�g)

            public InterfaceFieldClass(IAutomaticImplSupportedAsyncDisposable disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class ImplicitAsyncDisposableImplementClassFieldClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal ImplicitAsyncDisposableImplementClass disposable = new(); // SG0003�x�����������邱��(�m�F����ꍇ�̓\�[�X�擪��pragma���R�����g�A�E�g)
        }

        [AutomaticDisposeImpl]
        partial class ExplicitAsyncDisposableImplemetnClassFieldClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal ExplicitAsyncDisposableImplemetnClass disposable = new(); // SG0003�x�����������邱��(�m�F����ꍇ�̓\�[�X�擪��pragma���R�����g�A�E�g)
        }

        [Fact]
        public void �t�B�[���h��null�l�ƂȂ��Ă���ꍇ�ł�Dispose�ŗ�O�͔������Ȃ�()
        {
            var testeeObject = new NullFieldClass();
            testeeObject.Dispose();
        }

        [Fact]
        public void Dispose�������X���b�h���瓯���ɌĂяo����Ă��ŏ��̌Ăяo���݂̂��r���I�ɗL���ƂȂ�()
        {
            const int millisecondsTimeout = 1000;

            var testIterationCount = Math.Min(10, Environment.ProcessorCount * 3);

            for (int i = 0; i < testIterationCount; i++)
            {
                // �O�̂��߁A�������s����ύX���Ȃ��畡���񔽕����ă`�F�b�N����
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
                        // �e�N���X�̃����o�[�̔j���͔h���N���X�̃����o�[�̔j��������ɍs���邽��
                        // �����������ɂ����āA���̃^�C�~���O��disposeBlockEvent.IsSet���U�ƂȂ邱�Ƃ͂Ȃ��͂��B
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

                testeeObject.BaseImplTotalDisposeCallCount.Should().Be(0, "�܂�disposeStartEvent���Z�b�g����Ă��Ȃ��̂ŁADispose�͌Ă΂�Ă��Ȃ��͂��B");
                testeeObject.SelfImplTotalDisposeCallCount.Should().Be(0, "�܂�disposeStartEvent���Z�b�g����Ă��Ȃ��̂ŁADispose�͌Ă΂�Ă��Ȃ��͂��B");

                testeeObject.baseDisposable.ManagedContextTotalDisposeCount.Should().Be(0, "�܂�disposeStartEvent���Z�b�g����Ă��Ȃ��̂ŁADispose�͌Ă΂�Ă��Ȃ��͂��B");
                testeeObject.selfDisposable.ManagedContextTotalDisposeCount.Should().Be(0, "�܂�disposeStartEvent���Z�b�g����Ă��Ȃ��̂ŁADispose�͌Ă΂�Ă��Ȃ��͂��B");

                testeeObject.IsDisposed.Should().BeFalse("�܂�disposeStartEvent���Z�b�g����Ă��Ȃ��̂ŁADispose�͌Ă΂�Ă��Ȃ��͂��B");

                disposeStartEvent.Set();
                // �����X���b�h��������Dispose���J�n

                disposeEnteredEvent.Wait(millisecondsTimeout);
                // �����ꂩ�̃X���b�h����̌Ăяo���������o��Dispose�ɓ��B

                testeeObject.BaseImplTotalDisposeCallCount.Should().Be(0, "�܂������o��Dispose�̒��Ŕj���̐i�s���u���b�N����Ă���̂ŃJ�E���g�͕ω����Ȃ��͂��B");
                testeeObject.SelfImplTotalDisposeCallCount.Should().Be(0, "�܂������o��Dispose�̒��Ŕj���̐i�s���u���b�N����Ă���̂ŃJ�E���g�͕ω����Ȃ��͂��B");

                testeeObject.selfDisposable.ManagedContextTotalDisposeCount.Should().Be(0, "�܂�Dispose�̓����Ńu���b�N����Ă���̂ŃJ�E���g�͕ω����Ȃ��͂��B");
                testeeObject.IsDisposed.Should().BeTrue("�����Ŋ��ɔr���I��Dispose���J�n���ꂽ���_��Dispose�����O�ɐ^�ƂȂ�͂��B");

                disposeBlockEvent.Set();
                // �����o��Dispose�̃u���b�N������

                foreach (var thread in threads)
                {
                    thread.Join(millisecondsTimeout);
                }

                testeeObject.BaseImplSyncDisposeCallCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������҂�Dispose�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");
                testeeObject.SelfImplSyncDisposeCallCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������҂�Dispose�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");

                testeeObject.BaseImplAsyncDisposeCallCount.Should().Be(0, "DisposeAsync�ɂ͓`�����Ȃ��͂��B");
                testeeObject.SelfImplAsyncDisposeCallCount.Should().Be(0, "DisposeAsync�ɂ͓`�����Ȃ��͂��B");

                testeeObject.BaseImplReleaseUnmanagedResourceCallCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������҂�ReleaseUnmanagedResource�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");
                testeeObject.SelfImplReleaseUnmanagedResourceCallCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������҂�ReleaseUnmanagedResource�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");

                testeeObject.baseDisposable.ManagedContextSyncDisposeCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������o��Dispose�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");
                testeeObject.selfDisposable.ManagedContextSyncDisposeCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������o��Dispose�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");

                testeeObject.baseDisposable.ManagedContextAsyncDisposeCount.Should().Be(0, "DisposeAsync�ɂ͓`�����Ȃ��͂��B");
                testeeObject.selfDisposable.ManagedContextAsyncDisposeCount.Should().Be(0, "DisposeAsync�ɂ͓`�����Ȃ��͂��B");

                testeeObject.baseDisposable.UnmanagedContextDisposeCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������o��Dispose�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");
                testeeObject.selfDisposable.UnmanagedContextDisposeCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������o��Dispose�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");

                unsetCountAtBaseDisposableMemberDisposeCalling.Should().Be(0, "�e�N���X�̃����o��Dispose���h���N���X�̃����o��Dispose����ɌĂяo����Ă��Ȃ����0�̂͂��B");
            }
        }

        [Fact]
        public void readonly�t�B�[���h�ɑ΂��鎩������()
        {
            // IAsyncDisposable���������Ă���^�̃����o�������j�����邽�߂ɂ̓����o��IDisposable���������Ă���K�v������̂�
            // �e�X�g�ł͗����̃C���^�[�t�F�C�X����������IAutomaticImplSupportedAsyncDisposable���g�p

            var disposableMock = new Mock<IAutomaticImplSupportedAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());

            var testeeObject = new ReadonlyFieldClass(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Dispose�̌Ăяo�����`�����Ă��Ȃ��B");

            testeeObject.IsDisposed.Should().BeTrue("Dispose�̊�����͐^�łȂ���΂Ȃ�Ȃ��B");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "�Q��ڈȍ~��Dispose�̌Ăяo�����`�����Ă��܂��Ă���B");
        }

        [Fact]
        public void �W�F�l���b�N�^�t�B�[���h�ɑ΂��鎩������()
        {
            // IAsyncDisposable���������Ă���^�̃����o�������j�����邽�߂ɂ̓����o��IDisposable���������Ă���K�v������̂�
            // �e�X�g�ł͗����̃C���^�[�t�F�C�X����������IAutomaticImplSupportedAsyncDisposable���g�p

            var disposableMock = new Mock<IAutomaticImplSupportedAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());

            var testeeObject = new GenericTypeFieldClass<IAutomaticImplSupportedAsyncDisposable>(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Dispose�̌Ăяo�����`�����Ă��Ȃ��B");

            testeeObject.IsDisposed.Should().BeTrue("Dispose�̊�����͐^�łȂ���΂Ȃ�Ȃ��B");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "�Q��ڈȍ~��Dispose�̌Ăяo�����`�����Ă��܂��Ă���B");
        }

        [Fact]
        public void IAsyncDisposable�^�̃t�B�[���h�ɑ΂��鎩������()
        {
            // IAsyncDisposable���������Ă���^�̃����o�������j�����邽�߂ɂ̓����o��IDisposable���������Ă���K�v������̂�
            // �e�X�g�ł͗����̃C���^�[�t�F�C�X����������IAutomaticImplSupportedAsyncDisposable���g�p

            var disposableMock = new Mock<IAutomaticImplSupportedAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());

            var testeeObject = new InterfaceFieldClass(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Dispose�̌Ăяo�����`�����Ă��Ȃ��B");

            testeeObject.IsDisposed.Should().BeTrue("Dispose�̊�����͐^�łȂ���΂Ȃ�Ȃ��B");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "�Q��ڈȍ~��Dispose�̌Ăяo�����`�����Ă��܂��Ă���B");
        }

        [Fact]
        public void IAsyncDisposable�𒼐ڎ������Ă���^�̃t�B�[���h�ɑ΂��鎩������()
        {
            var testeeObject = new ImplicitAsyncDisposableImplementClassFieldClass();

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "�ŏ���Dispose�̌Ăяo���͓`������Ȃ���΂Ȃ�Ȃ��B");

            testeeObject.IsDisposed.Should().BeTrue("Dispose�̊�����͐^�łȂ���΂Ȃ�Ȃ��B");

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "�Q��ڈȍ~��Dispose�̌Ăяo�����`�����Ă͂����Ȃ��B");
        }

        [Fact]
        public void IAsyncDisposable�𒼐ږ����I�Ɏ������Ă���^�̃t�B�[���h�ɑ΂��鎩������()
        {
            var testeeObject = new ExplicitAsyncDisposableImplemetnClassFieldClass();

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "�ŏ���Dispose�̌Ăяo���͓`������Ȃ���΂Ȃ�Ȃ��B");

            testeeObject.IsDisposed.Should().BeTrue("Dispose�̊�����͐^�łȂ���΂Ȃ�Ȃ��B");

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "�Q��ڈȍ~��Dispose�̌Ăяo�����`�����Ă͂����Ȃ��B");
        }
    }
}
