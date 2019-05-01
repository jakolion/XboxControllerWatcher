using System.Collections.Generic;

namespace XboxControllerWatcher
{
    public class ControllerButtonState
    {
        public Dictionary<string, bool> isSelected = new Dictionary<string, bool>();
        private readonly Dictionary<string, ushort> _values;

        public ControllerButtonState ()
        {
            // create list of buttons
            _values = new Dictionary<string, ushort>()
            {
                { "PadUp", 0x0001 },
                { "PadDown", 0x0002 },
                { "PadLeft", 0x0004 },
                { "PadRight", 0x0008 },
                { "Start", 0x0010 },
                { "Back", 0x0020 },
                { "LeftThumb", 0x0040 },
                { "RightThumb", 0x0080 },
                { "LeftShoulder", 0x0100 },
                { "RightShoulder", 0x0200 },
                //{ "Guide", 0x0400 },
                { "A", 0x1000 },
                { "B", 0x2000 },
                { "X", 0x4000 },
                { "Y", 0x8000 }
            };
        }

        public void Copy ( ControllerButtonState newState )
        {
            isSelected.Clear();
            foreach ( KeyValuePair<string, ushort> entry in _values )
            {
                if ( newState.isSelected.ContainsKey( entry.Key ) && newState.isSelected[entry.Key] )
                    isSelected[entry.Key] = true;
            }
        }

        public ushort ButtonsToNum ()
        {
            ushort sum = 0;

            foreach ( KeyValuePair<string, ushort> entry in _values )
            {
                if ( isSelected.ContainsKey( entry.Key ) && isSelected[entry.Key] )
                    sum += entry.Value;
            }

            return sum;
        }

        public string ButtonsToString ()
        {
            string str = "";

            foreach ( KeyValuePair<string, bool> entry in isSelected )
            {
                if ( entry.Value )
                    str += entry.Key + ", ";
            }

            if ( str != "" )
                str = str.Substring( 0, str.Length - 2 );

            return str;
        }

        public int ButtonsCount ()
        {
            int count = 0;

            foreach ( KeyValuePair<string, bool> entry in isSelected )
            {
                if ( entry.Value )
                    count++;
            }

            return count;
        }

        public bool GetButtonSelected ( string name )
        {
            if ( !isSelected.ContainsKey( name ) )
                return false;

            return isSelected[name];
        }

        public void SetButtonSelected ( string name, bool selected )
        {
            if ( selected )
            {
                if ( _values.ContainsKey( name ) )
                    isSelected[name] = selected;
            }
            else
            {
                if ( isSelected.ContainsKey( name ) )
                    isSelected.Remove( name );
            }
        }

        public void CleanButtons ()
        {
            List<string> toDeleteList = new List<string>();
            foreach ( KeyValuePair<string, bool> entry in isSelected )
            {
                if ( !_values.ContainsKey( entry.Key ) || !isSelected[entry.Key] )
                    toDeleteList.Add( entry.Key );
            }
            foreach ( string toDelete in toDeleteList )
                isSelected.Remove( toDelete );
        }
    }
}
