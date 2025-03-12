using Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator.Embedding;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator
{
    [Generator(LanguageNames.CSharp)]
    public partial class SourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            StaticSources.StaticSource.Register(context);

            var enableNotificationSupportAttributeSymbol = context.CompilationProvider
                .Select((compilation, cancellationToken) =>
                {
                    return UsingSymbols.CreateFrom(compilation);
                });

            var csDeclarationProvider = context.CreateCsDeclarationProvider();

            var methodSourceBuildInputArgs = context.SyntaxProvider.ForAttributeWithMetadataName(StaticSourceAttribute.GetFullyQualifiedMetadataName(typeof(EnableNotificationSupportAttribute)), isPropertyDeclaration, transform)
                .Combine(enableNotificationSupportAttributeSymbol)
                .Combine(csDeclarationProvider)
                .Select((v, _) => (v.Left.Left.propertySymbol, v.Left.Left.enableNotificationSupportAttribute, usingSymbols: v.Left.Right, csDeclarationProvider: v.Right))
                .Select(toMethodSourceBuildInputArgs);

            context.RegisterSourceOutput(methodSourceBuildInputArgs, GenerateMethod);

            var propertyNameInputArgs = methodSourceBuildInputArgs
                .Collect()
                .SelectMany(toPropertyInputArgs);

            context.RegisterSourceOutput(propertyNameInputArgs, GenerateEventArg);


            static bool isPropertyDeclaration(SyntaxNode node, CancellationToken cancellationToken)
            {
                return node is PropertyDeclarationSyntax;
            }

            static (IPropertySymbol? propertySymbol, AttributeData enableNotificationSupportAttribute) transform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
            {
                var propertySymbol = context.TargetSymbol as IPropertySymbol;
                DebugSGen.AssertIsNotNull(propertySymbol);
                DebugSGen.Assert(context.Attributes.Length == 1);
                return (propertySymbol, context.Attributes[0]);
            }

            MethodSourceBuildInputs? toMethodSourceBuildInputArgs((IPropertySymbol? propertySymbol, AttributeData enableNotificationSupportAttribute, UsingSymbols usingSymbols, CsDeclarationProvider csDeclarationProvider) args, CancellationToken cancellationToken)
            {
                var (propertySymbol, enableNotificationSupportAttribute, usingSymbols, csDeclarationProvider) = args;

                if (propertySymbol is null)
                    return null;

                var result = new MethodSourceBuildInputs(propertySymbol, usingSymbols, enableNotificationSupportAttribute, csDeclarationProvider);

                return result;
            }

            IEnumerable<EventArgSourceBuilderInputs> toPropertyInputArgs(ImmutableArray<MethodSourceBuildInputs?> sourceBuildInputs, CancellationToken cancellationToken)
            {
                foreach (var propertiesInClass in sourceBuildInputs.Where(v => !cancellationToken.IsCancellationRequested && v is not null).ToLookup(v => v!.ContainingTypeDeclaration))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var properties = Enumerable.Empty<(string, PropertyEventArgClass)>()
                        .Concat(
                            propertiesInClass
                                .Where(v => v?.EnabledNotifyPropertyChanged == true)
                                .SelectMany(v => v!.PropertyEventArgNames.Values.Select(name => (name, PropertyEventArgClass.Changed)))
                        )
                        .Concat(
                            propertiesInClass
                                .Where(v => v?.EnabledNotifyPropertyChanging == true)
                                .SelectMany(v => v!.PropertyEventArgNames.Values.Select(name => (name, PropertyEventArgClass.Changing)))
                        )
                        .Distinct()
                        .ToImmutableArray();

                    yield return new (propertiesInClass.Key, properties);
                }
            }
        }



        void GenerateMethod(SourceProductionContext context, MethodSourceBuildInputs? sourceBuildInputs)
        {
            if (sourceBuildInputs is null) return;

            if (sourceBuildInputs.IsEventArgsOnly) return;

            try
            {
                using var sourceBuilder = new MethodSourceBuilder(context, sourceBuildInputs);

                sourceBuilder.Build();

                sourceBuilder.Commit();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        void GenerateEventArg(SourceProductionContext context, EventArgSourceBuilderInputs args)
        {
            try
            {
                using var sourceBuilder = new EventArgSourceBuilder(context, args);

                sourceBuilder.Build();

                sourceBuilder.Commit();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
