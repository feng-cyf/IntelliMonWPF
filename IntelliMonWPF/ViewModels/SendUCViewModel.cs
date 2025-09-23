using DryIoc.Messages;
using IntelliMonWPF.Enum;
using IntelliMonWPF.Interface;
using IntelliMonWPF.Models;
using IntelliMonWPF.Models.Manger;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IntelliMonWPF.ViewModels
{
    internal class SendUCViewModel : BindableBase,IDialogAware
    {
        public DialogCloseListener RequestClose { get; }=new DialogCloseListener();

        public bool CanCloseDialog()
        {
           return true;
        }

        public void OnDialogClosed()
        {
            
        }
        public ISnackbarMessageQueue MessageQueue { get; }
        private ModbusDictManger ModbusDictManger { get; set; }
        private IMessages messages;
        public SendUCViewModel(ModbusDictManger modbusDictManger,IMessages messages)
        {
            ModbusDictManger = modbusDictManger;
            MessageQueue= new SnackbarMessageQueue(TimeSpan.FromSeconds(4));
            SendCmd = new DelegateCommand(async()=>await Send());
            CloseCmd = new DelegateCommand(Close);
            this.messages = messages;
        }
        public void OnDialogOpened(IDialogParameters parameters)
        {
            var DeviceModel= parameters.GetValue<DeviceModel>("DeviceModel");
            var ReadModel = parameters.GetValue<ReadModel>("ReadModel");
            SelectedFuctionList = ReadModel.SendFuction;
            SlaveId=ReadModel.SlaveId;
            Name=DeviceModel.DeviceName;
        }
        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { _Name = value;
                RaisePropertyChanged();
            }
        }
        private int _SlaveId;

        public int SlaveId
        {
            get { return _SlaveId; }
            set { _SlaveId = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<KeyValuePair<ModbusEnum.SendType,string>> _SelectedFuctionList=new ObservableCollection<KeyValuePair<ModbusEnum.SendType, string>>();

        public ObservableCollection<KeyValuePair<ModbusEnum.SendType, string>> SelectedFuctionList
        {
            get { return _SelectedFuctionList; }
            set { _SelectedFuctionList = value;
                RaisePropertyChanged();
            }
        }
        private KeyValuePair<ModbusEnum.SendType,string> _SelectFuction;

        public KeyValuePair<ModbusEnum.SendType,string> SelectFuction
        {
            get { return _SelectFuction; }
            set { _SelectFuction = value;
                RaisePropertyChanged();
            }
        }

        private int _StartAddress;

        public int StartAddress
        {
            get { return _StartAddress; }
            set { _StartAddress = value;
                RaisePropertyChanged();
            }
        }

        private string _SendData;

        public string SendData
        {
            get { return _SendData; }
            set { _SendData = value;
                RaisePropertyChanged();
            }
        }

        private bool SureData(string Data)
        {
            if (Data == null) { MessageQueue?.Enqueue("数据不能为空");return false; }
            switch (SelectFuction.Key)
            {
                case ModbusEnum.SendType.WriteSingleCoil:
                    if (Data!="0" && Data != "1")
                    {
                        MessageQueue?.Enqueue("数据非法不合格", null, null, null, false, true, TimeSpan.FromSeconds(2));
                        return false;
                    }
                    break;
                case ModbusEnum.SendType.WriteMultipleCoils:
                    var bools = Data.Split(new[] {" ",","},StringSplitOptions.RemoveEmptyEntries);
                    if (!bools.Contains("1") && bools.Contains("0"))
                    {messages.ShowMessage("数据非法，请检查是否为零和一"); return false; }
                    break;
                case ModbusEnum.SendType.WriteSingleRegister:
                    if (!float.TryParse(Data,out float a))
                    {
                        MessageQueue?.Enqueue("非法数据，请检查是否为浮点数或者整数");
                        return false;
                    }
                    break;
                case ModbusEnum.SendType.WriteMultipleRegisters:
                    var result = Data.Split(new[] {" ",","},StringSplitOptions.RemoveEmptyEntries);
                    foreach (var s in result)
                    {
                        if (!float.TryParse(s,out float b))
                        {
                            MessageQueue?.Enqueue("存在非法数据");
                            return false;
                        }
                    }
                    break;
            }
            return true;
        }
        public DelegateCommand SendCmd {  get; set; }
        private async Task Send()
        {
            var master = ModbusDictManger.ModbusMangeDict[Name].Channel;
            var sendModel = new SendModel() { SavelId=Convert.ToByte(SlaveId),StartAddre=(ushort)StartAddress,SendType=SelectFuction.Key};
            if (!SureData(SendData)) return;

            switch (SelectFuction.Key)
            {
                case ModbusEnum.SendType.WriteSingleCoil:
                    sendModel.SendDataTypr[ModbusEnum.SendType.WriteSingleCoil] =
                        new Data { Statu = SendData == "1" };
                    await master.SendAsyance(sendModel);
                    break;

                case ModbusEnum.SendType.WriteMultipleCoils:
                    sendModel.SendDataTypr[ModbusEnum.SendType.WriteMultipleCoils] =
                        new Data
                        {
                            Status = SendData.Split(new[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(x => x == "1")
                                                   .ToArray()
                        };
                    await master.SendAsyance(sendModel);
                    break;

                case ModbusEnum.SendType.WriteSingleRegister:
                    if (int.TryParse(SendData, out int intVal))
                    {
                        var b = BitConverter.GetBytes(intVal);
                        sendModel.SendDataTypr[SelectFuction.Key] = new Data
                        {
                            arr = BitConverter.ToUInt16(b)
                        };
                    }
                    else if (float.TryParse(SendData, out float dblVal))
                    {
                        var bytes = BitConverter.GetBytes(dblVal); // 8字节
                        
                        sendModel.SendDataTypr[ModbusEnum.SendType.WriteMultipleRegisters] = new Data { arrs = new[] { BitConverter.ToUInt16(bytes, 0), BitConverter.ToUInt16(bytes, 2) } };
                        sendModel.SendType=ModbusEnum.SendType.WriteMultipleRegisters;
                    }
                    await master.SendAsyance(sendModel);
                    break;


                case ModbusEnum.SendType.WriteMultipleRegisters:
                    var parts = SendData.Split(new[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);
                    var registers = new List<ushort>();

                    foreach (var part in parts)
                    {
                        if (int.TryParse(part, out int val))
                        {
                            registers.Add(BitConverter.ToUInt16(BitConverter.GetBytes(val)));
                        }
                        else if (float.TryParse(part, out float dVal))
                        {
                            var b = BitConverter.GetBytes(dVal);
                            registers.AddRange(new[] { BitConverter.ToUInt16(b, 0), BitConverter.ToUInt16(b, 2) });
                        }
                    }

                    sendModel.SendDataTypr[SelectFuction.Key] = new Data { arrs = registers.ToArray() };
                    await master.SendAsyance(sendModel);
                    break;
            }
            SendStatus += $"\n已发送{SendData}";

        }
        public DelegateCommand CloseCmd { get; set; }
        private void Close()
        {
            RequestClose.Invoke();
        }
        private string _SendStatus;

        public string SendStatus
        {
            get { return _SendStatus; }
            set { _SendStatus = value;
                RaisePropertyChanged();
            }
        }



    }
}
