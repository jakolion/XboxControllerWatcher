using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace XboxControllerWatcher
{
    public class Settings
    {
        private string _configFilename = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".json";
        public List<Hotkey> hotkeys = new List<Hotkey>();

        override
        public string ToString ()
        {
            // remove invalid buttons
            foreach ( Hotkey hotkey in hotkeys )
                hotkey.buttonState.CleanButtons();

            return new JavaScriptSerializer().Serialize( hotkeys );
        }

        public void FromString ( string json )
        {
            hotkeys = new JavaScriptSerializer().Deserialize<List<Hotkey>>( json );
        }

        public void ReadConfig ()
        {
            if ( File.Exists( @GetConfigPath() ) )
                FromString( File.ReadAllText( @GetConfigPath() ) );
        }

        public void WriteConfig ()
        {
            File.WriteAllText( @GetConfigPath(), ToString() );
        }

        private string GetApplicationFolder ()
        {
            return Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location );
        }

        private string GetConfigPath ()
        {
            return GetApplicationFolder() + "\\" + _configFilename;
        }
    }
}
