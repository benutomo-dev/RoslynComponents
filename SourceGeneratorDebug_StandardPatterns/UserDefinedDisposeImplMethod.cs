using Benutomo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceGeneratorDebug_StandardPatterns
{
    [AutomaticDisposeImpl]
    partial class UserDefinedDisposeImplMethod : IDisposable, IAsyncDisposable
    {
        [UnmanagedResourceReleaseMethod]
        void UnmanagedResourceReleaseMethod() { }

        [ManagedObjectDisposeMethod]
        void ManagedObjectDisposeMethod() { }

        [ManagedObjectAsyncDisposeMethod]
        ValueTask ManagedObjectDisposeMethodAsync() => default;

    }
}
