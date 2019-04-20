using System.Threading;
using System.Windows;

namespace XboxControllerWatcher
{
    public partial class App : Application
    {
        private static Mutex mutex;

        void AppStartup ( object sender, StartupEventArgs e )
        {
            // try to create mutex for checking for another instance
            bool mutexCreatedSuccessfully;
            mutex = new Mutex( true, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, out mutexCreatedSuccessfully );
            if ( mutexCreatedSuccessfully )
            {
                // no other instance running
                InfoWindow infoWindow = new InfoWindow();
                _ = new Watcher( infoWindow );
            }
            else
            {
                // another instance is already running
                Current.Shutdown();
            }
        }
    }
}
