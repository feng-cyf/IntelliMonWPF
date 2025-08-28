using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace IntelliMonWPF.Base
{
	public class ModbuslBase : BindableBase
	{
		private SerialPort serialPort;
		private ObservableCollection<string> _PortName = new ObservableCollection<string>();

		public ObservableCollection<string> PortName
		{
			get { return _PortName; }
			set
			{
				_PortName = value;
				RaisePropertyChanged();
			}
		}
		private DispatcherTimer portTimer;
		public ModbuslBase()
		{
			InitPortName();
			portTimer = new DispatcherTimer();
			portTimer.Interval = TimeSpan.FromSeconds(1);
			portTimer.Tick += (s, e) => InitPortName();
			portTimer.Start();
		}

		private void InitPortName()
		{
			if (PortName == null)
				return;
			string[] ports = SerialPort.GetPortNames();
			foreach (string s in SerialPort.GetPortNames())
			{
				if (!PortName.Contains(s))
					PortName.Add(s);
			}
			foreach (string s in PortName.ToList())
			{
				if (!ports.Contains(s))
					PortName.Remove(s);
			}
			for (int i = 1; i < PortName.Count; i++)
			{
				for (int j =i; j > 0; j--)
				{
					int len = PortName[0].Length ;
					if (int.TryParse(PortName[j][3..len], out int a) && int.TryParse(PortName[j - 1][3..len], out int b))
					{
						if (a < b)
						{
							var temp = PortName[j];
							PortName[j] = PortName[j - 1];
							PortName[j - 1] = temp;
						}
						else break;
                    }
				}
			}
		}
	}
}
