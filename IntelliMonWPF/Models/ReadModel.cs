using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.Models
{
    internal class ReadModel : INotifyPropertyChanged
    {
        public int StartAdress { get; set; } = 0;
        public int NumAdress { get; set; } = 8;
        private int _ReadTimeout;

        public int ReadTimeout
        {
            get { return _ReadTimeout; }
            set
            {
                _ReadTimeout = value;
                PropertyChanged.Invoke(this,new PropertyChangedEventArgs(nameof(ReadTimeout)));
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
