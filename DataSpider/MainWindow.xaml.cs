using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace DataSpider
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> Processes { get; set; }

        public string SelectedProcessName { get; set; }

        public ObservableCollection<SpiderResult> Results { get; set; }

        public List<DataType> DataTypes { get; set; }

        public MainWindow()
        {
            DataTypes = new List<DataType>();
            DataTypes.Add(DataType.Byte);
            DataTypes.Add(DataType.Int);
            DataTypes.Add(DataType.Long);
            DataTypes.Add(DataType.Float);
            DataTypes.Add(DataType.UInt);
            DataTypes.Add(DataType.UTF16);
            Results = new ObservableCollection<SpiderResult>();
            Processes = new ObservableCollection<string>();
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                Processes.Add(process.ProcessName);
            }
            InitializeComponent();
        }

        private void TextBox_ProcessFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            Processes.Clear();
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (process.ProcessName.ToLower().Contains(TextBox_ProcessFilter.Text.ToLower()))
                {
                    Processes.Add(process.ProcessName);
                }
            }
        }

        private void Button_Seach_Click(object sender, RoutedEventArgs e)
        {
            var processes = Process.GetProcessesByName(SelectedProcessName);
            if (!processes.Any())
            {
                MessageBox.Show("Select a process");
                return;
            }
            var process = processes.First();
            var memory = new Memory(process.Id);
            if (!int.TryParse(TextBox_Levels.Text, out int levels))
            {
                MessageBox.Show(this, "Can't parse levels.");
                return;
            }
            if (!int.TryParse(TextBox_Variance.Text, out int variance))
            {
                MessageBox.Show(this, "Can't parse variance.");
                return;
            }
            if (!int.TryParse(TextBox_Length.Text, out int length))
            {
                MessageBox.Show(this, "Can't parse length.");
                return;
            }
            if (levels > 10)
            {
                MessageBox.Show(this, "Only 10 levels deep supported.");
                return;
            }
            var size = (DataType)ComboBox_Size.SelectedValue;

            long longValue = 0;
            float floatValue = 0;
            switch (size)
            {
                case DataType.UTF16:

                    break;
                case DataType.Float:
                    if (!float.TryParse(TextBox_Value.Text, out floatValue))
                    {
                        MessageBox.Show(this, "Can't parse value as float.");
                        return;
                    }
                    break;
                default:
                    if (!long.TryParse(TextBox_Value.Text, out longValue))
                    {
                        MessageBox.Show(this, "Can't parse value as non float.");
                        return;
                    }
                    break;
            }
            if (!int.TryParse(TextBox_Alignment.Text, out int alignment))
            {
                MessageBox.Show(this, "Can't parse alignment.");
                return;
            }
            if (alignment <= 0)
            {
                alignment = 1;
            }

            long address;
            if (!long.TryParse(TextBox_Address.Text, out address))
            {
                MessageBox.Show(this, "Can't parse address.");
                return;
            }
            var searchRequest = new SpiderSearchRequest
            {
                DataType = size,
                AddressIncrement = alignment,
                StringValue = TextBox_Value.Text,
                FloatValue = floatValue,
                Length = length,
                Levels = levels,
                LongValue = longValue,
                Variance = variance,
                Is64Bit = CheckBox_Is64Bit.IsChecked.GetValueOrDefault(),
                Address = address,
                Match = CheckBox_Match.IsChecked.GetValueOrDefault()
            };
            if (int.TryParse(TextBox_StringLength.Text, out int stringLength))
            {
                searchRequest.StringLength = stringLength;
            }
            else
            {
                switch (searchRequest.DataType)
                {
                    case DataType.UTF16:
                        searchRequest.StringLength = searchRequest.StringValue.Length;
                        break;
                }
            }
            if (searchRequest.StringLength > 4)
            {
                searchRequest.ExtraRead = searchRequest.StringLength * 2;
            }
            else
            {
                searchRequest.ExtraRead = 8;
            }
            searchRequest.EndIncrement = length / searchRequest.AddressIncrement;

            var allData = memory.ReadBytes(searchRequest.Address, searchRequest.Length + searchRequest.ExtraRead);
            var results = new ConcurrentBag<SpiderResult>();
            Parallel.For(0, searchRequest.EndIncrement, index =>
            {
                var addressToCheck = searchRequest.Address + index * searchRequest.AddressIncrement;
                var indexToCheck = index * searchRequest.AddressIncrement;
                var currentResult = new SpiderResult() { Level = 0 };
                int currentLevel = 0;
                currentResult.Offset0 = index * searchRequest.AddressIncrement;
                currentResult.Address = addressToCheck;
                var match = CheckForMatchValue(allData, indexToCheck, searchRequest, memory);
                if (match != null)
                {
                    AddResult(currentLevel, currentResult, searchRequest, results, match, index, addressToCheck);
                }
                if (!(currentLevel >= searchRequest.Levels))
                {
                    var nextLevel = currentLevel + 1;
                    if (searchRequest.Is64Bit)
                    {
                        var pointer = BitConverter.ToInt64(allData, indexToCheck);
                        if (pointer > 50000)
                        {
                            CheckNextLevel(nextLevel, pointer, currentResult, searchRequest, results, memory);
                        }
                    }
                    else
                    {
                        var pointer = BitConverter.ToInt32(allData, indexToCheck);
                        if (pointer > 50000)
                        {
                            CheckNextLevel(nextLevel, pointer, currentResult, searchRequest, results, memory);
                        }
                    }
                }
            });

            Results.Clear();
            foreach (var spiderResult in results)
            {
                Results.Add(spiderResult);
            }
            memory.Close();

        }

        private string CheckForMatchValue(byte[] data, int index, SpiderSearchRequest request, Memory memory)
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

        private void CheckNextLevel(int level, long startAddress, SpiderResult parentResult, SpiderSearchRequest searchRequest, ConcurrentBag<SpiderResult> results, Memory memory)
        {
            var allData = memory.ReadBytes(startAddress, searchRequest.Length + searchRequest.ExtraRead);
            for (int index = 0; index < searchRequest.EndIncrement; index++)
            {
                var addressToCheck = searchRequest.Address + index * searchRequest.AddressIncrement;
                var indexToCheck = index * searchRequest.AddressIncrement;
                var match = CheckForMatchValue(allData, indexToCheck, searchRequest, memory);
                if (match != null)
                {
                    AddResult(level, parentResult, searchRequest, results, match, index, addressToCheck);
                }
                if (!(level >= searchRequest.Levels))
                {
                    var nextResult = new SpiderResult(parentResult);
                    nextResult.SetOffset(level, index * searchRequest.AddressIncrement);
                    var nextLevel = level + 1;
                    if (searchRequest.Is64Bit)
                    {
                        var pointer = BitConverter.ToInt64(allData, indexToCheck);
                        if (pointer > 50000)
                        {
                            CheckNextLevel(nextLevel, pointer, nextResult, searchRequest, results, memory);
                        }
                    }
                    else
                    {
                        var pointer = BitConverter.ToInt32(allData, indexToCheck);
                        if (pointer > 50000)
                        {
                            CheckNextLevel(nextLevel, pointer, nextResult, searchRequest, results, memory);
                        }
                    }
                }
            }
        }

        private static void AddResult(int level, SpiderResult parentResult, SpiderSearchRequest searchRequest, ConcurrentBag<SpiderResult> results, string match, int iterator, long addressFoundAt)
        {
            if (match != null)
            {
                var newResult = new SpiderResult(parentResult);
                newResult.Level = level;
                var offset = iterator * searchRequest.AddressIncrement;
                newResult.Address = addressFoundAt;
                newResult.SetOffset(level, offset);
                newResult.Value = match;
                if (long.TryParse(match, out long matchLong))
                {
                    newResult.HexValue = matchLong.ToString("X");
                }
                results.Add(newResult);
            }
        }

        private void TextBox_Hex_HexValue_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (long.TryParse(TextBox_Hex_HexValue.Text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out long val))
            {
                TextBox_Hex_IntValue.Text = val.ToString();
            }
        }

        private void TextBox_Int_IntValue_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (Int64.TryParse(TextBox_Int_IntValue.Text, out long val))
            {
                TextBox_Int_HexValue.Text = val.ToString("x");
            }
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog().GetValueOrDefault())
            {
                SerializationTool.SerializeJsonFile(saveFileDialog.FileName, Results);
            }
        }

        private void Button_Load_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog().GetValueOrDefault())
            {
                var results = SerializationTool.DeserializeJsonFile<ObservableCollection<SpiderResult>>(openFileDialog.FileName);
                Results.Clear();
                foreach (var spiderResult in results)
                {
                    Results.Add(spiderResult);
                }
            }
        }

        private void Button_ReloadValues_Click(object sender, RoutedEventArgs e)
        {
            foreach (var result in Results)
            {

            }
        }
    }


}
