using Benutomo;
using ConflictTestNamespace;

namespace SourceGeneratorDebug_CS0436
{
    public class Class1
    {
        [DisableArgumentCancellationTokenCheck]
        public void Test(CancellationToken cancellationToken)
        {
            ConflictTestClass conflictTestClass = new ConflictTestClass();
        }
    }
}