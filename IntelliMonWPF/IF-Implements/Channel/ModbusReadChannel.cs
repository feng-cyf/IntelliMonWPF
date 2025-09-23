using Autofac.Integration.Web;
using IntelliMonWPF.Base;
using IntelliMonWPF.Enum;
using IntelliMonWPF.Interface;
using IntelliMonWPF.Interface.Ichannel;
using IntelliMonWPF.Models;
using IntelliMonWPF.Models.Manger;
using Modbus;
using Modbus.Device;
using Modbus.IO;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace IntelliMonWPF.IF_Implements.Channel
{
    internal class ModbusReadChannel : IModbusReadChannel
    {
        #region 字段/属性/事件（一字未改）
        private SerialPort SerialPort;
        private TcpClient TcpClient;
        private ModbusMaster master;
        private DispatcherTimer timer;
        private event Action Closer;
        private ModbusDictManger ModbusDictManger;
        private IMessages messages = ContainerLocator.Container.Resolve<IMessages>();
        public bool IsConnected
        {
            get
            {
                try
                {
                    if (SerialPort != null) return SerialPort.IsOpen;
                    if (TcpClient != null) return IsTcpConnecting && TcpClient.Connected;
                    return false;
                }
                catch { return false; }
            }
        }
        public event Action<byte[]> DataReceived;
        #endregion

        #region 公开接口（签名原样）
        public async Task CloseAsyance()
        {
            cts?.Dispose();
            modbusSniffer.Stop();
            if (timer != null && timer.IsEnabled) { timer.Stop(); timer = null; }
            TryCloseSerial();
            TryCloseTcp();
        }

        public async Task OpenAsyance(DeviceModel deviceModel)
        {
            DeviceModel = deviceModel;
            await CreateMasterByProtocolAsync();
            _ = Task.Run(async () => await OnHeart());
        }

        public async Task ReadAsyance(ReadModel readModel)
        {
            lock (_lock)
            {
                var self = this;
                SerialRtuReadCon.Add((ReadModel) => self.RebuildNow(readModel), readModel, () => master);
            }
        }

        public async Task SendAsyance<T>(T data)
        {
            if (data is SendModel sm)
            {
                switch (sm.SendType)
                {
                    case ModbusEnum.SendType.WriteSingleCoil:
                        await master.WriteSingleCoilAsync(sm.SavelId, sm.StartAddre, sm.SendDataTypr[ModbusEnum.SendType.WriteSingleCoil].Statu.Value);
                        break;
                    case ModbusEnum.SendType.WriteMultipleCoils:
                        await master.WriteMultipleCoilsAsync(sm.SavelId, sm.StartAddre, sm.SendDataTypr[ModbusEnum.SendType.WriteMultipleCoils].Status);
                        break;
                    case ModbusEnum.SendType.WriteSingleRegister:
                        await master.WriteSingleRegisterAsync(sm.SavelId, sm.StartAddre, sm.SendDataTypr[ModbusEnum.SendType.WriteSingleRegister].arr.Value);
                        break;
                    case ModbusEnum.SendType.WriteMultipleRegisters:
                        await master.WriteMultipleRegistersAsync(sm.SavelId, sm.StartAddre, sm.SendDataTypr[ModbusEnum.SendType.WriteMultipleRegisters].arrs);
                        break;
                }
            }
        }
        #endregion

        #region 原私有字段（一字未改）
        private DeviceModel DeviceModel;
        private bool OnGetHeart = true;
        private bool _isReading = false;
        private LoggingSerialResource _resource;
        private SerialRtuReadCon SerialRtuReadCon = new();
        private object _lock = new();
        private ModbusSniffer modbusSniffer;
        private SemaphoreSlim _restartLock = new(1, 1);
        private CancellationTokenSource cts;
        #endregion

        #region 抽出来的私有方法（仅移动代码）
        private async Task CreateMasterByProtocolAsync()
        {
            switch (DeviceModel.Protocol)
            {
                case ModbusEnum.Modbus.SerialPort:
                    await OpenSerialAsync(DeviceModel);
                    DeviceModel.Status = "已连接";
                    break;
                case ModbusEnum.Modbus.TCP:
                    await OpenTcpAsync(DeviceModel);
                    DeviceModel.Status = "已连接";
                    break;
            }
            if (master != null)
            {
                master.Transport.ReadTimeout = 2000;
                master.Transport.WriteTimeout = 2000;
                master.Transport.Retries = 0;
            }
        }

        private void TryCloseSerial()
        {
            try
            {
                if (SerialPort != null)
                {
                    if (SerialPort.IsOpen) SerialPort.Close();
                    SerialPort.Dispose();
                    SerialPort = null;
                }
            }
            catch { }
        }

        private void TryCloseTcp()
        {
            try
            {
                if (TcpClient != null)
                {
                    if (TcpClient.Connected) TcpClient.Close();
                    TcpClient.Dispose();
                    TcpClient = null;
                }
            }
            catch { }
        }
        #endregion
        private bool IsTcpConnecting { get; set; }

        #region 原有方法（代码挪进上面抽出的辅助方法，方法签名不变）
        private async Task<bool> OpenSerialAsync(DeviceModel deviceModel)
        {
            if (deviceModel.Config is not SerialPortModel serialPortModel)
            {
               messages.ShowMessage("未知串口配置类型");
                return false;
            }
            try
            {
                if (SerialPort != null && SerialPort.IsOpen) SerialPort.Close();

                SerialPort = new SerialPort
                {
                    PortName = serialPortModel.PortName,
                    BaudRate = serialPortModel.BaudRate,
                    DataBits = serialPortModel.DataBits,
                    StopBits = serialPortModel.StopBits,
                    Parity = serialPortModel.Parity,
                    RtsEnable = serialPortModel.RtsEnable,
                    DtrEnable = serialPortModel.DtrEnable,
                    Handshake = serialPortModel.CTsEnable ? Handshake.RequestToSend : Handshake.None,
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };
                ModbusDictManger modbusDictManger = ContainerLocator.Container.Resolve<ModbusDictManger>();
                SerialPort.Open();
                _resource = new LoggingSerialResource(SerialPort, modbusDictManger);

                if (SerialPort.IsOpen)
                {
                    master = deviceModel.SerialPortType switch
                    {
                        ModbusEnum.SerialPortType.RTU => ModbusSerialMaster.CreateRtu(_resource),
                        ModbusEnum.SerialPortType.ASCII => ModbusSerialMaster.CreateAscii(_resource),
                        _ => throw new NotSupportedException()
                    };
                }
                return true;
            }
            catch (Exception ex)
            {
               messages.ShowMessage($"串口打开失败: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> OpenTcpAsync(DeviceModel deviceModel, int timeoutSeconds = 3)
        {
            cts=new CancellationTokenSource();
            if (deviceModel.Config is not TcpModel tcpClientModel)
            {
               messages.ShowMessage("未知TCP配置类型");
                return false;
            }
            ModbusDictManger modbusDictManger = ContainerLocator.Container.Resolve<ModbusDictManger>();
            modbusSniffer = new ModbusSniffer();
            try
            {
                TcpClient = modbusSniffer.CreateProxy(localPort: modbusDictManger.LocationPort(),
                                           remoteIp: tcpClientModel.Ip,
                                           remotePort: tcpClientModel.Port,
                                           modbusDictManger);
                master = ModbusIpMaster.CreateIp(TcpClient);
                if (master != null)
                {
                    master.Transport.ReadTimeout = 2000;
                    master.Transport.WriteTimeout = 2000;
                    master.Transport.Retries = 0;
                }
                var a = await master.ReadHoldingRegistersAsync(1, 0, 1);
                IsTcpConnecting = (a != null && a.Length > 0);
                return a != null && a.Length > 0;
            }
            catch (Exception ex)
            {
               messages.ShowMessage($"TCP连接失败: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region 重建相关（原逻辑不动，仅把重复代码拎方法）
        private async Task RebuildNow(ReadModel readModel)
        {
            if (readModel.Modbus == ModbusEnum.Modbus.SerialPort)
                _ = Task.Run(() => RebuildMasterAsync());
            else if (readModel.Modbus == ModbusEnum.Modbus.TCP)
                _ = Task.Run(() => RebulidTcpASync(readModel));
        }

        private async Task RebuildMasterAsync()
        {
            if (!await _restartLock.WaitAsync(0)) return;
            try
            {
                CancelAllReadTokens();
                await Task.Delay(200);
                master?.Dispose(); master = null;
                TryCloseSerial();
                bool ok = await OpenSerialAsync(DeviceModel);
                if (!ok) SetAllStatus("设备掉线,读取终止");
                else ReRegisterAllReads();
            }
            catch { }
            finally { _restartLock.Release(); }
        }

        private async Task RebulidTcpASync(ReadModel rm)
        {
            rm.MaxRebuile++;
            rm.cts.Cancel(); rm.cts.Dispose();
            if (rm.MaxRebuile <= 3)
            {
                rm.Status = "正在尝试重启";
                rm.cts = new CancellationTokenSource();
                lock (_lock)
                    SerialRtuReadCon.Add((rm) => RebuildNow(rm), rm, () => master);
            }
            else
            {
                rm.MaxRebuile = 0;
                rm.IsAutoCon = true;
                rm.Status = "未知异常，重启失败，请排查后打开";
            }
        }

        private void CancelAllReadTokens()
        {
            var dict = ContainerLocator.Container.Resolve<ModbusDictManger>()
                                .ModbusMangeDict[DeviceModel.DeviceName].readMangerModbus;
            foreach (var item in dict.Values)
            {
                item.cts.Cancel(); item.cts.Dispose(); item.cts = new CancellationTokenSource();
            }
        }

        private void SetAllStatus(string msg)
        {
            var dict = ContainerLocator.Container.Resolve<ModbusDictManger>()
                                .ModbusMangeDict[DeviceModel.DeviceName].readMangerModbus;
            foreach (var item in dict.Values)
            {
                item.cts.Cancel(); item.cts.Dispose(); item.Status = msg;
            }
        }
        private void ReRegisterAllReads()
        {
            var dict = ContainerLocator.Container.Resolve<ModbusDictManger>()
                                .ModbusMangeDict[DeviceModel.DeviceName].readMangerModbus;
            foreach (var item in dict.Values)
            {
                item.cts= new CancellationTokenSource();
                if (master == null) return;
                lock (_lock)
                {
                    var self = this;
                    SerialRtuReadCon.Add((ReadModel) => self.RebuildNow(item), item, () => master);
                }
            }
        }
        #endregion

        #region 心跳（原样不动，仅把重建 TCP 逻辑拎方法）
        private async Task<T> RunAsync<T>(Task<T> task,CancellationToken token)
        {
            try
            {
                var delay = Task.Delay(2000, token);
                 await Task.WhenAll(task, delay);
                return await task;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private int _RevuildErroe = 0;
        private int _MaxError = 3;
        private async Task OnHeart()
        {
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(3000, cts.Token); 
                try
                {
                    var heart = await RunAsync(master.ReadHoldingRegistersAsync(1, 0, 1),cts.Token);  
                    if (heart == null)
                    { 
                        ++_RevuildErroe;
                        if (_RevuildErroe < _MaxError) return;
                        NotifyOffline();
                        bool ok = await RebuildTcpAsync();  
                        if (ok)
                        {
                            ReRegisterAllReads();
                            DeviceModel.Status="设备掉线，已重建连接";
                        }
                        else
                        {
                            DeviceModel.Status = ("设备掉线，重建连接失败");
                        }
                    }
                }
                catch (IOException ex)
                {
                    ++_RevuildErroe;
                    if (_RevuildErroe < _MaxError) return;
                    NotifyOffline();
                    bool ok = await RebuildTcpAsync(); 
                    if (ok)
                    {
                        ReRegisterAllReads();
                        DeviceModel.Status = ("IO错误，设备重建连接成功");
                    }
                    else
                    {
                        DeviceModel.Status = ($"IO错误: {ex.Message}, 设备重建连接失败");
                    }
                }
                catch (TimeoutException ex)
                {
                    DeviceModel.Status = ($"超时错误: {ex.Message}, 请稍后再试");
                }
                catch (InvalidOperationException)
                {
                    ++_RevuildErroe;
                    if (_RevuildErroe < _MaxError) return;
                    NotifyOffline();
                    bool ok = await RebuildTcpAsync(); 
                    if (ok)
                    {
                        ReRegisterAllReads();
                        DeviceModel.Status = ("设备状态异常，已重建连接");
                    }
                    else
                    {
                        DeviceModel.Status = ("设备状态异常，重建连接失败");
                    }
                }
                catch (Exception ex)
                {
                    DeviceModel.Status = ($"发生异常: {ex.Message}, 请检查设备或网络");
                }
            }
        }

        private void NotifyOffline()
        {
            foreach (var item in DeviceModel.readMangerModbus.Values)
            {
                item.cts.Dispose();
                item.Status = "心跳检测异常，正在尝试重建";
            }
        }

        private async Task<bool> RebuildTcpAsync()
        {
            try
            {
                cts?.Dispose();
                master?.Dispose();
                TcpClient?.Dispose();
                modbusSniffer?.Dispose();
                if (DeviceModel.Config is TcpModel tcp)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        bool ok = await OpenTcpAsync(DeviceModel);
                        if (ok)
                        {
                            _RevuildErroe = 0;
                            return true;
                        }
                        await Task.Delay(4000);
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                DeviceModel.Status = ($"重建TCP连接失败: {ex.GetType().Name}");
                return false;
            }
        }
        #endregion
    }
}
