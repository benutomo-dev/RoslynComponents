using Microsoft.CodeAnalysis;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator
{
    record struct UsingSymbols(
        INamedTypeSymbol AutomaticDisposeImplAttributeSymbol,
        INamedTypeSymbol EnableAutomaticDisposeAttributeSymbol,
        INamedTypeSymbol DisableAutomaticDisposeAttributeSymbol,
        INamedTypeSymbol UnmanagedResourceReleaseMethodAttributeSymbol,
        INamedTypeSymbol ManagedObjectDisposeMethodAttributeSymbol,
        INamedTypeSymbol ManagedObjectAsyncDisposeMethodAttributeSymbol,
        INamedTypeSymbol DisposableSymbol,
        INamedTypeSymbol AsyncDisposableSymbol
        );
}
