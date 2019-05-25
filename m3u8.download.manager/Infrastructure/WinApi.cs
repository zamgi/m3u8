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
