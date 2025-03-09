namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator;

record struct EventArgSourceBuilderInputs(CsTypeDeclaration ContainingType, EquatableArray<(string Name, PropertyEventArgClass EventArgClass)> Properties);
