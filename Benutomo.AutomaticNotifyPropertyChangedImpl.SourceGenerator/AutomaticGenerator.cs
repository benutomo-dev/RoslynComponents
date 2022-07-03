using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator
{
    record struct GenerateEventArgInputs(TypeDefinitionInfo ContainingTypeInfo, ImmutableArray<(string Name, PropertyEventArgClass EventArgClass)> Properties);

    [Generator]
    public partial class AutomaticGenerator : IIncrementalGenerator
    {
#if DEBUG
        static StreamWriter _streamWriter;
        static AutomaticGenerator()
        {
            Directory.CreateDirectory(@"c:\var\log\AutomaticNotifyPropertyChangedImpl");
            var proc = Process.GetCurrentProcess();
            _streamWriter = new StreamWriter($@"c:\var\log\AutomaticNotifyPropertyChangedImpl\{DateTime.Now:yyyyMMddHHmmss}_{proc.Id}.txt");
            _streamWriter.WriteLine(proc);
        }

        [Conditional("DEBUG")]
        static void WriteLogLine(string line)
        {
            lock (_streamWriter)
            {
                _streamWriter.WriteLine(line);
                _streamWriter.Flush();
            }
        }
#else
        [Conditional("DEBUG")]
        static void WriteLogLine(string line)
        {
        }
#endif

        internal const string AttributeDefinedNameSpace = "Benutomo";

        internal const string EventToObservableName = "EventToObservable";
        internal const string EventToObservableFullyQualifiedMetadataName = "Benutomo.Internal.EventToObservable";
        private const string EventToObservableSource = @"
using System;

#nullable enable
#pragma warning disable CS0436 // このソース内の型が、別の参照アセンブリのの型と競合している場合の警告を抑止(この警告は同じソースジェネレータを使用している参照プロジェクトのInternalVisibleTo属性に指定されている場合に発生する)

namespace Benutomo.Internal
{
    internal class EventToObservable<T> : global::System.IObservable<T>
    {
        global::System.Action<global::System.EventHandler> _addHandler;
        global::System.Action<global::System.EventHandler> _removeHandler;
        global::System.Func<T> _valueGetter;
        bool _pushValueAtSubscribed;

        public EventToObservable(global::System.Action<EventHandler> addHandler, global::System.Action<EventHandler> removeHandler, global::System.Func<T> valueGetter, bool pushValueAtSubscribed)
        {
            _addHandler = addHandler ?? throw new global::System.ArgumentNullException(nameof(addHandler));
            _removeHandler = removeHandler ?? throw new global::System.ArgumentNullException(nameof(removeHandler));
            _valueGetter = valueGetter ?? throw new global::System.ArgumentNullException(nameof(valueGetter));
            _pushValueAtSubscribed = pushValueAtSubscribed;
        }

        public global::System.IDisposable Subscribe(global::System.IObserver<T> observer) => new Proxy(_addHandler, _removeHandler, _valueGetter, observer, _pushValueAtSubscribed);

        private class Proxy : global::System.IDisposable
        {
            global::System.Action<global::System.EventHandler>? _removeHandler;
            global::System.Func<T> _valueGetter;
            global::System.IObserver<T>? _observer;

            public Proxy(global::System.Action<global::System.EventHandler> addHandler, global::System.Action<global::System.EventHandler> removeHandler, global::System.Func<T> valueGetter, global::System.IObserver<T> observer, bool pushValueAtSubscribed)
            {
                addHandler(EventHandler);
                _removeHandler = removeHandler;
                _valueGetter = valueGetter;
                _observer = observer;

                if (pushValueAtSubscribed)
                {
                    PushValue();
                }
            }

            public void Dispose() => Close(exception: null);

            void PushValue()
            {
                if (global::System.Threading.Volatile.Read(ref _observer) is { } observer)
                {
                    // まだOnComplete/OnErrorの呼び出しに至っていない

                    T value = default!;

                    try
                    {
                        value = _valueGetter();
                    }
                    catch (global::System.Exception exception)
                    {
                        Close(exception);
                        return;
                    }

                    if (global::System.Threading.Volatile.Read(ref _removeHandler) is not null)
                    {
                        // _valueGetterの間にCloseが呼び出されていない(厳密にOnComplete/OnError後のOnNextを防止できる排他ではないがほぼ問題ないはず)

                        observer.OnNext(value);
                    }
                }
            }

            void Close(global::System.Exception? exception)
            {
                var removeHandler = global::System.Threading.Interlocked.Exchange(ref _removeHandler, null);
                removeHandler?.Invoke(EventHandler);

                var observer = global::System.Threading.Interlocked.Exchange(ref _observer, null);

                if (exception is null)
                {
                    observer?.OnCompleted();
                }
                else
                {
                    observer?.OnError(exception);
                }
            }

            void EventHandler(object? source, global::System.EventArgs args) => PushValue();
        }
    }
}
";

        internal const string EnableNotificationSupportAttributeName = "EnableNotificationSupportAttribute";
        internal const string EnableNotificationSupportAttributeFullyQualifiedMetadataName = "Benutomo.EnableNotificationSupportAttribute";
        private const string EnableNotificationSupportAttributeSource = @"
using System;

#nullable enable
#pragma warning disable CS0436 // このソース内の型が、別の参照アセンブリのの型と競合している場合の警告を抑止(この警告は同じソースジェネレータを使用している参照プロジェクトのInternalVisibleTo属性に指定されている場合に発生する)

namespace Benutomo
{
    /// <summary>
    /// Todo
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class EnableNotificationSupportAttribute : Attribute
    {
        public bool EventArgsOnly { get; set; } = false;
    }
}
";

        internal const string ChangedEventAttributeName = "ChangedEventAttribute";
        internal const string ChangedEventAttributeFullyQualifiedMetadataName = "Benutomo.ChangedEventAttribute";
        private const string ChangedEventAttributeSource = @"
using System;

#nullable enable
#pragma warning disable CS0436 // このソース内の型が、別の参照アセンブリのの型と競合している場合の警告を抑止(この警告は同じソースジェネレータを使用している参照プロジェクトのInternalVisibleTo属性に指定されている場合に発生する)

namespace Benutomo
{
    /// <summary>
    /// Todo
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class ChangedEventAttribute : Attribute
    {
        public ChangedEventAttribute() {}

        public ChangedEventAttribute(NotificationAccessibility accessibility) {}

        public ChangedEventAttribute(ExplicitInterfaceImplementation enableExplicitInterfaceImplementation) {}

        public ChangedEventAttribute(NotificationAccessibility accessibility, ExplicitInterfaceImplementation enableExplicitInterfaceImplementation) {}
    }
}
";

        internal const string ChangingEventAttributeName = "ChangingEventAttribute";
        internal const string ChangingEventAttributeFullyQualifiedMetadataName = "Benutomo.ChangingEventAttribute";
        private const string ChangingEventAttributeSource = @"
using System;

#nullable enable
#pragma warning disable CS0436 // このソース内の型が、別の参照アセンブリのの型と競合している場合の警告を抑止(この警告は同じソースジェネレータを使用している参照プロジェクトのInternalVisibleTo属性に指定されている場合に発生する)

namespace Benutomo
{
    /// <summary>
    /// Todo
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class ChangingEventAttribute : Attribute
    {
        public ChangingEventAttribute() {}

        public ChangingEventAttribute(NotificationAccessibility accessibility) {}

        public ChangingEventAttribute(ExplicitInterfaceImplementation enableExplicitInterfaceImplementation) {}

        public ChangingEventAttribute(NotificationAccessibility accessibility, ExplicitInterfaceImplementation enableExplicitInterfaceImplementation) {}
    }
}
";

        internal const string ChangedObservableAttributeName = "ChangedObservableAttribute";
        internal const string ChangedObservableAttributeFullyQualifiedMetadataName = "Benutomo.ChangedObservableAttribute";
        private const string ChangedObservableAttributeSource = @"
using System;

#nullable enable
#pragma warning disable CS0436 // このソース内の型が、別の参照アセンブリのの型と競合している場合の警告を抑止(この警告は同じソースジェネレータを使用している参照プロジェクトのInternalVisibleTo属性に指定されている場合に発生する)

namespace Benutomo
{
    /// <summary>
    /// Todo
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class ChangedObservableAttribute : Attribute
    {
        public ChangedObservableAttribute() {}

        public ChangedObservableAttribute(NotificationAccessibility Accessibility) {}
    }
}
";

        internal const string ChangingObservableAttributeName = "ChangingObservableAttribute";
        internal const string ChangingObservableAttributeFullyQualifiedMetadataName = "Benutomo.ChangingObservableAttribute";
        private const string ChangingObservableAttributeSource = @"
using System;

#nullable enable
#pragma warning disable CS0436 // このソース内の型が、別の参照アセンブリのの型と競合している場合の警告を抑止(この警告は同じソースジェネレータを使用している参照プロジェクトのInternalVisibleTo属性に指定されている場合に発生する

namespace Benutomo
{
    /// <summary>
    /// Todo
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class ChangingObservableAttribute : Attribute
    {
        public ChangingObservableAttribute() {}

        public ChangingObservableAttribute(NotificationAccessibility Accessibility) {}
    }
}
";

        internal const string NotificationAccessibilityName = "NotificationAccessibility";
        internal const string NotificationAccessibilityFullyQualifiedMetadataName = "Benutomo.NotificationAccessibility";
        internal const int NotificationAccessibilityPublic = 0;
        internal const int NotificationAccessibilityProtected = 1;
        internal const int NotificationAccessibilityInternal = 2;
        internal const int NotificationAccessibilityProtectedInternal = 3;
        internal const int NotificationAccessibilityPrivateProtected = 4;
        internal const int NotificationAccessibilityPrivate = 5;
        private const string NotificationAccessibilitySource = @"
using System;

#nullable enable
#pragma warning disable CS0436 // このソース内の型が、別の参照アセンブリのの型と競合している場合の警告を抑止(この警告は同じソースジェネレータを使用している参照プロジェクトのInternalVisibleTo属性に指定されている場合に発生する

namespace Benutomo
{
    /// <summary>
    /// Todo
    /// </summary>
    internal enum NotificationAccessibility : int
    {
        Public,
        Protected,
        Internal,
        ProtectedInternal,
        PrivateProtected,
        Private,
    }
}
";

        internal const string ExplicitInterfaceImplementationName = "ExplicitInterfaceImplementation";
        internal const string ExplicitInterfaceImplementationFullyQualifiedMetadataName = "Benutomo.ExplicitInterfaceImplementation";
        internal const int ExplicitInterfaceImplementationEnable = 0;
        internal const int ExplicitInterfaceImplementationDisable = 1;
        private const string ExplicitInterfaceImplementationSource = @"
using System;

#nullable enable
#pragma warning disable CS0436 // このソース内の型が、別の参照アセンブリのの型と競合している場合の警告を抑止(この警告は同じソースジェネレータを使用している参照プロジェクトのInternalVisibleTo属性に指定されている場合に発生する

namespace Benutomo
{
    /// <summary>
    /// Todo
    /// </summary>
    internal enum ExplicitInterfaceImplementation : int
    {
        Enable,
        Disable,
    }
}
";


        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            WriteLogLine("Begin Initialize");

            context.RegisterPostInitializationOutput(PostInitialization);

            var enableNotificationSupportAttributeSymbol = context.CompilationProvider
                .Select((compilation, cancellationToken) =>
                {
                    WriteLogLine("Begin GetTypeByMetadataName");
                    
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var enableNotificationSupportAttributeSymbol = compilation.GetTypeByMetadataName(EnableNotificationSupportAttributeFullyQualifiedMetadataName) ?? throw new InvalidOperationException();
                        var changedEventAttributeSymbol = compilation.GetTypeByMetadataName(ChangedEventAttributeFullyQualifiedMetadataName) ?? throw new InvalidOperationException();
                        var changingEventAttributeSymbol = compilation.GetTypeByMetadataName(ChangingEventAttributeFullyQualifiedMetadataName) ?? throw new InvalidOperationException();
                        var changedObservableAttributeSymbol = compilation.GetTypeByMetadataName(ChangedObservableAttributeFullyQualifiedMetadataName) ?? throw new InvalidOperationException();
                        var changingObservableAttributeSymbol = compilation.GetTypeByMetadataName(ChangingObservableAttributeFullyQualifiedMetadataName) ?? throw new InvalidOperationException();
                        var notifyPropertyChangedSymbol = compilation.GetTypeByMetadataName("System.ComponentModel.INotifyPropertyChanged") ?? throw new InvalidOperationException();
                        var notifyPropertyChangingSymbol = compilation.GetTypeByMetadataName("System.ComponentModel.INotifyPropertyChanging") ?? throw new InvalidOperationException();
                        var notificationAccessibilitySymbol = compilation.GetTypeByMetadataName(NotificationAccessibilityFullyQualifiedMetadataName) ?? throw new InvalidOperationException();
                        var explicitInterfaceImplementationSymbol = compilation.GetTypeByMetadataName(ExplicitInterfaceImplementationFullyQualifiedMetadataName) ?? throw new InvalidOperationException();
                        var actionSymbol = compilation.GetTypeByMetadataName("System.Action") ?? throw new InvalidOperationException();
                        var eventHandlerSymbol = compilation.GetTypeByMetadataName("System.EventHandler") ?? throw new InvalidOperationException();

                        return new UsingSymbols(
                            enableNotificationSupportAttributeSymbol,
                            changedEventAttributeSymbol,
                            changingEventAttributeSymbol,
                            changedObservableAttributeSymbol,
                            changingObservableAttributeSymbol,
                            notifyPropertyChangedSymbol,
                            notifyPropertyChangingSymbol,
                            notificationAccessibilitySymbol,
                            explicitInterfaceImplementationSymbol,
                            actionSymbol,
                            eventHandlerSymbol
                        );
                    }
                    catch (OperationCanceledException)
                    {
                        WriteLogLine("Canceled GetTypeByMetadataName");
                        throw;
                    }
                    catch (Exception ex)
                    {
                        WriteLogLine("Exception GetTypeByMetadataName");
                        WriteLogLine(ex.ToString());
                        throw;
                    }
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
                .SelectMany(ToPropertyInputArgs);

            context.RegisterSourceOutput(propertyNameInputArgs, GenerateEventArg);

            WriteLogLine("End Initialize");

            IEnumerable<(TypeDefinitionInfo, ImmutableArray<(string, PropertyEventArgClass)>)> ToPropertyInputArgs(ImmutableArray<MethodSourceBuildInputs?> sourceBuildInputs, CancellationToken cancellationToken)
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

                    yield return (propertiesInClass.Key, properties);
                }
            }
        }

        void PostInitialization(IncrementalGeneratorPostInitializationContext context)
        {
            WriteLogLine("Begin PostInitialization");

            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource($"{EventToObservableName}.cs", EventToObservableSource);

            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource($"{EnableNotificationSupportAttributeName}.cs", EnableNotificationSupportAttributeSource);

            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource($"{ChangingEventAttributeName}.cs", ChangingEventAttributeSource);

            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource($"{ChangedEventAttributeName}.cs", ChangedEventAttributeSource);

            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource($"{ChangingObservableAttributeName}.cs", ChangingObservableAttributeSource);

            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource($"{ChangedObservableAttributeName}.cs", ChangedObservableAttributeSource);

            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource($"{NotificationAccessibilityName}.cs", NotificationAccessibilitySource);

            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource($"{ExplicitInterfaceImplementationName}.cs", ExplicitInterfaceImplementationSource);

            WriteLogLine("End PostInitialization");
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
            WriteLogLine("Begin Transform");
            try
            {
                var propertyDeclarationSyntax = (PropertyDeclarationSyntax)context.Node;

                var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax, cancellationToken) as IPropertySymbol;

                WriteLogLine($"End Transform ({propertySymbol?.ContainingType?.Name}.{propertySymbol?.Name})");

                return propertySymbol;
            }
            catch (OperationCanceledException)
            {
                WriteLogLine($"Canceled Transform");
                throw;
            }
        }

        MethodSourceBuildInputs? ToMethodSourceBuildInputArgs((IPropertySymbol? Left, UsingSymbols Right) v, CancellationToken ct)
        {
            var propertySymbol = v.Left;
            var usingSymbols = v.Right;

            if (propertySymbol is null) return null;

            WriteLogLine($"Begin PostTransform ({propertySymbol.ContainingType?.Name}.{propertySymbol.Name})");

            try
            {
                var enableNotificationSupportAttributeData = propertySymbol.GetAttributes().FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, usingSymbols.EnableNotificationSupportAttribute));
                if (enableNotificationSupportAttributeData is null)
                {
                    return null;
                }

                var result = new MethodSourceBuildInputs(propertySymbol, usingSymbols, enableNotificationSupportAttributeData);

                WriteLogLine($"End PostTransform ({propertySymbol.ContainingType?.Name}.{propertySymbol.Name})");

                return result;
            }
            catch (OperationCanceledException)
            {
                WriteLogLine($"Canceled PostTransform ({propertySymbol.ContainingType?.Name}.{propertySymbol.Name})");
                throw;
            }
            catch (Exception ex)
            {
                WriteLogLine($"Exception PostTransform ({propertySymbol.ContainingType?.Name}.{propertySymbol.Name})");
                WriteLogLine(ex.ToString());
                throw;
            }
        }

        void GenerateMethod(SourceProductionContext context, MethodSourceBuildInputs? sourceBuildInputs)
        {
            if (sourceBuildInputs is null) return;

            if (sourceBuildInputs.IsEventArgsOnly) return;


            WriteLogLine($"Begin Generate ({sourceBuildInputs.ContainingTypeInfo.Name}.{sourceBuildInputs.InternalPropertyName})");

            try
            {
                Span<char> initialBuffer = stackalloc char[80000];

                using var sourceBuilder = new MethodSourceBuilder(context, sourceBuildInputs, initialBuffer);

                sourceBuilder.Build();

                context.AddSource(sourceBuilder.HintName, sourceBuilder.SourceText);

                WriteLogLine($"End Generate ({sourceBuildInputs.ContainingTypeInfo.Name}.{sourceBuildInputs.InternalPropertyName}) => {sourceBuilder.HintName}");
            }
            catch (OperationCanceledException)
            {
                WriteLogLine($"Canceled Generate ({sourceBuildInputs.ContainingTypeInfo.Name}.{sourceBuildInputs.InternalPropertyName})");
                throw;
            }
            catch (Exception ex)
            {
                WriteLogLine($"Exception in Generate ({sourceBuildInputs.ContainingTypeInfo.Name}.{sourceBuildInputs.InternalPropertyName})");
                WriteLogLine(ex.ToString());
                throw;
            }
        }

        void GenerateEventArg(SourceProductionContext context, (TypeDefinitionInfo containingTypeInfo, ImmutableArray<(string name, PropertyEventArgClass eventArgClass)> properties) args)
        {
            WriteLogLine($"Begin Generate {args.containingTypeInfo.Name} EventArgs");

            try
            {
                Span<char> initialBuffer = stackalloc char[80000];

                using var sourceBuilder = new ClassSourceBuilder(context, args.containingTypeInfo, initialBuffer);

                sourceBuilder.AppendLine("#nullable enable");
                sourceBuilder.AppendLine("#pragma warning disable CS0612,CS0618,CS0619");

                sourceBuilder.WriteTypeDeclarationStart();

                foreach (var property in args.properties)
                {
                    if (property.eventArgClass == PropertyEventArgClass.Changed)
                    {
                        var changedEventArgFieldName = $"__PropertyChangedEventArgs_{property.name}";

                        sourceBuilder.PutIndentSpace();
                        sourceBuilder.Append("private static global::System.ComponentModel.PropertyChangedEventArgs ");
                        sourceBuilder.Append(changedEventArgFieldName);
                        sourceBuilder.Append(" = new global::System.ComponentModel.PropertyChangedEventArgs(\"");
                        sourceBuilder.Append(property.name);
                        sourceBuilder.AppendLine("\");");
                    }
                    else if (property.eventArgClass == PropertyEventArgClass.Changing)
                    {
                        var changingEventArgFieldName = $"__PropertyChangingEventArgs_{property.name}";

                        sourceBuilder.PutIndentSpace();
                        sourceBuilder.Append("private static global::System.ComponentModel.PropertyChangingEventArgs ");
                        sourceBuilder.Append(changingEventArgFieldName);
                        sourceBuilder.Append(" = new global::System.ComponentModel.PropertyChangingEventArgs(\"");
                        sourceBuilder.Append(property.name);
                        sourceBuilder.AppendLine("\");");
                    }
                    else
                    {
                        Debug.Fail("invalid PropertyEventArgClass");
                    }
                }

                sourceBuilder.WriteTypeDeclarationEnd();

                var hintName = $"gen_{string.Join(".", sourceBuilder.HintingTypeNames)}.EventArgDeclarations_{sourceBuilder.NameSpace}.cs";

                context.AddSource(hintName, sourceBuilder.SourceText);

                WriteLogLine($"End Generate {args.containingTypeInfo.Name} EventArgs => {hintName}");
            }
            catch (OperationCanceledException)
            {
                WriteLogLine($"Canceled Generate {args.containingTypeInfo.Name} EventArgs");
                throw;
            }
            catch (Exception ex)
            {
                WriteLogLine($"Exception in Generate {args.containingTypeInfo.Name} EventArgs");
                WriteLogLine(ex.ToString());
                throw;
            }
        }
    }
}
