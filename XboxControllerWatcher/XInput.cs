using System.Runtime.InteropServices;

namespace XboxControllerWatcher
{
    class XInput
    {
        class Imports
        {
            internal const string DLL_NAME = "XInput1_4.dll";
            [DllImport( DLL_NAME )]
            public static extern uint XInputGetState ( uint controllerIndex, out ControllerState state );
            [DllImport( DLL_NAME )]
            public static extern void XInputSetState ( uint controllerIndex, float leftMotor, float rightMotor );
            [DllImport( DLL_NAME )]
            public static extern uint XInputGetBatteryInformation ( uint controllerIndex, byte devType, out ControllerBatteryInformation battery );
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct ControllerBatteryInformation
        {
            public byte type;
            public byte level;
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct ControllerState
        {
            uint packetNumber;
            ControllerControls controls;

            [StructLayout( LayoutKind.Sequential )]
            public struct ControllerControls
            {
                ushort buttons;
                byte leftTrigger;
                byte rightTrigger;
                short thumbLeftX;
                short thumbLeftY;
                short thumbRightX;
                short thumbRightY;
            }
        }

        private enum BatteryType : byte
        {
            Disconnected = 0,
            Wired = 1,
            Alkaline = 2,
            Nimh = 3,
            Unknown = 255
        }

        public static ControllerState GetState ( uint controllerIndex )
        {
            ControllerState state;
            Imports.XInputGetState( controllerIndex, out state );
            return state;
        }

        public static Controller GetBatteryInformation ( uint controllerIndex )
        {
            const byte BATTERY_DEVTYPE_GAMEPAD = 0;
            ControllerBatteryInformation batteryInformation;
            Imports.XInputGetBatteryInformation( controllerIndex, BATTERY_DEVTYPE_GAMEPAD, out batteryInformation );
            Controller controller = new Controller( controllerIndex );
            controller.isConnected = ( batteryInformation.type != (byte) BatteryType.Disconnected && batteryInformation.type != (byte) BatteryType.Unknown );
            if ( controller.isConnected )
            {
                controller.batteryLevel = (Controller.BatteryLevel) batteryInformation.level;
            }
            return controller;
        }

        public static void SetVibration ( uint controllerIndex, float leftMotor, float rightMotor )
        {
            Imports.XInputSetState( controllerIndex, leftMotor, rightMotor );
        }
    }
}
