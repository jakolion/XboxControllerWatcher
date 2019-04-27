using System.Collections.Generic;

namespace XboxControllerWatcher
{
    public class InfoQueueItem
    {
        public string text;
        public Controller controller;

        public InfoQueueItem ( string text, Controller controller )
        {
            this.text = text;
            this.controller = controller;
        }
    }

    public class InfoQueue
    {
        private List<InfoQueueItem> _data;

        public InfoQueue ()
        {
            _data = new List<InfoQueueItem>();
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
                _data.Add( new InfoQueueItem( text, controller ) );
        }

        public InfoQueueItem Get ()
        {
            if ( _data.Count > 0 )
            {
                InfoQueueItem iwqi = _data[0];
                _data.RemoveAt( 0 );
                return iwqi;
            }
            return null;
        }
    }
}
