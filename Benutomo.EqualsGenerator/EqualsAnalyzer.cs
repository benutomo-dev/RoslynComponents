using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Benutomo.EqualsGenerator
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EqualsAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// AutomaticDisposeImpl属性を付与した型の定義はpartialである必要があります。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_EqualsGenerator0001 = new DiagnosticDescriptor(
            "EqualsGenerator0001",
            "Xxx",
            "Xxx。",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// AutomaticDisposeImpl属性を付与した型の定義はpartialである必要があります。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_EqualsGenerator0002 = new DiagnosticDescriptor(
            "EqualsGenerator0002",
            "Xxx",
            "Xxx。",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            s_diagnosticDescriptor_EqualsGenerator0001,
            s_diagnosticDescriptor_EqualsGenerator0002
            );

#if DEBUG
        [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1026:同時実行を有効にします", Justification = "<保留中>")]
#endif
        public override void Initialize(AnalysisContext context)
        {
#if !DEBUG
            context.EnableConcurrentExecution();
#endif
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol namedTypeSymbol)
            {
                return;
            }

            var usingSymbols = new UsingSymbols(context.Compilation);
        }
    }
}
