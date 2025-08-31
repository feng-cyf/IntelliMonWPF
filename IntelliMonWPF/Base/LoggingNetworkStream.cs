using IntelliMonWPF.Models.Manger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IntelliMonWPF.Base
{
    internal class ModbusSniffer : IDisposable
    {
        private TcpListener _listener;
        private CancellationTokenSource _cts;
        private ModbusDictManger _modbusDictManger;

        /// <summary>
        /// 建立本地端口 -> 远端从站的透明代理，并在控制台打印所有收发
        /// </summary>
        public TcpClient CreateProxy(int localPort, string remoteIp, int remotePort, ModbusDictManger modbusDictManger)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _modbusDictManger = modbusDictManger;
            _listener = new TcpListener(IPAddress.Loopback, localPort);
            _listener.Start();

            _ = Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    TcpClient masterSide = await _listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => Forward(masterSide, remoteIp, remotePort, _cts.Token));
                }
            });

            // 返回一个已连到本地代理的 TcpClient
            var proxyClient = new TcpClient();
            proxyClient.Connect(IPAddress.Loopback, localPort);
            return proxyClient;
        }

        private async Task Forward(TcpClient masterSide, string remoteIp, int remotePort, CancellationToken token)
        {
            using var slaveSide = new TcpClient();
            await slaveSide.ConnectAsync(remoteIp, remotePort);

            var m2s = masterSide.GetStream();
            var s2m = slaveSide.GetStream();

            var t1 = CopyLog(m2s, s2m, "→ Slave", token);
            var t2 = CopyLog(s2m, m2s, "← Slave", token);

            await Task.WhenAny(t1, t2);
        }

        private async Task CopyLog(NetworkStream from, NetworkStream to, string dir, CancellationToken token)
        {
            var buf = new byte[1024];
            int len;
            while (!token.IsCancellationRequested && (len = await from.ReadAsync(buf, 0, buf.Length, token)) > 0)
            {
                _modbusDictManger.MoudbusQueue.Add($"{dir} {BitConverter.ToString(buf, 0, len)}");
                await to.WriteAsync(buf, 0, len, token);
            }
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _listener?.Stop();
            _listener?.Dispose();
            _listener = null;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
