using Microsoft.CodeAnalysis;
using System.Linq;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator
{
    class AutomaticDisposeContextChecker
    {
        AutomaticDisposeImplMode _defaultMode;

        internal AutomaticDisposeContextChecker(AttributeData automaticDisposeImplAttributeData)
        {
            var defaultModeValue = automaticDisposeImplAttributeData.NamedArguments.SingleOrDefault(arg => arg.Key == AutomaticDisposeGenerator.AutomaticDisposeImplAttribute_DefaultMode).Value.Value;

            var defaultMode = defaultModeValue is int defaultModeRawValue ? (AutomaticDisposeImplMode)defaultModeRawValue : AutomaticDisposeImplMode.Default;
            _defaultMode = defaultMode switch
            {
                AutomaticDisposeImplMode.Disable => AutomaticDisposeImplMode.Disable,
                _ => AutomaticDisposeImplMode.Enable,
            };
        }

        public bool IsDisabledModeField(IFieldSymbol fieldSymbol)
        {
            var automaticDisposeModeAttributeData = fieldSymbol.GetAttributes().SingleOrDefault(attr => AutomaticDisposeGenerator.IsAutomaticDisposeImplModeAttribute(attr.AttributeClass));

            var modeValue = automaticDisposeModeAttributeData?.ConstructorArguments.SingleOrDefault().Value;

            var designationMode = modeValue is int modeRawValue ? (AutomaticDisposeImplMode)modeRawValue : AutomaticDisposeImplMode.Default;
            var actualMode = designationMode switch
            {
                AutomaticDisposeImplMode.Disable => AutomaticDisposeImplMode.Disable,
                AutomaticDisposeImplMode.Enable => AutomaticDisposeImplMode.Enable,
                _ => _defaultMode,
            };

            return actualMode == AutomaticDisposeImplMode.Disable;
        }

        public bool IsDisabledModeProperty(IPropertySymbol propertySymbol)
        {
            var automaticDisposeModeAttributeData = propertySymbol.GetAttributes().SingleOrDefault(attr => AutomaticDisposeGenerator.IsAutomaticDisposeImplModeAttribute(attr.AttributeClass));

            var modeValue = automaticDisposeModeAttributeData?.ConstructorArguments.SingleOrDefault().Value;

            var designationMode = modeValue is int modeRawValue ? (AutomaticDisposeImplMode)modeRawValue : AutomaticDisposeImplMode.Default;
            var actualMode = designationMode switch
            {
                AutomaticDisposeImplMode.Disable => AutomaticDisposeImplMode.Disable,
                AutomaticDisposeImplMode.Enable => AutomaticDisposeImplMode.Enable,
                _ => _defaultMode,
            };

            return actualMode == AutomaticDisposeImplMode.Disable;
        }
    }
}
