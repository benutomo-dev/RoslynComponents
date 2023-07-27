using Benutomo;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SourceGeneratorDebug_StandardPatterns.EqualsGenerator
{
    [AutomaticEqualsImpl(AutomaticEqualsImplOptions.WithOperator)]
    internal partial class Class1<T> : INotifyPropertyChanged, IEquatable<Class1<T>>
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [EnableNotificationSupport]
        [RepresentingEquivalenceFor(nameof(__x))]
        [AutoGenEqualsEqualityComparer(nameof(EqualityComparer<long>.Default))]
        public int X { get => _X(); set => _X(value); }

        int y;
        int tt;
    }

    [AutomaticEqualsImpl(AutomaticEqualsImplOptions.None)]
    internal sealed partial class Class2 : Class1<int>
    {
        //[EnableNotificationSupport]
        [AutoGenEqualsEqualityComparer(nameof(EqualityComparer<long>.Default))]
        //public int Y { get => _Y(); set => _Y(value); }

        int tt;
    }

    [AutomaticEqualsImpl]
    partial struct Struct1 : IEquatable<Struct1>
    {
        //[IsNotEquivalenceFactor]
        int x;
    }

}
