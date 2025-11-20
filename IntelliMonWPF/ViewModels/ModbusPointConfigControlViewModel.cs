using IntelliMonWPF.DTOs;
using IntelliMonWPF.Event;
using IntelliMonWPF.Event.EventBus;
using IntelliMonWPF.HttpClient;
using IntelliMonWPF.IF_Implements.Factory;
using IntelliMonWPF.Interface.IFactory;
using IntelliMonWPF.Interface.IMangerInferface;
using IntelliMonWPF.Models;
using IntelliMonWPF.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class ModbusPointConfigControlViewModel : BindableBase
    {
        private DataBus dataBus = DataBus.Instance;
        private IDialogService dialogService;
        private IDictManger<string,DeviceModel> modbusDictManger;
        private IDictMangerFactory dictMangerFactory;
        private DeviceSaveApiClient deviceSaveApiClient = new();
        private readonly IEventAggregator eventAggregator;
        public ModbusPointConfigControlViewModel(IDialogService dialogService, DictMangerFactory dictMangerFactory, IEventAggregator eventAggregator)
        {
            this.dictMangerFactory = dictMangerFactory;
            this.modbusDictManger = dictMangerFactory.CreateDictManger<string, DeviceModel>(DictMangerType.DeviceModel);
            this.dialogService = dialogService;
            AddPointCmd = new DelegateCommand(AddPoint);
            PointList = new ObservableCollection<PointModel>();
            PointView = CollectionViewSource.GetDefaultView(PointList);
            this.eventAggregator = eventAggregator;
            SavePointCmd = new DelegateCommand(async () => await SavePoint());
            UpdatePointCmd = new DelegateCommand(async () => await UpdatePoint());
            SearchPointCmd= new DelegateCommand(SeatchPoint);
            RemovePointEvent.GetRemovePointEvent += RemovePoint;
            UpdatePointEvent.GetUpdatePointEvent += UPdatePointList;
        }
        private PointModel _SelectPoint;

        public PointModel SelectPoint
        {
            get { return _SelectPoint; }
            set { _SelectPoint = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand AddPointCmd { get; set; }
        private void AddPoint()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Pointlist", PointList);
            dialogService.ShowDialog("AddModbusPoint", parameter);
        }
        private ObservableCollection<PointModel> _PointList;

        public ObservableCollection<PointModel> PointList
        {
            get { return _PointList; }
            set { _PointList = value;
                RaisePropertyChanged();
                PointView?.Refresh();
            }
        }
        private void RemovePoint(PointModel pm)
        {
            if (PointList.Contains(pm))
            {
                PointList.Remove(pm);
                LoggingService.Instance.Publish(LogType.PointConfig, $"移除设备 {pm.mapDevice.Value.Item1} 从站 {pm.mapDevice.Value.Item2} 点 {pm.PointName} 配置");
            }
        }
        private void UPdatePointList(UpdatePointClass pm)
        {
            var data = PointList.Where(x=>x.mapDevice.Key==pm.PointName
             && x.mapDevice.Value.Item1==pm.DeviceName).FirstOrDefault();
            if (data == null) return;
            data.Len=pm.Length;
            data.mapDevice= new KeyValuePair<string, (string, int)>(pm.PointName, (pm.DeviceName, pm.PointId));
            data.StartAddress=pm.StartAddress.ToString();
            data.RegisterType=pm.RegisterType;
        }
        public DelegateCommand SavePointCmd { get; set; }
        private async Task SavePoint()
        {
            List<PointDTO> pointDTOs = new List<PointDTO>();
            pointDTOs.AddRange(PointList.Select(x => new PointDTO
            {
                PointName = x.PointName,
                SlaveId = x.mapDevice.Value.Item2,
                DeviceName = x.mapDevice.Value.Item1,
                Unit = x.Unit,
                Desc = x.Desc,
                Len = x.Len,
                RegisterType = x.RegisterType,
                DataType = x.DataType,
                AccessType = x.AccessType,
                ScaleFactor = x.ScaleFactor,
                Offset = x.Offset,
            }));
            ApiRequest<List<PointDTO>> apiRequest = new ApiRequest<List<PointDTO>>() { Route = "deviceSave/data/pointSave", Method = RestSharp.Method.Post, Parsmeters = pointDTOs };
            var reault = await deviceSaveApiClient.PointSave(apiRequest);
            eventAggregator.GetEvent<ShowMesssgeWindow>().Publish(new Tuple<string, int>(reault.message, 3));
            var json= JsonConvert.SerializeObject(reault);
            LoggingService.Instance.Publish(LogType.PointApi, $"保存点位返回结果:{json}");
        }

        private string _Search;

        public string Search
        {
            get { return _Search; }
            set { _Search = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand RefreshCmd => new DelegateCommand(() =>
        {
            PointView.Filter = OnSearchList;
            PointView.Refresh();
        });

        private bool OnSearchList(object obj)
        {
            if (obj is not PointModel point) return false;

            if (string.IsNullOrWhiteSpace(Search)) return true;

            if (int.TryParse(Search, out int num))
            {
                return point.PointName.Contains(Search, StringComparison.OrdinalIgnoreCase)
                    || point.Desc.Contains(Search, StringComparison.OrdinalIgnoreCase)
                    || point.mapDevice.Value.Item2 == num;
            }

            return point.PointName.Contains(Search, StringComparison.OrdinalIgnoreCase)
                || point.Desc.Contains(Search, StringComparison.OrdinalIgnoreCase);
        }
        private ICollectionView _PointView;

        public ICollectionView PointView
        {
            get { return _PointView; }
            set { _PointView = value;
                RaisePropertyChanged();
            }
        }
        public DelegateCommand ClearCmd => new DelegateCommand(() => { Search = string.Empty; });
        public DelegateCommand ShowEditCmd => new DelegateCommand(() =>
        {
            if (SelectPoint == null) return;
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Pointlist", SelectPoint);
            dialogService.ShowDialog("EditPointUC", parameter);
        });
        public DelegateCommand DelPointCmd => new DelegateCommand(() =>
        {
            var result = MessageBox.Show("是否确认删除该点位？", "删除点位", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (SelectPoint != null && result == MessageBoxResult.OK)
            {
                var device = modbusDictManger.GetValue(SelectPoint.mapDevice.Value.Item1);
                int startAddress = 0;
                if (!string.IsNullOrWhiteSpace(SelectPoint.StartAddress))
                {
                    int.TryParse(SelectPoint.StartAddress, out startAddress);
                }
                var readModel = device.readMangerModbus[(SelectPoint.mapDevice.Value.Item1, SelectPoint.mapDevice.Value.Item2, startAddress)];
                readModel.PointModels = null;
                PointList.Remove(SelectPoint);
                LoggingService.Instance.Publish(LogType.PointConfig, $"删除设备 {SelectPoint.mapDevice.Value.Item1} 从站 {SelectPoint.mapDevice.Value.Item2} 点 {SelectPoint.PointName} 配置");
            }
        });

        public DelegateCommand UpdatePointCmd { get; }

        private async  Task UpdatePoint()
        {
            if (PointList.Count == 0) return;
            List<EditPointDTO> edit = new List<EditPointDTO>();
            edit.AddRange(PointList.Select(x=>new EditPointDTO()
            {
                DeviceName=x.mapDevice.Value.Item1,
                SlaveId=x.mapDevice.Value.Item2,
                PointName= x.PointName ?? "",
                AccessType=x.AccessType ?? "",
                Unit=x.Unit ?? "",
                ScaleFactor=x.ScaleFactor ?? "",
                Offset=x.Offset ??"",
                Desc=x.Desc ??""
            }));
            ApiRequest<List<EditPointDTO>> apiRequest = new ApiRequest<List<EditPointDTO>>() 
            {
                Method=RestSharp.Method.Post,
                Route= "deviceSave/data/editPoint",
                Parsmeters=edit
            };
            var result= await deviceSaveApiClient.EditPoint(apiRequest);
            eventAggregator.GetEvent<ShowMesssgeWindow>().Publish(new Tuple<string, int>(result.message, 3));
            var json=JsonConvert.SerializeObject(result);
            LoggingService.Instance.Publish(LogType.PointApi, $"更新点位返回结果:{json}");
        }
        private PointSearch _PointSearch;

        public PointSearch PointSearch
        {
            get { return _PointSearch; }
            set { _PointSearch = value;
                RaisePropertyChanged();
            }
        }
        public DelegateCommand SearchPointCmd { get; set; }
        private void SeatchPoint()
        {
            if (PointSearch == null) return;
            PointView.Filter = x =>
            {
                if (x is not PointModel point) return false;
                bool matches = true;
                if (!string.IsNullOrWhiteSpace(PointSearch.PointName))
                {
                    matches &= point.PointName.Contains(PointSearch.PointName, StringComparison.OrdinalIgnoreCase);
                }
                if (!string.IsNullOrWhiteSpace(PointSearch.RegisterType))
                {
                    matches &= point.RegisterType.Equals(PointSearch.RegisterType, StringComparison.OrdinalIgnoreCase);
                }
                if (!string.IsNullOrWhiteSpace(PointSearch.DataType))
                {
                    matches &= point.DataType.Equals(PointSearch.DataType, StringComparison.OrdinalIgnoreCase);
                }
                if (!string.IsNullOrWhiteSpace(PointSearch.AccessType))
                {
                    matches &= point.AccessType.Equals(PointSearch.AccessType, StringComparison.OrdinalIgnoreCase);
                }
                return matches;
            };
            PointView.Refresh();
        }
        public DelegateCommand PandasCmd => new DelegateCommand(() =>
        {
            if ( PointList.Count==0)
            {
                eventAggregator.GetEvent<ShowMesssgeWindow>().Publish(new Tuple<string, int>("列表无数据", 3));
                return;
            }
           DialogParameters parameter = new DialogParameters();
            parameter.Add("Pointlist", PointList);
            dialogService.ShowDialog("ToCEUC", parameter);
        });
        public DelegateCommand RefrashData => new DelegateCommand(() =>
        {
            if (PointList.Count == 0) return;
            dataBus.ClearOldKey(PointList.Select(x=>x.PointName));
            LoggingService.Instance.Publish(LogType.PointConfig, $"清除点位旧数据缓存");
        });
    }
    public class PointSearch
    {
        public string PointName { get; set; } = "";
        public string RegisterType { get; set; } = "";
        public string DataType { get; set; } = "";
        public string AccessType { get; set; } = "";
    }
}
