using IntelliMonWPF.IF_Implements.MangerInferface;
using IntelliMonWPF.Interface.IFactory;
using IntelliMonWPF.Interface.IMangerInferface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.IF_Implements.Factory
{
    public class DictMangerFactory: IDictMangerFactory
    {
        private ModbusDictManger _modbusDictManger;
        private IEventAggregator _eventAggregator;
        public DictMangerFactory(EventAggregator eventAggregator,ModbusDictManger modbusDictManger) 
        {
            _eventAggregator = eventAggregator;
            _modbusDictManger = modbusDictManger;
        }

        public IDictManger<TKey, T> CreateDictManger<TKey, T>(DictMangerType type) where T : class
        {
            switch (type)
            {
                case DictMangerType.DeviceModel:
                    return _modbusDictManger as IDictManger<TKey, T>;
                default:
                    throw new NotImplementedException();
            }
        }
    }
    public enum DictMangerType
    {
        DeviceModel
    }
}
