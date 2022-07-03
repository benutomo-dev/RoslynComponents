using Microsoft.CodeAnalysis;

namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator
{
    record UsingSymbols(
        INamedTypeSymbol EnableNotificationSupportAttribute,
        INamedTypeSymbol ChangedEventAttribute,
        INamedTypeSymbol ChangingEventAttribute,
        INamedTypeSymbol ChangedObservableAttribute,
        INamedTypeSymbol ChantingObservableAttribute,
        INamedTypeSymbol INotifyPropertyChanged,
        INamedTypeSymbol INotifyPropertyChanging,
        INamedTypeSymbol NotificationAccessibility,
        INamedTypeSymbol ExplicitInterfaceImplementation,
        INamedTypeSymbol Action,
        INamedTypeSymbol EventHandler
        )
    {
        public static UsingSymbols CreateFrom(Compilation compilation)
        {
            var enableNotificationSupportAttributeSymbol = compilation.GetTypeByMetadataName(AutomaticGenerator.EnableNotificationSupportAttributeFullyQualifiedMetadataName) ?? throw new InvalidOperationException();
            var changedEventAttributeSymbol = compilation.GetTypeByMetadataName(AutomaticGenerator.ChangedEventAttributeFullyQualifiedMetadataName) ?? throw new InvalidOperationException();
            var changingEventAttributeSymbol = compilation.GetTypeByMetadataName(AutomaticGenerator.ChangingEventAttributeFullyQualifiedMetadataName) ?? throw new InvalidOperationException();
            var changedObservableAttributeSymbol = compilation.GetTypeByMetadataName(AutomaticGenerator.ChangedObservableAttributeFullyQualifiedMetadataName) ?? throw new InvalidOperationException();
            var changingObservableAttributeSymbol = compilation.GetTypeByMetadataName(AutomaticGenerator.ChangingObservableAttributeFullyQualifiedMetadataName) ?? throw new InvalidOperationException();
            var notifyPropertyChangedSymbol = compilation.GetTypeByMetadataName("System.ComponentModel.INotifyPropertyChanged") ?? throw new InvalidOperationException();
            var notifyPropertyChangingSymbol = compilation.GetTypeByMetadataName("System.ComponentModel.INotifyPropertyChanging") ?? throw new InvalidOperationException();
            var notificationAccessibilitySymbol = compilation.GetTypeByMetadataName(AutomaticGenerator.NotificationAccessibilityFullyQualifiedMetadataName) ?? throw new InvalidOperationException();
            var explicitInterfaceImplementationSymbol = compilation.GetTypeByMetadataName(AutomaticGenerator.ExplicitInterfaceImplementationFullyQualifiedMetadataName) ?? throw new InvalidOperationException();
            var actionSymbol = compilation.GetTypeByMetadataName("System.Action") ?? throw new InvalidOperationException();
            var eventHandlerSymbol = compilation.GetTypeByMetadataName("System.EventHandler") ?? throw new InvalidOperationException();

            return new UsingSymbols(
                enableNotificationSupportAttributeSymbol,
                changedEventAttributeSymbol,
                changingEventAttributeSymbol,
                changedObservableAttributeSymbol,
                changingObservableAttributeSymbol,
                notifyPropertyChangedSymbol,
                notifyPropertyChangingSymbol,
                notificationAccessibilitySymbol,
                explicitInterfaceImplementationSymbol,
                actionSymbol,
                eventHandlerSymbol
            );
        }

        public virtual bool Equals(UsingSymbols other)
        {
            var result =
                SymbolEqualityComparer.Default.Equals(EnableNotificationSupportAttribute, other.EnableNotificationSupportAttribute) &&
                SymbolEqualityComparer.Default.Equals(ChangedEventAttribute, other.ChangedEventAttribute) &&
                SymbolEqualityComparer.Default.Equals(ChangingEventAttribute, other.ChangingEventAttribute) &&
                SymbolEqualityComparer.Default.Equals(ChangedObservableAttribute, other.ChangedObservableAttribute) &&
                SymbolEqualityComparer.Default.Equals(ChantingObservableAttribute, other.ChantingObservableAttribute) &&
                SymbolEqualityComparer.Default.Equals(INotifyPropertyChanged, other.INotifyPropertyChanged) &&
                SymbolEqualityComparer.Default.Equals(INotifyPropertyChanging, other.INotifyPropertyChanging) &&
                SymbolEqualityComparer.Default.Equals(NotificationAccessibility, other.NotificationAccessibility) &&
                SymbolEqualityComparer.Default.Equals(ExplicitInterfaceImplementation, other.ExplicitInterfaceImplementation) &&
                SymbolEqualityComparer.Default.Equals(Action, other.Action) &&
                SymbolEqualityComparer.Default.Equals(EventHandler, other.EventHandler);

            return result;
        }

        public override int GetHashCode()
        {
            return
                SymbolEqualityComparer.Default.GetHashCode(EnableNotificationSupportAttribute) ^
                SymbolEqualityComparer.Default.GetHashCode(ChangedEventAttribute) ^
                SymbolEqualityComparer.Default.GetHashCode(ChangingEventAttribute) ^
                SymbolEqualityComparer.Default.GetHashCode(ChangedObservableAttribute) ^
                SymbolEqualityComparer.Default.GetHashCode(ChantingObservableAttribute) ^
                SymbolEqualityComparer.Default.GetHashCode(INotifyPropertyChanged) ^
                SymbolEqualityComparer.Default.GetHashCode(INotifyPropertyChanging) ^
                SymbolEqualityComparer.Default.GetHashCode(NotificationAccessibility) ^
                SymbolEqualityComparer.Default.GetHashCode(ExplicitInterfaceImplementation) ^
                SymbolEqualityComparer.Default.GetHashCode(Action) ^
                SymbolEqualityComparer.Default.GetHashCode(EventHandler);
        }
    }
}
