using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.DeviceConfig
{
    internal class SerialRtuReadCon
    {
        private ModbusMaster master;
        private CancellationTokenSource cts;
        public SerialRtuReadCon(ModbusMaster master)
        {
            this.master = master;
        }
        public void Stpo()
        {
            cts?.Cancel();
            cts?.Dispose();
        }
    }
}
