using Autofac.Integration.Web;
using IntelliMonWPF.Base;
using IntelliMonWPF.Enum;
using IntelliMonWPF.Interface.Ichannel;
using IntelliMonWPF.Models;
using IntelliMonWPF.Models.Manger;
using Modbus;
using Modbus.Device;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace IntelliMonWPF.IF_Implements.Channel
{
    internal class ModbusReadChannel : IModbusReadChannel
    {
        private SerialPort SerialPort;
        private TcpClient TcpClient;
        private ModbusMaster master;
        private DispatcherTimer timer;
        private TcpPack tcppack;
        private event Action Closer;
        public bool IsConnected
        {
            get
            {
                try
                {
                    if (SerialPort != null)
                        return SerialPort.IsOpen;

                    if (TcpClient != null)
                        return TcpClient.Connected;

                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        public event Action<byte[]> DataReceived;

        public async Task CloseAsyance()
        {
            modbusSniffer.Stop();
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            if (timer!=null && timer.IsEnabled)
            {
                timer.Stop();
                timer = null;
            }
            try
            {
                if (SerialPort != null)
                {
                    if (SerialPort.IsOpen)
                        SerialPort.Close();
                    SerialPort.Dispose();
                    SerialPort = null;
                }

                if (TcpClient != null)
                {
                    if (TcpClient.Connected)
                        TcpClient.Close();
                        TcpClient.Dispose();
                    TcpClient = null;
                    tcpPack.Stop();
                }

                master = null;
            }
            catch { }
        }

        public async Task OpenAsyance(DeviceModel deviceModel)
        {
            switch (deviceModel.Protocol)
            {
                case ModbusEnum.Modbus.SerialPort:
                    await OpenSerialAsync(deviceModel);
                    break;
                case ModbusEnum.Modbus.TCP:
                    await OpenTcpAsync(deviceModel);
                    break;
            }
            timer = new DispatcherTimer();

            timer.Tick += async (s, e) =>
            {
                if (_isReading) return;
                _isReading = true;
                try
                {
                    await ReadAsyance(deviceModel);
                }
                finally
                {
                    _isReading = false;
                }
            };
            timer.Start();
        }
        private bool _isReading = false;
        private async Task OpenSerialAsync(DeviceModel deviceModel)
        {
            if (deviceModel.Config is not SerialPortModel serialPortModel)
            {
                MessageBox.Show("未知串口配置类型");
                return;
            }

            try
            {
                if (SerialPort != null && SerialPort.IsOpen)
                    SerialPort.Close();

                SerialPort = new SerialPort
                {
                    PortName = serialPortModel.PortName,
                    BaudRate = serialPortModel.BaudRate,
                    DataBits = serialPortModel.DataBits,
                    StopBits = serialPortModel.StopBits,
                    Parity = serialPortModel.Parity,
                    RtsEnable = serialPortModel.RtsEnable,
                    DtrEnable = serialPortModel.DtrEnable,
                    Handshake = serialPortModel.CTsEnable ? Handshake.RequestToSend : Handshake.None
                };
                ModbusDictManger modbusDictManger = ContainerLocator.Container.Resolve<ModbusDictManger>();
                SerialPort.Open();
                SerialPort.ReadTimeout = 1000;
                var logSerialPort = new LoggingSerialResource(SerialPort, modbusDictManger);

                if (SerialPort.IsOpen)
                {
                    master = deviceModel.SerialPortType switch
                    {
                        ModbusEnum.SerialPortType.RTU => ModbusSerialMaster.CreateRtu(logSerialPort),
                        ModbusEnum.SerialPortType.ASCII => ModbusSerialMaster.CreateAscii(logSerialPort),
                        _ => throw new NotSupportedException()
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"串口打开失败: {ex.Message}");
            }
           

        }
        private TcpPack tcpPack=new TcpPack();
        private ModbusSniffer modbusSniffer;

        private async Task OpenTcpAsync(DeviceModel deviceModel, int timeoutSeconds = 3)
        {
            if (deviceModel.Config is not TcpModel tcpClientModel)
            {
                MessageBox.Show("未知TCP配置类型");
                return;
            }

            ModbusDictManger modbusDictManger= ContainerLocator.Container.Resolve<ModbusDictManger>();
            modbusSniffer = new ModbusSniffer();
            try
            {
                TcpClient = modbusSniffer.CreateProxy(localPort: modbusDictManger.LocationPort(),
                                       remoteIp: tcpClientModel.Ip,
                                       remotePort: tcpClientModel.Port,
                                       modbusDictManger);
                master = ModbusIpMaster.CreateIp(TcpClient);

            }
            catch (Exception ex)
            {
                TcpClient?.Close();
                MessageBox.Show($"TCP连接失败: {ex.Message}");
                return;
            }
        }

        /*================= 公共字段 =================*/
        private int _emptyReadCount = 0;
        private const int MaxEmptyRead = 3;
        private CancellationTokenSource _cts;
        /*================= 唯一超时包装器 =================*/
        private async Task<T> RunWithTimeoutAsync<T>(Task<T> task, DeviceModel dm)
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose(); 
            }
            _cts = new CancellationTokenSource();

            int timeout = dm.ReadModel.ReadTimeout == 0 ? 3100 : dm.ReadModel.ReadTimeout;
            _cts.CancelAfter(timeout);

            // 使用带取消令牌的延迟任务
            var delay = Task.Delay(Timeout.Infinite, _cts.Token);

            var completedTask = await Task.WhenAny(task, delay);

            if (completedTask == delay)
            {
                dm.Status = "超时";
                goto REBUILD;
            }
            else
            {
                try
                {
                    var result = await task;

                    if (result is Array arr && arr.Length == 0)
                    {
                        _emptyReadCount++;
                        if (_emptyReadCount >= MaxEmptyRead)
                        {
                            dm.Status = "异常-空读";
                            goto REBUILD;
                        }
                    }
                    else
                    {
                        _emptyReadCount = 0;
                        dm.Status = "已连接";
                    }
                    return result;
                }
                catch (SlaveException)
                {
                    dm.Status = "从站异常";
                }
                catch (Exception ex)
                {
                    throw;
                    dm.Status = $"异常-{ex.GetType().Name}";
                    goto REBUILD;
                }
            }

        REBUILD:
            _emptyReadCount = 0;
            await RebuildMasterAsync(dm);
            return default;
        }


        /*================= ReadAsyance：一行搞定 =================*/
        public async Task ReadAsyance(DeviceModel dm)
        {
            timer.Interval = TimeSpan.FromMilliseconds(dm.PeriodTime);
            if (!IsConnected) return;

            byte id = Convert.ToByte(dm.SlaveId);
            ushort addr = (ushort)dm.ReadModel.StartAdress;
            ushort cnt = (ushort)dm.ReadModel.NumAdress;

            switch (dm.Function.Key)
            {
                case "01": await RunWithTimeoutAsync(master.ReadCoilsAsync(id, addr, cnt), dm); break;
                case "02": await RunWithTimeoutAsync(master.ReadInputsAsync(id, addr, cnt), dm); break;
                case "03":var a= await RunWithTimeoutAsync(master.ReadHoldingRegistersAsync(id, addr, cnt), dm); 
                    break;
                case "04": await RunWithTimeoutAsync(master.ReadInputRegistersAsync(id, addr, cnt), dm); break;
            }
        }

        /*================= 串口+Master 重建 =================*/
        private async Task RebuildMasterAsync(DeviceModel dm)
        {
            master?.Dispose();
            master = null;

            try
            {
                if (dm.Protocol == ModbusEnum.Modbus.SerialPort)
                {
                    // 先关再开，彻底清缓存
                    if (SerialPort != null && SerialPort.IsOpen)
                        SerialPort.Close();
                    SerialPort?.Dispose();

                    await OpenSerialAsync(dm);   // 复用你已有的方法
                }
                else
                {
                    /* TCP 同理 */
                    await OpenTcpAsync(dm);
                }
            }
            catch (Exception ex)
            {
                dm.Status = $"重建失败-{ex.Message}";
            }
        }

        public Task SendAsyance<T>(T data)
        {
            throw new NotImplementedException();
        }
    }
}
