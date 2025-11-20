using IntelliMonWPF.Event.EventBus;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class DataMonitoringUCViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly DataBus _dataBus = DataBus.Instance;

    // 1. 直接使用DataBus的ObservableCollection列表（无需KeyValuePair）
    public ObservableCollection<Data<int>> IntRegisters { get; }
    public ObservableCollection<Data<float>> FloatRegisters { get; }

    public DataMonitoringUCViewModel()
    {
        // 2. 直接绑定DataBus的列表（Prism注入后自动关联，列表变更UI实时更新）
        IntRegisters = _dataBus.SingalIntList;
        FloatRegisters = _dataBus.SingalFloatList;

        // 保留测试数据（直接加进DataBus列表，UI会自动刷新）
        AddTestData();
    }

    // 测试数据：直接添加到DataBus的列表中
    private void AddTestData()
    {
        IntRegisters.Add(new Data<int>
        {
            Name = "测试Int寄存器",
            Value = 123,
        });
        FloatRegisters.Add(new Data<float>
        {
            Name = "测试Float寄存器",
            Value = 3.14f,
        });
    }

    // 原有Dispose、PropertyChanged逻辑不变...
    public void Dispose()
    {
        // 若DataBus列表后续加了事件订阅，这里清理；当前直接绑定无需额外解绑
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}