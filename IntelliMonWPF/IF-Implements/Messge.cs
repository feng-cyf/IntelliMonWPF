using IntelliMonWPF.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IntelliMonWPF.IF_Implements
{
    public class Message : IMessages
    {
        public void ShowMessage(string message, string info = "")
            => MessageBox.Show(message, info);

        public bool ShowMessageSure(string message, string info = "",
                                    MsgBtn btn = MsgBtn.OKCancel,
                                    MsgIco ico = MsgIco.Question)
        {
            var wpfBtn = (MessageBoxButton)(int)btn;
            var wpfIco = (MessageBoxImage)(int)ico;
            var wpfRes = MessageBox.Show(message, info, wpfBtn, wpfIco);
            return wpfRes == MessageBoxResult.OK;
        }
    }
}
