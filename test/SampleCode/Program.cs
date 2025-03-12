using Benutomo;

namespace SampleCode
{
    // 自動実装を適用するクラス
    [AutomaticDisposeImpl]
    public partial class DisposeableTest : IDisposable, IAsyncDisposable
    {
        // DisposeableTestのDipose()とDiposeAsync()は自動実装されるため、定義不要

        // IDisposable.Dispose()による破棄が可能なフィールド
        [EnableAutomaticDispose]
        ConsoleOutputDisposable consoleOutputDisposable = new ConsoleOutputDisposable();

        // IDisposable.Dispose()とIAsyncDisposable.DisposeAsync()のどちらでも破棄が可能なプロパティ
        [EnableAutomaticDispose]
        ConsoleOutputAsyncDisposable consoleOutputAsyncDisposable { get; } = new ConsoleOutputAsyncDisposable();

        public DisposeableTest()
        {
            Console.WriteLine("Created new DisposeableTest");
        }
    }

    // 以降は、出力例のためのコード

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