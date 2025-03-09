﻿using Microsoft.CodeAnalysis;
using static SourceGeneratorCommons.SourceBuilder;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator;

class MethodSourceBuilder : IDisposable
{
    private bool IsEnabledFinalize => _sourceBuildInputs._userDefinedUnmanagedResourceReleaseMethodName is not null;

    private MethodSourceBuilderInputs _sourceBuildInputs;

    private SourceBuilder _sourceBuilder;

    public MethodSourceBuilder(SourceProductionContext context, MethodSourceBuilderInputs sourceBuildInputs)
    {
        _sourceBuildInputs = sourceBuildInputs;
        _sourceBuilder = new SourceBuilder(context, sourceBuildInputs.TargetType.MakeStandardHintName());
    }

    public void Dispose()
    {
        _sourceBuilder.Dispose();
    }

    #region _sourceBuilder Methods
    public void PutIndentSpace() => _sourceBuilder.PutIndentSpace();
    public void Append(string text) => _sourceBuilder.Append(text);
    public void Append(ReadOnlySpan<char> text) => _sourceBuilder.Append(text);
    public void AppendLine(string text) => _sourceBuilder.AppendLine(text);
    public void AppendLine(ReadOnlySpan<char> text) => _sourceBuilder.AppendLine(text);
    public void AppendLine() => _sourceBuilder.AppendLine();
    public _BlockEndDisposable BeginBlock(string blockHeadLine) => _sourceBuilder.BeginBlock(blockHeadLine);
    public _BlockEndDisposable BeginBlock(ReadOnlySpan<char> text) => _sourceBuilder.BeginBlock(text);
    public _BlockEndDisposable BeginBlock() => _sourceBuilder.BeginBlock();
    public _BlockEndDisposable BeginTypeDeclaration(string? classDecralationLineComment) => _sourceBuilder.BeginTypeDefinitionBlock(_sourceBuildInputs.TargetType, classDecralationLineComment);
    #endregion

    public void Commit()
    {
        _sourceBuilder.Commit();
    }

    public void Build()
    {
        _sourceBuilder.Clear();

#if !DEBUG
        _sourceBuilder.AppendLine("// <auto-generated />");
#endif
        _sourceBuilder.AppendLine("#nullable enable");
        _sourceBuilder.AppendLine("#pragma warning disable CS0612,CS0618,CS0619");
        _sourceBuilder.AppendLine("#pragma warning disable CS0436");

        using (BeginTypeDeclaration(" // This is implementation class by AutomaticDisposeImpl."))
        {
            WriteBody();
        }
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
        PutIndentSpace(); AppendLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
        PutIndentSpace(); AppendLine("public bool IsDisposed => (global::System.Threading.Thread.VolatileRead(ref __generator_internal_disposeState) != __generator_internal_BeNotInitiatedAnyDispose);");
    }

    void WriteFinalizer()
    {
        AppendLine();

        PutIndentSpace(); AppendLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
        PutIndentSpace();
        Append("~");
        Append(_sourceBuildInputs.TargetType.Name);
        AppendLine("()");
        using (BeginBlock())
        {
            PutIndentSpace(); AppendLine("// Release unmaneged resources.");
            PutIndentSpace(); AppendLine("Dispose(disposing: false);");
        }
    }

    void WriteIDisposableDisposeMethod()
    {
        AppendLine();

        PutIndentSpace(); AppendLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
        using (BeginBlock("public void Dispose()"))
        {
            PutIndentSpace(); AppendLine("var dispose_state = global::System.Threading.Interlocked.CompareExchange(ref __generator_internal_disposeState, __generator_internal_InitiatedSyncDispose, __generator_internal_BeNotInitiatedAnyDispose);");
            PutIndentSpace(); AppendLine("if (dispose_state == __generator_internal_BeNotInitiatedAnyDispose)");
            using (BeginBlock())
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
        }
    }

    void WriteDisposeCoreMethod()
    {
        AppendLine();

        PutIndentSpace(); AppendLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
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
        using (BeginBlock())
        {
            PutIndentSpace(); AppendLine("if (disposing)");
            using (BeginBlock())
            {
                PutIndentSpace(); AppendLine("var managedObjectDisposeState = global::System.Threading.Interlocked.Exchange(ref __generator_internal_managedObjectDisposeState, 1);");
                PutIndentSpace(); AppendLine("if (managedObjectDisposeState == 0)");
                using (BeginBlock())
                {
                    foreach (var memberName in _sourceBuildInputs.SyncDisposeMembersInDisposeMethod)
                    {
                        using (BeginBlock("try"))
                        {
                            PutIndentSpace();
                            Append("(this.");
                            Append(memberName);
                            AppendLine(" as global::System.IDisposable)?.Dispose();");
                        }
                        using (BeginBlock("catch (global::System.Exception ex)"))
                        {
                            PutIndentSpace();
                            Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the ");
                            Append(memberName);
                            AppendLine(@".Dispose() calling. Message=\""{ex.Message}\"""");");
                        }
                    }


                    if (_sourceBuildInputs._userDefinedManagedObjectDisposeMethodName is not null)
                    {
                        using (BeginBlock("try"))
                        {
                            AppendLine("");

                            PutIndentSpace(); AppendLine("// Invoke user implemented disposer.");
                            PutIndentSpace();
                            Append("this.");
                            Append(_sourceBuildInputs._userDefinedManagedObjectDisposeMethodName);
                            AppendLine("();");
                        }
                        using (BeginBlock("catch (global::System.Exception ex)"))
                        {
                            PutIndentSpace();
                            Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the this.");
                            Append(_sourceBuildInputs._userDefinedManagedObjectDisposeMethodName);
                            AppendLine(@"() calling. Message=\""{ex.Message}\"""");");
                        }
                    }
                }
            }

            if (_sourceBuildInputs._userDefinedUnmanagedResourceReleaseMethodName is not null)
            {
                PutIndentSpace(); AppendLine("var unmanagedResourceReleaseState = global::System.Threading.Interlocked.Exchange(ref __generator_internal_unmanagedResourceReleaseState, 1);");
                PutIndentSpace(); AppendLine("if (unmanagedResourceReleaseState == 0)");
                using (BeginBlock())
                {
                    using (BeginBlock("try"))
                    {
                        PutIndentSpace(); AppendLine("// Invoke user implemented finalizer.");
                        PutIndentSpace();
                        Append("this.");
                        Append(_sourceBuildInputs._userDefinedUnmanagedResourceReleaseMethodName);
                        AppendLine("();");
                    }
                    using (BeginBlock("catch (global::System.Exception ex)"))
                    {
                        PutIndentSpace();
                        Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the this.");
                        Append(_sourceBuildInputs._userDefinedUnmanagedResourceReleaseMethodName);
                        AppendLine(@"() calling. Message=\""{ex.Message}\"""");");
                    }
                }
            }

            if (_sourceBuildInputs._isDisposableSubClass)
            {
                using (BeginBlock("try"))
                {
                    PutIndentSpace(); AppendLine("base.Dispose(disposing);");
                }
                using (BeginBlock("catch (global::System.Exception ex)"))
                {
                    PutIndentSpace(); AppendLine(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the base.Dispose(bool) calling. Message=\""{ex.Message}\"""");");
                }
            }
        }
    }



    void WriteIAsyncDisposableDisposeAsyncMethod()
    {
        AppendLine();

        PutIndentSpace(); AppendLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
        AppendLine("#pragma warning disable CS1998");
        using (BeginBlock("public async global::System.Threading.Tasks.ValueTask DisposeAsync()"))
        {
            PutIndentSpace(); AppendLine("var dispose_state = global::System.Threading.Interlocked.CompareExchange(ref __generator_internal_disposeState, __generator_internal_InitiatedAsyncDispose, __generator_internal_BeNotInitiatedAnyDispose);");
            PutIndentSpace(); AppendLine("if (dispose_state == __generator_internal_BeNotInitiatedAnyDispose)");
            using (BeginBlock())
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
        }
        AppendLine("#pragma warning restore CS1998");
    }

    void WriteDisposeAsyncCoreMethod()
    {
        AppendLine();

        PutIndentSpace(); AppendLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
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
        using (BeginBlock())
        {
            PutIndentSpace(); AppendLine("var managedObjectDisposeState = global::System.Threading.Interlocked.Exchange(ref __generator_internal_managedObjectDisposeState, 1);");
            PutIndentSpace(); AppendLine("if (managedObjectDisposeState == 0)");
            using (BeginBlock())
            {
                foreach (var memberName in _sourceBuildInputs.AsyncDisposeMembersInAsyncDisposeMethod)
                {
                    using (BeginBlock("try"))
                    {
                        PutIndentSpace();
                        Append("if (this.");
                        Append(memberName);
                        Append(" is global::System.IAsyncDisposable asyncDisposable_");
                        Append(memberName);
                        AppendLine(")");
                        using (BeginBlock())
                        {
                            PutIndentSpace();
                            Append("await asyncDisposable_");
                            Append(memberName);
                            AppendLine(".DisposeAsync().ConfigureAwait(false);");
                        }
                    }
                    using (BeginBlock("catch (global::System.Exception ex)"))
                    {
                        PutIndentSpace();
                        Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the ");
                        Append(memberName);
                        AppendLine(@".Dispose() calling. Message=\""{ex.Message}\"""");");
                    }
                }

                foreach (var memberName in _sourceBuildInputs.SyncDisposeMembersInAsyncDisposeMethod)
                {
                    using (BeginBlock("try"))
                    {
                        PutIndentSpace();
                        Append("(this.");
                        Append(memberName);
                        AppendLine(" as global::System.IDisposable)?.Dispose();");
                    }
                    using (BeginBlock("catch (global::System.Exception ex)"))
                    {
                        PutIndentSpace();
                        Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the ");
                        Append(memberName);
                        AppendLine(@".Dispose() calling. Message=\""{ex.Message}\"""");");
                    }
                }

                if (_sourceBuildInputs._userDefinedManagedObjectAsyncDisposeMethodName is not null)
                {
                    using (BeginBlock("try"))
                    {
                        AppendLine("");

                        PutIndentSpace(); AppendLine("// Invoke user implemented disposer.");
                        PutIndentSpace();
                        Append("await this.");
                        Append(_sourceBuildInputs._userDefinedManagedObjectAsyncDisposeMethodName);
                        AppendLine("().ConfigureAwait(false);");
                    }
                    using (BeginBlock("catch (global::System.Exception ex)"))
                    {
                        PutIndentSpace();
                        Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the this.");
                        Append(_sourceBuildInputs._userDefinedManagedObjectAsyncDisposeMethodName);
                        AppendLine(@"() calling. Message=\""{ex.Message}\"""");");
                    }
                }
                else if (_sourceBuildInputs._userDefinedManagedObjectDisposeMethodName is not null)
                {
                    using (BeginBlock("try"))
                    {
                        AppendLine("");

                        PutIndentSpace(); AppendLine("// Invoke user implemented disposer.");
                        PutIndentSpace();
                        Append("this.");
                        Append(_sourceBuildInputs._userDefinedManagedObjectDisposeMethodName);
                        AppendLine("();");
                    }
                    using (BeginBlock("catch (global::System.Exception ex)"))
                    {
                        PutIndentSpace();
                        Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the this.");
                        Append(_sourceBuildInputs._userDefinedManagedObjectDisposeMethodName);
                        AppendLine(@"() calling. Message=\""{ex.Message}\"""");");
                    }
                }
            }

            AppendLine();

            if (_sourceBuildInputs._isAsyncDisposableSubClass)
            {
                using (BeginBlock("try"))
                {
                    PutIndentSpace(); AppendLine("await base.DisposeAsyncCore().ConfigureAwait(false);");
                }
                using (BeginBlock("catch (global::System.Exception ex)"))
                {
                    PutIndentSpace(); AppendLine(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the base.DisposeAsyncCore() calling. Message=\""{ex.Message}\"""");");
                }
            }
        }
        AppendLine("#pragma warning restore CS1998");
    }
}
