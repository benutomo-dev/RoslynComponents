using Microsoft.CodeAnalysis;
using System.Buffers;
using System.Diagnostics;

namespace Benutomo.SourceGeneratorCommons
{
    ref struct SourceBuilder
    {
        public string SourceText => _chachedSourceText ??= _buffer.Slice(0, _length).ToString();

        string? _chachedSourceText;

        Span<char> _buffer;

        int _length;

        char[]? _arrayPoolBuffer;

        public SourceProductionContext Context { get; }

        int _currentIndentCount = 0;

        const string IndentText = "    ";

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
                _buffer = Span<char>.Empty;
                ArrayPool<char>.Shared.Return(_arrayPoolBuffer);
                _arrayPoolBuffer = null;
            }
        }

        void ExpandBuffer(int requiredSize)
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

        void InternalClear()
        {
            if (_length != 0)
            {
                _chachedSourceText = null;
                _length = 0;
            }
        }

        void InternalAppend(ReadOnlySpan<char> text)
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
