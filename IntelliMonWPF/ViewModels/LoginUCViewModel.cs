using IntelliMonWPF.DTOs;
using IntelliMonWPF.HttpClient;
using IntelliMonWPF.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IntelliMonWPF.ViewModels
{
    internal class LoginUCViewModel : BindableBase, IDialogAware
    {
        public DialogCloseListener RequestClose { get; }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
           
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
           
        }
        private IMessages messages;
        public LoginUCViewModel(IDialogService dialogService,IMessages messages)
        {
            RequestClose = new DialogCloseListener();
            this.dialogService = dialogService;
            this.messages = messages;
        }

        private string _Username;

        public string Username
        {
            get { return _Username; }
            set { _Username = value;
                RaisePropertyChanged();
            }
        }
        private string _Pwd;

        public string Pwd
        {
            get { return _Pwd; }
            set { _Pwd = value;
                RaisePropertyChanged();
            }
        }
        private ApiClient apiClient = new ApiClient();
        public DelegateCommand LoginCommand => new DelegateCommand(() =>
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Pwd))
            {
               messages.ShowMessage("用户名或密码不能为空");
                return;
            }
            
            var ApiResponse = apiClient.Excute<UserInfo, LoginDTO>(new ApiRequest<LoginDTO>
            {
                Route = "login",
                Method = RestSharp.Method.Post,
                Parsmeters = new LoginDTO
                {
                    username = Username,
                    password = Pwd
                }
            });
            if (ApiResponse.code == 200)
            {

                // 关闭对话框并传递结果
                var parameters = new DialogParameters
                {
                    { "userInfo", ApiResponse.data }
                };
                RequestClose.Invoke(parameters,ButtonResult.OK);
            }
            else
            {
               messages.ShowMessage($"登陆失败: {ApiResponse.message}");
            }
        });
        private IDialogService dialogService { get; set; }
        public DelegateCommand RegisterCommand => new DelegateCommand(() =>
        {
            dialogService.ShowDialog("RegisterUC", null, r =>
            {
                if (r != null && r.Result == ButtonResult.OK)
                {
                   messages.ShowMessage("注册成功，请登录");
                }
            }); 
        });
    }
}
