using System;
using System.Timers;
using System.Threading;
using Timer = System.Timers.Timer;
using System.Diagnostics;

namespace XboxControllerWatcher
{
    public class Watcher
    {
        private Main _main;
        private Timer _hotkeyTimer;
        private Timer _batteryTimer;
        private Controller[] _controllerList;

        public Watcher ( Main main )
        {
            _main = main;

            // create list of controllers
            _controllerList = new Controller[Constants.MAX_CONTROLLERS];
            for ( uint i = 0; i < Constants.MAX_CONTROLLERS; i++ )
                _controllerList[i] = new Controller( i );

            // set timer for hotkey checks
            _hotkeyTimer = new Timer();
            _hotkeyTimer.Interval = Constants.POLL_INTERVAL_HOTKEY;
            _hotkeyTimer.AutoReset = false;
            _hotkeyTimer.Elapsed += ( sender, e ) => OnHotkeyTimerEvent( sender, e );
            _hotkeyTimer.Enabled = true;
            _hotkeyTimer.Start();

            // set timer for battery updates
            _batteryTimer = new Timer();
            _batteryTimer.Interval = Constants.POLL_INTERVAL_BATTERY_LEVEL;
            _batteryTimer.AutoReset = false;
            _batteryTimer.Elapsed += ( sender, e ) => OnBatteryTimerEvent( sender, e );
            _batteryTimer.Enabled = true;
            _batteryTimer.Start();
        }

        private void OnHotkeyTimerEvent ( object source, EventArgs e )
        {
            // check if there is at least one controller connected
            bool isAnyControllerConnected = false;
            for ( uint i = 0; i < Constants.MAX_CONTROLLERS; i++ )
            {
                if ( _controllerList[i].isConnected )
                {
                    isAnyControllerConnected = true;
                    break;
                }
            }

            // check if there are any hotkeys to be monitored
            bool isAnyHotkeyEnabled = false;
            if ( isAnyControllerConnected )
            {
                foreach ( Hotkey hotkey in _main.settings.hotkeys )
                {
                    if ( hotkey.isEnabled )
                    {
                        isAnyHotkeyEnabled = true;
                        break;
                    }
                }
            }

            // check if at least one controller is connected and one
            // hotkey is defined, otherwise we want to reduce the load
            if ( isAnyControllerConnected && isAnyHotkeyEnabled )
            {
                // set short intervall and continue
                _hotkeyTimer.Interval = Constants.POLL_INTERVAL_HOTKEY;
            }
            else
            {
                // set long interval and restart the loop
                _hotkeyTimer.Interval = Constants.POLL_INTERVAL_HOTKEY_INACTIVE;

                // restart timer
                _hotkeyTimer.Start();

                return;
            }

            // check all controllers for button presses
            for ( uint i = 0; i < Constants.MAX_CONTROLLERS; i++ )
            {
                // get current button state
                ushort state = XInput.GetButtonState( i );

                // loop through hotkeys
                bool hotkeyDetected = false;
                bool hotkeyDetectedWindowShown = false;
                foreach ( Hotkey hotkey in _main.settings.hotkeys )
                {
                    if ( hotkey.isEnabled )
                    {
                        if ( hotkey.buttonState.ButtonsToNum() == state )
                        {
                            // hotkey detected
                            hotkeyDetected = true;

                            // execute command
                            if ( hotkey.isCommandBatteryLevel )
                            {
                                if ( _controllerList[i].isConnected )
                                    _main.windowInfo.ShowInfo( "Controller " + ( i + 1 ) + " Battery Level", _controllerList[i] );
                            }
                            else
                            {
                                // show detected window
                                if ( !hotkeyDetectedWindowShown )
                                {
                                    _main.ShowHotkeyDetected( hotkey.name );
                                    hotkeyDetectedWindowShown = true;
                                }

                                // start the process
                                Process process = new Process();
                                process.StartInfo.FileName = "cmd";
                                process.StartInfo.Arguments = "/c " + hotkey.commandCustom;
                                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                process.Start();
                            }
                        }
                    }
                }

                // if hotkey detected, wait for release
                if ( hotkeyDetected )
                {
                    while ( state == XInput.GetButtonState( i ) )
                    {
                        Thread.Sleep( Constants.POLL_INTERVAL_HOTKEY );
                    }
                }
            }

            // restart timer
            _hotkeyTimer.Start();
        }

        private void OnBatteryTimerEvent ( object source, ElapsedEventArgs e )
        {
            // check all controllers for new status
            for ( uint i = 0; i < Constants.MAX_CONTROLLERS; i++ )
            {
                // get current status
                Controller newController = XInput.GetBatteryInformation( i );
                Controller oldController = _controllerList[i];

                // check for update
                if ( newController.isConnected != oldController.isConnected || newController.batteryLevel != oldController.batteryLevel )
                {
                    // update controller list
                    _controllerList[i] = newController;

                    // compare status
                    if ( newController.isConnected != oldController.isConnected )
                    {
                        // connection status has been changed
                        if ( newController.isConnected )
                        {
                            // controller has been connected
                            _main.windowInfo.ShowInfo( "Controller " + ( i + 1 ) + " Battery Level (connected)", newController );
                        }
                        else
                        {
                            // controller has been disconnected
                            // show last battery level
                            Controller disconnectedController = oldController;
                            disconnectedController.isConnected = false;
                            _main.windowInfo.ShowInfo( "Controller " + ( i + 1 ) + " Battery Level (disconnected)", disconnectedController );
                        }
                    }
                    else if ( newController.batteryLevel != oldController.batteryLevel )
                    {
                        // battery level has changed
                        _main.windowInfo.ShowInfo( "Controller " + ( i + 1 ) + " Battery Level", newController );
                    }
                }
            }

            // update context menu
            _main.SetBatteryLevelMenuItems( _controllerList );

            // restart timer
            _batteryTimer.Start();
        }
    }
}
