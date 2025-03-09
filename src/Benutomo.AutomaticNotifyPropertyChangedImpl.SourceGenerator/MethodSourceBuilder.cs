﻿using Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator.Embedding;
using Microsoft.CodeAnalysis;
using static SourceGeneratorCommons.SourceBuilder;

namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator
{
    class MethodSourceBuilder : IDisposable
    {
        delegate void MethodSourceBuilderAction(MethodSourceBuilder builder);
        delegate void MethodSourceBuilderAction<T1>(MethodSourceBuilder builder, T1 arg1);

        SourceProductionContext _context;

        SourceBuilder _sourceBuilder;

        MethodSourceBuildInputs _sourceBuildInputs;


        public MethodSourceBuilder(SourceProductionContext context, MethodSourceBuildInputs sourceBuildInputs)
        {
            _sourceBuildInputs = sourceBuildInputs;
            _sourceBuilder = new SourceBuilder(context, $"{sourceBuildInputs.ContainingType.MakeStandardHintName()}.EventMethods.{_sourceBuildInputs.InternalPropertyName}");
        }

        public void Commit()
        {
            _sourceBuilder.Commit();
        }

        public void Dispose()
        {
            _sourceBuilder.Dispose();
        }

        #region _sourceBuilder Methods
        public SourceProductionContext Context => _context;
        public string SourceText => _sourceBuilder.SourceText;
        public void PutIndentSpace() => _sourceBuilder.PutIndentSpace();
        public void Clear() => _sourceBuilder.Clear();
        public void Append(string text) => _sourceBuilder.Append(text);
        public void Append(ReadOnlySpan<char> text) => _sourceBuilder.Append(text);
        public void AppendLine(string text) => _sourceBuilder.AppendLine(text);
        public void AppendLine(ReadOnlySpan<char> text) => _sourceBuilder.AppendLine(text);
        public void AppendLine() => _sourceBuilder.AppendLine();
        public _BlockEndDisposable BeginBlock(string blockHeadLine) => _sourceBuilder.BeginBlock(blockHeadLine);
        public _BlockEndDisposable BeginBlock(ReadOnlySpan<char> text) => _sourceBuilder.BeginBlock(text);
        public _BlockEndDisposable BeginBlock() => _sourceBuilder.BeginBlock();
        public _BlockEndDisposable BeginTypeDeclaration(string? classDecralationLineComment) => _sourceBuilder.BeginTypeDefinitionBlock(_sourceBuildInputs.ContainingType, classDecralationLineComment);
        #endregion

        public void Build()
        {
            _sourceBuilder.Clear();

#if !DEBUG
            AppendLine("// <auto-generated />");
#endif

            AppendLine("#nullable enable");
            AppendLine("#pragma warning disable CA1507");
            AppendLine("#pragma warning disable CS0612 // Obsolete属性でマークされたメソッドの呼び出しに対する警告を抑止");
            AppendLine("#pragma warning disable CS0618 // Obsolete属性でマークされたメソッドの呼び出しに対する警告を抑止");
            AppendLine("#pragma warning disable CS0619 // Obsolete属性でマークされたメソッドの呼び出しに対するエラーを抑止");
            AppendLine("#pragma warning disable CS0436");

            using (BeginTypeDeclaration(" // This is implementation class by AutomaticNotifyPropertyChangedImpl."))
            {
                WriteBody();
            }
        }

        void WriteBody()
        {
            string changedEventBaseName;
            if (_sourceBuildInputs.ChangedEventAccessibility == GenerateMemberAccessibility.PrivateForExplicitImplimetOnly)
            {
                changedEventBaseName = _sourceBuildInputs.FieldName;
            }
            else
            {
                changedEventBaseName = _sourceBuildInputs.DefaultNotificationPropertyName;
            }

            string changingEventBaseName;
            if (_sourceBuildInputs.ChangedEventAccessibility == GenerateMemberAccessibility.PrivateForExplicitImplimetOnly)
            {
                changingEventBaseName = _sourceBuildInputs.FieldName;
            }
            else
            {
                changingEventBaseName = _sourceBuildInputs.DefaultNotificationPropertyName;
            }


            foreach (var explicitImplementation in _sourceBuildInputs.ExplicitInterfaceImplementations)
            {
                // 実装インターフェイスのイベントの明示的実装
                AppendLine();
                RenderExpliciteImplimentaionEvent(explicitImplementation, changedEventBaseName, changingEventBaseName);
            }

            if (_sourceBuildInputs.ChangedEventAccessibility != GenerateMemberAccessibility.None)
            {
                // Changedイベントの実装
                AppendLine();
                RenderChangedEvent(changedEventBaseName);
            }

            if (_sourceBuildInputs.ChangingEventAccessibility != GenerateMemberAccessibility.None)
            {
                // Changingイベントの実装
                AppendLine();
                RenderChangngEvent(changingEventBaseName);
            }

            if (_sourceBuildInputs.ChangedObservableAccesibility != GenerateMemberAccessibility.None)
            {
                // ChangedAsObservable(bool pushValueAtSubscribed = false)の実装
                AppendLine();
                RenderChangedAsObservable();
            }

            if (_sourceBuildInputs.ChangingObservableAccesibility != GenerateMemberAccessibility.None)
            {
                // ChangingAsObservable()の実装
                AppendLine();
                RenderChangingAsObservable();
            }

            // フィールドメンバの定義
            AppendLine();
            RenderInternalField();

            // getterメソッドの実装
            AppendLine();
            RenderGetterMethod();
            
            // デフォルトsetterメソッドの実装
            AppendLine();
            RenderDefaultSetterMethod(changedEventBaseName, changingEventBaseName);

            // デフォルトsetterメソッドの実装
            if (_sourceBuildInputs.PropertyTypeIsInterlockExchangeable)
            {
                AppendLine();
                RenderInterlockedExchangeSetterMethod(changedEventBaseName, changingEventBaseName);
            }

            // IEqualityComparer指定付きsetterメソッドの実装
            AppendLine();
            RenderCustomEqualsSetterMethod(changedEventBaseName, changingEventBaseName);


            // 遅延通知版setterメソッドの実装
            AppendLine();
            RenderDeferredNotificationSetterMethod(changedEventBaseName, changingEventBaseName);

            // IEqualityComparer指定付き遅延通知版setterメソッドの実装
            AppendLine();
            RenderCustomEqualsDeferredNotificationSetterMethod(changedEventBaseName, changingEventBaseName);

            // 遅延通知用structの定義
            AppendLine();
            RenderDefferedNotificationStruct(changedEventBaseName);

            return;
        }

        static string ToAccessibilityToken(GenerateMemberAccessibility accessibility)
        {
            switch (accessibility)
            {
                case GenerateMemberAccessibility.Public: return "public";
                case GenerateMemberAccessibility.Protected: return "protected";
                case GenerateMemberAccessibility.Internal: return "internal";
                case GenerateMemberAccessibility.ProrectedInternal: return "protected internal";
                case GenerateMemberAccessibility.PrivateProrected: return "private protected";
                case GenerateMemberAccessibility.Private: return "private";
                case GenerateMemberAccessibility.PrivateForExplicitImplimetOnly: return "private";
                default: throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// <see cref="explicitImplementation"/>の<see cref="ExplicitImplementationArgs.EventType"/>の値で以下のいずれかを生成
        /// <code>event global::System.EventHandler? global::Namespace.IInterface.PropertyXChanging { add => PropertyXChanging += value; remove => PropertyXChanging += value;}</code>
        /// <code>event global::System.EventHandler? global::Namespace.IInterface.ProcertyXChanged  { add => ProcertyXChanged  += value; remove => ProcertyXChanged  += value;}</code>
        /// </summary>
        void RenderExpliciteImplimentaionEvent(ExplicitImplementationArgs explicitImplementation, string changedEventBaseName, string changingEventBaseName)
        {
            string redirectEventHandlerBasename;
            string redirectEventHandlerSuffix;
            switch (explicitImplementation.EventType)
            {
                case ExplicitImplementationEventType.ChangedEventHandler:
                    redirectEventHandlerBasename = changedEventBaseName;
                    redirectEventHandlerSuffix = "Changed";
                    break;
                case ExplicitImplementationEventType.ChangingEventHandler:
                    redirectEventHandlerBasename = changingEventBaseName;
                    redirectEventHandlerSuffix = "Changing";
                    break;
                default:
                    return;
            }

            RenderExcludeFromCodeCoverageAttribute();
            PutIndentSpace();
            Append("event global::System.EventHandler? ");
            Append(explicitImplementation.InterfaceType);
            Append(".");
            Append(explicitImplementation.InterfaceEventName);
            Append(" { add => ");
            Append(redirectEventHandlerBasename);
            Append(redirectEventHandlerSuffix);
            Append(" += value; remove => ");
            Append(redirectEventHandlerBasename);
            Append(redirectEventHandlerSuffix);
            AppendLine(" += value;}");
        }

        void RenderChangedEvent(string changedEventBaseName)
        {
            RenderExcludeFromCodeCoverageAttribute();
            PutIndentSpace();
            Append(ToAccessibilityToken(_sourceBuildInputs.ChangedEventAccessibility));
            Append(" event global::System.EventHandler? ");
            Append(changedEventBaseName);
            AppendLine("Changed;");
        }

        void RenderChangngEvent(string changingEventBaseName)
        {
            RenderExcludeFromCodeCoverageAttribute();
            PutIndentSpace();
            Append(ToAccessibilityToken(_sourceBuildInputs.ChangingEventAccessibility));
            Append(" event global::System.EventHandler? ");
            Append(changingEventBaseName);
            AppendLine("Changing;");
        }

        void RenderChangedAsObservable()
        {
            RenderExcludeFromCodeCoverageAttribute();
            RenderAggressiveInliningAttribute();
            RenderAutoGeneratedInternalAccessorMethodAttribute();
            PutIndentSpace();
            Append(ToAccessibilityToken(_sourceBuildInputs.ChangedObservableAccesibility));
            Append(" global::System.IObservable<");
            Append(_sourceBuildInputs.PropertyType);
            Append("> ");
            Append(_sourceBuildInputs.DefaultNotificationPropertyName);
            Append("ChangedAsObservable(bool pushValueAtSubscribed = false) =>  new global::Benutomo.Internal.EventToObservable<");
            Append(_sourceBuildInputs.PropertyType);
            Append(">(h => ");
            Append(_sourceBuildInputs.DefaultNotificationPropertyName);
            Append("Changed += h, h => ");
            Append(_sourceBuildInputs.DefaultNotificationPropertyName);
            Append("Changed -= h, () => ");
            Append(_sourceBuildInputs.FieldName);
            AppendLine(", pushValueAtSubscribed);");
        }

        void RenderChangingAsObservable()
        {
            RenderExcludeFromCodeCoverageAttribute();
            RenderAggressiveInliningAttribute();
            RenderAutoGeneratedInternalAccessorMethodAttribute();
            PutIndentSpace();
            Append(ToAccessibilityToken(_sourceBuildInputs.ChangingObservableAccesibility));
            Append(" global::System.IObservable<object?> ");
            Append(_sourceBuildInputs.DefaultNotificationPropertyName);
            Append("ChangingAsObservable() =>  new global::Benutomo.Internal.EventToObservable<object?>(h => ");
            Append(_sourceBuildInputs.DefaultNotificationPropertyName);
            Append("Changing += h, h => ");
            Append(_sourceBuildInputs.DefaultNotificationPropertyName);
            AppendLine("Changing -= h, () => this, pushValueAtSubscribed: false);");
        }

        void RenderDefferedNotificationStruct(string changedEventBaseName)
        {
            RenderExcludeFromCodeCoverageAttribute();
            using (BeginBlock($"public ref struct {_sourceBuildInputs.DefferedNotificationDisposableName}"))
            {
                PutIndentSpace();
                Append("private ");
                Append(_sourceBuildInputs.ContainingType.Name);
                if (_sourceBuildInputs.ContainingType is CsGenericDefinableTypeDeclaration { GenericTypeParams : { IsDefaultOrEmpty: false } genericTypeParams1 })
                {
                    Append("<");

                    for (int i = 0; i < genericTypeParams1.Length; i++)
                    {
                        var genericTypeArg = genericTypeParams1[i];

                        Append(genericTypeArg.Name);

                        if (i < genericTypeParams1.Length - 1)
                        {
                            Append(", ");
                        }
                    }

                    Append(">");
                }
                AppendLine("? _source;");

                AppendLine();

                Append($"internal {_sourceBuildInputs.DefferedNotificationDisposableName}({_sourceBuildInputs.ContainingType.Name}");
                if (_sourceBuildInputs.ContainingType is CsGenericDefinableTypeDeclaration { GenericTypeParams: { IsDefaultOrEmpty: false } genericTypeParams2 })
                {
                    Append("<");

                    for (int i = 0; i < genericTypeParams2.Length; i++)
                    {
                        var genericTypeArg = genericTypeParams2[i];

                        Append(genericTypeArg.Name);

                        if (i < genericTypeParams2.Length - 1)
                        {
                            Append(", ");
                        }
                    }

                    Append(">");
                }
                Append($"? source)");
                using (BeginBlock())
                {
                    PutIndentSpace();
                    AppendLine("_source = source;");
                }

                AppendLine();

                using (BeginBlock("public void Dispose()"))
                {
                    using (BeginBlock("if (_source is not null)"))
                    {
                        RenderChangedNotificationSection("_source", changedEventBaseName);

                        PutIndentSpace();
                        AppendLine("_source = null;");
                    }
                }
            }
        }

        void RenderInternalField()
        {
            PutIndentSpace(); AppendLine("/// <summary>");
            PutIndentSpace();
            Append("/// <see cref=\"");
            Append(_sourceBuildInputs.MethodName);
            AppendLine("\" />メソッドが読み書きする内部フィールドです。");
            PutIndentSpace(); AppendLine("/// </summary>");

            PutIndentSpace();
            AppendLine(@"[global::System.Diagnostics.DebuggerBrowsableAttribute(global::System.Diagnostics.DebuggerBrowsableState.Never)]");

            PutIndentSpace();
            Append(@"[global::");
            Append(StaticSourceAttribute.GetFullyQualifiedMetadataName<AutoGeneratedInternalFieldAttribute>());
            Append(@"(""");
            Append(_sourceBuildInputs.DefaultNotificationPropertyName);
            Append(@""")");
            AppendLine(@"]");

            PutIndentSpace();
            Append("private ");
            Append(_sourceBuildInputs.PropertyType);
            Append(" ");
            Append(_sourceBuildInputs.FieldName);
            AppendLine(";");
        }

        void RenderExcludeFromCodeCoverageAttribute()
        {
            PutIndentSpace();
            AppendLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
        }

        void RenderAggressiveInliningAttribute()
        {
            PutIndentSpace();
            AppendLine("[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        }

        void RenderAutoGeneratedInternalAccessorMethodAttribute()
        {
            PutIndentSpace();
            Append(@"[global::");
            Append(StaticSourceAttribute.GetFullyQualifiedMetadataName<AutoGeneratedInternalAccessorMethodAttribute>());
            AppendLine(@"]");
        }

        void RenderGetterMethod()
        {
            PutIndentSpace(); AppendLine("/// <summary>");
            PutIndentSpace();
            Append("/// ");
            Append(_sourceBuildInputs.DefaultNotificationPropertyName);
            AppendLine("プロパティの自動実装ゲッタメソッドです。");
            PutIndentSpace(); AppendLine("/// </summary>");
            RenderExcludeFromCodeCoverageAttribute();
            RenderAggressiveInliningAttribute();
            RenderAutoGeneratedInternalAccessorMethodAttribute();
            PutIndentSpace();
            Append("private ");
            Append(_sourceBuildInputs.PropertyType);
            Append(" ");
            Append(_sourceBuildInputs.MethodName);
            Append("() => this.");
            Append(_sourceBuildInputs.FieldName);
            AppendLine(";");
        }

        void RenderSetterMethodDeclarationCommonPrefixPart(string returnType, string methodName)
        {
            if (_sourceBuildInputs.PropertyTypeIsReferenceType && _sourceBuildInputs.PropertyTypeNullableAnnotation == NullableAnnotation.NotAnnotated)
            {
                PutIndentSpace();
                Append(@"[global::System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(");
                Append(_sourceBuildInputs.FieldName);
                AppendLine(@"))]");
            }
            RenderExcludeFromCodeCoverageAttribute();
            RenderAggressiveInliningAttribute();
            RenderAutoGeneratedInternalAccessorMethodAttribute();
            PutIndentSpace();
            Append("private ");
            Append(returnType);
            Append(" ");
            Append(methodName);
            Append("(");
            Append(_sourceBuildInputs.PropertyType);
            Append(" value");
        }

        void RenderDefaultSetterMethodImplCommonPrefixPart()
        {

            if (_sourceBuildInputs.PropertyTypeIsReferenceType)
            {
                if (_sourceBuildInputs.PropertyTypeNullableAnnotation == NullableAnnotation.NotAnnotated)
                {
                    PutIndentSpace();
                    AppendLine("if (value is null) throw new global::System.ArgumentNullException(nameof(value));");
                }
                else if (_sourceBuildInputs.PropertyTypeNullableAnnotation == NullableAnnotation.None)
                {
                    var descripter = new DiagnosticDescriptor("SGN001", "Nullable context is not enabled.", "Set the Nullable property to enable in the project file or set #nullable enable in the source code.", "code", DiagnosticSeverity.Warning, isEnabledByDefault: true);

                    foreach (var declaration in _sourceBuildInputs.PropertyDeclaringSyntaxReferences)
                    {
                        Context.ReportDiagnostic(Diagnostic.Create(descripter, declaration.GetSyntax(Context.CancellationToken).GetLocation()));
                    }
                }

                if (_sourceBuildInputs.PropertyTypeIsSystemString)
                {
                    PutIndentSpace();
                    Append("if (");
                    Append(_sourceBuildInputs.FieldName);
                    Append(" == value) ");
                }
                else
                {
                    PutIndentSpace();
                    Append("if (object.ReferenceEquals(");
                    Append(_sourceBuildInputs.FieldName);
                    Append(", value)) ");
                }
            }
            else
            {
                PutIndentSpace();
                Append("if (global::System.Collections.Generic.EqualityComparer<");
                Append(_sourceBuildInputs.PropertyType);
                Append(">.Default.Equals(");
                Append(_sourceBuildInputs.FieldName);
                Append(", value)) ");
            }
        }

        void RenderChangingNotificationSection(string changingEventBaseName)
        {
            if (_sourceBuildInputs.ChangingEventAccessibility != GenerateMemberAccessibility.None)
            {
                PutIndentSpace();
                Append(changingEventBaseName);
                AppendLine("Changing?.Invoke(this, global::System.EventArgs.Empty);");
            }

            if (_sourceBuildInputs.EnabledNotifyPropertyChanging)
            {
                var changingEventArgFieldName = $"__PropertyChangingEventArgs_{_sourceBuildInputs.DefaultNotificationPropertyName}";

                PutIndentSpace();
                Append("this.PropertyChanging?.Invoke(this, ");
                Append(changingEventArgFieldName);
                AppendLine(");");
            }
        }

        void RenderChangedNotificationSection(string sourceName, string changedEventBaseName)
        {
            if (_sourceBuildInputs.ChangedEventAccessibility != GenerateMemberAccessibility.None)
            {
                PutIndentSpace();
                Append(sourceName);
                Append(".");
                Append(changedEventBaseName);
                Append("Changed?.Invoke(");
                Append(sourceName);
                AppendLine(", global::System.EventArgs.Empty);");
            }

            if (_sourceBuildInputs.EnabledNotifyPropertyChanged)
            {
                var changedEventArgFieldName = $"__PropertyChangedEventArgs_{_sourceBuildInputs.DefaultNotificationPropertyName}";

                PutIndentSpace();
                Append(sourceName);
                Append(".PropertyChanged?.Invoke(");
                Append(sourceName);
                Append(", ");
                Append(changedEventArgFieldName);
                AppendLine(");");
            }
        }

        void RenderFieldChangeSection()
        {
            PutIndentSpace();
            Append(_sourceBuildInputs.FieldName);
            AppendLine(" = value;");
        }

        static void RenderHasNotChangedBooleanReturnStatement(MethodSourceBuilder builder)
        {
            // false(変更無し)を返却して終了
            builder.AppendLine("return false;");
        }

        static void RenderBooleanReturnAfterChangedPart(MethodSourceBuilder builder, string changedEventBaseName)
        {
            // 通常版は直接変更の通知を実施
            builder.RenderChangedNotificationSection("this", changedEventBaseName);

            // true(変更有り)を返却して終了
            builder.PutIndentSpace();
            builder.AppendLine("return true;");
        }

        static void RenderHasNotChangedDefferedNotificationReturnStatement(MethodSourceBuilder builder)
        {
            // 変更通知の発生しないオブジェクトを返却して終了
            builder.Append("return new ");
            builder.Append(builder._sourceBuildInputs.DefferedNotificationDisposableName);
            builder.AppendLine("(null);");
        }

        static void RenderDefferedNotificationAfterChangedPart(MethodSourceBuilder builder, string changedEventBaseName)
        {
            // Dispose時に変更通知を発生させるオブジェクトを返却して終了
            builder.PutIndentSpace();
            builder.Append("return new ");
            builder.Append(builder._sourceBuildInputs.DefferedNotificationDisposableName);
            builder.AppendLine("(this);");
        }

        void RenderDefaultSetterMethodMainPart(
            string changedEventBaseName,
            string changingEventBaseName,
            MethodSourceBuilderAction renderHasNotChangedReturnStatement,
            MethodSourceBuilderAction<string> renderAfterChangedPart
            )
        {
            AppendLine(")");
            using (BeginBlock())
            {
                RenderDefaultSetterMethodImplCommonPrefixPart();
                renderHasNotChangedReturnStatement(this);

                AppendLine();

                // フィールドの変更
                RenderChangingNotificationSection(changingEventBaseName);
                RenderFieldChangeSection();

                // フィールドの変更後の処理
                renderAfterChangedPart(this, changedEventBaseName);
            }
        }

        void RenderDefaultSetterMethod(string changedEventBaseName, string changingEventBaseName)
        {
            PutIndentSpace(); AppendLine("/// <summary>");
            PutIndentSpace();
            Append("/// ");
            Append(_sourceBuildInputs.DefaultNotificationPropertyName);
            AppendLine("プロパティの標準的な自動実装セッタメソッドです。現在の内部フィールドの値と引数で渡される値が異なる場合に内部フィールドの値を更新し、イベントを発生させ、戻り値としてtrueを返却します。現在の内部フィールドの値と引数で渡される値が同一である場合にイベントは発生せず、戻り値としてfalseを返却します。");
            PutIndentSpace(); AppendLine("/// </summary>");
            PutIndentSpace(); AppendLine("/// <param name=\"value\">プロパティの新しい設定値</param>");
            PutIndentSpace(); AppendLine("/// <return>内部フィールドの値が更新された場合はtrue</return>");
            RenderSetterMethodDeclarationCommonPrefixPart(returnType: "bool", _sourceBuildInputs.MethodName);

            RenderDefaultSetterMethodMainPart(
                changedEventBaseName,
                changingEventBaseName,
                RenderHasNotChangedBooleanReturnStatement, // return false;
                RenderBooleanReturnAfterChangedPart        // Changedイベントの発行、return true;
                );
        }

        void RenderDeferredNotificationSetterMethod(string changedEventBaseName, string changingEventBaseName)
        {
            PutIndentSpace(); AppendLine("/// <summary>");
            PutIndentSpace();
            Append("/// ");
            Append(_sourceBuildInputs.DefaultNotificationPropertyName);
            AppendLine("プロパティの変更後通知をメソッド戻り値のDiposeまで遅延する自動実装セッタメソッドです。現在の内部フィールドの値と引数で渡される値が異なる場合に内部フィールドの値を更新します。変更前の通知イベントはこのメソッド内で発生します。変更後の通知イベントはこのメソッドの戻り値のDiposeで発生します。");
            PutIndentSpace(); AppendLine("/// </summary>");
            PutIndentSpace(); AppendLine("/// <param name=\"value\">プロパティの新しい設定値</param>");
            PutIndentSpace(); AppendLine("/// <return>内部フィールドの値が更新された場合はtrue</return>");
            RenderSetterMethodDeclarationCommonPrefixPart(returnType: _sourceBuildInputs.DefferedNotificationDisposableName, _sourceBuildInputs.DefferedNotificationMethodName);

            RenderDefaultSetterMethodMainPart(
                changedEventBaseName,
                changingEventBaseName,
                RenderHasNotChangedDefferedNotificationReturnStatement, // return new XxxDisposable(null);
                RenderDefferedNotificationAfterChangedPart              // return new XxxDisposable(this);
                );
        }

        void RenderInterlockedExchangeSetterMethod(string changedEventBaseName, string changingEventBaseName)
        {
            bool requiredChangingNotification = false
                || _sourceBuildInputs.ChangingEventAccessibility != GenerateMemberAccessibility.None
                || _sourceBuildInputs.EnabledNotifyPropertyChanging
                ;

            PutIndentSpace(); AppendLine("/// <summary>");
            PutIndentSpace();
            Append("/// ");
            Append(_sourceBuildInputs.DefaultNotificationPropertyName);
            AppendLine("プロパティの内部フィールドを<see cref=\"global::System.Threading.Interloced.Exchange\">によって更新する自動実装セッタメソッドです。ユーザ実装で変更前後の値に対して何だかの処理を付加的に行う必要がある場合などに使用します。現在の内部フィールドの値と引数で渡される値が異なる場合に内部フィールドの値を更新し、イベントを発生させ、outパラメータに変更の値を設定し、戻り値としてtrueを返却します。現在の内部フィールドの値と引数で渡される値が同一である場合にイベントは発生せず、戻り値としてfalseを返却します。");
            PutIndentSpace(); AppendLine("/// </summary>");
            PutIndentSpace(); AppendLine("/// <param name=\"value\">プロパティの新しい設定値</param>");
            PutIndentSpace(); AppendLine("/// <param name=\"prevValue\">プロパティの変更前の設定値</param>");
            PutIndentSpace(); AppendLine("/// <return>内部フィールドの値が更新された場合はtrue</return>");
            RenderSetterMethodDeclarationCommonPrefixPart(returnType: "bool", _sourceBuildInputs.MethodName);
            Append(", ");
            if (_sourceBuildInputs.PropertyTypeIsReferenceType && _sourceBuildInputs.PropertyTypeNullableAnnotation == NullableAnnotation.NotAnnotated)
            {
                Append("[global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] ");
                Append("out ");
                Append(_sourceBuildInputs.PropertyType);
                Append("?");
            }
            else
            {
                Append("out ");
                Append(_sourceBuildInputs.PropertyType);
            }
            AppendLine(" prevValue)");
            using (BeginBlock())
            {
                if (_sourceBuildInputs.PropertyTypeIsReferenceType)
                {
                    if (_sourceBuildInputs.PropertyTypeNullableAnnotation == NullableAnnotation.NotAnnotated)
                    {
                        PutIndentSpace();
                        AppendLine("if (value is null) throw new global::System.ArgumentNullException(nameof(value));");
                    }
                    else if (_sourceBuildInputs.PropertyTypeNullableAnnotation == NullableAnnotation.None)
                    {
                        var descripter = new DiagnosticDescriptor("SGN001", "Nullable context is not enabled.", "Set the Nullable property to enable in the project file or set #nullable enable in the source code.", "code", DiagnosticSeverity.Warning, isEnabledByDefault: true);

                        foreach (var declaration in _sourceBuildInputs.PropertyDeclaringSyntaxReferences)
                        {
                            Context.ReportDiagnostic(Diagnostic.Create(descripter, declaration.GetSyntax(Context.CancellationToken).GetLocation()));
                        }
                    }

                    if (requiredChangingNotification)
                    {
                        // Changing系の通知を行う場合は無駄な通知を避けるために、実際の設定前に同一性の判定を挟む

                        if (_sourceBuildInputs.PropertyTypeIsSystemString)
                        {
                            PutIndentSpace();
                            Append("if (global::System.Threading.Volatile.Read(ref ");
                            Append(_sourceBuildInputs.FieldName);
                            AppendLine("!) == value)");
                        }
                        else
                        {
                            PutIndentSpace();
                            Append("if (object.ReferenceEquals(global::System.Threading.Volatile.Read(ref ");
                            Append(_sourceBuildInputs.FieldName);
                            AppendLine("!), value))");
                        }
                        using (BeginBlock())
                        {
                            PutIndentSpace(); AppendLine("prevValue = default;");
                            PutIndentSpace(); AppendLine("return false;");
                        }
                    }
                }
                else
                {
                    if (requiredChangingNotification)
                    {
                        // Changing系の通知を行う場合は無駄な通知を避けるために、実際の設定前に同一性の判定を挟む

                        PutIndentSpace();
                        Append("if (global::System.Threading.Volatile.Read(ref ");
                        Append(_sourceBuildInputs.FieldName);
                        AppendLine(") == value)");
                        using (BeginBlock())
                        {
                            PutIndentSpace(); AppendLine("prevValue = default;");
                            PutIndentSpace(); AppendLine("return false;");
                        }
                    }
                }

                AppendLine();

                // フィールドの変更と変更前後通知を行う処理
                RenderChangingNotificationSection(changingEventBaseName);

                PutIndentSpace();
                Append("prevValue = global::System.Threading.Interlocked.Exchange(ref ");
                Append(_sourceBuildInputs.FieldName);
                if (_sourceBuildInputs.PropertyTypeIsReferenceType && _sourceBuildInputs.PropertyTypeNullableAnnotation == NullableAnnotation.NotAnnotated)
                {
                    Append("!");
                }
                Append(", value)");
                if (_sourceBuildInputs.PropertyTypeIsReferenceType && _sourceBuildInputs.PropertyTypeNullableAnnotation == NullableAnnotation.NotAnnotated)
                {
                    AppendLine("!;");
                }
                else
                {
                    AppendLine(";");
                }

                // 値の変更が発生しなかった場合はChangedを省略
                // ChangingとChangedはFormClosingとFormClosedの関係を参考にするならばXxxingに対して必ずしもXxxedの通知が発生する必要はない
                if (_sourceBuildInputs.PropertyTypeIsReferenceType)
                {
                    if (_sourceBuildInputs.PropertyTypeIsSystemString)
                    {
                        PutIndentSpace();
                        AppendLine("if (prevValue == value)");
                    }
                    else
                    {
                        PutIndentSpace();
                        AppendLine("if (object.ReferenceEquals(prevValue, value))");
                    }
                    using (BeginBlock())
                    {
                        PutIndentSpace(); AppendLine("prevValue = default;");
                        PutIndentSpace(); AppendLine("return false;");
                    }
                }
                else
                {
                    PutIndentSpace();
                    AppendLine("if (prevValue == value)");
                    using (BeginBlock())
                    {
                        PutIndentSpace(); AppendLine("prevValue = default;");
                        PutIndentSpace(); AppendLine("return false;");
                    }
                }

                RenderChangedNotificationSection("this", changedEventBaseName);

                PutIndentSpace();
                AppendLine("return true;");
            }
        }

        void RenderCustomEqualsSetterMethodMainPart(
            string changedEventBaseName,
            string changingEventBaseName,
            MethodSourceBuilderAction renderHasNotChangedReturnStatement,
            MethodSourceBuilderAction<string> renderAfterChangedPart
            )
        {
            Append(", global::System.Collections.Generic.IEqualityComparer<");
            Append(_sourceBuildInputs.PropertyType);
            AppendLine("> equalityComparer)");
            using (BeginBlock())
            {
                AppendLine("#pragma warning disable CS8604,CS8618,CS8774");
                PutIndentSpace();
                Append("if (equalityComparer.Equals(");
                Append(_sourceBuildInputs.FieldName);
                Append(", value)) ");
                renderHasNotChangedReturnStatement(this);
                AppendLine("#pragma warning restore CS8604,CS8618,CS8774");

                AppendLine();

                // フィールドの変更
                RenderChangingNotificationSection(changingEventBaseName);
                RenderFieldChangeSection();

                // フィールドの変更後の処理
                renderAfterChangedPart(this, changedEventBaseName);
            }
        }

        void RenderCustomEqualsSetterMethod(string changedEventBaseName, string changingEventBaseName)
        {
            PutIndentSpace(); AppendLine("/// <summary>");
            PutIndentSpace();
            Append("/// ");
            Append(_sourceBuildInputs.DefaultNotificationPropertyName);
            AppendLine("プロパティの内部フィールドを指定したEqualityComparerを用いて更新する自動実装セッタメソッドです。現在の内部フィールドの値と引数で渡される値が異なる場合に内部フィールドの値を更新し、イベントを発生させ、戻り値としてtrueを返却します。現在の内部フィールドの値と引数で渡される値が同一である場合にイベントは発生せず、戻り値としてfalseを返却します。");
            PutIndentSpace(); AppendLine("/// </summary>");
            PutIndentSpace(); AppendLine("/// <param name=\"value\">プロパティの新しい設定値</param>");
            PutIndentSpace(); AppendLine("/// <param name=\"equalityComparer\">フィールドの更新要否の判定に用いるIEqualityComparerジェネリックインターフェイスを実装したオブジェクト</param>");
            PutIndentSpace(); AppendLine("/// <return>内部フィールドの値が更新された場合はtrue</return>");
            RenderSetterMethodDeclarationCommonPrefixPart(returnType: "bool", _sourceBuildInputs.MethodName);

            RenderCustomEqualsSetterMethodMainPart(
                changedEventBaseName,
                changingEventBaseName,
                RenderHasNotChangedBooleanReturnStatement, // return false;
                RenderBooleanReturnAfterChangedPart        // Changedイベントの発行、return true;
                );
        }

        void RenderCustomEqualsDeferredNotificationSetterMethod(string changedEventBaseName, string changingEventBaseName)
        {
            PutIndentSpace(); AppendLine("/// <summary>");
            PutIndentSpace();
            Append("/// ");
            Append(_sourceBuildInputs.DefaultNotificationPropertyName);
            AppendLine("プロパティの内部フィールドを指定したEqualityComparerを用いて更新する自動実装セッタメソッドです。現在の内部フィールドの値と引数で渡される値が異なる場合に内部フィールドの値を更新し、イベントを発生させ、戻り値としてtrueを返却します。現在の内部フィールドの値と引数で渡される値が同一である場合にイベントは発生せず、戻り値としてfalseを返却します。");
            PutIndentSpace(); AppendLine("/// </summary>");
            PutIndentSpace(); AppendLine("/// <param name=\"value\">プロパティの新しい設定値</param>");
            PutIndentSpace(); AppendLine("/// <param name=\"equalityComparer\">フィールドの更新要否の判定に用いるIEqualityComparerジェネリックインターフェイスを実装したオブジェクト</param>");
            PutIndentSpace(); AppendLine("/// <return>内部フィールドの値が更新された場合はtrue</return>");
            RenderSetterMethodDeclarationCommonPrefixPart(returnType: _sourceBuildInputs.DefferedNotificationDisposableName, _sourceBuildInputs.DefferedNotificationMethodName);

            RenderCustomEqualsSetterMethodMainPart(
                changedEventBaseName,
                changingEventBaseName,
                RenderHasNotChangedDefferedNotificationReturnStatement, // return new XxxDisposable(null);
                RenderDefferedNotificationAfterChangedPart              // return new XxxDisposable(this);
                );
        }
    }
}
