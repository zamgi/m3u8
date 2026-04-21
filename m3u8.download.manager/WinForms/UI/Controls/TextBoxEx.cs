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
        public const int BORDER_THICKNESS       = 1;
        public const int CLEAR_BUTTON_THICKNESS = 2;

        public TextBoxEx() => BorderColor = Color.Silver;
        protected override void OnHandleCreated( EventArgs e )
        {
            base.OnHandleCreated( e );
            WinApi.DwmSetWindowAttribute_DWMNCRP_DISABLED( this.Handle );
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        var cp = base.CreateParams;
        //        cp.ClassStyle |= (WinApi.CS_HREDRAW | WinApi.CS_VREDRAW);
        //        return (cp);
        //    }
        //}

        public event EventHandler ClearButtonClick;

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
        //new public BorderStyle BorderStyle
        //{
        //    get => base.BorderStyle;
        //    set => base.BorderStyle = _BorderColor.HasValue ? BorderStyle.Fixed3D : value;
        //}
        public int BorderThickness { get; set; } = BORDER_THICKNESS;

        public Color  ClearButtonColor          { get; set; } = Color.Silver;
        public Color  ClearButtonColorHover     { get; set; } = Color.Black;
        public Color? ClearButtonBackcolor      { get; set; }
        public Color? ClearButtonBackcolorHover { get; set; }
        public int    ClearButtonThickness      { get; set; } = CLEAR_BUTTON_THICKNESS;

        public string PlaceHolderText { get; set; }
        public bool   DrawClearButton { get; set; } = true;

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

            /// <summary>
            /// 
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct WINDOWPOS
            {
                public IntPtr hwnd;
                public IntPtr hwndInsertAfter;
                public int x;
                public int y;
                public int cx;
                public int cy;
                public uint flags;
            }

            private const string USER32_DLL = "user32.dll";
            [DllImport(USER32_DLL)] public static extern IntPtr GetWindowDC( IntPtr hWnd );
            [DllImport(USER32_DLL)] public static extern int ReleaseDC( IntPtr hWnd, IntPtr hDC );
            [DllImport(USER32_DLL)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool InvalidateRect( [In] IntPtr hWnd, [In] IntPtr rect, [In] int bErase );
            [DllImport(USER32_DLL)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool InvalidateRect( [In] IntPtr hWnd, [In] ref RECT rect, [In] int bErase );
            [DllImport(USER32_DLL)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool GetClientRect( IntPtr hWnd, out RECT rc );
            [DllImport(USER32_DLL)] public static extern int MapWindowPoints( IntPtr hWndFrom, IntPtr hWndTo, ref POINT pt, int cPoints );
            [DllImport(USER32_DLL)] public static extern bool RedrawWindow( IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags );
            [DllImport("dwmapi.dll")] public static extern int DwmSetWindowAttribute( IntPtr hwnd, int attr, ref int attrValue, int attrSize );

            const uint RDW_INVALIDATE = 0x0001;
            const uint RDW_FRAME      = 0x0020;
            const uint RDW_UPDATENOW  = 0x0100;

            //public const int CS_VREDRAW = 0x1;
            //public const int CS_HREDRAW = 0x2;            

            public const int WM_NCPAINT     = 0x85;
            public const int WM_PAINT       = 0x000F;
            public const int WM_MOUSEMOVE   = 0x0200;
            public const int WM_LBUTTONDOWN = 0x0201;
            public const int WM_LBUTTONUP   = 0x0202;
            //public const int WM_EXITSIZEMOVE = 0x0232;
            public const int WM_SIZE        = 0x0005;
            //public const int WM_SIZING      = 0x0214;
            public const int WM_NCCALCSIZE = 0x0083;
            public const int WM_ERASEBKGND = 0x0014;
            public const int WM_HSCROLL    = 0x0114;
            public const int WM_VSCROLL    = 0x0115;
            public const int WM_MOUSEWHEEL = 0x020A;

            public const int WVR_HREDRAW   = 0x0100;
            public const int WVR_VREDRAW   = 0x0200;
            public const int WM_WINDOWPOSCHANGING = 0x0046;
            public const int SWP_NOCOPYBITS       = 0x0100;

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
            public static bool RedrawWindow( IntPtr hWnd ) => RedrawWindow( hWnd, IntPtr.Zero, IntPtr.Zero, RDW_FRAME | RDW_INVALIDATE | RDW_UPDATENOW );
            public static int DwmSetWindowAttribute_DWMNCRP_DISABLED( IntPtr hWnd )
            {
                // В конструкторе или OnHandleCreated
                int DWMNCRP_DISABLED = 2; 
                return DwmSetWindowAttribute( hWnd, 2, ref DWMNCRP_DISABLED, sizeof(int) );
            }
            public static void WM_WINDOWPOSCHANGING_Set_SWP_NOCOPYBITS_flag( ref Message m )
            {
                var wp = (WINDOWPOS) Marshal.PtrToStructure( m.LParam, typeof(WINDOWPOS) );
                // Добавляем флаг NOCOPYBITS. Это заставит Windows полностью инвалидировать всё окно (и NC-область тоже)
                wp.flags |= SWP_NOCOPYBITS;
                Marshal.StructureToPtr( wp, m.LParam, true );
            }
        }

        protected override void WndProc( ref Message m )
        {
            switch ( m.Msg )
            {
                case WinApi.WM_WINDOWPOSCHANGING:
                    WinApi.WM_WINDOWPOSCHANGING_Set_SWP_NOCOPYBITS_flag( ref m );
                    break;

                #region comm
                //case WinApi.WM_NCCALCSIZE:
                //    // Создаем место под рамку, даже если BorderStyle = None
                //    if ( m.WParam != IntPtr.Zero )
                //    {
                //        base.WndProc( ref m );
                //        WinApi.RedrawWindow( m.HWnd );
                //        //goto case WinApi.WM_NCPAINT;
                //        return;

                //        //var rect = (WinApi.RECT) Marshal.PtrToStructure( m.LParam, typeof(WinApi.RECT) );
                //        //rect.left   -= 2*BORDER_THICKNESS;
                //        //rect.top    -= 2*BORDER_THICKNESS;
                //        //rect.right  += 2*BORDER_THICKNESS;
                //        //rect.bottom += 2*BORDER_THICKNESS;
                //        //Marshal.StructureToPtr( rect, m.LParam, true );
                //        //m.Result = IntPtr.Zero;
                //        //return;
                //    }
                //    break;
                #endregion

                case WinApi.WM_ERASEBKGND: 
                    if ( BorderColor.HasValue )
                    {
                        base.WndProc( ref m );
                        goto case WinApi.WM_NCPAINT;
                    }
                    break;

                case WinApi.WM_HSCROLL: case WinApi.WM_VSCROLL: case WinApi.WM_MOUSEWHEEL:
                    if ( DrawClearButton )
                    {                        
                        base.WndProc( ref m );
                        this.Refresh();
                        return;
                    }
                    break;

                case WinApi.WM_NCPAINT:
                    if ( BorderColor.HasValue )
                    {
                        DrawBorder_Routine( m.HWnd/*Handle*/, BorderColor.Value );
                        m.Result = IntPtr.Zero; // Сообщаем, что мы все отрисовали сами
                        return;
                    }
                    break;

                case WinApi.WM_PAINT:
                    base.WndProc( ref m );

                    if ( BorderColor.HasValue )
                    {
                        DrawBorder_Routine( m.HWnd, BorderColor.Value );
                    }

                    if ( this.Text.IsNullOrEmpty() )
                    {
                        if ( !this.PlaceHolderText.IsNullOrEmpty() )
                        {
                            //base.WndProc( ref m );

                            DrawPlaceHolderText_Routine( m.HWnd );
                            m.Result = IntPtr.Zero;
                            //return;
                        }
                    }
                    else if ( DrawClearButton )
                    {
                        //base.WndProc( ref m );

                        DrawClearButton_Routine( m.HWnd );
                        m.Result = IntPtr.Zero;
                        //return;
                    }
                    //break;
                    return;

                case WinApi.WM_MOUSEMOVE:
                    if ( DrawClearButton && !this.Text.IsNullOrEmpty() )
                    {
                        var pt = WinApi.GetMousePos( m.HWnd, Control.MousePosition );
                        var rc = GetClearButtonRect( m.HWnd );

                        var inClearButton = rc.Contains( pt.x, pt.y );
                        if ( inClearButton != _LastMouseMove_InClearButton )
                        {
                            _LastMouseMove_InClearButton = inClearButton;
                            WinApi.InvalidateRect( m.HWnd );
                        }
                        //if ( inClearButton ) this.Cursor = Cursors.Arrow;
                        this.Cursor = inClearButton ? Cursors.Arrow : Cursors.IBeam;
                    }
                    break;

                case WinApi.WM_LBUTTONDOWN:
                    if ( DrawClearButton && !this.Text.IsNullOrEmpty() )
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
                    if ( DrawClearButton && _IsPushed_ClearButton && !this.Text.IsNullOrEmpty() )
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
                            ClearButtonClick?.Invoke( this, EventArgs.Empty );
                            return;
                        }
                    }
                    break;
            }

            base.WndProc( ref m );
        }

        private void DrawBorder_Routine( IntPtr hWnd, Color borderColor )
        {
            var hdc = WinApi.GetWindowDC( hWnd );
            if ( hdc != IntPtr.Zero )
            {
                using ( var gr = Graphics.FromHdc( hdc ) )
                {
                    ClearBorder_Routine( gr );

                    //var color = this.Focused ? ControlPaint.Dark( borderColor ) : borderColor;
                    var color = this.Focused ? borderColor : ControlPaint.Light( borderColor );
                    var bt = this.BorderThickness;
                    using ( var pen = new Pen( color, bt ) )
                    {
                        gr.DrawRectangle( /*Pens.Red*/pen, 0, 0, Width - bt, Height - bt );
                    }
                }

                WinApi.ReleaseDC( hWnd, hdc );
            }
        }
        //private void ClearBorder_Routine( IntPtr hWnd )
        //{
        //    var hdc = WinApi.GetWindowDC( hWnd );
        //    if ( hdc != IntPtr.Zero )
        //    {
        //        using ( var gr = Graphics.FromHdc( hdc ) )
        //        {
        //            ClearBorder_Routine( gr );
        //        }

        //        WinApi.ReleaseDC( hWnd, hdc );
        //    }
        //}
        private void ClearBorder_Routine( Graphics gr )
        {
            // Draw over the default border with the parent's background color
            const int BORDER_THICKNESS = 2;
            using ( var pen = new Pen( /*Color.Red*/this.Parent.BackColor, BORDER_THICKNESS ) )
            {
                gr.DrawRectangle( pen, 1, 1, this.Width - 2, this.Height - 2 );
            }
        }

        private bool _LastMouseMove_InClearButton;
        private bool _IsPushed_ClearButton;
        private void DrawPlaceHolderText_Routine( IntPtr hWnd )
        {
            using ( var gr = Graphics.FromHwnd( hWnd ) )
            {
                gr.DrawString( this.PlaceHolderText, this.Font, Brushes.Silver, 1, 1 );
            }
        }
        private void DrawClearButton_Routine( IntPtr hWnd )
        {
            var pt = WinApi.GetMousePos( hWnd, Control.MousePosition );
            var rc = GetClearButtonRect( hWnd ); //Debug.WriteLine( rc );

            _LastMouseMove_InClearButton = rc.Contains( pt.x, pt.y );
            if ( _LastMouseMove_InClearButton && _IsPushed_ClearButton ) rc.Offset( 1, 1 );
            var color     = _LastMouseMove_InClearButton ? ClearButtonColorHover     : ClearButtonColor;
            var backColor = _LastMouseMove_InClearButton ? ClearButtonBackcolorHover : ClearButtonBackcolor;
            //(var color, var backColor) = _LastMouseMove_InClearButton ? (ClearButtonColorHover, ClearButtonBackcolorHover) : (ClearButtonColor, ClearButtonBackcolor);

            using ( var gr = Graphics.FromHwnd( hWnd ) )
            {
                if ( backColor.HasValue )
                {
                    using var br = new SolidBrush( backColor.Value );
                    gr.FillRectangle( br, rc );
                }

                using ( var pen = new Pen( color, this.ClearButtonThickness ) )
                {
                    gr.DrawLine( pen, rc.X, rc.Y, rc.Right, rc.Bottom );
                    gr.DrawLine( pen, rc.X, rc.Bottom, rc.Right, rc.Y );
                }
            }
        }
        private RectangleF GetClearButtonRect( IntPtr hWnd ) => GetClearButtonRect( hWnd, (int) (this.Font.Size + 10) );
        private static RectangleF GetClearButtonRect( IntPtr hWnd, int CLEAR_BUTTON_MAX_HEIGHT )
        {
            var suc = WinApi.GetClientRect( hWnd, out var rc ); Debug.Assert( suc );

            var height = Math.Min( CLEAR_BUTTON_MAX_HEIGHT, rc.Height );
            var rect = new RectangleF( rc.left + (rc.Width - height + 4), rc.top + 4, height - 8, height - 8 );            
            return (rect);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static partial class Extensions
    {
        public static bool IsNullOrEmpty( this string s ) => string.IsNullOrEmpty( s );
    }
}
