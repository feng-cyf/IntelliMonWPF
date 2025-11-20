using System;
using System.Collections.ObjectModel;
using System.Windows;
using IntelliMonWPF.Services;

namespace IntelliMonWPF.ViewModels.PageViewModel
{
    public class ApiPageViewModel : BindableBase
    {
        // 与 ApiPage.xaml 中绑定名保持一致
        public ObservableCollection<string> MachineLog { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> PointApiLog { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ExcelLog { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> DeviceConfigLog { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> PointConfigLog { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> HistoryLog { get; } = new ObservableCollection<string>();

        public ApiPageViewModel()
        {
            LoggingService.Instance.LogReceived += OnLogReceived;
        }

        private void OnLogReceived(LogMessage msg)
        {
            var dsp = Application.Current?.Dispatcher;
            if (dsp == null) return;

            void add()
            {
                var entry = $"[{msg.Time:HH:mm:ss}] {msg.Message}";
                switch (msg.Type)
                {
                    case LogType.Machine: MachineLog.Add(entry); break;
                    case LogType.PointApi: PointApiLog.Add(entry); break;
                    case LogType.Excel: ExcelLog.Add(entry); break;
                    case LogType.DeviceConfig: DeviceConfigLog.Add(entry); break;
                    case LogType.PointConfig: PointConfigLog.Add(entry); break;
                    case LogType.History: HistoryLog.Add(entry); break;
                }
            }

            if (!dsp.CheckAccess()) dsp.BeginInvoke((Action)add);
            else add();
        }
    }
}
