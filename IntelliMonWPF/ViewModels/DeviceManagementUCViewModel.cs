using IntelliMonWPF.Models;
using IntelliMonWPF.Models.Manger;
using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IntelliMonWPF.ViewModels
{
    internal class DeviceManagementUCViewModel:BindableBase
    {
        private readonly ModbusDictManger modbusDevice;
        public DeviceManagementUCViewModel(IDialogService dialogService,ModbusDictManger modbusDictManger)
        {
            _dialogService = dialogService;
            AddDeviceCommand = new DelegateCommand(OnAddDevice);
            EditDeviceCmd = new DelegateCommand(EditDebice);
            this.modbusDevice = modbusDictManger;
            Devices =this.modbusDevice.ModbusMangeList;
            ShowPackUCCmdCommand = new DelegateCommand(ShowPackUC);
        }

        private ObservableCollection<DeviceModel> _Devices;

        public ObservableCollection<DeviceModel> Devices
        {
            get { return _Devices; }
            set { _Devices = value;
                RaisePropertyChanged();
            }
        }

        private IDialogService _dialogService { get; set; }
        public DelegateCommand AddDeviceCommand { get; set; }
        private void OnAddDevice()
        {
            _dialogService.ShowDialog("CreatDeviceUC", null, result => { });
        }
        public DelegateCommand EditDeviceCmd { get; set; }
        private void EditDebice()
        {
            DialogParameters parameter = new DialogParameters();
            if (SelectedDevice == null)
            {
                MessageBox.Show("请选择设备");
                return;
            }
            parameter.Add("DevicePar",SelectedDevice);
            _dialogService.ShowDialog("EditDeviceUC", parameter, callback => { });
        }
        private DeviceModel _SelectedDevice;

        public DeviceModel SelectedDevice
        {
            get { return _SelectedDevice; }
            set { _SelectedDevice = value;
                RaisePropertyChanged();
            }
        }
        public DelegateCommand ShowPackUCCmdCommand { get; set; }
        private void ShowPackUC() 
        {
            _dialogService.Show("ShowPackUC", null, callback => { });
        }
    }
}
