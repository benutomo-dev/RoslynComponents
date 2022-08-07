#nullable enable


namespace Benutomo
{
    public static class Cancellation
    {
        public static _UncancelableBlock UncancelableSection => new _UncancelableBlock();

        public readonly ref struct _UncancelableBlock
        {
            public void Dispose() { }
        }
    }
}
