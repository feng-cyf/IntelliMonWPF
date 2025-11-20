using IntelliMonWPF.Helper.Tools;
using IntelliMonWPF.IF_Implements.MangerInferface;
using IntelliMonWPF.Interface.IFactory;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace IntelliMonWPF.ViewModels.DialogsViewModels
{
    public class ShowPackUCViewModel : BindableBase, IDialogAware
    {
        public DialogCloseListener RequestClose { get; }=new DialogCloseListener();

        public bool CanCloseDialog()
        {
           return true;
        }

        public void OnDialogClosed()
        {
             _timer.Stop();
            PackList.Clear();
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
             
        }
        private readonly string title = "收发数据包";
        private readonly DispatcherTimer _timer;
        private int len = 001;
        private SingelTool singelTool=SingelTool.singelTool;
        public ShowPackUCViewModel()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _timer.Tick += (s, e) => OnAddPack();
            _timer.Start();
        }

        private void OnAddPack()
        {
            while (singelTool.MoudbusQueue.TryDequeue(out string msg))
            {
               
                PackList.Add($"{len.ToString()}  {msg}");
                LatestItem = msg;
                if (PackList.Count > 1000)
                    PackList.RemoveAt(0); 
                len++;
            }
        }


        private ObservableCollection<string> _PackList=new ObservableCollection<string>();

        public ObservableCollection<string> PackList
        {
            get { return _PackList; }
            set { _PackList = value;
                RaisePropertyChanged();
            }
        }
        private string _latestItem;
        public string LatestItem
        {
            get => _latestItem;
            set { _latestItem = value; RaisePropertyChanged(); }
        }
    }
}
