namespace XboxControllerWatcher
{
    public class Hotkey
    {
        public string name = "";
        public bool isEnabled = false;
        public bool isCommandBatteryLevel = false;
        public string commandCustom = "";
        public ControllerButtonState buttonState = new ControllerButtonState();
    }
}
