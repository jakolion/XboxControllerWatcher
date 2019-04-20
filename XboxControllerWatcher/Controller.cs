using System;
using System.Windows.Media.Imaging;

namespace XboxControllerWatcher
{
    public class Controller
    {
        public uint index;
        public bool isConnected;
        public BatteryLevel batteryLevel;

        public Controller( uint index )
        {
            this.index = index;
            this.isConnected = false;
            this.batteryLevel = BatteryLevel.Unknown;
        }

        public Controller ( uint index, bool isConnected, BatteryLevel batteryLevel )
        {
            this.index = index;
            this.isConnected = isConnected;
            this.batteryLevel = batteryLevel;
        }

        public string BatteryLevelToText ()
        {
            switch ( batteryLevel )
            {
                case BatteryLevel.Empty:
                    return "Empty";

                case BatteryLevel.Low:
                    return "Low";

                case BatteryLevel.Medium:
                    return "Medium";

                case BatteryLevel.Full:
                    return "Full";

                case BatteryLevel.Unknown:
                    return "Unknown";
            }
            return "Unknown (" + batteryLevel + ")";
        }

        public BitmapImage BatteryLevelToImage ()
        {
            const string PATH = "Resources/";
            string filename = "imageControllerEmpty";
            switch ( batteryLevel )
            {
                case BatteryLevel.Empty:
                    filename = "imageControllerEmpty";
                    break;

                case BatteryLevel.Low:
                    filename = "imageControllerLow";
                    break;

                case BatteryLevel.Medium:
                    filename = "imageControllerMedium";
                    break;

                case BatteryLevel.Full:
                    filename = "imageControllerFull";
                    break;
            }
            return new BitmapImage( new Uri( PATH + filename + ".png", UriKind.Relative ) );
        }

        public enum BatteryLevel : ushort
        {
            Empty = 0,
            Low = 1,
            Medium = 2,
            Full = 3,
            Unknown = 255
        }

        public enum Button : ushort
        {
            DPadUp = 0x0001,
            DPadDown = 0x0002,
            DPadLeft = 0x0004,
            DPadRight = 0x0008,
            Start = 0x0010,
            Back = 0x0020,
            LeftThumb = 0x0040,
            RightThumb = 0x0080,
            LeftShoulder = 0x0100,
            RightShoulder = 0x0200,
            Guide = 0x0400,
            A = 0x1000,
            B = 0x2000,
            X = 0x4000,
            Y = 0x8000
        }
    }
}
