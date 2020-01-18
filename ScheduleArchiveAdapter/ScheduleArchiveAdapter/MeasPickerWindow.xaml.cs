using AdapterUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScheduleArchiveAdapter
{
    /// <summary>
    /// Interaction logic for MeasPickerWindow.xaml
    /// </summary>
    public partial class MeasPickerWindow : Window
    {
        public ConfigurationManager Config_ { get; set; } = new ConfigurationManager();
        public DataFetcher DataFetcher_ { get; set; }
        public MeasPickerWindow()
        {
            InitializeComponent();
            Config_.Initialize();
            DataFetcher_ = new DataFetcher() { Config_ = Config_ };
            PopulateData();
        }

        private async Task PopulateData()
        {
            await PopulateScheduleTypes();
            await LoadMeasurements();
        }
        private async Task PopulateScheduleTypes()
        {
            List<SchType> schTypeItems = await DataFetcher_.FetchSchTypes();
            ScheduleTypesComboBox.ItemsSource = schTypeItems;
            ScheduleTypesComboBox.DisplayMemberPath = "Label";
            ScheduleTypesComboBox.SelectedValuePath = "Val";
            ScheduleTypesComboBox.SelectedIndex = 0;
        }

        private async Task LoadMeasurements()
        {
            List<string> IsgsGenNames = await DataFetcher_.FetchUtilsList();
            UtilNamesComboBox.ItemsSource = IsgsGenNames;
            UtilNamesComboBox.SelectedIndex = 0;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ShutdownApp();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            //todo console the selected measurement
            string selectedUtil = UtilNamesComboBox.SelectedItem.ToString();
            string selectedSchType = ScheduleTypesComboBox.SelectedItem.GetType().GetProperty("Val")?.GetValue(ScheduleTypesComboBox.SelectedItem, null).ToString();
            string resultText = $"{selectedUtil}|{selectedSchType}";
            ConsoleUtils.FlushMeasData(resultText, resultText, resultText);
            ShutdownApp();
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            RefreshMeasurements();
        }

        private void RefreshMeasurements()
        {
            LoadMeasurements();
        }

        private void ShutdownApp()
        {
            Application.Current.Shutdown();
        }
    }
}
