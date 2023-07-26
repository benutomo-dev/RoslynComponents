using Benutomo;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SourceGeneratorDebug_StandardPatterns.EqualsGenerator
{
    [AutomaticEqualsImpl]
    internal partial class Class1<T> : INotifyPropertyChanged, IEquatable<Class1<T>>
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [EnableNotificationSupport]
        [EqualityComparer(nameof(EqualityComparer<long>.Default))]
        public int X { get => _X(); set => _X(value); }

    }

    [AutomaticEqualsImpl]
    internal partial class Class2 : Class1<int>
    {
        [EnableNotificationSupport]
        [EqualityComparer(nameof(EqualityComparer<long>.Default))]
        public int Y { get => _Y(); set => _Y(value); }

    }
}
