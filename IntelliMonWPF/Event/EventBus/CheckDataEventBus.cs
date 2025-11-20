using IntelliMonWPF.Event.IEvents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace IntelliMonWPF.Event.EventBus
{
    internal class CheckDataEventBus : ICheckDataBus
    {
        private readonly ConcurrentDictionary<Type, ConcurrentBag<object>> _dict = new ConcurrentDictionary<Type, ConcurrentBag<object>>();

        public void Publish<T>(T evt) where T : IEvent
        {
            var type = typeof(T);
            if (!_dict.TryGetValue(type, out var bag)) return;
            foreach (var action in bag.ToArray()) 
            {
                try
                {
                    if (action is Action<T> act)
                    {
                        act(evt);
                    }
                    else if (action is Func<T, Task> func)
                    {
                        func(evt).GetAwaiter().GetResult();
                    }
                }
                catch (Exception ex) {  } 
            }
        }

        public async Task PublishAsync<T>(T evt) where T : IEvent
        {
            var type = typeof(T);
            if (!_dict.TryGetValue(type, out var bag)) return;
            object[] snap;
            var tasks = new ConcurrentBag<Task>();
            snap = bag.ToArray();
            foreach (var action in snap)
            {
                if (action is Func<T, Task> func)
                {
                    tasks.Add(SafeRunAsync(func, evt));
                }
            }
            await Task.WhenAll(tasks);
        }

        private async Task SafeRunAsync<T>(Func<T, Task> func, T evt)
        {
            try
            {
                await func(evt);
            }
            catch (Exception)
            {
                throw; 
            }
        }

        // 保留接口定义的SubPublish（Action<T>重载）
        public IDisposable SubPublish<T>(Action<T> action) where T : IEvent
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            var type = typeof(T);

            _dict.AddOrUpdate(
                type,
                _ => new ConcurrentBag<object> { action },
                (_, bag) =>
                {
                    bag.Add(action);
                    return bag;
                });

            // 修复退订逻辑（不删除方法，仅修正实现）
            return new Subscription(() =>
            {
                if (_dict.TryGetValue(type, out var bag))
                {
                    object temp;
                    var remaining = new ConcurrentBag<object>();
                    bool removed = false;

                    while (bag.TryTake(out temp))
                    {
                        if (!removed && temp == action)
                        {
                            removed = true;
                        }
                        else
                        {
                            remaining.Add(temp);
                        }
                    }

                    foreach (var item in remaining)
                    {
                        bag.Add(item);
                    }

                    if (bag.IsEmpty)
                    {
                        _dict.TryRemove(type, out _);
                    }
                }
            });
        }

        public IDisposable SubPublish<T>(Func<T, Task> func) where T : IEvent
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            var type = typeof(T);

            _dict.AddOrUpdate(
                type,
                _ => new ConcurrentBag<object> { func },
                (_, bag) =>
                {
                    bag.Add(func);
                    return bag;
                });

            return new Subscription(() =>
            {
                if (_dict.TryGetValue(type, out var bag))
                {
                    object temp;
                    var remaining = new ConcurrentBag<object>();
                    bool removed = false;

                    while (bag.TryTake(out temp))
                    {
                        if (!removed && temp == func)
                        {
                            removed = true;
                        }
                        else
                        {
                            remaining.Add(temp);
                        }
                    }

                    foreach (var item in remaining)
                    {
                        bag.Add(item);
                    }

                    if (bag.IsEmpty)
                    {
                        _dict.TryRemove(type, out _);
                    }
                }
            });
        }

        private sealed class Subscription : IDisposable
        {
            private Action _dispose;
            public Subscription(Action dispose) => _dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));
            public void Dispose() => Interlocked.Exchange(ref _dispose, null)?.Invoke();
        }
    }
}