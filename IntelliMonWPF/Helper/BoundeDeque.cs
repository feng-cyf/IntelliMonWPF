using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.Helper
{
    using System.Collections.Generic;

    internal class BoundedDeque<T>
    {
        private readonly LinkedList<T> _deque = new();
        private readonly int _maxSize;
        private readonly object _lock = new();

        public BoundedDeque(int maxSize)
        {
            _maxSize = maxSize;
        }

        // 入队（自动裁剪）
        public void Add(T item)
        {
            lock (_lock)
            {
                _deque.AddLast(item);
                if (_deque.Count > _maxSize)
                    _deque.RemoveFirst();
            }
        }

        // 出队（FIFO）
        public bool TryDequeue(out T item)
        {
            lock (_lock)
            {
                if (_deque.Count == 0)
                {
                    item = default!;
                    return false;
                }
                item = _deque.First!.Value;
                _deque.RemoveFirst();
                return true;
            }
        }

        // 当前数量
        public int Count
        {
            get { lock (_lock) return _deque.Count; }
        }
        public void Clear() 
        {
            lock (_lock) { _deque.Clear(); }
        }
      
    }

}
