using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Benutomo.SourceGeneratorCommons;

internal static class SemanticModelExtensions
{
    public static ImmutableArray<IMethodSymbol> LookupExtensionMethods(this SemanticModel semanticModel, int position, string? name = null, ITypeSymbol? recieverType = null, CancellationToken cancellationToken = default)
    {
        var extensionMethods = ImmutableArray.CreateBuilder<IMethodSymbol>();

        var enclosingSymbol = semanticModel.GetEnclosingSymbol(position, cancellationToken);

        ITypeSymbol? eclosingTypeSymbol = enclosingSymbol switch
        {
            ITypeSymbol => (ITypeSymbol)enclosingSymbol,
            _ => enclosingSymbol?.ContainingType,
        };

        if (eclosingTypeSymbol is null)
        {
            return ImmutableArray<IMethodSymbol>.Empty;
        }

        // LookupNamespacesAndTypesなどは別名前空間の同名クラスの重複がシャドウイングされてしまうので
        // 拡張メソッドを拾い上げるためにはusingで取り込まれている名前空間毎に全ての型を明示的に列挙する必要がある。

        foreach (var typeSymbol in semanticModel.Compilation.GlobalNamespace.GetTypeMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();

            extractExtensionMethods(semanticModel, extensionMethods, typeSymbol, eclosingTypeSymbol, name, recieverType, cancellationToken);
        }

        foreach (var importScope in semanticModel.GetImportScopes(position, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var import in importScope.Imports)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (import.NamespaceOrType is not INamespaceSymbol namespaceSymbol)
                {
                    continue;
                }

                foreach (var typeSymbol in namespaceSymbol.GetTypeMembers())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    extractExtensionMethods(semanticModel, extensionMethods, typeSymbol, eclosingTypeSymbol, name, recieverType, cancellationToken);
                }
            }
        }

        if (extensionMethods.Count == extensionMethods.Capacity)
        {
            return extensionMethods.MoveToImmutable();
        }
        else
        {
            cancellationToken.ThrowIfCancellationRequested();

            return extensionMethods.ToImmutable();
        }

        static void extractExtensionMethods(SemanticModel semanticModel, ImmutableArray<IMethodSymbol>.Builder extensionMethods, INamedTypeSymbol extensionMethodSourceTypeSymbol, ITypeSymbol? enclosingTypeSymbolOfUsePosition, string? name, ITypeSymbol? recieverType, CancellationToken cancellationToken)
        {
            if (extensionMethodSourceTypeSymbol is not { MightContainExtensionMethods: true })
            {
                // 拡張メソッドを持たない型を除外
                return;
            }

            foreach (var member in extensionMethodSourceTypeSymbol.GetMembers())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (member is not IMethodSymbol { IsExtensionMethod: true } methodSymbol)
                {
                    // 拡張メソッドではないメンバを除外
                    continue;
                }

                if (name is not null && methodSymbol.Name != name)
                {
                    // 指定されている拡張メソッド名と一致しない拡張メソッドを除外
                    continue;
                }

                if (enclosingTypeSymbolOfUsePosition is not null)
                {
                    if (!semanticModel.Compilation.IsSymbolAccessibleWithin(methodSymbol, enclosingTypeSymbolOfUsePosition))
                    {
                        // 使用箇所において対象の拡張メソッドが不可視(アクセス不能)
                        continue;
                    }
                }

                if (recieverType is null)
                {
                    // レシーバーの型が指定されていない場合はここで確定

                    extensionMethods.Add(methodSymbol);
                    continue;
                }

                if (recieverType.IsAssignableTo(methodSymbol.Parameters[0].Type))
                {
                    // 拡張メソッドの第1引数(擬似this)の型にレシーバーが代入可能ならば
                    // レシーバーに対する拡張メソッドとして機能する

                    extensionMethods.Add(methodSymbol);
                    continue;
                }
            }
        }
    }
}
