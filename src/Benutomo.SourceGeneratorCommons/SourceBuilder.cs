using Microsoft.CodeAnalysis;
using System.Buffers;
using System.Diagnostics;

namespace Benutomo.SourceGeneratorCommons
{
    internal ref struct SourceBuilder
    {
        public string SourceText => _chachedSourceText ??= _buffer.Slice(0, _length).ToString();

        private string? _chachedSourceText;
        private Span<char> _buffer;
        private int _length;
        private char[]? _arrayPoolBuffer;

        public SourceProductionContext Context { get; }

        private int _currentIndentCount = 0;
        private const string IndentText = "    ";

        public SourceBuilder(SourceProductionContext context, Span<char> initialBuffer)
        {
            Context = context;
            _buffer = initialBuffer;

            _chachedSourceText = null;
            _length = 0;
            _arrayPoolBuffer = null;
        }

        public void Dispose()
        {
            if (_arrayPoolBuffer is not null)
            {
                _length = 0;
                _buffer = [];
                ArrayPool<char>.Shared.Return(_arrayPoolBuffer);
                _arrayPoolBuffer = null;
            }
        }

        private void ExpandBuffer(int requiredSize)
        {
            Debug.Assert(_buffer.Length < _length + requiredSize);

            var nextBuffer = ArrayPool<char>.Shared.Rent((_buffer.Length + requiredSize) * 2);

            _buffer.CopyTo(nextBuffer.AsSpan());
            _buffer = nextBuffer;

            if (_arrayPoolBuffer is not null)
            {
                ArrayPool<char>.Shared.Return(_arrayPoolBuffer);
            }

            _arrayPoolBuffer = nextBuffer;
        }

        private void InternalClear()
        {
            if (_length != 0)
            {
                _chachedSourceText = null;
                _length = 0;
            }
        }

        private void InternalAppend(ReadOnlySpan<char> text)
        {
            if (text.Length <= 0) return;

            _chachedSourceText = null;

            if (_buffer.Length < _length + text.Length)
            {
                ExpandBuffer(text.Length);
            }

            text.CopyTo(_buffer.Slice(_length));
            _length += text.Length;
        }

        public void PutIndentSpace()
        {
            for (int i = 0; i < _currentIndentCount; i++)
            {
                InternalAppend(IndentText.AsSpan());
            }
        }

        public void Clear()
        {
            InternalClear();
        }
        public void Append(string text) => Append(text.AsSpan());
        public void AppendLine(string text) => AppendLine(text.AsSpan());
        public void BeginBlock(string text) => BeginBlock(text.AsSpan());

        public void Append(ReadOnlySpan<char> text)
        {
            InternalAppend(text);
        }


        public void AppendLine(ReadOnlySpan<char> text)
        {
            InternalAppend(text);
            AppendLine();
        }

        public void AppendLine()
        {
            InternalAppend("\r\n".AsSpan());
        }

        public void BeginTryBlock()
        {
            BeginBlock("try".AsSpan());
        }

        public void BeginFinallyBlock()
        {
            BeginBlock("finally".AsSpan());
        }

        public void BeginBlock(ReadOnlySpan<char> blockHeadLine)
        {
            Context.CancellationToken.ThrowIfCancellationRequested();

            PutIndentSpace();
            InternalAppend(blockHeadLine);
            AppendLine();
            BeginBlock();
        }

        public void BeginBlock()
        {
            Context.CancellationToken.ThrowIfCancellationRequested();

            PutIndentSpace();
            InternalAppend("{".AsSpan());
            AppendLine();
            _currentIndentCount++;
        }

        public void EndBlock()
        {
            Context.CancellationToken.ThrowIfCancellationRequested();

            _currentIndentCount--;
            PutIndentSpace();
            InternalAppend("}".AsSpan());
            AppendLine();
        }
    }

}
