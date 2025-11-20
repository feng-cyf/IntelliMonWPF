using IntelliMonWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.Interface.IMangerInferface
{
    public interface IDictManger<TKey, TValue>
    {
        IReadOnlyDictionary<TKey, TValue> Data { get; }
        void Add(TValue value);
        void Remove(TValue value);
        TValue GetValue(TKey key);
        bool ContainsKey(TKey key);
        IEnumerable<TValue> GetAllValues();
        IEnumerable<TKey> GetAllKeys();
    }
}