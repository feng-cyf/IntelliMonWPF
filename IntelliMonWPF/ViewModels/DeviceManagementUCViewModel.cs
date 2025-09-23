using IntelliMonWPF.DTOs;
using IntelliMonWPF.Event;
using IntelliMonWPF.HttpClient;
using IntelliMonWPF.Interface;
using IntelliMonWPF.Models;
using IntelliMonWPF.Models.Manger;
using MaterialDesignThemes.Wpf;
using Modbus.Device;
using Prism.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
        private IMessages messages;
        private IEventAggregator eventAggregator;
        public DeviceManagementUCViewModel(IDialogService dialogService, ModbusDictManger modbusDictManger,IMessages messages,IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            _dialogService = dialogService;
            AddDeviceCommand = new DelegateCommand(OnAddDevice);
            EditDeviceCmd = new DelegateCommand(EditDebice);
            this.modbusDevice = modbusDictManger;
            Devices = this.modbusDevice.ModbusMangeList;
            ShowPackUCCmdCommand = new DelegateCommand(ShowPackUC);
            StopDeviceCmd = new DelegateCommand(async () => { await StopDevice(); });
            OpenDeviceCmd=new DelegateCommand(async () => { await OpenDevice(); });
            RemoveDeviceCmd = new DelegateCommand(async () => { await RemoveDevice(); });
            ShoeAddDeviceCmd = new DelegateCommand(ShowAddDevice);
             SearchDeviceCmd = new DelegateCommand(Refresh);
            StopAllDeviceCmd = new DelegateCommand(StopAllDevice);
            StartDeviceCmd = new DelegateCommand(async () => await StartDevice());
            RowDoubleClickCommand = new DelegateCommand(RowDoubleClick);
            this.messages = messages;
            ApiSaveCmd = new DelegateCommand(async()=> await ApiSave());
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

            if (SelectedDevice == null)
            {
               messages.ShowMessage("请选择设备");
                return;
            }

            if (SelectedReadModel == null)
            {
               messages.ShowMessage("请选择要编辑的从站配置");
                return;
            }
            // 打开编辑窗口
            DialogParameters parameter = new DialogParameters();
            parameter.Add("DeviceName", SelectedDevice.DeviceName);
            parameter.Add("ReadModelPar", SelectedReadModel);
            _dialogService.ShowDialog("EditDeviceUC", parameter, callback => { });
        }
        private ReadModel _selectedReadModel;
        public ReadModel SelectedReadModel
        {
            get => _selectedReadModel;
            set => SetProperty(ref _selectedReadModel, value);
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
            if (SelectedReadModel == null)
            {
               messages.ShowMessage("请选择要停止的从站设备"); return;
            }
            if (SelectedReadModel.cts.IsCancellationRequested)
            {
               messages.ShowMessage("该设备已经停止");
                return;
            }
            if (SelectedReadModel.cts != null && !SelectedReadModel.cts.IsCancellationRequested)
            {
                SelectedReadModel.cts.Cancel();
                SelectedReadModel.cts.Dispose();
                SelectedReadModel.Status = "已关闭";
            }
        }
        public DelegateCommand OpenDeviceCmd { get; set; }
        private async Task OpenDevice()
        {
            if (SelectedReadModel == null)
            {
               messages.ShowMessage("请选择要启动的从站设备"); return;
            }
            if (!SelectedReadModel.cts.IsCancellationRequested) {messages.ShowMessage("设备正在运行"); }
            DeviceModel model = modbusDevice.ModbusMangeDict[SelectedDevice.DeviceName];
            SelectedReadModel.cts = new CancellationTokenSource();
            await SelectedDevice.Channel.ReadAsyance(SelectedReadModel);
        }

        public DelegateCommand RemoveDeviceCmd { get; set; }
        private ICollectionView collectionView;
        private async Task RemoveDevice() 
        {
            if (SelectedReadModel == null)
            {
               messages.ShowMessage("请选择从站设备");return;
            }
            string path = $"是否删除设备{SelectedDevice.DeviceName}之中的{SelectedReadModel.SlaveId}?";
            if (!SelectedReadModel.cts.IsCancellationRequested)
            {
                path = "设备正在运行,\n" + path;
            }
            var result = messages.ShowMessageSure(path, "提醒");
            if (result)
            {
                SelectedReadModel.cts.Cancel(); SelectedReadModel.cts.Dispose();
                modbusDevice.Remove(SelectedDevice.DeviceName,SelectedReadModel.SlaveId,SelectedReadModel);
            }
        }
        public DelegateCommand StartDeviceCmd { get; set; }
        private async Task StartDevice()
        {
            if (SelectedReadModel == null) {messages.ShowMessage("请选择设备"); return; }
            if (SelectedDevice.Channel.IsConnected) {messages.ShowMessage("设备已经打开"); }
            await SelectedDevice.Channel.OpenAsyance(SelectedDevice);
            foreach(var item in SelectedDevice.readMangerModbus.Values)
            {
                item.cts= new CancellationTokenSource();
                _=Task.Run(async()=> await SelectedDevice.Channel.ReadAsyance(item));
            } 
        }
        public DelegateCommand StopAllDeviceCmd {  get; set; }
        private void StopAllDevice()
        {
            if (SelectedDevice == null)
            {
               messages.ShowMessage("请选择设备");return;
            }
           foreach (var item in SelectedDevice.readMangerModbus.Values)
            {
                if (!SelectedDevice.Channel.IsConnected && item.cts.IsCancellationRequested)
                {
                    messages.ShowMessage("当前设备已经关闭");
                    return;
                }
                item.cts.Dispose();
                item.Status = "从站已关闭";
            }
           SelectedDevice.Channel.CloseAsyance();
            if (!SelectedDevice.Channel.IsConnected)
            {
                SelectedDevice.Status = "已关闭";
            }
        }
        public DelegateCommand ShoeAddDeviceCmd { get; set; }
        private void ShowAddDevice()
        {
            _dialogService.Show("AddDeviceModbusUC");
        }
        public DelegateCommand SearchDeviceCmd {  get; set; }
        private void Refresh()
        {
            collectionView = CollectionViewSource.GetDefaultView(modbusDevice.ModbusMangeList);
            collectionView.Filter = FilterDevice;
            collectionView.Refresh();
        }

        public DelegateCommand RowDoubleClickCommand {  get; set; }
        private void RowDoubleClick()
        {
            DialogParameters paramerter = new();
            paramerter.Add("DeviceModel", SelectedDevice);
            paramerter.Add("ReadModel", SelectedReadModel);
            if (SelectedReadModel.Function == "02" || SelectedReadModel.Function == "04")
            {
                eventAggregator.GetEvent<ShowMesssgeWindow>().Publish(new Tuple<string, int>("该功能码为只读", 3));
                return;
            }
            _dialogService.Show("SendUC", paramerter, callback => { });
        }
        private bool FilterDevice(object item)
        {
            if (item is not DeviceModel device)
                return false;

            // 名称过滤
            bool nameMatch = string.IsNullOrEmpty(DeviceCon) ||
                            device.readMangerModbus.Values.Any(x => x.SlaveId == Convert.ToInt32(DeviceCon));

            //// 连接（IP）过滤
            //bool conMatch = string.IsNullOrEmpty(DeviceName) ||
            //               device.readMangerModbus.Keys.Contains();

            // 状态过滤
            bool statusMatch = string.IsNullOrEmpty(DeviceStatus) ||
                              (device.readMangerModbus.Values.Any(x=>x.Status.Contains(DeviceStatus)));

            return nameMatch && statusMatch;
        }

        public DelegateCommand ApiSaveCmd {  get; set; }
        private DeviceSaveApiClient DeviceSaveApiClient = new();
        private async Task ApiSave()
        {
            List<DeviceDTO> deviceDTOs = new List<DeviceDTO>();
            deviceDTOs.AddRange(
                Devices
                    .SelectMany(x => x.ReadModels)  
                    .Select(readModel => new DeviceDTO  
                    {
                        DeviceName = readModel.DeviceName,
                        SlaveId = readModel.SlaveId,
                    })
            );
           ApiRequest<List<DeviceDTO>> apiRequest= new ApiRequest<List<DeviceDTO>>() 
           {
               Method=RestSharp.Method.Post,
               Route= "deviceSave/data/batch",
               Parsmeters=deviceDTOs
           };
           var result= await DeviceSaveApiClient.DeviceSave(apiRequest);
            eventAggregator.GetEvent<ShowMesssgeWindow>().Publish(new Tuple<string, int>(result.message, 3));
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
