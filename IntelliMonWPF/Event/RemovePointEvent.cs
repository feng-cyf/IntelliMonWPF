using IntelliMonWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.Event
{
    internal static class RemovePointEvent
    {
        public static event Action<PointModel> GetRemovePointEvent;
        public static void OnGetRemovePointEvent(PointModel pointModel)
        {
            GetRemovePointEvent?.Invoke(pointModel);
        }
    }
    
}
