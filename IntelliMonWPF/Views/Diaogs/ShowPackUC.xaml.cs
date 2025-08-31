using IntelliMonWPF.ViewModels.DialogsViewModels;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace IntelliMonWPF.Views.Diaogs
{
    public partial class ShowPackUC : UserControl
    {
        // 1. 新增：标记用户是否手动滚动过（未滚到底）
        private bool _isUserScrolled = false;
        // 2. 新增：缓存ListBox的ScrollViewer，避免重复查找
        private ScrollViewer _scrollViewer;

        public ShowPackUC()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                if (DataContext is ShowPackUCViewModel vm)
                {
                    // 3. 先获取并缓存ScrollViewer，同时监听滚动事件
                    _scrollViewer = GetScrollViewer(PackListBox);
                    if (_scrollViewer != null)
                    {
                        // 监听用户滚动操作
                        _scrollViewer.ScrollChanged += OnScrollChanged;
                    }

                    // 4. 监听ViewModel的最新项变化
                    vm.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(vm.LatestItem) && vm.LatestItem != null && _scrollViewer != null)
                        {
                            // 延迟滚动，确保UI已更新
                            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                            {
                                // 关键：只有用户未手动滚动（或已滚到底），才自动滚底
                                if (!_isUserScrolled)
                                {
                                    PackListBox.ScrollIntoView(vm.LatestItem);
                                    _scrollViewer.ScrollToBottom();
                                }
                            }));
                        }
                    };
                }
            };

            // 5. 控件卸载时移除事件订阅，避免内存泄漏
            Unloaded += (s, e) =>
            {
                if (_scrollViewer != null)
                {
                    _scrollViewer.ScrollChanged -= OnScrollChanged;
                }
                if (DataContext is ShowPackUCViewModel vm)
                {
                    vm.PropertyChanged -= (sender, args) => { };
                }
            };
        }

        // 6. 滚动事件处理：判断用户是否手动滚动
        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // 只处理“垂直滚动”且“是用户主动操作”的情况（排除代码触发的滚动）
            if (e.VerticalChange != 0 && e.OriginalSource == _scrollViewer)
            {
                // 计算是否已滚到底部（VerticalOffset + ViewportHeight ≈ ExtentHeight，允许微小误差）
                bool isScrolledToBottom = Math.Abs(_scrollViewer.VerticalOffset + _scrollViewer.ViewportHeight - _scrollViewer.ExtentHeight) < 1;

                // 用户手动滚动且未滚到底 → 标记为“需暂停自动滚底”
                _isUserScrolled = !isScrolledToBottom;
            }
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

        private void PackListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (sender is ListBox lb && lb.SelectedItems.Count > 0)
                {
                    var text = string.Join(Environment.NewLine, lb.SelectedItems.Cast<string>());
                    Clipboard.SetText(text);
                }
            }
        }
    }
}