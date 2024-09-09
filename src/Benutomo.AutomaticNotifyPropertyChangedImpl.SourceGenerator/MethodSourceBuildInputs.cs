using Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator.Embedding;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;

namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator
{
    enum ExplicitImplementationEventType
    {
        ChangedEventHandler,
        ChangingEventHandler,
    }

    record struct ExplicitImplementationArgs(string InterfaceType, ExplicitImplementationEventType EventType, string InterfaceEventName);

    sealed class MethodSourceBuildInputs : IEquatable<MethodSourceBuildInputs?>
    {
        public const string DefferedNotificationMethodSuffix = "WithDefferedNotification";

        public TypeDefinitionInfo ContainingTypeInfo;

        public string InternalPropertyName;

        public ImmutableArray<string> PropertyEventArgNames;

        public string DefaultNotificationPropertyName;

        public string FieldName;

        public string MethodName;

        public string DefferedNotificationMethodName;

        public string DefferedNotificationDisposableName;

        public string PropertyType;

        public bool PropertyTypeIsReferenceType;

        public bool PropertyTypeIsSystemString;

        public bool PropertyTypeIsInterlockExchangeable;

        public NullableAnnotation PropertyTypeNullableAnnotation;

        public ImmutableArray<SyntaxReference> PropertyDeclaringSyntaxReferences;

        public bool IsEventArgsOnly;

        public bool EnabledNotifyPropertyChanging;

        public bool EnabledNotifyPropertyChanged;

        public ImmutableArray<ExplicitImplementationArgs> ExplicitInterfaceImplementations;

        public GenerateMemberAccessibility ChangedEventAccessibility = GenerateMemberAccessibility.None;

        public GenerateMemberAccessibility ChangingEventAccessibility = GenerateMemberAccessibility.None;

        public GenerateMemberAccessibility ChangedObservableAccesibility = GenerateMemberAccessibility.None;

        public GenerateMemberAccessibility ChangingObservableAccesibility = GenerateMemberAccessibility.None;

        public MethodSourceBuildInputs(IPropertySymbol propertySymbol, UsingSymbols usingSymbols, AttributeData enableNotificationSupportAttributeData)
        {
            ContainingTypeInfo = propertySymbol.ContainingType.BuildTypeDefinitionInfo();

            InternalPropertyName = propertySymbol.Name;
            if (propertySymbol.ExplicitInterfaceImplementations.Length > 0)
            {
                PropertyEventArgNames = propertySymbol.ExplicitInterfaceImplementations.Select(v => v.Name).Distinct().ToImmutableArray();

                var interfaceName = propertySymbol.ExplicitInterfaceImplementations[0].ContainingType.Name;
                var interfacePropertyName = propertySymbol.ExplicitInterfaceImplementations[0].Name;

                DefaultNotificationPropertyName = interfacePropertyName;
                FieldName = $"__{interfaceName}_{interfacePropertyName}";
                MethodName = $"_{interfaceName}_{interfacePropertyName}";
                DefferedNotificationMethodName = $"_{interfaceName}_{interfacePropertyName}{DefferedNotificationMethodSuffix}";
                DefferedNotificationDisposableName = $"___{interfaceName}_{interfacePropertyName}WithDefferedNotificationDisposable";
            }
            else
            {
                PropertyEventArgNames = ImmutableArray.Create(propertySymbol.Name);
                DefaultNotificationPropertyName = propertySymbol.Name;
                FieldName = $"__{char.ToLowerInvariant(propertySymbol.Name[0])}{propertySymbol.Name.Substring(1)}";
                MethodName = $"_{propertySymbol.Name}";
                DefferedNotificationMethodName = $"_{propertySymbol.Name}{DefferedNotificationMethodSuffix}";
                DefferedNotificationDisposableName = $"___{propertySymbol.Name}WithDefferedNotificationDisposable";
            }

            StringBuilder typeNameBuilder = new StringBuilder();
            typeNameBuilder.AppendFullTypeNameWithNamespaceAlias(propertySymbol.Type);
            PropertyType = typeNameBuilder.ToString();
            PropertyTypeIsReferenceType = propertySymbol.Type.IsReferenceType;
            PropertyTypeIsSystemString = propertySymbol.Type.SpecialType == SpecialType.System_String;
            PropertyTypeIsInterlockExchangeable = propertySymbol.Type.IsInterlockedExchangable();
            PropertyTypeNullableAnnotation = propertySymbol.Type.NullableAnnotation;

            PropertyDeclaringSyntaxReferences = propertySymbol.DeclaringSyntaxReferences;

            IsEventArgsOnly = enableNotificationSupportAttributeData.NamedArguments.Where(v => v.Key == "EventArgsOnly").Select(v => (bool)(v.Value.Value ?? false)).FirstOrDefault();

            EnabledNotifyPropertyChanging = propertySymbol.ContainingType.AllInterfaces.Any(v => SymbolEqualityComparer.Default.Equals(v, usingSymbols.INotifyPropertyChanging));
            EnabledNotifyPropertyChanged = propertySymbol.ContainingType.AllInterfaces.Any(v => SymbolEqualityComparer.Default.Equals(v, usingSymbols.INotifyPropertyChanged));

            var defaultEnableExplicitInterfaceImplementations = propertySymbol.ExplicitInterfaceImplementations.Length > 0;

            ImmutableArray<ExplicitImplementationArgs>.Builder? explicitImplementationsBuilder = null;

            foreach (var attributeData in propertySymbol.GetAttributes())
            {
                GenerateMemberAccessibility defaultAccessibility = (propertySymbol.GetMethod?.DeclaredAccessibility, propertySymbol.DeclaredAccessibility) switch
                {
                    (Accessibility.Public, _) => GenerateMemberAccessibility.Public,
                    (Accessibility.Protected, _) => GenerateMemberAccessibility.Protected,
                    (Accessibility.ProtectedOrInternal, _) => GenerateMemberAccessibility.ProrectedInternal,
                    (Accessibility.ProtectedAndInternal, _) => GenerateMemberAccessibility.PrivateProrected,
                    (Accessibility.Internal, _) => GenerateMemberAccessibility.Internal,
                    (Accessibility.Private, _) => GenerateMemberAccessibility.Private,
                    (Accessibility.NotApplicable, Accessibility.Public) => GenerateMemberAccessibility.Public,
                    (Accessibility.NotApplicable, Accessibility.Protected) => GenerateMemberAccessibility.Protected,
                    (Accessibility.NotApplicable, Accessibility.ProtectedOrInternal) => GenerateMemberAccessibility.ProrectedInternal,
                    (Accessibility.NotApplicable, Accessibility.ProtectedAndInternal) => GenerateMemberAccessibility.PrivateProrected,
                    (Accessibility.NotApplicable, Accessibility.Internal) => GenerateMemberAccessibility.Internal,
                    (Accessibility.NotApplicable, Accessibility.Private) => GenerateMemberAccessibility.Private,
                    _ => GenerateMemberAccessibility.Private,
                };

                if (SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, usingSymbols.ChangedEventAttribute))
                {
                    (ChangedEventAccessibility, var enabledxplicitInterfaceImplementations) = resolveGenerateMemberAccessibility(
                        usingSymbols,
                        attributeData,
                        defaultAccessibility,
                        defaultEnableExplicitInterfaceImplementations
                        );

                    if (enabledxplicitInterfaceImplementations)
                    {
                        collectExplicitImplimentaionEvents(
                            ref explicitImplementationsBuilder,
                            propertySymbol,
                            usingSymbols,
                            "Changed",
                            ExplicitImplementationEventType.ChangedEventHandler
                            );
                    }
                }
                else if (SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, usingSymbols.ChangingEventAttribute))
                {
                    (ChangingEventAccessibility, var enabledxplicitInterfaceImplementations) = resolveGenerateMemberAccessibility(
                        usingSymbols,
                        attributeData,
                        defaultAccessibility,
                        defaultEnableExplicitInterfaceImplementations
                        );

                    if (enabledxplicitInterfaceImplementations)
                    {
                        collectExplicitImplimentaionEvents(
                            ref explicitImplementationsBuilder,
                            propertySymbol,
                            usingSymbols,
                            "Changing",
                            ExplicitImplementationEventType.ChangingEventHandler
                            );
                    }
                }
                else if (SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, usingSymbols.ChangedObservableAttribute))
                {
                    (ChangedObservableAccesibility, _) = resolveGenerateMemberAccessibility(
                        usingSymbols,
                        attributeData,
                        defaultAccessibility,
                        false
                        );
                }
                else if (SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, usingSymbols.ChantingObservableAttribute))
                {
                    (ChangingObservableAccesibility, _) = resolveGenerateMemberAccessibility(
                        usingSymbols,
                        attributeData,
                        defaultAccessibility,
                        false
                        );
                }
            }

            ExplicitInterfaceImplementations = explicitImplementationsBuilder?.ToImmutable() ?? ImmutableArray<ExplicitImplementationArgs>.Empty;

            if (ChangedEventAccessibility == GenerateMemberAccessibility.None && ChangedObservableAccesibility != GenerateMemberAccessibility.None)
            {
                // Observableはイベントを変換する形で実装するのでprivateでイベントを用意する。
                ChangedEventAccessibility = GenerateMemberAccessibility.Private;
            }

            if (ChangedEventAccessibility == GenerateMemberAccessibility.None && ExplicitInterfaceImplementations.Any(v => v.EventType == ExplicitImplementationEventType.ChangedEventHandler))
            {
                // 明示的実装のためのprivateでイベントを用意する。
                ChangedEventAccessibility = GenerateMemberAccessibility.PrivateForExplicitImplimetOnly;
            }

            if (ChangingEventAccessibility == GenerateMemberAccessibility.None && ChangingObservableAccesibility != GenerateMemberAccessibility.None)
            {
                // Observableやイベントの明示的実装はイベントを変換する形で実装するのでprivateでイベントを用意する。
                ChangingEventAccessibility = GenerateMemberAccessibility.Private;
            }

            if (ChangingEventAccessibility == GenerateMemberAccessibility.None && ExplicitInterfaceImplementations.Any(v => v.EventType == ExplicitImplementationEventType.ChangingEventHandler))
            {
                // 明示的実装のためのprivateでイベントを用意する。
                ChangingEventAccessibility = GenerateMemberAccessibility.PrivateForExplicitImplimetOnly;
            }



            return;


            static void collectExplicitImplimentaionEvents(
                ref ImmutableArray<ExplicitImplementationArgs>.Builder? explicitImplementationsBuilder,
                IPropertySymbol propertySymbol,
                UsingSymbols usingSymbols,
                string postfix,
                ExplicitImplementationEventType eventHandlerType
                )
            {
                foreach (var explicitInterfaceImplementation in propertySymbol.ExplicitInterfaceImplementations)
                {
                    var eventSourcePropertyName = explicitInterfaceImplementation.Name;

                    string? interfaceType = null;

                    foreach (var interfaceEvent in explicitInterfaceImplementation.ContainingType.GetMembers().OfType<IEventSymbol>())
                    {
                        if (!isMatch(interfaceEvent.Name, eventSourcePropertyName, postfix))
                        {
                            continue;
                        }
                        
                        if (!SymbolEqualityComparer.Default.Equals(interfaceEvent.Type, usingSymbols.EventHandler))
                        {
                            continue;
                        }

                        explicitImplementationsBuilder ??= ImmutableArray.CreateBuilder<ExplicitImplementationArgs>();

                        if (interfaceType is null)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendFullTypeNameWithNamespaceAlias(explicitInterfaceImplementation.ContainingType);
                            interfaceType = sb.ToString();
                        }

                        explicitImplementationsBuilder.Add(new(interfaceType, eventHandlerType, interfaceEvent.Name));
                    }
                }
                return;

                static bool isMatch(string memberName, string propertyName, string postfix)
                {
                    return true
                        && memberName.Length == propertyName.Length + postfix.Length
                        && memberName.AsSpan().Slice(0, propertyName.Length).SequenceEqual(propertyName.AsSpan())
                        && memberName.AsSpan().Slice(propertyName.Length, postfix.Length).SequenceEqual(postfix.AsSpan())
                        ;
                }
            }

            static (GenerateMemberAccessibility, bool) resolveGenerateMemberAccessibility(
                UsingSymbols usingSymbols,
                AttributeData attributeData,
                GenerateMemberAccessibility defaultAccessibility,
                bool defaultEnableExplicitInterfaceImplementations
                )
            {
                if (attributeData.ConstructorArguments.Length == 0)
                {
                    if (defaultEnableExplicitInterfaceImplementations)
                    {
                        return (GenerateMemberAccessibility.None, true);
                    }
                    else
                    {
                        return (defaultAccessibility, false);
                    }
                }

                if (attributeData.ConstructorArguments.Length == 1)
                {
                    if (SymbolEqualityComparer.Default.Equals(attributeData.ConstructorArguments[0].Type, usingSymbols.NotificationAccessibility))
                    {
                        return (resolveGenerateMemberAccessibilityCore(attributeData.ConstructorArguments[0].Value, defaultAccessibility), defaultEnableExplicitInterfaceImplementations);
                    }
                    else if (SymbolEqualityComparer.Default.Equals(attributeData.ConstructorArguments[0].Type, usingSymbols.ExplicitInterfaceImplementation))
                    {
                        return (GenerateMemberAccessibility.None, resolveExplicitInterfaceImplementationCore(attributeData.ConstructorArguments[0].Value));
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
                else if (attributeData.ConstructorArguments.Length == 2)
                {
                    return (
                        resolveGenerateMemberAccessibilityCore(attributeData.ConstructorArguments[0].Value, defaultAccessibility),
                        resolveExplicitInterfaceImplementationCore(attributeData.ConstructorArguments[1].Value)
                        );
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            static GenerateMemberAccessibility resolveGenerateMemberAccessibilityCore(object? value, GenerateMemberAccessibility defaultAccessibility)
            {
                return value switch
                {
                    (int)NotificationAccessibility.Public            => GenerateMemberAccessibility.Public,
                    (int)NotificationAccessibility.Internal          => GenerateMemberAccessibility.Internal,
                    (int)NotificationAccessibility.Protected         => GenerateMemberAccessibility.Protected,
                    (int)NotificationAccessibility.ProtectedInternal => GenerateMemberAccessibility.ProrectedInternal,
                    (int)NotificationAccessibility.PrivateProtected  => GenerateMemberAccessibility.PrivateProrected,
                    (int)NotificationAccessibility.Private           => GenerateMemberAccessibility.Private,
                    _ => GenerateMemberAccessibility.None,
                };
            }

            static bool resolveExplicitInterfaceImplementationCore(object? value)
            {
                return value switch
                {
                    (int)ExplicitInterfaceImplementation.Enable  => true,
                    (int)ExplicitInterfaceImplementation.Disable => false,
                    _ => true,
                };
            }
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as MethodSourceBuildInputs);
        }

        public bool Equals(MethodSourceBuildInputs? other)
        {
            var result = other is not null &&
                   EqualityComparer<TypeDefinitionInfo>.Default.Equals(ContainingTypeInfo, other.ContainingTypeInfo) &&
                   PropertyEventArgNames.SequenceEqual(other.PropertyEventArgNames) &&
                   PropertyType == other.PropertyType &&
                   PropertyTypeIsReferenceType == other.PropertyTypeIsReferenceType &&
                   PropertyTypeIsInterlockExchangeable == other.PropertyTypeIsInterlockExchangeable &&
                   PropertyTypeNullableAnnotation == other.PropertyTypeNullableAnnotation &&
                   IsEventArgsOnly == other.IsEventArgsOnly &&
                   EnabledNotifyPropertyChanging == other.EnabledNotifyPropertyChanging &&
                   EnabledNotifyPropertyChanged == other.EnabledNotifyPropertyChanged &&
                   ChangedEventAccessibility == other.ChangedEventAccessibility &&
                   ChangingEventAccessibility == other.ChangingEventAccessibility &&
                   ChangedObservableAccesibility == other.ChangedObservableAccesibility &&
                   ChangingObservableAccesibility == other.ChangingObservableAccesibility;

            return result;
        }

        public override int GetHashCode()
        {
            int hashCode = 126218788;
            hashCode = hashCode * -1521134295 + EqualityComparer<TypeDefinitionInfo>.Default.GetHashCode(ContainingTypeInfo);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PropertyEventArgNames[0]);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PropertyType);
            hashCode = hashCode * -1521134295 + PropertyTypeIsReferenceType.GetHashCode();
            hashCode = hashCode * -1521134295 + PropertyTypeIsInterlockExchangeable.GetHashCode();
            hashCode = hashCode * -1521134295 + PropertyTypeNullableAnnotation.GetHashCode();
            hashCode = hashCode * -1521134295 + IsEventArgsOnly.GetHashCode();
            hashCode = hashCode * -1521134295 + EnabledNotifyPropertyChanging.GetHashCode();
            hashCode = hashCode * -1521134295 + EnabledNotifyPropertyChanged.GetHashCode();
            hashCode = hashCode * -1521134295 + ChangedEventAccessibility.GetHashCode();
            hashCode = hashCode * -1521134295 + ChangingEventAccessibility.GetHashCode();
            hashCode = hashCode * -1521134295 + ChangedObservableAccesibility.GetHashCode();
            hashCode = hashCode * -1521134295 + ChangingObservableAccesibility.GetHashCode();
            return hashCode;
        }
    }
}
