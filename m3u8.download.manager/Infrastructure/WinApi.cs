using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal static class WinApi
    {
        #region [.SetForegroundWindow.]
        [DllImport("user32.dll")] public static extern bool SetForegroundWindow( IntPtr hWnd );
        #endregion

        #region [.SetForceForegroundWindow.]
        // When you don't want the ProcessId, use this overload and pass IntPtr.Zero for the second parameter
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId( IntPtr hWnd, IntPtr ProcessId );
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow(); 
        [DllImport("user32.dll")] private static extern bool AttachThreadInput( uint idAttach, uint idAttachTo, bool fAttach );
        [DllImport("user32.dll", SetLastError=true)] private static extern bool BringWindowToTop( IntPtr hWnd );
        [DllImport("user32.dll")] private static extern bool ShowWindow( IntPtr hWnd, uint nCmdShow );
        [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();

        private const uint SW_SHOW = 5;

        public static void SetForceForegroundWindow( IntPtr hWnd )
        {
            uint foreThread = GetWindowThreadProcessId( GetForegroundWindow(), IntPtr.Zero );
            uint appThread  = GetCurrentThreadId();            
            if ( foreThread != appThread )
            {
                AttachThreadInput( foreThread, appThread, true );
                BringWindowToTop( hWnd );
                ShowWindow( hWnd, SW_SHOW );
                AttachThreadInput( foreThread, appThread, false );
            }
            else
            {
                BringWindowToTop( hWnd );
                ShowWindow( hWnd, SW_SHOW );
            }
        }
        #endregion

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
