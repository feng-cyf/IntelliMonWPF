using System;

namespace IntelliMonWPF.Services
{
    public enum LogType
    {
        Machine,
        PointApi,
        Excel,
        DeviceConfig,
        PointConfig,
        History
    }

    public class LogMessage
    {
        public LogType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Time { get; set; } = DateTime.Now;
    }

    public class LoggingService
    {
        private static readonly Lazy<LoggingService> _instance = new(() => new LoggingService());
        public static LoggingService Instance => _instance.Value;

        private LoggingService() { }

        public event Action<LogMessage>? LogReceived;

        public void Publish(LogType type, string message)
        {
            LogReceived?.Invoke(new LogMessage { Type = type, Message = message });
        }
    }
}