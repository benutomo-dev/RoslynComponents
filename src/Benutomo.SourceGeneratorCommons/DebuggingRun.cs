using System.Diagnostics;

namespace Benutomo.SourceGeneratorCommons;

internal sealed class DebuggingRun
{
    [Conditional("DEBUG")]
    public static void Assert(bool condition)
    {
        if (!Debugger.IsAttached) return;

        Debug.Assert(condition);
    }
}
