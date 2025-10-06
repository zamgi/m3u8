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

        public static bool Try_ShellExploreAndSelectFile( string filePath, out Exception error )
        {
            if ( IsWinNT() )
                return (WinNT.ShellExploreAndSelectFile( filePath, out error ));
            error = new NotSupportedException();
            return (false);
        }

        //public static bool Try_CoInitialize( out Exception error )
        //{
        //    if ( IsWinNT() )
        //    {
        //        error = default;
        //        return (WinNT.CoInitialize());
        //    }
        //    error = new NotSupportedException();
        //    return (false);
        //}

        /// <summary>
        /// 
        /// </summary>
        private static class WinNT
        {
            #region [.MessageBox.]
            private const string USER32_DLL = "user32.dll";

            [DllImport(USER32_DLL, SetLastError=true, CharSet=CharSet.Auto)]
            private static extern int MessageBox( IntPtr hWnd, string text, string caption, uint type );
            public static void MessageBox_ShowError( string text, string caption )
            {
                const int MB_ICONEXCLAMATION = 0x00000030;

                MessageBox( IntPtr.Zero, text, caption, MB_ICONEXCLAMATION );
            }
            #endregion

            //#region [.CoInitialize.]
            //[DllImport(OLE32_DLL, SetLastError=true)] private static extern int CoInitialize( IntPtr pvReserved );
            //[DllImport(OLE32_DLL, SetLastError=true)] public static extern void CoUninitialize();

            //public static bool CoInitialize() => (CoInitialize( IntPtr.Zero ) == S_OK);
            //#endregion

            #region [.ShellExploreAndSelectFile.]
            private const int    S_OK        = 0x0;
            //private const int    S_FALSE     = 0x1;
            //private const int    RPC_E_CHANGED_MODE = -2147417850; //0x80010106;
            private const string SHELL32_DLL = "shell32.dll";
            [DllImport(SHELL32_DLL)] private static extern int SHParseDisplayName( [MarshalAs(UnmanagedType.LPWStr)] string pszName, IntPtr pbc, out IntPtr ppidl, /*SFGAO*/uint sfgaoIn, /*out SFGAO*/IntPtr psfgaoOut );
            [DllImport(SHELL32_DLL)] private static extern int SHOpenFolderAndSelectItems( IntPtr pidlFolder, uint cidl, IntPtr apidl, uint dwFlags );
            private static int SHParseDisplayName_( string pszName, out IntPtr pidl ) => SHParseDisplayName( pszName, IntPtr.Zero, out pidl, 0, IntPtr.Zero );
            private static int SHOpenFolderAndSelectItems_( IntPtr pidlFolder ) => SHOpenFolderAndSelectItems( pidlFolder, 0, IntPtr.Zero, 0 ); 

            private const string OLE32_DLL = "ole32.dll";
            [DllImport(OLE32_DLL)] private static extern void CoTaskMemFree( IntPtr pv );

            public static bool ShellExploreAndSelectFile( string filePath, out Exception error )
            {
                // Parse the full filename into a pidl
                var hr = SHParseDisplayName_( filePath, out var pidl );
                if ( hr == S_OK )
                {
                    try
                    {
                        // Open Explorer and select the thing
                        hr = SHOpenFolderAndSelectItems_( pidl );
                        error = Marshal.GetExceptionForHR( hr );
                        return (hr == S_OK);
                    }
                    finally
                    {
                        // Use the task allocator to free to returned pidl
                        CoTaskMemFree( pidl );
                    }
                }
                error = Marshal.GetExceptionForHR( hr );
                return (false);
            }
            #endregion
        }
    }
}
