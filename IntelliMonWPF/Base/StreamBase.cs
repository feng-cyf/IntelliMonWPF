using IntelliMonWPF.Helper.Tools;

using Modbus.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;

public class LoggingSerialResource : IStreamResource, IDisposable
{
    private readonly SerialPort _serialPort;
    private SingelTool singelTool=>SingelTool.singelTool;
    /* 接收缓冲区 & 3.5T 计时 */
    private readonly List<byte> _rxBuffer = new();
    private DateTime _lastRxTime = DateTime.MinValue;
    private readonly double _char3_5T;          // 3.5 字符时间(ms)
    private readonly Timer _frameTimer;         // 轮询帧结束

    public LoggingSerialResource(SerialPort serialPort)
    {
        _serialPort = serialPort ?? throw new ArgumentNullException(nameof(serialPort));

        /* 计算 3.5 字符时间：11 bit/字符 × 3.5 */
        _char3_5T = 11.0 * 3.5 * 1000 / _serialPort.BaudRate;

        /* 10 ms 轮询一次是否静默超时 */
        _frameTimer = new Timer(OnFrameTimeout, null, 0, 10);
    }

    /*------------------------------------------------
     * IStreamResource 实现
     *------------------------------------------------*/
    public int Read(byte[] buffer, int offset, int count)
    {
        try
        {
            if (_serialPort == null || !_serialPort.IsOpen)
                return 0;

            int n = _serialPort.BaseStream.Read(buffer, offset, count);
            if (n > 0)
            {
                lock (_rxBuffer)
                {
                    _rxBuffer.AddRange(buffer.Skip(offset).Take(n));
                    _lastRxTime = DateTime.Now;
                }
            }
            return n;
        }
        catch (TimeoutException)
        {
            return 0;
        }
        catch (OperationCanceledException)
        {
            return 0;
        }
        catch (InvalidOperationException)
        {
            // 串口已关闭或 BaseStream 不可用
            return 0;
        }
    }

    public void Write(byte[] buffer, int offset, int count)
    {
        var tx = buffer.Skip(offset).Take(count).ToArray();
        singelTool.MoudbusQueue.Add("TX: " + BitConverter.ToString(tx));
        _serialPort.BaseStream.Write(buffer, offset, count);
    }

    public void DiscardInBuffer() => _serialPort.DiscardInBuffer();
    public void Dispose()
    {
        _frameTimer?.Dispose();
        _serialPort?.Dispose();
    }

    public int InfiniteTimeout => -1;
    public int ReadTimeout
    {
        get => _serialPort.ReadTimeout;
        set => _serialPort.ReadTimeout = value;
    }
    public int WriteTimeout
    {
        get => _serialPort.WriteTimeout;
        set => _serialPort.WriteTimeout = value;
    }

    /*------------------------------------------------
     * 3.5T 超时后解析整帧
     *------------------------------------------------*/
    private void OnFrameTimeout(object _)
    {
        lock (_rxBuffer)
        {
            if (_rxBuffer.Count == 0) return;

            /* 是否已静默足够长时间 */
            if ((DateTime.Now - _lastRxTime).TotalMilliseconds < _char3_5T)
                return;

            var frame = _rxBuffer.ToArray();
            _rxBuffer.Clear();

            /* CRC 校验 */
            if (frame.Length >= 5 && CheckCrc(frame))
            {
                singelTool.MoudbusQueue.Add("RX: " + BitConverter.ToString(frame));
            }
            /* CRC 错误 -> 直接丢弃整包，避免死循环 */
        }
    }

    /*------------------------------------------------
     * CRC16-Modbus
     *------------------------------------------------*/
    private static bool CheckCrc(byte[] frame)
    {
        int len = frame.Length;
        if (len < 3) return false;

        ushort calc = Crc16(frame, len - 2);
        ushort recv = (ushort)(frame[^1] << 8 | frame[^2]);
        return calc == recv;
    }

    private static ushort Crc16(byte[] data, int length)
    {
        const ushort poly = 0xA001;
        ushort crc = 0xFFFF;

        for (int i = 0; i < length; i++)
        {
            crc ^= data[i];
            for (int j = 0; j < 8; j++)
                crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ poly) : (ushort)(crc >> 1);
        }
        return crc;
    }
}