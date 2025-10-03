using Microsoft.CodeAnalysis;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator;

class MethodSourceBuilderInputs : IEquatable<MethodSourceBuilderInputs?>
{
    public CsTypeDeclaration TargetType;

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


    public MethodSourceBuilderInputs(INamedTypeSymbol namedTypeSymbol, UsingSymbols usingSymbols, CsDeclarationProvider csDeclarationProvider, AttributeData automaticDisposeImplAttributeData)
    {
        TargetType = csDeclarationProvider.GetTypeDeclaration(namedTypeSymbol);

        var automaticDisposeContextChecker = new AutomaticDisposeContextChecker(automaticDisposeImplAttributeData, usingSymbols);

        _isIDisposableIntafeceImplementer = namedTypeSymbol.IsAssignableTo(usingSymbols.IDisposable, csDeclarationProvider.Compilation);

        _isIAsyncDisposableIntafeceImplementer = usingSymbols.IAsyncDisposable is not null && namedTypeSymbol.IsAssignableTo(usingSymbols.IAsyncDisposable, csDeclarationProvider.Compilation);

        _isDisposableSubClass = namedTypeSymbol.BaseType.IsAssignableTo(usingSymbols.IDisposable, csDeclarationProvider.Compilation);

        _isAsyncDisposableSubClass = usingSymbols.IAsyncDisposable is not null && namedTypeSymbol.BaseType.IsAssignableTo(usingSymbols.IAsyncDisposable, csDeclarationProvider.Compilation);

        _isInheritalbeClass = !namedTypeSymbol.IsValueType && !namedTypeSymbol.IsSealed;

        SyncDisposeMembersInDisposeMethod = [];
        SyncDisposeMembersInAsyncDisposeMethod = [];
        AsyncDisposeMembersInAsyncDisposeMethod = [];

        foreach (var member in namedTypeSymbol.GetMembers())
        {
            if (member.IsStatic) continue;

            if (member is IMethodSymbol methodSymbol)
            {
                foreach (var attribute in member.GetAttributes())
                {
                    if (_userDefinedUnmanagedResourceReleaseMethodName is null && attribute.AttributeClass.IsSameSymbolTo(usingSymbols.UnmanagedResourceReleaseMethodAttribute))
                    {
                        _userDefinedUnmanagedResourceReleaseMethodName = methodSymbol.Name;
                        break;
                    }
                    if (_userDefinedManagedObjectDisposeMethodName is null && attribute.AttributeClass.IsSameSymbolTo(usingSymbols.ManagedObjectDisposeMethodAttribute))
                    {
                        _userDefinedManagedObjectDisposeMethodName = methodSymbol.Name;
                        break;
                    }
                    if (_userDefinedManagedObjectAsyncDisposeMethodName is null && attribute.AttributeClass.IsSameSymbolTo(usingSymbols.ManagedObjectAsyncDisposeMethodAttribute))
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

                var isAssignableToIDisposable = fieldSymbol.IsAssignableTo(usingSymbols.IDisposable, csDeclarationProvider.Compilation);
                var isAssignableToIAsyncDisposable = usingSymbols.IAsyncDisposable is not null && fieldSymbol.IsAssignableTo(usingSymbols.IAsyncDisposable, csDeclarationProvider.Compilation);

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

                var isAssignableToIDisposable = propertySymbol.IsAssignableTo(usingSymbols.IDisposable, csDeclarationProvider.Compilation);
                var isAssignableToIAsyncDisposable = usingSymbols.IAsyncDisposable is not null && propertySymbol.IsAssignableTo(usingSymbols.IAsyncDisposable, csDeclarationProvider.Compilation);

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



    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as MethodSourceBuilderInputs);
    }

    public bool Equals(MethodSourceBuilderInputs? other)
    {
        var result = other is not null &&
               EqualityComparer<CsTypeDeclaration>.Default.Equals(TargetType, other.TargetType) &&
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
        hashCode = hashCode * -1521134295 + EqualityComparer<CsTypeDeclaration>.Default.GetHashCode(TargetType);
        hashCode = hashCode * -1521134295 + SyncDisposeMembersInDisposeMethod.Aggregate(0, (hash, v) => hash ^ EqualityComparer<string>.Default.GetHashCode(v));
        hashCode = hashCode * -1521134295 + SyncDisposeMembersInAsyncDisposeMethod.Aggregate(0, (hash, v) => hash ^ EqualityComparer<string>.Default.GetHashCode(v));
        hashCode = hashCode * -1521134295 + AsyncDisposeMembersInAsyncDisposeMethod.Aggregate(0, (hash, v) => hash ^ EqualityComparer<string>.Default.GetHashCode(v));
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
