using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.Models
{
    internal class ReadDataModel
    {
        public bool coil {  get; set; }
        public bool[] Coils { get; set; }
        public ushort register { get; set; }
        public ushort[] Registers { get; set; }
    }
}
