using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

//using M = System.Runtime.CompilerServices.MethodImplAttribute;
//using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal class NumericUpDownEx : NumericUpDown
    {
        private StringBuilder _Buf;
        public NumericUpDownEx() => _Buf = new StringBuilder();

        //public new int DecimalPlaces
        //{
        //    [M(O.AggressiveInlining)] get => base.DecimalPlaces;
        //    set
        //    {
        //        base.DecimalPlaces = value;
        //        this.InitialDecimalPlaces = value;
        //    }
        //}

        //private int? _InitialDecimalPlaces;
        //private int InitialDecimalPlaces 
        //{ 
        //    get => _InitialDecimalPlaces.GetValueOrDefault( base.DecimalPlaces );
        //    set
        //    {
        //        _InitialDecimalPlaces = value;

        //        this.ValueChanged -= NumericUpDownEx_ValueChanged;
        //        if ( value != 0 )
        //        {
        //            this.ValueChanged += NumericUpDownEx_ValueChanged;
        //        }
        //    }
        //}

        ////private static (int trimLength, bool trimDecimalSeparator) TrimEndDecimalPlaces( string s, char decimalSeparator )
        ////{
        ////    var trimLength           = 0;
        ////    var trimDecimalSeparator = false;
        ////    for ( var i = s.Length - 1; 0 <= i; i-- )
        ////    {
        ////        var ch = s[ i ];
        ////        if ( ch != '0' )
        ////        {
        ////            if ( ch == '.' || ch == ',' )
        ////            {
        ////                trimDecimalSeparator = true;
        ////            }
        ////            break;
        ////        }
        ////        trimLength++;
        ////    }
        ////    return (trimLength, trimDecimalSeparator);
        ////}
        //private static (int trimLength, bool trimDecimalSeparator) TrimEndDecimalPlaces( string s, string decimalSeparator )
        //{
        //    var sep_idx = s.IndexOf( decimalSeparator );
        //    if ( sep_idx == -1 ) return (0, false);

        //    var trimLength           = 0;
        //    var trimDecimalSeparator = false;
        //    for ( var i = s.Length - 1; sep_idx <= i; i-- )
        //    {
        //        var ch = s[ i ];
        //        if ( ch != '0' )
        //        {
        //            trimDecimalSeparator = (i == sep_idx);
        //            break;
        //        }
        //        trimLength++;
        //    }
        //    return (trimLength, trimDecimalSeparator);
        //}
        //private static (int trimLength, bool trimDecimalSeparator) TrimEndDecimalPlaces( string s, int decimalSeparatorIndex )
        //{
        //    if ( decimalSeparatorIndex == -1 ) return (0, false);

        //    var trimLength           = 0;
        //    var trimDecimalSeparator = false;
        //    for ( var i = s.Length - 1; decimalSeparatorIndex <= i; i-- )
        //    {
        //        var ch = s[ i ];
        //        if ( ch != '0' )
        //        {
        //            trimDecimalSeparator = (i == decimalSeparatorIndex);
        //            break;
        //        }
        //        trimLength++;
        //    }
        //    return (trimLength, trimDecimalSeparator);
        //}
        //private static (int count, int decimalSeparatorIndex) GetDigitCountAfterDecimalSeparator( string s, string decimalSeparator )
        //{
        //    var sep_idx = s.IndexOf( decimalSeparator );
        //    if ( sep_idx == -1 ) return (0, -1);

        //    var cnt = s.Length - sep_idx - 1;
        //    return (cnt, sep_idx);
        //}
        //private void Set_DecimalPlaces_IfChanged( int dp )
        //{
        //    if ( base.DecimalPlaces != dp ) base.DecimalPlaces = dp;
        //}
        //private void NumericUpDownEx_ValueChanged( object sender, EventArgs e )
        //{
        //    var idp = this.InitialDecimalPlaces;
        //    if ( idp == 0 )
        //    {
        //        this.ValueChanged -= NumericUpDownEx_ValueChanged;
        //        return;
        //    }

        //    var v     = this.Value;
        //    var v_i32 = (int) v;
        //    if ( v == v_i32 )
        //    {
        //        Set_DecimalPlaces_IfChanged( 0 );
        //    }
        //    else
        //    {
        //        var v_txt = v.ToString();
        //        var (count, decimalSeparatorIndex) = GetDigitCountAfterDecimalSeparator( v_txt, Application.CurrentCulture.NumberFormat.NumberDecimalSeparator );
        //            v_txt = v_txt.PadRight( v_txt.Length + Math.Max( 0, idp - count ), '0' );
        //        var (trimLength, trimDecimalSeparator) = TrimEndDecimalPlaces( v_txt, decimalSeparatorIndex );
        //        if ( 0 < trimLength )
        //        {
        //            Set_DecimalPlaces_IfChanged( trimDecimalSeparator ? 0 : Math.Max( 0, idp - trimLength ) );
        //        }
        //        else
        //        {
        //            Set_DecimalPlaces_IfChanged( idp );
        //        }
        //    }
        //}


        protected override void UpdateEditText()
        {
            // If we're initializing, we don't want to update the edit text yet,
            // just in case the value is invalid.
            //if ( _initializing )
            //{
            //    return;
            //}

            // If the current value is user-edited, then parse this value before reformatting
            if ( UserEdit )
            {
                ParseEditText();
            }

            //---base.UpdateEditText();

            // Verify that the user is not starting the string with a "-"
            // before attempting to set the Value property since a "-" is a valid character with
            // which to start a string representing a negative number.
            var txt = this.Text;
            if ( !string.IsNullOrEmpty( txt ) && !(txt.Length == 1 && txt == "-") )
            {
                this.ChangingText = true;

                this.Text = GetNumberText( this.Value );
            }
        }
        private string GetNumberText( decimal d )
        {
            string txt;
            if ( this.Hexadecimal )
            {
                txt = ((long) d).ToString( "X", CultureInfo.InvariantCulture );
            }
            else
            {
                //---txt = d.ToString( $"{(this.ThousandsSeparator ? "N" : "F")}{base.DecimalPlaces}", CultureInfo.CurrentCulture );
                txt = d.ToString( $"F{base.DecimalPlaces}", CultureInfo.CurrentCulture );
                txt = TrimEndDecimalPlaces( txt, Application.CurrentCulture.NumberFormat.NumberDecimalSeparator, _Buf );
            }
            return (txt);
        }
        private static string TrimEndDecimalPlaces( string s, string decimalSeparator, StringBuilder buf )
        {
            var sep_idx = s.IndexOf( decimalSeparator );
            if ( sep_idx == -1 ) return (s);

            var trimLength           = 0;
            var trimDecimalSeparator = false;
            for ( var i = s.Length - 1; sep_idx <= i; i-- )
            {
                var ch = s[ i ];
                if ( ch != '0' )
                {
                    trimDecimalSeparator = (i == sep_idx);
                    break;
                }
                trimLength++;
            }
            var n_s = buf.Clear().Append( s, 0, s.Length - trimLength - (trimDecimalSeparator ? 1 : 0) ).ToString();
            return (n_s);
        }


        public decimal? Increment_MouseWheel { get; set; }
        public bool     Round2NextTenGroup   { get; set; }
        public int      ValueAsInt32         { get => (int) this.Value; set => this.Value = value; }
        protected override void OnMouseWheel( MouseEventArgs e )
        {
            if ( e is HandledMouseEventArgs hme )
            {
                hme.Handled = true;
            }

            if ( 0 < e.Delta )
            {
                var v = GetValue( 1 );
                this.Value = Math.Max( this.Minimum,  Math.Min( this.Maximum, v ) );
            }
            else if ( e.Delta < 0 )
            {
                var v = GetValue( -1 );
                this.Value = Math.Min( this.Maximum,  Math.Max( this.Minimum, v ) );
            }
        }
        public void Set_Increment_MouseWheel( int v ) => Increment_MouseWheel = v;
        private decimal GetValue( int sign ) => GetValue( this.Value, sign * Increment_MouseWheel.GetValueOrDefault( this.Increment ), Round2NextTenGroup );
        private static decimal GetValue( decimal value, decimal increment, bool round2NextTenGroup )
        {
            var d = value + increment;
            if ( round2NextTenGroup )
            {
                var tenGroupCnt = 0;
                for ( int i = (int) Math.Abs( increment ); 1 < i; tenGroupCnt++ )
                {
                    i /= 10;
                }

                var tenGroupOrder = Math.Max( 1, tenGroupCnt ) * 10;
                d = ((int) (d + tenGroupOrder / 2) / tenGroupOrder) * tenGroupOrder;
            }
            return (d);
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
            private OverriddenNativeWindow() { }
            public static void AssignHWnd( IntPtr hWnd ) => new OverriddenNativeWindow().AssignHandle( hWnd );

            protected override void WndProc( ref Message m )
            {
                const int WM_NCHITTEST  = 0x0084;
                const int WM_SETFOCUS   = 0x0007;
                const int HTTRANSPARENT = -1;

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

        public NumericUpDownEx_Transparent() { }

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
