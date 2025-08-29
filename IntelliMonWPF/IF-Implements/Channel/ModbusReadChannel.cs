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
                    TcpClient = null;
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
                var logSerialPort = new LoggingSerialResource(SerialPort,modbusDictManger);
               
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

        private async Task OpenTcpAsync(DeviceModel deviceModel, int timeoutSeconds = 3)
        {
            if (deviceModel.Config is not TcpModel tcpClientModel)
            {
                MessageBox.Show("未知TCP配置类型");
                return;
            }

            await CloseAsyance(); // 确保之前连接关闭

            TcpClient = new TcpClient();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

            try
            {
                await TcpClient.ConnectAsync(tcpClientModel.Ip, tcpClientModel.Port);

            }
            catch (Exception ex)
            {
                TcpClient?.Close();
                MessageBox.Show($"TCP连接失败: {ex.Message}");
                return;
            }
        }
         
        private async Task<T> RunWithTimeoutAsync<T>(Task<T> task ,DeviceModel deviceModel)
        {
            bool NotRead = false;
            int timeoutMilliseconds=deviceModel.ReadModel.ReadTimeout==0? 3000:deviceModel.ReadModel.ReadTimeout;
            var delayTask = Task.Delay(timeoutMilliseconds);

            var completed = await Task.WhenAny(task, delayTask);

            if (completed == delayTask)
            {
                MessageBox.Show("操作超时");
                return default; 
            }
            
            try
            {
                return await task;
            }
            catch (SlaveException ex)
            {
                NotRead= true;
                return default; 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"操作失败：{ex.Message}");
                return default;
            }
            finally
            {
                if (NotRead)
                {
                    deviceModel.Status = "异常";
                }
                else
                {
                    deviceModel.Status = "已连接";
                }
            }
        }



        public async Task ReadAsyance(DeviceModel deviceModel)
        {
            timer.Interval=TimeSpan.FromMilliseconds(deviceModel.PeriodTime);
            if (IsConnected)
            {
                byte SlaveId = Convert.ToByte(deviceModel.SlaveId);
                ushort StatrAdress = BitConverter.ToUInt16(BitConverter.GetBytes(deviceModel.ReadModel.StartAdress));
                ushort ReadCount = BitConverter.ToUInt16(BitConverter.GetBytes(deviceModel.ReadModel.NumAdress));

                switch (deviceModel.Function.Key)
                {
                    case "01":
                        bool[] status= await RunWithTimeoutAsync(master.ReadCoilsAsync(SlaveId, StatrAdress, ReadCount),deviceModel);
                        break;
                    case "02":
                        bool[] Inputs = await RunWithTimeoutAsync(master.ReadInputsAsync(SlaveId, StatrAdress, ReadCount), deviceModel);
                        break;
                    case "03":
                        ushort[] RWRegister = await RunWithTimeoutAsync(master.ReadHoldingRegistersAsync(SlaveId, StatrAdress, ReadCount), deviceModel);
                        break;
                    case "04":
                        ushort[] result = await RunWithTimeoutAsync(master.ReadInputRegistersAsync(SlaveId, StatrAdress, ReadCount), deviceModel);
                        break;
                }
                

            }
        }

        public Task SendAsyance<T>(T data)
        {
            throw new NotImplementedException();
        }
    }
}
