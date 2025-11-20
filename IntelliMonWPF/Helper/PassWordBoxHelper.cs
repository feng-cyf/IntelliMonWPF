using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IntelliMonWPF.Helper
{
    public class PassWordBoxHelper
    {
        public static string GetPassWord(DependencyObject obj)
        {
            return (string)obj.GetValue(PassWordProperty);
        }

        public static void SetPassWord(DependencyObject obj, string value)
        {
            obj.SetValue(PassWordProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PassWordProperty =
            DependencyProperty.RegisterAttached("PassWord", typeof(string), typeof(PassWordBoxHelper), new PropertyMetadata(string.Empty,OnPassWordChange));

        private static void OnPassWordChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
           if (d is System.Windows.Controls.PasswordBox passwordBox)
            {
               passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
                if (!GetCanChange(passwordBox))
                {
                     passwordBox.Password = e.NewValue?.ToString() ?? string.Empty;
                }
                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.PasswordBox passwordBox)
            {
                SetCanChange(passwordBox, true);
                SetPassWord(passwordBox, passwordBox.Password);
                SetCanChange(passwordBox, false);
            }
        }

        public static bool GetCanChange(DependencyObject obj)
        {
            return (bool)obj.GetValue(CanChangeProperty);
        }

        public static void SetCanChange(DependencyObject obj, bool value)
        {
            obj.SetValue(CanChangeProperty, value);
        }

        // Using a DependencyProperty as the backing store for CanChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanChangeProperty =
            DependencyProperty.RegisterAttached("CanChangeanChange", typeof(bool), typeof(PassWordBoxHelper));



    }
}
