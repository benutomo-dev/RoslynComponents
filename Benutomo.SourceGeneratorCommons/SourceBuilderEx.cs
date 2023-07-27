using Microsoft.CodeAnalysis;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace Benutomo.SourceGeneratorCommons;

internal class SourceBuilderEx : IDisposable
{
    public string SourceText => _chachedSourceText ??= _buffer?.AsSpan(0, _length).ToString() ?? "";

    private string? _chachedSourceText;

    private int _length;

    private char[]? _buffer;

    private int _currentIndentCount = 0;

    private SourceProductionContext _context;

    private string _hintName;

    private const string IndentText = "    ";

    private static ConcurrentDictionary<string, int> _initialBufferSizeDictionary = new ConcurrentDictionary<string, int>();

    public SourceBuilderEx(SourceProductionContext context, string hintName)
    {
        _context = context;
        _hintName = hintName;

        if (!_initialBufferSizeDictionary.TryGetValue(hintName, out var initialMinimumCapacityLength))
        {
            initialMinimumCapacityLength = 1024;
        }
        _buffer = ArrayPool<char>.Shared.Rent(initialMinimumCapacityLength);
        _length = 0;
    }

    public void Dispose()
    {
        if (_buffer is not null)
        {
            _length = 0;
            ArrayPool<char>.Shared.Return(_buffer);
            _buffer = null;
        }
    }

    public void Commit()
    {
        _context.AddSource(_hintName, SourceText);

        _initialBufferSizeDictionary.AddOrUpdate(_hintName, _length, (_, _) => _length);

        Dispose();
    }

    void ExpandBuffer(int requiredSize)
    {
        if (_buffer is null) throw new ObjectDisposedException(null);

        _context.CancellationToken.ThrowIfCancellationRequested();

        Debug.Assert(_buffer.Length < _length + requiredSize);

        var nextBuffer = ArrayPool<char>.Shared.Rent((_buffer.Length + requiredSize) * 2);

        _buffer.CopyTo(nextBuffer.AsSpan());

        ArrayPool<char>.Shared.Return(_buffer);

        _buffer = nextBuffer;
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
        if (_buffer is null) throw new ObjectDisposedException(null);

        _context.CancellationToken.ThrowIfCancellationRequested();

        if (text.Length <= 0) return;

        _chachedSourceText = null;

        if (_buffer.Length < _length + text.Length)
        {
            ExpandBuffer(text.Length);
        }

        text.CopyTo(_buffer.AsSpan(_length));
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
    public _BlockEndDisposable BeginBlock(string text) => BeginBlock(text.AsSpan());

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

    public _BlockEndDisposable BeginTypeDefinitionBlock(TypeDefinitionInfo typeDefinitionInfo, string? classDecralationLineTail)
    {
        return beginTypeBlock(this, typeDefinitionInfo, isDesingationType: true, classDecralationLineTail);

        static _BlockEndDisposable beginTypeBlock(SourceBuilderEx self, TypeDefinitionInfo namedTypeSymbol, bool isDesingationType, string? classDecralationLineTail)
        {
            _BlockEndDisposable outerBlockEnd = default;

            if (namedTypeSymbol.Container is NameSpaceInfo nameSpace && !string.IsNullOrWhiteSpace(nameSpace.Name))
            {
                outerBlockEnd = beginNameSpace(self, nameSpace);
            }
            else if (namedTypeSymbol.Container is TypeDefinitionInfo typeInfo)
            {
                outerBlockEnd = beginTypeBlock(self, typeInfo, isDesingationType: false, null);
            }

            self.PutIndentSpace();
            self.Append("partial ");
            self.Append(namedTypeSymbol.IsValueType ? "struct " : "class ");
            self.Append(namedTypeSymbol.Name);

            if (namedTypeSymbol.GenericTypeArgs.Length > 0)
            {
                self.Append("<");

                for (int i = 0; i < namedTypeSymbol.GenericTypeArgs.Length; i++)
                {
                    var genericTypeArg = namedTypeSymbol.GenericTypeArgs[i];

                    self.Append(genericTypeArg);

                    if (i < namedTypeSymbol.GenericTypeArgs.Length - 1)
                    {
                        self.Append(", ");
                    }
                }

                self.Append(">");

                var hintingTypeNameBuilder = new StringBuilder();

                hintingTypeNameBuilder.Append(namedTypeSymbol.Name);
                hintingTypeNameBuilder.Append("{");
                hintingTypeNameBuilder.Append(string.Join("_", namedTypeSymbol.GenericTypeArgs));
                hintingTypeNameBuilder.Append("}");
            }

            if (isDesingationType && classDecralationLineTail is not null)
            {
                self.Append(classDecralationLineTail);
            }
            self.AppendLine("");

            return self.BeginBlock().Combine(outerBlockEnd);
        }

        static _BlockEndDisposable beginNameSpace(SourceBuilderEx self, NameSpaceInfo namespaceSymbol)
        {
            self.PutIndentSpace();
            self.Append("namespace ");
            self.Append(namespaceSymbol.Name);
            self.AppendLine("");

            return self.BeginBlock();
        }
    }

    public _BlockEndDisposable BeginTryBlock()
    {
        return BeginBlock("try".AsSpan());
    }

    public _BlockEndDisposable BeginFinallyBlock()
    {
        return BeginBlock("finally".AsSpan());
    }

    public _BlockEndDisposable BeginBlock(ReadOnlySpan<char> blockHeadLine)
    {
        PutIndentSpace();
        InternalAppend(blockHeadLine);
        AppendLine();
        return BeginBlock();
    }

    public _BlockEndDisposable BeginBlock()
    {
        PutIndentSpace();
        InternalAppend("{".AsSpan());
        AppendLine();
        _currentIndentCount++;

        return new _BlockEndDisposable(this);
    }

    private void EndBlock()
    {
        _currentIndentCount--;
        PutIndentSpace();
        InternalAppend("}".AsSpan());
        AppendLine();
    }


    public struct _BlockEndDisposable : IDisposable
    {
        private SourceBuilderEx? _sourceBuilder;

        private int _nestCount;

        internal _BlockEndDisposable(SourceBuilderEx? sourceBuilder)
        {    
            _sourceBuilder = sourceBuilder;
            _nestCount = 1;
        }

        public _BlockEndDisposable Combine(_BlockEndDisposable other)
        {
            if (other._sourceBuilder is null && _nestCount == 0) return this;

            if (_nestCount <= 0) throw new InvalidOperationException();
            if (_sourceBuilder != other._sourceBuilder) throw new ArgumentException(nameof(other));
            if (other._nestCount <= 0) throw new ArgumentException(nameof(other));

            return new _BlockEndDisposable
            {
                _sourceBuilder = _sourceBuilder,
                _nestCount = _nestCount + other._nestCount,
            };
        }

        public void Dispose()
        {
            if (_sourceBuilder is not null)
            {
                for (int i = 0; i < _nestCount; i++)
                {
                    _sourceBuilder.EndBlock();
                }
            }
            _sourceBuilder = null;
            _nestCount = 0;
        }
    }
}
