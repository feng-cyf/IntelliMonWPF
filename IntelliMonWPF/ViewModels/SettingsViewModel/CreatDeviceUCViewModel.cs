using IntelliMonWPF.Base;
using IntelliMonWPF.Enum;
using IntelliMonWPF.Models;
using IntelliMonWPF.Models.Manger;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace IntelliMonWPF.ViewModels.SettingsViewModel
{
    internal class CreatDeviceUCViewModel : ModbuslBase, IDialogAware
    {
        public DialogCloseListener RequestClose { get; }= new DialogCloseListener();

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {

        }
        private ModbusDictManger ModbusDictManger;
        public CreatDeviceUCViewModel(ModbusDictManger modbusDictManger)
        {
            #region 参数初始化
            Modbus = new ObservableCollection<KeyValuePair<ModbusEnum.Modbus, string>> 
            {
                new KeyValuePair<ModbusEnum.Modbus, string>(ModbusEnum.Modbus.SerialPort,"SerialPoer"),
                new KeyValuePair<ModbusEnum.Modbus, string>(ModbusEnum.Modbus.TCP,"TCP"),
                new KeyValuePair<ModbusEnum.Modbus, string>(ModbusEnum.Modbus.UDP,"UDP"),
            };
            SelectModbus = Modbus[0];
            #endregion
            ModbusDictManger = modbusDictManger;
            AddDeviceCmd = new DelegateCommand(AddDevice);
            GetIpAdress();
            OnIpAderss(null,null);
        }
        private void GetIpAdress()
        {
            NetworkChange.NetworkAddressChanged += OnIpAderss;
        }

        private void OnIpAderss(object? sender, EventArgs e)
        {
            string hostName = Dns.GetHostName();
            IPHostEntry host = Dns.GetHostEntry(hostName);
            IpAdressList = new ObservableCollection<string>(
                host.AddressList
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork || (ip.Equals(IPAddress.Loopback)))
                    .Select(ip => ip.ToString())
            );
            
        }

        public DelegateCommand AddDeviceCmd { get; set; }
        private void AddDevice()
        {
            if (!int.TryParse(PeriodTime,out var a))
            {
                MessageBox.Show("轮询间隔请输入整数，且不能为空");
                return;
            }
            if (!int.TryParse(SalveID,out var b))
            {
                MessageBox.Show("从站地址请输入整数，且不能为空");
                return;
            }
            switch (SelectModbus.Key)
            {
                case ModbusEnum.Modbus.SerialPort:
                    AddSerialDevice();
                    break;
                case ModbusEnum.Modbus.TCP:
                    AddTcpDevice();
                    break;
            }
           
        }
        public DelegateCommand CloseCmd => new DelegateCommand(() =>
        {
            RequestClose.Invoke();
        });
        private void AddTcpDevice()
        {
           if (!CanWriteIP && string.IsNullOrEmpty(IpAdresss))
            {
                MessageBox.Show("Ip不能为空");
                return;
            }
           TcpModel tcpModel = new TcpModel();
            tcpModel.Port = Port;
            tcpModel.Ip=CanWriteIP==false?IpAdresss:string.Empty;
            DeviceModel deviceModel = new DeviceModel() 
            {
                Config = tcpModel,
                Port = Port,
                PeriodTime = Convert.ToInt32(PeriodTime),
                Type="TCPModbus",Protocol=SelectModbus.Key,
                ConnectionString= IpAdresss + ":" + Port.ToString(),
                Channel= new IF_Implements.Channel.ModbusReadChannel(),
                Name=IpAdresss+":"+Port.ToString(),SlaveId=Convert.ToInt32(SalveID)
            };
            if (!ModbusDictManger.ModbusMangeDict.ContainsKey(deviceModel.Name))
            { 
                deviceModel.Channel.OpenAsyance(deviceModel);
                if (deviceModel.Channel.IsConnected)
                {
                    deviceModel.Status = "已连接";
                    ModbusDictManger.AddDevice(deviceModel);
                    RequestClose.Invoke();
                }
            }
            else MessageBox.Show("当前IP已经连接");
           
        }

        private void AddSerialDevice()
        {
            if (string.IsNullOrEmpty(SelectPortName))
            {
                MessageBox.Show("串口名字不能为空");
                return;
            }
            var SerialParameter = new SerialPortModel();
            SerialParameter.PortName = SelectPortName;
            SerialParameter.BaudRate = SelectedBaudRate;
            SerialParameter.Parity = SelectedParity;
            SerialParameter.StopBits = SelectedStopBits;
            SerialParameter.DataBits = SelectedDataBits;
            SerialParameter.RtsEnable = IsRTS;
            SerialParameter.DtrEnable = IsDSR;
            SerialParameter.RTSDeily = RTSDeily;
            SerialParameter.CTsEnable = IsCTS;
            DeviceModel deviceModel = new DeviceModel()
            {
                Name = SelectPortName,
                Config = SerialParameter,
                Channel = new IF_Implements.Channel.ModbusReadChannel(),
                Protocol = SelectModbus.Key,
                SerialPortType = SerialPortType == true ? ModbusEnum.SerialPortType.RTU : ModbusEnum.SerialPortType.ASCII,
                Type = SerialPortType == true ? "RTUModbus" : "ASCIIModbus",
                Port=0,PeriodTime=Convert.ToInt32(PeriodTime),
                ConnectionString="串口",SlaveId=Convert.ToInt32(SalveID)
            };
            deviceModel.Channel.OpenAsyance(deviceModel);
            if (deviceModel.Channel.IsConnected)
            {
                deviceModel.Status = "已连接";
                ModbusDictManger.AddDevice(deviceModel);
                RequestClose.Invoke();
            }
            
        }
        #region 参数设置
        private ObservableCollection<KeyValuePair<ModbusEnum.Modbus,string>> _Modbus;

        public ObservableCollection<KeyValuePair<ModbusEnum.Modbus, string>> Modbus
        {
            get { return _Modbus; }
            set { _Modbus = value;
                RaisePropertyChanged();
            }
        }

        private KeyValuePair<ModbusEnum.Modbus,string> _SelectModbus;

        public KeyValuePair<ModbusEnum.Modbus,string> SelectModbus
        {
            get { return _SelectModbus; }
            set { _SelectModbus = value;
                RaisePropertyChanged();
            }
        }

        private bool _IsRTS;

        public bool IsRTS
        {
            get { return _IsRTS; }
            set { _IsRTS = value;
                RaisePropertyChanged();
            }
        }
        private bool _IsCTS;

        public bool IsCTS
        {
            get { return _IsCTS; }
            set { _IsCTS = value;
                RaisePropertyChanged();
            }
        }
        private bool _IsDSR=true;

        public bool IsDSR
        {
            get { return _IsDSR; }
            set { _IsDSR = value;
                RaisePropertyChanged();
            }
        }
        public int _RTSDeily=1;
        public int RTSDeily
        {
            get { return _RTSDeily; }
            set { _RTSDeily = value;
                RaisePropertyChanged();
            }
        }
        private bool _SerialPortType=true;

        public bool SerialPortType
        {
            get { return _SerialPortType; }
            set { _SerialPortType = value;
                RaisePropertyChanged();
            }
        }
        private string _PeriodTime;

        public string PeriodTime
        {
            get { return _PeriodTime; }
            set { _PeriodTime = value;
                RaisePropertyChanged();
                
            }
        }

        private string _IpAdresss;

        public string IpAdresss
        {
            get { return _IpAdresss; }
            set { _IpAdresss = value;
                RaisePropertyChanged();
            }
        }
        private int _Port=502;

        public int Port
        {
            get { return _Port; }
            set { _Port = value;
                RaisePropertyChanged();
            }
        }
        private bool _canWriteIP;
        public bool CanWriteIP
        {
            get => _canWriteIP;
            set
            {
                if (_canWriteIP != value)
                {
                    _canWriteIP = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(CanWriteIPText)); // 通知反向属性刷新
                }
            }
        }

        public bool CanWriteIPText => !CanWriteIP; // 直接取反，不用单独存储
        private ObservableCollection<string> _IpAdressList;

        public ObservableCollection<string> IpAdressList
        {
            get { return _IpAdressList; }
            set { _IpAdressList = value;
                RaisePropertyChanged();
            }
        }

        private string _SalveID;

        public string SalveID
        {
            get { return _SalveID; }
            set { _SalveID = value;
                RaisePropertyChanged();
            }
        }


        #region 串口参数
        private string _SelectPortName;

        public string SelectPortName
        {
            get { return _SelectPortName; }
            set { _SelectPortName = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<int> _BaudRates = new ObservableCollection<int>()
        { 9600, 19200, 38400, 57600, 115200 };
        public ObservableCollection<int> BaudRates
        {
            get { return _BaudRates; }
            set { SetProperty(ref _BaudRates, value); }
        }

        // 数据位
        private ObservableCollection<int> _DataBits = new ObservableCollection<int>() { 5, 6, 7, 8 };
        public ObservableCollection<int> DataBitsList
        {
            get { return _DataBits; }
            set { SetProperty(ref _DataBits, value); }
        }

        // 停止位
        private ObservableCollection<StopBits> _StopBits = new ObservableCollection<StopBits>()
        { StopBits.One, StopBits.OnePointFive, StopBits.Two };
        public ObservableCollection<StopBits> StopBitsList
        {
            get { return _StopBits; }
            set { SetProperty(ref _StopBits, value); }
        }

        // 校验位
        private ObservableCollection<Parity> _Parities = new ObservableCollection<Parity>()
        { Parity.None, Parity.Odd, Parity.Even, Parity.Mark, Parity.Space };
        public ObservableCollection<Parity> Parities
        {
            get { return _Parities; }
            set { SetProperty(ref _Parities, value); }
        }

        #endregion

        #region 串口参数绑定
        private int _SelectedBaudRate = 9600;
        public int SelectedBaudRate
        {
            get { return _SelectedBaudRate; }
            set { SetProperty(ref _SelectedBaudRate, value); }
        }

        private int _SelectedDataBits = 8;
        public int SelectedDataBits
        {
            get { return _SelectedDataBits; }
            set { SetProperty(ref _SelectedDataBits, value); }
        }

        private StopBits _SelectedStopBits = StopBits.One;
        public StopBits SelectedStopBits
        {
            get { return _SelectedStopBits; }
            set { SetProperty(ref _SelectedStopBits, value); }
        }

        private Parity _SelectedParity = Parity.None;
        public Parity SelectedParity
        {
            get { return _SelectedParity; }
            set { SetProperty(ref _SelectedParity, value); }
        }


        #endregion

        #endregion
    }
}
