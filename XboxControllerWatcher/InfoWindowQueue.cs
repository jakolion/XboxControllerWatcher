using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XboxControllerWatcher
{
    public class InfoWindowQueueItem
    {
        public string text;
        public Controller controller;

        public InfoWindowQueueItem ( string text, Controller controller )
        {
            this.text = text;
            this.controller = controller;
        }
    }

    public class InfoWindowQueue
    {
        private List<InfoWindowQueueItem> _data;

        public InfoWindowQueue ()
        {
            _data = new List<InfoWindowQueueItem>();
        }

        public int Size ()
        {
            return _data.Count;
        }

        public void Add ( string text, Controller controller )
        {
            // check if the controller index is in the list already
            bool found = false;
            for ( int i = 0; i < _data.Count; i++ )
            {
                if ( _data[i].controller.index == controller.index )
                {
                    // update the list entry
                    _data[i].text = text;
                    _data[i].controller = controller;
                    found = true;
                    break;
                }
            }

            // if controller wasn't found, add it
            if ( !found )
                _data.Add( new InfoWindowQueueItem( text, controller ) );
        }

        public InfoWindowQueueItem Get ()
        {
            if ( _data.Count > 0 )
            {
                InfoWindowQueueItem iwqi = _data[0];
                _data.RemoveAt( 0 );
                return iwqi;
            }
            return null;
        }
    }
}
