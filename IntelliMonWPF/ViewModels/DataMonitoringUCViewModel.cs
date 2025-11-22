using IntelliMonWPF.Event.EventBus;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class DataMonitoringUCViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly DataBus _dataBus = DataBus.Instance;

    public ObservableCollection<Data<int>> IntRegisters { get; }
    public ObservableCollection<Data<float>> FloatRegisters { get; }

    public DataMonitoringUCViewModel()
    {
        IntRegisters = _dataBus.SingalIntList;
        FloatRegisters = _dataBus.SingalFloatList;

        AddTestData();
    }

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

    public void Dispose()
    {
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}