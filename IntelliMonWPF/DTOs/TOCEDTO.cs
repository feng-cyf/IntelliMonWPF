using IntelliMonWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.DTOs
{
    internal class TOCEDTO
    {
        public string part { get; set; }
        public string name { get; set; }
        public string Type { get; set; }
        public List<PointModel> l { get; set; }
    }
}
