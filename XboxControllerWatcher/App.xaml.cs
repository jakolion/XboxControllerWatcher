using System.Windows;

namespace XboxControllerWatcher
{
    public partial class App : Application
    {
        void AppStartup ( object sender, StartupEventArgs e )
        {
            InfoWindow infoWindow = new InfoWindow();
            _ = new Watcher( infoWindow );
        }
    }
}
