﻿using Benutomo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static Benutomo.NotificationAccessibility;

namespace SourceGeneratorDebug_StandardPatterns.AutomaticNotifyPropertyChangedImpl
{
    interface IInterface1
    {
        event EventHandler NumberChanged;
        event EventHandler NumberChanging;

        int Number { get; set; }
    }

    public partial class Class3
    {
        [EnableNotificationSupport]
        private string[] Texts
        {
            get => _Texts();
            set => _Texts(value);
        }

        public Class3()
        {
            __texts = null!;
        }
    }

    public partial class Class99<T1, T2>
    {
        [EnableNotificationSupport]
        private Func<T1?, T2> Texts
        {
            get => _Texts();
            set => _Texts(value);
        }

        public Class99()
        {
            __texts = null!;
        }
    }

    public partial class Class1 : INotifyPropertyChanged, INotifyPropertyChanging, IInterface1
    {
        public event PropertyChangingEventHandler? PropertyChanging;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Class1()
        {
            __x = null!;
            __inner = null!;
            __text = null!;
        }

        class Class2
        {
        }

        [EnableNotificationSupport(EventArgsOnly = false)]
        [ChangedEvent(Public)]
        [ChangingEvent(Public)]
        [ChangedObservable(Public)]
        [ChangingObservable(Private)]
        public bool? IsEnabled
        {
            get => _IsEnabled();
            set => _IsEnabled(value, EqualityComparer<bool?>.Default);
        }

#nullable enable
        [EnableNotificationSupport]
        [ChangedEvent]
        [ChangingEvent]
        public string Text
        {
            get => _Text();
            set => _Text(value);
        }
#nullable restore

#nullable disable
        [EnableNotificationSupport]
        [ChangedObservable(Public)]
        [ChangingObservable(Public)]
        [ChangedEvent]
        [ChangingEvent]
        public int Number
        {
            get => _Number();
            set => _Number(value);
        }
#nullable restore

        int IInterface1.Number
        {
            get => _Number();
            set => _Number(value);
        }

        [EnableNotificationSupport]
        public List<Dictionary<(int, string?), long>> X
        {
            get => _X();
            set => _X(value);
        }

        [EnableNotificationSupport]
        Class2 Inner
        {
            get => _Inner();
            set => _Inner(value);
        }

        private void Test()
        {
            using var x = _NumberWithDefferedNotification(0);

            using (_NumberWithDefferedNotification(0))
            {
                ;
            }
        }
    }
}


