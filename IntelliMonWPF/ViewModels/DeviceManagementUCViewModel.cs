using IntelliMonWPF.Models;
using IntelliMonWPF.Models.Manger;
using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace IntelliMonWPF.ViewModels
{
    internal class DeviceManagementUCViewModel : BindableBase
    {
        private readonly ModbusDictManger modbusDevice;
        public DeviceManagementUCViewModel(IDialogService dialogService, ModbusDictManger modbusDictManger)
        {
            _dialogService = dialogService;
            AddDeviceCommand = new DelegateCommand(OnAddDevice);
            EditDeviceCmd = new DelegateCommand(EditDebice);
            this.modbusDevice = modbusDictManger;
            Devices = this.modbusDevice.ModbusMangeList;
            ShowPackUCCmdCommand = new DelegateCommand(ShowPackUC);
            StopDeviceCmd = new DelegateCommand(async () => { await StopDevice(); });
            OpenDeviceCmd=new DelegateCommand(async () => { await OpenDevice(); });
            RemoveDeviceCmd = new DelegateCommand(async () => { await RemoveDevice(); });
           
            SearchDeviceCmd = new DelegateCommand(Refresh);
        }

        private ObservableCollection<DeviceModel> _Devices;

        public ObservableCollection<DeviceModel> Devices
        {
            get { return _Devices; }
            set
            {
                _Devices = value;
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
            parameter.Add("DevicePar", SelectedDevice);
            _dialogService.ShowDialog("EditDeviceUC", parameter, callback => { });
        }
        private DeviceModel _SelectedDevice;

        public DeviceModel SelectedDevice
        {
            get { return _SelectedDevice; }
            set
            {
                _SelectedDevice = value;
                RaisePropertyChanged();
               
            }
        }
        public DelegateCommand ShowPackUCCmdCommand { get; set; }
        private void ShowPackUC()
        {
            _dialogService.Show("ShowPackUC", null, callback => { });
        }
        public DelegateCommand StopDeviceCmd { get; set; }
        private async Task StopDevice()
        {
            if (SelectedDevice == null)
            {
                MessageBox.Show("请选择设备"); return;
            }
            if (!SelectedDevice.Channel.IsConnected)
            {
                MessageBox.Show($"{SelectedDevice.Name}设备已经关闭");
                return;
            }
            await SelectedDevice.Channel.CloseAsyance();
            if (!SelectedDevice.Channel.IsConnected)
            {
                SelectedDevice.Status = "已断开";
            }
        }
        public DelegateCommand OpenDeviceCmd { get; set; }
        private async Task OpenDevice()
        {
            if (SelectedDevice == null)
            {
                MessageBox.Show("请选择设备"); return;
            }
            if (SelectedDevice.Channel.IsConnected) { MessageBox.Show("设备正在运行"); }
            DeviceModel model = modbusDevice.ModbusMangeDict[SelectedDevice.ConnectionString];
            await SelectedDevice.Channel.OpenAsyance(model);
        }

        public DelegateCommand RemoveDeviceCmd { get; set; }
        private ICollectionView collectionView;
        private async Task RemoveDevice() 
        {
            if (SelectedDevice == null)
            {
                MessageBox.Show("请选择设备");return;
            }
            var result= MessageBox.Show($"Remove device {SelectedDevice.ConnectionString}","提醒",MessageBoxButton.OKCancel,MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                if (SelectedDevice.Channel.IsConnected)
                {
                    await SelectedDevice.Channel.CloseAsyance();
                    modbusDevice.Remove(SelectedDevice);
                }
                else
                {
                    MessageBox.Show("设备异常无法移除");
                    return;
                }
            }
        }
        public DelegateCommand SearchDeviceCmd {  get; set; }
        private void Refresh()
        {
            collectionView = CollectionViewSource.GetDefaultView(modbusDevice.ModbusMangeList);
            collectionView.Filter = FilterDevice;
            collectionView.Refresh();
        }
        private bool FilterDevice(object item)
        {
            if (item is not DeviceModel device)
                return false;

            // 名称过滤
            bool nameMatch = string.IsNullOrEmpty(DeviceName) ||
                            device.Name.IndexOf(DeviceName, StringComparison.OrdinalIgnoreCase) >= 0;

            // 连接（IP）过滤
            bool conMatch = string.IsNullOrEmpty(DeviceCon) ||
                           device.ConnectionString.IndexOf(DeviceCon, StringComparison.OrdinalIgnoreCase) >= 0;

            // 状态过滤
            bool statusMatch = string.IsNullOrEmpty(DeviceStatus) ||
                              device.Status.IndexOf(DeviceStatus, StringComparison.OrdinalIgnoreCase) >= 0;

            return nameMatch && conMatch && statusMatch;
        }



        private string _DeviceName;

        public string DeviceName
        {
            get { return _DeviceName; }
            set { _DeviceName = value;
                RaisePropertyChanged();
            }
        }

        private string _DeviceCon;

        public string DeviceCon
        {
            get { return _DeviceCon; }
            set { _DeviceCon = value;
                RaisePropertyChanged();
            }
        }

        private string _DeviceStatus;

        public string DeviceStatus
        {
            get { return _DeviceStatus; }
            set { _DeviceStatus = value;
                RaisePropertyChanged();
            }
        }

    }
}
