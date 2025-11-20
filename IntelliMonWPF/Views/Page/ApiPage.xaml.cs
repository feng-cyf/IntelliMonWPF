using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Prism.Ioc;

namespace IntelliMonWPF.Views.Page
{
    public partial class ApiPage
    {
        // 依赖属性，用于动画滚动
        private static readonly DependencyProperty AnimatedVerticalOffsetProperty =
            DependencyProperty.Register(
                "AnimatedVerticalOffset",
                typeof(double),
                typeof(ApiPage),
                new PropertyMetadata(0.0, OnAnimatedVerticalOffsetChanged));

        private static void OnAnimatedVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var page = (ApiPage)d;
            var sv = page.FindVisualParent<ScrollViewer>(page);
            if (sv != null)
                sv.ScrollToVerticalOffset((double)e.NewValue);
        }

        public ApiPage()
        {
            InitializeComponent();

            // 将页面的 DataContext 指向注册的单例 ViewModel
            // 因为通过 Frame.Navigate(new Uri(...)) 创建 Page 时并不会自动使用 Prism 的容器来注入 DataContext，
            // 导致订阅了 LoggingService 的单例 ViewModel 与页面绑定实例不一致（UI 不会更新）。
            // 这里显式从容器解析单例 ViewModel 并设置为 DataContext，使 ObservableCollection 更新能反映到界面。
            try
            {
                var vm = ContainerLocator.Container.Resolve<ViewModels.PageViewModel.ApiPageViewModel>();
                DataContext = vm;
            }
            catch
            {
                // 解析失败时保持原样（设计时或未配置容器），避免抛出异常影响设计器或运行。
            }
        }

        /// <summary>
        /// 滚动到指定区域（通过 x:Name 查找元素）
        /// </summary>
        public void ScrollToSection(string sectionName)
        {
            // 1. 找到目标元素
            var target = FindName(sectionName) as FrameworkElement;
            if (target == null)
                return;

            // 2. 找到父级 ScrollViewer（可能在 Window 中）
            var scrollViewer = FindVisualParent<ScrollViewer>(this);
            if (scrollViewer == null)
            {
                target.BringIntoView(); // 兜底方案
                return;
            }

            // 3. 计算目标位置（相对 ScrollViewer 的偏移）
            var transform = target.TransformToAncestor(scrollViewer);
            var point = transform.Transform(new Point(0, 0));

            double offset = point.Y + scrollViewer.VerticalOffset - 30; // 偏移30像素
            if (offset < 0) offset = 0;

            // 4. 动画滚动
            var animation = new DoubleAnimation
            {
                To = offset,
                Duration = TimeSpan.FromMilliseconds(350),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            BeginAnimation(AnimatedVerticalOffsetProperty, animation);
        }

        /// <summary>
        /// 向上查找指定类型的父元素
        /// </summary>
        private T FindVisualParent<T>(DependencyObject obj) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is T parentT)
                    return parentT;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}