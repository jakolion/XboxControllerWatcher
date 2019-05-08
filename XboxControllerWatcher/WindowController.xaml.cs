using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace XboxControllerWatcher
{
    public partial class WindowController : Window
    {
        private readonly SolidColorBrush COLOR_TEXT_DEFAULT = SystemColors.WindowTextBrush;
        private readonly SolidColorBrush COLOR_TEXT_DISABLED = Brushes.Gray;
        private readonly SolidColorBrush COLOR_TEXT_ERROR = Brushes.Red;
        private readonly SolidColorBrush COLOR_BUTTON_DEFAULT = Brushes.Gray;
        private readonly SolidColorBrush COLOR_BUTTON_SELECTED = Brushes.Green;
        private readonly SolidColorBrush COLOR_BUTTON_MOUSE_OVER = SystemColors.HighlightBrush;

        private ControllerButtonState _controllerButtonState;
        private bool _saveButtonClickedOnce = false;
        private Settings _settings;
        private int _hotkeyIndex;
        private Hotkey _hotkey;
        private List<Border> _controllerButtons;
        private bool _checkEnabled;

        public WindowController ( Settings settings, int index, Window owner )
        {
            InitializeComponent();

            // set owner
            Owner = owner;

            // create list of all controller buttons for later updates
            _controllerButtons = new List<Border>()
            {
                buttonPadUp,
                buttonPadDown,
                buttonPadLeft,
                buttonPadRight,
                buttonStart,
                buttonBack,
                buttonLeftThumb,
                buttonRightThumb,
                buttonLeftShoulder,
                buttonRightShoulder,
                buttonA,
                buttonB,
                buttonX,
                buttonY
            };

            // get settings
            _settings = settings;
            _hotkeyIndex = index;
            if ( _hotkeyIndex == -1 )
            {
                // create new hotkey
                _hotkey = new Hotkey();
                _hotkey.isEnabled = true;
                _hotkey.isCommandBatteryLevel = true;
            }
            else
            {
                // load existing hotkey
                _hotkey = _settings.hotkeys[_hotkeyIndex];
            }

            // set UI elements
            _checkEnabled = false;
            inputName.Text = _hotkey.name;
            radioCommandTypeBatteryLevel.IsChecked = _hotkey.isCommandBatteryLevel;
            radioCommandTypeCustom.IsChecked = !_hotkey.isCommandBatteryLevel;
            inputCommand.Text = _hotkey.commandCustom;
            inputEnabled.IsChecked = _hotkey.isEnabled;
            _controllerButtonState = new ControllerButtonState();
            _controllerButtonState.Copy( _hotkey.buttonState );

            // set controlleer button colors
            foreach ( Border controllerButton in _controllerButtons )
            {
                // remove "button" from control name
                string buttonStateName = controllerButton.Name.Substring( 6 );
                controllerButton.Background = ( _controllerButtonState.GetButtonSelected( buttonStateName ) ? COLOR_BUTTON_SELECTED : COLOR_BUTTON_DEFAULT );
            }

            _checkEnabled = true;
            Check();
        }

        private int Check ()
        {
            int errorCount = 0;

            if ( _checkEnabled )
            {
                // name
                if ( inputName.Text == "" )
                {
                    errorCount++;
                    textName.Foreground = ( _saveButtonClickedOnce ? COLOR_TEXT_ERROR : COLOR_TEXT_DEFAULT );
                }
                else
                {
                    textName.Foreground = COLOR_TEXT_DEFAULT;
                }

                // command
                if ( Convert.ToBoolean( radioCommandTypeCustom.IsChecked ) && inputCommand.Text == "" )
                {
                    errorCount++;
                    textCommand.Foreground = ( _saveButtonClickedOnce ? COLOR_TEXT_ERROR : COLOR_TEXT_DEFAULT );
                    radioCommandTypeCustom.Foreground = ( _saveButtonClickedOnce ? COLOR_TEXT_ERROR : COLOR_TEXT_DEFAULT );
                }
                else
                {
                    textCommand.Foreground = COLOR_TEXT_DEFAULT;
                    radioCommandTypeCustom.Foreground = COLOR_TEXT_DEFAULT;
                }

                textCustomCommand.Foreground = ( Convert.ToBoolean( radioCommandTypeCustom.IsChecked ) ? COLOR_TEXT_DEFAULT : COLOR_TEXT_DISABLED );
                inputCommand.IsEnabled = Convert.ToBoolean( radioCommandTypeCustom.IsChecked );
                buttonCustomCommandTest.IsEnabled = Convert.ToBoolean( radioCommandTypeCustom.IsChecked );

                // buttons
                if ( _controllerButtonState.ButtonsCount() < 2 )
                {
                    errorCount++;
                    textButtons.Foreground = ( _saveButtonClickedOnce ? COLOR_TEXT_ERROR : COLOR_TEXT_DEFAULT );
                    outputButtons.Text = "Please select at least two buttons on the right side";
                    outputButtons.FontStyle = FontStyles.Italic;
                    outputButtons.Foreground = ( _saveButtonClickedOnce ? COLOR_TEXT_ERROR : COLOR_TEXT_DEFAULT );
                }
                else
                {
                    textButtons.Foreground = COLOR_TEXT_DEFAULT;
                    outputButtons.Text = _controllerButtonState.ButtonsToString();
                    outputButtons.FontStyle = FontStyles.Normal;
                    outputButtons.Foreground = COLOR_TEXT_DEFAULT;
                }
            }
            return errorCount;
        }

        private void ButtonCustomCommandTest_Click ( object sender, RoutedEventArgs e )
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd";
            process.StartInfo.Arguments = "/c " + inputCommand.Text;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
        }

        private void ButtonSave_Click ( object sender, RoutedEventArgs e )
        {
            _saveButtonClickedOnce = true;
            if ( Check() == 0 )
            {
                _hotkey.name = inputName.Text;
                _hotkey.isCommandBatteryLevel = Convert.ToBoolean( radioCommandTypeBatteryLevel.IsChecked );
                _hotkey.commandCustom = inputCommand.Text;
                _hotkey.buttonState.Copy( _controllerButtonState );
                _hotkey.isEnabled = Convert.ToBoolean( inputEnabled.IsChecked );
                if ( _hotkeyIndex == -1 )
                    _settings.hotkeys.Add( _hotkey );

                _settings.WriteConfig();
                Close();
            }
        }

        private void ButtonCancel_Click ( object sender, RoutedEventArgs e )
        {
            Close();
        }

        private void Input_TextChanged ( object sender, TextChangedEventArgs e )
        {
            Check();
        }

        private void ControllerButton_MouseEnter ( object sender, MouseEventArgs e )
        {
            Border button = (Border) sender;
            // remove "button" from control name
            string buttonStateName = button.Name.Substring( 6 );
            if ( _controllerButtonState.GetButtonSelected( buttonStateName ) )
                return;

            button.Background = COLOR_BUTTON_MOUSE_OVER;
        }

        private void ControllerButton_MouseLeave ( object sender, MouseEventArgs e )
        {
            Border button = (Border) sender;
            // remove "button" from control name
            string buttonStateName = button.Name.Substring( 6 );
            if ( _controllerButtonState.GetButtonSelected( buttonStateName ) )
                return;

            button.Background = COLOR_BUTTON_DEFAULT;
        }

        private void ControllerButton_MouseUp ( object sender, MouseButtonEventArgs e )
        {
            Border button = (Border) sender;
            // remove "button" from control name
            string buttonStateName = button.Name.Substring( 6 );
            _controllerButtonState.SetButtonSelected( buttonStateName, !_controllerButtonState.GetButtonSelected( buttonStateName ) );
            button.Background = ( _controllerButtonState.GetButtonSelected( buttonStateName ) ? COLOR_BUTTON_SELECTED : COLOR_BUTTON_MOUSE_OVER );
            Check();
        }

        private void CommandType_Click ( object sender, RoutedEventArgs e )
        {
            Check();
        }
    }
}
