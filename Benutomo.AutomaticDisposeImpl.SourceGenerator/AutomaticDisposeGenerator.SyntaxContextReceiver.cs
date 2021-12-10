using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

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
                if (context.Node is ClassDeclarationSyntax classDeclarationSyntax)
                {
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

                    bool isAutomaticDisposeImplAnnotationedDeclaration = false;
                    
                    foreach (var attributeList in classDeclarationSyntax.AttributeLists)
                    {
                        foreach (var attribute in attributeList.Attributes)
                        {
                            if (IsAutomaticDisposeImplAttribute(context.SemanticModel.GetTypeInfo(attribute).Type))
                            {
                                isAutomaticDisposeImplAnnotationedDeclaration = true;
                                goto LOOP_END_isAutomaticDisposeImplAnnotationedDeclaration;
                            }
                        }
                    }
                    LOOP_END_isAutomaticDisposeImplAnnotationedDeclaration:

                    if (isAutomaticDisposeImplAnnotationedDeclaration)
                    {
                        AnotatedClassDeclarations.Add((classDeclarationSyntax, namedTypeSymbol));
                    }
                }
            }
        }
    }
}
