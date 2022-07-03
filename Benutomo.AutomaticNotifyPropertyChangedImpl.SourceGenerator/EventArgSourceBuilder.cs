using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator
{
    ref struct EventArgSourceBuilder
    {
        public string HintName => $"gen_{string.Join(".", _sourceBuilder.HintingTypeNames)}.EventArgDeclarations_{_sourceBuilder.NameSpace}.cs";

        ClassSourceBuilder _sourceBuilder;

        readonly GenerateEventArgInputs _sourceBuildInputs;


        public EventArgSourceBuilder(SourceProductionContext context, GenerateEventArgInputs sourceBuildInputs, Span<char> initialBuffer)
        {
            _sourceBuildInputs = sourceBuildInputs;
            _sourceBuilder = new ClassSourceBuilder(context, sourceBuildInputs.ContainingTypeInfo, initialBuffer);
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
        public void AppendLine() => _sourceBuilder.AppendLine();
        public void AppendLine(ReadOnlySpan<char> text) => _sourceBuilder.AppendLine(text);
        public void BeginTryBlock() => _sourceBuilder.BeginTryBlock();
        public void BeginFinallyBlock() => _sourceBuilder.BeginFinallyBlock();
        public void BeginBlock(string blockHeadLine) => _sourceBuilder.BeginBlock(blockHeadLine);
        public void BeginBlock(ReadOnlySpan<char> text) => _sourceBuilder.BeginBlock(text);
        public void BeginBlock() => _sourceBuilder.BeginBlock();
        public void EndBlock() => _sourceBuilder.EndBlock();
        public void WriteTypeDeclarationStart() => _sourceBuilder.WriteTypeDeclarationStart();
        public void WriteTypeDeclarationEnd() => _sourceBuilder.WriteTypeDeclarationEnd();
        #endregion

        public void Build()
        {
            _sourceBuilder.Clear();

            AppendLine("#nullable enable");
            AppendLine("#pragma warning disable CS0612 // Obsolete属性でマークされたメソッドの呼び出しに対する警告を抑止");
            AppendLine("#pragma warning disable CS0618 // Obsolete属性でマークされたメソッドの呼び出しに対する警告を抑止");
            AppendLine("#pragma warning disable CS0619 // Obsolete属性でマークされたメソッドの呼び出しに対するエラーを抑止");

            WriteTypeDeclarationStart();

            WriteBody();

            WriteTypeDeclarationEnd();
        }

        void WriteBody()
        {
            foreach (var property in _sourceBuildInputs.Properties)
            {
                if (property.EventArgClass == PropertyEventArgClass.Changed)
                {
                    var changedEventArgFieldName = $"__PropertyChangedEventArgs_{property.Name}";

                    PutIndentSpace();
                    Append("private static global::System.ComponentModel.PropertyChangedEventArgs ");
                    Append(changedEventArgFieldName);
                    Append(" = new global::System.ComponentModel.PropertyChangedEventArgs(\"");
                    Append(property.Name);
                    AppendLine("\");");
                }
                else if (property.EventArgClass == PropertyEventArgClass.Changing)
                {
                    var changingEventArgFieldName = $"__PropertyChangingEventArgs_{property.Name}";

                    PutIndentSpace();
                    Append("private static global::System.ComponentModel.PropertyChangingEventArgs ");
                    Append(changingEventArgFieldName);
                    Append(" = new global::System.ComponentModel.PropertyChangingEventArgs(\"");
                    Append(property.Name);
                    AppendLine("\");");
                }
                else
                {
                    Debug.Fail("invalid PropertyEventArgClass");
                }
            }
        }
    }
}
