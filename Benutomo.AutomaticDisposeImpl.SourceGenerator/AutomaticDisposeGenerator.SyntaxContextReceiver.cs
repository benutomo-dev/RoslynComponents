using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator
{
    public partial class AutomaticDisposeGenerator
    {
        class SyntaxContextReceiver : ISyntaxContextReceiver
        {
#pragma warning disable RS1024 // シンボルを正しく比較する
            /// <summary>
            /// コンパイル対象全体から作られる型のシンボルと構文木内でその型を定義しているClassDeclarationSyntaxの対応テーブル
            /// </summary>
            public Dictionary<ISymbol, List<ClassDeclarationSyntax>> ClassDeclarationTable { get; } = new(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // シンボルを正しく比較する

            /// <summary>
            /// コンパイル対象全体でAutomaticDisposeImpl属性がつけられている型定義の一覧
            /// </summary>
            public List<(ClassDeclarationSyntax syntaxNode, INamedTypeSymbol symbol)> AnotatedClassDeclarations { get; } = new();

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is not ClassDeclarationSyntax classDeclarationSyntax)
                {
                    return;
                }

                if (context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol namedTypeSymbol)
                {
                    return;
                }

                if (!ClassDeclarationTable.TryGetValue(namedTypeSymbol, out var classDeclarationSyntaxes))
                {
                    classDeclarationSyntaxes = new List<ClassDeclarationSyntax>();
                    ClassDeclarationTable.Add(namedTypeSymbol, classDeclarationSyntaxes);
                }

                classDeclarationSyntaxes.Add(classDeclarationSyntax);

                var isAutomaticDisposeImplAnnotationedDeclaration = classDeclarationSyntax.AttributeLists.SelectMany(attrList => attrList.Attributes)
                                                                                                         .Select(attributeSyntax => context.SemanticModel.GetTypeInfo(attributeSyntax).Type)
                                                                                                         .Any(IsAutomaticDisposeImplAnnotationTypeSymbol);

                if (!isAutomaticDisposeImplAnnotationedDeclaration)
                {
                    return;
                }

                AnotatedClassDeclarations.Add((classDeclarationSyntax, namedTypeSymbol));

                return;
            }
        }
    }
}
