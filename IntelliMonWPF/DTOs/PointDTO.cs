using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.DTOs
{
    public class PointDTO
    {
        public string DeviceName { get; set; }
        public int SlaveId { get; set; }
        public string PointName { get; set; }
        public int Len { get; set; }
        public string RegisterType { get; set; }
        public string DataType {  get; set; }
        public string AccessType {  get; set; }
        public string Unit { get; set; }
        public string ScaleFactor { get; set; }
        public string Offset {  get; set; }
        public string Desc { get; set; }
    }
}