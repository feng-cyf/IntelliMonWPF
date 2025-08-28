using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.Models
{
    
    public class SerialPortModel
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public Parity Parity { get; set; }
        public bool RtsEnable { get; set; }
        public bool DtrEnable { get; set; }
        public bool CTsEnable { get; set; }
        public int RTSDeily { get; set; }
    }
}
