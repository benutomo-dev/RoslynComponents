using Benutomo;
using System.ComponentModel;
using static Benutomo.NotificationAccessibility;
//using static Benutomo.ExplicitInterfaceImplementation;

using CAttribute = Benutomo.EnableNotificationSupportAttribute;

namespace SourceGeneratorDebug
{
    interface IInterface1
    {
        //event EventHandler NumberChanged;
        //event EventHandler NumberChanging;

        int Number { get; set; }
    }

    public partial class Class1 : INotifyPropertyChanged, INotifyPropertyChanging, IInterface1
    {
        public event PropertyChangingEventHandler? PropertyChanging;

        public event PropertyChangedEventHandler? PropertyChanged;



        class Class2
        {
        }

        //        [EnableNotificationSupport(EventArgsOnly = false)]
        //        [ChangedEvent(Public)]
        //        [ChangingEvent(Public)]
        //        [ChangedObservable(Public)]
        //        [ChangingObservable(Private)]
        //        public bool? IsEnabled
        //        {
        //            get => _IsEnabled();
        //            set => _IsEnabled(value, EqualityComparer<bool?>.Default);
        //        }

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

        //[EnableNotificationSupport]
        //public List<Dictionary<(int, string?), long>> X
        //{
        //    get => _X();
        //    set => _X(value);
        //}

        //[EnableNotificationSupport]
        //Class2 Inner
        //{
        //    get => _Inner();
        //    set => _Inner(value);
        //}
    }
}


