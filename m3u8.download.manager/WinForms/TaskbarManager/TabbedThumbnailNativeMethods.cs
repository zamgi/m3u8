using System.Runtime.InteropServices;

namespace System.Windows.Forms.Taskbar
{
    internal static class TabbedThumbnailNativeMethods
    {
        internal const int DisplayFrame = 0x00000001;

        internal const int ForceIconicRepresentation = 7;
        internal const int HasIconicBitmap           = 10;

        internal const uint MsgfltAdd     = 1;
        internal const uint MsgfltRemove  = 2;
        internal const int  ScClose       = 0xF060;
        internal const int  ScMaximize    = 0xF030;
        internal const int  ScMinimize    = 0xF020;
        internal const uint WaActive      = 1;
        internal const uint WaClickActive = 2;
        internal const uint WmDwmSendIconicLivePreviewBitmap = 0x0326;
        internal const uint WmDwmSendIconicThumbnail         = 0x0323;

        private const string USER32_DLL = "user32.dll";
        private const string DWMAPI_DLL = "dwmapi.dll";
        private const string GDI32_DLL  = "gdi32.dll";

        [DllImport(USER32_DLL, SetLastError=true)]
        internal static extern IntPtr ChangeWindowMessageFilter( uint message, uint dwFlag );

        [DllImport(USER32_DLL)][return: MarshalAs(UnmanagedType.Bool)] internal static extern bool ClientToScreen( IntPtr hwnd, ref NativePoint point );
        [DllImport(DWMAPI_DLL)] internal static extern int DwmInvalidateIconicBitmaps( IntPtr hwnd );
        [DllImport(DWMAPI_DLL)] internal static extern int DwmSetIconicLivePreviewBitmap( IntPtr hwnd, IntPtr hbitmap, ref NativePoint ptClient, uint flags );
        [DllImport(DWMAPI_DLL)] internal static extern int DwmSetIconicLivePreviewBitmap( IntPtr hwnd, IntPtr hbitmap, IntPtr ptClient, uint flags );
        [DllImport(DWMAPI_DLL)] internal static extern int DwmSetIconicThumbnail( IntPtr hwnd, IntPtr hbitmap, uint flags );
        [DllImport(DWMAPI_DLL, PreserveSig=true)] internal static extern int DwmSetWindowAttribute( IntPtr hwnd, uint dwAttributeToSet, IntPtr pvAttributeValue, uint cbAttribute );

        /// <summary>
        /// Call this method to either enable custom previews on the taskbar (second argument as true) or to disable (second argument as
        /// false). If called with True, the method will call DwmSetWindowAttribute for the specific window handle and let DWM know that we
        /// will be providing a custom bitmap for the thumbnail as well as Aero peek.
        /// </summary>
        internal static void EnableCustomWindowPreview( IntPtr hwnd, bool enable )
        {
            var t = Marshal.AllocHGlobal( 4 );
            Marshal.WriteInt32( t, enable ? 1 : 0 );

            try
            {
                var rc = DwmSetWindowAttribute( hwnd, HasIconicBitmap, t, 4 );
                if ( rc != 0 )
                {
                    throw Marshal.GetExceptionForHR( rc );
                }

                rc = DwmSetWindowAttribute( hwnd, ForceIconicRepresentation, t, 4 );
                if ( rc != 0 )
                {
                    throw Marshal.GetExceptionForHR( rc );
                }
            }
            finally
            {
                Marshal.FreeHGlobal( t );
            }
        }

        [DllImport(USER32_DLL)][return: MarshalAs(UnmanagedType.Bool)] internal static extern bool GetClientRect( IntPtr hwnd, ref NativeRect rect );

        internal static bool GetClientSize( IntPtr hwnd, out System.Drawing.Size size )
        {
            var rect = new NativeRect();
            if ( !GetClientRect( hwnd, ref rect ) )
            {
                size = new System.Drawing.Size( -1, -1 );
                return (false);
            }
            size = new System.Drawing.Size( rect.Right, rect.Bottom );
            return (true);
        }

        [DllImport(USER32_DLL)] internal static extern IntPtr GetWindowDC( IntPtr hwnd );
        [DllImport(USER32_DLL)][return: MarshalAs(UnmanagedType.Bool)] internal static extern bool GetWindowRect( IntPtr hwnd, ref NativeRect rect );
        [DllImport(USER32_DLL)] internal static extern int ReleaseDC( IntPtr hwnd, IntPtr hdc );

        /// <summary>Sets the specified iconic thumbnail for the specified window. This is typically done in response to a DWM message.</summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="hBitmap">The thumbnail bitmap.</param>
        internal static void SetIconicThumbnail( IntPtr hwnd, IntPtr hBitmap )
        {
            var rc = DwmSetIconicThumbnail( hwnd, hBitmap, DisplayFrame );
            if ( rc != 0 )
            {
                throw Marshal.GetExceptionForHR( rc );
            }
        }

        /// <summary>
        /// Sets the specified peek (live preview) bitmap for the specified window. This is typically done in response to a DWM message.
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="bitmap">The thumbnail bitmap.</param>
        /// <param name="displayFrame">Whether to display a standard window frame around the bitmap.</param>
        internal static void SetPeekBitmap( IntPtr hwnd, IntPtr bitmap, bool displayFrame )
        {
            var rc = DwmSetIconicLivePreviewBitmap(
                hwnd,
                bitmap,
                IntPtr.Zero,
                displayFrame ? DisplayFrame : (uint) 0 );
            if ( rc != 0 )
            {
                throw (Marshal.GetExceptionForHR( rc ));
            }
        }

        /// <summary>
        /// Sets the specified peek (live preview) bitmap for the specified window. This is typically done in response to a DWM message.
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="bitmap">The thumbnail bitmap.</param>
        /// <param name="offset">
        /// The client area offset at which to display the specified bitmap. The rest of the parent window will be displayed as "remembered"
        /// by the DWM.
        /// </param>
        /// <param name="displayFrame">Whether to display a standard window frame around the bitmap.</param>
        internal static void SetPeekBitmap( IntPtr hwnd, IntPtr bitmap, System.Drawing.Point offset, bool displayFrame )
        {
            var nativePoint = new NativePoint( offset.X, offset.Y );
            var rc = DwmSetIconicLivePreviewBitmap(
                hwnd,
                bitmap,
                ref nativePoint,
                displayFrame ? DisplayFrame : (uint) 0 );

            if ( rc != 0 )
            {
                var ex = Marshal.GetExceptionForHR( rc );
                if ( ex is ArgumentException )
                {
                    // Ignore argument exception as it's not really recommended to be throwing exception when rendering the peek bitmap. If
                    // it's some other kind of exception, then throw it.
                }
                else
                {
                    throw ex;
                }
            }
        }

        [DllImport(GDI32_DLL)] [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool StretchBlt( IntPtr hDestDC, int destX, int destY, int destWidth, int destHeight, IntPtr hSrcDC, int srcX, int srcY, int srcWidth, int srcHeight, uint operation );
    }
}