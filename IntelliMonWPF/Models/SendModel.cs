using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.Models
{
    
    class SendModel<T>
    {
        public byte SavelId { get; set; }
        public ushort StartAddre { get; set; }
        public T Data {  get; set; }
    }
}
