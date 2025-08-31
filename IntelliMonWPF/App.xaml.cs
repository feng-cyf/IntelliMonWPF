using IntelliMonWPF.HttpClient;
using IntelliMonWPF.Models.Manger;
using IntelliMonWPF.ViewModels;
using IntelliMonWPF.ViewModels.DialogsViewModels;
using IntelliMonWPF.ViewModels.SettingsViewModel;
using IntelliMonWPF.Views;
using IntelliMonWPF.Views.Diaogs;
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
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<ShowPackUC, ShowPackUCViewModel>();
            containerRegistry.RegisterDialog<CreatDeviceUC, CreatDeviceUCViewModel>();
            containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();
            containerRegistry.RegisterDialog<LoginUC, LoginUCViewModel>();
            containerRegistry.RegisterDialog<RegisterUC, RegisterUCViewModel>();
            containerRegistry.RegisterForNavigation<DeviceManagementUC, DeviceManagementUCViewModel>();
            containerRegistry.RegisterSingleton<ModbusDictManger>();
            containerRegistry.RegisterDialog<EditDeviceUC, EditDeviceUCViewModel>();
            containerRegistry.RegisterForNavigation<ModbusPointConfigControl, ModbusPointConfigControlViewModel>();
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
        //                mainVm.SetLoginUser(userInfo); // 自定义方法，把登录数据传给 VM
        //            }
        //            base.OnInitialized();
        //        }
        //        else
        //        {
        //            // User clicked Cancel or closed the dialog, shut down the application
        //            Current.Shutdown();
        //        }
        //    });

        //}
    }

}
