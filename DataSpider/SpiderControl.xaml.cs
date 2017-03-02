using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DataSpider.Annotations;
using DataSpider.Properties;
using DataSpider.SearchTools;
using Microsoft.Win32;

namespace DataSpider
{
    /// <summary>
    /// Interaction logic for SpiderControl.xaml
    /// </summary>
    public partial class SpiderControl : UserControl, INotifyPropertyChanged
    {
        private readonly MainWindow _mainWindow;
        private readonly TabItem _tabItem;
        private SpiderSearch _spiderSearch;
        private List<SpiderResult> _visibleResults;

        public List<SpiderResult> VisibleResults
        {
            get { return _visibleResults; }
            set
            {
                if (Equals(value, _visibleResults)) return;
                _visibleResults = value;
                OnPropertyChanged();
            }
        }

        public SpiderSearch SpiderSearch
        {
            get { return _spiderSearch; }
            set
            {
                if (Equals(value, _spiderSearch)) return;
                _spiderSearch = value;
                OnPropertyChanged();
            }
        }

        public List<DataType> DataTypes { get; set; }
        public List<int> Alignments { get; set; }

        public SpiderControl(MainWindow mainWindow, TabItem tabItem, SpiderSearch search) : this(mainWindow, tabItem)
        {
            SpiderSearch = search;
            VisibleResults = SpiderSearch.Results.Take(1000).ToList();
        }

        public SpiderControl(MainWindow mainWindow, TabItem tabItem)
        {
            if (SpiderSearch == null)
            {
                SpiderSearch = new SpiderSearch()
                {
                    Aligned = false,
                    Is64Bit = true,
                    DataType = DataType.UTF16,
                    Length = 2096,
                    Levels = 2,
                    Match = true,
                    Name = "New Tab",
                    Variance = 0,
                    StringLength = 200,
                    Alignment = 2
                };
            }
            _mainWindow = mainWindow;
            _tabItem = tabItem;
            Alignments = new List<int>();
            Alignments.Add(1);
            Alignments.Add(2);
            Alignments.Add(4);
            Alignments.Add(8);
            DataTypes = new List<DataType>();
            DataTypes.Add(DataType.Byte);
            DataTypes.Add(DataType.Int);
            DataTypes.Add(DataType.Long);
            DataTypes.Add(DataType.Float);
            DataTypes.Add(DataType.UInt);
            DataTypes.Add(DataType.UTF16);

            InitializeComponent();
        }

        public void Button_Seach_Click(object sender, RoutedEventArgs e)
        {
            var processes = Process.GetProcessesByName(_mainWindow.SelectedProcessName);
            if (!processes.Any())
            {
                MessageBox.Show("Select a process");
                return;
            }
            var process = processes.First();
            var memory = new Memory(process.Id);
            try
            {
                if (SpiderSearch.Levels > 10)
                {
                    MessageBox.Show(_mainWindow, "Only 10 levels deep supported.");
                    return;
                }
                switch (SpiderSearch.DataType)
                {
                    case DataType.UTF16:
                        if (SpiderSearch.StringLength <= 0)
                        {
                            SpiderSearch.StringLength = SpiderSearch.StringValue.Length * 2;
                        }
                        break;
                    case DataType.Float:
                        float floatValue;
                        if (!float.TryParse(TextBox_Value.Text, out floatValue))
                        {
                            MessageBox.Show(_mainWindow, "Can't parse value as float.");
                            return;
                        }
                        SpiderSearch.FloatValue = floatValue;
                        break;
                    default:
                        long longValue;
                        if (!long.TryParse(TextBox_Value.Text, out longValue))
                        {
                            MessageBox.Show(_mainWindow, "Can't parse value as non float.");
                            return;
                        }
                        SpiderSearch.LongValue = longValue;
                        break;
                }
                if (SpiderSearch.StringLength > 8)
                {
                    SpiderSearch.ExtraRead = SpiderSearch.StringLength;
                }
                else
                {
                    SpiderSearch.ExtraRead = 8;
                }
                SpiderSearch.EndIncrement = SpiderSearch.Length / SpiderSearch.Alignment;

                var allData = memory.ReadBytes(SpiderSearch.Address, SpiderSearch.Length + SpiderSearch.ExtraRead);
                var results = new ConcurrentBag<SpiderResult>();
                Parallel.For(0, SpiderSearch.EndIncrement, index =>
                {
                    var offset = index * SpiderSearch.Alignment;
                    var addressToCheck = SpiderSearch.Address + offset;
                    int currentLevel = 0;
                    var currentResult = new SpiderResult() { Level = currentLevel };
                    currentResult.Offset0 = offset;
                    //currentResult.Address0 = addressToCheck;
                    var match = CheckForMatchValue(allData, offset, SpiderSearch);
                    if (match != null)
                    {
                        AddResult(currentLevel, currentResult, results, match, offset, addressToCheck);
                    }
                    if (!(currentLevel >= SpiderSearch.Levels))
                    {
                        var nextLevel = currentLevel + 1;
                        if (SpiderSearch.Is64Bit)
                        {
                            var pointer = BitConverter.ToInt64(allData, offset);
                            if (pointer > 50000)
                            {
                                CheckNextLevel(nextLevel, pointer, currentResult, SpiderSearch, results, memory);
                            }
                        }
                        else
                        {
                            var pointer = BitConverter.ToInt32(allData, offset);
                            if (pointer > 50000)
                            {
                                CheckNextLevel(nextLevel, pointer, currentResult, SpiderSearch, results, memory);
                            }
                        }
                    }
                });

                SpiderSearch.Results.Clear();
                foreach (var spiderResult in results)
                {
                    SpiderSearch.Results.Add(spiderResult);
                }
                VisibleResults = SpiderSearch.Results.Take(200).ToList();
            }
            finally
            {
                memory.Close();
            }
        }

        private void CheckNextLevel(int level, long startAddress, SpiderResult parentResult, SpiderSearch search, ConcurrentBag<SpiderResult> results, Memory memory)
        {
            var allData = memory.ReadBytes(startAddress, search.Length + search.ExtraRead);
            for (int index = 0; index < search.EndIncrement; index++)
            {
                var offset = index * search.Alignment;
                var addressToCheck = startAddress + offset;
                var match = CheckForMatchValue(allData, offset, search);
                if (match != null)
                {
                    AddResult(level, parentResult, results, match, offset, addressToCheck);
                }
                if (!(level >= search.Levels))
                {
                    var nextResult = new SpiderResult(parentResult);
                    nextResult.SetOffset(level, index * search.Alignment, addressToCheck);
                    var nextLevel = level + 1;
                    if (search.Is64Bit)
                    {
                        var pointer = BitConverter.ToInt64(allData, offset);
                        if (pointer > 50000)
                        {
                            CheckNextLevel(nextLevel, pointer, nextResult, search, results, memory);
                        }
                    }
                    else
                    {
                        var pointer = BitConverter.ToInt32(allData, offset);
                        if (pointer > 50000)
                        {
                            CheckNextLevel(nextLevel, pointer, nextResult, search, results, memory);
                        }
                    }
                }
            }
        }

        private string CheckForMatchValue(byte[] data, int index, SpiderSearch request)
        {
            switch (request.DataType)
            {
                case DataType.UTF16:
                    string stringValue = Encoding.Unicode.GetString(data, index, request.StringLength);
                    int num = stringValue.IndexOf('\0');
                    stringValue = num > 0 ? stringValue.Substring(0, num) : stringValue;
                    if (String.IsNullOrWhiteSpace(stringValue))
                    {
                        return null;
                    }
                    bool match = stringValue.StartsWith(request.StringValue);
                    if (match || !request.Match)
                    {
                        return stringValue;
                    }
                    break;
                case DataType.Byte:
                    var byteValue = data[index];
                    if (byteValue == (byte)request.LongValue || !request.Match)
                    {
                        return byteValue.ToString();
                    }
                    break;
                case DataType.Int:
                    var intValue = BitConverter.ToInt32(data, index);
                    if (intValue == (int)request.LongValue || !request.Match)
                    {
                        return intValue.ToString();
                    }
                    break;
                case DataType.Long:
                    var longValue = BitConverter.ToInt64(data, index);
                    if (longValue == request.LongValue || !request.Match)
                    {
                        return longValue.ToString();
                    }
                    break;
                case DataType.Float:
                    var floatValue = BitConverter.ToSingle(data, index);
                    if (Math.Abs(floatValue - request.FloatValue) < .001 || !request.Match)
                    {
                        return floatValue.ToString(CultureInfo.InvariantCulture);
                    }
                    break;
                case DataType.UInt:
                    var uIntValue = BitConverter.ToUInt32(data, index);
                    if (uIntValue == (uint)request.LongValue || !request.Match)
                    {
                        return uIntValue.ToString();
                    }
                    break;
            }
            return null;
        }

        private static void AddResult(int level, SpiderResult parentResult, ConcurrentBag<SpiderResult> results, string match, int offset, long addressFoundAt)
        {
            if (match != null)
            {
                var newResult = new SpiderResult(parentResult);
                newResult.Level = level;
                newResult.SetOffset(level, offset, addressFoundAt);
                newResult.Value = match;
                results.Add(newResult);
            }
        }
        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog().GetValueOrDefault())
                {
                    SerializationTool.SerializeJsonFile(saveFileDialog.FileName, SpiderSearch);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_Load_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog().GetValueOrDefault())
                {
                    SpiderSearch = SerializationTool.DeserializeJsonFile<SpiderSearch>(openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Button_ReloadValues_Click(object sender, RoutedEventArgs e)
        {
            var processes = Process.GetProcessesByName(_mainWindow.SelectedProcessName);
            if (!processes.Any())
            {
                MessageBox.Show("Select a process");
                return;
            }
            var process = processes.First();
            var memory = new Memory(process.Id);
            try
            {
                SpiderSearch.Address = long.Parse(TextBox_Address.Text);
                SpiderSearch.Reload(memory);
                foreach (var visibleResult in VisibleResults)
                {
                    visibleResult.CallPropertyChanged("Current");
                    visibleResult.CallPropertyChanged("Value");
                }
            }
            finally
            {
                memory.Close();
            }
        }

        public void Button_ReFilterValues_Click(object sender, RoutedEventArgs e)
        {
            var processes = Process.GetProcessesByName(_mainWindow.SelectedProcessName);
            if (!processes.Any())
            {
                MessageBox.Show("Select a process");
                return;
            }
            var process = processes.First();
            var memory = new Memory(process.Id);
            try
            {
                SpiderSearch.Address = long.Parse(TextBox_Address.Text);
                SpiderSearch.Reload(memory);

            }
            finally
            {
                memory.Close();
            }
            var results = SpiderSearch.Results.Where(o => TextBox_Value.Text != o.Current).ToList();
            var dict = SpiderSearch.Results.ToDictionary(o => o.Id, o => o);
            foreach (var spiderResult in results)
            {
                dict.Remove(spiderResult.Id);
            }
            SpiderSearch.Results = dict.Values.ToList();
            VisibleResults = SpiderSearch.Results.Take(200).ToList();
        }

        private void TextBox_TabName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _tabItem.Header = TextBox_TabName.Text;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }


}
