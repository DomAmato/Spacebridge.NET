using System;
using System.Windows;
using System.Windows.Forms;

namespace Spacebridge
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private readonly NotifyIcon TrayIcon;
        private readonly ContextMenuStrip contextMenu1;
        private readonly ToolStripMenuItem  exit;
        private readonly ToolStripMenuItem  keyManager;
        private readonly ToolStripMenuItem connections;
        private readonly System.ComponentModel.IContainer components;

        private KeyManager KeyWindow;
        public App()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenu1 = new ContextMenuStrip();
            this.exit = new ToolStripMenuItem("Exit", null, new EventHandler(this.ExitItem_Click));
            this.keyManager = new ToolStripMenuItem("Key Manager", null, new EventHandler(this.KeyItem_Click));
            this.connections = new ToolStripMenuItem("Connections", null, new ToolStripMenuItem[] { });

            // Initialize contextMenu1
            this.contextMenu1.Items.AddRange(
                        new ToolStripMenuItem[] { connections, this.keyManager, this.exit });

            // Create the NotifyIcon.
            this.TrayIcon = new NotifyIcon(this.components);

            // The Icon property sets the icon that will appear
            // in the systray for this application.
            TrayIcon.Icon = Spacebridge.Resources.tray_icon;

            // The ContextMenu property sets the menu that will
            // appear when the systray icon is right clicked.
            TrayIcon.ContextMenuStrip = this.contextMenu1;

            // The Text property sets the text that will be displayed,
            // in a tooltip, when the mouse hovers over the systray icon.
            TrayIcon.Text = "Spacebridge";
            TrayIcon.Visible = true;

            // Handle the DoubleClick event to activate the form.
            TrayIcon.Click += TrayIcon_Click;
        }

        public void AddConnectionToContextMenu(int local_port, string menu_item)
        {
            ToolStripMenuItem connectionItem = new ToolStripMenuItem
            {
                Text = menu_item,
                Tag = local_port
            };
            connectionItem.Click += new EventHandler(this.Disconnect_Click);

            this.connections.DropDownItems.Add(connectionItem);
        }

        public void RemoveConnectionToContextMenu(string menu_item)
        {
            foreach(ToolStripMenuItem connection in this.connections.DropDownItems)
            {
                if(connection.Text == menu_item)
                {
                    this.connections.DropDownItems.Remove(connection);
                    break;
                }
            }
        }

        private void Disconnect_Click(object sender, EventArgs e)
        {
            foreach(ToolStripMenuItem connection in this.connections.DropDownItems)
            {
                if(connection.Tag == ((ContextMenuStrip)sender).Tag)
                {
                    this.connections.DropDownItems.Remove(connection);
                    break; 
                }
            }
            SSH.StopForwarding((int)((ContextMenuStrip)sender).Tag);
        }

        void TrayIcon_Click(object sender, EventArgs e)
        {
            //events comes here
            MainWindow.Visibility = Visibility.Visible;
            MainWindow.WindowState = WindowState.Normal;
        }

        // When the exit menu item is clicked, make a call to terminate the ApplicationContext.
        private void KeyItem_Click(object sender, EventArgs e)
        {
            if(KeyWindow == null)
            {
                KeyWindow = new KeyManager();
                KeyWindow.Closed += Window_Closing;
            }
            KeyWindow.Show();
        }

        private void Window_Closing(object sender, EventArgs e)
        {
            KeyWindow = null;
        }

        // When the exit menu item is clicked, make a call to terminate the ApplicationContext.
        private void ExitItem_Click(object sender, EventArgs e)
        {
            Current.Shutdown();
        }
    }
}
