using System;
using Benutomo;

namespace SourceGeneratorDebug_NoPolyfill
{
    [AutomaticDisposeImpl]
    public partial class Class1 : IDisposable
    {
        [EnableAutomaticDispose]
        IDisposable? xx { get; set; }

    }
}