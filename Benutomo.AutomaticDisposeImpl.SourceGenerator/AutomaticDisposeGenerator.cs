using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator
{
    [Generator]
    public partial class AutomaticDisposeGenerator : ISourceGenerator
    {
        internal const string AttribureProvideNameSpace = "Benutomo";

        internal const string AutomaticDisposeImplAttributeCoreName = "AutomaticDisposeImpl";
        internal const string AutomaticDisposeImplAttributeName = "AutomaticDisposeImplAttribute";
        internal const string AutomaticDisposeImplAttributeFullyQualifiedMetadataName = "Benutomo.AutomaticDisposeImplAttribute";
        internal const string AutomaticDisposeImplAttribute_DefaultMode = "DefaultMode";
        internal const string AutomaticDisposeImplAttribute_ReleaseUnmanagedResourcesMethod = "ReleaseUnmanagedResourcesMethod";
        internal const string AutomaticDisposeImplAttribute_SelfDisposeMethod = "SelfDisposeMethod";
        internal const string AutomaticDisposeImplAttribute_SelfDisposeAsyncMethod = "SelfDisposeAsyncMethod";

        internal const string AutomaticDisposeImplModeAttributeName = "AutomaticDisposeImplModeAttribute";
        internal const string AutomaticDisposeImplModeAttributeFullyQualifiedMetadataName = "Benutomo.AutomaticDisposeImplModeAttribute";

        /// <summary>
        /// <see cref="AutomaticDisposeImplMode"/>定数内の定義と一致させること
        /// </summary>
        const string AutomaticDisposeImplModeSource = @"
namespace Benutomo
{
    /// <summary>
    /// 破棄(<see cref=""System.IDisposable"" />,<see cref=""System.IAsyncDisposable"" />)をサポートするメンバを自動実装Disposeの対象とすることに関する振る舞いの指定。
    /// </summary>
    public enum AutomaticDisposeImplMode
    {
        /// <summary>
        /// デフォルト。メンバに指定した場合はクラス全体の設定と同じとする。クラス全体もデフォルトの場合は破棄(<see cref=""System.IDisposable"" />,<see cref=""System.IAsyncDisposable"" />)をサポートするメンバは<see cref=""Enable"" />を設定した場合と同様に扱う。
        /// </summary>
        Default,

        /// <summary>
        /// 自動実装されるDisposeの対象とする。
        /// </summary>
        Enable,

        /// <summary>
        /// 自動実装されるDisposeの対象外とする。Disableにした場合、あえて破棄をしてはならないような特殊ケースでない限り<see cref=""AutomaticDisposeImplAttribute"" />に<see cref=""AutomaticDisposeImplAttribute.SelfDisposeMethod"" />と必要に応じて<see cref=""AutomaticDisposeImplAttribute.SelfDisposeAsyncMethod"" />を指定し、そのメソッド内でこのメンバの破棄を実装すること。
        /// </summary>
        Disable,
    }
}
";

        const string AutomaticDisposeImplAttributeSource = @"
using System;

#nullable enable

namespace Benutomo
{
    /// <summary>
    /// 指定したクラスに破棄(<see cref=""System.IDisposable"" />,<see cref=""System.IAsyncDisposable"" />)をサポートするメンバを破棄する<see cref=""System.IDisposable.Dispose"" />メソッドおよび<see cref=""System.IAsyncDisposable.DisposeAsync"" />メソッド(当該クラスに<see cref=""System.IAsyncDisposable"" />インターフェイスが含まれている場合のみ)を自動実装する。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AutomaticDisposeImplAttribute : Attribute
    {
        /// <summary>
        /// メンバ毎の<see cref=""AutomaticDisposeImplMode"" />に<see cref=""AutomaticDisposeImplMode.Default"" />が指定されている場合にフォールバックする設定。
        /// </summary>
        public AutomaticDisposeImplMode DefaultMode { get; set; }

        /// <summary>
        /// アンマネージドリソースの手動解放を実装したメソッドを指定する。このメソッドは通常の破棄およびGCのファイナライズのタイミング自動実装側から呼び出される。一般的な場合において実装者はここに指定したメソッドを自分自身で呼び出す必要はない。
        /// </summary>
        public string? ReleaseUnmanagedResourcesMethod { get; set; }

        /// <summary>
        /// マネージドリソースの手動解放を実装するメソッド(引数なしで戻り値はvoid)を指定する。このメソッドは通常の破棄で自動実装側から呼び出される。一般的な場合において実装者はここに指定したメソッドを自分自身で呼び出す必要はない。明示的なDisposeがされずにGCのファイナライズによる解放が発生した場合にこのメソッドは呼び出されないことに注意。アンマネージドリソースの解放は<see cref=""ReleaseUnmanagedResourcesMethod""/>で指定するメソッドで行うこと。
        /// </summary>
        public string? SelfDisposeMethod { get; set; }

        /// <summary>
        /// マネージドリソースの非同期処理による手動解放を実装するメソッド(引数なしで戻り値は<see cref=""System.Threading.ValueTask"" />などawait可能な型)を指定する。このメソッドは通常の破棄で自動実装側から呼び出される。一般的な場合において実装者はここに指定したメソッドを自分自身で呼び出す必要はない。<see cref=""SelfDisposeAsyncMethod""/>と<see cref=""SelfDisposeMethod""/>で指定されたメソッドは破棄が同期的におこなわれたか非同期的におこなわれたかによってどちらか一方のみしか呼び出されないため、どちらが呼び出されても手動破棄の対象に漏れがないように実装すること。
        /// </summary>
        public string? SelfDisposeAsyncMethod { get; set; }
    }
}
";
        const string AutomaticDisposeImplModeAttributeSource = @"
using System;

#nullable enable

namespace Benutomo
{
    /// <summary>
    /// メンバの破棄の自動実装の設定を明示する属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AutomaticDisposeImplModeAttribute : Attribute
    {
        /// <summary>
        /// 自動実装Disposeの対象とするか否かに関する設定。
        /// </summary>
        public AutomaticDisposeImplMode Mode { get; }

        /// <summary>
        /// メンバの破棄の自動実装の設定を明示する属性。
        /// </summary>
        /// <param name=""mode"">自動実装Disposeの対象とするか否かに関する設定。</param>
        public AutomaticDisposeImplModeAttribute(AutomaticDisposeImplMode mode)
        {
            Mode = mode;
        }
    }
}
";

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(PostInitialization);
            context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
        }

        void PostInitialization(GeneratorPostInitializationContext context)
        {
            context.AddSource("AutomaticDisposeImplAttribute.cs", AutomaticDisposeImplAttributeSource);
            context.AddSource("AutomaticDisposeImplMode.cs", AutomaticDisposeImplModeSource);
            context.AddSource("AutomaticDisposeImplModeAttribute.cs", AutomaticDisposeImplModeAttributeSource);
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxContextReceiver syntaxContextReciever)
            {
                context.ReportDiagnostic(Diagnostic.Create(AutomaticDisposeAnalyzer.s_diagnosticDescriptor_SG9999, null));
                return;
            }

            var automaticDisposeImplAttributeSymbol = context.Compilation.GetTypeByMetadataName(AutomaticDisposeImplAttributeFullyQualifiedMetadataName);

            foreach (var anotatedClassDeclaration in syntaxContextReciever.AnotatedClassDeclarations)
            {
                try
                {
                    if (!anotatedClassDeclaration.syntaxNode.Modifiers.Any(modifier => modifier.ValueText == "partial"))
                    {
                        // AnalyzerでSG0001の報告を実装
                        continue;
                    }

                    if (!IsAssignableTypeSymbolToIDisposable(anotatedClassDeclaration.symbol) && !IsAssignableTypeSymbolToIAsyncDisposable(anotatedClassDeclaration.symbol))
                    {
                        // AnalyzerでSG0002の報告を実装
                        continue;
                    }

                    var automaticDisposeAttributeData = anotatedClassDeclaration.symbol.GetAttributes().SingleOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, automaticDisposeImplAttributeSymbol));
                    if (automaticDisposeAttributeData is null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(AutomaticDisposeAnalyzer.s_diagnosticDescriptor_SG9998, anotatedClassDeclaration.syntaxNode.Identifier.GetLocation(), anotatedClassDeclaration.symbol.Name));
                        continue;
                    }

                    var sourceBuilder = new SourceBuilder(context, anotatedClassDeclaration.symbol, automaticDisposeAttributeData);

                    sourceBuilder.Build();

                    context.AddSource(sourceBuilder.HintName, sourceBuilder.SourceText);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    context.ReportDiagnostic(Diagnostic.Create(AutomaticDisposeAnalyzer.s_diagnosticDescriptor_SG9998, anotatedClassDeclaration.syntaxNode.Identifier.GetLocation(), anotatedClassDeclaration.symbol.Name));
                    continue;
                }
            }
        }


        internal static bool IsAutomaticDisposeImplAnnotationTypeSymbol(ITypeSymbol? typeSymbol)
        {
            if (typeSymbol is null) return false;

            if (typeSymbol.Name != AutomaticDisposeImplAttributeName) return false;

            var containingNamespaceSymbol = typeSymbol.ContainingNamespace;

            if (containingNamespaceSymbol is null) return false;

            if (containingNamespaceSymbol.Name != AttribureProvideNameSpace) return false;

            if (containingNamespaceSymbol.ContainingNamespace is null) return false;

            if (!containingNamespaceSymbol.ContainingNamespace.IsGlobalNamespace) return false;

            return true;
        }


        internal static bool IsAutomaticDisposeImplModeAnnotationTypeSymbol(ITypeSymbol? typeSymbol)
        {
            if (typeSymbol is null) return false;

            if (typeSymbol.Name != AutomaticDisposeImplModeAttributeName) return false;

            var containingNamespaceSymbol = typeSymbol.ContainingNamespace;

            if (containingNamespaceSymbol is null) return false;

            if (containingNamespaceSymbol.Name != AttribureProvideNameSpace) return false;

            if (containingNamespaceSymbol.ContainingNamespace is null) return false;

            if (!containingNamespaceSymbol.ContainingNamespace.IsGlobalNamespace) return false;

            return true;
        }

        internal static bool IsAssignableTypeSymbolToIDisposable(ITypeSymbol? typeSymbol)
        {
            if (typeSymbol is null) return false;

            if (IsIDisposable(typeSymbol)) return true;

            if (typeSymbol.AllInterfaces.Any(IsIDisposable)) return true;

            // ジェネリック型の型パラメータの場合は型パラメータの制約を再帰的に確認
            if (typeSymbol is ITypeParameterSymbol typeParameterSymbol && typeParameterSymbol.ConstraintTypes.Any(IsAssignableTypeSymbolToIDisposable))
            {
                return true;
            }

            return false;
        }

        internal static bool IsAssignableTypeSymbolToIAsyncDisposable(ITypeSymbol? typeSymbol)
        {
            if (typeSymbol is null) return false;

            if (IsIAsyncDisposable(typeSymbol)) return true;

            if (typeSymbol.AllInterfaces.Any(IsIAsyncDisposable)) return true;

            // ジェネリック型の型パラメータの場合は型パラメータの制約を再帰的に確認
            if (typeSymbol is ITypeParameterSymbol typeParameterSymbol && typeParameterSymbol.ConstraintTypes.Any(IsAssignableTypeSymbolToIAsyncDisposable))
            {
                return true;
            }

            return false;
        }

        internal static bool IsIDisposable(ITypeSymbol? typeSymbol)
        {
            if (typeSymbol is null) return false;

            if (typeSymbol.Name != "IDisposable") return false;

            var containingNamespaceSymbol = typeSymbol.ContainingNamespace;

            if (containingNamespaceSymbol is null) return false;

            if (containingNamespaceSymbol.Name != "System") return false;

            if (containingNamespaceSymbol.ContainingNamespace is null) return false;

            if (!containingNamespaceSymbol.ContainingNamespace.IsGlobalNamespace) return false;

            return true;
        }

        internal static bool IsIAsyncDisposable(ITypeSymbol? typeSymbol)
        {
            if (typeSymbol is null) return false;

            if (typeSymbol.Name != "IAsyncDisposable") return false;

            var containingNamespaceSymbol = typeSymbol.ContainingNamespace;

            if (containingNamespaceSymbol is null) return false;

            if (containingNamespaceSymbol.Name != "System") return false;

            if (containingNamespaceSymbol.ContainingNamespace is null) return false;

            if (!containingNamespaceSymbol.ContainingNamespace.IsGlobalNamespace) return false;

            return true;
        }

        internal static bool IsAutomaticDisposeImplSubClass(INamedTypeSymbol? namedTypeSymbol)
        {
            if (namedTypeSymbol is null)
            {
                return false;
            }

            var isAutomaticDisposeImplAnnotationed = namedTypeSymbol.GetAttributes().Select(attrData => attrData.AttributeClass).Any(IsAutomaticDisposeImplAnnotationTypeSymbol);

            if (isAutomaticDisposeImplAnnotationed)
            {
                return true;
            }

            if (namedTypeSymbol.BaseType is null || namedTypeSymbol.BaseType.SpecialType == SpecialType.System_Object)
            {
                return false;
            }

            return IsAutomaticDisposeImplSubClass(namedTypeSymbol.BaseType);
        }

        internal static bool TryGetBaseClassOwnedIsDisposableSymbol(INamedTypeSymbol? namedTypeSymbol, out ISymbol symbol)
        {
            if (namedTypeSymbol is null || namedTypeSymbol.SpecialType == SpecialType.System_Object)
            {
                symbol = null!;
                return false;
            }

            symbol = namedTypeSymbol.GetMembers().FirstOrDefault(member => member.Name == "IsDisposed" && !member.IsStatic)!;

            if (symbol is null)
            {
                return TryGetBaseClassOwnedIsDisposableSymbol(namedTypeSymbol.BaseType, out symbol);
            }
            else
            {
                return true;
            }
        }
    }
}
