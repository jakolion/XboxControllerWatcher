using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace XboxControllerWatcher
{
    public partial class WindowHotkeys : Window
    {
        private Settings _settings;
        private List<ListItem> _listItems = new List<ListItem>();

        public WindowHotkeys ( Settings settings )
        {
            InitializeComponent();
            list.ItemsSource = _listItems;
            _settings = settings;
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
            new WindowController( _settings, -1 ).ShowDialog();
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
                new WindowController( _settings, li.index ).ShowDialog();
                RefreshUi();
            }
        }

        private void ButtonRemove_Click ( object sender, RoutedEventArgs e )
        {
            if ( list.SelectedItems.Count > 0 )
            {
                if ( !Convert.ToBoolean( new WindowHotkeyRemovalConfirmation().ShowDialog() ) )
                    return;

                foreach ( ListItem li in list.SelectedItems )
                    _settings.hotkeys.RemoveAt( li.index );

                _settings.WriteConfig();
                RefreshUi();
            }
        }

        private void List_MouseDoubleClick ( object sender, MouseButtonEventArgs e )
        {
            ListItem li = ( e.OriginalSource as FrameworkElement ).DataContext as ListItem;
            if ( li != null )
            {
                new WindowController( _settings, li.index ).ShowDialog();
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
