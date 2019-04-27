using System;
using System.Windows;
using System.Windows.Threading;

namespace XboxControllerWatcher
{
    public partial class WindowHotkeyDetection : Window
    {
        DispatcherTimer _timer;
        public WindowHotkeyDetection ()
        {
            InitializeComponent();

            // create timer
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds( Constants.WINDOW_HOTKEY_DETECTED_SHOW_DURATION );
            _timer.Tick += OnTimerEvent;
        }

        public void ShowWindow ( string name )
        {
            // stop the timer if running
            _timer.Stop();

            // set text
            text.Text = name;

            // show
            Show();

            // start the timer
            _timer.Start();
        }

        private void OnTimerEvent ( object sender, EventArgs e )
        {
            // hide the window
            Hide();
        }

        private void Window_SizeChanged ( object sender, SizeChangedEventArgs e )
        {
            // set position of window
            Left = SystemParameters.WorkArea.Width / 2 - ActualWidth / 2;
            Top = ( SystemParameters.WorkArea.Height - ActualHeight ) * 0.1;
        }
    }
}
