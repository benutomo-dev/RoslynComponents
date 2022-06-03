using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator
{
    public partial class AutomaticDisposeGenerator
    {
        interface ITypeContainer
        {
            string Name { get; }
        }

        class NameSpaceInfo : ITypeContainer, IEquatable<NameSpaceInfo?>
        {
            public string Name { get; }

            public NameSpaceInfo(string name)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
            }

            public override bool Equals(object? obj)
            {
                return Equals(obj as NameSpaceInfo);
            }

            public bool Equals(NameSpaceInfo? other)
            {
                return other is not null &&
                       Name == other.Name;
            }

            public override int GetHashCode()
            {
                return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
            }
        }

        class TypeDefinitionInfo : ITypeContainer, IEquatable<TypeDefinitionInfo?>
        {
            public ITypeContainer Container { get; }

            public string Name { get; }

            public bool IsValueType { get; }

            public bool IsNullableAnoteted { get; }

            public ImmutableArray<string> GenericTypeArgs { get; }

            public TypeDefinitionInfo(ITypeContainer container, string name, bool isValueType, bool isNullableAnoteted, ImmutableArray<string> genericTypeArgs)
            {
                Container = container ?? throw new ArgumentNullException(nameof(container));
                Name = name ?? throw new ArgumentNullException(nameof(name));
                IsValueType = isValueType;
                IsNullableAnoteted = isNullableAnoteted;
                GenericTypeArgs = genericTypeArgs;
            }

            public override bool Equals(object? obj)
            {
                return Equals(obj as TypeDefinitionInfo);
            }

            public bool Equals(TypeDefinitionInfo? other)
            {
                return other is not null &&
                       EqualityComparer<ITypeContainer>.Default.Equals(Container, other.Container) &&
                       Name == other.Name &&
                       IsValueType == other.IsValueType &&
                       IsNullableAnoteted == other.IsNullableAnoteted &&
                       GenericTypeArgs.SequenceEqual(other.GenericTypeArgs);
            }

            public override int GetHashCode()
            {
                int hashCode = 319546947;
                hashCode = hashCode * -1521134295 + EqualityComparer<ITypeContainer>.Default.GetHashCode(Container);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
                hashCode = hashCode * -1521134295 + IsValueType.GetHashCode();
                hashCode = hashCode * -1521134295 + IsNullableAnoteted.GetHashCode();
                hashCode = hashCode * -1521134295 + GenericTypeArgs.Sum(v => v.GetHashCode());
                return hashCode;
            }
        }

        class SourceBuildInputs : IEquatable<SourceBuildInputs?>
        {
            public TypeDefinitionInfo TargetTypeInfo;

            public List<string> SyncDisposeMembersInDisposeMethod;

            public List<string> SyncDisposeMembersInAsyncDisposeMethod;

            public List<string> AsyncDisposeMembersInAsyncDisposeMethod;

            public string? _userDefinedUnmanagedResourceReleaseMethodName;

            public string? _userDefinedManagedObjectDisposeMethodName;

            public string? _userDefinedManagedObjectAsyncDisposeMethodName;

            public bool _isIDisposableIntafeceImplementer;

            public bool _isIAsyncDisposableIntafeceImplementer;

            public bool _isDisposableSubClass;

            public bool _isAsyncDisposableSubClass;

            public bool _isInheritalbeClass;


            public SourceBuildInputs(INamedTypeSymbol namedTypeSymbol, UsingSymbols usingSymbols, AttributeData automaticDisposeImplAttributeData)
            {
                TargetTypeInfo = BuildTypeDefinitionInfo(namedTypeSymbol);

                var automaticDisposeContextChecker = new AutomaticDisposeContextChecker(automaticDisposeImplAttributeData);

                _isIDisposableIntafeceImplementer = IsAssignableToIDisposable(namedTypeSymbol);

                _isIAsyncDisposableIntafeceImplementer = IsAssignableToIAsyncDisposable(namedTypeSymbol);

                _isDisposableSubClass = IsAssignableToIDisposable(namedTypeSymbol.BaseType);

                _isAsyncDisposableSubClass = IsAssignableToIAsyncDisposable(namedTypeSymbol.BaseType);

                _isInheritalbeClass = !namedTypeSymbol.IsValueType && !namedTypeSymbol.IsSealed;

                SyncDisposeMembersInDisposeMethod = new();
                SyncDisposeMembersInAsyncDisposeMethod = new();
                AsyncDisposeMembersInAsyncDisposeMethod = new();

                foreach (var member in namedTypeSymbol.GetMembers())
                {
                    if (member.IsStatic) continue;

                    if (member is IMethodSymbol methodSymbol)
                    {
                        foreach (var attribute in member.GetAttributes())
                        {
                            if (_userDefinedUnmanagedResourceReleaseMethodName is null && IsUnmanagedResourceReleaseMethodAttribute(attribute.AttributeClass))
                            {
                                _userDefinedUnmanagedResourceReleaseMethodName = methodSymbol.Name;
                                break;
                            }
                            if (_userDefinedManagedObjectDisposeMethodName is null && IsManagedObjectDisposeMethodAttribute(attribute.AttributeClass))
                            {
                                _userDefinedManagedObjectDisposeMethodName = methodSymbol.Name;
                                break;
                            }
                            if (_userDefinedManagedObjectAsyncDisposeMethodName is null && IsManagedObjectAsyncDisposeMethodAttribute(attribute.AttributeClass))
                            {
                                _userDefinedManagedObjectAsyncDisposeMethodName = methodSymbol.Name;
                                break;
                            }
                        }
                    }
                    else if (member is IFieldSymbol fieldSymbol)
                    {
                        if (fieldSymbol.IsImplicitlyDeclared) continue;

                        if (!automaticDisposeContextChecker.IsEnableField(fieldSymbol)) continue;

                        var isAssignableToIDisposable = IsAssignableToIDisposable(fieldSymbol.Type);
                        var isAssignableToIAsyncDisposable = IsAssignableToIAsyncDisposable(fieldSymbol.Type);

                        if (isAssignableToIDisposable)
                        {
                            // Dispose()が呼び出し可能
                            SyncDisposeMembersInDisposeMethod.Add(fieldSymbol.Name);

                            if (!isAssignableToIAsyncDisposable)
                            {
                                // Dispose()のみが呼び出し可能
                                SyncDisposeMembersInAsyncDisposeMethod.Add(fieldSymbol.Name);
                            }
                        }

                        if (isAssignableToIAsyncDisposable)
                        {
                            // DisposeAsync()が呼び出し可能
                            AsyncDisposeMembersInAsyncDisposeMethod.Add(fieldSymbol.Name);
                        }
                    }
                    else if (member is IPropertySymbol propertySymbol)
                    {
                        if (propertySymbol.IsImplicitlyDeclared) continue;

                        if (!automaticDisposeContextChecker.IsEnableProperty(propertySymbol)) continue;

                        var isAssignableToIDisposable = IsAssignableToIDisposable(propertySymbol.Type);
                        var isAssignableToIAsyncDisposable = IsAssignableToIAsyncDisposable(propertySymbol.Type);

                        if (isAssignableToIDisposable)
                        {
                            // Dispose()が呼び出し可能
                            SyncDisposeMembersInDisposeMethod.Add(propertySymbol.Name);

                            if (!isAssignableToIAsyncDisposable)
                            {
                                // Dispose()のみが呼び出し可能
                                SyncDisposeMembersInAsyncDisposeMethod.Add(propertySymbol.Name);
                            }
                        }

                        if (isAssignableToIAsyncDisposable)
                        {
                            // DisposeAsync()が呼び出し可能
                            AsyncDisposeMembersInAsyncDisposeMethod.Add(propertySymbol.Name);
                        }
                    }
                }

                return;



                static TypeDefinitionInfo BuildTypeDefinitionInfo(ITypeSymbol typeSymbol)
                {
                    ITypeContainer container;

                    if (typeSymbol.ContainingType is null)
                    {
                        var namespaceBuilder = new StringBuilder();
                        AppendFullNamespace(namespaceBuilder, typeSymbol.ContainingNamespace);

                        container = new NameSpaceInfo(namespaceBuilder.ToString());
                    }
                    else
                    {
                        container = BuildTypeDefinitionInfo(typeSymbol.ContainingType);
                    }

                    ImmutableArray<string> genericTypeArgs = ImmutableArray<string>.Empty;

                    if (typeSymbol is INamedTypeSymbol namedTypeSymbol && !namedTypeSymbol.TypeArguments.IsDefaultOrEmpty)
                    {
                        var builder = ImmutableArray.CreateBuilder<string>(namedTypeSymbol.TypeArguments.Length);

                        for (int i = 0; i < namedTypeSymbol.TypeArguments.Length; i++)
                        {
                            builder.Add(namedTypeSymbol.TypeArguments[i].Name);
                        }

                        genericTypeArgs = builder.MoveToImmutable();
                    }

                    return new TypeDefinitionInfo(container, typeSymbol.Name, typeSymbol.IsValueType, typeSymbol.NullableAnnotation == NullableAnnotation.Annotated, genericTypeArgs);
                }

                static void AppendFullTypeName(StringBuilder typeNameBuilder, ITypeSymbol typeSymbol)
                {
                    if (typeSymbol.ContainingType is null)
                    {
                        typeNameBuilder.Append("global::");
                        AppendFullNamespace(typeNameBuilder, typeSymbol.ContainingNamespace);
                    }
                    else
                    {
                        AppendFullTypeName(typeNameBuilder, typeSymbol.ContainingType);
                    }
                    typeNameBuilder.Append(".");

                    typeNameBuilder.Append(typeSymbol.Name);

                    if (typeSymbol is INamedTypeSymbol namedTypeSymbol && !namedTypeSymbol.TypeArguments.IsDefaultOrEmpty)
                    {
                        var typeArguments = namedTypeSymbol.TypeArguments;

                        typeNameBuilder.Append("<");

                        for (int i = 0; i < typeArguments.Length - 1; i++)
                        {
                            AppendFullTypeName(typeNameBuilder, typeArguments[i]);
                            typeNameBuilder.Append(", ");
                        }
                        AppendFullTypeName(typeNameBuilder, typeArguments[typeArguments.Length - 1]);

                        typeNameBuilder.Append(">");
                    }

                    if (typeSymbol.IsReferenceType && typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
                    {
                        typeNameBuilder.Append("?");
                    }
                }

                static void AppendFullNamespace(StringBuilder namespaceNameBuilder, INamespaceSymbol namespaceSymbol)
                {
                    if (namespaceSymbol.ContainingNamespace is not null && !namespaceSymbol.ContainingNamespace.IsGlobalNamespace)
                    {
                        AppendFullNamespace(namespaceNameBuilder, namespaceSymbol.ContainingNamespace);
                        namespaceNameBuilder.Append(".");
                    }

                    namespaceNameBuilder.Append(namespaceSymbol.Name);
                }
            }

            public override bool Equals(object? obj)
            {
                return Equals(obj as SourceBuildInputs);
            }

            public bool Equals(SourceBuildInputs? other)
            {
                var result = other is not null &&
                       EqualityComparer<TypeDefinitionInfo>.Default.Equals(TargetTypeInfo, other.TargetTypeInfo) &&
                       SyncDisposeMembersInDisposeMethod.SequenceEqual(other.SyncDisposeMembersInDisposeMethod) &&
                       SyncDisposeMembersInAsyncDisposeMethod.SequenceEqual(other.SyncDisposeMembersInAsyncDisposeMethod) &&
                       AsyncDisposeMembersInAsyncDisposeMethod.SequenceEqual(other.AsyncDisposeMembersInAsyncDisposeMethod) &&
                       _userDefinedUnmanagedResourceReleaseMethodName == other._userDefinedUnmanagedResourceReleaseMethodName &&
                       _userDefinedManagedObjectDisposeMethodName == other._userDefinedManagedObjectDisposeMethodName &&
                       _userDefinedManagedObjectAsyncDisposeMethodName == other._userDefinedManagedObjectAsyncDisposeMethodName &&
                       _isIDisposableIntafeceImplementer == other._isIDisposableIntafeceImplementer &&
                       _isIAsyncDisposableIntafeceImplementer == other._isIAsyncDisposableIntafeceImplementer &&
                       _isDisposableSubClass == other._isDisposableSubClass &&
                       _isAsyncDisposableSubClass == other._isAsyncDisposableSubClass &&
                       _isInheritalbeClass == other._isInheritalbeClass;

                WriteLogLine($"SourceBuildInputs.Equals({TargetTypeInfo.Name}, {other?.TargetTypeInfo.Name}) => {result}");

                return result;
            }

            public override int GetHashCode()
            {
                int hashCode = -160234080;
                hashCode = hashCode * -1521134295 + EqualityComparer<TypeDefinitionInfo>.Default.GetHashCode(TargetTypeInfo);
                hashCode = hashCode * -1521134295 + SyncDisposeMembersInDisposeMethod.Sum(v => EqualityComparer<string>.Default.GetHashCode(v));
                hashCode = hashCode * -1521134295 + SyncDisposeMembersInAsyncDisposeMethod.Sum(v => EqualityComparer<string>.Default.GetHashCode(v));
                hashCode = hashCode * -1521134295 + AsyncDisposeMembersInAsyncDisposeMethod.Sum(v => EqualityComparer<string>.Default.GetHashCode(v));
                hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(_userDefinedUnmanagedResourceReleaseMethodName);
                hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(_userDefinedManagedObjectDisposeMethodName);
                hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(_userDefinedManagedObjectAsyncDisposeMethodName);
                hashCode = hashCode * -1521134295 + _isIDisposableIntafeceImplementer.GetHashCode();
                hashCode = hashCode * -1521134295 + _isIAsyncDisposableIntafeceImplementer.GetHashCode();
                hashCode = hashCode * -1521134295 + _isDisposableSubClass.GetHashCode();
                hashCode = hashCode * -1521134295 + _isAsyncDisposableSubClass.GetHashCode();
                hashCode = hashCode * -1521134295 + _isInheritalbeClass.GetHashCode();
                return hashCode;
            }
        }

        class SourceBuilder
        {
            public string HintName => $"gen_{string.Join(".", _hintingTypeNames)}_{_nameSpace}_AutomaticDisposeImpl.cs";

            public string SourceText => _sourceBuilder.ToString();

            private bool IsEnabledFinalize => _sourceBuildInputs._userDefinedUnmanagedResourceReleaseMethodName is not null;

            readonly SourceProductionContext _context;

            readonly SourceBuildInputs _sourceBuildInputs;

            public List<string> _hintingTypeNames = new List<string>();

            public string _nameSpace;

            public StringBuilder _sourceBuilder = new StringBuilder(4000);

            int _currentIndentCount = 0;

            const string indentText = "    ";

            public SourceBuilder(SourceProductionContext context, SourceBuildInputs sourceBuildInputs)
            {
                _context = context;
                _sourceBuildInputs = sourceBuildInputs;

            }

            public void Build()
            {
                _context.CancellationToken.ThrowIfCancellationRequested();

                _hintingTypeNames.Clear();
                _nameSpace = "";
                _sourceBuilder.Clear();

                _sourceBuilder.AppendLine("#nullable enable");
                _sourceBuilder.AppendLine("#pragma warning disable CS0612,CS0618,CS0619");

                _context.CancellationToken.ThrowIfCancellationRequested();
                WriteTypeDeclarationStart();

                _context.CancellationToken.ThrowIfCancellationRequested();
                WriteBody();

                _context.CancellationToken.ThrowIfCancellationRequested();
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
                WriteContainingTypeStart(_sourceBuildInputs.TargetTypeInfo, isDesingationType: true);

                return;

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

                    _context.CancellationToken.ThrowIfCancellationRequested();

                    PutIndentSpace();
                    _sourceBuilder.Append("partial ");
                    _sourceBuilder.Append(namedTypeSymbol.IsValueType ? "struct " : "class ");
                    _sourceBuilder.Append(namedTypeSymbol.Name);

                    if (namedTypeSymbol.GenericTypeArgs.Length > 0)
                    {
                        _sourceBuilder.Append("<");
                        _sourceBuilder.Append(string.Join(", ", namedTypeSymbol.GenericTypeArgs));
                        _sourceBuilder.Append(">");

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

                        if (_sourceBuildInputs._isIDisposableIntafeceImplementer && _sourceBuildInputs._isIAsyncDisposableIntafeceImplementer)
                        {
                            _sourceBuilder.Append(" // This is implementation class for IDisposable and IAysncDisposables by AutomaticDisposeImpl.");
                        }
                        else if (_sourceBuildInputs._isIDisposableIntafeceImplementer)
                        {
                            _sourceBuilder.Append(" // This is implementation class for IDisposable by AutomaticDisposeImpl.");
                        }
                        else if (_sourceBuildInputs._isIAsyncDisposableIntafeceImplementer)
                        {
                            _sourceBuilder.Append(" // This is implementation class for IAysncDisposables by AutomaticDisposeImpl.");
                        }
                    }
                    _sourceBuilder.AppendLine("");

                    BeginBlock();
                }

                void WriteContainingNameSpaceStart(NameSpaceInfo namespaceSymbol)
                {
                    PutIndentSpace();
                    _sourceBuilder.Append("namespace ");
                    _sourceBuilder.Append(namespaceSymbol.Name);
                    _sourceBuilder.AppendLine("");

                    _nameSpace = namespaceSymbol.Name;

                    BeginBlock();
                }
            }

            void WriteTypeDeclarationEnd()
            {
                WriteContainingTypeEnd(_sourceBuildInputs.TargetTypeInfo);

                return;

                void WriteContainingTypeEnd(TypeDefinitionInfo namedTypeSymbol)
                {
                    EndBlock();

                    _context.CancellationToken.ThrowIfCancellationRequested();

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

            void WriteBody()
            {
                if (!_sourceBuildInputs._isDisposableSubClass)
                {
                    _context.CancellationToken.ThrowIfCancellationRequested();

                    PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.Browsable(false)]");
                    PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
                    PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Obsolete(\"AutomaticDisposeImplによって生成されたフィールドです。一般のコードから参照してはいけません。\")]");
                    PutIndentSpace(); _sourceBuilder.AppendLine("private const int __generator_internal_BeNotInitiatedAnyDispose = 0;");

                    PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.Browsable(false)]");
                    PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
                    PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Obsolete(\"AutomaticDisposeImplによって生成されたフィールドです。一般のコードから参照してはいけません。\")]");
                    PutIndentSpace(); _sourceBuilder.AppendLine("private const int __generator_internal_InitiatedSyncDispose  = 1;");

                    PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.Browsable(false)]");
                    PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
                    PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Obsolete(\"AutomaticDisposeImplによって生成されたフィールドです。一般のコードから参照してはいけません。\")]");
                    PutIndentSpace(); _sourceBuilder.AppendLine("private const int __generator_internal_InitiatedAsyncDispose = 2;");

                    PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.Browsable(false)]");
                    PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
                    PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Obsolete(\"AutomaticDisposeImplによって生成されたフィールドです。一般のコードから参照してはいけません。\")]");
                    PutIndentSpace(); _sourceBuilder.AppendLine("private const int __generator_internal_DisposeAlreadyCompleted = 9;");

                    PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.Browsable(false)]");
                    PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
                    PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Obsolete(\"AutomaticDisposeImplによって生成されたフィールドです。一般のコードから参照してはいけません。\")]");
                    PutIndentSpace(); _sourceBuilder.AppendLine("private int __generator_internal_disposeState = __generator_internal_BeNotInitiatedAnyDispose;");

                    // 自分がIDispose.Dispose()を定義する場合に限って自己管理している破棄状態(__generator_internal_disposeState)を利用してIsDisposedプロパティを公開する。
                    WriteIsDisposedProperty();
                }

                _sourceBuilder.AppendLine();
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.Browsable(false)]");
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
                PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Obsolete(\"AutomaticDisposeImplによって生成されたフィールドです。一般のコードから参照してはいけません。\")]");
                PutIndentSpace(); _sourceBuilder.AppendLine("private int __generator_internal_managedObjectDisposeState = 0;");

                if (IsEnabledFinalize)
                {
                    _context.CancellationToken.ThrowIfCancellationRequested();

                    _sourceBuilder.AppendLine();
                    PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.Browsable(false)]");
                    PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
                    PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Obsolete(\"AutomaticDisposeImplによって生成されたフィールドです。一般のコードから参照してはいけません。\")]");
                    PutIndentSpace(); _sourceBuilder.AppendLine("private int __generator_internal_unmanagedResourceReleaseState = 0;");

                    WriteFinalizer();
                }

                if (_sourceBuildInputs._isIDisposableIntafeceImplementer || IsEnabledFinalize)
                {
                    _context.CancellationToken.ThrowIfCancellationRequested();

                    WriteDisposeCoreMethod();
                }

                if (_sourceBuildInputs._isIDisposableIntafeceImplementer)
                {
                    if (!_sourceBuildInputs._isDisposableSubClass)
                    {
                        _context.CancellationToken.ThrowIfCancellationRequested();

                        WriteIDisposableDisposeMethod();
                    }

                }

                if (_sourceBuildInputs._isIAsyncDisposableIntafeceImplementer)
                {
                    if (!_sourceBuildInputs._isAsyncDisposableSubClass)
                    {
                        _context.CancellationToken.ThrowIfCancellationRequested();

                        WriteIAsyncDisposableDisposeAsyncMethod();
                    }

                    _context.CancellationToken.ThrowIfCancellationRequested();

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
                    _sourceBuilder.Append(_sourceBuildInputs.TargetTypeInfo.Name);
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
                            if (_sourceBuildInputs._isIDisposableIntafeceImplementer || IsEnabledFinalize)
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

                    if (_sourceBuildInputs._isInheritalbeClass)
                    {
                        if (_sourceBuildInputs._isDisposableSubClass)
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
                        PutIndentSpace(); _sourceBuilder.AppendLine("[global::System.Obsolete(\"AutomaticDisposeImplによって生成されたメソッドです。一般のコードから参照してはいけません。\")]");
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
                                foreach (var memberName in _sourceBuildInputs.SyncDisposeMembersInDisposeMethod)
                                {
                                    _context.CancellationToken.ThrowIfCancellationRequested();

                                    BeginTryBlock();
                                    {
                                        PutIndentSpace();
                                        _sourceBuilder.Append("(this.");
                                        _sourceBuilder.Append(memberName);
                                        _sourceBuilder.AppendLine(" as global::System.IDisposable)?.Dispose();");
                                    }
                                    EndBlock();
                                    BeginBlock("catch (global::System.Exception ex)");
                                    {
                                        PutIndentSpace();
                                        _sourceBuilder.Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the ");
                                        _sourceBuilder.Append(memberName);
                                        _sourceBuilder.AppendLine(@".Dispose() calling. Message=\""{ex.Message}\"""");");
                                    }
                                    EndBlock();
                                }


                                if (_sourceBuildInputs._userDefinedManagedObjectDisposeMethodName is not null)
                                {
                                    _context.CancellationToken.ThrowIfCancellationRequested();

                                    BeginTryBlock();
                                    {
                                        _sourceBuilder.AppendLine("");

                                        PutIndentSpace(); _sourceBuilder.AppendLine("// Invoke user implemented disposer.");
                                        PutIndentSpace();
                                        _sourceBuilder.Append("this.");
                                        _sourceBuilder.Append(_sourceBuildInputs._userDefinedManagedObjectDisposeMethodName);
                                        _sourceBuilder.AppendLine("();");
                                    }
                                    EndBlock();
                                    BeginBlock("catch (global::System.Exception ex)");
                                    {
                                        PutIndentSpace();
                                        _sourceBuilder.Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the this.");
                                        _sourceBuilder.Append(_sourceBuildInputs._userDefinedManagedObjectDisposeMethodName);
                                        _sourceBuilder.AppendLine(@"() calling. Message=\""{ex.Message}\"""");");
                                    }
                                    EndBlock();
                                }
                            }
                            EndBlock();
                        }
                        EndBlock();

                        if (_sourceBuildInputs._userDefinedUnmanagedResourceReleaseMethodName is not null)
                        {
                            _context.CancellationToken.ThrowIfCancellationRequested();

                            PutIndentSpace(); _sourceBuilder.AppendLine("var unmanagedResourceReleaseState = global::System.Threading.Interlocked.Exchange(ref __generator_internal_unmanagedResourceReleaseState, 1);");
                            PutIndentSpace(); _sourceBuilder.AppendLine("if (unmanagedResourceReleaseState == 0)");
                            BeginBlock();
                            {
                                BeginTryBlock();
                                {
                                    PutIndentSpace(); _sourceBuilder.AppendLine("// Invoke user implemented finalizer.");
                                    PutIndentSpace();
                                    _sourceBuilder.Append("this.");
                                    _sourceBuilder.Append(_sourceBuildInputs._userDefinedUnmanagedResourceReleaseMethodName);
                                    _sourceBuilder.AppendLine("();");
                                }
                                EndBlock();
                                BeginBlock("catch (global::System.Exception ex)");
                                {
                                    PutIndentSpace();
                                    _sourceBuilder.Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the this.");
                                    _sourceBuilder.Append(_sourceBuildInputs._userDefinedUnmanagedResourceReleaseMethodName);
                                    _sourceBuilder.AppendLine(@"() calling. Message=\""{ex.Message}\"""");");
                                }
                                EndBlock();
                            }
                            EndBlock();
                        }

                        if (_sourceBuildInputs._isDisposableSubClass)
                        {
                            _context.CancellationToken.ThrowIfCancellationRequested();

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

                            if (_sourceBuildInputs._isIDisposableIntafeceImplementer || IsEnabledFinalize)
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
                    if (_sourceBuildInputs._isInheritalbeClass)
                    {
                        if (_sourceBuildInputs._isAsyncDisposableSubClass)
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
                            foreach (var memberName in _sourceBuildInputs.AsyncDisposeMembersInAsyncDisposeMethod)
                            {
                                _context.CancellationToken.ThrowIfCancellationRequested();

                                BeginTryBlock();
                                {
                                    PutIndentSpace();
                                    _sourceBuilder.Append("if (this.");
                                    _sourceBuilder.Append(memberName);
                                    _sourceBuilder.Append(" is global::System.IAsyncDisposable asyncDisposable_");
                                    _sourceBuilder.Append(memberName);
                                    _sourceBuilder.AppendLine(")");
                                    BeginBlock();
                                    {
                                        PutIndentSpace();
                                        _sourceBuilder.Append("await asyncDisposable_");
                                        _sourceBuilder.Append(memberName);
                                        _sourceBuilder.AppendLine(".DisposeAsync().ConfigureAwait(false);");
                                    }
                                    EndBlock();
                                }
                                EndBlock();
                                BeginBlock("catch (global::System.Exception ex)");
                                {
                                    PutIndentSpace();
                                    _sourceBuilder.Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the ");
                                    _sourceBuilder.Append(memberName);
                                    _sourceBuilder.AppendLine(@".Dispose() calling. Message=\""{ex.Message}\"""");");
                                }
                                EndBlock();
                            }

                            foreach (var memberName in _sourceBuildInputs.SyncDisposeMembersInAsyncDisposeMethod)
                            {
                                _context.CancellationToken.ThrowIfCancellationRequested();

                                BeginTryBlock();
                                {
                                    PutIndentSpace();
                                    _sourceBuilder.Append("(this.");
                                    _sourceBuilder.Append(memberName);
                                    _sourceBuilder.AppendLine(" as global::System.IDisposable)?.Dispose();");
                                }
                                EndBlock();
                                BeginBlock("catch (global::System.Exception ex)");
                                {
                                    PutIndentSpace();
                                    _sourceBuilder.Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the ");
                                    _sourceBuilder.Append(memberName);
                                    _sourceBuilder.AppendLine(@".Dispose() calling. Message=\""{ex.Message}\"""");");
                                }
                                EndBlock();
                            }

                            if (_sourceBuildInputs._userDefinedManagedObjectAsyncDisposeMethodName is not null)
                            {
                                BeginTryBlock();
                                {
                                    _sourceBuilder.AppendLine("");

                                    PutIndentSpace(); _sourceBuilder.AppendLine("// Invoke user implemented disposer.");
                                    PutIndentSpace();
                                    _sourceBuilder.Append("await this.");
                                    _sourceBuilder.Append(_sourceBuildInputs._userDefinedManagedObjectAsyncDisposeMethodName);
                                    _sourceBuilder.AppendLine("().ConfigureAwait(false);");
                                }
                                EndBlock();
                                BeginBlock("catch (global::System.Exception ex)");
                                {
                                    PutIndentSpace();
                                    _sourceBuilder.Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the this.");
                                    _sourceBuilder.Append(_sourceBuildInputs._userDefinedManagedObjectAsyncDisposeMethodName);
                                    _sourceBuilder.AppendLine(@"() calling. Message=\""{ex.Message}\"""");");
                                }
                                EndBlock();
                            }
                            else if (_sourceBuildInputs._userDefinedManagedObjectDisposeMethodName is not null)
                            {
                                BeginTryBlock();
                                {
                                    _sourceBuilder.AppendLine("");

                                    PutIndentSpace(); _sourceBuilder.AppendLine("// Invoke user implemented disposer.");
                                    PutIndentSpace();
                                    _sourceBuilder.Append("this.");
                                    _sourceBuilder.Append(_sourceBuildInputs._userDefinedManagedObjectDisposeMethodName);
                                    _sourceBuilder.AppendLine("();");
                                }
                                EndBlock();
                                BeginBlock("catch (global::System.Exception ex)");
                                {
                                    PutIndentSpace();
                                    _sourceBuilder.Append(@"global::System.Diagnostics.Debug.Fail($""Caught an exception in the this.");
                                    _sourceBuilder.Append(_sourceBuildInputs._userDefinedManagedObjectDisposeMethodName);
                                    _sourceBuilder.AppendLine(@"() calling. Message=\""{ex.Message}\"""");");
                                }
                                EndBlock();
                            }
                        }
                        EndBlock();

                        _sourceBuilder.AppendLine();

                        if (_sourceBuildInputs._isAsyncDisposableSubClass)
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
