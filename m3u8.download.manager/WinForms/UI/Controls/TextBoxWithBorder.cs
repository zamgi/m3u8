using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal class TextBoxWithBorder : TextBox
    {
        public TextBoxWithBorder() => BorderColor = Color.Silver;


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


        private const string user32_dll = "user32.dll";
        [DllImport(user32_dll)] private static extern IntPtr GetWindowDC( IntPtr hWnd );
        [DllImport(user32_dll)] private static extern int ReleaseDC( IntPtr hWnd, IntPtr hDC );

        private const int WM_NCPAINT = 0x85;
        protected override void WndProc( ref Message m )
        {
            switch ( m.Msg )
            {
                case WM_NCPAINT:
                    base.WndProc( ref m );

                    if ( BorderColor.HasValue )
                    {
                        var hdc = GetWindowDC( m.HWnd/*Handle*/ );
                        if ( hdc != IntPtr.Zero )
                        {
                            //var color = this.Focused ? ControlPaint.Dark( BorderColor.Value ) : BorderColor.Value;
                            var color = this.Focused ? BorderColor.Value : ControlPaint.Light( BorderColor.Value );
                            using ( var g   = Graphics.FromHdc( hdc ) )
                            using ( var pen = new Pen( color, 1 ) )
                            {
                                g.DrawRectangle( /*Pens.Red*/pen, 0, 0, Width - 1, Height - 1 );
                            }
                            ReleaseDC( m.HWnd/*Handle*/, hdc );
                        }
                    }
                    break;

                default:
                    base.WndProc( ref m );
                    break;
            }
        }
    }
}
