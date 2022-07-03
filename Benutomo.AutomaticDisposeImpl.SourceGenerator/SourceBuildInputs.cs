using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;

using static Benutomo.AutomaticDisposeImpl.SourceGenerator.AutomaticDisposeGenerator;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator
{
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
}
