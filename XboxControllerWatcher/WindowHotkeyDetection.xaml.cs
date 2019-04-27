using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace XboxControllerWatcher
{
    public partial class WindowHotkeyDetection : Window
    {
        private DispatcherTimer _timer;
        private DoubleAnimation _animation;

        public WindowHotkeyDetection ()
        {
            InitializeComponent();

            // set opacity
            Opacity = 0.0;
            
            // configure animation for fading
            _animation = new DoubleAnimation();
            _animation.BeginTime = null;
            _animation.FillBehavior = FillBehavior.HoldEnd;
            Timeline.SetDesiredFrameRate( _animation, 60 );

            // create timer
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds( Constants.WINDOW_HOTKEY_DETECTED_SHOW_DURATION );
            _timer.Tick += OnTimerEvent;
        }

        public void ShowWindow ( string name )
        {
            // stop the animation
            if ( _animation.BeginTime != null )
            {
                _animation.BeginTime = null;
                _animation.Completed -= FadingFinished;
            }

            // stop the timer if running
            _timer.Stop();

            // set text
            text.Text = name;

            Show();

            // start auto hide timer if opacity is 100%
            if ( Opacity == 1.0 )
            {
                _timer.Start();
                return;
            }

            // start new fade in animation from current opacity to 100%
            _animation.BeginTime = TimeSpan.FromMilliseconds( 0 );
            _animation.From = Opacity;
            _animation.To = 1.0;
            int duration = Convert.ToInt32( Constants.WINDOW_HOTKEY_DETECTED_FADE_IN_DURATION * ( 1.0 - Opacity ) );
            _animation.Duration = TimeSpan.FromMilliseconds( duration );
            _animation.Completed += FadingFinished;
            BeginAnimation( OpacityProperty, _animation );
        }

        private void FadeOut ()
        {
            // stop the animation
            if ( _animation.BeginTime != null )
            {
                _animation.BeginTime = null;
                _animation.Completed -= FadingFinished;
            }
            
            // if opacity is already 0%, there is no need for an animation
            if ( Opacity == 0.0 )
            {
                Hide();
                return;
            }

            // start new fade out animation from current opacity to 0% or 50%
            _animation.BeginTime = TimeSpan.FromMilliseconds( 0 );
            _animation.From = Opacity;
            _animation.To = 0.0;
            int duration = Convert.ToInt32( Constants.WINDOW_HOTKEY_DETECTED_FADE_OUT_DURATION * ( Opacity - _animation.To ) );
            _animation.Duration = TimeSpan.FromMilliseconds( duration );
            _animation.Completed += FadingFinished;
            BeginAnimation( OpacityProperty, _animation );
        }

        private void FadingFinished ( object sender, EventArgs e )
        {
            // stop animation
            _animation.BeginTime = null;
            _animation.Completed -= FadingFinished;

            // start timer if opacity is 100%
            if ( _animation.To == 1.0 )
                    _timer.Start();
        }

        private void OnTimerEvent ( object sender, EventArgs e )
        {
            FadeOut();
        }

        private void Window_SizeChanged ( object sender, SizeChangedEventArgs e )
        {
            // set position of window
            Left = SystemParameters.WorkArea.Width / 2 - ActualWidth / 2;
            Top = ( SystemParameters.WorkArea.Height - ActualHeight ) * 0.1;
        }
    }
}
