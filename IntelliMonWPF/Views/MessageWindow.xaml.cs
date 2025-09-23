using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace IntelliMonWPF.Views
{
    /// <summary>
    /// MessageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MessageWindow : Window
    {
        private DispatcherTimer timer;
        private int Time;
        public MessageWindow(int Time)
        {
            this.Time = Time;
            InitializeComponent();
            timer = new DispatcherTimer() {Interval=TimeSpan.FromSeconds(Time)};
            timer.Tick += (s, e) =>
            {
                timer?.Stop();
                this.Close();
            };
            Loaded += (s, e) => { timer.Start(); };
        }
    }
}
