using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Spacebridge
{
    /// <summary>
    /// Interaction logic for KeyManager.xaml
    /// </summary>
    public partial class KeyManager : Window
    {
        private readonly List<Tuple<Button, Label, TextBlock>> keys_ui = new List<Tuple<Button, Label, TextBlock>>();
        public KeyManager()
        {
            InitializeComponent();
            FillKeys();
        }

        private async void FillKeys()
        {
            try
            {
                var keys = await API.GetTunnelKeys(true);
                var index = 0;
                foreach (var key in keys["data"].EnumerateArray())
                {
                    var pub_key = new TextBlock { HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10 + (60 * index), 30, 0), Text = key.GetProperty("public_key").GetString().Substring(0, 70), TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Top, Height = 50, Width = 300, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#55e4c4")) };
                    var key_state = new Label { HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(80, 15 + (60 * index), 0, 0), Content = (key.GetProperty("disabled").GetInt32() == 1 ? "Disabled" : "Enabled"), Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString((key.GetProperty("disabled").GetInt32() == 1 ? "#f17e96" : "#55e4c4"))), FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Top };
                    var toggleBtn = new Button { Content = "Toggle", HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(25, 18 + (60 * index), 0, 0), VerticalAlignment = VerticalAlignment.Top, Background = Brushes.White, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#253054")), FontWeight = FontWeights.Bold, Padding = new Thickness(4, 2, 4, 2), BorderBrush = null };
                    toggleBtn.Tag = new Tuple<int, bool, int>(key.GetProperty("id").GetInt32(), key.GetProperty("disabled").GetInt32() == 0, index);
                    toggleBtn.Click += Toggle_Click;
                    KeyScrollGrid.Children.Add(pub_key);
                    KeyScrollGrid.Children.Add(key_state);
                    KeyScrollGrid.Children.Add(toggleBtn);
                    index++;
                }
            }
            catch (HttpRequestException e)
            {
                var error = new TextBlock {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(30, 10, 30, 0),
                    Text = "Request failed. Have you entered a valid API key yet?",
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Top,
                    Height = 150,
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.IndianRed };
                KeyScrollGrid.Children.Add(error);
            }
        }

        private async void Toggle_Click(object sender, RoutedEventArgs e)
        {
            var data = (Tuple<int, bool, int>)((Button)sender).Tag;
            var res = await API.SetTunnelKeyState(data.Item1, !data.Item2);
            if (res["success"].GetBoolean())
            {
                ((Label)KeyScrollGrid.Children[1 + (data.Item3 * 3)]).Content = (data.Item2 ? "Disabled" : "Enabled");
                ((Label)KeyScrollGrid.Children[1 + (data.Item3 * 3)]).Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString((data.Item2 ? "#FFD63A3A" : "#FF1BD9B3")));
                ((Button)sender).Tag = new Tuple<int, bool, int>(data.Item1, !data.Item2, data.Item3);
            }
        }
    }
}
