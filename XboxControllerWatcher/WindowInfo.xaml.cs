using System;
using System.Windows;
using System.Windows.Media.Animation;
using Timer = System.Windows.Forms.Timer;

namespace XboxControllerWatcher
{
    public partial class WindowInfo : Window
    {
        private Timer _autohideTimer;
        private bool _autohideCompletely = false;
        private uint _currentControllerIndex = 0;
        private bool _mouseIsOver = false;
        private InfoQueue _infoQueue;
        private DoubleAnimation _animation;

        public WindowInfo ()
        {
            InitializeComponent();

            // set opacity to 0%
            Opacity = 0;

            // configure timer for auto hiding the window
            _autohideTimer = new Timer();
            _autohideTimer.Enabled = false;
            _autohideTimer.Interval = Constants.WINDOW_INFO_SHOW_DURATION;
            _autohideTimer.Tick += OnAutohideTimerEvent;

            // create info queue
            _infoQueue = new InfoQueue();

            // configure animation for fading
            _animation = new DoubleAnimation();
            _animation.FillBehavior = FillBehavior.Stop;
            Timeline.SetDesiredFrameRate( _animation, 60 );
        }

        private void OnAutohideTimerEvent ( Object source, EventArgs e )
        {
            // stop timer
            _autohideTimer.Stop();

            // hide info
            HideInfo( false );
        }

        private void Window_Closing ( object sender, System.ComponentModel.CancelEventArgs e )
        {
            // stop timer
            _autohideTimer.Stop();
        }

        private void Window_MouseEnter ( object sender, System.Windows.Input.MouseEventArgs e )
        {
            _mouseIsOver = true;

            // stop timer
            _autohideTimer.Stop();

            // ignore if fade in to 100% or fade out to 0% is active
            if ( _animation.BeginTime != null && ( _animation.To == 1.0 || _animation.To == 0.0 ) )
                return;

            FadeInInfo();
        }

        private void Window_MouseLeave ( object sender, System.Windows.Input.MouseEventArgs e )
        {
            _mouseIsOver = false;

            // ignore if fade in to 100% or fade out to 0% is active
            if ( _animation.BeginTime != null && ( _animation.To == 1.0 || _animation.To == 0.0 ) )
                return;

            // ignore if opacity is 0%
            if ( Opacity == 0.0 )
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

        public void ShowInfo ( string title, Controller controller )
        {
            Dispatcher.Invoke( () =>
            {
                // check if the window is currently show
                if ( Visibility == Visibility.Visible )
                {
                    // check if the status of the currently shown controller index has changed
                    if ( controller.index != _currentControllerIndex )
                    {
                        // new info should be shown, put into wait queue
                        _infoQueue.Add( title, controller );
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

                // position of window
                Left = SystemParameters.WorkArea.Width - Width;
                Top = ( SystemParameters.WorkArea.Height - Height ) * 0.95;

                // set current controller index
                _currentControllerIndex = controller.index;

                // fade in the window
                FadeInInfo();
            } );
        }

        private void HideInfo ( bool hideCompletely )
        {
            FadeOutInfo( hideCompletely );
        }

        private void FadeInInfo ()
        {
            // stop the animation, keep current opacity
            if ( _animation.BeginTime != null )
            {
                _animation.BeginTime = null;
                _animation.Completed -= FadingFinished;
                double tmpOpacity = Opacity;
                Opacity = tmpOpacity;
                BeginAnimation( OpacityProperty, null );
            }

            Show();

            // start auto hide timer if mouse is not over
            if ( !_mouseIsOver )
                _autohideTimer.Start();

            // if opacity is already 100%, there is no need for an animation
            if ( Opacity == 1.0 )
                return;

            // start new fade in animation from current opacity to 100%
            _animation.BeginTime = TimeSpan.FromMilliseconds( 0 );
            _animation.From = Opacity;
            _animation.To = 1.0;
            int duration = Convert.ToInt32( Constants.WINDOW_INFO_FADING_IN_DURATION * ( 1.0 - Opacity ) );
            _animation.Duration = TimeSpan.FromMilliseconds( duration );
            _animation.Completed += FadingFinished;
            BeginAnimation( OpacityProperty, _animation );
        }

        private void FadeOutInfo ( bool hideCompletely )
        {
            // stop the animation, keep current opacity
            if ( _animation.BeginTime != null )
            {
                _animation.BeginTime = null;
                _animation.Completed -= FadingFinished;
                double tmpOpacity = Opacity;
                Opacity = tmpOpacity;
                BeginAnimation( OpacityProperty, null );
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
            _animation.To = ( _autohideCompletely || hideCompletely ? 0.0 : 0.5 );
            int duration = Convert.ToInt32( Constants.WINDOW_INFO_FADING_OUT_DURATION * ( Opacity - _animation.To ) );
            _animation.Duration = TimeSpan.FromMilliseconds( duration );
            _animation.Completed += FadingFinished;
            BeginAnimation( OpacityProperty, _animation );
        }

        private void FadingFinished ( object sender, EventArgs e )
        {
            // stop animation, set opacity to target value
            _animation.BeginTime = null;
            _animation.Completed -= FadingFinished;
            if ( _animation.To != null ) Opacity = Convert.ToDouble( _animation.To );
            BeginAnimation( OpacityProperty, null );

            // start auto hide timer if opacity is 100% and no mouse is over
            if ( Opacity == 1.0 && !_mouseIsOver )
                // start auto hide timer
                _autohideTimer.Start();

            // hide window if opacity is 0%
            if ( Opacity == 0.0 )
                Hide();

            // check if there is an item in the wait queue
            if ( Opacity == 0.0 && _infoQueue.Size() > 0 )
            {
                // get next queue item and show info window
                InfoQueueItem iqi = _infoQueue.Get();
                ShowInfo( iqi.text, iqi.controller );
            }
        }

    }
}
