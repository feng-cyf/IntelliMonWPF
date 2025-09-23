using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliMonWPF.Enum
{
    internal class ModbusEnum
    {
        public enum Modbus
        {
            SerialPort,
            TCP, UDP
        }
        public enum SerialPortType
        {
            RTU,ASCII
        }
        public enum SendType
        {
            WriteSingleCoil, WriteSingleRegister, WriteMultipleCoils, WriteMultipleRegisters
        }
        public enum ModbusRead
        {
            ReadInputCoils, ReadCoils, ReadRegisters, ReadInputRegister
        }
    }
}
