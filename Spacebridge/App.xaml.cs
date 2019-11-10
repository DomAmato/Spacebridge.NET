﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Spacebridge
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private System.Windows.Forms.NotifyIcon TrayIcon;
        private System.Windows.Forms.ContextMenu contextMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.ComponentModel.IContainer components;
        public App()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();

            // Initialize contextMenu1
            this.contextMenu1.MenuItems.AddRange(
                        new System.Windows.Forms.MenuItem[] { this.menuItem1 });

            // Initialize menuItem1
            this.menuItem1.Index = 0;
            this.menuItem1.Text = "Exit";
            this.menuItem1.Click += new EventHandler(this.exitItem_Click);

            // Create the NotifyIcon.
            this.TrayIcon = new System.Windows.Forms.NotifyIcon(this.components);

            // The Icon property sets the icon that will appear
            // in the systray for this application.
            TrayIcon.Icon = Spacebridge.Resources.App_Icon;

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

        void TrayIcon_Click(object sender, EventArgs e)
        {
            //events comes here
            MainWindow.Visibility = Visibility.Visible;
            MainWindow.WindowState = WindowState.Normal;
        }

        // When the exit menu item is clicked, make a call to terminate the ApplicationContext.
        private void exitItem_Click(object sender, EventArgs e)
        {
            Current.Shutdown();
        }
    }
}