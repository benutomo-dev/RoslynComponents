using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Linq;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator
{
    [Generator]
    public partial class AutomaticDisposeGenerator : ISourceGenerator
    {
        internal const string AttributeDefinedNameSpace = "Benutomo";




        private const string AutomaticDisposeImplModeSource = @"
namespace Benutomo
{
    /// <summary>
    /// 破棄(<see cref=""System.IDisposable"" />,<see cref=""System.IAsyncDisposable"" />)をサポートするメンバを自動実装Disposeの対象とすることに関する振る舞いの指定。
    /// </summary>
    internal enum AutomaticDisposeImplMode
    {
        /// <summary>
        /// <see cref=""System.IDisposable"" />,<see cref=""System.IAsyncDisposable"" />を継承する型を持つメンバは暗黙的に自動Dispose呼び出しの対象となる。
        /// </summary>
        Implicit,

        /// <summary>
        /// <see cref=""System.IDisposable"" />,<see cref=""System.IAsyncDisposable"" />を継承する型を持つメンバは自動Dispose呼び出しの対象となる。
        /// </summary>
        Explicit,
    }
}
";

        internal const string AutomaticDisposeImplAttributeCoreName = "AutomaticDisposeImpl";
        internal const string AutomaticDisposeImplAttributeName = "AutomaticDisposeImplAttribute";
        internal const string AutomaticDisposeImplAttributeFullyQualifiedMetadataName = "Benutomo.AutomaticDisposeImplAttribute";
        internal const string AutomaticDisposeImplAttributeModeName = "Mode";
        private const string AutomaticDisposeImplAttributeSource = @"
using System;

#nullable enable

namespace Benutomo
{
    /// <summary>
    /// 指定したクラスに破棄(<see cref=""System.IDisposable"" />,<see cref=""System.IAsyncDisposable"" />)をサポートするメンバを破棄する<see cref=""System.IDisposable.Dispose"" />メソッドおよび<see cref=""System.IAsyncDisposable.DisposeAsync"" />メソッド(当該クラスに<see cref=""System.IAsyncDisposable"" />インターフェイスが含まれている場合のみ)を自動実装する。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal class AutomaticDisposeImplAttribute : Attribute
    {
        /// <summary>
        /// 自動破棄実装の既定動作を設定する。
        /// </summary>
        public AutomaticDisposeImplMode Mode { get; set; }
    }
}
";

        internal const string EnableAutomaticDisposeAttributeName = "EnableAutomaticDisposeAttribute";
        internal const string EnableAutomaticDisposeAttributeFullyQualifiedMetadataName = "Benutomo.EnableAutomaticDisposeAttribute";
        private const string EnableAutomaticDisposeAttributeSource = @"
using System;

#nullable enable

namespace Benutomo
{
    /// <summary>
    /// このオブジェクトの破棄と同時に自動的に<see cref=""System.IDisposable.Dispose"" />メソッドまたは<see cref=""System.IAsyncDisposable.DisposeAsync"" />メソッドを呼び出します。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal class EnableAutomaticDisposeAttribute : Attribute
    {
    }
}
";

        internal const string DisableAutomaticDisposeAttributeName = "DisableAutomaticDisposeAttribute";
        internal const string DisableAutomaticDisposeAttributeFullyQualifiedMetadataName = "Benutomo.DisableAutomaticDisposeAttribute";
        private const string DisableAutomaticDisposeAttributeSource = @"
using System;

#nullable enable

namespace Benutomo
{
    /// <summary>
    /// このメンバに対して、<see cref=""System.IDisposable.Dispose"" />メソッドまたは<see cref=""System.IAsyncDisposable.DisposeAsync"" />メソッドの自動呼出しは行いません。このオブジェクトで破棄するのが不適当であるかユーザ自身が<see cref=""System.IDisposable.Dispose"" />メソッドまたは<see cref=""System.IAsyncDisposable.DisposeAsync"" />メソッドの呼び出しを実装するメンバです。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal class DisableAutomaticDisposeAttribute : Attribute
    {
    }
}
";

#if false
        internal const string AutomaticDisposeImplModeAttributeName = "AutomaticDisposeImplModeAttribute";
        internal const string AutomaticDisposeImplModeAttributeFullyQualifiedMetadataName = "Benutomo.AutomaticDisposeImplModeAttribute";
        private const string AutomaticDisposeImplModeAttributeSource = @"
using System;

#nullable enable

namespace Benutomo
{
    /// <summary>
    /// メンバの破棄の自動実装の設定を明示する属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal class AutomaticDisposeImplModeAttribute : Attribute
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
#endif

        internal const string UnmanagedResourceReleaseMethodAttributeName = "UnmanagedResourceReleaseMethodAttribute";
        internal const string UnmanagedResourceReleaseMethodAttributeFullyQualifiedMetadataName = "Benutomo.UnmanagedResourceReleaseMethodAttribute";
        private const string UnmanagedResourceReleaseMethodAttributeSource = @"
using System;

#nullable enable

namespace Benutomo
{
    /// <summary>
    /// <see cref=""Benutomo.AutomaticDisposeImplAttribute""/>を利用しているクラスで、ユーザが実装するアンマネージドリソースの解放を行うメソッド(引数なしで戻り値はvoid)に付与する。このメソッドはこのオブジェクトのDispose()またはDisposeAsync()、デストラクタのいずれかが初めて実行された時に自動実装コードから呼び出される。この属性を付与したメソッドは、実装者の責任でGCのファイナライズスレッドから呼び出されても問題無いように実装しなければならないことに注意すること。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    internal class UnmanagedResourceReleaseMethodAttribute : Attribute
    {
        /// <summary>
        /// <inheritdoc cref=""Benutomo.UnmanagedResourceReleaseMethodAttribute""/>
        /// </summary>
        public UnmanagedResourceReleaseMethodAttribute() { }
    }
}
";

        internal const string ManagedObjectDisposeMethodAttributeName = "ManagedObjectDisposeMethodAttribute";
        internal const string ManagedObjectDisposeMethodAttributeFullyQualifiedMetadataName = "Benutomo.ManagedObjectDisposeMethodAttribute";
        private const string ManagedObjectDisposeMethodAttributeSource = @"
using System;

#nullable enable

namespace Benutomo
{
    /// <summary>
    /// <see cref=""Benutomo.AutomaticDisposeImplAttribute""/>を利用しているクラスで、ユーザが実装するマネージドオブジェクトを同期的な処理による破棄を行うメソッドに付与する。このメソッドはデストラクタからは呼び出されない。デストラクタからも呼び出される必要がある場合は<see cref=""Benutomo.UnmanagedResourceReleaseMethodAttribute"">を使用すること。この属性を付与するメソッドは引数なしで戻り値はvoidである必要がある。このメソッドはこのオブジェクトのDispose()が初めて実行された時に自動実装コードから呼び出される。ただし、このメソッドを所有するクラスがIAsyncDisposableも実装していて、かつ、DisposeAsync()によってこのオブジェクトが破棄された場合は、この属性が付与されているメソッドは呼び出されず、<see cref=""Benutomo.ManagedObjectAsyncDisposeMethodAttribute"">が付与されているメソッドが呼び出される。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    internal class ManagedObjectDisposeMethodAttribute : Attribute
    {
        /// <summary>
        /// <inheritdoc cref=""Benutomo.ManagedObjectDisposeMethodAttribute""/>
        /// </summary>
        public ManagedObjectDisposeMethodAttribute() { }
    }
}
";

        internal const string ManagedObjectAsyncDisposeMethodAttributeName = "ManagedObjectAsyncDisposeMethodAttribute";
        internal const string ManagedObjectAsyncDisposeMethodAttributeFullyQualifiedMetadataName = "Benutomo.ManagedObjectAsyncDisposeMethodAttribute";
        private const string ManagedObjectAsyncDisposeMethodAttributeSource = @"
using System;

#nullable enable

namespace Benutomo
{
    /// <summary>
    /// <see cref=""Benutomo.AutomaticDisposeImplAttribute""/>を利用しているクラスで、ユーザが実装するマネージドオブジェクトを非同期的な処理による破棄を行うメソッドに付与する。このメソッドはデストラクタからは呼び出されない。デストラクタからも呼び出される必要がある場合はデストラクタで必要な処理を全て同期的に行うようにした上で<see cref=""Benutomo.UnmanagedResourceReleaseMethodAttribute"">を使用すること。この属性を付与するメソッドは引数なしで戻り値は<see cref=""System.Threading.ValueTask"" />などawait可能な型である必要がある。このメソッドはこのオブジェクトのDisposeAsync()が初めて実行された時に自動実装コードから呼び出される。ただし、このメソッドを所有するクラスがIDisposableも実装していて、かつ、Dispose()によってこのオブジェクトが破棄された場合は、この属性が付与されているメソッドは呼び出されず、<see cref=""Benutomo.ManagedObjectDisposeMethodAttribute"">が付与されているメソッドが呼び出される。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    internal class ManagedObjectAsyncDisposeMethodAttribute : Attribute
    {
        /// <summary>
        /// <inheritdoc cref=""Benutomo.ManagedObjectAsyncDisposeMethodAttribute""/>
        /// </summary>
        public ManagedObjectAsyncDisposeMethodAttribute() { }
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
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource("AutomaticDisposeImplAttribute.cs", AutomaticDisposeImplAttributeSource);
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource("AutomaticDisposeImplMode.cs", AutomaticDisposeImplModeSource);
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource("EnableAutomaticDisposeAttribute.cs", EnableAutomaticDisposeAttributeSource);
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource("DisableAutomaticDisposeAttribute.cs", DisableAutomaticDisposeAttributeSource);
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource("UnmanagedResourceReleaseMethodAttribute.cs", UnmanagedResourceReleaseMethodAttributeSource);
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource("ManagedObjectDisposeMethodAttribute.cs", ManagedObjectDisposeMethodAttributeSource);
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource("ManagedObjectAsyncDisposeMethodAttribute.cs", ManagedObjectAsyncDisposeMethodAttributeSource);
        }

        public void Execute(GeneratorExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            if (context.SyntaxContextReceiver is not SyntaxContextReceiver syntaxContextReciever)
            {
                context.ReportDiagnostic(Diagnostic.Create(AutomaticDisposeAnalyzer.s_diagnosticDescriptor_SG9999, null));
                return;
            }

            var automaticDisposeImplAttributeSymbol = context.Compilation.GetTypeByMetadataName(AutomaticDisposeImplAttributeFullyQualifiedMetadataName);

            foreach (var anotatedClassDeclaration in syntaxContextReciever.AnotatedClassDeclarations)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (!anotatedClassDeclaration.syntaxNode.Modifiers.Any(modifier => modifier.ValueText == "partial"))
                    {
                        // AnalyzerでSG0001の報告を実装
                        continue;
                    }

                    if (!IsAssignableToIDisposable(anotatedClassDeclaration.symbol) && !IsAssignableToIAsyncDisposable(anotatedClassDeclaration.symbol))
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

        private static bool IsXSymbolImpl(ITypeSymbol? typeSymbol, string ns1, string typeName)
        {
            Debug.Assert(!ns1.Contains("."));
            Debug.Assert(!typeName.Contains("."));

            if (typeSymbol is null) return false;

            if (typeSymbol.Name != typeName) return false;

            var containingNamespaceSymbol = typeSymbol.ContainingNamespace;

            if (containingNamespaceSymbol is null) return false;

            if (containingNamespaceSymbol.Name != ns1) return false;

            if (containingNamespaceSymbol.ContainingNamespace is null) return false;

            if (!containingNamespaceSymbol.ContainingNamespace.IsGlobalNamespace) return false;

            return true;
        }


        private static bool IsAssignableToIXImpl(ITypeSymbol? typeSymbol, Func<ITypeSymbol, bool> isXTypeFunc, Func<ITypeSymbol, bool> isAssignableToXFunc)
        {
            if (typeSymbol is null) return false;

            if (isXTypeFunc(typeSymbol)) return true;

            if (typeSymbol.AllInterfaces.Any((Func<INamedTypeSymbol, bool>)isXTypeFunc)) return true;

            // ジェネリック型の型パラメータの場合は型パラメータの制約を再帰的に確認
            if (typeSymbol is ITypeParameterSymbol typeParameterSymbol && typeParameterSymbol.ConstraintTypes.Any(isAssignableToXFunc))
            {
                return true;
            }

            return false;
        }

        private static bool IsXAttributedMemberImpl(ISymbol? symbol, Func<INamedTypeSymbol,bool> isXAttributeSymbol)
        {
            if (symbol is null) return false;

            foreach (var attributeData in symbol.GetAttributes())
            {
                if (attributeData.AttributeClass is not null && isXAttributeSymbol(attributeData.AttributeClass))
                {
                    return true;
                }
            }

            return false;
        }


        internal static bool IsAutomaticDisposeImplAttribute(ITypeSymbol? typeSymbol) => IsXSymbolImpl(typeSymbol, AttributeDefinedNameSpace, AutomaticDisposeImplAttributeName);

        internal static bool IsEnableAutomaticDisposeAttribute(ITypeSymbol? typeSymbol) => IsXSymbolImpl(typeSymbol, AttributeDefinedNameSpace, EnableAutomaticDisposeAttributeName);

        internal static bool IsDisableAutomaticDisposeAttribute(ITypeSymbol? typeSymbol) => IsXSymbolImpl(typeSymbol, AttributeDefinedNameSpace, DisableAutomaticDisposeAttributeName);

        internal static bool IsUnmanagedResourceReleaseMethodAttribute(ITypeSymbol? typeSymbol) => IsXSymbolImpl(typeSymbol, AttributeDefinedNameSpace, UnmanagedResourceReleaseMethodAttributeName);

        internal static bool IsManagedObjectDisposeMethodAttribute(ITypeSymbol? typeSymbol) => IsXSymbolImpl(typeSymbol, AttributeDefinedNameSpace, ManagedObjectDisposeMethodAttributeName);

        internal static bool IsManagedObjectAsyncDisposeMethodAttribute(ITypeSymbol? typeSymbol) => IsXSymbolImpl(typeSymbol, AttributeDefinedNameSpace, ManagedObjectAsyncDisposeMethodAttributeName);

        internal static bool IsIDisposable(ITypeSymbol? typeSymbol) => IsXSymbolImpl(typeSymbol, "System", "IDisposable");

        internal static bool IsIAsyncDisposable(ITypeSymbol? typeSymbol) => IsXSymbolImpl(typeSymbol, "System", "IAsyncDisposable");

        internal static bool IsAssignableToIDisposable(ITypeSymbol? typeSymbol) => IsAssignableToIXImpl(typeSymbol, IsIDisposable, IsAssignableToIDisposable);

        internal static bool IsAssignableToIAsyncDisposable(ITypeSymbol? typeSymbol) => IsAssignableToIXImpl(typeSymbol, IsIAsyncDisposable, IsAssignableToIAsyncDisposable);

        internal static bool IsEnableAutomaticDisposeAttributedMember(ISymbol symbol) => IsXAttributedMemberImpl(symbol, IsEnableAutomaticDisposeAttribute);

        internal static bool IsDisableAutomaticDisposeAttributedMember(ISymbol symbol) => IsXAttributedMemberImpl(symbol, IsDisableAutomaticDisposeAttribute);

        internal static bool IsUnmanagedResourceReleaseMethodAttributedMember(ISymbol symbol) => IsXAttributedMemberImpl(symbol, IsUnmanagedResourceReleaseMethodAttribute);

        internal static bool IsManagedObjectDisposeMethodAttributedMember(ISymbol symbol) => IsXAttributedMemberImpl(symbol, IsManagedObjectDisposeMethodAttribute);

        internal static bool IsManagedObjectAsyncDisposeMethodAttributedMember(ISymbol symbol) => IsXAttributedMemberImpl(symbol, IsManagedObjectAsyncDisposeMethodAttribute);

        internal static bool IsAutomaticDisposeImplSubClass(INamedTypeSymbol? namedTypeSymbol)
        {
            if (namedTypeSymbol is null)
            {
                return false;
            }

            var isAutomaticDisposeImplAnnotationed = namedTypeSymbol.GetAttributes().Select(attrData => attrData.AttributeClass).Any(IsAutomaticDisposeImplAttribute);

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
