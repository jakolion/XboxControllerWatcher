using System;
using System.Threading;
using System.Drawing;
using System.Windows;
using System.Globalization;
using System.Windows.Media.Animation;
using System.Reflection;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using ContextMenu = System.Windows.Forms.ContextMenu;
using MenuItem = System.Windows.Forms.MenuItem;
using Application = System.Windows.Forms.Application;
using Timer = System.Windows.Forms.Timer;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using MouseButtons = System.Windows.Forms.MouseButtons;

namespace XboxControllerWatcher
{
    public partial class InfoWindow : Window
    {
        private NotifyIcon _trayIcon;
        private MenuItem[] _batteryLevelMenuItems;
        private Timer _autohideTimer;
        private bool _autohideCompletely = false;
        private uint _currentControllerIndex = 0;
        private bool _mouseIsOver = false;
        private InfoWindowQueue _infoWindowQueue;
        private DoubleAnimation _animation;

        public InfoWindow ()
        {
            InitializeComponent();

            // set language to English/US
            Thread.CurrentThread.CurrentCulture = new CultureInfo( "en-US" );
            Thread.CurrentThread.CurrentUICulture = new CultureInfo( "en-US" );

            // set opacity
            Opacity = 0;

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

            trayMenu.MenuItems.Add( "Show Controller 1 Low", OnShowTest1 );
            trayMenu.MenuItems.Add( "Show Controller 1 Full", OnShowTest2 );
            trayMenu.MenuItems.Add( "Show Controller 2 Full", OnShowTest3 );

            trayMenu.MenuItems.Add( "-" );

#endif

            MenuItem autostartMenuItem = new MenuItem( "Autostart" );
            autostartMenuItem.Checked = Autostart.GetAutostart();
            autostartMenuItem.Click += OnAutostart;
            trayMenu.MenuItems.Add( autostartMenuItem );

            trayMenu.MenuItems.Add( "Exit", OnTrayMenuExitClicked );

            // add tray menu to icon
            _trayIcon.ContextMenu = trayMenu;

            // enable tray icon
            _trayIcon.Visible = true;

            // set timer for hiding the window
            _autohideTimer = new Timer();
            _autohideTimer.Enabled = false;
            _autohideTimer.Interval = Constants.INFO_WINDOW_SHOW_DURATION;
            _autohideTimer.Tick += OnTimerEvent;

            // create InfoWindowQueue
            _infoWindowQueue = new InfoWindowQueue();

            // create animation for fading
            _animation = new DoubleAnimation();
            _animation.FillBehavior = FillBehavior.Stop;
            Timeline.SetDesiredFrameRate( _animation, 60 );
        }

#if DEBUG

        private void OnShowTest1 ( object sender, EventArgs e )
        {
            Controller c = new Controller( 0 );
            c.batteryLevel = Controller.BatteryLevel.Low;
            ShowInfo( "Controller 1 Battery Level", c );
        }

        private void OnShowTest2 ( object sender, EventArgs e )
        {
            Controller c = new Controller( 0 );
            c.batteryLevel = Controller.BatteryLevel.Full;
            ShowInfo( "Controller 1 Battery Level", c );
        }

        private void OnShowTest3 ( object sender, EventArgs e )
        {
            Controller c = new Controller( 1 );
            c.batteryLevel = Controller.BatteryLevel.Full;
            ShowInfo( "Controller 2 Battery Level", c );
        }

#endif

        private void OnAutostart ( object sender, EventArgs e )
        {
            MenuItem menuItem = (MenuItem) sender;
            Autostart.SetAutoStart( !menuItem.Checked );
            menuItem.Checked = Autostart.GetAutostart();
        }

        private void OnTimerEvent ( Object source, EventArgs e )
        {
            _autohideTimer.Stop();
            HideInfo( false );
        }

        private void OnTrayMenuExitClicked ( object sender, EventArgs e )
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void OnTrayMenu_MouseUp ( object sender, MouseEventArgs e )
        {
            if ( e.Button == MouseButtons.Left )
            {
                MethodInfo mi = typeof( NotifyIcon ).GetMethod( "ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic );
                mi.Invoke( _trayIcon, null );
            }
        }

        private void Window_Closing ( object sender, System.ComponentModel.CancelEventArgs e )
        {
            _trayIcon.Dispose();
        }

        private void Window_MouseEnter ( object sender, System.Windows.Input.MouseEventArgs e )
        {
            _mouseIsOver = true;

            // ignore if fade in to 100% or fade out to 0% is active
            if ( _animation.BeginTime != null && ( _animation.To == 1.0 || _animation.To == 0.0 ) )
                return;

            // stop timer
            _autohideTimer.Stop();

            FadeInInfo();
        }

        private void Window_MouseLeave ( object sender, System.Windows.Input.MouseEventArgs e )
        {
            _mouseIsOver = false;

            // ignore if fade in to 100% or fade out to 0% is active
            if ( _animation.BeginTime != null && ( _animation.To == 1.0 || _animation.To == 0.0 ) )
                return;

            // ignore if window was currently hidden
            if ( Visibility != Visibility.Visible )
                return;

            FadeInInfo();
        }

        private void Window_MouseUp ( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            _autohideTimer.Stop();
            HideInfo( true );
        }

        public void SetBatteryLevelMenuItems ( Controller[] controllerList )
        {
            Dispatcher.Invoke( (Action) ( () =>
            {
                for ( uint i = 0; i < Constants.MAX_CONTROLLERS; i++ )
                {
                    _batteryLevelMenuItems[i].Text = "    " + ( i + 1 ).ToString() + ". " + ( controllerList[i].isConnected ? controllerList[i].BatteryLevelToText() : "-" );
                }
            } ) );
        }

        public void ShowInfo ( string title, Controller controller )
        {
            this.Dispatcher.Invoke( (Action) ( () =>
            {
                // check if the window is currently show
                if ( Visibility == Visibility.Visible )
                {
                    // check if the status of the currently shown controller index has changed
                    if ( controller.index != _currentControllerIndex )
                    {
                        // new info should be shown, put into wait queue
                        _infoWindowQueue.Add( title, controller );
                        return;
                    }
                }

                // stop timer
                _autohideTimer.Stop();

                // set if window will auto hide completely
                _autohideCompletely = ( controller.batteryLevel == Controller.BatteryLevel.Full || controller.batteryLevel == Controller.BatteryLevel.Medium );

                // set data
                infoTitle.Text = title;
                infoStatus.Text = controller.BatteryLevelToText();
                infoImage.Source = controller.BatteryLevelToImage();
                infoX.Visibility = ( _autohideCompletely ? Visibility.Hidden : Visibility.Visible );

                // position window
                Left = SystemParameters.WorkArea.Width - this.Width;
                Top = ( SystemParameters.WorkArea.Height - this.Height ) * 0.95;

                // set current controller index
                _currentControllerIndex = controller.index;

                // fade in the window
                FadeInInfo();
            } ) );
        }

        private void HideInfo ( bool completely )
        {
            FadeOutInfo( completely );
        }

        private void FadeInInfo ()
        {
            _animation.BeginTime = null;
            _animation.Completed -= FadingFinished;
            double tmpOpacity = Opacity;
            Opacity = tmpOpacity;
            BeginAnimation( OpacityProperty, null );

            Show();

            if ( Opacity == 1.0 && !_mouseIsOver )
            {
                // start timer until fade out
                _autohideTimer.Start();
                return;
            }

            // set new fade in animation
            _animation.BeginTime = TimeSpan.FromMilliseconds( 0 );
            _animation.From = Opacity;
            _animation.To = 1.0;
            int duration = Convert.ToInt32( Constants.INFO_WINDOW_FADING_IN_DURATION * ( 1.0 - Opacity ) );
            _animation.Duration = TimeSpan.FromMilliseconds( duration );
            _animation.Completed += FadingFinished;
            BeginAnimation( OpacityProperty, _animation );
        }

        private void FadeOutInfo ( bool completely )
        {
            _animation.BeginTime = null;
            _animation.Completed -= FadingFinished;
            double tmpOpacity = Opacity;
            Opacity = tmpOpacity;
            BeginAnimation( OpacityProperty, null );

            if ( Opacity == 0.0 )
            {
                Hide();
                return;
            }

            // set new fade out animation
            _animation.BeginTime = TimeSpan.FromMilliseconds( 0 );
            _animation.From = Opacity;
            _animation.To = ( _autohideCompletely || completely ? 0.0 : 0.5 );
            int duration = Convert.ToInt32( Constants.INFO_WINDOW_FADING_OUT_DURATION * ( Opacity - _animation.To ) );
            _animation.Duration = TimeSpan.FromMilliseconds( duration );
            _animation.Completed += FadingFinished;
            BeginAnimation( OpacityProperty, _animation );
        }

        private void FadingFinished ( object sender, EventArgs e )
        {
            _animation.BeginTime = null;
            _animation.Completed -= FadingFinished;
            if ( _animation.To != null ) Opacity = (double) _animation.To;
            BeginAnimation( OpacityProperty, null );

            if ( Opacity == 1.0 && !_mouseIsOver )
                // start auto hide timer
                _autohideTimer.Start();

            if ( Opacity == 0.0 )
                Hide();

            // check if there is an item in the wait queue
            if ( Opacity == 0 && _infoWindowQueue.Size() > 0 )
            {
                InfoWindowQueueItem iwqi = _infoWindowQueue.Get();
                ShowInfo( iwqi.text, iwqi.controller );
            }
        }

    }
}
