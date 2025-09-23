using IntelliMonWPF.Enum;
using IntelliMonWPF.Interface;
using IntelliMonWPF.Interface.Ichannel;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace IntelliMonWPF.Models
{
    internal class DeviceModel : BindableBase
    {
       
        public string Type { get; set; }
       
        public string DeviceName { get; set; }
        public object Config { get; set; }
        // ----------- 不常变化的属性（保持自动属性） -------------
        public IModbusReadChannel Channel { get; set; }  // 设备的接口实现
        public ModbusEnum.Modbus Protocol { get; set; } // 协议类型
        public ModbusEnum.SerialPortType SerialPortType { get; set; }
        public Dictionary<(string DeviceName, int SlaveId), ReadModel> readMangerModbus { get; set; } = new Dictionary<(string DeviceName, int SlaveId), ReadModel>();
        // DeviceModel
        public ObservableCollection<ReadModel> ReadModels { get; set; } = new();
        private IMessages messages= ContainerLocator.Container.Resolve<IMessages>();

        // ----------- INotifyPropertyChanged 实现 -------------        
        public void Add(string DeviceName,int SlaveId,ReadModel readModel)
        {
            if (readMangerModbus.Keys.Contains((DeviceName, SlaveId)))
            {
               messages.ShowMessage("该设备已经存在");
            }
            else
            {
                readMangerModbus.Add((DeviceName, SlaveId), readModel);
                ReadModels.Add(readModel);
            }
        }
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
