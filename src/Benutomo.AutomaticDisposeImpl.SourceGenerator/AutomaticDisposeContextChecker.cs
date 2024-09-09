using Benutomo.AutomaticDisposeImpl.SourceGenerator.Embedding;
using Microsoft.CodeAnalysis;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator
{
    class AutomaticDisposeContextChecker
    {
        internal AutomaticDisposeImplMode Mode { get; }

        UsingSymbols _usingSymbols { get; }

        internal AutomaticDisposeContextChecker(AttributeData automaticDisposeImplAttributeData, UsingSymbols usingSymbols)
        {
            var defaultModeValue = automaticDisposeImplAttributeData.NamedArguments.SingleOrDefault(arg => arg.Key == nameof(AutomaticDisposeImplAttribute.Mode)).Value.Value;

            var defaultMode = defaultModeValue is int defaultModeRawValue ? (AutomaticDisposeImplMode)defaultModeRawValue : AutomaticDisposeImplMode.Explicit;

            Mode = defaultMode switch
            {
                AutomaticDisposeImplMode.Implicit => AutomaticDisposeImplMode.Implicit,
                AutomaticDisposeImplMode.Explicit => AutomaticDisposeImplMode.Explicit,
                _ => AutomaticDisposeImplMode.Explicit,
            };

            _usingSymbols = usingSymbols;
        }

        public bool IsEnableField(IFieldSymbol fieldSymbol)
        {
            if (fieldSymbol.IsAttributedBy(_usingSymbols.DisableAutomaticDisposeAttribute))
            {
                return false;
            }

            if (Mode == AutomaticDisposeImplMode.Explicit)
            {
                if (!fieldSymbol.IsAttributedBy(_usingSymbols.EnableAutomaticDisposeAttribute))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsEnableProperty(IPropertySymbol propertySymbol)
        {
            if (propertySymbol.IsAttributedBy(_usingSymbols.DisableAutomaticDisposeAttribute))
            {
                return false;
            }

            if (Mode == AutomaticDisposeImplMode.Explicit)
            {
                if (!propertySymbol.IsAttributedBy(_usingSymbols.EnableAutomaticDisposeAttribute))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
