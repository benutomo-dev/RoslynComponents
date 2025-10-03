using Benutomo.AutomaticDisposeImpl.SourceGenerator.Embedding;
using Microsoft.CodeAnalysis;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator;

record UsingSymbols(
    INamedTypeSymbol AutomaticDisposeImplAttribute,
    INamedTypeSymbol EnableAutomaticDisposeAttribute,
    INamedTypeSymbol DisableAutomaticDisposeAttribute,
    INamedTypeSymbol UnmanagedResourceReleaseMethodAttribute,
    INamedTypeSymbol ManagedObjectDisposeMethodAttribute,
    INamedTypeSymbol ManagedObjectAsyncDisposeMethodAttribute,
    INamedTypeSymbol IDisposable,
    INamedTypeSymbol? IAsyncDisposable,
    INamedTypeSymbol Task,
    INamedTypeSymbol? ValueTask
    )
{
    internal static UsingSymbols CreateFrom(Compilation compilation)
    {
        var automaticDisposeImplAttributeSymbol = compilation.GetTypeByMetadataName(StaticSourceAttribute.GetFullyQualifiedMetadataName<AutomaticDisposeImplAttribute>()) ?? throw new InvalidOperationException();
        var enableAutomaticDisposeAttributeSymbol = compilation.GetTypeByMetadataName(StaticSourceAttribute.GetFullyQualifiedMetadataName<EnableAutomaticDisposeAttribute>()) ?? throw new InvalidOperationException();
        var disableAutomaticDisposeAttributeSymbol = compilation.GetTypeByMetadataName(StaticSourceAttribute.GetFullyQualifiedMetadataName<DisableAutomaticDisposeAttribute>()) ?? throw new InvalidOperationException();
        var unmanagedResourceReleaseMethodAttributeSymbol = compilation.GetTypeByMetadataName(StaticSourceAttribute.GetFullyQualifiedMetadataName<UnmanagedResourceReleaseMethodAttribute>()) ?? throw new InvalidOperationException();
        var managedObjectDisposeMethodAttributeSymbol = compilation.GetTypeByMetadataName(StaticSourceAttribute.GetFullyQualifiedMetadataName<ManagedObjectDisposeMethodAttribute>()) ?? throw new InvalidOperationException();
        var managedObjectAsyncDisposeMethodAttributeSymbol = compilation.GetTypeByMetadataName(StaticSourceAttribute.GetFullyQualifiedMetadataName<ManagedObjectAsyncDisposeMethodAttribute>()) ?? throw new InvalidOperationException();
        var disposableSymbol = compilation.GetTypeByMetadataName("System.IDisposable") ?? throw new InvalidOperationException();
        var asyncDisposableSymbol = compilation.GetTypeByMetadataName("System.IAsyncDisposable");
        var taskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task") ?? throw new InvalidOperationException();
        var valueTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");

        return new UsingSymbols(
            automaticDisposeImplAttributeSymbol,
            enableAutomaticDisposeAttributeSymbol,
            disableAutomaticDisposeAttributeSymbol,
            unmanagedResourceReleaseMethodAttributeSymbol,
            managedObjectDisposeMethodAttributeSymbol,
            managedObjectAsyncDisposeMethodAttributeSymbol,
            disposableSymbol,
            asyncDisposableSymbol,
            taskSymbol,
            valueTaskSymbol
        );
    }

    public virtual bool Equals(UsingSymbols other)
    {
        var result =
            SymbolEqualityComparer.Default.Equals(AutomaticDisposeImplAttribute, other.AutomaticDisposeImplAttribute) &&
            SymbolEqualityComparer.Default.Equals(EnableAutomaticDisposeAttribute, other.EnableAutomaticDisposeAttribute) &&
            SymbolEqualityComparer.Default.Equals(DisableAutomaticDisposeAttribute, other.DisableAutomaticDisposeAttribute) &&
            SymbolEqualityComparer.Default.Equals(UnmanagedResourceReleaseMethodAttribute, other.UnmanagedResourceReleaseMethodAttribute) &&
            SymbolEqualityComparer.Default.Equals(ManagedObjectDisposeMethodAttribute, other.ManagedObjectDisposeMethodAttribute) &&
            SymbolEqualityComparer.Default.Equals(ManagedObjectAsyncDisposeMethodAttribute, other.ManagedObjectAsyncDisposeMethodAttribute) &&
            SymbolEqualityComparer.Default.Equals(IDisposable, other.IDisposable) &&
            SymbolEqualityComparer.Default.Equals(IAsyncDisposable, other.IAsyncDisposable);

        return result;
    }

    public override int GetHashCode()
    {
        return
            SymbolEqualityComparer.Default.GetHashCode(AutomaticDisposeImplAttribute) ^
            SymbolEqualityComparer.Default.GetHashCode(EnableAutomaticDisposeAttribute) ^
            SymbolEqualityComparer.Default.GetHashCode(DisableAutomaticDisposeAttribute) ^
            SymbolEqualityComparer.Default.GetHashCode(UnmanagedResourceReleaseMethodAttribute) ^
            SymbolEqualityComparer.Default.GetHashCode(ManagedObjectDisposeMethodAttribute) ^
            SymbolEqualityComparer.Default.GetHashCode(ManagedObjectAsyncDisposeMethodAttribute) ^
            SymbolEqualityComparer.Default.GetHashCode(IDisposable) ^
            SymbolEqualityComparer.Default.GetHashCode(IAsyncDisposable);
    }
}
