using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    internal static class WinApi
    {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow( IntPtr hWnd );

        /// <summary>
        /// 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            private const int WPF_RESTORETOMAXIMIZED = 2;

            public int       length;
            public int       flags;
            public int       showCmd;
            public Point     ptMinPosition;
            public Point     ptMaxPosition;
            public Rectangle rcNormalPosition;

            public bool IsRestoredWindowWillBeMaximized => (flags == WPF_RESTORETOMAXIMIZED);
            public static WINDOWPLACEMENT Default 
            {
                get
                {
                    var wp = default(WINDOWPLACEMENT);
                    wp.length = Marshal.SizeOf( wp );
                    return (wp);
                }
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowPlacement( IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl );

        public static WINDOWPLACEMENT? GetWindowPlacement( IntPtr hWnd )
        {
            var wp = WINDOWPLACEMENT.Default;
            if ( GetWindowPlacement( hWnd, ref wp ) )
            {
                return (wp);
            }
            return (null);
        }

        public static FormWindowState ToFormWindowState( this WINDOWPLACEMENT? wp )
        {
            if ( !wp.HasValue || !wp.Value.IsRestoredWindowWillBeMaximized )
            {
                return (FormWindowState.Normal);
            }
            return (FormWindowState.Maximized);
        }

        #region [.Redraw Suspend/Resume.]
        [DllImport("user32.dll", EntryPoint="SendMessageA", ExactSpelling=true, CharSet=CharSet.Ansi, SetLastError=true)]
        private static extern int SendMessage( IntPtr hwnd, int wMsg, int wParam, int lParam );
        private const int WM_SETREDRAW = 0xB;

        public static void SuspendDrawing( this Control control ) => SendMessage( control.Handle, WM_SETREDRAW, 0 /*FALSE*/, 0 );
        public static void ResumeDrawing( this Control control, bool redraw = true )
        {
            SendMessage( control.Handle, WM_SETREDRAW, 1 /*TRUE*/, 0 );

            if ( redraw )
            {
                control.Refresh();
            }
        }
        #endregion
    }
}
