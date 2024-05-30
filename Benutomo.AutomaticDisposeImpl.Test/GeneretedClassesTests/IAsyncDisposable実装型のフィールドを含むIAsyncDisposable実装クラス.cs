using Benutomo.AutomaticDisposeImpl.Test.TestUtils;
using FluentAssertions;
using Moq;
using Xunit;

namespace Benutomo.AutomaticDisposeImpl.Test.GeneretedClassesTests
{
    public partial class IAsyncDisposable�����^�̃t�B�[���h���܂�IAsyncDisposable�����N���X
    {
        [AutomaticDisposeImpl]
        partial class NullFieldClass : IAsyncDisposable, IDisposable/*AutomaticDisposeImpl�ɂ��IAsyncDisposable�̎��������ł�IDisposable���K�{*/
        {
            [EnableAutomaticDispose]
            internal IAutomaticImplSupportedAsyncDisposable? disposable = null; // SG0003�x�����������Ȃ�����
        }

        [AutomaticDisposeImpl]
        partial class ExclusivityTestBaseClass : IAsyncDisposable, IDisposable/*AutomaticDisposeImpl�ɂ��IAsyncDisposable�̎��������ł�IDisposable���K�{*/
        {
            [EnableAutomaticDispose]
            internal ImplicitAsyncDisposableImplementClass baseDisposable = new(); // SG0003�x�����������Ȃ�����

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
            internal ImplicitAsyncDisposableImplementClass selfDisposable = new(); // SG0003�x�����������Ȃ�����

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
        partial class ReadonlyFieldClass : IAsyncDisposable, IDisposable/*AutomaticDisposeImpl�ɂ��IAsyncDisposable�̎��������ł�IDisposable���K�{*/
        {
            [EnableAutomaticDispose]
            internal readonly IAutomaticImplSupportedAsyncDisposable disposable; // SG0003�x�����������Ȃ�����

            public ReadonlyFieldClass(IAutomaticImplSupportedAsyncDisposable disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class GenericTypeFieldClass<T> : IAsyncDisposable, IDisposable/*AutomaticDisposeImpl�ɂ��IAsyncDisposable�̎��������ł�IDisposable���K�{*/ where T : IDisposable, IAsyncDisposable
        {
            [EnableAutomaticDispose]
            internal T disposable; // SG0003�x�����������Ȃ�����

            public GenericTypeFieldClass(T disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class InterfaceFieldClass : IAsyncDisposable, IDisposable/*AutomaticDisposeImpl�ɂ��IAsyncDisposable�̎��������ł�IDisposable���K�{*/
        {
            [EnableAutomaticDispose]
            internal IAutomaticImplSupportedAsyncDisposable disposable; // SG0003�x�����������Ȃ�����

            public InterfaceFieldClass(IAutomaticImplSupportedAsyncDisposable disposable)
            {
                this.disposable = disposable;
            }
        }

        [AutomaticDisposeImpl]
        partial class ImplicitAsyncDisposableImplementClassFieldClass : IAsyncDisposable, IDisposable/*AutomaticDisposeImpl�ɂ��IAsyncDisposable�̎��������ł�IDisposable���K�{*/
        {
            [EnableAutomaticDispose]
            internal ImplicitAsyncDisposableImplementClass disposable = new(); // SG0003�x�����������Ȃ�����
        }

        [AutomaticDisposeImpl]
        partial class ExplicitAsyncDisposableImplemetnClassFieldClass : IAsyncDisposable, IDisposable/*AutomaticDisposeImpl�ɂ��IAsyncDisposable�̎��������ł�IDisposable���K�{*/
        {
            [EnableAutomaticDispose]
            internal ExplicitAsyncDisposableImplemetnClass disposable = new(); // SG0003�x�����������Ȃ�����
        }

        [Fact]
        public void �t�B�[���h��null�l�ƂȂ��Ă���ꍇ�ł�Dispose�ŗ�O�͔������Ȃ�()
        {
            var testeeObject = new NullFieldClass();
            testeeObject.Dispose();
        }

        [Fact]
        public void Dispose��DisposeAsync�������X���b�h���瓯���ɌĂяo����Ă��ŏ��̌Ăяo���݂̂��r���I�ɗL���ƂȂ�()
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

                testeeObject.baseDisposable.OnDisposeAsync = () =>
                {
                    if (!disposeBlockEvent.IsSet)
                    {
                        // �e�N���X�̃����o�[�̔j���͔h���N���X�̃����o�[�̔j��������ɍs���邽��
                        // �����������ɂ����āA���̃^�C�~���O��disposeBlockEvent.IsSet���U�ƂȂ邱�Ƃ͂Ȃ��͂��B
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

                testeeObject.BaseImplTotalDisposeCallCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������҂�Dispose�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");
                testeeObject.SelfImplTotalDisposeCallCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������҂�Dispose�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");

                testeeObject.BaseImplReleaseUnmanagedResourceCallCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������҂�ReleaseUnmanagedResource�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");
                testeeObject.SelfImplReleaseUnmanagedResourceCallCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������҂�ReleaseUnmanagedResource�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");

                testeeObject.baseDisposable.ManagedContextTotalDisposeCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������o��Dispose�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");
                testeeObject.selfDisposable.ManagedContextTotalDisposeCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������o��Dispose�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");

                testeeObject.baseDisposable.UnmanagedContextDisposeCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������o��Dispose�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");
                testeeObject.selfDisposable.UnmanagedContextDisposeCount.Should().Be(1, "�����X���b�h���瓯����Dispose���Ăяo����Ă��Ă������o��Dispose�͍ŏ��̂P��ɑ΂��Ă̂ݔr���I�ɌĂяo�����͂��B");

                unsetCountAtBaseDisposableMemberDisposeCalling.Should().Be(0, "�e�N���X�̃����o��Dispose���h���N���X�̃����o��Dispose����ɌĂяo����Ă��Ȃ����0�̂͂��B");
            }
        }

        [Fact]
        public async Task �t�B�[���h��null�l�ƂȂ��Ă���ꍇ�ł�DisposeAsync�ŗ�O�͔������Ȃ�()
        {
            var testeeObject = new NullFieldClass();
            await testeeObject.DisposeAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task readonly�t�B�[���h�ɑ΂��鎩�������ɂ��Dispose()
        {
            // IAsyncDisposable���������Ă���^�̃����o�������j�����邽�߂ɂ̓����o��IDisposable���������Ă���K�v������̂�
            // �e�X�g�ł͗����̃C���^�[�t�F�C�X����������IAutomaticImplSupportedAsyncDisposable���g�p

            var disposableMock = new Mock<IAutomaticImplSupportedAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());
            disposableMock.Setup(v => v.DisposeAsync()).Returns(default(ValueTask));

            var testeeObject = new ReadonlyFieldClass(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Dispose�̌Ăяo�����`�����Ă��Ȃ��B");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "Dispose�̌Ăяo����DisposeAsync�ɓ`�����Ă��܂��Ă���B");

            testeeObject.IsDisposed.Should().BeTrue("Dispose�̊�����͐^�łȂ���΂Ȃ�Ȃ��B");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
        }


        [Fact]
        public async Task readonly�t�B�[���h�ɑ΂��鎩�������ɂ��DisposeAsync()
        {
            // IAsyncDisposable���������Ă���^�̃����o�������j�����邽�߂ɂ̓����o��IDisposable���������Ă���K�v������̂�
            // �e�X�g�ł͗����̃C���^�[�t�F�C�X����������IAutomaticImplSupportedAsyncDisposable���g�p

            var disposableMock = new Mock<IAutomaticImplSupportedAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());
            disposableMock.Setup(v => v.DisposeAsync()).Returns(default(ValueTask));

            var testeeObject = new ReadonlyFieldClass(disposableMock.Object);

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "Dispose�̌Ăяo����DisposeAsync�ɓ`�����Ă��܂��Ă���B");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "Dispose�̌Ăяo�����`�����Ă��Ȃ��B");

            testeeObject.IsDisposed.Should().BeTrue("Dispose�̊�����͐^�łȂ���΂Ȃ�Ȃ��B");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
        }

        [Fact]
        public async Task �W�F�l���b�N�^�t�B�[���h�ɑ΂��鎩�������ɂ��Dispose()
        {
            // IAsyncDisposable���������Ă���^�̃����o�������j�����邽�߂ɂ̓����o��IDisposable���������Ă���K�v������̂�
            // �e�X�g�ł͗����̃C���^�[�t�F�C�X����������IAutomaticImplSupportedAsyncDisposable���g�p

            var disposableMock = new Mock<IAutomaticImplSupportedAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());
            disposableMock.Setup(v => v.DisposeAsync()).Returns(default(ValueTask));

            var testeeObject = new GenericTypeFieldClass<IAutomaticImplSupportedAsyncDisposable>(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Dispose�̌Ăяo�����`�����Ă��Ȃ��B");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "Dispose�̌Ăяo����DisposeAsync�ɓ`�����Ă��܂��Ă���B");

            testeeObject.IsDisposed.Should().BeTrue("Dispose�̊�����͐^�łȂ���΂Ȃ�Ȃ��B");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
        }

        [Fact]
        public async Task �W�F�l���b�N�^�t�B�[���h�ɑ΂��鎩�������ɂ��DisposeAsync()
        {
            // IAsyncDisposable���������Ă���^�̃����o�������j�����邽�߂ɂ̓����o��IDisposable���������Ă���K�v������̂�
            // �e�X�g�ł͗����̃C���^�[�t�F�C�X����������IAutomaticImplSupportedAsyncDisposable���g�p

            var disposableMock = new Mock<IAutomaticImplSupportedAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());
            disposableMock.Setup(v => v.DisposeAsync()).Returns(default(ValueTask));

            var testeeObject = new GenericTypeFieldClass<IAutomaticImplSupportedAsyncDisposable>(disposableMock.Object);

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "Dispose�̌Ăяo����DisposeAsync�ɓ`�����Ă��܂��Ă���B");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "Dispose�̌Ăяo�����`�����Ă��Ȃ��B");

            testeeObject.IsDisposed.Should().BeTrue("Dispose�̊�����͐^�łȂ���΂Ȃ�Ȃ��B");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
        }

        [Fact]
        public async Task IAsyncDisposable�^�̃t�B�[���h�ɑ΂��鎩�������ɂ��Dispose()
        {
            // IAsyncDisposable���������Ă���^�̃����o�������j�����邽�߂ɂ̓����o��IDisposable���������Ă���K�v������̂�
            // �e�X�g�ł͗����̃C���^�[�t�F�C�X����������IAutomaticImplSupportedAsyncDisposable���g�p

            var disposableMock = new Mock<IAutomaticImplSupportedAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());
            disposableMock.Setup(v => v.DisposeAsync()).Returns(default(ValueTask));

            var testeeObject = new InterfaceFieldClass(disposableMock.Object);

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "Dispose�̌Ăяo�����`�����Ă��Ȃ��B");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "Dispose�̌Ăяo����DisposeAsync�ɓ`�����Ă��܂��Ă���B");

            testeeObject.IsDisposed.Should().BeTrue("Dispose�̊�����͐^�łȂ���΂Ȃ�Ȃ��B");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Once(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Never(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
        }

        [Fact]
        public async Task IAsyncDisposable�^�̃t�B�[���h�ɑ΂��鎩�������ɂ��DisposeAsync()
        {
            // IAsyncDisposable���������Ă���^�̃����o�������j�����邽�߂ɂ̓����o��IDisposable���������Ă���K�v������̂�
            // �e�X�g�ł͗����̃C���^�[�t�F�C�X����������IAutomaticImplSupportedAsyncDisposable���g�p

            var disposableMock = new Mock<IAutomaticImplSupportedAsyncDisposable>(MockBehavior.Strict);

            disposableMock.Setup(v => v.Dispose());
            disposableMock.Setup(v => v.DisposeAsync()).Returns(default(ValueTask));

            var testeeObject = new InterfaceFieldClass(disposableMock.Object);

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "Dispose�̌Ăяo����DisposeAsync�ɓ`�����Ă��܂��Ă���B");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "Dispose�̌Ăяo�����`�����Ă��Ȃ��B");

            testeeObject.IsDisposed.Should().BeTrue("Dispose�̊�����͐^�łȂ���΂Ȃ�Ȃ��B");

            testeeObject.Dispose();
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            disposableMock.Verify(v => v.Dispose(), Times.Never(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            disposableMock.Verify(v => v.DisposeAsync(), Times.Once(), "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
        }

        [Fact]
        public async Task IAsyncDisposable�𒼐ڎ������Ă���^�̃t�B�[���h�ɑ΂��鎩�������ɂ��Dispose()
        {
            var testeeObject = new ImplicitAsyncDisposableImplementClassFieldClass();

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "�ŏ���Dispose�̌Ăяo���͓`������Ȃ���΂Ȃ�Ȃ��B");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(0, "Dispose�̌Ăяo����DisposeAsync�ɓ`�����Ă��܂��Ă���B");

            testeeObject.IsDisposed.Should().BeTrue("Dispose�̊�����͐^�łȂ���΂Ȃ�Ȃ��B");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(0, "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(0, "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
        }

        [Fact]
        public async Task IAsyncDisposable�𒼐ڎ������Ă���^�̃t�B�[���h�ɑ΂��鎩�������ɂ��DisposeAsync()
        {
            var testeeObject = new ImplicitAsyncDisposableImplementClassFieldClass();

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(0, "DisposeAsync�̌Ăяo����Dispose�ɓ`�����Ă��܂��Ă���B");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(1, "�ŏ���DisposeAsync�̌Ăяo���͓`������Ȃ���΂Ȃ�Ȃ��B");

            testeeObject.IsDisposed.Should().BeTrue("Dispose�̊�����͐^�łȂ���΂Ȃ�Ȃ��B");

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(0, "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(1, "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(0, "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(1, "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
        }

        [Fact]
        public async Task IAsyncDisposable�𒼐ږ����I�Ɏ������Ă���^�̃t�B�[���h�ɑ΂��鎩�������ɂ��Dispose()
        {
            var testeeObject = new ExplicitAsyncDisposableImplemetnClassFieldClass();

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "�ŏ���Dispose�̌Ăяo���͓`������Ȃ���΂Ȃ�Ȃ��B");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(0, "Dispose�̌Ăяo����DisposeAsync�ɓ`�����Ă��܂��Ă���B");

            testeeObject.IsDisposed.Should().BeTrue("Dispose�̊�����͐^�łȂ���΂Ȃ�Ȃ��B");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(0, "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(1, "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(0, "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
        }

        [Fact]
        public async Task IAsyncDisposable�𒼐ږ����I�Ɏ������Ă���^�̃t�B�[���h�ɑ΂��鎩�������ɂ��DisposeAsync()
        {
            var testeeObject = new ExplicitAsyncDisposableImplemetnClassFieldClass();

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(0, "DisposeAsync�̌Ăяo����Dispose�ɓ`�����Ă��܂��Ă���B");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(1, "�ŏ���DisposeAsync�̌Ăяo���͓`������Ȃ���΂Ȃ�Ȃ��B");

            testeeObject.IsDisposed.Should().BeTrue("Dispose�̊�����͐^�łȂ���΂Ȃ�Ȃ��B");

            testeeObject.Dispose();
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(0, "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(1, "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");

            await testeeObject.DisposeAsync().ConfigureAwait(false);
            testeeObject.disposable.ManagedContextSyncDisposeCount.Should().Be(0, "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
            testeeObject.disposable.ManagedContextAsyncDisposeCount.Should().Be(1, "�Q��ڈȍ~�̔j���̌Ăяo�����`�����Ă��܂��Ă���B");
        }
    }
}
