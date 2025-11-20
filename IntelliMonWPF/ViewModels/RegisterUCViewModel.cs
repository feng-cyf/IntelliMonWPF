using IntelliMonWPF.DTOs;
using IntelliMonWPF.HttpClient;
using IntelliMonWPF.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IntelliMonWPF.ViewModels
{
    public class RegisterUCViewModel : BindableBase, IDialogAware
    {

        public string Title => "用户注册";
        public DialogCloseListener RequestClose { get; }=new DialogCloseListener();

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
        private ObservableCollection<KeyValuePair<int,string>> _JobName=new ObservableCollection<KeyValuePair<int, string>>();

        public ObservableCollection<KeyValuePair<int, string>> JobName
        {
            get { return _JobName; }
            set { _JobName = value;
                RaisePropertyChanged();
            }
        }
        private IMessages messages;
        public RegisterUCViewModel(IMessages messages)
        {
            ShowJobName();
            RegisterCmd= new DelegateCommand(Register);
            this.messages= messages;
        }
        private ApiClient apiClient = new ApiClient();
        public DelegateCommand RegisterCmd { get; set; }
        private void Register()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Pwd) || string.IsNullOrEmpty(ConfirmPwd))
            {
               messages.ShowMessage("请填写完整信息");
                return;
            }
            if (Pwd != ConfirmPwd)
            {
               messages.ShowMessage("两次密码输入不一致");
                return;
            }
            RegisterDTO registerDTO = new RegisterDTO()
            {
                username = Username,
                password = Pwd,
                job_id = SelectId.Key
            };
            var request = new ApiRequest<RegisterDTO>()
            {
                Method = RestSharp.Method.Post,
                Route = "register/user",
                Parsmeters = registerDTO
            };
            var response = apiClient.Excute<string, RegisterDTO>(request);
            if (response != null && response.code == 200)
            {
               messages.ShowMessage("注册成功");
            }
            else
            {
               messages.ShowMessage("注册失败:" + response.message);
            } 
        }
        private string _Username;

        public string Username
        {
            get { return _Username; }
            set { _Username = value;
                RaisePropertyChanged();
            }
        }
        private string _ConfirmPwd;

        public string ConfirmPwd
        {
            get { return _ConfirmPwd; }
            set {
                _ConfirmPwd = value;
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
       

        private KeyValuePair<int,string> _SelectId;

        public KeyValuePair<int,string> SelectId
        {
            get { return _SelectId; }
            set { _SelectId = value;
                RaisePropertyChanged();
            }
        }


        private void ShowJobName()
        {
            ApiRequest<object> request = new() 
            {
                Method=RestSharp.Method.Get,
                Route= "select",
                Parsmeters=null
            };
            var data = apiClient.SelectJob(request);
            if (data!=null && data.code == 200)
            {
                var jobList = data.data;
                foreach (var job in jobList)
                {
                    JobName.Add( new KeyValuePair<int,string>(job.Id, job.JobName));
                }
            }
        }

        public DelegateCommand CloseCmd => new DelegateCommand(() =>
        {
            RequestClose.Invoke(new DialogResult(ButtonResult.No));
        });
    }
}
