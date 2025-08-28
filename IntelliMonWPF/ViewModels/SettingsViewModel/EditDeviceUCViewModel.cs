using IntelliMonWPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.ViewModels.SettingsViewModel
{
    internal class EditDeviceUCViewModel : BindableBase, IDialogAware
    {
        private DeviceModel DeviceModel { get; set; }
        public DialogCloseListener RequestClose { get; }=new DialogCloseListener();

        public bool CanCloseDialog()
        {
           return true;
        }

        public void OnDialogClosed()
        {
             
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("DevicePar"))
            {
                DeviceModel = parameters.GetValue<DeviceModel>("DevicePar");
            }

            // ⚡ 从 FunctionList 里匹配，而不是直接赋值
            var matched = FunctionList.FirstOrDefault(x => x.Key == DeviceModel.Function.Key);
            if (!matched.Equals(default(KeyValuePair<string, string>)))
            {
                SelectFunction = matched;
            }

            SavleID = DeviceModel.SlaveId;
            TimeOut = DeviceModel.Timeout;
            Interval = DeviceModel.PeriodTime;
        }
        public EditDeviceUCViewModel()
        {
            UpdateCmd=new DelegateCommand(Update);
        }

        #region 参数设置
        private ObservableCollection<KeyValuePair<string,string>> _FunctionList=new ObservableCollection<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("02", "02 输入线圈读取"),
            new KeyValuePair<string, string>("04", "04 寄存器读取")
        };

        public ObservableCollection<KeyValuePair<string, string>> FunctionList
        {
            get { return _FunctionList; }
            set { _FunctionList = value;
                RaisePropertyChanged();
            }
        }
        private KeyValuePair<string,string> _SelectFuntion;

        public KeyValuePair<string,string> SelectFuntion
        {
            get { return _SelectFuntion; }
            set { _SelectFuntion = value;
                RaisePropertyChanged();
            }
        }
        private KeyValuePair<string,string> _SelectFunction;

        public KeyValuePair<string,string> SelectFunction
        {
            get { return _SelectFunction; }
            set { _SelectFunction = value;
                RaisePropertyChanged();
            }
        }
        private int _SavleID;

        public int SavleID
        {
            get { return _SavleID; }
            set { _SavleID = value;
                RaisePropertyChanged();
            }
        }
        private int _TimeOut;

        public int TimeOut
        {
            get { return _TimeOut; }
            set { _TimeOut = value;
                RaisePropertyChanged();
            }
        }
        private int _Interval;

        public int Interval
        {
            get { return _Interval; }
            set { _Interval = value;
                RaisePropertyChanged();
            }
        }
        public DelegateCommand UpdateCmd {  get; set; }
        private void Update()
        {
            DeviceModel.SlaveId = SavleID;
            DeviceModel.Timeout=TimeOut;
            DeviceModel.Function = SelectFunction;
            DeviceModel.PeriodTime = Interval;
        }
        #endregion
    }
}
