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

            // Where句を使用しない。
            // https://github.com/dotnet/roslyn/issues/57991
            // 今は、Where句を使用するとSource GeneratorがVSでインクリメンタルに実行されたときに
            // 対象のコードの状態や編集内容などによって突然内部状態が壊れて機能しなくなる問題がおきる。

            var anotatedProperties = context.SyntaxProvider.CreateSyntaxProvider(IsAttributeAttachedPropertyDeclarationSystax, ToPropertySymbol);

            var methodSourceBuildInputArgs = anotatedProperties
                .Combine(enableNotificationSupportAttributeSymbol)
                .Select(ToMethodSourceBuildInputArgs);

            context.RegisterSourceOutput(methodSourceBuildInputArgs, GenerateMethod);

            var propertyNameInputArgs = methodSourceBuildInputArgs
                .Collect()
                .SelectMany(toPropertyInputArgs);

            context.RegisterSourceOutput(propertyNameInputArgs, GenerateEventArg);

            IEnumerable<EventArgSourceBuilderInputs> toPropertyInputArgs(ImmutableArray<MethodSourceBuildInputs?> sourceBuildInputs, CancellationToken cancellationToken)
            {
                foreach (var propertiesInClass in sourceBuildInputs.Where(v => !cancellationToken.IsCancellationRequested && v is not null).ToLookup(v => v!.ContainingTypeInfo))
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

        MethodSourceBuildInputs? ToMethodSourceBuildInputArgs((IPropertySymbol? Left, UsingSymbols Right) v, CancellationToken ct)
        {
            var propertySymbol = v.Left;
            var usingSymbols = v.Right;

            if (propertySymbol is null) return null;

            try
            {
                var enableNotificationSupportAttributeData = propertySymbol.GetAttributes().FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, usingSymbols.EnableNotificationSupportAttribute));
                if (enableNotificationSupportAttributeData is null)
                {
                    return null;
                }

                var result = new MethodSourceBuildInputs(propertySymbol, usingSymbols, enableNotificationSupportAttributeData);

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
                Span<char> initialBuffer = stackalloc char[80000];

                using var sourceBuilder = new MethodSourceBuilder(context, sourceBuildInputs, initialBuffer);

                sourceBuilder.Build();

                context.AddSource(sourceBuilder.HintName, sourceBuilder.SourceText);
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
                Span<char> initialBuffer = stackalloc char[80000];

                using var sourceBuilder = new EventArgSourceBuilder(context, args, initialBuffer);

                sourceBuilder.Build();

                context.AddSource(sourceBuilder.HintName, sourceBuilder.SourceText);
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
