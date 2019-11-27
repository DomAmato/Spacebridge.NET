using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Spacebridge
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private NotifyIcon TrayIcon;
        private ContextMenu contextMenu1;
        private MenuItem menuItem1;
        private MenuItem menuItem2;
        private System.ComponentModel.IContainer components;

        private KeyManager keyWindow;
        public App()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenu1 = new ContextMenu();
            this.menuItem1 = new MenuItem();
            this.menuItem2 = new MenuItem();

            // Initialize contextMenu1
            this.contextMenu1.MenuItems.AddRange(
                        new MenuItem[] { this.menuItem1, this.menuItem2 });

            // Initialize menuItem1
            this.menuItem1.Index = 1;
            this.menuItem1.Text = "Exit";
            this.menuItem1.Click += new EventHandler(this.exitItem_Click);

            this.menuItem2.Index = 0;
            this.menuItem2.Text = "Key Manager";
            this.menuItem2.Click += new EventHandler(this.keyItem_Click);

            // Create the NotifyIcon.
            this.TrayIcon = new NotifyIcon(this.components);

            // The Icon property sets the icon that will appear
            // in the systray for this application.
            TrayIcon.Icon = Spacebridge.Resources.tray_icon;

            // The ContextMenu property sets the menu that will
            // appear when the systray icon is right clicked.
            TrayIcon.ContextMenu = this.contextMenu1;

            // The Text property sets the text that will be displayed,
            // in a tooltip, when the mouse hovers over the systray icon.
            TrayIcon.Text = "Spacebridge";
            TrayIcon.Visible = true;

            // Handle the DoubleClick event to activate the form.
            TrayIcon.Click += TrayIcon_Click;
        }

        public void AddConnectionToContextMenu(int local_port, string menu_item)
        {
            MenuItem connectionItem = new MenuItem();

            connectionItem.Index = this.contextMenu1.MenuItems.Count;
            connectionItem.Text = menu_item;
            connectionItem.Tag = local_port;
            connectionItem.Click += new EventHandler(this.Disconnect_Click);

            this.contextMenu1.MenuItems.Add(connectionItem);
        }

        private void Disconnect_Click(object sender, EventArgs e)
        {
            SSH.StopForwarding((int)((MenuItem)sender).Tag);
        }

        void TrayIcon_Click(object sender, EventArgs e)
        {
            //events comes here
            MainWindow.Visibility = Visibility.Visible;
            MainWindow.WindowState = WindowState.Normal;
        }

        // When the exit menu item is clicked, make a call to terminate the ApplicationContext.
        private void keyItem_Click(object sender, EventArgs e)
        {
            if(keyWindow == null)
            {
                keyWindow = new KeyManager();
                keyWindow.Closed += Window_Closing;
            }
            keyWindow.Show();
        }

        private void Window_Closing(object sender, EventArgs e)
        {
            keyWindow = null;
        }

        // When the exit menu item is clicked, make a call to terminate the ApplicationContext.
        private void exitItem_Click(object sender, EventArgs e)
        {
            Current.Shutdown();
        }
    }
}
