using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.Event.IEvents
{
    internal interface ICheckDataBus
    {
        IDisposable SubPublish<T>(Action<T> action) where T:IEvent;
        IDisposable SubPublish<T>(Func<T,Task> func) where T:IEvent;
        void Publish<T>(T evt) where T:IEvent;
        Task PublishAsync<T>(T evt) where T:IEvent;
    }
}
