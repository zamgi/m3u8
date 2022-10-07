using System.Runtime.InteropServices;
using System.Text;

namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal class NumericUpDownEx : NumericUpDown
    {
        protected override void OnMouseWheel( MouseEventArgs e )
        {
            if ( e is HandledMouseEventArgs hme )
            {
                hme.Handled = true;
            }

            if ( 0 < e.Delta )
            {
                var v = this.Value + this.Increment;
                this.Value = Math.Min( this.Maximum, v );
            }
            else if ( e.Delta < 0 )
            {
                var v = this.Value - this.Increment;
                this.Value = Math.Max( this.Minimum, v );
            }
        }
        public void Fire_ValueChanged() => this.OnValueChanged( EventArgs.Empty );
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class NumericUpDownEx_Transparent : NumericUpDownEx
    {
        /// <summary>
        /// 
        /// </summary>
        private sealed class OverriddenNativeWindow : NativeWindow
        {
            private const int WM_NCHITTEST  = 0x0084;
            private const int WM_SETFOCUS   = 0x0007;
            private const int HTTRANSPARENT = -1;

            private OverriddenNativeWindow() { }
            public static void AssignHWnd( IntPtr hWnd ) => new OverriddenNativeWindow().AssignHandle( hWnd );

            protected override void WndProc( ref Message m )
            {
                switch ( m.Msg )
                {
                    case WM_NCHITTEST:
                    case WM_SETFOCUS:
                        m.Result = new IntPtr( HTTRANSPARENT ); 
                    return;
                }
                base.WndProc( ref m );
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            EnumChildWindows( this.Handle, EnumChildWindows_Routine, IntPtr.Zero );
        }

        private delegate bool EnumWindowsProc( IntPtr hWnd, IntPtr lParam );
        [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] private static extern bool EnumChildWindows( IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam );
        [DllImport("user32.dll")] private static extern int GetClassName( IntPtr hWnd, StringBuilder lpClassName, int nMaxCount );
        #region comm.
        /*
        private const int GWL_STYLE = -16;
        [DllImport("user32.dll", SetLastError=true)] private static extern IntPtr GetWindowLong( IntPtr hWnd, int nIndex );
        [DllImport("user32.dll")] private static extern IntPtr SetWindowLong( IntPtr hWnd, int nIndex, IntPtr dwNewLong );
        */
        #endregion

        private bool EnumChildWindows_Routine( IntPtr hWnd, IntPtr lParam )
        {
            #region comm.
            /*
            var style = GetWindowLong( hWnd, GWL_STYLE ).ToInt64();
            style &= ~ES_NOHIDESEL;
            SetWindowLong( hWnd, GWL_STYLE, new IntPtr( style ) );
            */
            #endregion

            var buf = new StringBuilder( 0x400 );
            var len = GetClassName( hWnd, buf, buf.Capacity );
            if ( (1 < len) && buf.ToString( 0, len /*- 1*/ ).ToUpperInvariant().Contains( "EDIT" ) /*"WindowsForms10.EDIT.app.0.1983833_r8_ad1"*/ )
            {
                OverriddenNativeWindow.AssignHWnd( hWnd );
            }            
            return (true);
        }
    }
}
