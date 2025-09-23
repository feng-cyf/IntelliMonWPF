using IntelliMonWPF.Enum;
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
        private ReadModel ReadModel { get; set; }
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
            if (parameters.ContainsKey("DeviceName") || parameters.ContainsKey("ReadModelPar"))
            {
                ReadModel = parameters.GetValue<ReadModel>("ReadModelPar");
                DeviceName = parameters.GetValue<string>("DeviceName");
            }

            // ⚡ 从 FunctionList 里匹配，而不是直接赋值
            var matched = FunctionList.FirstOrDefault(x => x.Key == ReadModel.ModbusRead);
            if (!matched.Equals(default(KeyValuePair<ModbusEnum.ModbusRead, string>)))
            {
                SelectFunction = matched;
            }

            SavleID = ReadModel.SlaveId;
            Interval = ReadModel.Interavel;
            StartAdress = ReadModel.StartAddress;
            Number = ReadModel.NumAddress;
        }
        public EditDeviceUCViewModel()
        {
            UpdateCmd=new DelegateCommand(Update);
            CloseCmd=new DelegateCommand(Close);
        }
        private string _DeviceName;

        public string DeviceName
        {
            get { return _DeviceName; }
            set { _DeviceName = value;
                RaisePropertyChanged();
            }
        }

        #region 参数设置
        private ObservableCollection<KeyValuePair<ModbusEnum.ModbusRead, string>> _FunctionList = new ObservableCollection<KeyValuePair<ModbusEnum.ModbusRead, string>>
        {
            new KeyValuePair<ModbusEnum.ModbusRead, string>(ModbusEnum.ModbusRead.ReadCoils, "读取线圈状态 (Read Coils)"),
            new KeyValuePair<ModbusEnum.ModbusRead, string>(ModbusEnum.ModbusRead.ReadInputCoils, "读取离散输入 (Read Discrete Inputs)"),
            new KeyValuePair<ModbusEnum.ModbusRead, string>(ModbusEnum.ModbusRead.ReadRegisters, "读取保持寄存器 (Read Holding Registers)"),
            new KeyValuePair<ModbusEnum.ModbusRead, string>(ModbusEnum.ModbusRead.ReadInputRegister, "读取输入寄存器 (Read Input Registers)")
        };

        public ObservableCollection<KeyValuePair<ModbusEnum.ModbusRead, string>> FunctionList
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
        private KeyValuePair<ModbusEnum.ModbusRead,string> _SelectFunction;

        public KeyValuePair<ModbusEnum.ModbusRead,string> SelectFunction
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

        private int _StartAdress;

        public int StartAdress
        {
            get { return _StartAdress; }
            set { _StartAdress = value;
                RaisePropertyChanged();
            }
        }
        private int _Number;

        public int Number
        {
            get { return _Number; }
            set { _Number = value; }
        }


        public DelegateCommand UpdateCmd {  get; set; }
        private void Update()
        {
            ReadModel.SlaveId = SavleID;
            ReadModel.ModbusRead = SelectFunction.Key;
            ReadModel.Interavel = Interval;
            ReadModel.NumAddress = Number;
            ReadModel.StartAddress = StartAdress;
            RequestClose.Invoke();
        }

        public DelegateCommand CloseCmd {  get; set; }
        private void Close()
        {
            RequestClose.Invoke();
        }
        #endregion
    }
}
