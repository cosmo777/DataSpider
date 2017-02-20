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
        public ObservableCollection<SpiderResult> Matches { get; set; }

        public string SelectedProcessName { get; set; }


        public MainWindow()
        {
            Matches = new ObservableCollection<SpiderResult>();
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
                if (openFileDialog.ShowDialog().GetValueOrDefault())
                {
                    var spiderSearch = SerializationTool.DeserializeJsonFile<SpiderSearch>(openFileDialog.FileName);
                    var tabItem = new TabItem();
                    tabItem.Header = spiderSearch.Name;
                    tabItem.Content = new SpiderControl(this, tabItem, spiderSearch);
                    tabSearches.Items.Add(tabItem);
                    tabSearches.SelectedItem = tabItem;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void Button_FindMatches_Click(object sender, RoutedEventArgs e)
        {
            Matches.Clear();
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
            if (searches.Count == 0)
            {
                return;
            }
            if (searches.Count == 1)
            {
                var firstResult = searches.First();
                foreach (var result in firstResult.Results)
                {
                    Matches.Add(result);
                }
                return;
            }
            var candidates = searches.First().Results;
            for (var index = 1; index < searches.Count; index++)
            {
                var spiderSearch = searches[index];
                var candidatesToRemove = new List<SpiderResult>();
                foreach (var candidate in candidates)
                {
                    if (!spiderSearch.Results.Any(o => o.Level == candidate.Level && o.Offset0 == candidate.Offset0 && o.Offset1 == candidate.Offset1
                     && o.Offset2 == candidate.Offset2 && o.Offset3 == candidate.Offset3 && o.Offset4 == candidate.Offset4 && o.Offset5 == candidate.Offset5
                      && o.Offset6 == candidate.Offset6 && o.Offset7 == candidate.Offset7 && o.Offset8 == candidate.Offset8 && o.Offset9 == candidate.Offset9
                      && o.Offset10 == candidate.Offset10 && o.Value == spiderSearch.StringValue))
                    {
                        candidatesToRemove.Add(candidate);
                    }
                }
                foreach (var candidateToRemove in candidatesToRemove)
                {
                    candidates.Remove(candidateToRemove);
                }
            }
            foreach (var spiderResult in candidates)
            {
                Matches.Add(spiderResult);
            }
        }
    }


}
