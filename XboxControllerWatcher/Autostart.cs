using Microsoft.Win32;

namespace XboxControllerWatcher
{
    class Autostart
    {
        const string AUTOSTART_REGISTRY_PATH = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";

        public static bool GetAutostart ()
        {
            RegistryKey key;
            key = Registry.CurrentUser.OpenSubKey( AUTOSTART_REGISTRY_PATH );
            if ( key == null )
                return false;

            string currentAutostart = (string) key.GetValue( GetAppName(), null );
            if ( currentAutostart == null )
            {
                // entry doesn't exist
                return false;
            }
            else if ( currentAutostart != GetApplicationPath() )
            {
                // entry exists but has a wrong value
                SetAutoStart( true );
                return true;
            }
            else
            {
                return true;
            }
        }

        public static void SetAutoStart ( bool enable )
        {
            if ( enable )
            {
                RegistryKey key;
                key = Registry.CurrentUser.OpenSubKey( AUTOSTART_REGISTRY_PATH, true );
                if ( key != null )
                {
                    key.SetValue( GetAppName(), GetApplicationPath(), RegistryValueKind.String );
                }
            }
            else
            {
                RegistryKey key;
                key = Registry.CurrentUser.OpenSubKey( AUTOSTART_REGISTRY_PATH, true );
                if ( key != null )
                {
                    key.DeleteValue( GetAppName() );
                }
            }
        }

        private static string GetAppName ()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        }

        private static string GetApplicationPath ()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }
    }
}
