using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.DTOs
{
    public class DataSendDTO
    {
        [JsonProperty("DeviceName")]
        public string DeviceName { get; set; }
        [JsonProperty("SlaveId")]
        public int SlaveId { get; set; }
        public List<float> ushorts { get; set; }
    }
}
