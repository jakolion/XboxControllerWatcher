using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace XboxControllerWatcher
{
    public partial class WindowConfiguration : Window
    {
        private Settings _settings;
        private List<ListItem> _listItems = new List<ListItem>();

        public WindowConfiguration ( Settings settings )
        {
            InitializeComponent();

            // read settings
            _settings = settings;

            // set data for hotkey list
            list.ItemsSource = _listItems;

            // create dropdown menu
            inputNotificationPersistentBatteryLevel.Items.Insert( 0, "<Never>" );
            for ( Controller.BatteryLevel i = 0; i <= Controller.BatteryLevel.Full; i++ )
            {
                Controller c = new Controller( 0, false, i );
                // increase index by one
                int index = (int) i + 1;
                inputNotificationPersistentBatteryLevel.Items.Insert( index, c.BatteryLevelToText() );
            }

            // set current values
            inputNotificationCustomCommand.IsChecked = _settings.notificationCustomCommand;
            inputNotificationPersistentBatteryLevel.SelectedIndex = _settings.notificationPersistentBatteryLevel + 1;

            // load ui
            RefreshUi();
        }

        public void RefreshUi ()
        {
            _listItems.Clear();
            for ( int i = 0; i < _settings.hotkeys.Count; i++ )
            {
                ListItem li = new ListItem();
                li.index = i;
                li.name = _settings.hotkeys[i].name;
                li.enabled = _settings.hotkeys[i].isEnabled;
                li.command = _settings.hotkeys[i].isCommandBatteryLevel ? "<Show Battery Level>" : _settings.hotkeys[i].commandCustom;
                li.buttons = _settings.hotkeys[i].buttonState.ButtonsToString();
                _listItems.Add( li );
            }

            // update items
            list.Items.Refresh();

            // update columns width
            GridView gv = list.View as GridView;
            if ( gv != null )
            {
                foreach ( var c in gv.Columns )
                {
                    if ( double.IsNaN( c.Width ) )
                    {
                        c.Width = c.ActualWidth;
                    }
                    c.Width = double.NaN;
                }
            }

            Check();
        }

        private void Check ()
        {
            ButtonEdit.IsEnabled = list.SelectedItems.Count == 1;
            ButtonRemove.IsEnabled = list.SelectedItems.Count > 0;
        }

        private void ButtonAdd_Click ( object sender, RoutedEventArgs e )
        {
            new WindowController( _settings, -1, this ).ShowDialog();
            RefreshUi();
        }

        private void ButtonClose_Click ( object sender, RoutedEventArgs e )
        {
            Close();
        }

        private void ButtonEdit_Click ( object sender, RoutedEventArgs e )
        {
            ListItem li = list.SelectedItem as ListItem;
            if ( li != null )
            {
                new WindowController( _settings, li.index, this ).ShowDialog();
                RefreshUi();
            }
        }

        private void ButtonRemove_Click ( object sender, RoutedEventArgs e )
        {
            if ( list.SelectedItems.Count > 0 )
            {
                if ( !Convert.ToBoolean( new WindowHotkeyRemovalConfirmation( this ).ShowDialog() ) )
                    return;

                List<int> ids = new List<int>();
                foreach ( ListItem li in list.SelectedItems )
                    ids.Add( li.index );

                ids.Sort();
                ids.Reverse();

                foreach ( int i in ids )
                    _settings.hotkeys.RemoveAt( i );

                _settings.WriteConfig();
                RefreshUi();
            }
        }

        private void List_MouseDoubleClick ( object sender, MouseButtonEventArgs e )
        {
            ListItem li = ( e.OriginalSource as FrameworkElement ).DataContext as ListItem;
            if ( li != null )
            {
                new WindowController( _settings, li.index, this ).ShowDialog();
                RefreshUi();
            }
        }

        private void CheckBox_Clicked ( object sender, RoutedEventArgs e )
        {
            CheckBox checkbox = sender as CheckBox;
            if ( checkbox != null )
            {
                try
                {
                    int i = (int) checkbox.Tag;
                    _settings.hotkeys[i].isEnabled = Convert.ToBoolean( checkbox.IsChecked );
                    _settings.WriteConfig();
                    RefreshUi();
                }
                catch { }
            }
        }

        private void List_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            Check();
        }

        private void List_MouseDown ( object sender, MouseButtonEventArgs e )
        {
            // unselect all items if click doesn't hit an element
            HitTestResult r = VisualTreeHelper.HitTest( this, e.GetPosition( this ) );
            if ( r.VisualHit.GetType() != typeof( ListBoxItem ) )
                list.UnselectAll();
        }

        private void InputNotificationCustomCommand_Click ( object sender, RoutedEventArgs e )
        {
            _settings.notificationCustomCommand = Convert.ToBoolean( inputNotificationCustomCommand.IsChecked );
            _settings.WriteConfig();
        }

        private void InputNotificationPersistentBatteryLevel_SelectionChanged ( object sender, System.Windows.Controls.SelectionChangedEventArgs e )
        {
            _settings.notificationPersistentBatteryLevel = inputNotificationPersistentBatteryLevel.SelectedIndex - 1;
            _settings.WriteConfig();
        }
    }

    class ListItem
    {
        public int index { get; set; }
        public bool enabled { get; set; }
        public string name { get; set; }
        public string command { get; set; }
        public string buttons { get; set; }
    }
}
