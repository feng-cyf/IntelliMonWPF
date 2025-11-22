using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace IntelliMonWPF.Event.EventBus
{
    using IntelliMonWPF.Models;
    using System.Collections.ObjectModel;

    // 1. 泛型数据容器（支持属性变更通知）
    public class Data<T> : BindableBase
    {
        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { _Name = value;
                RaisePropertyChanged();
            }
        }

        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value)) return;
                _value = value;
                RaisePropertyChanged();
            }
        }
        private bool _IsValid=true;

        public bool IsValid
        {
            get { return _IsValid; }
            set { _IsValid = value; }
        }

    }
    public class DataBus
    {
        // 单例实例
        private static readonly Lazy<DataBus> _instance = new Lazy<DataBus>(() => new DataBus());
        public static DataBus Instance => _instance.Value;

        private DataBus() { }
        private readonly ConcurrentDictionary<string,int> StatusIndex= new ConcurrentDictionary<string, int>();
        private readonly ConcurrentDictionary<string,int> SingalIntListIndex = new ConcurrentDictionary<string, int>();
        private readonly ConcurrentDictionary<string, int> SingalFloatDictIndex = new ConcurrentDictionary<string, int>();
        public ConcurrentDictionary<string, int> FloatsListIndex { get; } = new ConcurrentDictionary<string, int>();
        public ObservableCollection<Data<bool[]>> StatusList { get; } = new ObservableCollection<Data<bool[]>>();
        public ObservableCollection<Data<int>> SingalIntList { get; } = new ObservableCollection<Data<int>>();
        public ObservableCollection<Data<float>> SingalFloatList { get; } = new ObservableCollection<Data<float>>();
        public ObservableCollection<Data<decimal[]>> FloatsList { get; } = new ObservableCollection<Data<decimal[]>>();
        private void UpdateData<T>(ObservableCollection<Data<T>> dataList, ConcurrentDictionary<string, int> indexDict, string key, T value)
        {
            if (indexDict.TryGetValue(key, out int index))
            {
                App.Current.Dispatcher.Invoke(() =>
                    {
                        lock (dataList)
                            dataList[index].Value = value;
                    });
            }
            else
            {
                var dataItem = new Data<T> { Name = key, Value = value };
                lock (dataList)
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        dataList.Add(dataItem);
                    });
                indexDict.TryAdd(key, dataList.Count - 1);
            }
        }
        public void AddData(ReadModel rm, object data)
        {
            if (rm == null || data == null) return;

            string key = rm.Name.Value;
            if (string.IsNullOrEmpty(key)) return;

            switch (data)
            {
                case bool[] boolArray:
                    UpdateData(StatusList, StatusIndex, key, boolArray);
                    break;
                case int intValue:
                  UpdateData(SingalIntList, SingalIntListIndex, key, intValue);
                    break;

                case float floatValue:
                 UpdateData(SingalFloatList, SingalFloatDictIndex, key, floatValue);
                    break;

                case decimal[] decimalArray:
                  UpdateData(FloatsList, FloatsListIndex, key, decimalArray);
                    break;

                default:
                    Console.WriteLine($"不支持的数据类型：{data.GetType()}");
                    break;
            }
        }
        private void RemoveData<T>(ObservableCollection<Data<T>> list,ConcurrentDictionary<string,int> dict,string key)
        {
            if (dict.TryGetValue(key, out int index))
            {
                lock(list)
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        list[index].IsValid = false;
                    });
            }
        }
        //清除旧键（自动触发通知）
        public void ClearOldKey(IEnumerable<string> oldKey)
        {
            if (oldKey == null) return;
            var oldKeySet = new HashSet<string>(oldKey);

            foreach (var key in StatusIndex.Keys.ToList())
                if (!oldKeySet.Contains(key))
                    RemoveData(StatusList, StatusIndex, key);

            foreach (var key in SingalIntListIndex.Keys.ToList())
                if (!oldKeySet.Contains(key))
                    RemoveData(SingalIntList, SingalIntListIndex, key);

            foreach (var key in SingalFloatDictIndex.Keys.ToList())
                if (!oldKeySet.Contains(key))
                    RemoveData(SingalFloatList, SingalFloatDictIndex, key);

            foreach (var key in FloatsListIndex.Keys.ToList())
                if (!oldKeySet.Contains(key))
                    RemoveData(FloatsList, FloatsListIndex, key);
        }
    }
}