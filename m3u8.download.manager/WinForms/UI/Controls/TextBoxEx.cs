using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using m3u8.download.manager.infrastructure;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class TextBoxEx : TextBox
    {
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
        private const int WM_PASTE   = 0x0302;
        #region comm. prev.
        /*
        protected override void WndProc( ref Message m )
        {
            base.WndProc( ref m );

            if ( m.Msg == WM_PASTE )
            {
                var text = Clipboard.GetText();
                var text_trimmed = PathnameCleaner.CleanPathnameAndFilename( text?.Trim() ); //---text?.Trim().Replace( '\r', ' ' ).Replace( '\n', ' ' ).Replace( '\t', ' ' );
                if ( text != text_trimmed )
                {
                    this.Text = text_trimmed;
                }
            }
        }
        //*/
        #endregion
        protected override void WndProc( ref Message m )
        {
            switch ( m.Msg )
            {
                case WM_NCPAINT:
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

                case WM_PASTE: 
                    var text         = Clipboard.GetText();
                    var text_trimmed = PathnameCleaner.CleanPathnameAndFilename( text?.Trim() );

                    var t = (Start: this.SelectionStart, Length: this.SelectionLength);
                    var old_text = this.Text;
                    var new_text = old_text.Substring( 0, t.Start ) + text_trimmed + old_text.Substring( t.Start + t.Length );
                    if ( new_text != old_text )
                    {
                        this.Text = new_text;
                        this.SelectionStart = t.Start + text_trimmed.Length;
                    }
                    break;

                default:
                    base.WndProc( ref m );
                    break;
            }
        }
    }
}
