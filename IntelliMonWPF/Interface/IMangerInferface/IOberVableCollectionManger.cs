using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.Interface.IMangerInferface
{
    public interface IOberVableCollectionManger<T>
    {
        void Add(T dto);
        void Remove(T dto);
        IEnumerable<T> GetAll();
        int Count { get; }
        ReadOnlyObservableCollection<T> GetBing();
    }
}
