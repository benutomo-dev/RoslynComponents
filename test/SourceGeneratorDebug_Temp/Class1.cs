using Benutomo;
using System;
using System.ComponentModel;

namespace SourceGeneratorDebug_Temp;

public partial class Class99<T1, T2> where T1 : struct
{
    [EnableNotificationSupport]
    [ChangedObservable]
    [ChangingObservable]
    private Func<T1?, T2>? Texts
    {
        get => _Texts();
        set => _Texts(value);
    }

    public Class99()
    {
        __texts = null!;
    }
}
