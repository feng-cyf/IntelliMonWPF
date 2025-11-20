using IntelliMonWPF.Event;
using IntelliMonWPF.Interface.IMangerInferface;
using IntelliMonWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.IF_Implements.MangerInferface
{
    public class ModbusDictManger : IDictManger<string,DeviceModel>
    {
        private IEventAggregator _eventAggregator;
        public ModbusDictManger(IEventAggregator eventAggregator) 
        {
            _eventAggregator = eventAggregator;
        }
        public Dictionary<string, DeviceModel> DeviceDictManger { get; set; } = new Dictionary<string, DeviceModel>();

        public IReadOnlyDictionary<string, DeviceModel> Data => DeviceDictManger;

        public void Add(DeviceModel device)
        {
            if (DeviceDictManger.ContainsKey(device.DeviceName))
            {
                _eventAggregator.GetEvent<ShowMesssgeWindow>().Publish(new Tuple<string, int>($"{device.DeviceName}已经存在",3));
                return;
            }
            DeviceDictManger.Add(device.DeviceName, device);
        }

        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                _eventAggregator.GetEvent<ShowMesssgeWindow>().Publish(new Tuple<string, int>($"传入参数为空", 3));
                return false;
            }
            return DeviceDictManger.ContainsKey(key);
        }

        public IEnumerable<DeviceModel> GetAllValues()
        {
            return DeviceDictManger.Values;
        }

        public IEnumerable<string> GetAllKeys()
        {
            return DeviceDictManger.Keys;
        }

        public DeviceModel GetValue(string dto)
        {
            if (dto == null) 
            {
                _eventAggregator.GetEvent<ShowMesssgeWindow>().Publish(new Tuple<string, int>($"传入参数为空", 3));
                return null;
            }   
            if(!DeviceDictManger.TryGetValue(dto.ToString(), out DeviceModel device))
            {
               if(device==null)
                    _eventAggregator.GetEvent<ShowMesssgeWindow>().Publish(new Tuple<string, int>($"{dto.ToString()}不存在", 3));
            }
            return device;
        }

        public void Remove(DeviceModel dto)
        {
            if (DeviceDictManger.ContainsKey(dto.DeviceName))
                DeviceDictManger.Remove(dto.DeviceName);
        }
    }
}
