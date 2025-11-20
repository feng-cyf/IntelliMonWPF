using IntelliMonWPF.IF_Implements.Factory;
using IntelliMonWPF.Interface.IMangerInferface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.Interface.IFactory
{
    public interface IObserVableCollectionFactory
    {
        IOberVableCollectionManger<T> CreateOberVableCollectionManger<T>(OberVableCollectionType type) where T : class;
    }
}
