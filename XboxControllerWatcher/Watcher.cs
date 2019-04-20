using System.Timers;

namespace XboxControllerWatcher
{
    public class Watcher
    {
        private Timer _batteryTimer;
        private Controller[] _controllerList;
        private InfoWindow _infoWindow;

        public Watcher ( InfoWindow infoWindow )
        {
            // set instance of InfoWindow
            _infoWindow = infoWindow;

            // create list of controllers
            _controllerList = new Controller[Constants.MAX_CONTROLLERS];
            for ( uint i = 0; i < Constants.MAX_CONTROLLERS; i++ )
            {
                _controllerList[i] = new Controller( i );
            }

            // set timer for battery updates
            _batteryTimer = new Timer();
            _batteryTimer.Interval = Constants.BATTERY_LEVEL_POLL_INTERVAL;
            _batteryTimer.AutoReset = false;
            _batteryTimer.Elapsed += ( sender, e ) => OnBatteryTimerEvent( sender, e, _controllerList, _infoWindow, _batteryTimer ); ;
            _batteryTimer.Enabled = true;
            _batteryTimer.Start();
        }

        private static void OnBatteryTimerEvent ( object source, ElapsedEventArgs e, Controller[] controllerList, InfoWindow infoWindow, Timer _timer )
        {
            // check all controllers for new status
            for ( uint i = 0; i < Constants.MAX_CONTROLLERS; i++ )
            {
                // get current status
                Controller newController = XInput.GetBatteryInformation( i );
                Controller oldController = controllerList[i];

                // check for update
                if ( newController.isConnected != oldController.isConnected || newController.batteryLevel != oldController.batteryLevel )
                {
                    // update controller list
                    controllerList[i] = newController;

                    // compare status
                    if ( newController.isConnected != oldController.isConnected )
                    {
                        // connection status has been changed
                        if ( newController.isConnected )
                        {
                            // controller has been connected
                            infoWindow.ShowInfo( "Controller " + ( i + 1 ) + " Battery Level (connected)", newController );
                        }
                        else
                        {
                            // controller has been disconnected
                            // show last battery level
                            Controller disconnectedController = oldController;
                            disconnectedController.isConnected = false;
                            infoWindow.ShowInfo( "Controller " + ( i + 1 ) + " Battery Level (disconnected)", disconnectedController );
                        }
                    }
                    else if ( newController.batteryLevel != oldController.batteryLevel )
                    {
                        // battery level has changed
                        infoWindow.ShowInfo( "Controller " + ( i + 1 ) + " Battery Level", newController );
                    }
                }
            }
            infoWindow.SetBatteryLevelMenuItems( controllerList );
            _timer.Start();
        }
    }
}
