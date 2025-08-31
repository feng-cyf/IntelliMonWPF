using IntelliMonWPF.HttpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace IntelliMonWPF.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private object _selectedNavigationItem;
        public IRegionNavigationJournal _navigationJournal;

        public object SelectedNavigationItem
        {
            get => _selectedNavigationItem;
            set
            {
                if (SetProperty(ref _selectedNavigationItem, value))
                {
                  Navigate(value);
                }
            }
        }

        public List<NavigationItem> NavigationItems { get; }
        public ICommand NavigateCommand { get; }
        private readonly DispatcherTimer _timer;

        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            NavigationItems = new List<NavigationItem>
            {
            new NavigationItem { Title = "设备管理", ViewName = "DeviceManagementUC", Icon = "\uE871" },
            new NavigationItem { Title = "点表配置", ViewName = "ModbusPointConfigControl", Icon = "\uE8A5" },
            new NavigationItem { Title = "实时监控", ViewName = "RealtimeMonitoringView", Icon = "\uE8B0" },
            new NavigationItem { Title = "员工管理", ViewName = "EmployeeManagementView", Icon = "\uE774" },
            new NavigationItem { Title = "系统设置", ViewName = "SystemSettingsView", Icon = "\uE713" }
            };

            NavigateCommand = new DelegateCommand<object>(Navigate);
            _timer= new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            };
            _timer.Start();
        }

        private void Navigate(object navigationItem)
        {
            if (navigationItem is NavigationItem item)
            {
                _regionManager.Regions["Regions"].RequestNavigate(item.ViewName, result =>
                {
                    if (result != null)
                    {
                        _navigationJournal = result.Context.NavigationService.Journal;
                    }
                });
                _regionManager.Regions["Regions"].NavigationService.Navigated += (s, e) =>
                {
                    var currentView = e.Uri.OriginalString;

                    var match = NavigationItems.FirstOrDefault(x => x.ViewName == currentView);
                    if (match != null)
                    {
                        SelectedNavigationItem = match;
                    }
                };

            }
        }

        #region 界面操作
        public DelegateCommand GoBackCommand => new DelegateCommand(() =>
        {
            if (_navigationJournal != null && _navigationJournal.CanGoBack)
            {
                _navigationJournal.GoBack();
            }
        });

        public DelegateCommand GoForwardCommand => new DelegateCommand(() =>
        {
            if (_navigationJournal != null && _navigationJournal.CanGoForward)
            {
                _navigationJournal.GoForward();
            }
        });

        public DelegateCommand MinWindowCommand => new DelegateCommand(() =>
        {
            App.Current.MainWindow.WindowState = System.Windows.WindowState.Minimized;
        });

        public DelegateCommand MaxWindowCommand => new DelegateCommand(() =>
        {
            App.Current.MainWindow.WindowState =
                App.Current.MainWindow.WindowState == System.Windows.WindowState.Maximized
                    ? System.Windows.WindowState.Normal
                    : System.Windows.WindowState.Maximized;
        });

        public DelegateCommand CloseWindowCommand => new DelegateCommand(() =>
        {
            App.Current.Shutdown();
        });

        
        #endregion 参数绑定
        #region
        private string _Time;

        public string Time
        {
            get { return _Time; }
            set { _Time = value;
                RaisePropertyChanged();
            }
        }

        private string _Job;

        public string Job
        {
            get { return _Job; }
            set { _Job = value;
                RaisePropertyChanged(); }
        }

        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { _Name = value;
                RaisePropertyChanged(); }
        }

        #endregion
        public void SetLoginUser(UserInfo response)
        {
            Name= response.Username;
            Job= response.Job.JobName;
        }

    }

    public class NavigationItem
    {
        public string Title { get; set; }
        public string ViewName { get; set; }
        public string Icon { get; set; }
    }
}
       

