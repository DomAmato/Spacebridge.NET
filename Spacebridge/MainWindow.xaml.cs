using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace Spacebridge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Dictionary<string, JsonElement> orgDict;
        int selectedOrg = 0;
        int userId = 0;
        string apiKey;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void loadOrgs()
        {
            Task<Dictionary<string, JsonElement>> response = API.getOrganizationsAsync(userId);
            Dictionary<string, JsonElement> jsonResponse = await response;
            orgDict = jsonResponse["data"].EnumerateArray().ToDictionary(org => org.GetProperty("name").GetString());
            OrgDropdown.ItemsSource = jsonResponse["data"].EnumerateArray()
                .Select(org => org.GetProperty("name")).ToArray();
        }

        private async void loadDevices(int orgId)
        {
            Task<Dictionary<string, JsonElement>> response = API.getDevicesAsync(orgId);
            Dictionary<string, JsonElement> jsonResponse = await response;

            var deviceList = jsonResponse["data"];
            DeviceDropdown1.ItemsSource = deviceList.EnumerateArray()
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
            selectedOrg = orgDict[((JsonElement)e.AddedItems[0]).GetString()].GetProperty("id").GetInt32();
            loadDevices(selectedOrg);
            ScrollArea.Visibility = Visibility.Visible;
            DeviceLabel.Visibility = Visibility.Visible;
            RPortLabel.Visibility = Visibility.Visible;
            LPortLabel.Visibility = Visibility.Visible;
            DeviceDropdown1.Visibility = Visibility.Visible;
            Connect1.Visibility = Visibility.Visible;
            Local1.Visibility = Visibility.Visible;
            Remote1.Visibility = Visibility.Visible;
            Status1.Visibility = Visibility.Visible;
            AddDevice.Visibility = Visibility.Visible;
            CheckKeysExist();
        }

        private async void CheckKeysExist()
        {
            Task<Dictionary<string, JsonElement>> response = API.hasTunnelKey(userId);
            Dictionary<string, JsonElement> jsonResponse = await response;
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hologram/spacebridge.key.pub");
            if (jsonResponse["data"].GetArrayLength() == 0)
            {
                // No Keys have been uploaded
                GenerateText.Text = "No Keys have been uploaded";
                if (File.Exists(path))
                {
                    Generate.Content = "Upload Key";
                }
                Generate.Visibility = Visibility.Visible;
                GenerateText.Visibility = Visibility.Visible;
            }
            else if (File.Exists(path))
            {
                // A key exists locally, check if it matches any in the API
                bool exists = false;
                string pub_key = File.ReadAllText(path);
                foreach (var key in jsonResponse["data"].EnumerateArray())
                {
                    if (key.GetProperty("public_key").GetString() == pub_key)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    GenerateText.Text = "Local Key matches no uploaded keys";
                    Generate.Content = "Upload Key";
                    Generate.Visibility = Visibility.Visible;
                    GenerateText.Visibility = Visibility.Visible;
                }
            }
            else
            {
                GenerateText.Text = "No Keys exist locally";
                Generate.Visibility = Visibility.Visible;
                GenerateText.Visibility = Visibility.Visible;
            }

        }

        private void APIKey_PasswordChanged(object sender, RoutedEventArgs e)
        {
            apiKey = ((PasswordBox)e.Source).Password;
            API.setApiKey(apiKey);
            loadUserInfo();
        }

        private async void loadUserInfo()
        {
            Task<Dictionary<string, JsonElement>> response = API.getUserInfoAsync();
            Dictionary<string, JsonElement> jsonResponse = await response;
            if (jsonResponse["success"].GetBoolean())
            {
                userId = jsonResponse["data"].GetProperty("id").GetInt32();
                UserInfo.Visibility = Visibility.Visible;
                UserInfo.Text = "User Info: id - " + userId;
                Continue.IsEnabled = true;
            }
            else
            {
                Continue.IsEnabled = false;
            }

        }

        private void Do_Continue(object sender, RoutedEventArgs e)
        {
            loadOrgs();
            Title.Visibility = Visibility.Hidden;
            Continue.Visibility = Visibility.Hidden;
            UserInfo.Visibility = Visibility.Hidden;
            APIKeyLabel.Margin = new Thickness(30, 10, 0, 0);
            APIKey.Margin = new Thickness(100, 10, 46, 0);
            OrgDropdown.Visibility = Visibility.Visible;
            OrgLabel.Visibility = Visibility.Visible;
        }

        private void Generate_Key(object sender, RoutedEventArgs e)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hologram/spacebridge.key.pub");
            if (File.Exists(path))
            {
                API.uploadTunnelKey();
            }
            else
            {
                API.createTunnelKey();
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var local = Convert.ToInt32(Local1.Text);
                var remote = Convert.ToInt32(Remote1.Text);
                SSH.beginForwarding(local, remote);
            }
            catch (FormatException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}
