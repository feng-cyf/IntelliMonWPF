using IntelliMonWPF.ViewModels;
using IntelliMonWPF.Views;
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
            containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();
            containerRegistry.RegisterDialog<LoginUC, LoginUCViewModel>();
        }
        protected override void OnInitialized()
        {
            var dialogService = Container.Resolve<IDialogService>();
            dialogService.ShowDialog("LoginUC", null, r =>
            {
                if (r!=null && r.Result == ButtonResult.OK)
                {
                    base.OnInitialized();
                }
                else
                {
                    // User clicked Cancel or closed the dialog, shut down the application
                    Current.Shutdown();
                }
            }); 
            
        }
    }

}
