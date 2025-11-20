using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.Helper.Tools
{
    public class SingelTool
    {
        private static readonly Lazy<SingelTool> _instance = new Lazy<SingelTool>(() => new SingelTool());
        private SingelTool() { }
        public static SingelTool singelTool=> _instance.Value;
        public int locationPore { get; set; } = 1502;
        private object _lock = new object();
        internal BoundedDeque<string> MoudbusQueue { get; set; } = new BoundedDeque<string>(500);
        public int LocationPort()
        {
            lock (_lock)
            {
                return locationPore++;
            }
        }
    }
}
