using Benutomo;

// 同期的な破棄(IDisposable)を自動実装
[AutomaticDisposeImpl]
partial class Sample1 : IDisposable
{
    // 自動破棄するメンバにはEnableAutomaticDispose属性を付与
    [EnableAutomaticDispose]
    IDisposable? _disposable = null;

    // 自動破棄しないメンバにはDisableAutomaticDispose属性を付与
    [DisableAutomaticDispose]
    IDisposable? Disposable => _disposable;
}

// 非同期的な破棄(IAsyncDisposable)を自動実装
[AutomaticDisposeImpl]
partial class Sample2 : IAsyncDisposable
{
}

// 同期的な破棄(IDisposable)と非同期的な破棄(IAsyncDisposable)を自動実装
[AutomaticDisposeImpl]
partial class Sample3 : IDisposable, IAsyncDisposable
{
}

// インターフェイスが明示的に実装されていないため、NG。IDisposableとIAsyncDisposableの少なくとどちらか一方の実装が必要。
//[AutomaticDisposeImpl]
//partial class Sample4
//{
//}