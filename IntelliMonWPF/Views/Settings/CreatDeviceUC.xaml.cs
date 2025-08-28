using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IntelliMonWPF.Views.Settings
{
    /// <summary>
    /// CreatDeviceUC.xaml 的交互逻辑
    /// </summary>
    public partial class CreatDeviceUC : UserControl
    {
        public CreatDeviceUC()
        {
            InitializeComponent();
        }

        private void cboConnection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pnlSerial == null || pnlTCP==null)return;
            if (cboConnection.SelectedIndex == 0)
            {
                pnlSerial.IsEnabled= true;
                pnlTCP.IsEnabled= false;
            }
            else if (cboConnection.SelectedIndex==1 )
            {
                pnlTCP.IsEnabled= true;
                pnlSerial.IsEnabled= false;
            }
            else if (cboConnection.SelectedIndex==2 )
            {
                pnlTCP.IsEnabled= true;
                pnlSerial.IsEnabled= false;
            }
          
        }
    }
}
