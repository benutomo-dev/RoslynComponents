using Microsoft.CodeAnalysis;
using System.Text;

namespace Benutomo.SourceGeneratorCommons
{
    ref struct ClassSourceBuilder
    {
        SourceBuilder _sourceBuilder;

        TypeDefinitionInfo ContainingTypeInfo { get; }

        public string NameSpace => _nameSpace;

        public List<string> HintingTypeNames => _hintingTypeNames;

        string _nameSpace = "";

        readonly List<string> _hintingTypeNames = new List<string>();


        public ClassSourceBuilder(SourceProductionContext context, TypeDefinitionInfo containingTypeInfo, Span<char> initialBuffer)
        {
            ContainingTypeInfo = containingTypeInfo;
            _sourceBuilder = new SourceBuilder(context, initialBuffer);
        }

        public void Dispose()
        {
            _sourceBuilder.Dispose();
        }

        #region _sourceBuilder Methods
        public SourceProductionContext Context => _sourceBuilder.Context;
        public string SourceText => _sourceBuilder.SourceText;
        public void PutIndentSpace() => _sourceBuilder.PutIndentSpace();
        public void Clear() => _sourceBuilder.Clear();
        public void Append(string text) => _sourceBuilder.Append(text);
        public void Append(ReadOnlySpan<char> text) => _sourceBuilder.Append(text);
        public void AppendLine(string text) => _sourceBuilder.AppendLine(text);
        public void AppendLine(ReadOnlySpan<char> text) => _sourceBuilder.AppendLine(text);
        public void AppendLine() => _sourceBuilder.AppendLine();
        public void BeginTryBlock() => _sourceBuilder.BeginTryBlock();
        public void BeginFinallyBlock() => _sourceBuilder.BeginFinallyBlock();
        public void BeginBlock(string blockHeadLine) => _sourceBuilder.BeginBlock(blockHeadLine);
        public void BeginBlock(ReadOnlySpan<char> blockHeadLine) => _sourceBuilder.BeginBlock(blockHeadLine);
        public void BeginBlock() => _sourceBuilder.BeginBlock();
        public void EndBlock() => _sourceBuilder.EndBlock();
        #endregion

        public void WriteTypeDeclarationStart()
        {
            _nameSpace = "global";
            _hintingTypeNames.Clear();

            WriteContainingTypeStart(ContainingTypeInfo, isDesingationType: true);
        }

        void WriteContainingTypeStart(TypeDefinitionInfo namedTypeSymbol, bool isDesingationType)
        {
            if (namedTypeSymbol.Container is NameSpaceInfo nameSpace && !string.IsNullOrWhiteSpace(nameSpace.Name))
            {
                WriteContainingNameSpaceStart(nameSpace);
            }
            else if (namedTypeSymbol.Container is TypeDefinitionInfo typeInfo)
            {
                WriteContainingTypeStart(typeInfo, isDesingationType: false);
            }

            Context.CancellationToken.ThrowIfCancellationRequested();

            PutIndentSpace();
            Append("partial ");
            Append(namedTypeSymbol.IsValueType ? "struct " : "class ");
            Append(namedTypeSymbol.Name);

            if (namedTypeSymbol.GenericTypeArgs.Length > 0)
            {
                Append("<");

                for (int i = 0; i < namedTypeSymbol.GenericTypeArgs.Length; i++)
                {
                    var genericTypeArg = namedTypeSymbol.GenericTypeArgs[i];

                    Append(genericTypeArg);

                    if (i < namedTypeSymbol.GenericTypeArgs.Length - 1)
                    {
                        Append(", ");
                    }
                }

                Append(">");

                var hintingTypeNameBuilder = new StringBuilder();

                hintingTypeNameBuilder.Append(namedTypeSymbol.Name);
                hintingTypeNameBuilder.Append("{");
                hintingTypeNameBuilder.Append(string.Join("_", namedTypeSymbol.GenericTypeArgs));
                hintingTypeNameBuilder.Append("}");

                _hintingTypeNames.Add(hintingTypeNameBuilder.ToString());
            }
            else
            {
                _hintingTypeNames.Add(namedTypeSymbol.Name);
            }

            if (isDesingationType)
            {
                // なくてもいいが生成されたコードだけを見ても実装対象となっているインターフェイスが分かるようにしておく

                Append(" // This is implementation class by AutomaticNotifyPropertyChangedImpl.");
            }
            AppendLine("");

            BeginBlock();
        }

        void WriteContainingNameSpaceStart(NameSpaceInfo namespaceSymbol)
        {
            PutIndentSpace();
            Append("namespace ");
            Append(namespaceSymbol.Name);
            AppendLine("");

            _nameSpace = namespaceSymbol.Name;

            BeginBlock();
        }

        public void WriteTypeDeclarationEnd()
        {
            WriteContainingTypeEnd(ContainingTypeInfo);
        }

        void WriteContainingTypeEnd(TypeDefinitionInfo namedTypeSymbol)
        {
            EndBlock();

            Context.CancellationToken.ThrowIfCancellationRequested();


            if (namedTypeSymbol.Container is NameSpaceInfo nameSpace && !string.IsNullOrWhiteSpace(nameSpace.Name))
            {
                WriteContainingNameSpaceEnd(nameSpace);
            }
            else if (namedTypeSymbol.Container is TypeDefinitionInfo typeInfo)
            {
                WriteContainingTypeEnd(typeInfo);
            }
        }

        void WriteContainingNameSpaceEnd(NameSpaceInfo namespaceSymbol)
        {
            EndBlock();
        }
    }

}
