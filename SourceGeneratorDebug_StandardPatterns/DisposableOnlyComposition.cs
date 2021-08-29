using Benutomo;
using System;
using System.IO;

namespace SourceGeneratorDebug_StandardPatterns.Nest
{
    [AutomaticDisposeImpl]
    public partial class DisposableOnlyComposition1 : IDisposable
    {
        BinaryReader _binaryReaderFieald;
        readonly BinaryReader _binaryReaderReadonlyFieald;
        static BinaryReader s_binaryReaderFieald = null;
        static readonly BinaryReader s_binaryReaderReadonlyFieald = null;

        BinaryReader _binaryReaderProperty { get; set; }
        BinaryReader _binaryReaderGetonlyProperty { get; }
        static BinaryReader s_binaryReaderProperty { get; set; }
        static BinaryReader s_binaryReaderGetonlyProperty { get; }

        public DisposableOnlyComposition1(BinaryReader binaryReaderFieald, BinaryReader binaryReaderReadonlyFieald, BinaryReader binaryReaderProperty, BinaryReader binaryReaderGetonlyProperty)
        {
            _binaryReaderFieald = binaryReaderFieald ?? s_binaryReaderFieald ?? s_binaryReaderReadonlyFieald;
            _binaryReaderReadonlyFieald = binaryReaderReadonlyFieald;
            _binaryReaderProperty = binaryReaderProperty;
            _binaryReaderGetonlyProperty = binaryReaderGetonlyProperty;
        }
    }

    [AutomaticDisposeImpl]
    public partial class DisposableOnlyComposition2 : IDisposable, IAsyncDisposable
    {
        BinaryReader _binaryReaderFieald;
        readonly BinaryReader _binaryReaderReadonlyFieald;
        static BinaryReader s_binaryReaderFieald = null;
        static readonly BinaryReader s_binaryReaderReadonlyFieald = null;

        BinaryReader _binaryReaderProperty { get; set; }
        BinaryReader _binaryReaderGetonlyProperty { get; }
        static BinaryReader s_binaryReaderProperty { get; set; }
        static BinaryReader s_binaryReaderGetonlyProperty { get; }

        public DisposableOnlyComposition2(BinaryReader binaryReaderFieald, BinaryReader binaryReaderReadonlyFieald, BinaryReader binaryReaderProperty, BinaryReader binaryReaderGetonlyProperty)
        {
            _binaryReaderFieald = binaryReaderFieald ?? s_binaryReaderFieald ?? s_binaryReaderReadonlyFieald;
            _binaryReaderReadonlyFieald = binaryReaderReadonlyFieald;
            _binaryReaderProperty = binaryReaderProperty;
            _binaryReaderGetonlyProperty = binaryReaderGetonlyProperty;
        }
    }
}
