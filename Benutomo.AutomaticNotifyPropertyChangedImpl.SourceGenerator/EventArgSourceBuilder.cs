﻿using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator
{
    ref struct EventArgSourceBuilder
    {
        public string HintName => $"gen_{string.Join(".", _sourceBuilder.HintingTypeNames)}.EventArgDeclarations_{_sourceBuilder.NameSpace}.cs";

        ClassSourceBuilder _sourceBuilder;

        readonly EventArgSourceBuilderInputs _sourceBuildInputs;


        public EventArgSourceBuilder(SourceProductionContext context, EventArgSourceBuilderInputs sourceBuildInputs, Span<char> initialBuffer)
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
        public void WriteTypeDeclarationStart(string? classDecralationLineComment) => _sourceBuilder.WriteTypeDeclarationStart(classDecralationLineComment);
        public void WriteTypeDeclarationEnd() => _sourceBuilder.WriteTypeDeclarationEnd();
        #endregion

        public void Build()
        {
            _sourceBuilder.Clear();

            AppendLine("#nullable enable");
            AppendLine("#pragma warning disable CS0612 // Obsolete属性でマークされたメソッドの呼び出しに対する警告を抑止");
            AppendLine("#pragma warning disable CS0618 // Obsolete属性でマークされたメソッドの呼び出しに対する警告を抑止");
            AppendLine("#pragma warning disable CS0619 // Obsolete属性でマークされたメソッドの呼び出しに対するエラーを抑止");

            WriteTypeDeclarationStart("This is implementation class by AutomaticNotifyPropertyChangedImpl.");

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
                    
                    AppendLine();

                    PutIndentSpace(); AppendLine("/// <summary>");
                    PutIndentSpace();
                    Append("/// プロパティ名として<value>\"");
                    Append(property.Name);
                    AppendLine("\"</value>を設定した<see cref=\"global::System.ComponentModel.PropertyChangedEventArgs\" />クラスのインスタンスが設定された読取専用フィールドです。ユーザが実装するコード内からPropertyChangedイベントを発生させる場合などに利用することが出来ます。");
                    PutIndentSpace(); AppendLine("/// </summary>");
                    PutIndentSpace();
                    Append("private static readonly global::System.ComponentModel.PropertyChangedEventArgs ");
                    Append(changedEventArgFieldName);
                    Append(" = new global::System.ComponentModel.PropertyChangedEventArgs(\"");
                    Append(property.Name);
                    AppendLine("\");");
                }
                else if (property.EventArgClass == PropertyEventArgClass.Changing)
                {
                    var changingEventArgFieldName = $"__PropertyChangingEventArgs_{property.Name}";

                    AppendLine();

                    PutIndentSpace(); AppendLine("/// <summary>");
                    PutIndentSpace();
                    Append("/// プロパティ名として<value>\"");
                    Append(property.Name);
                    AppendLine("\"</value>を設定した<see cref=\"global::System.ComponentModel.PropertyChangingEventArgs\"クラスのインスタンスが設定された読取専用フィールドです。ユーザが実装するコード内からPropertyChangingイベントを発生させる場合などに利用することが出来ます。");
                    PutIndentSpace(); AppendLine("/// </summary>");
                    PutIndentSpace();
                    Append("private static readonly global::System.ComponentModel.PropertyChangingEventArgs ");
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
