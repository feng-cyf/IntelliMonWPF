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

namespace IntelliMonWPF.Views.Page
{
    /// <summary>
    /// ApiLoggingMainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ApiLoggingMainWindow : Window
    {
        public ApiLoggingMainWindow()
        {
            InitializeComponent();
            frame.PreviewMouseWheel += Frame_PreviewMouseWheel;
            // 只导航一次
            frame.Navigate(new Uri("/IntelliMonWPF;component/Views/Page/ApiPage.xaml", UriKind.Relative));
        }

        private void Frame_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 找到外层的 ScrollViewer
            ScrollViewer scrollViewer = FindVisualParent<ScrollViewer>(sender as DependencyObject);
            if (scrollViewer != null)
            {
                // 创建新的鼠标滚轮事件，并传递给 ScrollViewer
                MouseWheelEventArgs newEventArgs = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                newEventArgs.RoutedEvent = UIElement.MouseWheelEvent;
                newEventArgs.Source = sender;
                scrollViewer.RaiseEvent(newEventArgs);
            }
        }

        // 查找视觉树中指定类型的父元素的辅助方法
        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T typedParent)
                {
                    return typedParent;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var btn = sender as RadioButton;
            if (btn?.Tag is string tag)
            {
                // 只滚动，不重新导航
                if (frame.Content is IntelliMonWPF.Views.Page.ApiPage page)
                {
                    page.ScrollToSection(tag);
                }
            }
        }
    }
   
}
