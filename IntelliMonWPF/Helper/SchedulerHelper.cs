using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliMonWPF.Helper
{
    public class SchedulerHelper : TaskScheduler, IDisposable
    {
        private static readonly Lazy<SchedulerHelper> _Instance=new Lazy<SchedulerHelper>(()=>new SchedulerHelper(50));
        public static SchedulerHelper Instance => _Instance.Value;
        // 2. 改用 AutoResetEvent（自动重置信号量），避免假唤醒和空转
        private readonly AutoResetEvent _are = new AutoResetEvent(false);
        private readonly ConcurrentQueue<Task> _tasks = new ConcurrentQueue<Task>();
        private readonly Thread[] _threads;
        // 3. 用 volatile 保证多线程下的可见性（避免线程缓存导致的状态不一致）
        private volatile bool _isRunning = true;
        // 4. 记录实例 ID，避免线程命名重复
        private static int _instanceCount;
        private readonly int _instanceId;

        // 新增：跟踪为“带超时/带取消”的任务所创建的 CancellationTokenSource
        private readonly ConcurrentDictionary<Task, CancellationTokenSource> _taskCancellation = new ConcurrentDictionary<Task, CancellationTokenSource>();

                public SchedulerHelper(int maxTask)
        {
            if (maxTask < 1)
                throw new ArgumentOutOfRangeException(nameof(maxTask), "最大线程数不能小于 1");

            _instanceId = Interlocked.Increment(ref _instanceCount);
            _threads = new Thread[maxTask];

            for (int i = 0; i < maxTask; i++)
            {
                _threads[i] = new Thread(WorkerLoop)
                {
                    IsBackground = true,
                    // 5. 线程名包含实例 ID，便于调试区分
                    Name = $"SchedulerHelper_{_instanceId}_Thread_{i + 1}"
                };
                _threads[i].Start();
            }
        }

        private void WorkerLoop()
        {
            while (_isRunning)
            {
                try
                {
                    _are.WaitOne();

                    while (_tasks.TryDequeue(out var task) && _isRunning)
                    {
                        TryExecuteTask(task);
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // 9. 记录异常（建议替换为项目的日志框架，如 NLog、Serilog）
                    Console.WriteLine($"工作线程异常: {ex.Message}");
                }
            }
        }

        protected override IEnumerable<Task>? GetScheduledTasks()
        {
            // 用 ToArray() 确保返回的是“快照”，避免外部修改队列
            return _tasks.ToArray();
        }

        protected override void QueueTask(Task task)
        {
            if (!_isRunning)
                throw new InvalidOperationException("调度器已停止，无法添加任务");

            _tasks.Enqueue(task);
            _are.Set();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (Array.Exists(_threads, t => t.ManagedThreadId == Thread.CurrentThread.ManagedThreadId))
            {
                if (taskWasPreviouslyQueued)
                    _tasks.TryDequeue(out _);

                return TryExecuteTask(task);
            }
            return false;
        }

        // 新增：以超时运行一个异步函数（协作式取消）
        // 示例用法：
        // var t = SchedulerHelper.Instance.RunWithTimeout(async ct => { await DoWorkAsync(ct); }, TimeSpan.FromSeconds(5));
        public Task RunWithTimeout(Func<CancellationToken, Task> func, TimeSpan timeout)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            if (!_isRunning) throw new InvalidOperationException("调度器已停止，无法添加任务");

            var cts = new CancellationTokenSource(timeout);
            // 使用 Task.Factory.StartNew 将任务交给当前 TaskScheduler（this）调度，
            // 内部调用会触发本类的 QueueTask，因此能被我们的工作线程执行。
            Task task = Task.Factory.StartNew(() =>
            {
                // 在工作线程上同步等待用户提供的异步函数完成，
                // 以便 TryExecuteTask 在正确的上下文中运行函数体。
                func(cts.Token).GetAwaiter().GetResult();
            }, cts.Token, TaskCreationOptions.DenyChildAttach, this);

            // 记录 CTS 以便 WorkerLoop 在任务完成后清理；也可用于外部查询/取消
            _taskCancellation[task] = cts;
            return task;
        }

        // 新增：以超时运行一个同步动作（协作式取消）
        public Task RunWithTimeout(Action<CancellationToken> action, TimeSpan timeout,CancellationTokenSource cts=null)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (!_isRunning) throw new InvalidOperationException("调度器已停止，无法添加任务");
            if (cts==null)
            {
                cts = new CancellationTokenSource(timeout);
            }
            Task task = Task.Factory.StartNew(() =>
            {
                action(cts.Token);
            }, cts.Token, TaskCreationOptions.DenyChildAttach, this);

            _taskCancellation[task] = cts;
            return task;
        }

        // 可选：外部根据 Task 获取其关联的 CancellationToken（如果存在）
        public bool TryGetCancellationToken(Task task, out CancellationToken token)
        {
            token = CancellationToken.None;
            if (task == null) return false;
            if (_taskCancellation.TryGetValue(task, out var cts))
            {
                token = cts.Token;
                return true;
            }
            return false;
        }

        // 可选：外部强制请求取消（协作式）
        public bool TryCancel(Task task)
        {
            if (task == null) return false;
            if (_taskCancellation.TryGetValue(task, out var cts))
            {
                try
                {
                    if (!cts.IsCancellationRequested) cts.Cancel();
                    return true;
                }
                catch { return false; }
            }
            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 停止线程循环
                _isRunning = false;
                // 唤醒所有阻塞的线程，让它们退出循环
                _are.Set();

                foreach (var thread in _threads)
                {
                    if (thread.IsAlive)
                        thread.Join(1000);
                }

                // 清理尚未被移除的 CTS
                foreach (var kv in _taskCancellation.ToArray())
                {
                    try { kv.Value.Cancel(); } catch { }
                    try { kv.Value.Dispose(); } catch { }
                }
                _taskCancellation.Clear();

                _are.Dispose();
            }
        }

        ~SchedulerHelper()
        {
            Dispose(false);
        }
    }
}