using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Benutomo.ImmutableCollectionSupport;

[Generator(LanguageNames.CSharp)]
public partial class IncrementalGenerator : IIncrementalGenerator
{
    private struct ExtensionMethodSource : IEquatable<ExtensionMethodSource>
    {
        public IMethodSymbol Method { get; }

        public ImmutableArray<ITypeSymbol?> ModifyArgFlags { get; }

        public INamedTypeSymbol ImmutableArrayTSymbol { get; }

        public ExtensionMethodSource(IMethodSymbol method, ImmutableArray<ITypeSymbol?> modifyArgFlags, INamedTypeSymbol immutableArrayTSymbol)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            ModifyArgFlags = modifyArgFlags;
            ImmutableArrayTSymbol = immutableArrayTSymbol ?? throw new ArgumentNullException(nameof(method));

            if (method.Parameters.Length != modifyArgFlags.Length)
            {
                throw new ArgumentException(null, nameof(modifyArgFlags));
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is ExtensionMethodSource source && Equals(source);
        }

        public bool Equals(ExtensionMethodSource other)
        {
            return SymbolEqualityComparer.Default.Equals(Method, other.Method) &&
                   ModifyArgFlags.Cast<ISymbol>().SequenceEqual(other.ModifyArgFlags, SymbolEqualityComparer.Default) &&
                   SymbolEqualityComparer.Default.Equals(ImmutableArrayTSymbol, other.ImmutableArrayTSymbol);
        }

        public override int GetHashCode()
        {
            int hashCode = -808452536;
            hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(Method);
            foreach (var item in ModifyArgFlags) hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(item);
            hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(ImmutableArrayTSymbol);
            return hashCode;
        }
    }

    private sealed record Symbols(INamedTypeSymbol ImmutableArrayT);

    internal const string AutoGenImmutableArrayExtensionsClassName = "AuteGenImmutableArrayExtensions";

    internal const string BoxlessAsReadOnlyListMethodName = "BoxlessAsReadOnlyList";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        StaticSources.StaticSource.Register(context);

        var extensionNameSpaceSource = context.CompilationProvider
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select((v, ct) => (compilation: v.Left, configOptions: v.Right))
            .Select((v, ct) => (v.compilation, extensionNameSpace: $"Benutomo.ImmutableArraySupport.AutoGenExtensions.{v.compilation.AssemblyName}"));

        context.RegisterSourceOutput(extensionNameSpaceSource, (context, args) =>
        {
            context.AddSource($"{AutoGenImmutableArrayExtensionsClassName}.cs", $$"""
                global using {{args.extensionNameSpace}};

                #pragma warning disable CS0436
                #nullable enable

                namespace {{args.extensionNameSpace}}
                {
                    /// <summary>
                    /// Equalsメソッドの自動実装でこの属性を付与したメンバの等価性判定に使用する<see cref="IEqualityComparer{T}"。/>
                    /// </summary>
                    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
                    internal static partial class {{AutoGenImmutableArrayExtensionsClassName}}
                    {
                        /// <summary>
                        /// <see cref="ImmutableArray{T}"/>をボックス化を回避して<see cref="global::System.Collections.Generic.IReadOnlyList{T}"/>に変換する。
                        /// </summary>
                        /// <typeparam name="T"></typeparam>
                        /// <param name="immutableArray"></param>
                        /// <returns></returns>
                        public static global::System.Collections.Generic.IReadOnlyList<T> {{BoxlessAsReadOnlyListMethodName}}<T>(this global::System.Collections.Immutable.ImmutableArray<T> immutableArray)
                        {
                            if (immutableArray.IsDefaultOrEmpty) return global::System.Array.Empty<T>();

                            if (global::System.Runtime.InteropServices.MemoryMarshal.TryGetArray(immutableArray.AsMemory(), out var innerArray) && innerArray.Offset == 0 && innerArray.Array?.Length == immutableArray.Length)
                            {
                                return innerArray.Array;
                            }
                            else
                            {
                                global::System.Diagnostics.Debug.Fail("ImmutableArrayの内部配列の取得に失敗");
                #pragma warning disable {{ImmutableCollectionSupportAnalyzer.s_diagnosticDescriptor_ImmutableCollection0002.Id}}
                                return (global::System.Collections.Generic.IReadOnlyList<T>)immutableArray;
                #pragma warning restore {{ImmutableCollectionSupportAnalyzer.s_diagnosticDescriptor_ImmutableCollection0002.Id}}
                            }
                        }
                    }
                }
                """);
        });

        var compilationSource = extensionNameSpaceSource
            .Select((arg, ct) =>
            {
                var boxlessAsReadOnlyListMockSyntaxTree = CSharpSyntaxTree.ParseText($$"""
                    global using {{arg.extensionNameSpace}};
                    namespace {{arg.extensionNameSpace}}
                    {
                        internal static partial class {{AutoGenImmutableArrayExtensionsClassName}}
                        {
                            public static global::System.Collections.Generic.IReadOnlyList<T> {{BoxlessAsReadOnlyListMethodName}}<T>(this global::System.Collections.Immutable.ImmutableArray<T> immutableArray)
                            {
                                return null;
                            }
                        }
                    }
                    """);

                var effectiveCompilation = arg.compilation.AddSyntaxTrees(boxlessAsReadOnlyListMockSyntaxTree);

                var symbols = default(Symbols);

                var immutableArrayT = effectiveCompilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableArray`1");

                if (immutableArrayT is not null)
                {
                    symbols = new Symbols(immutableArrayT);
                }

                return (effectiveCompilation, symbols, arg.extensionNameSpace);
            });

        var source = context.SyntaxProvider.CreateSyntaxProvider(firstPredicate, (v, ct) => v)
            .Combine(compilationSource)
            .Select((v, ct) => (v.Left, v.Right.effectiveCompilation, v.Right.symbols, v.Right.extensionNameSpace))
            .Select(swapCompilationAndGetInvocationOperation)
            .Where(predicate)
            .Collect()
            .SelectMany(extractGenerateExtensionMethodSignatures)
            .Combine(extensionNameSpaceSource)
            .Select((v, ct) => (compilation: v.Right.compilation, namespaceName: v.Left.namespaceName, defaultNameSpceName: v.Right.extensionNameSpace, v.Left.methodName, v.Left.extensionMethodSources));

        context.RegisterSourceOutput(source, generate);


        static bool firstPredicate(SyntaxNode node, CancellationToken cancellationToken)
        {
            if (node is not MemberAccessExpressionSyntax memberAccessExpressionSyntax)
            {
                return false;
            }

            if (memberAccessExpressionSyntax.Parent is not InvocationExpressionSyntax invocationExpressionSyntax)
            {
                return false;
            }

            return true;
        }

        static (Compilation compilation, Symbols? symbols, string extensionNameSpace, IInvocationOperation operation) swapCompilationAndGetInvocationOperation((GeneratorSyntaxContext refereceContext, Compilation effectiveCompilation, Symbols? symbols, string extensionNameSpace) args, CancellationToken cancellationToken)
        {
            var memberAccessExpressionSyntaxOriginal = (MemberAccessExpressionSyntax)args.refereceContext.Node;

            var syntaxTree = args.effectiveCompilation.SyntaxTrees.FirstOrDefault(v => v.FilePath == memberAccessExpressionSyntaxOriginal.SyntaxTree.FilePath);

            if (syntaxTree is null)
            {
                Debug.Fail(null);
                return default;
            }

            var memberAccessExpressionSyntax = syntaxTree.GetRoot().FindNode(memberAccessExpressionSyntaxOriginal.Span) as MemberAccessExpressionSyntax;

            if (memberAccessExpressionSyntax is null)
            {
                Debug.Fail(null);
                return default;
            }

            var invocationExpressionSyntax = (InvocationExpressionSyntax)memberAccessExpressionSyntax.Parent!;

            var semanticsModel = args.effectiveCompilation.GetSemanticModel(invocationExpressionSyntax.SyntaxTree);

            var operation = semanticsModel.GetOperation(invocationExpressionSyntax, cancellationToken) as IInvocationOperation;

            if (operation is null)
            {
                //Debug.Fail(null);
                return default;
            }

            return (args.effectiveCompilation, args.symbols, args.extensionNameSpace, operation);
        }

        static bool predicate((Compilation compilation, Symbols? symbols, string namespaceName, IInvocationOperation? operation) arg)
        {
            if (arg.operation is null) return false;
            if (arg.symbols is null) return false;

            if (!arg.operation.TargetMethod.IsExtensionMethod)
            {
                return false;
            }

            if (arg.operation.TargetMethod.ContainingType.IsFileLocal)
            {
                return false;
            }

            foreach (var argument in arg.operation.Arguments)
            {
                if (argument.Value is IConversionOperation conversionOperation)
                {
                    if (SymbolEqualityComparer.Default.Equals(conversionOperation.Operand.Type?.OriginalDefinition, arg.symbols.ImmutableArrayT))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static ImmutableArray<(string? namespaceName, string methodName, ImmutableArray<ExtensionMethodSource> extensionMethodSources)> extractGenerateExtensionMethodSignatures(ImmutableArray<(Compilation compilation, Symbols? symbols, string namespaceName, IInvocationOperation operation)> collection, CancellationToken cancellationToken)
        {
            var dictionary = new Dictionary<(string? namespaceName, string methodName), HashSet<ExtensionMethodSource>>();

            foreach (var item in collection)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (item.symbols is null)
                {
                    Debug.Fail(null);
                    continue;
                }

                if (item.operation?.TargetMethod is not IMethodSymbol method)
                {
                    Debug.Fail(null);
                    continue;
                }

                var flags = ImmutableArray.CreateBuilder<ITypeSymbol?>(method.Parameters.Length);

                for (int i = 0; i < item.operation.Arguments.Length; i++)
                {
                    var argument = item.operation.Arguments[i];

                    if (argument.Value is IConversionOperation conversionOperation)
                    {
                        if (SymbolEqualityComparer.Default.Equals(conversionOperation.Operand.Type?.OriginalDefinition, item.symbols.ImmutableArrayT))
                        {
                            if (item.operation.TargetMethod.OriginalDefinition.Parameters[i].Type is INamedTypeSymbol argumentType && argumentType.IsGenericType)
                            {
                                flags.Add(item.symbols.ImmutableArrayT.Construct(argumentType.TypeArguments[0]));
                            }
                            else
                            {
                                flags.Add(conversionOperation.Operand.Type);
                            }
                            continue;
                        }
                    }

                    flags.Add(null);
                }

                var source = new ExtensionMethodSource(method.OriginalDefinition, flags.MoveToImmutable(), item.symbols.ImmutableArrayT);

                string? namespaceName;
                if (method.DeclaringSyntaxReferences.Length == 0)
                {
                    namespaceName = null;
                }
                else
                {
                    var namespaceBuilder = new StringBuilder();
                    namespaceBuilder.AppendFullNamespace(source.Method.ContainingType.ContainingNamespace);
                    namespaceName = namespaceBuilder.ToString();
                }

                var dictionaryKey = (namespaceName, source.Method.Name);

                if (!dictionary.TryGetValue(dictionaryKey, out var sources))
                {
                    sources = new HashSet<ExtensionMethodSource>();
                    dictionary.Add(dictionaryKey, sources);
                }

                sources.Add(source);
            }

            return dictionary.Select(v => (v.Key.namespaceName, v.Key.methodName, v.Value.ToImmutableArray())).ToImmutableArray();
        }

        static void generate(SourceProductionContext context, (Compilation compilation, string? namespaceName, string defaultNameSpceName, string name, ImmutableArray<ExtensionMethodSource> extensionMethodSources) arg)
        {
            string namespaceName;
            string hintName;
            bool extensionOfSelfDefinedMethod;

            if (arg.namespaceName is null)
            {
                namespaceName = arg.defaultNameSpceName;
                hintName = $"{AutoGenImmutableArrayExtensionsClassName}.{arg.name}.cs";
                extensionOfSelfDefinedMethod = false;
            }
            else
            {
                namespaceName = arg.namespaceName;
                hintName = $"{AutoGenImmutableArrayExtensionsClassName}.{arg.name}.{namespaceName}.cs";
                extensionOfSelfDefinedMethod = true;
            }

            using var sourceBuilder = new SourceBuilderEx(context, hintName);

            using (sourceBuilder.BeginBlock($"namespace {namespaceName}"))
            {
                string classDefinition;
                if (extensionOfSelfDefinedMethod)
                {
                    classDefinition = $"public static partial class {AutoGenImmutableArrayExtensionsClassName}";
                }
                else
                {
                    classDefinition = $"internal static partial class {AutoGenImmutableArrayExtensionsClassName}";
                }

                using (sourceBuilder.BeginBlock(classDefinition))
                {
                    foreach (var extensionMethodSource in arg.extensionMethodSources)
                    {
                        string returnType;
                        if (extensionMethodSource.Method.ReturnsVoid)
                        {
                            returnType = "void";
                        }
                        else
                        {
                            var builder = new StringBuilder();
                            builder.AppendFullTypeNameWithNamespaceAlias(extensionMethodSource.Method.ReturnType);
                            returnType = builder.ToString();
                        }

                        var typeParameters = string.Join(", ", extensionMethodSource.Method.TypeParameters.Select(v => v.Name));

                        var args = string.Join(", ", extensionMethodSource.Method.Parameters.Select((v, i) =>
                        {
                            var parameter = extensionMethodSource.Method.Parameters[i];

                            ITypeSymbol argType;
                            if (extensionMethodSource.ModifyArgFlags[i] is { } _argType)
                            {
                                //var originalArgType = (INamedTypeSymbol)parameter.Type;
                                //argType = extensionMethodSource.ImmutableArrayTSymbol.Construct(originalArgType.TypeArguments[0]);
                                argType = _argType;
                            }
                            else
                            {
                                argType = v.Type;
                            }

                            var argument = new StringBuilder();

                            switch (parameter.RefKind)
                            {
                                case RefKind.Ref:
                                    argument.Append("ref ");
                                    break;
                                case RefKind.In:
                                    argument.Append("in ");
                                    break;
                                case RefKind.Out:
                                    argument.Append("out ");
                                    break;
                            }
                            argument.AppendFullTypeNameWithNamespaceAlias(argType);
                            argument.Append(" @");
                            argument.Append(v.Name);

                            if (parameter.HasExplicitDefaultValue)
                            {
                                argument.Append(" = ");

                                if (parameter.ExplicitDefaultValue is null)
                                {
                                    if (argType.IsValueType)
                                    {
                                        argument.Append("default");
                                    }
                                    else
                                    {
                                        argument.Append("null");
                                    }
                                }
                                else
                                {
                                    if (argType.TypeKind == TypeKind.Enum && argType.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(v => v.ConstantValue == parameter.ExplicitDefaultValue) is { } enumMember)
                                    {
                                        argument.AppendFullTypeNameWithNamespaceAlias(argType);
                                        argument.Append('.');
                                        argument.Append(enumMember.Name);
                                    }
                                    else if (parameter.ExplicitDefaultValue is string stringDefaultValue)
                                    {
                                        argument.Append('"');
                                        argument.Append(stringDefaultValue);
                                        argument.Append('"');
                                    }
                                    else
                                    {
                                        argument.Append('(');
                                        argument.AppendFullTypeNameWithNamespaceAlias(argType);
                                        argument.Append(')');
                                        argument.Append(parameter.ExplicitDefaultValue);
                                    }
                                }
                            }

                            return argument.ToString();
                        }));

                        var callArgs = string.Join(", ", extensionMethodSource.Method.Parameters.Select((v, i) =>
                        {
                            var argValue = new StringBuilder();

                            switch (extensionMethodSource.Method.Parameters[i].RefKind)
                            {
                                case RefKind.Ref:
                                    argValue.Append("ref ");
                                    break;
                                case RefKind.Out:
                                    argValue.Append("out ");
                                    break;
                            }

                            if (extensionMethodSource.ModifyArgFlags[i] is not null)
                            {
                                argValue.Append("global::");
                                argValue.Append(arg.defaultNameSpceName);
                                argValue.Append('.');
                                argValue.Append(AutoGenImmutableArrayExtensionsClassName);
                                argValue.Append('.');
                                argValue.Append(BoxlessAsReadOnlyListMethodName);
                                argValue.Append("(@");
                                argValue.Append(v.Name);
                                argValue.Append(')');
                            }
                            else
                            {
                                argValue.Append('@');
                                argValue.Append(v.Name);
                            }

                            return argValue.ToString();
                        }));


                        /// <inheritdoc cref="System.Linq.Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, int, TResult})"/>

                        sourceBuilder.PutIndentSpace();
                        sourceBuilder.Append("/// <inheritdoc cref=\"");
                        sourceBuilder.AppendCref(extensionMethodSource.Method);
                        sourceBuilder.AppendLine("\"/>");

                        string methodAccessibility;
                        if (true
                            && extensionOfSelfDefinedMethod
                            && extensionMethodSource.Method.DeclaredAccessibility == Accessibility.Public
                            && extensionMethodSource.Method.ContainingType.DeclaredAccessibility == Accessibility.Public
                            )
                        {
                            methodAccessibility = "public";
                        }
                        else
                        {
                            methodAccessibility = "internal";
                        }

                        string methodDefinition;
                        if (string.IsNullOrEmpty(typeParameters))
                        {
                            methodDefinition = $"{methodAccessibility} static {returnType} {extensionMethodSource.Method.Name}(this {args})";
                        }
                        else
                        {
                            methodDefinition = $"{methodAccessibility} static {returnType} {extensionMethodSource.Method.Name}<{typeParameters}>(this {args})";
                        }

                        using (sourceBuilder.BeginBlock(methodDefinition))
                        {
                            sourceBuilder.PutIndentSpace();
                            sourceBuilder.AppendLine("#pragma warning disable ImmutableCollection0003");

                            sourceBuilder.PutIndentSpace();
                            if (!extensionMethodSource.Method.ReturnsVoid)
                            {
                                sourceBuilder.Append("return ");
                            }
                            sourceBuilder.AppendFullTypeNameWithNamespaceAlias(extensionMethodSource.Method.ContainingType);
                            sourceBuilder.Append(".");
                            sourceBuilder.Append(extensionMethodSource.Method.Name);
                            if (!string.IsNullOrEmpty(typeParameters))
                            {
                                sourceBuilder.Append("<");
                                sourceBuilder.Append(typeParameters);
                                sourceBuilder.Append(">");
                            }
                            sourceBuilder.Append("(");
                            sourceBuilder.Append(callArgs);
                            sourceBuilder.AppendLine(");");

                            sourceBuilder.PutIndentSpace();
                            sourceBuilder.AppendLine("#pragma warning restore ImmutableCollection0003");
                        }
                    }
                }
            }

            sourceBuilder.Commit();
        }
    }
}
