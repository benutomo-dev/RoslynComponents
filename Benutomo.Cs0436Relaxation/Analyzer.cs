﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Benutomo.Cs0436Relaxation
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// editorconfigファイルの編集が必要。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_RX_CS0436_0 = new DiagnosticDescriptor(
            "RX_CS0436_0",
            "The editorconfig file needs to be prepared",
            @"You must place an editorconfig file in your project that contains the following settings. ""dotnet_diagnostic.CS0436.severity = suggestion"".",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// ソースジェネレータによって生成されていないソースのシンボルに対してCS0436が発生。
        /// </summary>
        internal static DiagnosticDescriptor s_diagnosticDescriptor_RX_CS0436_1 = new DiagnosticDescriptor(
            "RX_CS0436_1",
            "CS0436 occurs for symbols in sources not generated by the source generator",
            "{0}",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            s_diagnosticDescriptor_RX_CS0436_0,
            s_diagnosticDescriptor_RX_CS0436_1
            );

#if DEBUG
        [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1026:同時実行を有効にします", Justification = "<保留中>")]
#endif
        public override void Initialize(AnalysisContext context)
        {
#if !DEBUG
            context.EnableConcurrentExecution();
#endif
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSemanticModelAction(SemanticModelAction);
            context.RegisterCompilationAction(CompilationAction);
        }

        private void CompilationAction(CompilationAnalysisContext context)
        {
            var syntaxTreeOptionsProvider = context.Compilation.Options.SyntaxTreeOptionsProvider;

            if (syntaxTreeOptionsProvider is null)
            {
                throw new InvalidOperationException("SyntaxTreeOptionsProviderが取得できませんでした。");
            }

            foreach (var diagnostic in context.Compilation.GetDeclarationDiagnostics(context.CancellationToken))
            {
                if (!IsCs0436(diagnostic))
                {
                    continue;
                }

                if (!IsSuppressed(diagnostic))
                {
                    context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_RX_CS0436_0, Location.None));
                    return;
                }
            }
        }

        private void SemanticModelAction(SemanticModelAnalysisContext context)
        {
            // ここで、context.SemanticModel.Compilation.GetDiagnosticsを呼び出すと、
            // おそらく、偶発的にメソッドのコンパイル完了前のSyntaxTreeのDiagnosticsの取得がされる場合がある関係で、
            // とくに、大きなプロジェクトのビルド途中でコンパイラが異常終了する現象が発生するようになるので注意
            
            if (!context.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.IntermediateOutputPath", out var intermediateOutPath)
                || string.IsNullOrWhiteSpace(intermediateOutPath))
            {
                intermediateOutPath = null;
            }
            if (intermediateOutPath is not null)
                intermediateOutPath = NormalizeDirectorySeparatorChar(intermediateOutPath);

            if (!context.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.CompilerGeneratedFilesOutputPath", out var generatedFilesOutPath)
                || string.IsNullOrWhiteSpace(generatedFilesOutPath))
            {
                generatedFilesOutPath = null;
            }
            if (generatedFilesOutPath is not null)
                generatedFilesOutPath = NormalizeDirectorySeparatorChar(generatedFilesOutPath);

            var diagnotics = context.SemanticModel.GetDiagnostics(null, context.CancellationToken);

            var syntaxTreeOptionsProvider = context.SemanticModel.Compilation.Options.SyntaxTreeOptionsProvider;

            if (syntaxTreeOptionsProvider is null)
            {
                throw new InvalidOperationException("SyntaxTreeOptionsProviderが取得できませんでした。");
            }

            foreach (var diagnostic in diagnotics)
            {
                if (diagnostic.Location.SourceTree != context.SemanticModel.SyntaxTree)
                {
                    continue;
                }

                if (!IsSuppressed(diagnostic))
                {
                    // 生のCS04036がinfo以下相当に抑止されていないならば、RX_CS0436_1を出す必要はない。
                    // ※ そのときは、別ルートでeditorconfigの準備を促す(RX_CS0436_0)が発生する。
                    continue;
                }

                if (!IsCs0436(diagnostic))
                {
                    continue;
                }

                if (!IsOccuredForAutogeneratedSysmbol(diagnostic, context.SemanticModel, syntaxTreeOptionsProvider, intermediateOutPath, generatedFilesOutPath, context.CancellationToken))
                {
                    context.ReportDiagnostic(Diagnostic.Create(s_diagnosticDescriptor_RX_CS0436_1, diagnostic.Location, diagnostic.GetMessage()));
                }
            }
        }

        private static bool IsSuppressed(Diagnostic diagnostic)
        {
            if (diagnostic.IsSuppressed || (diagnostic.Severity != DiagnosticSeverity.Warning && diagnostic.Severity != DiagnosticSeverity.Error))
            {
                return true;
            }

            return false;
        }

        private static bool IsCs0436(Diagnostic diagnostic)
        {
            return diagnostic.Id == "CS0436";
        }

        private static bool IsOccuredForAutogeneratedSysmbol(Diagnostic diagnostic, SemanticModel semanticModel, SyntaxTreeOptionsProvider syntaxTreeOptionsProvider, string? intermediateOutPath, string? generatedFilesOutPath, CancellationToken cancellationToken)
        {
            if (diagnostic.Location.SourceTree != semanticModel.SyntaxTree)
            {
                Debug.Fail("診断結果とセマンティックモデルのSystexTreeが不一致");
                return false;
            }

            var node = semanticModel.SyntaxTree.GetRoot().FindNode(diagnostic.Location.SourceSpan);

            if (node is null)
            {
                return false;
            }

            var symbolInfo = semanticModel.GetSymbolInfo(node, cancellationToken);

            if (symbolInfo.Symbol is null)
            {
                return false;
            }

            if (symbolInfo.Symbol.Locations.Length == 0)
            {
                return false;
            }

            foreach (var location in symbolInfo.Symbol.Locations)
            {
                if (!IsGeneratedCode(location, syntaxTreeOptionsProvider, intermediateOutPath, generatedFilesOutPath, cancellationToken))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsGeneratedCode(Location location, SyntaxTreeOptionsProvider syntaxTreeOptionsProvider, string? intermediateOutPath, string? generatedFilesOutPath, CancellationToken cancellationToken)
        {
            if (location.SourceTree is null) return false;

            var generatedKind = syntaxTreeOptionsProvider.IsGenerated(location.SourceTree, cancellationToken);

            if (generatedKind == GeneratedKind.NotGenerated) return false;

            var filePath = NormalizeDirectorySeparatorChar(location.SourceTree.FilePath);

            if (intermediateOutPath is not null && filePath.StartsWith(intermediateOutPath))
                return true; // 中間出力フォルダが出力先となっている場合は生成コードと判断

            if (generatedFilesOutPath is not null && filePath.StartsWith(generatedFilesOutPath))
                return true; // 生成ファイル出力フォルダが出力先となっている場合は生成コードと判断

            // VS17.10以降は生成コードの仮想的なファイルパスが以下の様になるので生成コードの場合にここに到達することはなくなる
            //   {コンパイラのアセンブリ出力先or生成ファイル出力フォルダ}/{アセンブリ名}/{生成クラス名}/{ソース登録時のHintName}

            // VS17.9までのソースジェネレータの生成コードのパスは
            //   {アセンブリ名}/{生成クラス名}/{ソース登録時のHintName}

            var filePathComponents = filePath.Split(Path.DirectorySeparatorChar);

            if (filePathComponents.Length != 3)
                return false;

            var assemblyName = filePathComponents[filePathComponents.Length - 3];
            var className = filePathComponents[filePathComponents.Length - 2];
            //var hintName = filePathComponents[filePathComponents.Length - 1];

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name != assemblyName)
                    continue;

                var type = assembly.GetType(className);
                if (type is null)
                    return false;

                if (Attribute.IsDefined(type, typeof(GeneratorAttribute)))
                {
                    // ソースジェネレータによって生成されたソースファイルと見做す
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        private static string NormalizeDirectorySeparatorChar(string path)
        {
            path = path
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            return path;
        }
    }
}
