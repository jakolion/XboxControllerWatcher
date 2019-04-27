﻿using System;
using System.Drawing;
using System.Threading;
using System.Globalization;
using System.Windows;
using System.Reflection;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using ContextMenu = System.Windows.Forms.ContextMenu;
using MenuItem = System.Windows.Forms.MenuItem;
using Application = System.Windows.Forms.Application;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using MouseButtons = System.Windows.Forms.MouseButtons;

namespace XboxControllerWatcher
{
    public partial class Main : Window
    {
        private NotifyIcon _trayIcon;
        private MenuItem[] _batteryLevelMenuItems;
        private WindowHotkeyDetection _windowHotkeyDetection;

        public Watcher watcher;
        public WindowInfo windowInfo;
        public Settings settings;

        public Main ()
        {
            InitializeComponent();

            // set language to English/US
            Thread.CurrentThread.CurrentCulture = new CultureInfo( "en-US" );
            Thread.CurrentThread.CurrentUICulture = new CultureInfo( "en-US" );

            // read settings file
            settings = new Settings();
            settings.ReadConfig();

            // create info window
            windowInfo = new WindowInfo();

            // create hotkey detection window
            _windowHotkeyDetection = new WindowHotkeyDetection();

            // create tray icon
            _trayIcon = new NotifyIcon();
            _trayIcon.Icon = (Icon) Properties.Resources.ResourceManager.GetObject( "icon" );
            _trayIcon.Text = Application.ProductName;
            _trayIcon.MouseUp += OnTrayMenu_MouseUp;

            // create tray menu
            ContextMenu trayMenu = new ContextMenu();

            MenuItem version = new MenuItem( Application.ProductName + " " + Helper.GetVersion() );
            version.Enabled = false;
            trayMenu.MenuItems.Add( version );

            trayMenu.MenuItems.Add( "Controller Hotkey Configuration...", OnHotkeyConfiguration );

            MenuItem autostartMenuItem = new MenuItem( "Autostart" );
            autostartMenuItem.Checked = Autostart.GetAutostart();
            autostartMenuItem.Click += OnAutostart;
            trayMenu.MenuItems.Add( autostartMenuItem );

            trayMenu.MenuItems.Add( "-" );

            MenuItem batteryLevelHeaderMenuItem = new MenuItem( "Controller Battery Level:" );
            batteryLevelHeaderMenuItem.Enabled = false;
            trayMenu.MenuItems.Add( batteryLevelHeaderMenuItem );
            _batteryLevelMenuItems = new MenuItem[Constants.MAX_CONTROLLERS];
            for ( uint i = 0; i < Constants.MAX_CONTROLLERS; i++ )
            {
                _batteryLevelMenuItems[i] = new MenuItem();
                _batteryLevelMenuItems[i].Enabled = false;
                trayMenu.MenuItems.Add( _batteryLevelMenuItems[i] );
            }

            trayMenu.MenuItems.Add( "-" );

#if DEBUG

            trayMenu.MenuItems.Add( "Test: Controller 1 Low", OnShowTest1 );
            trayMenu.MenuItems.Add( "Test: Controller 1 Full", OnShowTest2 );
            trayMenu.MenuItems.Add( "Test: Controller 2 Medium", OnShowTest3 );
            trayMenu.MenuItems.Add( "Test: Hotkey Detection", OnHotkeyTest1 );

            trayMenu.MenuItems.Add( "-" );

#endif

            trayMenu.MenuItems.Add( "Exit", OnTrayMenuExitClicked );

            // add tray menu to icon
            _trayIcon.ContextMenu = trayMenu;

            // enable tray icon
            _trayIcon.Visible = true;

            // create watcher
            watcher = new Watcher( this );

            // open hotkey config
            //new WindowHotkeys( ref _settings ).ShowDialog();

            // open new hotkey window
            //new WindowController( ref _settings, -1 ).ShowDialog();
        }

#if DEBUG

        private void OnShowTest1 ( object sender, EventArgs e )
        {
            Controller c = new Controller( 0 );
            c.batteryLevel = Controller.BatteryLevel.Low;
            windowInfo.ShowInfo( "Controller 1 Battery Level", c );
        }

        private void OnShowTest2 ( object sender, EventArgs e )
        {
            Controller c = new Controller( 0 );
            c.batteryLevel = Controller.BatteryLevel.Full;
            windowInfo.ShowInfo( "Controller 1 Battery Level", c );
        }

        private void OnShowTest3 ( object sender, EventArgs e )
        {
            Controller c = new Controller( 1 );
            c.batteryLevel = Controller.BatteryLevel.Medium;
            windowInfo.ShowInfo( "Controller 2 Battery Level", c );
        }

        private void OnHotkeyTest1 ( object sender, EventArgs e )
        {
            ShowHotkeyDetected( "Hotkey detected" );
        }

#endif

        private void OnHotkeyConfiguration ( object sender, EventArgs e )
        {
            new WindowHotkeys( settings ).ShowDialog();
        }

        private void OnAutostart ( object sender, EventArgs e )
        {
            MenuItem menuItem = (MenuItem) sender;
            Autostart.SetAutoStart( !menuItem.Checked );
            menuItem.Checked = Autostart.GetAutostart();
        }

        private void OnTrayMenuExitClicked ( object sender, EventArgs e )
        {
            Close();
        }

        private void OnTrayMenu_MouseUp ( object sender, MouseEventArgs e )
        {
            if ( e.Button == MouseButtons.Left )
            {
                MethodInfo mi = typeof( NotifyIcon ).GetMethod( "ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic );
                mi.Invoke( _trayIcon, null );
            }
        }

        public void SetBatteryLevelMenuItems ( Controller[] controllerList )
        {
            Dispatcher.Invoke( () =>
            {
                for ( uint i = 0; i < Constants.MAX_CONTROLLERS; i++ )
                {
                    _batteryLevelMenuItems[i].Text = "    " + ( i + 1 ).ToString() + ". " + ( controllerList[i].isConnected ? controllerList[i].BatteryLevelToText() : "-" );
                }
            } );
        }

        public void ShowHotkeyDetected ( string name )
        {
            Dispatcher.Invoke( () =>
            {
                _windowHotkeyDetection.ShowWindow( name );
            } );
        }

        private void Window_Closing ( object sender, System.ComponentModel.CancelEventArgs e )
        {
            // dispose tray icon
            _trayIcon.Dispose();

            // exit active threads
            Environment.Exit( 0 );

            // exit app
            System.Windows.Application.Current.Shutdown();
        }
    }
}
