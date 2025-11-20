using AutoMapper;
using IntelliMonWPF.Helper;
using IntelliMonWPF.Helper.Tools;
using IntelliMonWPF.HttpClient;
using IntelliMonWPF.IF_Implements;
using IntelliMonWPF.IF_Implements.Factory;
using IntelliMonWPF.IF_Implements.MangerInferface;
using IntelliMonWPF.Interface;
using IntelliMonWPF.Interface.IFactory;
using IntelliMonWPF.ViewModels;
using IntelliMonWPF.ViewModels.DialogsViewModels;
using IntelliMonWPF.ViewModels.PageViewModel;
using IntelliMonWPF.ViewModels.PageViewModel.WindowViewModel;
using IntelliMonWPF.ViewModels.SettingsViewModel;
using IntelliMonWPF.Views;
using IntelliMonWPF.Views.Diaogs;
using IntelliMonWPF.Views.Page;
using IntelliMonWPF.Views.Settings;
using System.Configuration;
using System.Data;
using System.Windows;

namespace IntelliMonWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        // 使用接口类型更合适
        public static IMapper Mapper { get; set; }

        protected override Window CreateShell()
        {
            Container.Resolve<MessageWindowViewModel>();
            Container.Resolve<ApiPageViewModel>();
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // AutoMapper 初始化并注册到容器
            //var mapperConfig = new MapperConfiguration(cfg =>
            //{
            //    cfg.AddProfile<DtoMappingProfileHelper>();
            //},null);

            //var mapper = mapperConfig.CreateMapper();
            //// 将 IMapper 单例实例注册到 Prism 容器，供其他类构造函数注入使用
            //containerRegistry.RegisterInstance<IMapper>(mapper);
            //// 可选：在静态属性保存一份全局访问
            //Mapper = mapper;

            containerRegistry.RegisterDialog<ShowPackUC, ShowPackUCViewModel>();
            containerRegistry.RegisterDialog<CreatDeviceUC, CreatDeviceUCViewModel>();
            containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();
            containerRegistry.RegisterDialog<LoginUC, LoginUCViewModel>();
            containerRegistry.RegisterDialog<RegisterUC, RegisterUCViewModel>();
            containerRegistry.RegisterForNavigation<DeviceManagementUC, DeviceManagementUCViewModel>();
            containerRegistry.RegisterDialog<EditDeviceUC, EditDeviceUCViewModel>();
            containerRegistry.RegisterForNavigation<ModbusPointConfigControl, ModbusPointConfigControlViewModel>();
            containerRegistry.RegisterDialog<AddDeviceModbusUC, AddDeviceModbusUCViewModel>();
            containerRegistry.RegisterDialog<SendUC, SendUCViewModel>();
            containerRegistry.RegisterDialog<AddModbusPoint, AddModbusPointViewModel>();
            containerRegistry.Register<IMessages, Message>();
            containerRegistry.RegisterForNavigation<MessageWindow, MessageWindowViewModel>();
            containerRegistry.RegisterSingleton<MessageWindowViewModel>();
            containerRegistry.RegisterDialog<EditPointUC, EditPointUCViewModel>();
            containerRegistry.RegisterDialog<ToCEUC, ToCEUCViewModel>();
            containerRegistry.Register<DeviceSaveApiClient>();
            containerRegistry.RegisterSingleton<ModbusDictManger>();
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<IDictMangerFactory, DictMangerFactory>();
            containerRegistry.RegisterSingleton<MobusObserVableManger>();
            containerRegistry.RegisterSingleton<IObserVableCollectionFactory, ObserVableFactory>();
            containerRegistry.RegisterForNavigation<ApiLoggingMainWindow, ApiLoggingMainWindowViewModel>();
            containerRegistry.RegisterForNavigation<ApiPage, ApiPageViewModel>();
            containerRegistry.RegisterSingleton<ApiPageViewModel>();
            containerRegistry.RegisterForNavigation<DataMonitoringUC, DataMonitoringUCViewModel>();
        }
        //protected override void OnInitialized()
        //{
        //    var dialogService = Container.Resolve<IDialogService>();
        //    dialogService.ShowDialog("LoginUC", null, r =>
        //    {
        //        if (r != null && r.Result == ButtonResult.OK)
        //        {
        //            if (r.Parameters.ContainsKey("userInfo"))
        //            {
        //                var userInfo = r.Parameters.GetValue<UserInfo>("userInfo");

        //                // 传给 MainWindow 的 VM
        //                var mainVm = (MainWindowViewModel)Current.MainWindow.DataContext;
        //                mainVm.SetLoginUser(userInfo); 
        //                mainVm.SelectedNavigationItem = mainVm.NavigationItems[0];
        //            }
        //        }
        //        else
        //        {
        //            // User clicked Cancel or closed the dialog, shut down the application
        //            Current.Shutdown();
        //        }
        //    });
        //    base.OnInitialized();

        //}
    }

}
