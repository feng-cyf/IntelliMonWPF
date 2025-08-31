using IntelliMonWPF.Models;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace IntelliMonWPF.Base
{
    internal class TcpPack
    {
        public CancellationTokenSource _cts;
        private TcpClient TcpClient;
        private Thread _thread;

        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            TcpClient?.Close();  // 关闭TcpClient连接
            TcpClient?.Dispose();
            TcpClient = null;
        }

        private void OpenListener(string ip, int port)
        {
            _cts = new CancellationTokenSource();
            TcpClient = new TcpClient(ip, port);

            _thread = new Thread(async () => await ReadPack(_cts.Token));
            _thread.Start();
        }

        private async Task ReadPack(CancellationToken cancellationToken)
        {
            try
            {
                using (var stream = TcpClient.GetStream())
                {
                    byte[] buffer = new byte[1024]; // 缓存区大小为1024字节
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            // 读取到数据，输出接收到的字节数据
                            string receivedData = BitConverter.ToString(buffer, 0, bytesRead);
                            Console.WriteLine("Received data: " + receivedData);
                            MessageBox.Show("Received data: " + receivedData);
                        }
                        else
                        {
                            // 如果读取到0字节，则表示连接关闭
                            Console.WriteLine("No data received or connection closed.");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while reading data: " + ex.Message);
            }
        }

        public void Start(TcpModel tcpModel)
        {
            OpenListener(tcpModel.Ip, tcpModel.Port);
        }
    }
}
