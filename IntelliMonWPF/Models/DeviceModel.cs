using IntelliMonWPF.Enum;
using IntelliMonWPF.Interface;
using IntelliMonWPF.Interface.Ichannel;
using Prism.Ioc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace IntelliMonWPF.Models
{
    public class DeviceModel : BindableBase
    {
        internal ConcurrentDictionary<(string,int),HashSet<int>> AdressSet { get; set; } = new ConcurrentDictionary<(string, int), HashSet<int>>();

        public string Type { get; set; }
        public string DeviceName { get; set; }
        public object Config { get; set; }
        public int HeartSize { get; set; } = 1;
        public IModbusReadChannel Channel { get; set; }  // 设备的接口实现
        public ModbusEnum.Modbus Protocol { get; set; } 
        public ModbusEnum.SerialPortType SerialPortType { get; set; }
        internal Dictionary<(string DeviceName, int SlaveId,int StartAdress), ReadModel> readMangerModbus { get; set; } = 
            new Dictionary<(string DeviceName, int SlaveId, int StartAdress), ReadModel>();
        public ObservableCollection<ReadModel> ReadModels { get; set; } = new();
       
        private string _Status;

        public string Status
        {
            get { return _Status; }
            set { _Status = value;
                RaisePropertyChanged();
            }
        }
    }
}
