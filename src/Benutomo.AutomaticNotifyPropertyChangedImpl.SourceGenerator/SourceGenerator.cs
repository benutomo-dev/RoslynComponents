using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGeneratorCommons.CSharp.Declarations;
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

            var anotatedProperties = context.SyntaxProvider.CreateSyntaxProvider(IsAttributeAttachedPropertyDeclarationSystax, ToPropertySymbol);

            var methodSourceBuildInputArgs = anotatedProperties
                .Combine(enableNotificationSupportAttributeSymbol)
                .Combine(csDeclarationProvider)
                .Select((v, _) => (propertySymbol: v.Left.Left, usingSymbols: v.Left.Right, csDeclarationProvider: v.Right))
                .Select(ToMethodSourceBuildInputArgs);

            context.RegisterSourceOutput(methodSourceBuildInputArgs, GenerateMethod);

            var propertyNameInputArgs = methodSourceBuildInputArgs
                .Collect()
                .SelectMany(toPropertyInputArgs);

            context.RegisterSourceOutput(propertyNameInputArgs, GenerateEventArg);

            IEnumerable<EventArgSourceBuilderInputs> toPropertyInputArgs(ImmutableArray<MethodSourceBuildInputs?> sourceBuildInputs, CancellationToken cancellationToken)
            {
                foreach (var propertiesInClass in sourceBuildInputs.Where(v => !cancellationToken.IsCancellationRequested && v is not null).ToLookup(v => v!.ContainingType))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var properties = Enumerable.Empty<(string, PropertyEventArgClass)>()
                        .Concat(
                            propertiesInClass
                                .Where(v => v?.EnabledNotifyPropertyChanged == true)
                                .SelectMany(v => v!.PropertyEventArgNames.Select(name => (name, PropertyEventArgClass.Changed)))
                        )
                        .Concat(
                            propertiesInClass
                                .Where(v => v?.EnabledNotifyPropertyChanging == true)
                                .SelectMany(v => v!.PropertyEventArgNames.Select(name => (name, PropertyEventArgClass.Changing)))
                        )
                        .Distinct()
                        .ToImmutableArray();

                    yield return new (propertiesInClass.Key, properties);
                }
            }
        }

        bool IsAttributeAttachedPropertyDeclarationSystax(SyntaxNode node, CancellationToken cancellationToken)
        {
            //WriteLogLine("Predicate");

            return node is PropertyDeclarationSyntax
            {
                AttributeLists.Count: > 0
            };
        }

        IPropertySymbol? ToPropertySymbol(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            try
            {
                var propertyDeclarationSyntax = (PropertyDeclarationSyntax)context.Node;

                var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax, cancellationToken) as IPropertySymbol;

                return propertySymbol;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }

        MethodSourceBuildInputs? ToMethodSourceBuildInputArgs((IPropertySymbol? propertySymbol, UsingSymbols usingSymbols, CsDeclarationProvider csDeclarationProvider) v, CancellationToken ct)
        {
            var (propertySymbol, usingSymbols, csDeclarationProvider) = v;

            if (propertySymbol is null) return null;

            try
            {
                var enableNotificationSupportAttributeData = propertySymbol.GetAttributes().FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, usingSymbols.EnableNotificationSupportAttribute));
                if (enableNotificationSupportAttributeData is null)
                {
                    return null;
                }

                var result = new MethodSourceBuildInputs(propertySymbol, usingSymbols, enableNotificationSupportAttributeData, csDeclarationProvider);

                return result;
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
