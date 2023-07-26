using Benutomo.EqualsGenerator.Embedding;
using Microsoft.CodeAnalysis;

namespace Benutomo.EqualsGenerator
{
    internal class UsingSymbols : IEquatable<UsingSymbols?>
    {
        public INamedTypeSymbol AutomaticEqualsImplAttribute { get; init; }
        public INamedTypeSymbol IsNotEquivalenceFactorAttribute { get; init; }
        public INamedTypeSymbol RepresentingEquivalenceForAttribute { get; init; }
        public INamedTypeSymbol EqualityComparerAttribute { get; init; }

        public INamedTypeSymbol IEquatable { get; init; }

        internal UsingSymbols(Compilation compilation)
        {
            AutomaticEqualsImplAttribute = compilation.GetTypeByMetadataName(StaticSourceAttribute.GetFullyQualifiedMetadataName<AutomaticEqualsImplAttribute>()) ?? throw new InvalidOperationException();
            IsNotEquivalenceFactorAttribute = compilation.GetTypeByMetadataName(StaticSourceAttribute.GetFullyQualifiedMetadataName<IsNotEquivalenceFactorAttribute>()) ?? throw new InvalidOperationException();
            RepresentingEquivalenceForAttribute = compilation.GetTypeByMetadataName(StaticSourceAttribute.GetFullyQualifiedMetadataName<RepresentingEquivalenceForAttribute>()) ?? throw new InvalidOperationException();
            EqualityComparerAttribute = compilation.GetTypeByMetadataName(StaticSourceAttribute.GetFullyQualifiedMetadataName<EqualityComparerAttribute>()) ?? throw new InvalidOperationException();
            IEquatable = compilation.GetTypeByMetadataName("System.IEquatable`1") ?? throw new InvalidOperationException();
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as UsingSymbols);
        }

        public bool Equals(UsingSymbols? other)
        {
            return other is not null &&
                   SymbolEqualityComparer.Default.Equals(AutomaticEqualsImplAttribute, other.AutomaticEqualsImplAttribute) &&
                   SymbolEqualityComparer.Default.Equals(IsNotEquivalenceFactorAttribute, other.IsNotEquivalenceFactorAttribute) &&
                   SymbolEqualityComparer.Default.Equals(RepresentingEquivalenceForAttribute, other.RepresentingEquivalenceForAttribute) &&
                   SymbolEqualityComparer.Default.Equals(EqualityComparerAttribute, other.EqualityComparerAttribute) &&
                   SymbolEqualityComparer.Default.Equals(IEquatable, other.IEquatable);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(AutomaticEqualsImplAttribute, SymbolEqualityComparer.Default);
            hashCode.Add(IsNotEquivalenceFactorAttribute, SymbolEqualityComparer.Default);
            hashCode.Add(RepresentingEquivalenceForAttribute, SymbolEqualityComparer.Default);
            hashCode.Add(EqualityComparerAttribute, SymbolEqualityComparer.Default);
            hashCode.Add(IEquatable, SymbolEqualityComparer.Default);
            return hashCode.ToHashCode();
        }
    }
}
