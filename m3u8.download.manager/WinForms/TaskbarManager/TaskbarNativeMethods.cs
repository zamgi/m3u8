﻿using System.Runtime.InteropServices;

namespace System.Windows.Forms.Taskbar
{
    /// <summary>
    /// 
    /// </summary>
    public enum ThumbnailAlphaType
    {
        /// <summary>Let the system decide.</summary>
        Unknown = 0,

        /// <summary>No transparency</summary>
        NoAlphaChannel = 1,

        /// <summary>Has transparency</summary>
        HasAlphaChannel = 2,
    }

    internal enum KnownDestinationCategory
    {
        Frequent = 1,
        Recent
    }

    internal enum SetTabPropertiesOption
    {
        None                      = 0x0, 
        UseAppThumbnailAlways     = 0x1,
        UseAppThumbnailWhenActive = 0x2,
        UseAppPeekAlways          = 0x4,
        UseAppPeekWhenActive      = 0x8
    }

    internal enum ShellAddToRecentDocs
    {
        Pidl            = 0x1,
        PathA           = 0x2,
        PathW           = 0x3,
        AppIdInfo       = 0x4, // indicates the data type is a pointer to a SHARDAPPIDINFO structure
        AppIdInfoIdList = 0x5, // indicates the data type is a pointer to a SHARDAPPIDINFOIDLIST structure
        Link            = 0x6, // indicates the data type is a pointer to an IShellLink instance
        AppIdInfoLink   = 0x7, // indicates the data type is a pointer to a SHARDAPPIDINFOLINK structure
    }

    internal enum TaskbarActiveTabSetting
    {
        UseMdiThumbnail   = 0x1,
        UseMdiLivePreview = 0x2
    }

    internal enum TaskbarProgressBarStatus
    {
        NoProgress    = 0,
        Indeterminate = 0x1,
        Normal        = 0x2,
        Error         = 0x4,
        Paused        = 0x8
    }

    internal enum ThumbButtonMask
    {
        Bitmap    = 0x1,
        Icon      = 0x2,
        Tooltip   = 0x4,
        THB_FLAGS = 0x8
    }

    [Flags] internal enum ThumbButtonOptions
    {
        Enabled        = 0x00000000,
        Disabled       = 0x00000001,
        DismissOnClick = 0x00000002,
        NoBackground   = 0x00000004,
        Hidden         = 0x00000008,
        NonInteractive = 0x00000010
    }

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
    internal struct ThumbButton
    {
        /// <summary>WPARAM value for a THUMBBUTTON being clicked.</summary>
        internal const int Clicked = 0x1800;

        [MarshalAs(UnmanagedType.U4)]
        internal ThumbButtonMask Mask;

        internal uint Id;
        internal uint Bitmap;
        internal IntPtr Icon;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
        internal string Tip;

        [MarshalAs(UnmanagedType.U4)]
        internal ThumbButtonOptions Flags;
    }

    internal static class TaskbarNativeMethods
    {
        internal const int  WmCommand                        = 0x0111;
        internal const uint WmDwmSendIconicLivePreviewBitmap = 0x0326;
        internal const uint WmDwmSendIconThumbnail           = 0x0323;

        private const string USER32_DLL  = "user32.dll";
        private const string SHELL32_DLL = "shell32.dll";

        // Register Window Message used by Shell to notify that the corresponding taskbar button has been added to the taskbar.
        internal static readonly uint WmTaskbarButtonCreated = RegisterWindowMessage( "TaskbarButtonCreated" );

        [DllImport(SHELL32_DLL)] public static extern int SHGetPropertyStoreForWindow( IntPtr hwnd, ref Guid iid /*IID_IPropertyStore*/, [Out, MarshalAs(UnmanagedType.Interface)] out IPropertyStore propertyStore );
        [DllImport(SHELL32_DLL)] internal static extern void GetCurrentProcessExplicitAppUserModelID( [Out, MarshalAs(UnmanagedType.LPWStr)] out string AppID );

        internal static IPropertyStore GetWindowPropertyStore( IntPtr hwnd )
        {
            var guid = new Guid(ShellIIDGuid.IPropertyStore );
            var rc = SHGetPropertyStoreForWindow( hwnd, ref guid, out var propStore );
            if ( rc != 0 )
            {
                throw (Marshal.GetExceptionForHR( rc ));
            }
            return (propStore);
        }

        [DllImport(USER32_DLL, EntryPoint="RegisterWindowMessage", SetLastError=true, CharSet=CharSet.Unicode)]
        internal static extern uint RegisterWindowMessage( [MarshalAs(UnmanagedType.LPWStr)] string lpString );

        [DllImport(SHELL32_DLL)] internal static extern void SetCurrentProcessExplicitAppUserModelID( [MarshalAs(UnmanagedType.LPWStr)] string AppID );

        /// <summary>Sets the window's application id by its window handle.</summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="appId">The application id.</param>
        internal static void SetWindowAppId( IntPtr hwnd, string appId ) => SetWindowProperty( hwnd, SystemProperties.System.AppUserModel.ID, appId );

        internal static void SetWindowProperty( IntPtr hwnd, PropertyKey propkey, string value )
        {
            // Get the IPropertyStore for the given window handle
            var propStore = GetWindowPropertyStore( hwnd );

            // Set the value
            using ( var pv = new PropVariant( value ) )
            {
                var result = propStore.SetValue( ref propkey, pv );
                if ( !CoreErrorHelper.Succeeded( result ) )
                {
                    throw (new ShellException( result ));
                }
            }

            // Dispose the IPropertyStore and PropVariant
            Marshal.ReleaseComObject( propStore );
        }

        [DllImport(SHELL32_DLL)] internal static extern void SHAddToRecentDocs( ShellAddToRecentDocs flags, [MarshalAs(UnmanagedType.LPWStr)] string path );

        internal static void SHAddToRecentDocs( string path ) => SHAddToRecentDocs( ShellAddToRecentDocs.PathW, path );

        internal static class TaskbarGuids
        {
            internal static Guid IObjectArray = new Guid("92CA9DCD-5622-4BBA-A805-5E9F541BD8C9");
            internal static Guid IUnknown     = new Guid("00000000-0000-0000-C000-000000000046");
        }
    }
}