using IntelliMonWPF.Enum;
using IntelliMonWPF.IF_Implements.Factory;
using IntelliMonWPF.Interface;
using IntelliMonWPF.Interface.IFactory;
using IntelliMonWPF.Interface.IMangerInferface;
using IntelliMonWPF.Models;
using IntelliMonWPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IntelliMonWPF.ViewModels
{
    class AddDeviceModbusUCViewModel :BindableBase, IDialogAware
    {
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
            if (DeviceNameList.Count == 0)
            {
               messages.ShowMessage("设备为空请添加设备","温馨提示");
            }
        }
        private readonly IDictManger<string,DeviceModel> modbusDictManger;
        private readonly IDictMangerFactory dictMangerFactory;
        private IMessages messages;
        public AddDeviceModbusUCViewModel(DictMangerFactory dictMangerFactory,IMessages messages)
        {
            this.dictMangerFactory = dictMangerFactory;
            modbusDictManger = dictMangerFactory.CreateDictManger<string, DeviceModel>(DictMangerType.DeviceModel);
            DeviceNameList=new ObservableCollection<string>(modbusDictManger.GetAllKeys());
            CanelCmd = new DelegateCommand(Canle);
            AddCmd= new DelegateCommand(Add);
            this.messages = messages;
        }
        private ObservableCollection<KeyValuePair<ModbusEnum.ModbusRead, string>> _FunctionList = new ObservableCollection<KeyValuePair<ModbusEnum.ModbusRead, string>>
        {
            new KeyValuePair<ModbusEnum.ModbusRead, string>(ModbusEnum.ModbusRead.ReadCoils, "读取线圈状态 (Read Coils)"),
            new KeyValuePair<ModbusEnum.ModbusRead, string>(ModbusEnum.ModbusRead.ReadInputCoils, "读取离散输入 (Read Discrete Inputs)"),
            new KeyValuePair<ModbusEnum.ModbusRead, string>(ModbusEnum.ModbusRead.ReadRegisters, "读取保持寄存器 (Read Holding Registers)"),
            new KeyValuePair<ModbusEnum.ModbusRead, string>(ModbusEnum.ModbusRead.ReadInputRegister, "读取输入寄存器 (Read Input Registers)")
        };
        private ObservableCollection<string> _DeviceNameList;

        public ObservableCollection<string> DeviceNameList
        {
            get { return _DeviceNameList; }
            set {
                _DeviceNameList = value;
                RaisePropertyChanged();
            }
        }


        public ObservableCollection<KeyValuePair<ModbusEnum.ModbusRead, string>> FunctionList
        {
            get { return _FunctionList; }
            set
            {
                _FunctionList = value;
                RaisePropertyChanged();
            }
        }
        private KeyValuePair<ModbusEnum.ModbusRead, string> _SelectFunction;

        public KeyValuePair<ModbusEnum.ModbusRead, string> SelectFunction
        {
            get { return _SelectFunction; }
            set
            {
                _SelectFunction = value;
                RaisePropertyChanged();
            }
        }
        private string _DeviceName;

        public string DeviceName
        {
            get { return _DeviceName; }
            set { _DeviceName = value; }
        }

        private string _SlaveId;

        public string SlaveId
        {
            get { return _SlaveId; }
            set { _SlaveId = value;
                RaisePropertyChanged();
            }
        }
        private int _StartAdress=0;

        public int StartAdress
        {
            get { return _StartAdress; }
            set { _StartAdress = value;
                RaisePropertyChanged();
            }
        }
        private int _ReadCount=8;

        public int ReadCount
        {
            get { return _ReadCount; }
            set { _ReadCount = value;
                RaisePropertyChanged();
            }
        }
        private int _Intervate=1000;

        public int Intervate
        {
            get { return _Intervate; }
            set { _Intervate = value;
                RaisePropertyChanged();
            }
        }
        public DelegateCommand AddCmd { get; set; }
        private DeviceModelServerClass _ModelServerClass;
        private void Add()
        {
            if (DeviceName==null || SlaveId == string.Empty)
            {
               messages.ShowMessage("参数不能为空"); return;
            }
            if (!int.TryParse(SlaveId,out int a))
            {
                if (a > 255 || a < 1)
                {
                   messages.ShowMessage("从站id范围在1到255之间");
                    return;
                }
                else {messages.ShowMessage("请输入整数"); return; }
            }
            ReadModel readModel = new ReadModel()
            {
                DeviceName = DeviceName,
                SlaveId = Convert.ToInt32(SlaveId),
                StartAddress = (ushort)StartAdress,
                NumAddress = (ushort)ReadCount,
                Interavel = Intervate,
                ModbusRead = SelectFunction.Key,
                Modbus = modbusDictManger.GetValue(DeviceName).Protocol
            };
            DeviceModel device = modbusDictManger.GetValue(DeviceName);
            _ModelServerClass = new DeviceModelServerClass(device);
            if (!_ModelServerClass.AddReadModel(DeviceName, readModel)) 
                return;
            modbusDictManger.GetValue(DeviceName).Channel.ReadAsyance(readModel);
            RequestClose.Invoke();
            LoggingService.Instance.Publish(LogType.DeviceConfig, $"添加设备 {DeviceName} 从站 {SlaveId} 读取点 {readModel.Name.Value} 配置");
        }
        public DelegateCommand CanelCmd { get; set; }
        private void Canle()
        {
            RequestClose.Invoke();
        }

    }
}
