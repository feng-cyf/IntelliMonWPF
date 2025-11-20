using IntelliMonWPF.DTOs;
using IntelliMonWPF.Event;
using IntelliMonWPF.HttpClient;
using IntelliMonWPF.Models;
using IntelliMonWPF.Services;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IntelliMonWPF.ViewModels.DialogsViewModels
{
    public class ToCEUCViewModel : BindableBase, IDialogAware
    {
        public DialogCloseListener RequestClose { get; } = new DialogCloseListener();

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }
        private ObservableCollection<PointModel> _PointList;

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("Pointlist"))
            {
                _PointList = parameters.GetValue<ObservableCollection<PointModel>>("Pointlist");
            }
        }
        private string _Name="";

        public string Name
        {
            get { return _Name; }
            set { _Name = value;
                RaisePropertyChanged();
            }
        }
        private bool _Type=false;

        public bool Type
        {
            get { return _Type; }
            set { _Type = value;
                RaisePropertyChanged();
            }
        }
        private string _Part;

        public string Part
        {
            get { return _Part; }
            set { _Part = value;
                RaisePropertyChanged();
            }
        }
        public DelegateCommand PandasCmd { get; set; }
        private async Task Pandas()
        {
            if (string.IsNullOrEmpty(TypeStr()))
            {
                _EventAggregator.GetEvent<ShowMesssgeWindow>().Publish(new Tuple<string, int>("路径不能为空", 3));
                return;
            }
            TOCEDTO tOCEDTO = new TOCEDTO() 
            {
                name=Name,
                part=Part,
                Type=TypeStr(),
                l=_PointList.ToList()
            };
            ApiRequest<TOCEDTO> apiRequest = new ApiRequest<TOCEDTO>()
            {
                Method = RestSharp.Method.Post,
                Route = "pandas/csv_excel",
                Parsmeters = tOCEDTO
            };
            var result = await apiClient.GetTOCE(apiRequest);

            if (result.code == 200)
            {
                _EventAggregator.GetEvent<ShowMesssgeWindow>()
                    .Publish(new Tuple<string, int>(result.message, 4));
            }

            string jsonString = JsonConvert.SerializeObject(result, Formatting.None);

            LoggingService.Instance.Publish(LogType.Excel, jsonString);
            RequestClose.Invoke();
        }
        public DelegateCommand GetPart { get; private set; }
        private IEventAggregator _EventAggregator;
        private DeviceSaveApiClient apiClient;
        public ToCEUCViewModel(IEventAggregator eventAggregator,DeviceSaveApiClient apiClient)
        {
            GetPart = new DelegateCommand(() =>
            {
                OpenFolderDialog openFileDialog = new OpenFolderDialog { Title = "请选择保存的路径" };
                if (openFileDialog.ShowDialog() == true)
                {
                    string name = openFileDialog.FolderName;
                    Part = name;
                }
            });
            _EventAggregator = eventAggregator;
            this.apiClient = apiClient;
            PandasCmd = new DelegateCommand(async () => await Pandas());
        }
        private string TypeStr() => Type switch
        {
            true => "Csv",
            false => "Excel"
        };
    }
}
