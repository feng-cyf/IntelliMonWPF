using IntelliMonWPF.DTOs;
using IntelliMonWPF.Enum;
using IntelliMonWPF.HttpClient;
using IntelliMonWPF.Models;
using Modbus;
using Modbus.Device;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;

internal class SerialRtuReadCon
{
    public static class SerialPortLock
    {
        public static readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);
    }
    private ApiClient _client=new ApiClient();
    public void Add(Func<ReadModel, Task> rebuildFunc, ReadModel readModel, Func<ModbusMaster?> masterProvider)
    {
        _ = Task.Run(async () => { await ReadLoopAsync(rebuildFunc, readModel, masterProvider); });
    }

    private async Task ReadLoopAsync(Func<ReadModel, Task> rebuildFunc, ReadModel readModel, Func<ModbusMaster?> masterProvider)
    {
        while (true)
        {
            if (readModel.cts.IsCancellationRequested) break;
            
            try
            {
                var m = masterProvider();
                if (m != null)
                    await ReadOnceAsync(rebuildFunc, readModel, m);
                else
                    readModel.Status = "master为空";
            }
            catch (ObjectDisposedException)
            {
                readModel.Status = "master已释放";
                break;
            }
            catch (Exception ex)
            {
                readModel.Status = $"异常: {ex.GetType().Name}";
            }

            try
            {
                await Task.Delay(readModel.Interavel, readModel.cts.Token);
            }
            catch (TaskCanceledException)
            {
                readModel.Status = "读取循环已取消";
                break; // 取消时直接跳出
            }
        }

    }

    public async Task ReadOnceAsync(Func<ReadModel, Task> rebuildFunc, ReadModel readModel, ModbusMaster master)
    {
        await SerialPortLock.Lock.WaitAsync(readModel.cts.Token);  // 🔒 串口全局锁
        try
        {
            byte slaveId = Convert.ToByte(readModel.SlaveId);
            ushort startAddr = (ushort)readModel.StartAddress;
            ushort count = (ushort)readModel.NumAddress;

            switch (readModel.ModbusRead)
            {
                case ModbusEnum.ModbusRead.ReadCoils:
                    var coils = await RunAsync(master.ReadCoilsAsync(slaveId, startAddr, count), readModel, rebuildFunc);
                    if (coils != null)
                    {
                        readModel.Status = $"读到 {((bool[])coils).Length} 个线圈";
                        // 可根据需要发送线圈数据
                        // await SendCoilData(readModel, (bool[])coils);
                    }
                    break;

                case ModbusEnum.ModbusRead.ReadInputCoils:
                    var inputs = await RunAsync(master.ReadInputsAsync(slaveId, startAddr, count), readModel, rebuildFunc);
                    if (inputs != null)
                    {
                        readModel.Status = $"读到 {((bool[])inputs).Length} 个输入线圈";
                        // 可发送输入线圈数据
                    }
                    break;

                case ModbusEnum.ModbusRead.ReadRegisters:
                    var regs = await RunAsync(master.ReadHoldingRegistersAsync(slaveId, startAddr, count), readModel, rebuildFunc);
                    if (regs != null)
                    {
                        readModel.Status = $"读到 {regs.Length} 个寄存器";
                        //await SendData(readModel, regs);  // 调用 API 发送数据
                    }
                    break;

                case ModbusEnum.ModbusRead.ReadInputRegister:
                    var inputRegs = await RunAsync(master.ReadInputRegistersAsync(slaveId, startAddr, count), readModel, rebuildFunc);
                    if (inputRegs != null)
                    {
                        readModel.Status = $"读到 {inputRegs.Length} 个输入寄存器";
                        // 可发送输入寄存器数据
                        //await SendData(readModel, inputRegs);
                    }
                    break;

                default:
                    readModel.Status = "未知读取类型";
                    break;
            }
        }
        catch (Exception ex)
        {
            readModel.Status = $"读取异常: {ex.Message}";
        }
        finally
        {
            SerialPortLock.Lock.Release();  // 🔓 释放锁
        }
    }


    private async Task<T> RunAsync<T>(Task<T> task, ReadModel rm, Func<ReadModel, Task> rebuildFunc)
    {

        var timeoutTask = Task.Delay(2500, rm.cts.Token);
        var finished = await Task.WhenAny(task, timeoutTask);
        try
        {
            if (finished != task)
            {
                rm.Status = "超时";
                rm.ErrorCount++;
                if (rm.ErrorCount >= rm.MaxError)
                {
                    goto rebuildFunc;
                }
                return default!;
            }

            var result = await task;
            if (result is Array arr && arr.Length == 0)
            {
                rm.Status = "空读";
                return default!;
            }

            rm.Status = "已连接";
            rm.ErrorCount = 0; // 成功清零
            rm.MaxRebuile = 0;
            return result;
        }
        catch (SlaveException se) when (se.SlaveExceptionCode <= 2)
        {
            // 地址/功能码问题，本轮放弃即可
            rm.Status = $"从站拒绝-{se.SlaveExceptionCode}";
            return default!;
        }
        catch (SlaveException se) when (se.SlaveExceptionCode >= 3)
        {
            // 04 内部故障，给两次机会
            rm.ErrorCount++;
            if (rm.ErrorCount < 2) return default!;
            goto rebuildFunc;          // 走重建
        }
        catch (SocketException sock) when (sock.ErrorCode is 10054 or 10060)
        {
            // 物理断或被踢
            goto rebuildFunc;
        }
        catch (SocketException sock) when (sock.ErrorCode is 10061)
        {
            // 端口没开，永远连不上
            rm.Status = "目标拒绝连接，请检查IP/端口";
            return default!;
        }
        catch (TimeoutException)
        {
            rm.Status = "超时";
            rm.ErrorCount++;
            if (rm.ErrorCount >= rm.MaxError)
            {
                goto rebuildFunc;
            }
        }
        catch (SlaveException)
        {
            rm.Status = "从站异常";
            return default!;
        }
        catch (OperationCanceledException)
        {
            rm.Status = "已取消";
            return default!;
        }
        catch (NullReferenceException)
        {
            rm.Status = "master异常";
            goto rebuildFunc;

        }
        catch (ObjectDisposedException)
        {
            rm.Status = "master已释放";
            goto rebuildFunc;
        }
        catch (Exception ex)
        {
            rm.Status = $"未知异常-{ex.GetType().Name}";
            goto rebuildFunc;
        }
    rebuildFunc:
        rm.ErrorCount = 0;
        await rebuildFunc(rm);
        if (rm.Modbus == ModbusEnum.Modbus.SerialPort)
        {
            rm.cts.Cancel();
            rm.cts.Dispose();
            rm.Status = "异常已关闭,请排查后打开";
        }
        return default!;
    }
    private async Task SendData(ReadModel rm, ushort[] ushorts)
    {
        DataSendDTO dataSendDTO = new DataSendDTO() 
        {
            DeviceName=rm.DeviceName,SlaveId=rm.SlaveId,ushorts=ushorts.Select(x=>(float)x).ToList()
        };
        ApiRequest<DataSendDTO> apiRequest = new ApiRequest<DataSendDTO>() 
        { ContentType = "application/json" ,
           Method=RestSharp.Method.Post,
           Route= "/data/data",
           Parsmeters=dataSendDTO
        };
        var result= await _client.SendData(apiRequest);
    }
}