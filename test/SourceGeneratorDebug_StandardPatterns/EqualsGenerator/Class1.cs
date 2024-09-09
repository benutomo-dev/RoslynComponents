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

        [AutoGenEqualsEqualityComparer(nameof(StringComparer.Ordinal), useDefaultEqualityComparerIfNull: true)]
        string? T1 { get; set; }

        [AutoGenEqualsEqualityComparer(nameof(StringComparer.Ordinal), useDefaultEqualityComparerIfNull: false)]
        string? T2 { get; set; }

        [AutoGenEqualsEqualityComparer(nameof(StringComparer.Ordinal))]
        string? T3 { get; set; }

        string? T4 { get; set; }

        public int Y { get; set; }


        [EnableNotificationSupport]
        [RepresentingEquivalenceFor(nameof(__x))]
        [AutoGenEqualsEqualityComparer(nameof(EqualityComparer<long>.Default))]
        public int X { get => _X(); set => _X(value); }

        [RepresentingEquivalenceFor(nameof(tt))]
        public int Tt { get => tt; set => tt = value; }

        int y;
        int tt;

        [HashCodeCacheField]
        int _hashCode;
    }

    [AutomaticEqualsImpl(AutomaticEqualsImplOptions.None)]
    internal sealed partial class Class2 : Class1<int>, IEquatable<Class2>
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

    internal sealed partial class Class3 : IEquatable<Class3>
    {
        event EventHandler? Event;

        int X { get; set; }

        [RepresentingEquivalenceFor(nameof(y))]
        int Y { get => y; set => y = value; }

        int Z => 1;

        int y;

        int this[int x] => x;


        [HashCodeCacheField]
        int _hashCode;

        [GetHashCodeImpl]
        public int GetHashCodeImpl()
        {
            if (_hashCode != 0) return _hashCode;
            _hashCode = HashCode.Combine(X, Y);
            _hashCode = _hashCode == 0 ? 1 : _hashCode;
            return _hashCode;
        }

        public override int GetHashCode()
        {
            //return 0;
            return GetHashCodeImpl();
            //return HashCode.Combine(X, Y);
        }

        public override bool Equals(object? obj) => obj is Class3 class3 && Equals(class3);

        public bool Equals(Class3? obj)
        {
            return obj is not null
                && GetHashCode() == obj.GetHashCode()
                && X == obj.X
                && Y == obj.Y
                ;
        }
    }

}
