using System;
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
        public Dictionary<string,DeviceModel> ModbusMangeDict { get; set; } = new Dictionary<string, DeviceModel>();
        public ObservableCollection<DeviceModel> ModbusMangeList { get; set; } = new ObservableCollection<DeviceModel>();
        public void AddDevice(DeviceModel device)
        {
            var conflict = ModbusMangeList.Any(d =>
                          d.ConnectionString == device.ConnectionString &&
                          d.Port == device.Port &&
                          d.SlaveId == device.SlaveId);

            if (conflict)
            {
                MessageBox.Show($"在 {device.ConnectionString}:{device.Port} 下，SlaveId={device.SlaveId} 已经存在！");
                return;
            }
            if (ModbusMangeDict.ContainsKey(device.ConnectionString))
            {
                MessageBox.Show($"{device.ConnectionString}已经存在");
                return;
            }
            ModbusMangeDict.Add(device.ConnectionString, device);
            ModbusMangeList.Add(device);
        }
        public void UpgradeDevice() 
        {
           
        }

    }
}
