using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Benutomo.AutomaticDisposeImpl.SourceGenerator
{
    ref struct AutomaticDisposeImplClassMemberReporter
    {
        public SymbolAnalysisContext context;
        public UsingSymbols usingSymbols;
        public INamedTypeSymbol namedTypeSymbol;
        public AutomaticDisposeContextChecker automaticDisposeContextChecker;
        public ISymbol member;
        public HashSet<string> dependencyMemberRegisteredSet;
        public bool isAssignableToIDisposable;
        public bool isAssignableToIAsyncDisposable;
        public bool isEnableAutomaticDisposeAttributedMember;
        public bool isDisableAutomaticDisposeAttributedMember;
        public bool isUnmanagedResourceReleaseMethodAttributeedMember;
        public bool isManagedObjectDisposeMethodAttributeedMember;
        public bool isManagedObjectAsyncDisposeMethodAttributeedMember;

        public void DoReport()
        {
            if (isManagedObjectDisposeMethodAttributeedMember)
            {
                if (isAssignableToIDisposable)
                {
                    if (!isValidMethodForManagedObjectDisposeMethodAttribute(member))
                    {
                        foreach (var location in member.Locations)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(AutomaticDisposeAnalyzer.s_diagnosticDescriptor_SG0010, location));
                        }
                    }

                    static bool isValidMethodForManagedObjectDisposeMethodAttribute(ISymbol? member)
                    {
                        if (member is not IMethodSymbol methodSymbol) return false;

                        if (methodSymbol.ReturnType.SpecialType != SpecialType.System_Void) return false;

                        if (methodSymbol.IsGenericMethod) return false;

                        if (!methodSymbol.Parameters.IsEmpty) return false;

                        return true;
                    }
                }
                else
                {
                    foreach (var location in member.Locations)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(AutomaticDisposeAnalyzer.s_diagnosticDescriptor_SG0007, location));
                    }
                }
            }

            if (isManagedObjectAsyncDisposeMethodAttributeedMember)
            {
                if (isAssignableToIAsyncDisposable)
                {
                    if (!isValidMethodForManagedObjectAsyncDisposeMethodAttribute(member, usingSymbols))
                    {
                        foreach (var location in member.Locations)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(AutomaticDisposeAnalyzer.s_diagnosticDescriptor_SG0014, location));
                        }
                    }

                    static bool isValidMethodForManagedObjectAsyncDisposeMethodAttribute(ISymbol? member, UsingSymbols usingSymbols)
                    {
                        if (member is not IMethodSymbol methodSymbol) return false;

                        if (true
                            && !SymbolEqualityComparer.Default.Equals(methodSymbol.ReturnType, usingSymbols.Task)
                            && !SymbolEqualityComparer.Default.Equals(methodSymbol.ReturnType, usingSymbols.ValueTask)
                            )
                        {
                            return false;
                        }

                        if (methodSymbol.IsGenericMethod) return false;

                        if (!methodSymbol.Parameters.IsEmpty) return false;

                        return true;
                    }
                }
                else
                {
                    foreach (var location in member.Locations)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(AutomaticDisposeAnalyzer.s_diagnosticDescriptor_SG0011, location));
                    }
                }
            }

            if (isEnableAutomaticDisposeAttributedMember && isDisableAutomaticDisposeAttributedMember)
            {
                foreach (var location in member.Locations)
                {
                    // EnableAutomaticDispose属性とDisableAutomaticDispose属性は同時付与できない
                    context.ReportDiagnostic(Diagnostic.Create(AutomaticDisposeAnalyzer.s_diagnosticDescriptor_SG0018, location));
                }
            }

            if (member is IFieldSymbol fieldSymbol)
            {
                var isAssignableToIDisposableMember = fieldSymbol.IsAssignableTo(usingSymbols.IDisposable);
                var isAssignableToIAsyncDisposableMember = usingSymbols.IAsyncDisposable is not null && fieldSymbol.IsAssignableTo(usingSymbols.IAsyncDisposable);
                var isEnableAutomaticDisposeMember = automaticDisposeContextChecker.IsEnableField(fieldSymbol);

                DoReportForFieldOrProptertyMember(member.Name, isAssignableToIDisposableMember, isAssignableToIAsyncDisposableMember, isEnableAutomaticDisposeMember);
            }

            if (member is IPropertySymbol propertySymbol)
            {
                var isAssignableToIDisposableMember = propertySymbol.IsAssignableTo(usingSymbols.IDisposable);
                var isAssignableToIAsyncDisposableMember = usingSymbols.IAsyncDisposable is not null && propertySymbol.IsAssignableTo(usingSymbols.IAsyncDisposable);
                var isEnableAutomaticDisposeMember = automaticDisposeContextChecker.IsEnableProperty(propertySymbol);

                DoReportForFieldOrProptertyMember(member.Name, isAssignableToIDisposableMember, isAssignableToIAsyncDisposableMember, isEnableAutomaticDisposeMember);
            }
        }

        private void DoReportForFieldOrProptertyMember(
                    string name,
                    bool isAssignableToIDisposableMember,
                    bool isAssignableToIAsyncDisposableMember,
                    bool isEnableAutomaticDisposeMember
                    )
        {
            if (member.IsImplicitlyDeclared)
            {
                return;
            }

            if (member.IsStatic)
            {
                DoReportForFieldOrProptertyStaticMember();
            }
            else
            {
                DoReportForFieldOrProptertyInstanceMember(name, isAssignableToIDisposableMember, isAssignableToIAsyncDisposableMember, isEnableAutomaticDisposeMember);
            }
        }

        void DoReportForFieldOrProptertyStaticMember()
        {
            if (isEnableAutomaticDisposeAttributedMember)
            {
                foreach (var location in member.Locations)
                {
                    // staticメンバにEnableAutomaticDispose属性は付与できない
                    context.ReportDiagnostic(Diagnostic.Create(AutomaticDisposeAnalyzer.s_diagnosticDescriptor_SG0022, location, member.Name, namedTypeSymbol.Name));
                }
            }

            if (isDisableAutomaticDisposeAttributedMember)
            {
                foreach (var location in member.Locations)
                {
                    // staticメンバにDisableAutomaticDispose属性は付与できない
                    context.ReportDiagnostic(Diagnostic.Create(AutomaticDisposeAnalyzer.s_diagnosticDescriptor_SG0023, location, member.Name, namedTypeSymbol.Name));
                }
            }
        }


        void DoReportForFieldOrProptertyInstanceMember(
                    string name,
                    bool isAssignableToIDisposableMember,
                    bool isAssignableToIAsyncDisposableMember,
                    bool isEnableAutomaticDisposeMember
                    )
        {
            if (automaticDisposeContextChecker.Mode == AutomaticDisposeImplMode.Explicit)
            {
                if (isAssignableToIDisposableMember || isAssignableToIAsyncDisposableMember)
                {
                    // メンバは自動破棄の対象となりうる条件を満たしている

                    if (!isEnableAutomaticDisposeAttributedMember && !isDisableAutomaticDisposeAttributedMember)
                    {
                        // メンバに自動破棄の有無を明示する属性が設定されていない

                        if (!dependencyMemberRegisteredSet.Contains(name))
                        {
                            // 他のメンバの依存関係として設定されていない

                            foreach (var location in member.Locations)
                            {
                                // 自動実装のモードがExplicitなのに、EnableAutomaticDisposeとDisableAutomaticDisposeのどちらの属性も付けられていない
                                context.ReportDiagnostic(Diagnostic.Create(AutomaticDisposeAnalyzer.s_diagnosticDescriptor_SG0019, location, member.Name, namedTypeSymbol.Name));
                            }
                        }
                    }
                }
            }

            if (isEnableAutomaticDisposeMember)
            {
                // 自動破棄の対象となるメンバ

                if (isAssignableToIDisposableMember && isAssignableToIAsyncDisposableMember && isAssignableToIDisposable && !isAssignableToIAsyncDisposable)
                {
                    // メンバはDisposeでもDisposeAsyncでも破棄できるが、メンバを含むクラスはIAsyncDisposableを実装していない

                    foreach (var location in member.Locations)
                    {
                        // このメンバは非同期破棄をサポートしているが、自動破棄では常に同期的破棄になることを注意
                        context.ReportDiagnostic(Diagnostic.Create(AutomaticDisposeAnalyzer.s_diagnosticDescriptor_SG0003, location, member.Name, namedTypeSymbol.Name));
                    }
                }

                if (!isAssignableToIDisposableMember && isAssignableToIAsyncDisposableMember)
                {
                    // メンバはDisposeAsyncでのみ破棄できる

                    if (isAssignableToIDisposable && !isAssignableToIAsyncDisposable)
                    {
                        // メンバを含むクラスはIDisposableだけを実装している

                        foreach (var location in member.Locations)
                        {
                            // このメンバはDisposeAsyncでしか破棄できないのに、このメンバを含むクラスはIDisposableしか実装していないので、自動破棄できない
                            context.ReportDiagnostic(Diagnostic.Create(AutomaticDisposeAnalyzer.s_diagnosticDescriptor_SG0004, location, member.Name, namedTypeSymbol.Name));
                        }
                    }
                }
            }

            if (!isAssignableToIDisposableMember && !isAssignableToIAsyncDisposableMember)
            {
                // IDisposableもIAsyncDisposableも実装していない

                if (isEnableAutomaticDisposeAttributedMember)
                {
                    // EnableAutomaticDispose属性を付与している

                    foreach (var location in member.Locations)
                    {
                        // IDisposableとIAysncDisposableのどちらも実装してい型のメンバにEnableAutomaticDispose属性は付与できない
                        context.ReportDiagnostic(Diagnostic.Create(AutomaticDisposeAnalyzer.s_diagnosticDescriptor_SG0020, location, member.Name, namedTypeSymbol.Name));
                    }
                }

                if (isDisableAutomaticDisposeAttributedMember)
                {
                    // DisableAutomaticDispose属性を付与している

                    foreach (var location in member.Locations)
                    {
                        // IDisposableとIAysncDisposableのどちらも実装してい型のメンバにDisableAutomaticDispose属性は付与できない
                        context.ReportDiagnostic(Diagnostic.Create(AutomaticDisposeAnalyzer.s_diagnosticDescriptor_SG0021, location, member.Name, namedTypeSymbol.Name));
                    }
                }
            }
        }
    }
}
