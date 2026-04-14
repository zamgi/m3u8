using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal class TextBoxEx : TextBox
    {
        public TextBoxEx() => BorderColor = Color.Silver;

        public Color? _BorderColor;
        public Color? BorderColor 
        { 
            get => _BorderColor;
            set
            {
                _BorderColor = value;
                if ( _BorderColor.HasValue ) BorderStyle = BorderStyle.Fixed3D;
            }
        }
        new public BorderStyle BorderStyle
        {
            get => base.BorderStyle;
            set => base.BorderStyle = _BorderColor.HasValue ? BorderStyle.Fixed3D : value;
        }

        public string PlaceHolderText { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private static class WinApi
        {
            /// <summary>
            /// 
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public RECT( in Rectangle rc ) => new RECT() { left = rc.Left, top = rc.Top, right = rc.Right, bottom = rc.Bottom };

                public int left;
                public int top;
                public int right;
                public int bottom;

                public int Height => bottom - top;
                public int Width => right - left;
            }

            /// <summary>
            /// 
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int x;
                public int y;
            }

            private const string USER32_DLL = "user32.dll";
            [DllImport(USER32_DLL)] public static extern IntPtr GetWindowDC( IntPtr hWnd );
            [DllImport(USER32_DLL)] public static extern int ReleaseDC( IntPtr hWnd, IntPtr hDC );
            [DllImport(USER32_DLL)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool InvalidateRect( [In] IntPtr hWnd, [In] IntPtr rect, [In] int bErase );
            [DllImport(USER32_DLL)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool InvalidateRect( [In] IntPtr hWnd, [In] ref RECT rect, [In] int bErase );
            [DllImport(USER32_DLL)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool GetClientRect( IntPtr hWnd, out RECT rc );
            [DllImport(USER32_DLL)] public static extern int MapWindowPoints( IntPtr hWndFrom, IntPtr hWndTo, ref POINT pt, int cPoints );

            public const int WM_NCPAINT     = 0x85;
            public const int WM_PAINT       = 0x000F;
            public const int WM_MOUSEMOVE   = 0x0200;
            public const int WM_LBUTTONDOWN = 0x0201;
            public const int WM_LBUTTONUP   = 0x0202;

            public static POINT GetMousePos( IntPtr hWnd, in Point mousePt )
            {
                //var suc = GetCursorPos( out var pt ); Debug.Assert( suc );
                var pt = new POINT() { x = mousePt.X, y = mousePt.Y };
                var res = MapWindowPoints( IntPtr.Zero, hWnd, ref pt, 1 );
                return (pt);
            }
            public static bool InvalidateRect( IntPtr hWnd, bool eraseBackground = true ) => InvalidateRect( hWnd, IntPtr.Zero, bErase: eraseBackground ? 1 : 0 );
            public static bool InvalidateRect( IntPtr hWnd, in Rectangle rc, bool eraseBackground = true )
            {
                var rect = new RECT( rc );
                return (InvalidateRect( hWnd, ref rect, bErase: eraseBackground ? 1 : 0 ));
            }
        }

        protected override void WndProc( ref Message m )
        {
            switch ( m.Msg )
            {
                case WinApi.WM_NCPAINT:
                    base.WndProc( ref m );

                    if ( BorderColor.HasValue )
                    {
                        DrawBorder( m.HWnd/*Handle*/, BorderColor.Value );
                    }
                    return;

                case WinApi.WM_PAINT:
                    if ( this.Text.IsNullOrEmpty() )
                    {
                        if ( !this.PlaceHolderText.IsNullOrEmpty() )
                        {
                            base.WndProc( ref m );

                            DrawPlaceHolderText( m.HWnd );
                            m.Result = IntPtr.Zero;
                            return;
                        }
                    }
                    else
                    {
                        base.WndProc( ref m );

                        DrawClearButton( m.HWnd );
                        m.Result = IntPtr.Zero;
                        return;
                    }
                    break;

                case WinApi.WM_MOUSEMOVE:
                    if ( !this.Text.IsNullOrEmpty() )
                    {
                        var pt = WinApi.GetMousePos( m.HWnd, Control.MousePosition );
                        var rc = GetClearButtonRect( m.HWnd );

                        var inClearButton = rc.Contains( pt.x, pt.y );
                        if ( inClearButton != _LastMouseMove_InClearButton )
                        {
                            _LastMouseMove_InClearButton = inClearButton;
                            WinApi.InvalidateRect( m.HWnd );
                        }
                        if ( inClearButton )
                        {
                            this.Cursor = Cursors.Arrow;
                        }
                        //_Parent.Cursor = inClearButton ? Cursors.Arrow : Cursors.IBeam;
                    }
                    break;

                case WinApi.WM_LBUTTONDOWN:
                    if ( !this.Text.IsNullOrEmpty() )
                    {
                        var pt = WinApi.GetMousePos( m.HWnd, Control.MousePosition );
                        var rc = GetClearButtonRect( m.HWnd );

                        _IsPushed_ClearButton = rc.Contains( pt.x, pt.y );
                        if ( _IsPushed_ClearButton )
                        {
                            if ( !this.Focused )
                            {
                                this.Focus();
                            }

                            base.WndProc( ref m );

                            WinApi.InvalidateRect( m.HWnd );
                            m.Result = IntPtr.Zero;
                            return;
                        }
                    }
                    break;

                case WinApi.WM_LBUTTONUP:
                    if ( _IsPushed_ClearButton && !this.Text.IsNullOrEmpty() )
                    {
                        _IsPushed_ClearButton = false;

                        var pt = WinApi.GetMousePos( m.HWnd, Control.MousePosition );
                        var rc = GetClearButtonRect( m.HWnd );

                        var inClearButton = rc.Contains( pt.x, pt.y );
                        if ( inClearButton )
                        {               
                            base.WndProc( ref m );

                            this.Clear();
                            WinApi.InvalidateRect( m.HWnd );

                            m.Result = IntPtr.Zero;
                            return;
                        }
                    }
                    break;
            }

            base.WndProc( ref m );
        }

        private void DrawBorder( IntPtr hWnd, Color borderColor )
        {
            var hdc = WinApi.GetWindowDC( hWnd );
            if ( hdc != IntPtr.Zero )
            {
                //var color = this.Focused ? ControlPaint.Dark( borderColor ) : borderColor;
                var color = this.Focused ? borderColor : ControlPaint.Light( borderColor );
                using ( var g = Graphics.FromHdc( hdc ) )
                using ( var pen = new Pen( color, 1 ) )
                {
                    g.DrawRectangle( /*Pens.Red*/pen, 0, 0, Width - 1, Height - 1 );
                }
                WinApi.ReleaseDC( hWnd, hdc );
            }
        }

        private bool _LastMouseMove_InClearButton;
        private bool _IsPushed_ClearButton;
        private void DrawPlaceHolderText( IntPtr hWnd )
        {
            using ( var gr = Graphics.FromHwnd( hWnd ) )
            {
                gr.DrawString( this.PlaceHolderText, this.Font, Brushes.Silver, 1, 1 );
            }
        }
        private void DrawClearButton( IntPtr hWnd )
        {
            var pt = WinApi.GetMousePos( hWnd, Control.MousePosition );
            var rc = GetClearButtonRect( hWnd );

            _LastMouseMove_InClearButton = rc.Contains( pt.x, pt.y );
            if ( _LastMouseMove_InClearButton && _IsPushed_ClearButton ) rc.Offset( 1, 1 );
            var br = _LastMouseMove_InClearButton ? Brushes.Black : Brushes.Silver;

            //DrawClearButton_Routine( handle, br, rc );
            using ( var gr = Graphics.FromHwnd( hWnd ) )
            using ( var pen = new Pen( br, 2 ) )
            {
                gr.DrawLine( pen, rc.X, rc.Y, rc.Right, rc.Bottom );
                gr.DrawLine( pen, rc.X, rc.Bottom, rc.Right, rc.Y );
            }
        }
        private static RectangleF GetClearButtonRect( IntPtr hWnd )
        {
            var suc = WinApi.GetClientRect( hWnd, out var rc ); Debug.Assert( suc );

            var rect = new RectangleF( rc.left + (rc.Width - rc.Height + 4), rc.top + 4, rc.Height - 8, rc.Height - 8 );
            return (rect);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static partial class Ext
    {
        public static bool IsNullOrEmpty( this string s ) => string.IsNullOrEmpty( s );
    }
}
