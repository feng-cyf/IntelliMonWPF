using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliMonWPF.Helper.Middle
{
    /// <summary>
    /// 调度上下文（承载任务状态、参数、取消信号等）
    /// </summary>
    public class SchedulerContext // 修正命名：Schduler → Scheduler（规范拼写）
    {
        /// <summary>
        /// 待执行的核心任务（可替换为具体业务委托，更灵活）
        /// </summary>
        public Func<CancellationToken, Task> BusinessTask { get; set; }

        /// <summary>
        /// 任务ID（用于日志/追踪）
        /// </summary>
        public string TaskId { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 执行状态（成功/失败）
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 异常信息（执行失败时存储）
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 取消令牌源（支持任务取消/超时）
        /// </summary>
        public CancellationTokenSource Cts { get; set; } = new CancellationTokenSource();
    }

    /// <summary>
    /// 调度中间件接口（规范中间件实现）
    /// </summary>
    public interface IScheduleMiddleware // 修正命名：ScheduleMiddle → ScheduleMiddleware（规范后缀）
    {
        /// <summary>
        /// 中间件执行方法
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="next">下一个中间件/核心任务</param>
        /// <returns>执行结果</returns>
        Task InvokeAsync(SchedulerContext context, Func<SchedulerContext, Task> next); // 修正命名：Schedule → InvokeAsync（更贴合异步语义）
    }

    /// <summary>
    /// 调度中间件委托（支持无接口的轻量中间件）
    /// </summary>
    /// <param name="context">上下文</param>
    /// <param name="next">下一个中间件/核心任务</param>
    /// <returns>执行结果</returns>
    public delegate Task ScheduleMiddlewareHandler(SchedulerContext context, Func<SchedulerContext, Task> next);

    /// <summary>
    /// 中间件构建器（组装中间件链条）
    /// </summary>
    internal class ScheduleMiddlewareBuilder // 修正命名：ScheduleMiddleBulider → ScheduleMiddlewareBuilder（规范拼写+命名）
    {
        // 存储中间件委托（接口实现会被转换为委托）
        private readonly List<ScheduleMiddlewareHandler> _middlewares = new List<ScheduleMiddlewareHandler>();

        /// <summary>
        /// 添加委托类型中间件（轻量无接口）
        /// </summary>
        /// <param name="handler">中间件委托</param>
        /// <returns>构建器（链式调用）</returns>
        public ScheduleMiddlewareBuilder Use(ScheduleMiddlewareHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            _middlewares.Add(handler);
            return this;
        }

        /// <summary>
        /// 添加接口类型中间件（强类型规范）
        /// </summary>
        /// <typeparam name="T">中间件类型（实现IScheduleMiddleware）</typeparam>
        /// <returns>构建器（链式调用）</returns>
        public ScheduleMiddlewareBuilder Use<T>() 
            where T : IScheduleMiddleware, new()
        {
            var middleware = new T();
            // 接口方法转换为委托，统一存储
            _middlewares.Add(middleware.InvokeAsync);
            return this;
        }

        /// <summary>
        /// 构建中间件链条（核心逻辑：反向拼接，保证注册顺序=执行顺序）
        /// </summary>
        /// <returns>最终的中间件执行入口</returns>
        public ScheduleMiddlewareHandler Build() // 修正命名：Bulider → Build（规范拼写）
        {
            // 链条终点：执行核心业务任务
            ScheduleMiddlewareHandler finalHandler = async (context, _) =>
            {
                try
                {
                    if (context.BusinessTask == null)
                        throw new InvalidOperationException("核心业务任务未设置");

                    await context.BusinessTask(context.Cts.Token);
                    context.IsSuccess = true;
                }
                catch (Exception ex)
                {
                    context.Exception = ex;
                    context.IsSuccess = false;
                }
                finally
                {
                    context.Cts.Dispose();
                }
            };

            // 反向遍历中间件，拼接成链：最后注册的中间件先执行（符合直觉）
            foreach (var middleware in _middlewares.AsEnumerable().Reverse())
            {
                var nextHandler = finalHandler;
                // 每个中间件都持有下一个中间件的引用
                finalHandler = async (context, next) =>
                {
                    await middleware(context, async(c)=>await nextHandler(c,next));
                };
            }

            return finalHandler;
        }
    }

    #region 示例中间件（直接用你的框架实现）
    /// <summary>
    /// 日志中间件（接口实现）
    /// </summary>
    public class LogScheduleMiddleware : IScheduleMiddleware
    {
        public async Task InvokeAsync(SchedulerContext context, Func<SchedulerContext, Task> next)
        {
            Console.WriteLine($"[日志中间件] 任务 {context.TaskId} 开始执行");
            var startTime = DateTime.Now;

            await next(context); // 执行下一个中间件

            var duration = DateTime.Now - startTime;
            Console.WriteLine($"[日志中间件] 任务 {context.TaskId} 执行结束 | 状态：{context.IsSuccess} | 耗时：{duration.TotalMilliseconds:F2}ms | 异常：{context.Exception?.Message ?? "无"}");
        }
    }

    /// <summary>
    /// 超时中间件（委托实现）
    /// </summary>
    public static class TimeoutMiddleware
    {
        public static async Task Handle(SchedulerContext context, Func<SchedulerContext, Task> next, int timeoutMs = 3000)
        {
            context.Cts.CancelAfter(timeoutMs);
            Console.WriteLine($"[超时中间件] 任务 {context.TaskId} 超时阈值：{timeoutMs}ms");

            try
            {
                await next(context);
                if (context.Exception is OperationCanceledException)
                {
                    context.Exception = new TimeoutException($"任务 {context.TaskId} 超时（{timeoutMs}ms）");
                    context.IsSuccess = false;
                }
            }
            catch (OperationCanceledException)
            {
                context.Exception = new TimeoutException($"任务 {context.TaskId} 超时（{timeoutMs}ms）");
                context.IsSuccess = false;
            }
        }
    }
    #endregion

    #region 测试代码（验证你的框架）
    public class Test
    {
        public static async Task RunTest()
        {
            // 1. 构建中间件链条
            var builder = new ScheduleMiddlewareBuilder()
                .Use<LogScheduleMiddleware>() // 添加接口中间件
                .Use((context, next) => TimeoutMiddleware.Handle(context, next, 2000)); // 添加委托中间件

            // 2. 构建执行入口
            var middlewarePipeline = builder.Build();

            // 3. 准备上下文和业务任务
            var context = new SchedulerContext
            {
                TaskId = "TEST001",
                BusinessTask = async (ct) =>
                {
                    Console.WriteLine("[业务任务] 执行核心逻辑...");
                    await Task.Delay(1500, ct); // 模拟1.5秒执行
                }
            };

            // 4. 执行
            await middlewarePipeline(context, null);

            Console.WriteLine($"\n最终结果：{(context.IsSuccess ? "成功" : "失败")}");
        }
    }
    #endregion
}