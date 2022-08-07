; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
CT0001  | Usage    |  Warning | CancellatoinTokenを受け付け可能なオーバーロードが存在
CT0002  | Usage    |  Warning | 引数のCancellationTokenが未使用
CT0003  | Usage    |  Warning | 引数にCancellationTokenの追加を推奨
CT0004  | Usage    |  Warning | CancellationTokenを引数に含むメソッドにUncancelable属性が付与されている
CT0005  | Usage    |  Warning | キャンセル禁止区間で明示的なキャンセルの影響を受ける可能性がある呼び出しがされている
CT0006  | Usage    |  Error   | UncancelableSectionがusing文以外で使用されている
