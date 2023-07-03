using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Testing.Model;

namespace Benutomo.AutomaticDisposeImpl.Test.CustumAnalyzerTesting
{
    public abstract class CSharpAnalyzerWithIncrementalSourceGenerationTest : AnalyzerTest<XUnitVerifier>
    {
        public override string Language => LanguageNames.CSharp;

        protected override string DefaultFileExt => "cs";

        public CSharpAnalyzerWithIncrementalSourceGenerationTest()
        {
            var assemblies = ReferenceAssemblies;
            ;
        }

        protected override CompilationOptions CreateCompilationOptions() => new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);

        protected override ParseOptions CreateParseOptions() => new CSharpParseOptions(LanguageVersion.CSharp9, DocumentationMode.Diagnose);

        protected override async Task<Compilation> GetProjectCompilationAsync(Project project, IVerifier verifier, CancellationToken cancellationToken)
        {
            var (finalProject, diagnostics) = await ApplySourceGeneratorAsync(GetSourceGenerators().ToImmutableArray(), project, verifier, cancellationToken).ConfigureAwait(false);
            return (await finalProject.GetCompilationAsync(cancellationToken).ConfigureAwait(false))!;
        }

        protected override async Task<Project> CreateProjectImplAsync(EvaluatedProjectState primaryProject, ImmutableArray<EvaluatedProjectState> additionalProjects, CancellationToken cancellationToken)
        {
            var project = await base.CreateProjectImplAsync(primaryProject, additionalProjects, cancellationToken);

            project = project.AddMetadataReference(MetadataReference.CreateFromFile(typeof(EnableAutomaticDisposeAttribute).Assembly.Location));

            return project;
        }

        private async Task<(Project project, ImmutableArray<Diagnostic> diagnostics)> ApplySourceGeneratorAsync(ImmutableArray<IIncrementalGenerator> sourceGenerators, Project project, IVerifier verifier, CancellationToken cancellationToken)
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

        protected abstract IEnumerable<IIncrementalGenerator> GetSourceGenerators();

        protected virtual GeneratorDriver CreateGeneratorDriver(Project project, ImmutableArray<IIncrementalGenerator> sourceGenerators)
        {
            return CSharpGeneratorDriver.Create(
                sourceGenerators.Select(GeneratorExtensions.AsSourceGenerator),
                project.AnalyzerOptions.AdditionalFiles,
                (CSharpParseOptions)project.ParseOptions!,
                project.AnalyzerOptions.AnalyzerConfigOptionsProvider);
        }
    }

    public class CSharpAnalyzerWithIncrementalSourceGenerationTest<TAnalyzer, TSourceGenerator> : CSharpAnalyzerWithIncrementalSourceGenerationTest
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TSourceGenerator : IIncrementalGenerator, new()
    {
        protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers() => new[] { (DiagnosticAnalyzer)new TAnalyzer() };

        protected override IEnumerable<IIncrementalGenerator> GetSourceGenerators() => new[] { (IIncrementalGenerator)new TSourceGenerator() };

    }


    public class CSharpAnalyzerWithIncrementalSourceGenerationVerifier<TAnalyzer, TSourceGenerator> : AnalyzerVerifier<TAnalyzer, CSharpAnalyzerWithIncrementalSourceGenerationTest<TAnalyzer, TSourceGenerator>, XUnitVerifier>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TSourceGenerator : IIncrementalGenerator, new()
    {
    }
}
