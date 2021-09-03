using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Benutomo.AutomaticDisposeImpl.SourceGenerator;
using System.Threading;

namespace Benutomo.AutomaticDisposeImpl.Test.CustumAnalyzerTesting
{
    public abstract class CSharpAnalyzerWithSourceGenerationTest : AnalyzerTest<XUnitVerifier>
    {
        public override string Language => LanguageNames.CSharp;

        protected override string DefaultFileExt => "cs";

        protected override CompilationOptions CreateCompilationOptions() => new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);

        protected override ParseOptions CreateParseOptions() => new CSharpParseOptions(LanguageVersion.CSharp9, DocumentationMode.Diagnose);

        protected override async Task<Compilation> GetProjectCompilationAsync(Project project, IVerifier verifier, CancellationToken cancellationToken)
        {
            var (finalProject, diagnostics) = await ApplySourceGeneratorAsync(GetSourceGenerators().ToImmutableArray(), project, verifier, cancellationToken).ConfigureAwait(false);
            return (await finalProject.GetCompilationAsync(cancellationToken).ConfigureAwait(false))!;
        }

        private async Task<(Project project, ImmutableArray<Diagnostic> diagnostics)> ApplySourceGeneratorAsync(ImmutableArray<ISourceGenerator> sourceGenerators, Project project, IVerifier verifier, CancellationToken cancellationToken)
        {
            var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            verifier.True(compilation is { });

            var driver = CreateGeneratorDriver(project, sourceGenerators).RunGenerators(compilation, cancellationToken);
            var result = driver.GetRunResult();

            var updatedProject = project;
            foreach (var tree in result.GeneratedTrees)
            {
                updatedProject = updatedProject.AddDocument(tree.FilePath, await tree.GetTextAsync(cancellationToken).ConfigureAwait(false), filePath: tree.FilePath).Project;
            }

            return (updatedProject, result.Diagnostics);
        }

        protected abstract IEnumerable<ISourceGenerator> GetSourceGenerators();

        protected virtual GeneratorDriver CreateGeneratorDriver(Project project, ImmutableArray<ISourceGenerator> sourceGenerators)
        {
            return CSharpGeneratorDriver.Create(
                sourceGenerators,
                project.AnalyzerOptions.AdditionalFiles,
                (CSharpParseOptions)project.ParseOptions!,
                project.AnalyzerOptions.AnalyzerConfigOptionsProvider);
        }
    }

    public class CSharpAnalyzerWithSourceGenerationTest<TAnalyzer, TSourceGenerator> : CSharpAnalyzerWithSourceGenerationTest
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TSourceGenerator : ISourceGenerator, new()
    {
        protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers() => new[] { (DiagnosticAnalyzer)new TAnalyzer() };

        protected override IEnumerable<ISourceGenerator> GetSourceGenerators() => new[] { (ISourceGenerator)new TSourceGenerator() };

    }


    public class CSharpAnalyzerWithSourceGenerationVerifier<TAnalyzer, TSourceGenerator> : AnalyzerVerifier<TAnalyzer, CSharpAnalyzerWithSourceGenerationTest<TAnalyzer, TSourceGenerator>, XUnitVerifier>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TSourceGenerator : ISourceGenerator, new()
    {
    }
}
