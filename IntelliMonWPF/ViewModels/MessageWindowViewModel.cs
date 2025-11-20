using IntelliMonWPF.Event;
using IntelliMonWPF.Views;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IntelliMonWPF.ViewModels
{
    public class MessageWindowViewModel:BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private string myVar;

        public string MyProperty
        {
            get { return myVar; }
            set { myVar = value;
                RaisePropertyChanged();
            }
        }

        public MessageWindowViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            this._eventAggregator.GetEvent<ShowMesssgeWindow>().Subscribe(tuple => ShowMessage(tuple.Item1, tuple.Item2));
        }
        private void ShowMessage( string msg,int time)
        {
            MyProperty = msg;
            MessageWindow messageWindow = new MessageWindow(time);
            messageWindow.Show();
        }
    }
}
