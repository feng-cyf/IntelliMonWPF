using IntelliMonWPF.ViewModels.DialogsViewModels;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace IntelliMonWPF.Views.Diaogs
{
    public partial class ShowPackUC : UserControl
    {
        public ShowPackUC()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                if (DataContext is ShowPackUCViewModel vm)
                {
                    // 监听最新项变化，而非集合变化
                    vm.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(vm.LatestItem) && vm.LatestItem != null)
                        {
                            // 延迟滚动，确保UI已更新
                            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                            {
                                PackListBox.ScrollIntoView(vm.LatestItem);

                                // 强制滚动到底部（针对虚拟化列表）
                                if (PackListBox.Items.Count > 0)
                                {
                                    var scrollViewer = GetScrollViewer(PackListBox);
                                    scrollViewer?.ScrollToBottom();
                                }
                            }));
                        }
                    };
                }
            };
        }

        // 辅助方法：获取ListBox内部的ScrollViewer
        private ScrollViewer GetScrollViewer(DependencyObject obj)
        {
            if (obj is ScrollViewer viewer)
                return viewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                var result = GetScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}