using System;
using System.Windows;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace XboxControllerWatcher
{
    public partial class WindowHotkeyDetection : Window
    {
        private DispatcherTimer _timer;
        private OpacityAnimation _animation;

        public WindowHotkeyDetection ()
        {
            InitializeComponent();

            // set opacity
            Opacity = 0.0;

            // create timer
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds( Constants.WINDOW_HOTKEY_DETECTED_SHOW_DURATION );
            _timer.Tick += OnTimerEvent;

            // create animation
            _animation = new OpacityAnimation( this, Constants.WINDOW_HOTKEY_DETECTED_FADE_IN_DURATION, Constants.WINDOW_HOTKEY_DETECTED_FADE_OUT_DURATION, AnimationCompleted );
        }

        public void ShowWindow ( string name )
        {
            // stop the timer if running
            _timer.Stop();

            // set new text
            text.Text = name;

            // show window
            Show();

            // start animation
            _animation.AnimateTo( 1.0 );
        }

        private void FadeOut ()
        {
            // start animation
            _animation.AnimateTo( 0.0 );
        }

        private void AnimationCompleted ( double to )
        {
            Dispatcher.Invoke( () =>
            {
                if ( to == 1.0 )
                    _timer.Start();

                if ( to == 0.0 )
                    Hide();
            } );
        }

        private void OnTimerEvent ( object sender, EventArgs e )
        {
            // stop the timer
            _timer.Stop();

            FadeOut();
        }

        private void Window_Closing ( object sender, System.ComponentModel.CancelEventArgs e )
        {
            // prevent window from closing
            e.Cancel = true;
        }

        private void Window_SizeChanged ( object sender, SizeChangedEventArgs e )
        {
            // set position of window
            Left = SystemParameters.WorkArea.Width / 2 - ActualWidth / 2;
            Top = ( SystemParameters.WorkArea.Height - ActualHeight ) * 0.1;
        }

        [DllImport( "user32.dll" )]
        public static extern int GetWindowLong ( IntPtr hwnd, int index );

        [DllImport( "user32.dll" )]
        public static extern int SetWindowLong ( IntPtr hwnd, int index, int newStyle );

        protected override void OnSourceInitialized ( EventArgs e )
        {
            const int WS_EX_TRANSPARENT = 0x00000020;
            const int WS_EX_NOACTIVATE = 0x08000000;
            const int GWL_EXSTYLE = -20;

            base.OnSourceInitialized( e );

            // get this window's handle
            IntPtr hwnd = new WindowInteropHelper( this ).Handle;

            // change the extended window style to include WS_EX_TRANSPARENT
            int extendedStyle = GetWindowLong( hwnd, GWL_EXSTYLE );
            SetWindowLong( hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE );
        }
    }
}
