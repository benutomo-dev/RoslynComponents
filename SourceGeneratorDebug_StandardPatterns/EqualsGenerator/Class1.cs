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

        public int Y { get; set; }


        [EnableNotificationSupport]
        [RepresentingEquivalenceFor(nameof(__x))]
        [AutoGenEqualsEqualityComparer(nameof(EqualityComparer<long>.Default))]
        public int X { get => _X(); set => _X(value); }

        [RepresentingEquivalenceFor(nameof(tt))]
        public int Tt { get => tt; set => tt = value; }

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

    internal sealed partial class Class3
    {
        int X { get; set; }

        [RepresentingEquivalenceFor(nameof(y))]
        int Y { get => y; set => y = value; }
        
        int Z => 1;

        int y;


        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override bool Equals(object? obj) => obj is Class3 class3 && Equals(class3);

        public bool Equals(Class3? obj)
        {
            return obj is not null
                && X == obj.X
                && Y == obj.Y
                ;
        }
    }

}
