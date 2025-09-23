using IntelliMonWPF.Enum;
using IntelliMonWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.Interface.Ichannel
{
    internal interface IModbusReadChannel
    {
        bool IsConnected { get; }
        Task OpenAsyance(DeviceModel deviceModel);
        Task CloseAsyance();
        event Action<byte[]> DataReceived;
        Task SendAsyance<T>(T data);
        Task ReadAsyance(ReadModel readModel);
    }
}
