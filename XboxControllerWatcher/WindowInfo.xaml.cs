using System;
using System.Windows;
using System.Windows.Threading;

namespace XboxControllerWatcher
{
    public partial class WindowInfo : Window
    {
        private DispatcherTimer _timer;
        private bool _autohideCompletely = false;
        private uint _currentControllerIndex = 0;
        private bool _mouseIsOver = false;
        private InfoQueue _infoQueue;
        private OpacityAnimation _animation;

        public WindowInfo ()
        {
            InitializeComponent();

            // set opacity
            Opacity = 0.0;

            // configure timer for auto hiding the window
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds( Constants.WINDOW_INFO_SHOW_DURATION );
            _timer.Tick += OnAutohideTimerEvent;

            // create info queue
            _infoQueue = new InfoQueue();

            // create animation
            _animation = new OpacityAnimation( this, Constants.WINDOW_INFO_FADE_IN_DURATION, Constants.WINDOW_INFO_FADE_OUT_DURATION, AnimationCompleted );
        }

        private void OnAutohideTimerEvent ( Object source, EventArgs e )
        {
            // stop timer
            _timer.Stop();

            // hide info
            HideInfo( false );
        }

        private void Window_Closing ( object sender, System.ComponentModel.CancelEventArgs e )
        {
            // prevent window from closing
            e.Cancel = true;
        }

        private void Window_MouseEnter ( object sender, System.Windows.Input.MouseEventArgs e )
        {
            _mouseIsOver = true;

            // stop timer
            _timer.Stop();

            // ignore if fade in to 100% or fade out to 0% is active
            if ( _animation.IsActive() && ( _animation.GetAnimationTo() == 1.0 || _animation.GetAnimationTo() == 0.0 ) )
                return;

            FadeIn();
        }

        private void Window_MouseLeave ( object sender, System.Windows.Input.MouseEventArgs e )
        {
            _mouseIsOver = false;

            // ignore if fade in to 100% or fade out to 0% is active
            if ( _animation.IsActive() && ( _animation.GetAnimationTo() == 1.0 || _animation.GetAnimationTo() == 0.0 ) )
                return;

            // ignore if opacity is 0%
            if ( Opacity == 0.0 )
                return;

            FadeIn();
        }

        private void Window_MouseUp ( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            _timer.Stop();
            HideInfo( true );
        }

        public void ShowInfo ( string title, Controller controller )
        {
            Dispatcher.Invoke( () =>
            {
                // check if the window is currently show
                if ( Opacity > 0.0 )
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
                _timer.Stop();

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
                FadeIn();
            } );
        }

        private void HideInfo ( bool hideCompletely )
        {
            FadeOut( hideCompletely );
        }

        private void FadeIn ()
        {
            Show();

            // start auto hide timer if opacity is 100% and no mouse is over
            if ( Opacity == 1.0 && !_mouseIsOver )
                _timer.Start();

            // if opacity is already 100%, there is no need for an animation
            if ( Opacity == 1.0 )
                return;

            // start new fade in animation from current opacity to 100%
            _animation.AnimateTo( 1.0 );
        }

        private void FadeOut ( bool hideCompletely )
        {
            // if opacity is already 0%, there is no need for an animation
            if ( Opacity == 0.0 )
            {
                Hide();
                return;
            }

            // start new fade out animation from current opacity to 0% or 50%
            _animation.AnimateTo( _autohideCompletely || hideCompletely ? 0.0 : 0.5 );
        }

        private void AnimationCompleted ( double to )
        {
            Dispatcher.Invoke( () =>
            {
                // start auto hide timer if opacity is 100% and no mouse is over
                if ( to == 1.0 && !_mouseIsOver )
                    _timer.Start();

                // hide if opacity is 0%
                if ( to == 0.0 )
                    Hide();

                // check if there is an item in the wait queue
                if ( to == 0.0 && _infoQueue.Size() > 0 )
                {
                    // get next queue item and show info window
                    InfoQueueItem iqi = _infoQueue.Get();
                    ShowInfo( iqi.text, iqi.controller );
                }
            } );
        }
    }
}
