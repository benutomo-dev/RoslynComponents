using Benutomo;
using System;
using System.Threading.Tasks;

namespace SourceGeneratorDebug_StandardPatterns
{
    class BaseClass : IDisposable, IAsyncDisposable
    {
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~BaseClass()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        protected virtual ValueTask DisposeAsyncCore()
        {
            throw new NotImplementedException();
        }
    }

    interface SubInterface2 : IDisposable, IAsyncDisposable
    {

    }

    [AutomaticDisposeImpl]
    partial class GenericTypeMember<T1, T2> : BaseClass where T1 : IDisposable where T2 : SubInterface2
    {
        T1 disposable = default;
        T2 asyncDisposable = default;
    }
}
