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

namespace IntelliMonWPF.ViewModels
{
    public class AddModbusPointViewModel :BindableBase, IDialogAware
    {
        private ObservableCollection<PointModel> _points;
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
            _points = parameters.GetValue<ObservableCollection<PointModel>>("Pointlist");
            DeviceNameList=new ObservableCollection<string>(ModbusDictManger.GetAllKeys());
        }
        private ObservableCollection<string> _DeviceNameList;

        public ObservableCollection<string> DeviceNameList
        {
            get { return _DeviceNameList; }
            set { _DeviceNameList = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<int> _SlaveId;

        public ObservableCollection<int> SlaveIdLIst
        {
            get { return _SlaveId; }
            set { _SlaveId = value;
                RaisePropertyChanged();
            }
        }
        private string _SelectDeviceName;

        public string SelectDeviceName
        {
            get { return _SelectDeviceName; }
            set
            {
                _SelectDeviceName = value;
                RaisePropertyChanged();
                SlaveIdLIst = new ObservableCollection<int>(
         new HashSet<int>(ModbusDictManger.GetValue(SelectDeviceName).readMangerModbus.Values.Select(x => x.SlaveId)));
            }
        }
        private string _DeviceFuction="请选择设备名称和从站地址";

        public string DeviceFuction
        {
            get { return _DeviceFuction; }
            set { _DeviceFuction = value;
                RaisePropertyChanged();
            }
        }
        private int _SelectSlaveId;

        public int SelectSlaveId
        {
            get { return _SelectSlaveId; }
            set { _SelectSlaveId = value;
                RaisePropertyChanged();
                GetStartAdressList();
            }
        }
        private string _StartAddress;

        public string StartAddress
        {
            get { return _StartAddress; }
            set { _StartAddress = value;
                RaisePropertyChanged();
                DataInit();
            }
        }

        private string _PointName;

        public string PointName
        {
            get { return _PointName; }
            set { _PointName = value;
                RaisePropertyChanged();
            }
        }
        private string _Len;

        public string Len
        {
            get { return _Len; }
            set { _Len = value;
                RaisePropertyChanged();
            }
        }
        private string _DataType;

        public string DataType
        {
            get { return _DataType; }
            set { _DataType = value;
                RaisePropertyChanged();
            }
        }
        private string _AccessType;

        public string AccessType
        {
            get { return _AccessType; }
            set {
                _AccessType = value;
                RaisePropertyChanged();
            }
        }
        private string _Unit;

        public string Unit
        {
            get { return _Unit; }
            set { _Unit = value;
                RaisePropertyChanged();
            }
        }
        private string _ScaleFactor;

        public string ScaleFactor
        {
            get { return _ScaleFactor; }
            set {
                _ScaleFactor = value;
            RaisePropertyChanged();
            }
        }
        private string _Offset;

        public string Offset
        {
            get { return _Offset; }
            set {
                _Offset = value;
                RaisePropertyChanged();
            }
        }

        private string _Desc;

        public string Desc
        {
            get { return _Desc; }
            set { _Desc = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<int> _StartAdressList;

        public ObservableCollection<int> StartAdressList
        {
            get { return _StartAdressList; }
            set { _StartAdressList = value;
                RaisePropertyChanged();
            }
        }

        private void GetStartAdressList()
        {
            StartAdressList = new ObservableCollection<int>(
         new HashSet<int>(ModbusDictManger.GetValue(SelectDeviceName).readMangerModbus.Values.Select(x => x.StartAddress)));
        }
        private void DataInit()
        {

            var Data = ModbusDictManger.GetValue(SelectDeviceName).readMangerModbus[(SelectDeviceName, SelectSlaveId,Convert.ToInt32(StartAddress))];
            DeviceFuction = Data.Device;
            AccessType = Data.Function switch 
            {
                "01"=>"可读可写",
                "02"=>"只读",
                "03"=>"可读可写",
                "04"=>"只读",
                _=>"未知类型"
            };
            Len = Data.NumAddress.ToString();
        }
        private IDictManger<string,DeviceModel> ModbusDictManger;
        private IDictMangerFactory dictMangerFactoryl;
        private IMessages messages;
        public AddModbusPointViewModel(DictMangerFactory dictMangerFactory,IMessages messages)
        {
            this.dictMangerFactoryl = dictMangerFactory;   
            ModbusDictManger = dictMangerFactory.CreateDictManger<string, DeviceModel>(DictMangerType.DeviceModel);
            this.messages = messages;
            AddCmd =new DelegateCommand(Add);
        }
        public DelegateCommand AddCmd { get;set; }
        private void Add()
        {
            if (string.IsNullOrEmpty(PointName) || string.IsNullOrEmpty(Offset)|| string.IsNullOrEmpty(Desc)) { messages.ShowMessage("数据不能为空");return; }
            var PointtAdd= ModbusDictManger.GetValue(SelectDeviceName).readMangerModbus[(SelectDeviceName, SelectSlaveId, Convert.ToInt32(StartAddress))].PointModels = new Models.PointModel()
            {
                PointName=PointName,
                StartAddress=StartAddress,
                RegisterType=DeviceFuction,
                Len=Convert.ToInt32(Len),
                DataType=DataType,
                AccessType=AccessType,
                ScaleFactor=ScaleFactor,
                Offset=Offset,
                Desc=Desc,
                Unit=Unit,
                mapDevice=new KeyValuePair<string, (string, int)>(PointName,(SelectDeviceName,SelectSlaveId))
            };
            _points.Add(PointtAdd);
            RequestClose.Invoke();
            LoggingService.Instance.Publish(LogType.PointConfig, $"添加设备 {SelectDeviceName} 从站 {SelectSlaveId} 点 {PointName} 配置");
        }
    }
}
