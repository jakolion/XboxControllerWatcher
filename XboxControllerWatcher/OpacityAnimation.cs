using System;
using System.Windows;
using System.Timers;
using System.Windows.Threading;

namespace XboxControllerWatcher
{
    class OpacityAnimation
    {
        private const int FRAMES_PER_SECOND = 60;

        private Timer _timer;
        private Timer _completeTimer;
        private Window _window;
        private double _fadeInStep;
        private double _fadeOutStep;
        private double _animateTo;
        private Action<double> _complete;

        public OpacityAnimation ( Window window, int fadeInDuration, int fadeOutDuration, Action<double> callback )
        {
            _window = window;
            _timer = new Timer();
            _timer.Interval = 1000 / FRAMES_PER_SECOND;
            _timer.Elapsed += OnTimerEvent;
            _timer.AutoReset = true;

            _completeTimer = new Timer();
            _completeTimer.Interval = 50;
            _completeTimer.Elapsed += ( s, e ) => {
                _complete( _animateTo );
            };
            _completeTimer.AutoReset = false;

            _fadeInStep = 1 / ( fadeInDuration / _timer.Interval );
            _fadeOutStep = 1 / ( fadeOutDuration / _timer.Interval );

            _complete = callback;
        }

        private void OnTimerEvent ( object sender, EventArgs e )
        {
            Application.Current.Dispatcher.Invoke( DispatcherPriority.Send, new Action( () =>
            {
                if ( _window.Opacity < ( _animateTo - _fadeInStep ) )
                {
                    // fade in
                    _window.Opacity += _fadeInStep;
                }
                else if ( _window.Opacity > ( _animateTo + _fadeOutStep ) )
                {
                    // fade out
                    _window.Opacity -= _fadeOutStep;
                }
                else
                {
                    // target reached
                    _timer.Stop();
                    _window.Opacity = _animateTo;
                    _completeTimer.Start();
                    //_complete( _animateTo );
                }
            } ) );
            //Application.Current.Dispatcher.Invoke( DispatcherPriority.ContextIdle, new Action( () => { } ) );
        }

        public void AnimateTo ( double to )
        {
            // stop current timer
            _timer.Stop();
            _completeTimer.Stop();

            // get current opacity
            double currentOpacity = 0;
            Application.Current.Dispatcher.Invoke( () =>
            {
                currentOpacity = _window.Opacity;
            } );

            // set new target
            _animateTo = to;

            // start timer
            _timer.Start();
        }

        public bool IsActive ()
        {
            return _timer.Enabled;
        }

        public double GetAnimationTo ()
        {
            return _animateTo;
        }
    }
}
