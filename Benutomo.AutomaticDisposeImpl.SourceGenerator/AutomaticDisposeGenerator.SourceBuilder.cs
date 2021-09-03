using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator
{
    public partial class AutomaticDisposeGenerator
    {
        class SourceBuilder
        {
            public string HintName => $"gen.{string.Join(".", _names)}.AutomaticDisposeImpl.cs";

            public string SourceText => _sourceBuilder.ToString();

            private bool IsEnabledFinalize => _userDefinedUnmanagedResourceReleaseMethod is not null;

            readonly INamedTypeSymbol _classDeclarationSymbol;

            readonly AutomaticDisposeContextChecker _automaticDisposeContextChecker;

            readonly List<IFieldSymbol> _disposableFields;

            readonly List<IPropertySymbol> _disposableProperties;

            readonly List<IFieldSymbol> _nonAsyncDisposableFields;

            readonly List<IPropertySymbol> _nonAsyncDisposableProperties;

            readonly List<IFieldSymbol> _asyncDisposableFields;

            readonly List<IPropertySymbol> _asyncDisposableProperties;

            readonly IMethodSymbol? _userDefinedUnmanagedResourceReleaseMethod;

            readonly IMethodSymbol? _userDefinedManagedObjectDisposeMethod;

            readonly IMethodSymbol? _userDefinedManagedObjectAsyncDisposeMethod;

            readonly List<string> _names = new List<string>();

            readonly StringBuilder _sourceBuilder = new StringBuilder(4000);

            readonly bool _isIDisposableIntafeceImplementer;

            readonly bool _isIAsyncDisposableIntafeceImplementer;

            readonly bool _isDisposableSubClass;

            readonly bool _isAsyncDisposableSubClass;

            readonly bool _isInheritalbeClass;

            int _currentIndentCount = 0;

            const string indentText = "    ";

            public SourceBuilder(GeneratorExecutionContext context, INamedTypeSymbol classDeclarationSymbol, AttributeData automaticDisposeAttributeData)
            {
                _classDeclarationSymbol = classDeclarationSymbol;
                _automaticDisposeContextChecker = new AutomaticDisposeContextChecker(automaticDisposeAttributeData);

                _isIDisposableIntafeceImplementer = IsAssignableToIDisposable(_classDeclarationSymbol);

                _isIAsyncDisposableIntafeceImplementer = IsAssignableToIAsyncDisposable(_classDeclarationSymbol);

                _isDisposableSubClass = IsAssignableToIDisposable(_classDeclarationSymbol.BaseType);

                _isAsyncDisposableSubClass = IsAssignableToIAsyncDisposable(_classDeclarationSymbol.BaseType);

                _isInheritalbeClass = !_classDeclarationSymbol.IsValueType && !_classDeclarationSymbol.IsSealed;

                _disposableFields = new();
                _disposableProperties = new();

                _nonAsyncDisposableFields = new();
                _nonAsyncDisposableProperties = new();

                _asyncDisposableFields = new();
                _asyncDisposableProperties = new();

                foreach (var member in classDeclarationSymbol.GetMembers())
                {
                    if (member.IsStatic) continue;

                    if (member is IMethodSymbol methodSymbol)
                    {
                        foreach (var attribute in member.GetAttributes())
                        {
                            if (_userDefinedUnmanagedResourceReleaseMethod is null && IsUnmanagedResourceReleaseMethodAttribute(attribute.AttributeClass))
                            {
                                _userDefinedUnmanagedResourceReleaseMethod = methodSymbol;
                                break;
                            }
                            if (_userDefinedManagedObjectDisposeMethod is null && IsManagedObjectDisposeMethodAttribute(attribute.AttributeClass))
                            {
                                _userDefinedManagedObjectDisposeMethod = methodSymbol;
                                break;
                            }
                            if (_userDefinedManagedObjectAsyncDisposeMethod is null && IsManagedObjectAsyncDisposeMethodAttribute(attribute.AttributeClass))
                            {
                                _userDefinedManagedObjectAsyncDisposeMethod = methodSymbol;
                                break;
                            }
                        }
                    }
                    else if (member is IFieldSymbol fieldSymbol)
                    {
                        if (fieldSymbol.IsImplicitlyDeclared) continue;

                        var isAssignableToIDisposable = IsAssignableToIDisposable(fieldSymbol.Type);
                        var isAssignableToIAsyncDisposable = IsAssignableToIAsyncDisposable(fieldSymbol.Type);

                        if (isAssignableToIDisposable)
                        {
                            // Dispose()が呼び出し可能
                            _disposableFields.Add(fieldSymbol);

                            if (!isAssignableToIAsyncDisposable)
                            {
                                // Dispose()のみが呼び出し可能
                                _nonAsyncDisposableFields.Add(fieldSymbol);
                            }
                        }

                        if (isAssignableToIAsyncDisposable)
                        {
                            // DisposeAsync()が呼び出し可能
                            _asyncDisposableFields.Add(fieldSymbol);
                        }
                    }
                    else if (member is IPropertySymbol propertySymbol)
                    {
                        if (propertySymbol.IsImplicitlyDeclared) continue;

                        var isAssignableToIDisposable = IsAssignableToIDisposable(propertySymbol.Type);
                        var isAssignableToIAsyncDisposable = IsAssignableToIAsyncDisposable(propertySymbol.Type);

                        if (isAssignableToIDisposable)
                        {
                            // Dispose()が呼び出し可能
                            _disposableProperties.Add(propertySymbol);

                            if (!isAssignableToIAsyncDisposable)
                            {
                                // Dispose()のみが呼び出し可能
                                _nonAsyncDisposableProperties.Add(propertySymbol);
                            }
                        }

                        if (isAssignableToIAsyncDisposable)
                        {
                            // DisposeAsync()が呼び出し可能
                            _asyncDisposableProperties.Add(propertySymbol);
                        }
                    }
                }
            }

            public void Build()
            {
                _names.Clear();
                _sourceBuilder.Clear();

                _sourceBuilder.AppendLine("#nullable enable");

                WriteTypeDeclarationStart();

                WriteBody();

                WriteTypeDeclarationEnd();

#if DEBUG
                var code = _sourceBuilder.ToString();
                ;
#endif
            }

            void PutIndentSpace()
            {
                for (int i = 0; i < _currentIndentCount; i++)
                {
                    _sourceBuilder.Append(indentText);
                }
            }

            void BeginTryBlock()
            {
                BeginBlock("try");
            }

            void BeginFinallyBlock()
            {
                BeginBlock("finally");
            }

            void BeginBlock(string blockHeadLine)
            {
                PutIndentSpace(); _sourceBuilder.AppendLine(blockHeadLine);
                BeginBlock();
            }

            void BeginBlock()
            {
                PutIndentSpace(); _sourceBuilder.AppendLine("{");
                _currentIndentCount++;
            }

            void EndBlock()
            {
                _currentIndentCount--;
                PutIndentSpace(); _sourceBuilder.AppendLine("}");
            }

            void WriteTypeDeclarationStart()
            {
                WriteContainingTypeStart(_classDeclarationSymbol, isDesingationType: true);

                return;

                void WriteContainingTypeStart(INamedTypeSymbol namedTypeSymbol, bool isDesingationType)
                {
                    if (namedTypeSymbol.ContainingType is not null)
                    {
                        WriteContainingTypeStart(namedTypeSymbol.ContainingType, false);
                    }
                    else if (namedTypeSymbol.ContainingNamespace is not null)
                    {
                        WriteContainingNameSpaceStart(namedTypeSymbol.ContainingNamespace);
                    }

                    PutIndentSpace();
                    _sourceBuilder.Append("partial ");
                    _sourceBuilder.Append(namedTypeSymbol.IsValueType ? "struct " : "class ");
                    _sourceBuilder.Append(namedTypeSymbol.Name);
                    if (namedTypeSymbol.IsGenericType)
                    {
                        _sourceBuilder.Append("<");
                        _sourceBuilder.Append(string.Join(", ", namedTypeSymbol.TypeArguments.Select(v => v.Name)));
                        _sourceBuilder.Append(">");
                    }
                    if (isDesingationType)
                    {
                        // なくてもいいが生成されたコードだけを見ても実装対象となっているインターフェイスが分かるようにしておく

                        if (_isIDisposableIntafeceImplementer && _isIAsyncDisposableIntafeceImplementer)
                        {
                            _sourceBuilder.Append(" : global::System.IDisposable, global::System.IAsyncDisposable");
                        }
                        else if (_isIDisposableIntafeceImplementer)
                        {
                            _sourceBuilder.Append(" : global::System.IDisposable");
                        }
                        else if (_isIAsyncDisposableIntafeceImplementer)
                        {
                            _sourceBuilder.Append(" : global::System.IAsyncDisposable");
                        }
                    }
                    _sourceBuilder.AppendLine("");

                    BeginBlock();


                    _names.Add(namedTypeSymbol.Name);
                }

                void WriteContainingNameSpaceStart(INamespaceSymbol namespaceSymbol)
                {
                    if (namespaceSymbol.IsGlobalNamespace) return;

                    WriteContainingNameSpaceStart(namespaceSymbol.ContainingNamespace);

                    PutIndentSpace();
                    _sourceBuilder.Append("namespace ");
                    _sourceBuilder.Append(namespaceSymbol.Name);
                    _sourceBuilder.AppendLine("");

                    BeginBlock();


                    _names.Add(namespaceSymbol.Name);
                }
            }

            void WriteTypeDeclarationEnd()
            {
                WriteContainingTypeEnd(_classDeclarationSymbol);

                return;

                void WriteContainingTypeEnd(INamedTypeSymbol namedTypeSymbol)
                {
                    EndBlock();

                    if (namedTypeSymbol.ContainingType is not null)
                    {
                        WriteContainingTypeEnd(namedTypeSymbol.ContainingType);
                    }
                    else if (namedTypeSymbol.ContainingNamespace is not null)
                    {
                        WriteContainingNameSpaceEnd(namedTypeSymbol.ContainingNamespace);
                    }
                }

                void WriteContainingNameSpaceEnd(INamespaceSymbol namespaceSymbol)
                {
                    if (namespaceSymbol.IsGlobalNamespace) return;

                    WriteContainingNameSpaceEnd(namespaceSymbol.ContainingNamespace);

                    EndBlock();
                }
            }

            void WriteBody()
            {
                if (!_isDisposableSubClass)
                {
                    PutIndentSpace(); _sourceBuilder.AppendLine("private const int __generator_internal_BeNotInitiatedAnyDispose = 0;");
                    PutIndentSpace(); _sourceBuilder.AppendLine("private const int __generator_internal_InitiatedSyncDispose  = 1;");
                    PutIndentSpace(); _sourceBuilder.AppendLine("private const int __generator_internal_InitiatedAsyncDispose = 2;");
                    PutIndentSpace(); _sourceBuilder.AppendLine("private const int __generator_internal_DisposeAlreadyCompleted = 9;");
                    PutIndentSpace(); _sourceBuilder.AppendLine("private int __generator_internal_disposeState = __generator_internal_BeNotInitiatedAnyDispose;");

                    // 自分がIDispose.Dispose()を定義する場合に限って自己管理している破棄状態(__generator_internal_disposeState)を利用してIsDisposedプロパティを公開する。
                    WriteIsDisposedProperty();
                }

                _sourceBuilder.AppendLine();
                PutIndentSpace(); _sourceBuilder.AppendLine("private int __generator_internal_managedObjectDisposeState = 0;");

                if (IsEnabledFinalize)
                {
                    _sourceBuilder.AppendLine();
                    PutIndentSpace(); _sourceBuilder.AppendLine("private int __generator_internal_unmanagedResourceReleaseState = 0;");

                    WriteFinalizer();
                }

                if (_isIDisposableIntafeceImplementer || IsEnabledFinalize)
                {
                    WriteDisposeCoreMethod();
                }

                if (_isIDisposableIntafeceImplementer)
                {
                    if (!_isDisposableSubClass)
                    {
                        WriteIDisposableDisposeMethod();
                    }

                }

                if (_isIAsyncDisposableIntafeceImplementer)
                {
                    if (!_isAsyncDisposableSubClass)
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


                void WriteIsDisposedProperty()
                {
                    _sourceBuilder.AppendLine();
                    PutIndentSpace(); _sourceBuilder.AppendLine("public bool IsDisposed => (global::System.Threading.Thread.VolatileRead(ref __generator_internal_disposeState) != __generator_internal_BeNotInitiatedAnyDispose);");
                }

                void WriteFinalizer()
                {
                    _sourceBuilder.AppendLine();

                    PutIndentSpace();
                    _sourceBuilder.Append("~");
                    _sourceBuilder.Append(_classDeclarationSymbol.Name);
                    _sourceBuilder.AppendLine("()");
                    BeginBlock();
                    {
                        PutIndentSpace(); _sourceBuilder.AppendLine("// Release unmaneged resources.");
                        PutIndentSpace(); _sourceBuilder.AppendLine("Dispose(disposing: false);");
                    }
                    EndBlock();
                }

                void WriteIDisposableDisposeMethod()
                {
                    _sourceBuilder.AppendLine();

                    BeginBlock("public void Dispose()");
                    {
                        PutIndentSpace(); _sourceBuilder.AppendLine("var dispose_state = global::System.Threading.Interlocked.CompareExchange(ref __generator_internal_disposeState, __generator_internal_InitiatedSyncDispose, __generator_internal_BeNotInitiatedAnyDispose);");
                        PutIndentSpace(); _sourceBuilder.AppendLine("if (dispose_state == __generator_internal_BeNotInitiatedAnyDispose)");
                        BeginBlock();
                        {
                            if (_isIDisposableIntafeceImplementer || IsEnabledFinalize)
                            {
                                _sourceBuilder.AppendLine();
                                PutIndentSpace(); _sourceBuilder.AppendLine("// Dispose managed members and release unmaneged resources.");
                                PutIndentSpace(); _sourceBuilder.AppendLine("Dispose(disposing: true);");
                            }

                            if (IsEnabledFinalize)
                            {
                                _sourceBuilder.AppendLine();
                                PutIndentSpace(); _sourceBuilder.AppendLine("global::System.GC.SuppressFinalize(this);");
                            }

                            _sourceBuilder.AppendLine();
                            PutIndentSpace(); _sourceBuilder.AppendLine("global::System.Threading.Thread.VolatileWrite(ref __generator_internal_disposeState, __generator_internal_DisposeAlreadyCompleted);");
                        }
                        EndBlock();
                    }
                    EndBlock();
                }

                void WriteDisposeCoreMethod()
                {
                    _sourceBuilder.AppendLine();

                    if (_isInheritalbeClass)
                    {
                        if (_isDisposableSubClass)
                        {
                            PutIndentSpace(); _sourceBuilder.AppendLine("protected override void Dispose(bool disposing)");
                        }
                        else
                        {
                            PutIndentSpace(); _sourceBuilder.AppendLine("protected virtual void Dispose(bool disposing)");
                        }
                    }
                    else
                    {
                        PutIndentSpace(); _sourceBuilder.AppendLine("private void Dispose(bool disposing)");
                    }
                    BeginBlock();
                    {
                        PutIndentSpace(); _sourceBuilder.AppendLine("if (disposing)");
                        BeginBlock();
                        {
                            PutIndentSpace(); _sourceBuilder.AppendLine("var managedObjectDisposeState = global::System.Threading.Interlocked.Exchange(ref __generator_internal_managedObjectDisposeState, 1);");
                            PutIndentSpace(); _sourceBuilder.AppendLine("if (managedObjectDisposeState == 0)");
                            BeginBlock();
                            {
                                foreach (var fieldSymbol in _disposableFields)
                                {
                                    if (_automaticDisposeContextChecker.IsDisabledModeField(fieldSymbol))
                                    {
                                        continue;
                                    }

                                    BeginTryBlock();
                                    {
                                        PutIndentSpace();
                                        _sourceBuilder.Append("(this.");
                                        _sourceBuilder.Append(fieldSymbol.Name);
                                        _sourceBuilder.AppendLine(" as global::System.IDisposable)?.Dispose();");
                                    }
                                    EndBlock();
                                    BeginBlock("catch (global::System.Exception ex)");
                                    {
                                        PutIndentSpace();
                                        _sourceBuilder.Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the ");
                                        _sourceBuilder.Append(fieldSymbol.Name);
                                        _sourceBuilder.AppendLine(@".Dispose() calling. Message=\""{ex.Message}\"""");");
                                    }
                                    EndBlock();
                                }

                                foreach (var propertySymbol in _disposableProperties)
                                {
                                    if (_automaticDisposeContextChecker.IsDisabledModeProperty(propertySymbol))
                                    {
                                        continue;
                                    }

                                    BeginTryBlock();
                                    {
                                        PutIndentSpace();
                                        _sourceBuilder.Append("(this.");
                                        _sourceBuilder.Append(propertySymbol.Name);
                                        _sourceBuilder.AppendLine(" as global::System.IDisposable)?.Dispose();");
                                    }
                                    EndBlock();
                                    BeginBlock("catch (global::System.Exception ex)");
                                    {
                                        PutIndentSpace();
                                        _sourceBuilder.Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the ");
                                        _sourceBuilder.Append(propertySymbol.Name);
                                        _sourceBuilder.AppendLine(@".Dispose() calling. Message=\""{ex.Message}\"""");");
                                    }
                                    EndBlock();
                                }


                                if (_userDefinedManagedObjectDisposeMethod is not null)
                                {
                                    BeginTryBlock();
                                    {
                                        _sourceBuilder.AppendLine("");

                                        PutIndentSpace(); _sourceBuilder.AppendLine("// Invoke user implemented disposer.");
                                        PutIndentSpace();
                                        _sourceBuilder.Append("this.");
                                        _sourceBuilder.Append(_userDefinedManagedObjectDisposeMethod.Name);
                                        _sourceBuilder.AppendLine("();");
                                    }
                                    EndBlock();
                                    BeginBlock("catch (global::System.Exception ex)");
                                    {
                                        PutIndentSpace();
                                        _sourceBuilder.Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the this.");
                                        _sourceBuilder.Append(_userDefinedManagedObjectDisposeMethod.Name);
                                        _sourceBuilder.AppendLine(@"() calling. Message=\""{ex.Message}\"""");");
                                    }
                                    EndBlock();
                                }
                            }
                            EndBlock();
                        }
                        EndBlock();

                        if (_userDefinedUnmanagedResourceReleaseMethod is not null)
                        {
                            PutIndentSpace(); _sourceBuilder.AppendLine("var unmanagedResourceReleaseState = global::System.Threading.Interlocked.Exchange(ref __generator_internal_unmanagedResourceReleaseState, 1);");
                            PutIndentSpace(); _sourceBuilder.AppendLine("if (unmanagedResourceReleaseState == 0)");
                            BeginBlock();
                            {
                                BeginTryBlock();
                                {
                                    PutIndentSpace(); _sourceBuilder.AppendLine("// Invoke user implemented finalizer.");
                                    PutIndentSpace();
                                    _sourceBuilder.Append("this.");
                                    _sourceBuilder.Append(_userDefinedUnmanagedResourceReleaseMethod.Name);
                                    _sourceBuilder.AppendLine("();");
                                }
                                EndBlock();
                                BeginBlock("catch (global::System.Exception ex)");
                                {
                                    PutIndentSpace();
                                    _sourceBuilder.Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the this.");
                                    _sourceBuilder.Append(_userDefinedUnmanagedResourceReleaseMethod.Name);
                                    _sourceBuilder.AppendLine(@"() calling. Message=\""{ex.Message}\"""");");
                                }
                                EndBlock();
                            }
                            EndBlock();
                        }

                        if (_isDisposableSubClass)
                        {
                            BeginTryBlock();
                            {
                                PutIndentSpace(); _sourceBuilder.AppendLine("base.Dispose(disposing);");
                            }
                            EndBlock();
                            BeginBlock("catch (global::System.Exception ex)");
                            {
                                PutIndentSpace(); _sourceBuilder.AppendLine(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the base.Dispose(bool) calling. Message=\""{ex.Message}\"""");");
                            }
                            EndBlock();
                        }
                    }
                    EndBlock();
                }



                void WriteIAsyncDisposableDisposeAsyncMethod()
                {
                    _sourceBuilder.AppendLine();

                    _sourceBuilder.AppendLine("#pragma warning disable CS1998");
                    BeginBlock("public async global::System.Threading.Tasks.ValueTask DisposeAsync()");
                    {
                        PutIndentSpace(); _sourceBuilder.AppendLine("var dispose_state = global::System.Threading.Interlocked.CompareExchange(ref __generator_internal_disposeState, __generator_internal_InitiatedAsyncDispose, __generator_internal_BeNotInitiatedAnyDispose);");
                        PutIndentSpace(); _sourceBuilder.AppendLine("if (dispose_state == __generator_internal_BeNotInitiatedAnyDispose)");
                        BeginBlock();
                        {
                            PutIndentSpace(); _sourceBuilder.AppendLine("await DisposeAsyncCore().ConfigureAwait(false);");

                            if (_isIDisposableIntafeceImplementer || IsEnabledFinalize)
                            {
                                _sourceBuilder.AppendLine();
                                PutIndentSpace(); _sourceBuilder.AppendLine("// Release unmaneged resources.");
                                PutIndentSpace(); _sourceBuilder.AppendLine("Dispose(disposing: false);");
                            }

                            if (IsEnabledFinalize)
                            {
                                _sourceBuilder.AppendLine();
                                _sourceBuilder.AppendLine("#pragma warning disable CA1816");
                                PutIndentSpace(); _sourceBuilder.AppendLine("global::System.GC.SuppressFinalize(this);");
                                _sourceBuilder.AppendLine("#pragma warning restore CA1816");
                            }

                            _sourceBuilder.AppendLine();
                            PutIndentSpace(); _sourceBuilder.AppendLine("global::System.Threading.Thread.VolatileWrite(ref __generator_internal_disposeState, __generator_internal_DisposeAlreadyCompleted);");
                        }
                        EndBlock();
                    }
                    EndBlock();
                    _sourceBuilder.AppendLine("#pragma warning restore CS1998");
                }

                void WriteDisposeAsyncCoreMethod()
                {
                    _sourceBuilder.AppendLine();

                    _sourceBuilder.AppendLine("#pragma warning disable CS1998");
                    PutIndentSpace();
                    if (_isInheritalbeClass)
                    {
                        if (_isAsyncDisposableSubClass)
                        {
                            _sourceBuilder.AppendLine("protected override async global::System.Threading.Tasks.ValueTask DisposeAsyncCore()");
                        }
                        else
                        {
                            _sourceBuilder.AppendLine("protected virtual async global::System.Threading.Tasks.ValueTask DisposeAsyncCore()");
                        }
                    }
                    else
                    {
                        _sourceBuilder.AppendLine("private async global::System.Threading.Tasks.ValueTask DisposeAsyncCore()");
                    }
                    BeginBlock();
                    {
                        PutIndentSpace(); _sourceBuilder.AppendLine("var managedObjectDisposeState = global::System.Threading.Interlocked.Exchange(ref __generator_internal_managedObjectDisposeState, 1);");
                        PutIndentSpace(); _sourceBuilder.AppendLine("if (managedObjectDisposeState == 0)");
                        BeginBlock();
                        {
                            foreach (var fieldSymbol in _asyncDisposableFields)
                            {
                                if (_automaticDisposeContextChecker.IsDisabledModeField(fieldSymbol))
                                {
                                    continue;
                                }

                                BeginTryBlock();
                                {
                                    PutIndentSpace();
                                    _sourceBuilder.Append("if (this.");
                                    _sourceBuilder.Append(fieldSymbol.Name);
                                    _sourceBuilder.Append(" is global::System.IAsyncDisposable asyncDisposable_");
                                    _sourceBuilder.Append(fieldSymbol.Name);
                                    _sourceBuilder.AppendLine(")");
                                    BeginBlock();
                                    {
                                        PutIndentSpace();
                                        _sourceBuilder.Append("await asyncDisposable_");
                                        _sourceBuilder.Append(fieldSymbol.Name);
                                        _sourceBuilder.AppendLine(".DisposeAsync().ConfigureAwait(false);");
                                    }
                                    EndBlock();
                                }
                                EndBlock();
                                BeginBlock("catch (global::System.Exception ex)");
                                {
                                    PutIndentSpace();
                                    _sourceBuilder.Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the ");
                                    _sourceBuilder.Append(fieldSymbol.Name);
                                    _sourceBuilder.AppendLine(@".Dispose() calling. Message=\""{ex.Message}\"""");");
                                }
                                EndBlock();
                            }


                            foreach (var propertySymbol in _asyncDisposableProperties)
                            {
                                if (_automaticDisposeContextChecker.IsDisabledModeProperty(propertySymbol))
                                {
                                    continue;
                                }

                                BeginTryBlock();
                                {
                                    PutIndentSpace();
                                    _sourceBuilder.Append("if (this.");
                                    _sourceBuilder.Append(propertySymbol.Name);
                                    _sourceBuilder.Append(" is global::System.IAsyncDisposable asyncDisposable_");
                                    _sourceBuilder.Append(propertySymbol.Name);
                                    _sourceBuilder.AppendLine(")");
                                    BeginBlock();
                                    {
                                        PutIndentSpace();
                                        _sourceBuilder.Append("await asyncDisposable_");
                                        _sourceBuilder.Append(propertySymbol.Name);
                                        _sourceBuilder.AppendLine(".DisposeAsync().ConfigureAwait(false);");
                                    }
                                    EndBlock();
                                }
                                EndBlock();
                                BeginBlock("catch (global::System.Exception ex)");
                                {
                                    PutIndentSpace();
                                    _sourceBuilder.Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the ");
                                    _sourceBuilder.Append(propertySymbol.Name);
                                    _sourceBuilder.AppendLine(@".Dispose() calling. Message=\""{ex.Message}\"""");");
                                }
                                EndBlock();
                            }

                            foreach (var fieldSymbol in _nonAsyncDisposableFields)
                            {
                                if (_automaticDisposeContextChecker.IsDisabledModeField(fieldSymbol))
                                {
                                    continue;
                                }

                                BeginTryBlock();
                                {
                                    PutIndentSpace();
                                    _sourceBuilder.Append("(this.");
                                    _sourceBuilder.Append(fieldSymbol.Name);
                                    _sourceBuilder.AppendLine(" as global::System.IDisposable)?.Dispose();");
                                }
                                EndBlock();
                                BeginBlock("catch (global::System.Exception ex)");
                                {
                                    PutIndentSpace();
                                    _sourceBuilder.Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the ");
                                    _sourceBuilder.Append(fieldSymbol.Name);
                                    _sourceBuilder.AppendLine(@".Dispose() calling. Message=\""{ex.Message}\"""");");
                                }
                                EndBlock();
                            }

                            foreach (var propertySymbol in _nonAsyncDisposableProperties)
                            {
                                if (_automaticDisposeContextChecker.IsDisabledModeProperty(propertySymbol))
                                {
                                    continue;
                                }

                                BeginTryBlock();
                                {
                                    PutIndentSpace();
                                    _sourceBuilder.Append("(this.");
                                    _sourceBuilder.Append(propertySymbol.Name);
                                    _sourceBuilder.AppendLine(" as global::System.IDisposable)?.Dispose();");
                                }
                                EndBlock();
                                BeginBlock("catch (global::System.Exception ex)");
                                {
                                    PutIndentSpace();
                                    _sourceBuilder.Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the ");
                                    _sourceBuilder.Append(propertySymbol.Name);
                                    _sourceBuilder.AppendLine(@".Dispose() calling. Message=\""{ex.Message}\"""");");
                                }
                                EndBlock();
                            }

                            if (_userDefinedManagedObjectAsyncDisposeMethod is not null)
                            {
                                BeginTryBlock();
                                {
                                    _sourceBuilder.AppendLine("");

                                    PutIndentSpace(); _sourceBuilder.AppendLine("// Invoke user implemented disposer.");
                                    PutIndentSpace();
                                    _sourceBuilder.Append("await this.");
                                    _sourceBuilder.Append(_userDefinedManagedObjectAsyncDisposeMethod.Name);
                                    _sourceBuilder.AppendLine("().ConfigureAwait(false);");
                                }
                                EndBlock();
                                BeginBlock("catch (global::System.Exception ex)");
                                {
                                    PutIndentSpace();
                                    _sourceBuilder.Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the this.");
                                    _sourceBuilder.Append(_userDefinedManagedObjectAsyncDisposeMethod.Name);
                                    _sourceBuilder.AppendLine(@"() calling. Message=\""{ex.Message}\"""");");
                                }
                                EndBlock();
                            }
                            else if (_userDefinedManagedObjectDisposeMethod is not null)
                            {
                                BeginTryBlock();
                                {
                                    _sourceBuilder.AppendLine("");

                                    PutIndentSpace(); _sourceBuilder.AppendLine("// Invoke user implemented disposer.");
                                    PutIndentSpace();
                                    _sourceBuilder.Append("this.");
                                    _sourceBuilder.Append(_userDefinedManagedObjectDisposeMethod.Name);
                                    _sourceBuilder.AppendLine("();");
                                }
                                EndBlock();
                                BeginBlock("catch (global::System.Exception ex)");
                                {
                                    PutIndentSpace();
                                    _sourceBuilder.Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the this.");
                                    _sourceBuilder.Append(_userDefinedManagedObjectDisposeMethod.Name);
                                    _sourceBuilder.AppendLine(@"() calling. Message=\""{ex.Message}\"""");");
                                }
                                EndBlock();
                            }
                        }
                        EndBlock();

                        _sourceBuilder.AppendLine();

                        if (_isAsyncDisposableSubClass)
                        {
                            BeginTryBlock();
                            {
                                PutIndentSpace(); _sourceBuilder.AppendLine("await base.DisposeAsyncCore().ConfigureAwait(false);");
                            }
                            EndBlock();
                            BeginBlock("catch (global::System.Exception ex)");
                            {
                                PutIndentSpace(); _sourceBuilder.AppendLine(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the base.DisposeAsyncCore() calling. Message=\""{ex.Message}\"""");");
                            }
                            EndBlock();
                        }
                    }
                    EndBlock();
                    _sourceBuilder.AppendLine("#pragma warning restore CS1998");
                }
            }
        }
    }
}
