using IntelliMonWPF.Event;
using IntelliMonWPF.Interface;
using IntelliMonWPF.Models;

public class DeviceModelServerClass
{
    private readonly DeviceModel _deviceModel;
    private readonly IMessages _messages= ContainerLocator.Container.Resolve<IMessages>();

    public DeviceModelServerClass(DeviceModel deviceModel)
    {
        _deviceModel = deviceModel ?? throw new ArgumentNullException(nameof(deviceModel));
    }

    //public ReadModel GetReadModel(string deviceName, int slaveId, int startAddress)
    //{
    //    var key = (deviceName, slaveId, startAddress);
    //    if (_deviceModel.readMangerModbus.TryGetValue(key, out var readModel))
    //    {
    //        return readModel;
    //    }
    //    return null;
    //}

    public bool ContainsReadModel(string deviceName, int slaveId, int startAddress)
    {
        var key = (deviceName, slaveId, startAddress);
        return _deviceModel.readMangerModbus.ContainsKey(key);
    }

    public bool AddReadModel(string deviceName, ReadModel readModel)
    {
        var key = (deviceName, readModel.SlaveId, readModel.StartAddress);
        var set=_deviceModel.AdressSet.GetOrAdd((deviceName, readModel.SlaveId), new HashSet<int>());
        var addresses = Enumerable.Range(readModel.StartAddress, readModel.NumAddress);
        if (set != null)
        {
            if (addresses.Any(addr => set.Contains(addr)))
            {
                _messages.ShowMessage("该读取点已被占用");
                return false;
            }

            if (ContainsReadModel(deviceName, readModel.SlaveId, readModel.StartAddress))
            {
                _messages.ShowMessage("该设备已经存在");
                return false;
            }
        }
        foreach (var addr in addresses)
        {
            set?.Add(addr);
        }
        _deviceModel.readMangerModbus.Add(key, readModel);
        _deviceModel.ReadModels.Add(readModel);
        return true;
    }

    public bool RemoveReadModel(string deviceName, int slaveId, int startAddress)
    {
        var SetKey = (deviceName, slaveId);
        _deviceModel.AdressSet.TryGetValue(SetKey,out var set);
        var key = (deviceName, slaveId, startAddress);
        if (!_deviceModel.readMangerModbus.TryGetValue(key, out var readModel))
        {
            _messages.ShowMessage("该设备不存在");
            return false;
        }

        var addresses = Enumerable.Range(readModel.StartAddress, readModel.NumAddress);
        foreach (var addr in addresses)
        {
            set?.Remove(addr);
        }
        RemovePointEvent.OnGetRemovePointEvent(readModel.PointModels);
        _deviceModel.readMangerModbus.Remove(key);
        _deviceModel.ReadModels.Remove(readModel);
        return true;
    }

    public List<ReadModel> GetAllReadModels()
    {
        return _deviceModel.readMangerModbus.Values.ToList();
    }
}