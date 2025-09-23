using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using IntelliMonWPF.Models;
using IntelliMonWPF.Models.Manger;
using Prism.Commands;

namespace IntelliMonWPF.ViewModels.SettingsViewModel
{
    internal class EditPointUCViewModel : BindableBase, IDialogAware
    {
        #region 1. 基础绑定属性（设备/从站相关）
        // 设备名称列表（绑定ComboBox的ItemsSource）
        private ObservableCollection<string> _deviceNameList;
        public ObservableCollection<string> DeviceNameList
        {
            get => _deviceNameList;
            set { _deviceNameList = value; RaisePropertyChanged(); }
        }

        // 选中的设备名称（绑定ComboBox的SelectedItem）
        private string _selectDeviceName;
        public string SelectDeviceName
        {
            get => _selectDeviceName;
            set { _selectDeviceName = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<int> _slaveIdList;
        public ObservableCollection<int> SlaveIdList
        {
            get => _slaveIdList;
            set { _slaveIdList = value; RaisePropertyChanged(); }
        }

        // 选中的从站ID（绑定ComboBox的SelectedItem）
        private int _selectSlaveId;
        public int SelectSlaveId
        {
            get => _selectSlaveId;
            set { _selectSlaveId = value; RaisePropertyChanged(); }
        }
        #endregion

        #region 2. 点位核心属性（不可编辑项）
        // 点名称（绑定TextBox，不可编辑）
        private string _pointName;
        public string PointName
        {
            get => _pointName;
            set { _pointName = value; RaisePropertyChanged(); }
        }

        // 寄存器类型（绑定TextBlock，如"03-读保持寄存器"）
        private string _deviceFuction;
        public string DeviceFuction
        {
            get => _deviceFuction;
            set { _deviceFuction = value; RaisePropertyChanged(); }
        }

        // 起始地址（绑定TextBox，不可编辑）
        private string _startAddress;
        public string StartAddress
        {
            get => _startAddress;
            set { _startAddress = value; RaisePropertyChanged(); }
        }

        // 数据长度（绑定TextBox，不可编辑）
        private int _len;
        public int Len
        {
            get => _len;
            set { _len = value; RaisePropertyChanged(); }
        }
        #endregion

        #region 3. 可编辑配置属性
        // 数据类型（绑定ComboBox，如Bool/Int16等）
        private string _dataType;
        public string DataType
        {
            get => _dataType;
            set { _dataType = value; RaisePropertyChanged(); }
        }

        // 读写权限（绑定ComboBox，如Read Only/Read/Write）
        private string _accessType;
        public string AccessType
        {
            get => _accessType;
            set { _accessType = value; RaisePropertyChanged(); }
        }

        // 单位（绑定TextBox，如℃、m/s）
        private string _unit;
        public string Unit
        {
            get => _unit;
            set { _unit = value; RaisePropertyChanged(); }
        }

        // 缩放因子（绑定TextBox，默认1.0）
        private string _scaleFactor;
        public string ScaleFactor
        {
            get => _scaleFactor;
            set { _scaleFactor = value; RaisePropertyChanged(); }
        }

        // 偏移量（绑定TextBox，默认0）
        private string _offset;
        public string Offset
        {
            get => _offset;
            set { _offset = value; RaisePropertyChanged(); }
        }

        // 描述（绑定多行TextBox）
        private string _desc;
        public string Desc
        {
            get => _desc;
            set { _desc = value; RaisePropertyChanged(); }
        }
        #endregion

        #region 4. 命令（保存/取消）
        // 保存命令（绑定界面"保存"按钮）
        public ICommand UpdateCmd { get; }

        // 取消命令（绑定界面"取消"按钮）
        public ICommand CancelCmd { get; }
        #endregion

        #region 5. 对话框接口实现
        public DialogCloseListener RequestClose { get; } = new();

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        // 对话框打开时接收参数（如传递待编辑的点位数据）
        public void OnDialogOpened(IDialogParameters parameters)
        {

            if (parameters.ContainsKey("Pointlist"))
            {
                var editData = parameters.GetValue<PointModel>("Pointlist");
                LoadEditData(editData); 
            }
        }
        #endregion

        #region 6. 辅助方法
        // 将待编辑的点位数据加载到ViewModel属性
        private void LoadEditData(PointModel data)
        {
            if (data == null) return;

            // 加载基础关联数据
            SelectDeviceName = data.mapDevice.Value.Item1;
            SelectSlaveId = data.mapDevice.Value.Item2;

            // 加载不可编辑的核心数据
            PointName = data.PointName;
            DeviceFuction = data.RegisterType;
            StartAddress = data.StartAddress;
            Len = data.Len;

            // 加载可编辑的配置数据
            DataType = data.DataType;
            AccessType = data.AccessType;
            Unit = data.Unit;
            ScaleFactor = data.ScaleFactor;
            Offset = data.Offset;
            Desc = data.Desc;
        }

        // 保存按钮执行逻辑（实际需对接数据库/配置文件）
        private void ExecuteUpdateCmd()
        {
            // 1. 数据校验（如单位不为空、缩放因子合理等）
            if (string.IsNullOrWhiteSpace(DataType))
            {
                // 可通过DialogService弹出提示（需注入IDialogService）
                return;
            }
            var point= ModbusDictManger.ModbusMangeDict[SelectDeviceName].readMangerModbus[(SelectDeviceName, SelectSlaveId)].PointModel;
            point.DataType = DataType;
            point.AccessType = AccessType;
            point.Unit = Unit;
            point.ScaleFactor = ScaleFactor;
            point.Offset = Offset;
            point.Desc = Desc;


        }

        // 取消按钮执行逻辑（关闭对话框）
        private void ExecuteCancelCmd()
        {
            RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
        }
        #endregion

        #region 7. 构造函数（初始化命令）
        private ModbusDictManger ModbusDictManger;
        public EditPointUCViewModel(ModbusDictManger modbusDictManger)
        {
            // 初始化命令（Prism的DelegateCommand）
            UpdateCmd = new DelegateCommand(ExecuteUpdateCmd);
            CancelCmd = new DelegateCommand(ExecuteCancelCmd);
            ModbusDictManger = modbusDictManger;
        }
        #endregion
    }

    
}