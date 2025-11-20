using IntelliMonWPF.Event.EventBus;
using IntelliMonWPF.Models;
using System;
using static IntelliMonWPF.Enum.ModbusEnum;

namespace IntelliMonWPF.Helper
{
    public static class DataConverter
    {
        private static DataBus _dataBus = DataBus.Instance;
        public static object ConvertData(ReadModel rm, object rawData)
        {
            if (rawData == null)
                return null;

            switch (rm.ModbusRead)
            {
                case ModbusRead.ReadCoils:
                case ModbusRead.ReadInputCoils:
                    if (rawData is bool[] coils)
                    {
                        _dataBus.AddData(rm, coils);
                        return coils;
                    }
                    else
                        throw new ArgumentException("线圈数据必须是 bool[] 类型");

                case ModbusRead.ReadRegisters:
                case ModbusRead.ReadInputRegister:
                    if (rawData is not ushort[] regs)
                        throw new ArgumentException("寄存器数据必须是 ushort[] 类型");

                    if (rm.NumAddress == 1)
                    {
                        _dataBus.AddData(rm, (int)regs[0]);
                        Console.WriteLine((int)regs[0]);
                        return (int)regs[0];
                    }

                    if (rm.NumAddress == 2)
                    {
                        byte[] bytes = new byte[4];

                        bytes[0] = (byte)(regs[0] >> 8);
                        bytes[1] = (byte)(regs[0] & 0xFF);
                        bytes[2] = (byte)(regs[1] >> 8);
                        bytes[3] = (byte)(regs[1] & 0xFF);

                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(bytes);
                        }

                        float result = BitConverter.ToSingle(bytes, 0);
                        _dataBus.AddData(rm, result);
                        return result;
                    }

                    return regs;

                default:
                    throw new NotSupportedException($"不支持的读取类型：{rm.ModbusRead}");
            }
        }
    }
}