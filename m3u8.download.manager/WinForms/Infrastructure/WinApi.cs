using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal static class WinApi
    {
        private const string USER32_DLL = "user32.dll";

        #region [.SetForegroundWindow.]
        [DllImport(USER32_DLL)] public static extern bool SetForegroundWindow( IntPtr hWnd );
        /*
        [DllImport(USER32_DLL, ExactSpelling=true, CharSet=CharSet.Auto)] private static extern IntPtr GetParent( IntPtr hWnd );
        /// <summary>
        /// 
        /// </summary>
        public enum GetWindowEnum : uint
        {
            GW_HWNDFIRST    = 0,
            GW_HWNDLAST     = 1,
            GW_HWNDNEXT     = 2,
            GW_HWNDPREV     = 3,
            GW_OWNER        = 4,
            GW_CHILD        = 5,
            GW_ENABLEDPOPUP = 6,
        }
        [DllImport(USER32_DLL, SetLastError=true)] public static extern IntPtr GetWindow( IntPtr hWnd, GetWindowEnum uCmd );
        [M(O.AggressiveInlining)] private static IntPtr GetOwnerWindow( this IntPtr hWnd ) => GetWindow( hWnd, GetWindowEnum.GW_OWNER );
        [M(O.AggressiveInlining)] private static IntPtr GetParentWindow( this IntPtr hWnd ) => GetParent( hWnd );
        [M(O.AggressiveInlining)] public static bool IsZero( this IntPtr hWnd ) => (hWnd == IntPtr.Zero);
        [M(O.AggressiveInlining)] public static IntPtr GetParentOrOwnerWindow( this IntPtr hWnd )
        {
            var p_wnd = hWnd.GetParentWindow();
            if ( p_wnd.IsZero() )
            {
                p_wnd = hWnd.GetOwnerWindow();
            }
            return (p_wnd);
        }

        [M(O.AggressiveInlining)] public static IntPtr GetTopForegroundWindow()
        {
            var foreWnd = GetForegroundWindow();
            for ( var p_wnd = foreWnd.GetParentOrOwnerWindow(); !p_wnd.IsZero(); )
            {
                foreWnd = p_wnd;
                p_wnd = foreWnd.GetParentOrOwnerWindow();
            }
            return (foreWnd);
        }
        //*/
        #endregion

        #region [.SetForceForegroundWindow.]
        [DllImport(USER32_DLL)] public static extern IntPtr GetForegroundWindow(); 
        [DllImport(USER32_DLL)] private static extern uint GetWindowThreadProcessId( IntPtr hWnd, IntPtr processId );
        [DllImport(USER32_DLL)] private static extern bool AttachThreadInput( uint idAttach, uint idAttachTo, bool fAttach );
        [DllImport(USER32_DLL, SetLastError=true)] private static extern bool BringWindowToTop( IntPtr hWnd );
        [DllImport(USER32_DLL)] private static extern bool ShowWindow( IntPtr hWnd, uint nCmdShow );
        [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();

        private const uint SW_SHOW = 5;

        public static void SetForceForegroundWindow( IntPtr hWnd, IntPtr foregroundWnd )
        {
            //IntPtr foreWnd    = GetForegroundWindow();
            uint foreThread = GetWindowThreadProcessId( foregroundWnd, IntPtr.Zero );
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
        [DllImport(USER32_DLL, EntryPoint="SendMessageA", ExactSpelling=true, CharSet=CharSet.Ansi, SetLastError=true)] private static extern int SendMessage( IntPtr hwnd, int wMsg, int wParam, int lParam );

        private const int WM_SETREDRAW = 0xB;

        [M(O.AggressiveInlining)] public static void SuspendDrawing( this Control control ) => SendMessage( control.Handle, WM_SETREDRAW, 0 /*FALSE*/, 0 );
        [M(O.AggressiveInlining)] public static void ResumeDrawing( this Control control, bool redraw = true )
        {
            SendMessage( control.Handle, WM_SETREDRAW, 1 /*TRUE*/, 0 );

            if ( redraw )
            {
                control.Refresh();
            }
        }
        #endregion

        #region [.WINDOWPLACEMENT.]
        /// <summary>
        /// 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)] public struct WINDOWPLACEMENT
        {
            private const int WPF_RESTORETOMAXIMIZED = 2;

            public int       length;
            public int       flags;
            public int       showCmd;
            public Point     ptMinPosition;
            public Point     ptMaxPosition;
            public Rectangle rcNormalPosition;

            public bool IsRestoredWindowWillBeMaximized => (flags == WPF_RESTORETOMAXIMIZED);
            public static WINDOWPLACEMENT Create() => new WINDOWPLACEMENT() { length = Marshal.SizeOf( typeof(WINDOWPLACEMENT) ) };
        }

        [DllImport(USER32_DLL)][return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetWindowPlacement( IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl );

        public static WINDOWPLACEMENT? GetWindowPlacement( IntPtr hWnd )
        {
            var wp = WINDOWPLACEMENT.Create();
            if ( GetWindowPlacement( hWnd, ref wp ) )
            {
                return (wp);
            }
            return (null);
        }

        public static FormWindowState ToFormWindowState( this WINDOWPLACEMENT? wp ) => ((!wp.HasValue || !wp.Value.IsRestoredWindowWillBeMaximized) ? FormWindowState.Normal : FormWindowState.Maximized);
        #endregion
    }
}
