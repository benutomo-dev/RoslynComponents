using Benutomo;
using System;
using System.IO;

namespace SourceGeneratorDebug_StandardPatterns.AutomaticDisposeImpl
{
    [AutomaticDisposeImpl]
    public partial class DisposableOnlyComposition1 : IDisposable
    {

        [EnableAutomaticDispose]
        BinaryReader? _binaryReaderFieald;

        [EnableAutomaticDispose]
        readonly BinaryReader? _binaryReaderReadonlyFieald;

        static BinaryReader? s_binaryReaderFieald = null;

        static readonly BinaryReader? s_binaryReaderReadonlyFieald = null;


        [EnableAutomaticDispose]
        BinaryReader? _binaryReaderProperty { get; set; }

        [EnableAutomaticDispose]
        BinaryReader? _binaryReaderGetonlyProperty { get; }

        static BinaryReader? s_binaryReaderProperty { get; set; }

        static BinaryReader? s_binaryReaderGetonlyProperty { get; }

        public DisposableOnlyComposition1(BinaryReader binaryReaderFieald, BinaryReader binaryReaderReadonlyFieald, BinaryReader binaryReaderProperty, BinaryReader binaryReaderGetonlyProperty)
        {
            _binaryReaderFieald = binaryReaderFieald ?? s_binaryReaderFieald ?? s_binaryReaderReadonlyFieald;
            _binaryReaderReadonlyFieald = binaryReaderReadonlyFieald;
            _binaryReaderProperty = binaryReaderProperty;
            _binaryReaderGetonlyProperty = binaryReaderGetonlyProperty;
        }
    }
}
 
namespace SourceGeneratorDebug_StandardPatterns.AutomaticDisposeImpl.Nest
{
    [AutomaticDisposeImpl]
    public partial class DisposableOnlyComposition2 : IDisposable, IAsyncDisposable
    {

        [EnableAutomaticDispose]
        BinaryReader? _binaryReaderFieald;

        [EnableAutomaticDispose]
        readonly BinaryReader? _binaryReaderReadonlyFieald;

        static BinaryReader? s_binaryReaderFieald = null;

        static readonly BinaryReader? s_binaryReaderReadonlyFieald = null;


        [EnableAutomaticDispose]
        BinaryReader? _binaryReaderProperty { get; set; }

        [EnableAutomaticDispose]
        BinaryReader? _binaryReaderGetonlyProperty { get; }

        static BinaryReader? s_binaryReaderProperty { get; set; }

        static BinaryReader? s_binaryReaderGetonlyProperty { get; }

        public DisposableOnlyComposition2(BinaryReader binaryReaderFieald, BinaryReader binaryReaderReadonlyFieald, BinaryReader binaryReaderProperty, BinaryReader binaryReaderGetonlyProperty)
        {
            _binaryReaderFieald = binaryReaderFieald ?? s_binaryReaderFieald ?? s_binaryReaderReadonlyFieald;
            _binaryReaderReadonlyFieald = binaryReaderReadonlyFieald;
            _binaryReaderProperty = binaryReaderProperty;
            _binaryReaderGetonlyProperty = binaryReaderGetonlyProperty;
        }
    }
}

namespace SourceGeneratorDebug_StandardPatterns.AutomaticDisposeImpl
{
    namespace Nest
    {
        [AutomaticDisposeImpl]
        public partial class DisposableOnlyComposition3 : IAsyncDisposable
        {

            [EnableAutomaticDispose]
            BinaryReader? _binaryReaderFieald;

            [EnableAutomaticDispose]
            readonly BinaryReader? _binaryReaderReadonlyFieald;

            static BinaryReader? s_binaryReaderFieald = null;

            static readonly BinaryReader? s_binaryReaderReadonlyFieald = null;


            [EnableAutomaticDispose]
            BinaryReader? _binaryReaderProperty { get; set; }

            [EnableAutomaticDispose]
            BinaryReader? _binaryReaderGetonlyProperty { get; }

            static BinaryReader? s_binaryReaderProperty { get; set; }

            static BinaryReader? s_binaryReaderGetonlyProperty { get; }

            public DisposableOnlyComposition3(BinaryReader binaryReaderFieald, BinaryReader binaryReaderReadonlyFieald, BinaryReader binaryReaderProperty, BinaryReader binaryReaderGetonlyProperty)
            {
                _binaryReaderFieald = binaryReaderFieald ?? s_binaryReaderFieald ?? s_binaryReaderReadonlyFieald;
                _binaryReaderReadonlyFieald = binaryReaderReadonlyFieald;
                _binaryReaderProperty = binaryReaderProperty;
                _binaryReaderGetonlyProperty = binaryReaderGetonlyProperty;
            }
        }
    }
}
