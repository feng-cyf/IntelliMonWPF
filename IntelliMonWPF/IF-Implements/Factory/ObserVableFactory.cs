using IntelliMonWPF.IF_Implements.MangerInferface;
using IntelliMonWPF.Interface.IFactory;
using IntelliMonWPF.Interface.IMangerInferface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.IF_Implements.Factory
{
    public class ObserVableFactory : IObserVableCollectionFactory
    {
        private MobusObserVableManger mobusObserVableManger;
        public ObserVableFactory(MobusObserVableManger mobusObserVableManger) 
        {
            this.mobusObserVableManger = mobusObserVableManger;
        }
        public IOberVableCollectionManger<T> CreateOberVableCollectionManger<T>(OberVableCollectionType type) where T : class
        {
            switch(type) 
            {
                case OberVableCollectionType.DeviceModel:
                    return mobusObserVableManger as IOberVableCollectionManger<T>;
                default:
                    throw new NotImplementedException();
            }
        }
    }
    public enum OberVableCollectionType
    {
        DeviceModel
    }
}
