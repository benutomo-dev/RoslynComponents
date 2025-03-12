using Microsoft.CodeAnalysis;

namespace Benutomo.EqualsGenerator
{
    internal sealed class EqualsHelper
    {
        public static IEnumerable<(ISymbol symbol, ITypeSymbol type, bool isHashCodeCache)> EnumerateValidMembers(ITypeSymbol typeSymbol, UsingSymbols usingSymbols, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var shadowingMembersQuery = typeSymbol.GetMembers()
                .Where(v => v is IFieldSymbol or IPropertySymbol)
                .SelectMany(v => v.GetAttributes())
                .Where(v => SymbolEqualityComparer.Default.Equals(v.AttributeClass, usingSymbols.RepresentingEquivalenceForAttribute))
                .SelectMany(v => v.ConstructorArguments)
                .SelectMany(v => v.Kind == TypedConstantKind.Array ? v.Values.Select(v => v.Value) : [v.Value] )
                .Select(v => v?.ToString())
                .Where(v => v is not null).Select(v => v!);

            var shadowingMembers = new HashSet<string>(shadowingMembersQuery);

            var members = enumerateValidMembers(typeSymbol, usingSymbols, semanticModel, shadowingMembers, cancellationToken);

            return members;


            static IEnumerable<(ISymbol symbol, ITypeSymbol type, bool isHashCodeCache)> enumerateValidMembers(ITypeSymbol typeSymbol, UsingSymbols usingSymbols, SemanticModel semanticModel, HashSet<string> shadowingMembers, CancellationToken cancellationToken)
            {
                foreach (var member in typeSymbol.GetMembers())
                {
                    if (member.IsStatic) continue;

                    if (shadowingMembers.Contains(member.Name)) continue;

                    var isIgnoredMember = member.GetAttributes()
                        .Any(v => SymbolEqualityComparer.Default.Equals(v.AttributeClass, usingSymbols.IsNotEquivalenceFactorAttribute));

                    if (isIgnoredMember) continue;

                    ITypeSymbol memberType;

                    if (member is IFieldSymbol fieldSymbol)
                    {
                        if (fieldSymbol.IsImplicitlyDeclared) continue;

                        memberType = fieldSymbol.Type;
                    }
                    else if (member is IPropertySymbol propertySymbol)
                    {
                        if (propertySymbol.IsImplicitlyDeclared) continue;

                        if (!propertySymbol.IsAttributedBy(usingSymbols.RepresentingEquivalenceForAttribute))
                        {
                            var isAutoImplementedProperty = propertySymbol.IsAutoImplementedProperty(cancellationToken);
                            if (!isAutoImplementedProperty) continue;
                        }

                        memberType = propertySymbol.Type;
                    }
                    else
                    {
                        continue;
                    }


                    yield return (member, memberType, member.IsAttributedBy(usingSymbols.HashCodeCacheFieldAttribute));
                }
            }
        }
    }
}
