using IntelliMonWPF.Interface.IMangerInferface;
using IntelliMonWPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.IF_Implements.MangerInferface
{
    public class MobusObserVableManger : IOberVableCollectionManger<DeviceModel>
    {
        public ObservableCollection<DeviceModel> DeviceList { get; } = new ObservableCollection<DeviceModel>();
        private ReadOnlyObservableCollection<DeviceModel> _readOnlyObservableCollection;
        public MobusObserVableManger()
        {
            _readOnlyObservableCollection = new ReadOnlyObservableCollection<DeviceModel>(DeviceList);
        }
        public int Count => DeviceList.Count;

        public void Add(DeviceModel dto)
        {
            if (dto == null) return;
            if (DeviceList.Any(d => d.DeviceName == dto.DeviceName))
                return;
            DeviceList.Add(dto);
        }

        public IEnumerable<DeviceModel> GetAll()
        {
            return DeviceList;
        }

        public ReadOnlyObservableCollection<DeviceModel> GetBing()
        {
            return _readOnlyObservableCollection;
        }

        public void Remove(DeviceModel dto)
        {
            if (dto == null) return;
            DeviceList.Remove(dto);
        }
    }
}
