using Application = System.Windows.Forms.Application;

namespace XboxControllerWatcher
{
    static class Helper
    {
        public static string GetVersion ()
        {
            string version = Application.ProductVersion;
            int lastDot = version.LastIndexOf( '.' );
            if ( lastDot < 0 )
                return version;

            return version.Substring( 0, lastDot );
        }
    }
}
