using IntelliMonWPF.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.Models
{
    
    public class SendModel
    {
        public byte SavelId { get; set; }
        public ushort StartAddre { get; set; }
        public ModbusEnum.SendType SendType { get; set; }
        public Dictionary<ModbusEnum.SendType, Data> SendDataTypr { get; set; } = new Dictionary<ModbusEnum.SendType, Data>();
    };
    public class Data
    {
        public bool? Statu { get; set; }
        public bool[]? Status { get; set; }
        public ushort? arr {  get; set; }
        public ushort[]? arrs { get; set; }
    }
}
