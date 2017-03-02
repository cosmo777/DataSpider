using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using DataSpider.Annotations;
using DataSpider.SearchTools;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace DataSpider
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private SpiderMatcher _spiderMatcher;
        private SpiderMatch _selectedMatch;
        private List<SpiderMatch> _matches;

        public List<SpiderMatch> Matches
        {
            get { return _matches; }
            set
            {
                if (Equals(value, _matches)) return;
                _matches = value;
                OnPropertyChanged();
            }
        }

        public SpiderMatcher SpiderMatcher
        {
            get { return _spiderMatcher; }
            set
            {
                if (Equals(value, _spiderMatcher)) return;
                _spiderMatcher = value;
                OnPropertyChanged();
            }
        }

        public SpiderMatch SelectedMatch
        {
            get { return _selectedMatch; }
            set
            {
                if (Equals(value, _selectedMatch)) return;
                _selectedMatch = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Processes { get; set; }

        public string SelectedProcessName { get; set; }


        public MainWindow()
        {
            Matches = new List<SpiderMatch>();
            SpiderMatcher = new SpiderMatcher();
            Processes = new ObservableCollection<string>();
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                Processes.Add(process.ProcessName);
            }
            InitializeComponent();
            if (Processes.Count > 0)
            {
                ComboBox_Processes.SelectedIndex = 0;
            }

            var task = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(1);
                    if (InputSpan.GetKeys(Keys.F2))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var content = tabSearches.SelectedContent as SpiderControl;
                            if (content != null)
                            {
                                content.Button_ReloadValues_Click(null, null);
                                Thread.Sleep(1000);
                            }
                        });
                    }
                    if (InputSpan.GetKeys(Keys.F3))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var content = tabSearches.SelectedContent as SpiderControl;
                            if (content != null)
                            {
                                content.Button_ReFilterValues_Click(null, null);
                                Thread.Sleep(1000);
                            }
                        });
                    }
                    if (InputSpan.GetKeys(Keys.F4))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var content = tabSearches.SelectedContent as SpiderControl;
                            if (content != null)
                            {
                                content.Button_Seach_Click(null, null);
                                Thread.Sleep(1000);
                            }
                        });
                    }
                }
            });
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

        public void AddTab()
        {
            var tabItem = new TabItem();
            tabItem.Header = "";
            tabItem.Content = new SpiderControl(this, tabItem);
            tabSearches.Items.Add(tabItem);
            tabSearches.SelectedItem = tabItem;
        }

        private void TextBox_Hex_HexValue_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            long val;
            if (long.TryParse(TextBox_Hex_HexValue.Text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out val))
            {
                TextBox_Hex_IntValue.Text = val.ToString();
            }
        }

        private void TextBox_Int_IntValue_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            long val;
            if (Int64.TryParse(TextBox_Int_IntValue.Text, out val))
            {
                TextBox_Int_HexValue.Text = val.ToString("x");
            }
        }

        private void Button_AddTab_Click(object sender, RoutedEventArgs e)
        {
            AddTab();
        }

        private void Button_RemoveTab_Click(object sender, RoutedEventArgs e)
        {
            tabSearches.Items.Remove(tabSearches.SelectedItem);
        }

        private void Button_LoadTab_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = true;
                if (openFileDialog.ShowDialog().GetValueOrDefault())
                {
                    foreach (var fileName in openFileDialog.FileNames)
                    {
                        var spiderSearch = SerializationTool.DeserializeJsonFile<SpiderSearch>(fileName);
                        var tabItem = new TabItem();
                        tabItem.Header = spiderSearch.Name;
                        tabItem.Content = new SpiderControl(this, tabItem, spiderSearch);
                        tabSearches.Items.Add(tabItem);
                        tabSearches.SelectedItem = tabItem;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }


        private void Button_ReloadValues_Click(object sender, RoutedEventArgs e)
        {
            var processes = Process.GetProcessesByName(SelectedProcessName);
            if (!processes.Any())
            {
                MessageBox.Show("Select a process");
                return;
            }
            var process = processes.First();
            var memory = new Memory(process.Id);
            try
            {
                if (SpiderMatcher != null)
                {
                    SpiderMatcher.ReloadValues(memory);
                }
            }
            finally
            {
                memory.Close();
            }
        }


        private void Button_FindMatches_Click(object sender, RoutedEventArgs e)
        {
            var processes = Process.GetProcessesByName(SelectedProcessName);
            if (!processes.Any())
            {
                MessageBox.Show("Select a process");
                return;
            }
            var process = processes.First();
            var memory = new Memory(process.Id);
            SpiderMatcher = new SpiderMatcher();
            try
            {
                var searches = new List<SpiderSearch>();
                var tabs = tabSearches.Items;
                foreach (var tab in tabs)
                {
                    var tabItem = tab as TabItem;
                    if (tabItem != null && tabItem.Content is SpiderControl)
                    {
                        var spiderControl = (SpiderControl)tabItem.Content;
                        searches.Add(spiderControl.SpiderSearch);
                    }
                }
                SpiderMatcher.Match(searches, memory);
                Matches = SpiderMatcher.Matches.Take(1000).ToList();
            }
            finally
            {
                memory.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public static class InputSpan
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int UnhookWindowsHookEx(int idHook);
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(UInt16 virtualKeyCode);
        public static bool GetKeys(Keys keyData)
        {
            var val = GetAsyncKeyState((ushort)keyData);
            return val < 0;
        }

        public static bool GetMouse(MouseButton mouseButton)
        {
            var val = GetAsyncKeyState((ushort)mouseButton);
            return val < 0;
        }

        public enum MouseButton
        {
            VK_MBUTTON = 0x04,//middle mouse button
            VK_LBUTTON = 0x01,//left mouse button
            VK_RBUTTON = 0x02//right mouse button
        }
    }


}
