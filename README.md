# AutomaticDisposeImpl

C#で`IDisposable`と`IAsyncDisposable`の実装パターンに対応するメソッドを自動実装するソースジェネレータです。

## Introduction

以下のサンプルで示すように、`IDisposable`と`IAsyncDisposable`インターフェイスの少なくとも一方を実装するクラスに`partial`キーワードと`AutomaticDisposeImpl`属性を付与すると、クラス内に含まれる`IDisposable`と`IAsyncDisposable`インターフェイスを実装してる型を持つメンバを破棄する`Dispose()`と`DisposeAsync()`が自動実装されます。

### サンプルコード

```cs
using System;
using System.Threading.Tasks;
using Benutomo;

namespace SampleCode
{
    [AutomaticDisposeImpl]
    public partial class DisposeableTest : IDisposable, IAsyncDisposable
    {
        // DisposeableTestのDipose()とDiposeAsync()は自動実装されるため、定義不要
        
        // IDisposable.Dispose()による破棄が可能なメンバ
        ConsoleOutputDisposable consoleOutputDisposable = new ConsoleOutputDisposable();

        // IDisposable.Dispose()とIAsyncDisposable.DisposeAsync()のどちらでも破棄が可能なメンバ
        ConsoleOutputAsyncDisposable consoleOutputAsyncDisposable = new ConsoleOutputAsyncDisposable();

        public DisposeableTest()
        {
            Console.WriteLine("Created new DisposeableTest");
        }
    }

    class Program
    {
        public static async Task Main()
        {
            var disposeTestInstance = new DisposeableTest();

            Console.WriteLine("Begin disposeTestInstance.Dispose()");
            disposeTestInstance.Dispose();
            Console.WriteLine("End disposeTestInstance.Dispose()");
            Console.WriteLine();

            var asyncDisposeTestInstance = new DisposeableTest();

            Console.WriteLine("Begin disposeTestInstance.DisposeAsync()");
            await asyncDisposeTestInstance.DisposeAsync();
            Console.WriteLine("End disposeTestInstance.DisposeAsync()");
            Console.WriteLine();
        }
    }
    
    class ConsoleOutputDisposable : IDisposable
    {
        public void Dispose()
        {
            Console.WriteLine("    Called Dispose() of ConsoleOutputDisposable.");
        }
    }

    class ConsoleOutputAsyncDisposable : IDisposable, IAsyncDisposable
    {
        public void Dispose()
        {
            Console.WriteLine("    Called Dispose() of ConsoleOutputAsyncDisposable.");
        }

        public ValueTask DisposeAsync()
        {
            Console.WriteLine("    Called DisposeAsync() of ConsoleOutputAsyncDisposable.");
            return default;
        }
    }
}
```

### サンプルコードを実行した際の出力例
以下のようにクラス内に含まれる`IDisposable`または`IAsyncDisposable`を実装したメンバの`Dispose()`と`DisposeAsync()`は、自動実装されたコードから呼び出されます。
自動実装クラスの`DisposeAsync()`はメンバの破棄にも基本は`DisposeAsync()`を呼び出しますが、メンバが`IDisposable`しか実装していない場合は`Dispose()`を呼び出します。
```
Created new DisposeableTest
Begin disposeTestInstance.Dispose()
    Called Dispose() of ConsoleOutputDisposable.
    Called Dispose() of ConsoleOutputAsyncDisposable.
End disposeTestInstance.Dispose()

Created new DisposeableTest
Begin disposeTestInstance.DisposeAsync()
    Called DisposeAsync() of ConsoleOutputAsyncDisposable.
    Called Dispose() of ConsoleOutputDisposable.
End disposeTestInstance.DisposeAsync()

```

## 使用方法

### インストール
[Nuget](https://www.nuget.org/packages/Benutomo.AutomaticDisposeImpl.SourceGenerator/)などを利用してプロジェクトのアナライザにBenutomo.AutomaticDisposeImpl.SourceGenerator.dllを追加します。

```
Install-Package Benutomo.AutomaticDisposeImpl.SourceGenerator
```

### 基本
以下のように、破棄の自動実装を使用したいクラスを含むC#のソースコードの先頭部に`using Benutomo;`を追加し、`IDisposable`を実装しているクラスに`partial`キーワードと`[AutomaticDisposeImpl]`属性を追加します。自動実装する意味がありませんが、メンバは空でも問題ありません。ただし、`IDisposable`と`IAsyncDisposable`は利用者側のコードで実装を明記する必要があります。

```cs
using Benutomo;
using System;

// 同期的な破棄(IDisposable)を自動実装
[AutomaticDisposeImpl]
partial class Sample : IDisposable
{
}

// 非同期的な破棄(IAsyncDisposable)を自動実装
[AutomaticDisposeImpl]
partial class Sample : IAsyncDisposable
{
}

// 同期的な破棄(IDisposable)と非同期的な破棄(IAsyncDisposable)を自動実装
[AutomaticDisposeImpl]
partial class Sample : IDisposable, IAsyncDisposable
{
}

// インターフェイスが明示的に実装されていないため、NG。IDisposableとIAsyncDisposableの少なくとどちらか一方の実装が必要。
[AutomaticDisposeImpl]
partial class Sample
{
}
```

### 破棄の自動実装から除外したいメンバを指定する

`[AutomaticDisposeImpl]`属性のオプション(DefaultMode)と`[AutomaticDisposeImplMode]`属性を使用すると、破棄の自動実装の対象をコントロールすることができます。

`[AutomaticDisposeImpl]`属性の`DefaultMode`に`AutomaticDisposeImplMode.Disable`を指定すると、クラス全体の自動実装コードからの破棄の呼び出しのデフォルトが無効になります(`Dispose()`メソッドと'DisposeAsync()'メソッドは作られますがデフォルトではメンバの破棄をしなくなります)。

また、メンバに対して`[AutomaticDisposeImplMode]`属性を付与することでメンバごとにクラスのデフォルトと異なる設定を適用することができます。設定できるモードは以下の３値です。

| 設定値 | 振る舞い |
|-|-|
| AutomaticDisposeImplMode.Disable | メンバの自動破棄を無効にします。 |
| AutomaticDisposeImplMode.Enable | メンバの自動破棄を有効にします。 |
| AutomaticDisposeImplMode.Default | `[AutomaticDisposeImplMode]`で使用した場合は`[AutomaticDisposeImpl]`の設定を継承します。`[AutomaticDisposeImpl]`属性の`DefaultMode`に使用した場合は`AutomaticDisposeImplMode.Enable`と見なされます。 |

```cs 
[AutomaticDisposeImpl(DefaultMode = AutomaticDisposeImplMode.Disable)] // メンバの自動破棄の規定値を無効に変更
partial class UserDefinedDisposeImplMethod2 : IDisposable, IAsyncDisposable
{
    IDisposable _ignoredDisposable1; // 規定値が無効であるため、このメンバのDispose()は自動実装コードからは呼び出されません。

    [AutomaticDisposeImplMode(AutomaticDisposeImplMode.Disable)] // メンバの自動破棄を無効化
    IDisposable _ignoredDisposable2; // 個別に無効化されているため、このメンバのDispose()は自動実装コードからは呼び出されません。

    [AutomaticDisposeImplMode(AutomaticDisposeImplMode.Enable)] // メンバの自動破棄を有効化
    IDisposable _automaticDisposedDisposable; // 個別に有効化されているため、このメンバのDispose()は自動実装コードからは呼び出されます。
}
```

### Dispose()などが呼び出されるタイミングで自動実装される処理と同時に独自の処理も実行する

`[ManagedObjectDisposeMethod]`属性と`[ManagedObjectAsyncDisposeMethod]`属性を使用すると、自動実装される`Dispose()`および、`DisposeAsync()`の中からユーザ側のコードで実装されるメソッドを呼び出させることができます。

```cs
[AutomaticDisposeImpl]
partial class UserDefinedDisposeImplSample : IDisposable, IAsyncDisposable
{
    [ManagedObjectDisposeMethod]
    void ManagedObjectDisposeMethod() { } // 自動実装のDispose()から呼び出される。メンバの自動破棄以外のユーザ独自の処理はここで実装することができる。

    [ManagedObjectAsyncDisposeMethod]
    ValueTask ManagedObjectDisposeMethodAsync() => default; // 自動実装のDisposeAsync()から呼び出される。メンバの自動破棄以外のユーザ独自の処理はここで実装することができる。
}
```

`[ManagedObjectDisposeMethod]`属性を付与するメソッドは戻り値がvoidかつ引数の存在しないインスタンスメソッドである必要があります。

`[ManagedObjectAsyncDisposeMethod]`属性を付与するメソッドは戻り値がValueTaskまたはTaskかつ引数の存在しないインスタンスメソッドである必要があります。

どちらの場合も、一つのクラス内で同じ属性を複数のメソッドに付与することはできません。

ℹ この機能の仕様として、自動実装コードが実行する破棄は同期的な破棄と非同期な破棄を含めて最大１回のみであることにご注意下さい。

例えば、自動実装したクラスのメソッドが

```cs
var sample = new UserDefinedDisposeImplSample();
sample.Dipose(); // この破棄のみが有効。以降の重複呼び出しは無視される。
await sample.DiposeAsync();
sample.Dipose();
```

のように呼び出された場合、ユーザのメソッドが呼ばれるのは最初の`sample.Dispose()`のタイミングで`ManagedObjectDisposeMethod()`が呼び出される１回のみです。そのあとに続く`await sample.DiposeAsync()`と２回目の`sample.Dispose()`は完全に無視されます。上記の例で`ManagedObjectDisposeMethodAsync()`が呼び出されることはありません。`ManagedObjectDisposeMethod()`も最初の１回のみ呼び出されます。

### アンマネージドリソースの破棄

`IDisposable.Dipose()`などで自動破棄できるメンバのほかに、`System.IntPtr`等を利用してアンマネージドリソースのハンドルなどを保持している場合は`[UnmanagedResourceReleaseMethod]`属性を利用することで、アンマネージドリソースの破棄を行うメソッドを自動実装されるコードから呼び出させることができます。

`[UnmanagedResourceReleaseMethod]`属性を使用したクラスはデストラクタも自動実装されます。そのため、明示的に`Dispose()`または`DisposeAsync()`の呼び出しがされずにガーベジコレクトさた場合もガーベジコレクタのファイナライズスレッドで`[UnmanagedResourceReleaseMethod]`属性を付与したメソッドが呼び出されます。

```cs
[AutomaticDisposeImpl]
partial class UserDefinedFinalizeImplSample : IDisposable, IAsyncDisposable
{
    [UnmanagedResourceReleaseMethod]
    void UnmanagedResourceReleaseMethod() { } // 自動実装のDispose(),DiposeAsync(),~UserDefinedFinalizeImplSample()から呼び出される。アンマネージドリソースの破棄はここで実装することができる。
}
```
