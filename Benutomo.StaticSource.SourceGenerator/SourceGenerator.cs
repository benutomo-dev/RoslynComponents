using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator
{
    [Generator(LanguageNames.CSharp)]
    public partial class SourceGenerator : IIncrementalGenerator
    {
        static Regex NamespaceRegex = new Regex(@"\bnamespace\s+([a-zA-Z0-9_.]+)\s*[;{\r\n]", RegexOptions.Compiled);
        static Regex TypeRegex = new Regex(@"\b(?:class|struct|enum)\s+([a-zA-Z0-9_]+)", RegexOptions.Compiled);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(PostInitializationOutput);


            var source = context.SyntaxProvider
                .ForAttributeWithMetadataName("Benutomo.StaticSourceAttribute", predicate, transform)
                .Collect();

            context.RegisterSourceOutput(source, GenerateMethod);

            static bool predicate(SyntaxNode node, CancellationToken cancellationToken)
            {
                return true;
            }

            static GeneratorAttributeSyntaxContext transform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
            {
                return context;
            }
        }

        const string UsingsKeyword = "Usings";
        const string DirectivesKeyword = "Directives";
        const string AttributesKeyword = "Attributes";

        void PostInitializationOutput(IncrementalGeneratorPostInitializationContext context)
        {
            context.AddSource("StaticSourceAttribute.cs", $$"""
            using System;

            namespace Benutomo {
                [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
                internal class StaticSourceAttribute : Attribute
                {
                    public string Namespace { get; }

                    public string[]? Usings { get; set; }
            
                    public string[]? Directives { get; set; }
            
                    public string[]? Attributes { get; set; }

                    public StaticSourceAttribute(string @namespace) { Namespace = @namespace; }
            
                    public static string GetNamespace<T>() => GetNamespace(typeof(T));
            
                    public static string GetNamespace(Type type)
                    {
                        var attribute = Attribute.GetCustomAttribute(type, typeof(StaticSourceAttribute)) as StaticSourceAttribute;
            
                        if (attribute is null)
                        {
                            throw new InvalidOperationException();
                        }
            
                        return attribute.Namespace;
                    }
            
                    public static string GetFullyQualifiedMetadataName<T>() => GetFullyQualifiedMetadataName(typeof(T));
            
                    public static string GetFullyQualifiedMetadataName(Type type)
                    {
                        var attribute = Attribute.GetCustomAttribute(type, typeof(StaticSourceAttribute)) as StaticSourceAttribute;
            
                        if (attribute is null)
                        {
                            throw new InvalidOperationException();
                        }
            
                        return $"{attribute.Namespace}.{type.Name}";
                    }
            
                    public static string GetAttributeName<T>()
                    {
                        const string commonSuffix = "Attribute";
            
                        if (typeof(T).Name.EndsWith(commonSuffix, StringComparison.Ordinal))
                        {
                            return typeof(T).Name.Substring(0, typeof(T).Name.Length - commonSuffix.Length);
                        }
                        else
                        {
                            return typeof(T).Name;
                        }
                    }
                }
            }
            """);
        }

        void GenerateMethod(SourceProductionContext context, ImmutableArray<GeneratorAttributeSyntaxContext> args)
        {

            var sources = new List<(string name, string code)>(args.Length);

            foreach (var arg in args)
            {
                var attributeData = arg.Attributes.First();

                var @namespace = attributeData.ConstructorArguments[0].Value?.ToString() ?? "System";

                @namespace = Regex.Replace(@namespace, @"[^a-zA-Z0-9_.]", _ => "_");

                StringBuilder usingSection = new StringBuilder();
                StringBuilder directiveSection = new StringBuilder();
                StringBuilder attributeSection = new StringBuilder();

                StringBuilder documentationCommentSection = new StringBuilder();

                if (arg.TargetNode.SyntaxTree.GetRoot(context.CancellationToken) is CompilationUnitSyntax compilationUnitSyntax)
                {
                    foreach (var usingSyntax in compilationUnitSyntax.Usings)
                    {
                        usingSection.AppendLine(usingSyntax.WithoutTrivia().ToString());
                    }
                }

                foreach (var attributeArg in attributeData.NamedArguments)
                {
                    if (attributeArg.Key == UsingsKeyword)
                    {
                        foreach (var value in attributeArg.Value.Values)
                        {
                            usingSection.AppendLine(value.Value?.ToString());
                        }
                    }
                    else if (attributeArg.Key == DirectivesKeyword)
                    {
                        foreach (var value in attributeArg.Value.Values)
                        {
                            directiveSection.AppendLine(value.Value?.ToString());
                        }
                    }
                    else if (attributeArg.Key == AttributesKeyword)
                    {
                        foreach (var value in attributeArg.Value.Values)
                        {
                            if (attributeSection.Length > 0)
                            {
                                attributeSection.Append("\r\n");
                            }
                            attributeSection.Append("    ");
                            attributeSection.Append(value.Value?.ToString());
                        }
                    }
                }

                if (arg.TargetNode is BaseTypeDeclarationSyntax typeDeclarationSyntax)
                {
                    var attributeLists = new SyntaxList<AttributeListSyntax>();

                    foreach (var attributeList in typeDeclarationSyntax.DescendantTokens().TakeWhile(v => v != typeDeclarationSyntax.Identifier))
                    {
                        foreach (var trivia in attributeList.LeadingTrivia.Where(v => v.IsKind(SyntaxKind.SingleLineCommentTrivia)))
                        {
                            var value = trivia.ToString();

                            if (value.StartsWith("///", StringComparison.Ordinal))
                            {
                                if (documentationCommentSection.Length > 0)
                                {
                                    documentationCommentSection.Append("\r\n");
                                }
                                documentationCommentSection.Append("    ");
                                documentationCommentSection.Append(value);
                            }
                        }
                    }

                    var toFullStringResult = typeDeclarationSyntax
                        .WithAttributeLists(attributeLists)
                        .ToFullString();

                    sources.Add((arg.TargetSymbol.Name, $$"""
                        // <auto-generated />
                        {{usingSection}}
                        {{directiveSection}}
                        namespace {{@namespace}} {
                        {{documentationCommentSection}}
                        {{attributeSection}}
                        {{toFullStringResult}}
                        }
                        """));
                }
                else
                {
                    ;
                }
            }

            StringBuilder addSourceStatements = new StringBuilder();
            foreach (var source in sources)
            {
                addSourceStatements.AppendLine($$"""""""""""
                    context.AddSource("{{source.name}}.g.cs", """"""""""
                    {{source.code}}
                    """""""""");
                    """"""""""");
                addSourceStatements.AppendLine();
            }

            var staticSourceContent = $$"""
                using System;
                using Microsoft.CodeAnalysis;

                namespace StaticSources
                {
                    partial class StaticSource
                    {
                        public static void Register(IncrementalGeneratorInitializationContext context)
                        {
                            context.RegisterPostInitializationOutput(RegisterCore);
                        }

                        private static void RegisterCore(IncrementalGeneratorPostInitializationContext context)
                        {
                            {{addSourceStatements}}
                        }
                    }
                }
                """;

            context.AddSource($"StaticSource.cs", staticSourceContent);
        }
    }
}
