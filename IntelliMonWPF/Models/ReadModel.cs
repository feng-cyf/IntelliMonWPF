using IntelliMonWPF.Enum;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.Models
{
    public class ReadModel : INotifyPropertyChanged
    {
        public KeyValuePair<string,string> Name=> new KeyValuePair<string, string>($"{DeviceName}-{SlaveId.ToString()}-{StartAddress}",
            string.IsNullOrWhiteSpace(PointModels?.PointName)? $"{DeviceName}-{SlaveId.ToString()}-{StartAddress}":PointModels.PointName);
        public ObservableCollection<KeyValuePair<ModbusEnum.SendType, string>> SendFuction => Function switch
        {
            "01" =>new ObservableCollection<KeyValuePair<ModbusEnum.SendType, string>>()
            {
                new KeyValuePair<ModbusEnum.SendType, string>(ModbusEnum.SendType.WriteSingleCoil, "发送单个线圈") ,new KeyValuePair<ModbusEnum.SendType, string>(ModbusEnum.SendType.WriteMultipleCoils,"发送多个线圈")
            },
            "02"=>throw new NotImplementedException(),
            "03"=>new ObservableCollection<KeyValuePair<ModbusEnum.SendType, string>>() { new KeyValuePair<ModbusEnum.SendType, string>(ModbusEnum.SendType.WriteSingleRegister, "发送单个寄存器"), new KeyValuePair<ModbusEnum.SendType, string>(ModbusEnum.SendType.WriteMultipleRegisters, "发送多个寄存器") },
            "04"=>throw new NotImplementedException(),
            _=>throw new NotImplementedException()
        };
        public string DeviceName {  get; set; }
        public bool IsAutoCon { get; set; } = false;
        public int MaxRebuile { get; set; } = 3;
        public ModbusEnum.Modbus Modbus { get; set; }
        public int ErrorCount { get; set; }
        public int MaxError => Modbus switch 
        {
            ModbusEnum.Modbus.SerialPort =>1,
            ModbusEnum.Modbus.TCP=>3,
            _=>throw new InvalidOperationException()
        };
        public CancellationTokenSource cts { get; set; }=new CancellationTokenSource();
        private ModbusEnum.ModbusRead _ModbusRead;

        public ModbusEnum.ModbusRead ModbusRead
        {
            get { return _ModbusRead; }
            set { _ModbusRead = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Function));
            }
        }
        private int _StartAddress;

        public int StartAddress
        {
            get { return _StartAddress; }
            set { _StartAddress = value;
                OnPropertyChanged();
            }
        }
        private int _NumAddress=8;

        public int NumAddress
        {
            get { return _NumAddress; }
            set { _NumAddress = value;
                OnPropertyChanged();
            }
        }

        private int _SlaveId;

        public int SlaveId
        {
            get { return _SlaveId; }
            set { _SlaveId = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SlaveId))); }
        }

        private string _Status;

        public string Status
        {
            get { return _Status; }
            set { _Status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        private int _Interavel;

        public int Interavel
        {
            get { return _Interavel; }
            set { _Interavel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Interavel))); }
        }
        public string Device => Function switch
        {
            "01" => "Coil",
            "02" => "Discrete Input",
            "03" => "Holding Register",
            "04" => "Input Register",
            _ => "Unknown"
        };

        public string Function => ModbusRead switch
        {
            ModbusEnum.ModbusRead.ReadCoils => "01",
            ModbusEnum.ModbusRead.ReadInputCoils => "02",
            ModbusEnum.ModbusRead.ReadRegisters => "03",
            ModbusEnum.ModbusRead.ReadInputRegister => "04",
            _ => "未知类型"
        };
        public PointModel PointModels { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;
        //界面参数
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class PointModel : INotifyPropertyChanged
    {
        private string? _pointName;
        private string? _startAddress;
        private int _len;
        private string _registerType = string.Empty;
        private string? _dataType;
        private string? _accessType;
        private string? _unit;
        private string? _scaleFactor="1.0";
        private string? _offset="1";
        private string? _desc;
        public KeyValuePair<string, (string, int)> mapDevice {  get; set; }
        public string? PointName
        {
            get => _pointName;
            set
            {
                _pointName = value;
                RaisePropertyChanged();
            }
        }

        public string? StartAddress
        {
            get => _startAddress;
            set
            {
                _startAddress = value;
                RaisePropertyChanged();
            }
        }

        public int Len
        {
            get => _len;
            set
            {
                _len = value;
                RaisePropertyChanged();
            }
        }

        public string RegisterType
        {
            get => _registerType;
            set
            {
                _registerType = value;
                RaisePropertyChanged();
            }
        }

        public string? DataType
        {
            get => _dataType;
            set
            {
                _dataType = value;
                RaisePropertyChanged();
            }
        }

        public string? AccessType
        {
            get => _accessType;
            set
            {
                _accessType = value;
                RaisePropertyChanged();
            }
        }

        public string? Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                RaisePropertyChanged();
            }
        }

        public string? ScaleFactor
        {
            get => _scaleFactor;
            set
            {
                _scaleFactor = value;
                RaisePropertyChanged();
            }
        }

        public string? Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                RaisePropertyChanged();
            }
        }

        public string? Desc
        {
            get => _desc;
            set
            {
                _desc = value;
                RaisePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
