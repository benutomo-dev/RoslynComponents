using Microsoft.CodeAnalysis;
using System.Linq;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator
{
    class AutomaticDisposeContextChecker
    {
        internal AutomaticDisposeImplMode Mode { get; }

        internal AutomaticDisposeContextChecker(AttributeData automaticDisposeImplAttributeData)
        {
            var defaultModeValue = automaticDisposeImplAttributeData.NamedArguments.SingleOrDefault(arg => arg.Key == AutomaticDisposeGenerator.AutomaticDisposeImplAttributeModeName).Value.Value;

            var defaultMode = defaultModeValue is int defaultModeRawValue ? (AutomaticDisposeImplMode)defaultModeRawValue : AutomaticDisposeImplMode.Explicit;

            Mode = defaultMode switch
            {
                AutomaticDisposeImplMode.Implicit => AutomaticDisposeImplMode.Implicit,
                AutomaticDisposeImplMode.Explicit => AutomaticDisposeImplMode.Explicit,
                _ => AutomaticDisposeImplMode.Explicit,
            };
        }

        public bool IsEnableField(IFieldSymbol fieldSymbol)
        {
            if (fieldSymbol.GetAttributes().Any(attr => AutomaticDisposeGenerator.IsDisableAutomaticDisposeAttribute(attr.AttributeClass)))
            {
                return false;
            }

            if (Mode == AutomaticDisposeImplMode.Explicit)
            {
                if (!fieldSymbol.GetAttributes().Any(attr => AutomaticDisposeGenerator.IsEnableAutomaticDisposeAttribute(attr.AttributeClass)))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsEnableProperty(IPropertySymbol propertySymbol)
        {
            if (propertySymbol.GetAttributes().Any(attr => AutomaticDisposeGenerator.IsDisableAutomaticDisposeAttribute(attr.AttributeClass)))
            {
                return false;
            }

            if (Mode == AutomaticDisposeImplMode.Explicit)
            {
                if (!propertySymbol.GetAttributes().Any(attr => AutomaticDisposeGenerator.IsEnableAutomaticDisposeAttribute(attr.AttributeClass)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
