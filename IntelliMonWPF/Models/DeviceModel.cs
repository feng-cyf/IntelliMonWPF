using IntelliMonWPF.Enum;
using IntelliMonWPF.Interface.Ichannel;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace IntelliMonWPF.Models
{
    internal class DeviceModel : INotifyPropertyChanged
    {
        public DeviceModel()
        {
            ReadModel=new ReadModel();
        }
        // ----------- 不常变化的属性（保持自动属性） -------------
        public string DeviceName { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Port { get; set; }
        public object Config { get; set; }
        public IModbusReadChannel Channel { get; set; }  // 设备的接口实现
        public ModbusEnum.Modbus Protocol { get; set; } // 协议类型
        public ReadModel ReadModel { get; set; }
        public ModbusEnum.SerialPortType SerialPortType { get; set; }

        // ----------- 需要通知 UI 的属性 -------------
        private int _slaveId;
        public int SlaveId
        {
            get => _slaveId;
            set { if (_slaveId != value) { _slaveId = value; OnPropertyChanged(nameof(SlaveId)); } }
        }
        

        private int _periodTime;
        public int PeriodTime
        {
            get => _periodTime;
            set { if (_periodTime != value) { _periodTime = value; OnPropertyChanged(nameof(PeriodTime)); } }
        }

        private string _status;
        public string Status
        {
            get => _status;
            set { if (_status != value) { _status = value; OnPropertyChanged(nameof(Status)); } }
        }

        private string _connectionString;
        public string ConnectionString
        {
            get => _connectionString;
            set { if (_connectionString != value) { _connectionString = value; OnPropertyChanged(nameof(ConnectionString)); } }
        }

        private KeyValuePair<string, string> _function = new KeyValuePair<string, string>("04", "输入寄存器读取");
        public KeyValuePair<string, string> Function
        {
            get => _function;
            set { if (!_function.Equals(value)) { _function = value; OnPropertyChanged(nameof(Function)); } }
        }

        // ----------- INotifyPropertyChanged 实现 -------------
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
