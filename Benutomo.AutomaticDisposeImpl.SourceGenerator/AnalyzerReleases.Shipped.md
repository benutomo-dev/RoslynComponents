; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/master/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
SG0001  | Usage     | Error    | AutomaticDisposeImpl属性を付与する型にはpartialキーワードが必須です。, [Documentation](https://www.google.co.jp/)
SG0002  | Usage     | Error    | AutomaticDisposeImpl属性を付与する型にはIDisposableまたはIAsyncDisposableインターフェイスが必要, [Documentation](https://www.google.co.jp/)
SG0003  | Usage     | Warning  | メンバの非同期破棄メソッドを利用するためにはIAsyncDisposableインターフェイスが必要, [Documentation](https://www.google.co.jp/)
SG0004  | Usage     | Warning  | 非同期破棄のみをサポートするメンバはIDiposableのみを実装するクラスの自動破棄対象外, [Documentation](https://www.google.co.jp/)
SG9998  | Execution | Error    | ソース生成の失敗, [Documentation](https://www.google.co.jp/)
SG9999  | Execution | Error    | 異常終了, [Documentation](https://www.google.co.jp/)
