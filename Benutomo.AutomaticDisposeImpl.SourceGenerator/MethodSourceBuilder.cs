﻿using Microsoft.CodeAnalysis;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator
{
    ref struct MethodSourceBuilder
    {
        public string HintName => $"gen_{string.Join(".", _sourceBuilder.HintingTypeNames)}_{_sourceBuilder.NameSpace}_AutomaticDisposeImpl.cs";

        private bool IsEnabledFinalize => _sourceBuildInputs._userDefinedUnmanagedResourceReleaseMethodName is not null;

        MethodSourceBuilderInputs _sourceBuildInputs;

        ClassSourceBuilder _sourceBuilder;

        public MethodSourceBuilder(SourceProductionContext context, MethodSourceBuilderInputs sourceBuildInputs, Span<char> initialBuffer)
        {
            _sourceBuildInputs = sourceBuildInputs;
            _sourceBuilder = new ClassSourceBuilder(context, sourceBuildInputs.TargetTypeInfo, initialBuffer);

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
        public void BeginBlock(ReadOnlySpan<char> text) => _sourceBuilder.BeginBlock(text);
        public void BeginBlock() => _sourceBuilder.BeginBlock();
        public void EndBlock() => _sourceBuilder.EndBlock();
        public void WriteTypeDeclarationStart(string? classDecralationLineComment) => _sourceBuilder.WriteTypeDeclarationStart(classDecralationLineComment);
        public void WriteTypeDeclarationEnd() => _sourceBuilder.WriteTypeDeclarationEnd();
        #endregion


        public void Build()
        {
            _sourceBuilder.Clear();

            _sourceBuilder.AppendLine("#nullable enable");
            _sourceBuilder.AppendLine("#pragma warning disable CS0612,CS0618,CS0619");

            WriteTypeDeclarationStart("This is implementation class by AutomaticDisposeImpl.");

            WriteBody();

            WriteTypeDeclarationEnd();
        }

        void WriteBody()
        {
            if (!_sourceBuildInputs._isDisposableSubClass)
            {
#if !DEBUG
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Diagnostics.DebuggerBrowsableAttribute(global::System.Diagnostics.DebuggerBrowsableState.Never)]");
#endif
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.Browsable(false)]");
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Obsolete(\"AutomaticDisposeImplによって生成されたフィールドです。一般のコードから参照してはいけません。\")]");
                PutIndentSpace(); _sourceBuilder.AppendLine("private const int __generator_internal_BeNotInitiatedAnyDispose = 0;");

#if !DEBUG
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Diagnostics.DebuggerBrowsableAttribute(global::System.Diagnostics.DebuggerBrowsableState.Never)]");
#endif
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.Browsable(false)]");
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Obsolete(\"AutomaticDisposeImplによって生成されたフィールドです。一般のコードから参照してはいけません。\")]");
                PutIndentSpace(); _sourceBuilder.AppendLine("private const int __generator_internal_InitiatedSyncDispose  = 1;");

#if !DEBUG
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Diagnostics.DebuggerBrowsableAttribute(global::System.Diagnostics.DebuggerBrowsableState.Never)]");
#endif
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.Browsable(false)]");
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Obsolete(\"AutomaticDisposeImplによって生成されたフィールドです。一般のコードから参照してはいけません。\")]");
                PutIndentSpace(); _sourceBuilder.AppendLine("private const int __generator_internal_InitiatedAsyncDispose = 2;");

#if !DEBUG
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Diagnostics.DebuggerBrowsableAttribute(global::System.Diagnostics.DebuggerBrowsableState.Never)]");
#endif
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.Browsable(false)]");
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Obsolete(\"AutomaticDisposeImplによって生成されたフィールドです。一般のコードから参照してはいけません。\")]");
                PutIndentSpace(); _sourceBuilder.AppendLine("private const int __generator_internal_DisposeAlreadyCompleted = 9;");

                PutIndentSpace();

#if !DEBUG
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Diagnostics.DebuggerBrowsableAttribute(global::System.Diagnostics.DebuggerBrowsableState.Never)]");
#endif
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.Browsable(false)]");
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Obsolete(\"AutomaticDisposeImplによって生成されたフィールドです。一般のコードから参照してはいけません。\")]");
                PutIndentSpace(); _sourceBuilder.AppendLine("private int __generator_internal_disposeState = __generator_internal_BeNotInitiatedAnyDispose;");

                // 自分がIDispose.Dispose()を定義する場合に限って自己管理している破棄状態(__generator_internal_disposeState)を利用してIsDisposedプロパティを公開する。
                WriteIsDisposedProperty();
            }

            _sourceBuilder.AppendLine();
#if !DEBUG
            PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Diagnostics.DebuggerBrowsableAttribute(global::System.Diagnostics.DebuggerBrowsableState.Never)]");
#endif
            PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.Browsable(false)]");
            PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
            PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Obsolete(\"AutomaticDisposeImplによって生成されたフィールドです。一般のコードから参照してはいけません。\")]");
            PutIndentSpace(); _sourceBuilder.AppendLine("private int __generator_internal_managedObjectDisposeState = 0;");

            if (IsEnabledFinalize)
            {
                _sourceBuilder.AppendLine();
#if !DEBUG
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Diagnostics.DebuggerBrowsableAttribute(global::System.Diagnostics.DebuggerBrowsableState.Never)]");
#endif
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.Browsable(false)]");
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Obsolete(\"AutomaticDisposeImplによって生成されたフィールドです。一般のコードから参照してはいけません。\")]");
                PutIndentSpace(); _sourceBuilder.AppendLine("private int __generator_internal_unmanagedResourceReleaseState = 0;");

                WriteFinalizer();
            }

            if (_sourceBuildInputs._isIDisposableIntafeceImplementer || IsEnabledFinalize)
            {
                WriteDisposeCoreMethod();
            }

            if (_sourceBuildInputs._isIDisposableIntafeceImplementer)
            {
                if (!_sourceBuildInputs._isDisposableSubClass)
                {
                    WriteIDisposableDisposeMethod();
                }

            }

            if (_sourceBuildInputs._isIAsyncDisposableIntafeceImplementer)
            {
                if (!_sourceBuildInputs._isAsyncDisposableSubClass)
                {
                    WriteIAsyncDisposableDisposeAsyncMethod();
                }

                WriteDisposeAsyncCoreMethod();
            }
            else
            {
                // AnalyzerでIAsyncDisposableインターフェイスを実装している型のフィールドとプロパティに対するSG0003の報告を実装
            }

            return;


        }
        void WriteIsDisposedProperty()
        {
            AppendLine();
            PutIndentSpace(); AppendLine("public bool IsDisposed => (global::System.Threading.Thread.VolatileRead(ref __generator_internal_disposeState) != __generator_internal_BeNotInitiatedAnyDispose);");
        }

        void WriteFinalizer()
        {
            AppendLine();

            PutIndentSpace();
            Append("~");
            Append(_sourceBuildInputs.TargetTypeInfo.Name);
            AppendLine("()");
            BeginBlock();
            {
                PutIndentSpace(); AppendLine("// Release unmaneged resources.");
                PutIndentSpace(); AppendLine("Dispose(disposing: false);");
            }
            EndBlock();
        }

        void WriteIDisposableDisposeMethod()
        {
            AppendLine();

            BeginBlock("public void Dispose()");
            {
                PutIndentSpace(); AppendLine("var dispose_state = global::System.Threading.Interlocked.CompareExchange(ref __generator_internal_disposeState, __generator_internal_InitiatedSyncDispose, __generator_internal_BeNotInitiatedAnyDispose);");
                PutIndentSpace(); AppendLine("if (dispose_state == __generator_internal_BeNotInitiatedAnyDispose)");
                BeginBlock();
                {
                    if (_sourceBuildInputs._isIDisposableIntafeceImplementer || IsEnabledFinalize)
                    {
                        AppendLine();
                        PutIndentSpace(); AppendLine("// Dispose managed members and release unmaneged resources.");
                        PutIndentSpace(); AppendLine("Dispose(disposing: true);");
                    }

                    if (IsEnabledFinalize)
                    {
                        AppendLine();
                        PutIndentSpace(); AppendLine("global::System.GC.SuppressFinalize(this);");
                    }

                    AppendLine();
                    PutIndentSpace(); AppendLine("global::System.Threading.Thread.VolatileWrite(ref __generator_internal_disposeState, __generator_internal_DisposeAlreadyCompleted);");
                }
                EndBlock();
            }
            EndBlock();
        }

        void WriteDisposeCoreMethod()
        {
            AppendLine();

            if (_sourceBuildInputs._isInheritalbeClass)
            {
                if (_sourceBuildInputs._isDisposableSubClass)
                {
                    PutIndentSpace(); AppendLine("protected override void Dispose(bool disposing)");
                }
                else
                {
                    PutIndentSpace(); AppendLine("protected virtual void Dispose(bool disposing)");
                }
            }
            else
            {
                PutIndentSpace(); AppendLine("[global::System.Obsolete(\"AutomaticDisposeImplによって生成されたメソッドです。一般のコードから参照してはいけません。\")]");
                PutIndentSpace(); AppendLine("private void Dispose(bool disposing)");
            }
            BeginBlock();
            {
                PutIndentSpace(); AppendLine("if (disposing)");
                BeginBlock();
                {
                    PutIndentSpace(); AppendLine("var managedObjectDisposeState = global::System.Threading.Interlocked.Exchange(ref __generator_internal_managedObjectDisposeState, 1);");
                    PutIndentSpace(); AppendLine("if (managedObjectDisposeState == 0)");
                    BeginBlock();
                    {
                        foreach (var memberName in _sourceBuildInputs.SyncDisposeMembersInDisposeMethod)
                        {
                            BeginTryBlock();
                            {
                                PutIndentSpace();
                                Append("(this.");
                                Append(memberName);
                                AppendLine(" as global::System.IDisposable)?.Dispose();");
                            }
                            EndBlock();
                            BeginBlock("catch (global::System.Exception ex)");
                            {
                                PutIndentSpace();
                                Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the ");
                                Append(memberName);
                                AppendLine(@".Dispose() calling. Message=\""{ex.Message}\"""");");
                            }
                            EndBlock();
                        }


                        if (_sourceBuildInputs._userDefinedManagedObjectDisposeMethodName is not null)
                        {
                            BeginTryBlock();
                            {
                                AppendLine("");

                                PutIndentSpace(); AppendLine("// Invoke user implemented disposer.");
                                PutIndentSpace();
                                Append("this.");
                                Append(_sourceBuildInputs._userDefinedManagedObjectDisposeMethodName);
                                AppendLine("();");
                            }
                            EndBlock();
                            BeginBlock("catch (global::System.Exception ex)");
                            {
                                PutIndentSpace();
                                Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the this.");
                                Append(_sourceBuildInputs._userDefinedManagedObjectDisposeMethodName);
                                AppendLine(@"() calling. Message=\""{ex.Message}\"""");");
                            }
                            EndBlock();
                        }
                    }
                    EndBlock();
                }
                EndBlock();

                if (_sourceBuildInputs._userDefinedUnmanagedResourceReleaseMethodName is not null)
                {
                    PutIndentSpace(); AppendLine("var unmanagedResourceReleaseState = global::System.Threading.Interlocked.Exchange(ref __generator_internal_unmanagedResourceReleaseState, 1);");
                    PutIndentSpace(); AppendLine("if (unmanagedResourceReleaseState == 0)");
                    BeginBlock();
                    {
                        BeginTryBlock();
                        {
                            PutIndentSpace(); AppendLine("// Invoke user implemented finalizer.");
                            PutIndentSpace();
                            Append("this.");
                            Append(_sourceBuildInputs._userDefinedUnmanagedResourceReleaseMethodName);
                            AppendLine("();");
                        }
                        EndBlock();
                        BeginBlock("catch (global::System.Exception ex)");
                        {
                            PutIndentSpace();
                            Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the this.");
                            Append(_sourceBuildInputs._userDefinedUnmanagedResourceReleaseMethodName);
                            AppendLine(@"() calling. Message=\""{ex.Message}\"""");");
                        }
                        EndBlock();
                    }
                    EndBlock();
                }

                if (_sourceBuildInputs._isDisposableSubClass)
                {
                    BeginTryBlock();
                    {
                        PutIndentSpace(); AppendLine("base.Dispose(disposing);");
                    }
                    EndBlock();
                    BeginBlock("catch (global::System.Exception ex)");
                    {
                        PutIndentSpace(); AppendLine(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the base.Dispose(bool) calling. Message=\""{ex.Message}\"""");");
                    }
                    EndBlock();
                }
            }
            EndBlock();
        }



        void WriteIAsyncDisposableDisposeAsyncMethod()
        {
            AppendLine();

            AppendLine("#pragma warning disable CS1998");
            BeginBlock("public async global::System.Threading.Tasks.ValueTask DisposeAsync()");
            {
                PutIndentSpace(); AppendLine("var dispose_state = global::System.Threading.Interlocked.CompareExchange(ref __generator_internal_disposeState, __generator_internal_InitiatedAsyncDispose, __generator_internal_BeNotInitiatedAnyDispose);");
                PutIndentSpace(); AppendLine("if (dispose_state == __generator_internal_BeNotInitiatedAnyDispose)");
                BeginBlock();
                {
                    PutIndentSpace(); AppendLine("await DisposeAsyncCore().ConfigureAwait(false);");

                    if (_sourceBuildInputs._isIDisposableIntafeceImplementer || IsEnabledFinalize)
                    {
                        AppendLine();
                        PutIndentSpace(); AppendLine("// Release unmaneged resources.");
                        PutIndentSpace(); AppendLine("Dispose(disposing: false);");
                    }

                    if (IsEnabledFinalize)
                    {
                        AppendLine();
                        AppendLine("#pragma warning disable CA1816");
                        PutIndentSpace(); AppendLine("global::System.GC.SuppressFinalize(this);");
                        AppendLine("#pragma warning restore CA1816");
                    }

                    AppendLine();
                    PutIndentSpace(); AppendLine("global::System.Threading.Thread.VolatileWrite(ref __generator_internal_disposeState, __generator_internal_DisposeAlreadyCompleted);");
                }
                EndBlock();
            }
            EndBlock();
            AppendLine("#pragma warning restore CS1998");
        }

        void WriteDisposeAsyncCoreMethod()
        {
            AppendLine();

            AppendLine("#pragma warning disable CS1998");
            PutIndentSpace();
            if (_sourceBuildInputs._isInheritalbeClass)
            {
                if (_sourceBuildInputs._isAsyncDisposableSubClass)
                {
                    AppendLine("protected override async global::System.Threading.Tasks.ValueTask DisposeAsyncCore()");
                }
                else
                {
                    AppendLine("protected virtual async global::System.Threading.Tasks.ValueTask DisposeAsyncCore()");
                }
            }
            else
            {
                AppendLine("private async global::System.Threading.Tasks.ValueTask DisposeAsyncCore()");
            }
            BeginBlock();
            {
                PutIndentSpace(); AppendLine("var managedObjectDisposeState = global::System.Threading.Interlocked.Exchange(ref __generator_internal_managedObjectDisposeState, 1);");
                PutIndentSpace(); AppendLine("if (managedObjectDisposeState == 0)");
                BeginBlock();
                {
                    foreach (var memberName in _sourceBuildInputs.AsyncDisposeMembersInAsyncDisposeMethod)
                    {
                        BeginTryBlock();
                        {
                            PutIndentSpace();
                            Append("if (this.");
                            Append(memberName);
                            Append(" is global::System.IAsyncDisposable asyncDisposable_");
                            Append(memberName);
                            AppendLine(")");
                            BeginBlock();
                            {
                                PutIndentSpace();
                                Append("await asyncDisposable_");
                                Append(memberName);
                                AppendLine(".DisposeAsync().ConfigureAwait(false);");
                            }
                            EndBlock();
                        }
                        EndBlock();
                        BeginBlock("catch (global::System.Exception ex)");
                        {
                            PutIndentSpace();
                            Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the ");
                            Append(memberName);
                            AppendLine(@".Dispose() calling. Message=\""{ex.Message}\"""");");
                        }
                        EndBlock();
                    }

                    foreach (var memberName in _sourceBuildInputs.SyncDisposeMembersInAsyncDisposeMethod)
                    {
                        BeginTryBlock();
                        {
                            PutIndentSpace();
                            Append("(this.");
                            Append(memberName);
                            AppendLine(" as global::System.IDisposable)?.Dispose();");
                        }
                        EndBlock();
                        BeginBlock("catch (global::System.Exception ex)");
                        {
                            PutIndentSpace();
                            Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the ");
                            Append(memberName);
                            AppendLine(@".Dispose() calling. Message=\""{ex.Message}\"""");");
                        }
                        EndBlock();
                    }

                    if (_sourceBuildInputs._userDefinedManagedObjectAsyncDisposeMethodName is not null)
                    {
                        BeginTryBlock();
                        {
                            AppendLine("");

                            PutIndentSpace(); AppendLine("// Invoke user implemented disposer.");
                            PutIndentSpace();
                            Append("await this.");
                            Append(_sourceBuildInputs._userDefinedManagedObjectAsyncDisposeMethodName);
                            AppendLine("().ConfigureAwait(false);");
                        }
                        EndBlock();
                        BeginBlock("catch (global::System.Exception ex)");
                        {
                            PutIndentSpace();
                            Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the this.");
                            Append(_sourceBuildInputs._userDefinedManagedObjectAsyncDisposeMethodName);
                            AppendLine(@"() calling. Message=\""{ex.Message}\"""");");
                        }
                        EndBlock();
                    }
                    else if (_sourceBuildInputs._userDefinedManagedObjectDisposeMethodName is not null)
                    {
                        BeginTryBlock();
                        {
                            AppendLine("");

                            PutIndentSpace(); AppendLine("// Invoke user implemented disposer.");
                            PutIndentSpace();
                            Append("this.");
                            Append(_sourceBuildInputs._userDefinedManagedObjectDisposeMethodName);
                            AppendLine("();");
                        }
                        EndBlock();
                        BeginBlock("catch (global::System.Exception ex)");
                        {
                            PutIndentSpace();
                            Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the this.");
                            Append(_sourceBuildInputs._userDefinedManagedObjectDisposeMethodName);
                            AppendLine(@"() calling. Message=\""{ex.Message}\"""");");
                        }
                        EndBlock();
                    }
                }
                EndBlock();

                AppendLine();

                if (_sourceBuildInputs._isAsyncDisposableSubClass)
                {
                    BeginTryBlock();
                    {
                        PutIndentSpace(); AppendLine("await base.DisposeAsyncCore().ConfigureAwait(false);");
                    }
                    EndBlock();
                    BeginBlock("catch (global::System.Exception ex)");
                    {
                        PutIndentSpace(); AppendLine(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the base.DisposeAsyncCore() calling. Message=\""{ex.Message}\"""");");
                    }
                    EndBlock();
                }
            }
            EndBlock();
            AppendLine("#pragma warning restore CS1998");
        }
    }
}
