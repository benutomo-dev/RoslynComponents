using System.Collections.Immutable;

namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator
{
    record struct EventArgSourceBuilderInputs(TypeDefinitionInfo ContainingTypeInfo, ImmutableArray<(string Name, PropertyEventArgClass EventArgClass)> Properties);
}
