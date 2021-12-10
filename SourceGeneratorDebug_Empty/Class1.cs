using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Benutomo;

namespace SourceGeneratorDebug_Empty
{
    [AutomaticDisposeImpl(Mode = AutomaticDisposeImplMode.Implicit)]
    partial class ImplicitAutoDisposeClass : IDisposable
    {
        IDisposable? disposable;

        public ImplicitAutoDisposeClass(IDisposable? disposable)
        {
            this.disposable = disposable;
        }
    }
}
