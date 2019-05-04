using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace XboxControllerWatcher
{
    public partial class WindowHotkeyRemovalConfirmation : Window
    {
        public WindowHotkeyRemovalConfirmation ( Window owner )
        {
            InitializeComponent();
            Owner = owner;
            image.Source = ToImageSource( SystemIcons.Warning );
        }

        private void ButtonRemove_Click ( object sender, RoutedEventArgs e )
        {
            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click ( object sender, RoutedEventArgs e )
        {
            DialogResult = false;
            Close();
        }

        private void Window_Closing ( object sender, System.ComponentModel.CancelEventArgs e )
        {
            if ( DialogResult == null )
                DialogResult = false;
        }

        [DllImport( "gdi32.dll", SetLastError = true )]
        private static extern bool DeleteObject ( IntPtr hObject );

        private ImageSource ToImageSource ( Icon icon )
        {
            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions() );

            DeleteObject( hBitmap );

            return wpfBitmap;
        }
    }
}
