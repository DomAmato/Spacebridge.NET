using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Spacebridge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Dictionary<string, JsonElement> orgDict;
        int selectedOrg = 0; 
        string apiKey; 

        public MainWindow()
        {
            InitializeComponent();
        }

        private async Task loadOrgs()
        {
            Task<Dictionary<string, JsonElement>> response = API.getUserInfoAsync();
            Dictionary<string, JsonElement> jsonResponse = await response;
            response = API.getOrganizationsAsync(jsonResponse["data"].GetProperty("id").GetInt32());
            jsonResponse = await response;
            orgDict = jsonResponse["data"].EnumerateArray().ToDictionary(org => org.GetProperty("name").GetString());
            OrgDropdown.ItemsSource = jsonResponse["data"].EnumerateArray()
                .Select(org => org.GetProperty("name")).ToArray();
        }

        private async Task loadDevices(int orgId)
        {
            Task<Dictionary<string, JsonElement>> response = API.getDevicesAsync(orgId);
            Dictionary<string, JsonElement> jsonResponse = await response;

            var deviceList = jsonResponse["data"];
            foreach (var device in deviceList.EnumerateArray())
            {
                System.Diagnostics.Debug.WriteLine(device.GetProperty("devicename"));
            }
            Dropdown1.ItemsSource = deviceList.EnumerateArray()
                .Select(device => device.GetProperty("devicename")).ToArray();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void Organizations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Selected SSID
            selectedOrg = orgDict[((JsonElement) e.AddedItems[0]).GetString()].GetProperty("id").GetInt32();
            _ = loadDevices(selectedOrg);
        }

        private void APIKey_PasswordChanged(object sender, RoutedEventArgs e)
        {
            apiKey = ((PasswordBox)e.Source).Password;
            API.setApiKey(apiKey);
            _ = loadOrgs();
        }

        private void Generate_Key(object sender, RoutedEventArgs e)
        {
            SSH.createRSAKey();
        }
    }
}
