using IntelliMonWPF.Enum;
using IntelliMonWPF.Interface.Ichannel;
using IntelliMonWPF.Models;
using Modbus.Device;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IntelliMonWPF.IF_Implements.Channel
{
    internal class ModbusReadChannel : IModbusReadChannel
    {
        private SerialPort SerialPort;
        private TcpClient TcpClient;
        private ModbusMaster master;
        public bool IsConnected
        {
            get
            {
                try
                {
                    if (master != null && SerialPort != null)
                        return SerialPort.IsOpen;

                    if (master != null)
                    {
                        if (TcpClient != null)
                            return TcpClient.Connected;
                        
                    }
                    return false;
                }
                catch (ObjectDisposedException)
                {
                    return false;
                }
            }
        }

        public event Action<byte[]> DataReceived;

        public async Task CloseAsyance()
        {
           if (SerialPort!=null && SerialPort.IsOpen)
            {
                SerialPort.Close();
                SerialPort.Dispose();
            }
            if (TcpClient!=null && TcpClient.Connected)
            {
                TcpClient = null;
            }
            master = null;
        }

    

        public async Task OpenAsyance(DeviceModel deviceModel)
        {
           switch (deviceModel.Protocol)
            {
                case ModbusEnum.Modbus.SerialPort:
                    if (deviceModel.Config is SerialPortModel serialPortModel)
                    {
                        try
                        {
                            if (SerialPort != null && SerialPort.IsOpen)
                                SerialPort.Close(); // 防止重复打开

                            SerialPort = new SerialPort
                            {
                                PortName = serialPortModel.PortName,
                                BaudRate = serialPortModel.BaudRate,
                                DataBits = serialPortModel.DataBits,
                                StopBits = serialPortModel.StopBits,
                                Parity = serialPortModel.Parity,
                                RtsEnable = serialPortModel.RtsEnable,
                                DtrEnable = serialPortModel.DtrEnable,
                            };

                            if (serialPortModel.CTsEnable)
                                SerialPort.Handshake = Handshake.RequestToSend;

                            SerialPort.Open();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"串口打开失败：{ex.Message}");
                        }

                        if (SerialPort.IsOpen)
                        {
                            if (deviceModel.SerialPortType == ModbusEnum.SerialPortType.RTU)
                                master = ModbusSerialMaster.CreateRtu(SerialPort);
                            else if (deviceModel.SerialPortType == ModbusEnum.SerialPortType.ASCII)
                                master = ModbusSerialMaster.CreateAscii(SerialPort);
                        }
                       
                    }
                    else { MessageBox.Show("未知类型"); }
                    break;
                case ModbusEnum.Modbus.TCP:
                    if (deviceModel.Config is TcpModel tcpClientModel)
                    {
                        await Task.Run(async () =>
                        {
                            var tcp = new TcpClient();
                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                            var connectTask = tcp.ConnectAsync(tcpClientModel.Ip, tcpClientModel.Port);

                            var completed = await Task.WhenAny(connectTask, Task.Delay(-1, cts.Token));
                            if (completed == connectTask)
                            {
                                await connectTask;
                                if (tcp.Connected)
                                    master = ModbusIpMaster.CreateIp(tcp);
                            }
                            else
                            {
                                tcp.Close();
                                MessageBox.Show("连接超时");
                            }
                        });

                    }
                    break;
            }
        }
        public async Task<bool> OpenTcpAsync(TcpModel tcpClientModel, int timeoutSeconds = 3)
        {
            TcpClient = new TcpClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

            try
            {
                var connectTask = TcpClient.ConnectAsync(tcpClientModel.Ip, tcpClientModel.Port);
                var completed = await Task.WhenAny(connectTask, Task.Delay(-1, cts.Token));

                if (completed != connectTask)
                {
                    TcpClient.Close();
                    return false; // 超时
                }

                await connectTask; // 确保异常抛出
                if (TcpClient.Connected)
                {
                    master = ModbusIpMaster.CreateIp(TcpClient);
                    return true;
                }

                return false;
            }
            catch
            {
                TcpClient?.Close();
                return false;
            }
        }


        public Task ReadAsyance()
        {
            throw new NotImplementedException();
        }

       

        public Task SendAsyance<T>(T data)
        {
            throw new NotImplementedException();
        }
    }
}
