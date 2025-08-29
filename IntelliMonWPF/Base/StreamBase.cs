using IntelliMonWPF.Models.Manger;
using Modbus.IO;
using System;
using System.IO.Ports;

internal class LoggingSerialResource : IStreamResource
{
    private readonly SerialPort _serialPort;
    private readonly ModbusDictManger modbusDictManger;
    private List<byte> _rxBuffer = new List<byte>();

    public LoggingSerialResource(SerialPort serialPort, ModbusDictManger dictManger)
    {
        _serialPort = serialPort;
        modbusDictManger = dictManger;
    }

    public int Read(byte[] buffer, int offset, int count)
    {
        int n = _serialPort.BaseStream.Read(buffer, offset, count);
        if (n > 0)
        {
            // 把本次收到的字节加入缓存
            for (int i = 0; i < n; i++)
                _rxBuffer.Add(buffer[offset + i]);

            // 循环尝试提取完整帧
            while (TryExtractFrame(out var frame))
            {
                modbusDictManger.MoudbusQueue.Enqueue("RX: " + BitConverter.ToString(frame));
            }
        }
        return n;
    }

    private bool TryExtractFrame(out byte[] frame)
    {
        frame = null;

        // 至少要有 5 字节（地址+功能码+最小 PDU+CRC）
        if (_rxBuffer.Count < 5) return false;

        // 根据功能码算 PDU 长度
        byte func = _rxBuffer[1];
        int pduLen;
        switch (func)
        {
            case 0x01 or 0x02 or 0x03 or 0x04:
                if (_rxBuffer.Count < 3) return false;
                pduLen = 2 + 1 + _rxBuffer[2];   // 2 字节头 + 1 字节字节计数 + 数据
                break;

            case 0x05 or 0x06:                 // 写单线圈/寄存器
                pduLen = 2 + 2;                // 固定 4 字节
                break;

            case 0x0F or 0x10:                // 写多线圈/寄存器
                if (_rxBuffer.Count < 7) return false;
                pduLen = 2 + 2 + 2 + 1 + _rxBuffer[6];   // 起始+数量+字节计数+数据
                break;

            default:
                // 未知功能码，直接整帧清空，避免死循环
                _rxBuffer.Clear();
                return false;
        }

        int total = 1 + 1 + pduLen + 2;       // 地址+功能码+pdu+crc
        if (_rxBuffer.Count < total) return false;

        var candidate = _rxBuffer.Take(total).ToArray();

        if (!CheckCrc(candidate))
        {
            // 找不到帧头，直接清空
            _rxBuffer.Clear();
            return false;
        }

        frame = candidate;
        _rxBuffer.RemoveRange(0, total);
        return true;
    }

    private bool CheckCrc(byte[] frame)
    {
        if (frame.Length < 3) return false;
        ushort calc = Crc16(frame, frame.Length - 2);
        ushort recv = (ushort)(frame[^1] << 8 | frame[^2]);
        return calc == recv;
    }

    private ushort Crc16(byte[] data, int length)
    {
        const ushort polynomial = 0xA001;
        ushort crc = 0xFFFF;

        for (int i = 0; i < length; i++)
        {
            crc ^= data[i];
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x0001) != 0)
                    crc = (ushort)((crc >> 1) ^ polynomial);
                else
                    crc >>= 1;
            }
        }

        return crc;
    }



    public void Write(byte[] buffer, int offset, int count)
    {
        var data = new byte[count];
        Array.Copy(buffer, offset, data, 0, count);
        Console.WriteLine("TX: " + BitConverter.ToString(data));

        _serialPort.BaseStream.Write(buffer, offset, count);
    }

    public void DiscardInBuffer() => _serialPort.DiscardInBuffer();
    public void Dispose() => _serialPort.Dispose();

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
}
