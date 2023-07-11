using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

//using Avalonia.Platform;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static class PlatformHelper
    {
        //[NOT WOKING in Avalonia.11]//
        //public static OperatingSystemType GetOperatingSystemType()
        //{
        //    var rp = AvaloniaLocator.Current.GetService< IRuntimePlatform >();
        //    if ( rp != null )
        //    {
        //        return (rp.GetRuntimeInfo().OperatingSystem);
        //    }
        //    else
        //    {
        //        switch ( Environment.OSVersion.Platform )
        //        {
        //            case PlatformID.Win32NT: return (OperatingSystemType.WinNT);
        //            case PlatformID.Unix   : return (OperatingSystemType.Linux);
        //            case PlatformID.MacOSX : return (OperatingSystemType.OSX);
        //            default                : return (OperatingSystemType.Unknown);
        //        }
        //    }
        //}

        //public static bool IsWinNT() => (GetOperatingSystemType() == OperatingSystemType.WinNT);

        /// <summary>
        /// 
        /// </summary>
        public enum OperatingSystemType_CUSTOM_
        {
            Unknown,

            WinNT,
            Linux,
            OSX,
        }

        public static OperatingSystemType_CUSTOM_ GetOperatingSystemType()
        {
            switch ( Environment.OSVersion.Platform )
            {
                case PlatformID.Win32NT: return (OperatingSystemType_CUSTOM_.WinNT);
                case PlatformID.Unix   : return (OperatingSystemType_CUSTOM_.Linux);
                case PlatformID.MacOSX : return (OperatingSystemType_CUSTOM_.OSX);
                default                : return (OperatingSystemType_CUSTOM_.Unknown);
            }
        }

        public static bool IsWinNT() => (GetOperatingSystemType() == OperatingSystemType_CUSTOM_.WinNT);

        public static void TryMessageBox_ShowError( string text, string caption )
        {
            if ( IsWinNT() )
            {
                WinNT.MessageBox_ShowError( text, caption );
            }
            else
            {
                try
                {
                    Extensions.MessageBox_ShowError( text, caption ).Wait();
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );

                    throw (new InvalidOperationException( text ));
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private static class WinNT
        {
            [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Auto)]
            private static extern int MessageBox( IntPtr hWnd, string text, string caption, uint type );

            public static void MessageBox_ShowError( string text, string caption )
            {
                const int MB_ICONEXCLAMATION = 0x00000030;

                MessageBox( IntPtr.Zero, text, caption, MB_ICONEXCLAMATION );
            }
        }
    }
}
