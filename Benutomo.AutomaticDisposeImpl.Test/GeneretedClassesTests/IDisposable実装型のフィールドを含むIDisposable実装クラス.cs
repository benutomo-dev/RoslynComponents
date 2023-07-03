using Benutomo.AutomaticDisposeImpl.Test.TestUtils;
using FluentAssertions;
using Moq;
using Xunit;

namespace Benutomo.AutomaticDisposeImpl.Test.GeneretedClassesTests
{
    public partial class IDisposable�����^�̃t�B�[���h���܂�IDisposable�����N���X
    {
        [AutomaticDisposeImpl]
        partial class NullFieldClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal IDisposable? disposable = null;
        }

        [AutomaticDisposeImpl]
        partial class ExclusivityTestBaseClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal ImplicitDisposableImplementClass baseDisposable = new();

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
            internal ImplicitDisposableImplementClass selfDisposable = new();

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
        partial class ReadonlyFieldClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal readonly IDisposable disposable;

            public ReadonlyFieldClass(IDisposable disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class GenericTypeFieldClass<T> : IDisposable where T : IDisposable
        {
            [EnableAutomaticDispose]
            internal T disposable;

            public GenericTypeFieldClass(T disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class InterfaceFieldClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal IDisposable disposable;

            public InterfaceFieldClass(IDisposable disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class ImplicitDisposableImplementClassFieldClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal ImplicitDisposableImplementClass disposable = new();
        }

        [AutomaticDisposeImpl]
        partial class ExplicitDisposableImplemetnClassFieldClass : IDisposable
        {
            [EnableAutomaticDispose]
            internal ExplicitDisposableImplemetnClass disposable = new();
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

                testeeObject.BaseImplDisposeCallCount.Should().Be(0, "�܂�disposeStartEvent���Z�b�g����Ă��Ȃ��̂ŁADispose�͌Ă΂�Ă��Ȃ��͂��B");
                testeeObject.SelfImplDisposeCallCount.Should().Be(0, "�܂�disposeStartEvent���Z�b�g����Ă��Ȃ��̂ŁADispose�͌Ă΂�Ă��Ȃ��͂��B");

                testeeObject.baseDisposable.ManagedContextDisposeCount.Should().Be(0, "�܂�disposeStartEvent���Z�b�g����Ă��Ȃ��̂ŁADispose�͌Ă΂�Ă��Ȃ��͂��B");
                testeeObject.selfDisposable.ManagedContextDisposeCount.Should().Be(0, "�܂�disposeStartEvent���Z�b�g����Ă��Ȃ��̂ŁADispose�͌Ă΂�Ă��Ȃ��͂��B");

                testeeObject.IsDisposed.Should().BeFalse("�܂�disposeStartEvent���Z�b�g����Ă��Ȃ��̂ŁADispose�͌Ă΂�Ă��Ȃ��͂��B");

                disposeStartEvent.Set();
                // �����X���b�h��������Dispose���J�n

                disposeEnteredEvent.Wait(millisecondsTimeout);
                // �����ꂩ�̃X���b�h����̌Ăяo���������o��Dispose�ɓ��B

                testeeObject.BaseImplDisposeCallCount.Should().Be(0, "�܂������o��Dispose�̒��Ŕj���̐i�s���u���b�N����Ă���̂ŃJ�E���g�͕ω����Ȃ��͂��B");
                testeeObject.SelfImplDisposeCallCount.Should().Be(0, "�܂������o��Dispose�̒��Ŕj���̐i�s���u���b�N����Ă���̂ŃJ�E���g�͕ω����Ȃ��͂��B");

                testeeObject.selfDisposable.ManagedContextDisposeCount.Should().Be(0, "�܂�Dispose�̓����Ńu���b�N����Ă���̂ŃJ�E���g�͕ω����Ȃ��͂��B");
                testeeObject.IsDisposed.Should().BeTrue("�����Ŋ��ɔr���I��Dispose���J�n���ꂽ���_��Dispose�����O�ɐ^�ƂȂ�͂��B");

                disposeBlockEvent.Set();
                // �����o��Dispose�̃u���b�N������

                foreach (var thread in threads)
                {
                    thread.Join(millisecondsTimeout);
                }

                testeeObject.BaseImplDisposeCallCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������҂�Dispose�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");
                testeeObject.SelfImplDisposeCallCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������҂�Dispose�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");

                testeeObject.BaseImplReleaseUnmanagedResourceCallCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������҂�ReleaseUnmanagedResource�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");
                testeeObject.SelfImplReleaseUnmanagedResourceCallCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������҂�ReleaseUnmanagedResource�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");

                testeeObject.baseDisposable.ManagedContextDisposeCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������o��Dispose�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");
                testeeObject.selfDisposable.ManagedContextDisposeCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������o��Dispose�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");

                testeeObject.baseDisposable.UnmanagedContextDisposeCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������o��Dispose�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");
                testeeObject.selfDisposable.UnmanagedContextDisposeCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������o��Dispose�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");

                unsetCountAtBaseDisposableMemberDisposeCalling.Should().Be(0, "�e�N���X�̃����o��Dispose���h���N���X�̃����o��Dispose����ɌĂяo����Ă��Ȃ����0�̂͂��B");
            }
        }

        [Fact]
        public void readonly�t�B�[���h�ɑ΂��鎩������()
        {
            var disposableMock = new Mock<IDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());

            var testeeObject = new ReadonlyFieldClass(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Dispose�̌Ăяo�����`�����Ă��Ȃ��B");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "�Q��ڈȍ~��Dispose�̌Ăяo�����`�����Ă��܂��Ă���B");
        }

        [Fact]
        public void �W�F�l���b�N�^�t�B�[���h�ɑ΂��鎩������()
        {
            var disposableMock = new Mock<IDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());

            var testeeObject = new GenericTypeFieldClass<IDisposable>(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Dispose�̌Ăяo�����`�����Ă��Ȃ��B");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "�Q��ڈȍ~��Dispose�̌Ăяo�����`�����Ă��܂��Ă���B");
        }

        [Fact]
        public void IDisposable�^�̃t�B�[���h�ɑ΂��鎩������()
        {
            var disposableMock = new Mock<IDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());

            var testeeObject = new InterfaceFieldClass(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Dispose�̌Ăяo�����`�����Ă��Ȃ��B");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "�Q��ڈȍ~��Dispose�̌Ăяo�����`�����Ă��܂��Ă���B");
        }

        [Fact]
        public void IDisposable�𒼐ڎ������Ă���^�̃t�B�[���h�ɑ΂��鎩������()
        {
            var testeeObject = new ImplicitDisposableImplementClassFieldClass();

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextDisposeCount.Should().Be(1, "�ŏ���Dispose�̌Ăяo���͓`������Ȃ���΂Ȃ�Ȃ��B");

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextDisposeCount.Should().Be(1, "�Q��ڈȍ~��Dispose�̌Ăяo�����`�����Ă͂����Ȃ��B");
        }

        [Fact]
        public void IDisposable�𒼐ږ����I�Ɏ������Ă���^�̃t�B�[���h�ɑ΂��鎩������()
        {
            var testeeObject = new ExplicitDisposableImplemetnClassFieldClass();

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextDisposeCount.Should().Be(1, "�ŏ���Dispose�̌Ăяo���͓`������Ȃ���΂Ȃ�Ȃ��B");

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextDisposeCount.Should().Be(1, "�Q��ڈȍ~��Dispose�̌Ăяo�����`�����Ă͂����Ȃ��B");
        }
    }
}
