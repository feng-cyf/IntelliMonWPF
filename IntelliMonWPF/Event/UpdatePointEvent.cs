using IntelliMonWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.Event
{
    public class UpdatePointClass
    {
        public string DeviceName { get; set; }
        public string PointName { get; set; }
        public int PointId { get; set; }
        public int Length { get; set; }
        public int StartAddress { get; set; }
        public string RegisterType { get; set; }

    }
    internal static class UpdatePointEvent
    {
        public static event Action<UpdatePointClass> GetUpdatePointEvent;
        public static void OnGetUpdatePointEvent(UpdatePointClass pointModel)
        {
            GetUpdatePointEvent?.Invoke(pointModel);
        }
    }
}
