using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IntelliMonWPF.Interface
{
    public enum MsgBtn { OK, OKCancel, YesNo }      // 替代 MessageBoxButton
    public enum MsgIco { Info, Warning, Question, Error } // 替代 MessageBoxImage
    public enum MsgRst { OK, Cancel, Yes, No }      // 替代 MessageBoxResult
    public interface IMessages
    {
        void ShowMessage(string message, string info = "");

        // 只改签名，名字不动
        bool ShowMessageSure(string message, string info = "",
                             MsgBtn btn = MsgBtn.OKCancel,
                             MsgIco ico = MsgIco.Question);
    }
}
