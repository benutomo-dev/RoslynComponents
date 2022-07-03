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

    class MethodSourceBuildInputs : IEquatable<MethodSourceBuildInputs?>
    {
        public TypeDefinitionInfo ContainingTypeInfo;

        public string InternalPropertyName;

        public ImmutableArray<string> PropertyEventArgNames;

        public string DefaultNotificationPropertyName;

        public string FieldName;

        public string MethodName;

        public string PropertyType;

        public bool PropertyTypeIsReferenceType;

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
            ContainingTypeInfo = BuildTypeDefinitionInfo(propertySymbol.ContainingType);

            InternalPropertyName = propertySymbol.Name;
            if (propertySymbol.ExplicitInterfaceImplementations.Length > 0)
            {
                PropertyEventArgNames = propertySymbol.ExplicitInterfaceImplementations.Select(v => v.Name).Distinct().ToImmutableArray();

                var interfaceName = propertySymbol.ExplicitInterfaceImplementations[0].ContainingType.Name;
                var interfacePropertyName = propertySymbol.ExplicitInterfaceImplementations[0].Name;

                DefaultNotificationPropertyName = interfacePropertyName;
                FieldName = $"__{interfaceName}_{interfacePropertyName}";
                MethodName = $"_{interfaceName}_{interfacePropertyName}";
            }
            else
            {
                PropertyEventArgNames = ImmutableArray.Create(propertySymbol.Name);
                DefaultNotificationPropertyName = propertySymbol.Name;
                FieldName = $"__{char.ToLowerInvariant(propertySymbol.Name[0])}{propertySymbol.Name.Substring(1)}";
                MethodName = $"_{propertySymbol.Name}";
            }

            StringBuilder typeNameBuilder = new StringBuilder();
            AppendFullTypeName(typeNameBuilder, propertySymbol.Type);
            PropertyType = typeNameBuilder.ToString();
            PropertyTypeIsReferenceType = propertySymbol.Type.IsReferenceType;
            PropertyTypeIsInterlockExchangeable = IsInterlockedExchangable(propertySymbol.Type);
            PropertyTypeNullableAnnotation = propertySymbol.Type.NullableAnnotation;

            PropertyDeclaringSyntaxReferences = propertySymbol.DeclaringSyntaxReferences;

            IsEventArgsOnly = enableNotificationSupportAttributeData.NamedArguments.Where(v => v.Key == "EventArgsOnly").Select(v => (bool)(v.Value.Value ?? false)).FirstOrDefault();

            EnabledNotifyPropertyChanging = propertySymbol.ContainingType.AllInterfaces.Any(v => SymbolEqualityComparer.Default.Equals(v, usingSymbols.NotifyPropertyChanging));
            EnabledNotifyPropertyChanged = propertySymbol.ContainingType.AllInterfaces.Any(v => SymbolEqualityComparer.Default.Equals(v, usingSymbols.NotifyPropertyChanged));

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

                if (SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, usingSymbols.ChangedEvent))
                {
                    (ChangedEventAccessibility, var enabledxplicitInterfaceImplementations) = ResolveGenerateMemberAccessibility(
                        usingSymbols,
                        attributeData,
                        defaultAccessibility,
                        defaultEnableExplicitInterfaceImplementations
                        );

                    if (enabledxplicitInterfaceImplementations)
                    {
                        CollectExplicitImplimentaionEvents(
                            ref explicitImplementationsBuilder,
                            propertySymbol,
                            usingSymbols,
                            "Changed",
                            ExplicitImplementationEventType.ChangedEventHandler
                            );
                    }
                }
                else if (SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, usingSymbols.ChangingEvent))
                {
                    (ChangingEventAccessibility, var enabledxplicitInterfaceImplementations) = ResolveGenerateMemberAccessibility(
                        usingSymbols,
                        attributeData,
                        defaultAccessibility,
                        defaultEnableExplicitInterfaceImplementations
                        );

                    if (enabledxplicitInterfaceImplementations)
                    {
                        CollectExplicitImplimentaionEvents(
                            ref explicitImplementationsBuilder,
                            propertySymbol,
                            usingSymbols,
                            "Changing",
                            ExplicitImplementationEventType.ChangingEventHandler
                            );
                    }
                }
                else if (SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, usingSymbols.ChangedObservable))
                {
                    (ChangedObservableAccesibility, _) = ResolveGenerateMemberAccessibility(
                        usingSymbols,
                        attributeData,
                        defaultAccessibility,
                        false
                        );
                }
                else if (SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, usingSymbols.ChantingObservable))
                {
                    (ChangingObservableAccesibility, _) = ResolveGenerateMemberAccessibility(
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

            static bool IsInterlockedExchangable(ITypeSymbol typeSymbol)
            {
                return typeSymbol switch
                {
                    { IsReferenceType: true } => true,
                    { SpecialType: SpecialType.System_Int32 } => true,
                    { SpecialType: SpecialType.System_Int64 } => true,
                    { SpecialType: SpecialType.System_IntPtr } => true,
                    { SpecialType: SpecialType.System_Single } => true,
                    { SpecialType: SpecialType.System_Double } => true,
                    _ => false,
                };
            }


            static void CollectExplicitImplimentaionEvents(
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
                        if (!IsMatch(interfaceEvent.Name, eventSourcePropertyName, postfix))
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
                            AppendFullTypeName(sb, explicitInterfaceImplementation.ContainingType);
                            interfaceType = sb.ToString();
                        }

                        explicitImplementationsBuilder.Add(new(interfaceType, eventHandlerType, interfaceEvent.Name));
                    }
                }
                return;

                static bool IsMatch(string memberName, string propertyName, string postfix)
                {
                    return true
                        && memberName.Length == propertyName.Length + postfix.Length
                        && memberName.AsSpan().Slice(0, propertyName.Length).SequenceEqual(propertyName.AsSpan())
                        && memberName.AsSpan().Slice(propertyName.Length, postfix.Length).SequenceEqual(postfix.AsSpan())
                        ;
                }
            }

            static (GenerateMemberAccessibility, bool) ResolveGenerateMemberAccessibility(
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
                        return (ResolveGenerateMemberAccessibilityCore(attributeData.ConstructorArguments[0].Value, defaultAccessibility), defaultEnableExplicitInterfaceImplementations);
                    }
                    else if (SymbolEqualityComparer.Default.Equals(attributeData.ConstructorArguments[0].Type, usingSymbols.ExplicitInterfaceImplementation))
                    {
                        return (GenerateMemberAccessibility.None, ResolveExplicitInterfaceImplementationCore(attributeData.ConstructorArguments[0].Value));
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
                else if (attributeData.ConstructorArguments.Length == 2)
                {
                    return (
                        ResolveGenerateMemberAccessibilityCore(attributeData.ConstructorArguments[0].Value, defaultAccessibility),
                        ResolveExplicitInterfaceImplementationCore(attributeData.ConstructorArguments[1].Value)
                        );
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            static GenerateMemberAccessibility ResolveGenerateMemberAccessibilityCore(object? value, GenerateMemberAccessibility defaultAccessibility)
            {
                return value switch
                {
                    AutomaticGenerator.NotificationAccessibilityPublic => GenerateMemberAccessibility.Public,
                    AutomaticGenerator.NotificationAccessibilityInternal => GenerateMemberAccessibility.Internal,
                    AutomaticGenerator.NotificationAccessibilityProtected => GenerateMemberAccessibility.Protected,
                    AutomaticGenerator.NotificationAccessibilityProtectedInternal => GenerateMemberAccessibility.ProrectedInternal,
                    AutomaticGenerator.NotificationAccessibilityPrivateProtected => GenerateMemberAccessibility.PrivateProrected,
                    AutomaticGenerator.NotificationAccessibilityPrivate => GenerateMemberAccessibility.Private,
                    _ => GenerateMemberAccessibility.None,
                };
            }

            static bool ResolveExplicitInterfaceImplementationCore(object? value)
            {
                return value switch
                {
                    AutomaticGenerator.ExplicitInterfaceImplementationEnable => true,
                    AutomaticGenerator.ExplicitInterfaceImplementationDisable => false,
                    _ => true,
                };
            }

            static TypeDefinitionInfo BuildTypeDefinitionInfo(ITypeSymbol typeSymbol)
            {
                ITypeContainer container;

                if (typeSymbol.ContainingType is null)
                {
                    var namespaceBuilder = new StringBuilder();
                    AppendFullNamespace(namespaceBuilder, typeSymbol.ContainingNamespace);

                    container = new NameSpaceInfo(namespaceBuilder.ToString());
                }
                else
                {
                    container = BuildTypeDefinitionInfo(typeSymbol.ContainingType);
                }

                ImmutableArray<string> genericTypeArgs = ImmutableArray<string>.Empty;

                if (typeSymbol is INamedTypeSymbol namedTypeSymbol && !namedTypeSymbol.TypeArguments.IsDefaultOrEmpty)
                {
                    var builder = ImmutableArray.CreateBuilder<string>(namedTypeSymbol.TypeArguments.Length);

                    for (int i = 0; i < namedTypeSymbol.TypeArguments.Length; i++)
                    {
                        builder.Add(namedTypeSymbol.TypeArguments[i].Name);
                    }

                    genericTypeArgs = builder.MoveToImmutable();
                }

                return new TypeDefinitionInfo(container, typeSymbol.Name, typeSymbol.IsValueType, typeSymbol.NullableAnnotation == NullableAnnotation.Annotated, genericTypeArgs);
            }

            static void AppendFullTypeName(StringBuilder typeNameBuilder, ITypeSymbol typeSymbol)
            {
                if (typeSymbol.ContainingType is null)
                {
                    typeNameBuilder.Append("global::");
                    AppendFullNamespace(typeNameBuilder, typeSymbol.ContainingNamespace);
                }
                else
                {
                    AppendFullTypeName(typeNameBuilder, typeSymbol.ContainingType);
                }
                typeNameBuilder.Append(".");

                typeNameBuilder.Append(typeSymbol.Name);

                if (typeSymbol is INamedTypeSymbol namedTypeSymbol && !namedTypeSymbol.TypeArguments.IsDefaultOrEmpty)
                {
                    var typeArguments = namedTypeSymbol.TypeArguments;

                    typeNameBuilder.Append("<");

                    for (int i = 0; i < typeArguments.Length - 1; i++)
                    {
                        AppendFullTypeName(typeNameBuilder, typeArguments[i]);
                        typeNameBuilder.Append(", ");
                    }
                    AppendFullTypeName(typeNameBuilder, typeArguments[typeArguments.Length - 1]);

                    typeNameBuilder.Append(">");
                }

                if (typeSymbol.IsReferenceType && typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    typeNameBuilder.Append("?");
                }
            }

            static void AppendFullNamespace(StringBuilder namespaceNameBuilder, INamespaceSymbol namespaceSymbol)
            {
                if (namespaceSymbol.ContainingNamespace is not null && !namespaceSymbol.ContainingNamespace.IsGlobalNamespace)
                {
                    AppendFullNamespace(namespaceNameBuilder, namespaceSymbol.ContainingNamespace);
                    namespaceNameBuilder.Append(".");
                }

                namespaceNameBuilder.Append(namespaceSymbol.Name);
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
