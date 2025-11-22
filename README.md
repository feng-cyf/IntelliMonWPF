# IntelliMonWPF - 智能设备监控系统

## 项目概述

IntelliMonWPF是一个完整的工业设备监控系统，采用C# WPF前端和Python FastAPI后端的全栈架构，实现了基于Modbus协议的设备通信、数据采集、存储和分析功能。系统支持多种通信方式，包括Modbus RTU和TCP，可灵活配置设备参数和采集点表，并提供实时数据监控和历史数据分析能力。

**核心价值**：为工业环境提供高效、稳定的设备监控解决方案，支持多协议通信、实时数据采集和历史数据分析，助力工业自动化系统的可靠运行。

## 系统架构

系统采用前后端分离的架构设计，实现了选择性数据分析以及配置导入数据库
### 前端架构 (WPF应用)
- **框架**：C# WPF + Prism MVVM框架
- **通信层**：Modbus协议实现（RTU/ASCII/TCP）
- **核心模块**：设备管理、点表配置、数据监控、用户认证
- **设计模式**：MVVM、依赖注入、工厂模式、事件总线

### 后端架构 (FastAPI服务)
- **框架**：Python FastAPI
- **数据库**：关系型数据库 + Redis缓存
- **ORM**：Tortoise ORM
- **API模块**：用户认证、设备数据管理、数据分析、预测模型

## 核心功能

### 设备管理
- 支持Modbus RTU/TCP设备的添加、编辑和删除
- 自动端口检测和连接状态监控
- 设备参数配置和实时状态显示
- 批量设备数据管理和同步

### 点表配置
- 灵活的Modbus寄存器点表配置
- 支持多种数据类型和访问方式
- 点表批量导入导出功能
- 点位实时数据监控和更新

### 数据监控
- 实时数据采集和显示
- 历史数据查询和趋势分析
- 数据分析统计（最大值、最小值、平均值、标准差等）
- 数据异常监测和警报提示

### 用户管理
- 用户注册、登录和权限控制
- 员工职位和角色管理
- 安全的密码加密存储

### 数据存储与分析
- 结构化数据存储和查询
- Redis缓存优化数据访问性能
- 数据统计分析和预测功能
- 批量数据处理和计算

## 技术栈

### 前端
- **开发语言**：C#
- **框架**：WPF, Prism
- **通信**：SerialPort, Socket
- **UI组件**：WPF原生控件
- **依赖注入**：Prism.Unity
- **架构模式**：MVVM

### 后端
- **开发语言**：Python 3.13
- **Web框架**：FastAPI
- **数据库ORM**：Tortoise ORM
- **缓存**：Redis
- **认证**：bcrypt密码加密
- **数据分析**：NumPy
- **异步处理**：asyncio

## 系统流程图

### 数据采集流程
1. 前端通过Modbus协议采集设备数据
2. 数据经过处理后发送至后端API
3. 后端API存储数据至数据库并进行分析

### 用户认证流程
1. 用户提交登录凭证至前端
2. 前端调用后端认证API
3. 后端验证凭证并返回用户信息

## 快速开始

### 前置条件
- .NET Framework 4.7.2+ 或 .NET 5+
- Python 3.11+
- Redis 6.0+
- 数据库服务（MySQL）

### 后端部署
```bash
# 进入后端目录
cd fastapi_save

# 安装依赖
pip install -r requirements.txt

# 启动服务
python -m orm.main
```

### 前端运行
```bash
# 进入前端目录
cd IntelliMonWPF

# 使用Visual Studio打开解决方案
# 编译并运行
```

### 配置说明
- 后端配置文件位于`fastapi_save/orm/dict.py`
- 数据库连接配置可在其中修改
- Redis连接默认配置为localhost:6379



## 核心代码示例

### Modbus通信示例
```csharp
// Modbus RTU通信基础实现
public class ModbuslBase : INotifyPropertyChanged
{
    // 可用串口列表
    private ObservableCollection<string> _availablePorts;
    public ObservableCollection<string> AvailablePorts
    {
        get => _availablePorts;
        set => SetProperty(ref _availablePorts, value);
    }
    
    // 自动检测可用串口
    private async Task AutoDetectAvailablePorts()
    {
        var ports = SerialPort.GetPortNames();
        AvailablePorts = new ObservableCollection<string>(ports);
    }
    
    // 其他Modbus通信相关方法...
}
```

### 设备管理示例
```csharp
// 设备管理视图模型
public class DeviceManagementUCViewModel : BindableBase
{
    // 设备集合
    private ObservableCollection<DeviceModel> _devices;
    public ObservableCollection<DeviceModel> Devices
    {
        get => _devices;
        set => SetProperty(ref _devices, value);
    }
    
    // 添加设备命令
    public DelegateCommand AddDeviceCommand { get; private set; }
    
    // 构造函数和初始化
    public DeviceManagementUCViewModel(IEventAggregator eventAggregator)
    {
        Devices = new ObservableCollection<DeviceModel>();
        AddDeviceCommand = new DelegateCommand(OnAddDevice);
        // 其他初始化...
    }
}
```

### 后端API示例
```python
# 数据保存API
@app_data.post("/data", response_model=Request)
async def data(request: Data):
    """处理数据并返回计算结果"""
    if request.ushorts is None or len(request.ushorts) == 0:
        raise HTTPException(status_code=400, detail="传入结果为空")

    try:
        future = global_process_pool.submit(perform_calculations, request.ushorts)

        result = await asyncio.to_thread(future.result)

        return result

    except Exception as e:
        return {
            "code": 500,
            "message": f"服务器错误: {str(e)}",
            "Number": 0
        }
```


## 未来改进方向

### 近期计划
- 完善数据监控模块的实时图表展示功能
- 增强用户界面的响应速度和视觉体验

### 中期目标
- 实现完整的员工管理模块和权限系统
- 完善系统设置功能，支持更多可配置选项
- 增强预测模型的准确性和实时性
- 添加设备报警阈值设置和自动通知功能

## 开发背景

本项目为工业自动化监控领域的实践项目，旨在设计一个灵活、可靠的设备监控系统，支持工业环境中常见的Modbus协议设备通信和数据管理。

## 项目价值
- 提供统一的设备监控平台，降低工业自动化系统集成复杂度
- 通过实时数据采集和分析，提高设备运行效率和故障预警能力
- 采用模块化设计，便于后续功能扩展和维护
- 展示了全栈开发能力，包括前端WPF应用和后端FastAPI服务的开发
## 📸 系统截图

### 设备管理界面
<img width="1280" height="800" alt="fe9fa26d8c2866a5b3015dc596cbba6" src="https://github.com/user-attachments/assets/21828d9d-ccc1-4cc3-b382-aba8ec1dd12a" />


### 点表配置界面
<img width="1280" height="800" alt="1e05ae132291aea2b2de2af4e45ff07" src="https://github.com/user-attachments/assets/23da85ef-8d0f-41bd-86cd-90e3b02a7ba5" />


### 采集日志界面
<img width="1280" height="800" alt="4d22af611d5e6c53c41905b0223f7c0" src="https://github.com/user-attachments/assets/b850fcf3-4cec-466e-9d85-18b6c3e8f7bc" />

### 操作日志信息页面
<img width="1280" height="800" alt="7061e3337a8257a8f1eaaa074e069ca" src="https://github.com/user-attachments/assets/c7c72f74-dac0-4808-bd68-6e7e29f23d81" />


## 许可证

本项目仅供学习和参考使用。
