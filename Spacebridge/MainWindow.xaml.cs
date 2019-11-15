using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Spacebridge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Dictionary<string, JsonElement> orgDict;
        Dictionary<string, JsonElement> deviceDict;
        int selectedOrg = 0;
        int userId = 0;
        string apiKey;
        private readonly List<Tuple<Ellipse, ComboBox, TextBox, TextBox, Button>> devices_list = new List<Tuple<Ellipse, ComboBox, TextBox, TextBox, Button>>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void LoadOrgs()
        {
            Task<Dictionary<string, JsonElement>> response = API.GetOrganizationsAsync(userId);
            Dictionary<string, JsonElement> jsonResponse = await response;
            orgDict = jsonResponse["data"].EnumerateArray().ToDictionary(org => org.GetProperty("name").GetString());
            OrgDropdown.ItemsSource = jsonResponse["data"].EnumerateArray()
                .Select(org => org.GetProperty("name")).ToArray();
        }

        private async void LoadDevices(int orgId)
        {
            Task<Dictionary<string, JsonElement>> response = API.GetDevicesAsync(orgId);
            Dictionary<string, JsonElement> jsonResponse = await response;

            deviceDict = jsonResponse["data"].EnumerateArray().ToDictionary(org => org.GetProperty("devicename").GetString());
            foreach (var device in devices_list) {
                device.Item2.ItemsSource = jsonResponse["data"].EnumerateArray()
                .Select(device => device.GetProperty("devicename")).ToArray();
            }
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
            if (ScrollArea.Visibility == Visibility.Hidden)
            {
                ScrollArea.Visibility = Visibility.Visible;
                DeviceLabel.Visibility = Visibility.Visible;
                RPortLabel.Visibility = Visibility.Visible;
                LPortLabel.Visibility = Visibility.Visible;
                AddDevice.Visibility = Visibility.Visible;
                var status = new Ellipse { HorizontalAlignment = HorizontalAlignment.Left, Height = 18, Margin = new Thickness(16, 22, 0, 0), Stroke = Brushes.Black, VerticalAlignment = VerticalAlignment.Top, Width = 18, Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDC3F3F")) };
                var deviceMenu = new ComboBox { HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(50, 20, 0, 0), VerticalAlignment = VerticalAlignment.Top, Width = 120 };
                var local = new TextBox { HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(185, 20, 0, 0), Text = "Local", TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Top, Width = 65, Height = 22 };
                var remote = new TextBox { HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(265, 20, 0, 0), Text = "Remote", TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Top, Width = 65, Height = 22 };
                var connect = new Button { Content = "Connect", HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(344, 20, 0, 0), VerticalAlignment = VerticalAlignment.Top, Background = Brushes.White, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF063248")), FontWeight = FontWeights.Bold, Padding = new Thickness(4, 2, 4, 2), BorderBrush = null, Width = 81 };
                connect.Tag = 0;
                connect.Click += Connect_Click;
                ScrollGrid.Children.Add(status);
                ScrollGrid.Children.Add(deviceMenu);
                ScrollGrid.Children.Add(local);
                ScrollGrid.Children.Add(remote);
                ScrollGrid.Children.Add(connect);
                devices_list.Add(new Tuple<Ellipse, ComboBox, TextBox, TextBox, Button>(status, deviceMenu, local, remote, connect));
                CheckKeysExist();
            }
            LoadDevices(selectedOrg);
        }

        private async void CheckKeysExist()
        {
            Task<Dictionary<string, JsonElement>> response = API.HasTunnelKey(userId);
            Dictionary<string, JsonElement> jsonResponse = await response;
            var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hologram/spacebridge.key.pub");
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
            API.SetApiKey(apiKey);
            LoadUserInfo();
        }

        private async void LoadUserInfo()
        {
            Task<Dictionary<string, JsonElement>> response = API.GetUserInfoAsync();
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
            LoadOrgs();
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
            var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hologram/spacebridge.key.pub");
            if (File.Exists(path))
            {
                _ = API.UploadTunnelKey();
            }
            else
            {
                API.CreateTunnelKey();
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            int index = (int)((Button)sender).Tag;
            try
            {
                var local = Convert.ToInt32(devices_list[index].Item3.Text);
                var remote = Convert.ToInt32(devices_list[index].Item4.Text);
                var success = SSH.BeginForwarding(deviceDict[devices_list[index].Item2.Text].GetProperty("id").GetInt32(), local, remote);
                if (success)
                {
                    devices_list[index].Item1.Fill = Brushes.Green;
                    var tooltip = new ToolTip
                    {
                        Content = "Connected to " + devices_list[index].Item2.Text + " - Forwarding local port: " + local
                    };
                    devices_list[index].Item1.ToolTip = tooltip;
                }
            }
            catch (FormatException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void AddDevice_Click(object sender, RoutedEventArgs e)
        {
            var index = devices_list.Count();
            var status = new Ellipse { HorizontalAlignment = HorizontalAlignment.Left, Height = 18, Margin = new Thickness(16,22 + (25 * index),0,0), Stroke = Brushes.Black, VerticalAlignment = VerticalAlignment.Top, Width = 18, Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDC3F3F")) };
            var deviceMenu = new ComboBox { HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(50, 20 + (25 * index), 0, 0), VerticalAlignment = VerticalAlignment.Top, Width = 120 };
            deviceMenu.ItemsSource = deviceDict.Keys;
            var local = new TextBox { HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(185, 20 + (25 * index), 0,0), Text = "Local", TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Top, Width = 65, Height = 22 };
            var remote = new TextBox { HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(265, 20 + (25 * index), 0, 0), Text = "Remote", TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Top, Width = 65, Height = 22 };
            var connect = new Button { Content = "Connect", HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(344, 20 + (25 * index), 0,0), VerticalAlignment = VerticalAlignment.Top, Background = Brushes.White, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF063248")), FontWeight = FontWeights.Bold, Padding = new Thickness(4,2,4,2), BorderBrush = null, Width = 81 };
            connect.Tag = index;
            connect.Click += Connect_Click;
            ScrollGrid.Children.Add(status);
            ScrollGrid.Children.Add(deviceMenu);
            ScrollGrid.Children.Add(local);
            ScrollGrid.Children.Add(remote);
            ScrollGrid.Children.Add(connect);
            devices_list.Add(new Tuple<Ellipse, ComboBox, TextBox, TextBox, Button>(status, deviceMenu, local, remote, connect));
        }
    }
}
