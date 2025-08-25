using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.ViewModels
{
    internal class LoginUCViewModel : BindableBase, IDialogAware
    {
        public DialogCloseListener RequestClose { get; }

        public bool CanCloseDialog()
        {
            throw new NotImplementedException();
        }

        public void OnDialogClosed()
        {
            throw new NotImplementedException();
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            throw new NotImplementedException();
        }
        public LoginUCViewModel()
        {
            RequestClose=new DialogCloseListener();
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


    }
}
