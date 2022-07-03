using Microsoft.CodeAnalysis;

namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator
{
    record UsingSymbols(
        INamedTypeSymbol EnableNotificationSupportAttribute,
        INamedTypeSymbol ChangedEvent,
        INamedTypeSymbol ChangingEvent,
        INamedTypeSymbol ChangedObservable,
        INamedTypeSymbol ChantingObservable,
        INamedTypeSymbol NotifyPropertyChanged,
        INamedTypeSymbol NotifyPropertyChanging,
        INamedTypeSymbol NotificationAccessibility,
        INamedTypeSymbol ExplicitInterfaceImplementation,
        INamedTypeSymbol Action,
        INamedTypeSymbol EventHandler
        )
    {
        public virtual bool Equals(UsingSymbols other)
        {
            var result =
                SymbolEqualityComparer.IncludeNullability.Equals(EnableNotificationSupportAttribute, other.EnableNotificationSupportAttribute) &&
                SymbolEqualityComparer.IncludeNullability.Equals(ChangedEvent, other.ChangedEvent) &&
                SymbolEqualityComparer.IncludeNullability.Equals(ChangingEvent, other.ChangingEvent) &&
                SymbolEqualityComparer.IncludeNullability.Equals(ChangedObservable, other.ChangedObservable) &&
                SymbolEqualityComparer.IncludeNullability.Equals(ChantingObservable, other.ChantingObservable) &&
                SymbolEqualityComparer.IncludeNullability.Equals(NotifyPropertyChanged, other.NotifyPropertyChanged) &&
                SymbolEqualityComparer.IncludeNullability.Equals(NotifyPropertyChanging, other.NotifyPropertyChanging) &&
                SymbolEqualityComparer.IncludeNullability.Equals(NotificationAccessibility, other.NotificationAccessibility) &&
                SymbolEqualityComparer.IncludeNullability.Equals(ExplicitInterfaceImplementation, other.ExplicitInterfaceImplementation) &&
                SymbolEqualityComparer.IncludeNullability.Equals(Action, other.Action) &&
                SymbolEqualityComparer.IncludeNullability.Equals(EventHandler, other.EventHandler);

            return result;
        }

        public override int GetHashCode()
        {
            return
                SymbolEqualityComparer.IncludeNullability.GetHashCode(EnableNotificationSupportAttribute) ^
                SymbolEqualityComparer.IncludeNullability.GetHashCode(ChangedEvent) ^
                SymbolEqualityComparer.IncludeNullability.GetHashCode(ChangingEvent) ^
                SymbolEqualityComparer.IncludeNullability.GetHashCode(ChangedObservable) ^
                SymbolEqualityComparer.IncludeNullability.GetHashCode(ChantingObservable) ^
                SymbolEqualityComparer.IncludeNullability.GetHashCode(NotifyPropertyChanged) ^
                SymbolEqualityComparer.IncludeNullability.GetHashCode(NotifyPropertyChanging) ^
                SymbolEqualityComparer.IncludeNullability.GetHashCode(NotificationAccessibility) ^
                SymbolEqualityComparer.IncludeNullability.GetHashCode(ExplicitInterfaceImplementation) ^
                SymbolEqualityComparer.IncludeNullability.GetHashCode(Action) ^
                SymbolEqualityComparer.IncludeNullability.GetHashCode(EventHandler);
        }
    }
}
