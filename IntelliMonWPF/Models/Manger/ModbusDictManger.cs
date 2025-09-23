using IntelliMonWPF.Helper;
using IntelliMonWPF.Interface;
using Prism.Ioc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IntelliMonWPF.Models.Manger
{
     class ModbusDictManger
     {
        public int locationPore { get; set; } = 1502;
        private object _lock=new object();
        public BoundedDeque<string> MoudbusQueue { get; set; }=new BoundedDeque<string>(500);
        public Dictionary<string,DeviceModel> ModbusMangeDict { get; set; } = new Dictionary<string, DeviceModel>();
        public ObservableCollection<DeviceModel> ModbusMangeList { get; set; } = new ObservableCollection<DeviceModel>();
        private IMessages messages = ContainerLocator.Container.Resolve<IMessages>();
        public void AddDevice(DeviceModel device)
        {
            
            if (ModbusMangeDict.ContainsKey(device.DeviceName))
            {
               messages.ShowMessage($"{device.DeviceName}已经存在");
                return;
            }
            ModbusMangeDict.Add(device.DeviceName, device);
            ModbusMangeList.Add(device);
        }
        public void UpgradeDevice() 
        {
           
        }
        public void Remove(string Name,int id,ReadModel readModel) 
        {
            ModbusMangeDict[Name].readMangerModbus.Remove((Name,id));
            ModbusMangeDict[Name].ReadModels.Remove(readModel);
        }
        public int LocationPort()
        {
            lock (_lock)
            {
                return locationPore++;
            }
        }
    }
}
