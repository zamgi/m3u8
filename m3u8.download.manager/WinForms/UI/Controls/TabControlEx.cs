using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal class TabControlEx : TabControl
    {
        /// <summary>
        /// 
        /// </summary>
        public delegate Color GetForecolorForTabPageTextDelegate( TabPage tabPage );

        public GetForecolorForTabPageTextDelegate GetForecolorForTabPageText { get; set; }

        public TabControlEx() => this.DrawMode = TabDrawMode.OwnerDrawFixed;

        #region [.hide win-api focus-rect.]
        protected override bool ShowFocusCues => false;
        protected override bool ShowKeyboardCues => false;

        //public override Rectangle DisplayRectangle
        //{
        //    get
        //    {
        //        const int OFF = 1;
        //        var rc = base.DisplayRectangle;
        //        return (new Rectangle( rc.Left - OFF, rc.Top - OFF, rc.Width + 2*OFF, rc.Height + 2*OFF ));
        //    }
        //}
        protected override void WndProc( ref Message m )
        {            
            // Это сообщение обычно скрывает фокус после клика мышью.
            // Мы его игнорируем, если оно пытается скрыть индикаторы.
            if ( m.Msg == WM_UPDATEUISTATE )
            {
                // Устанавливаем флаг UISF_HIDEFOCUS (1) в LowOrder Word
                m.WParam = (IntPtr) MAKE_LONG( UISF_HIDEFOCUS, UIS_SET | UIS_CLEAR );
            }
            base.WndProc( ref m );
        }

        private const int WM_CHANGEUISTATE = 0x0127;
        private const int WM_UPDATEUISTATE = 0x0128;
        private const int UIS_SET          = 1;
        private const int UISF_HIDEFOCUS   = 0x1;
        private const int UIS_CLEAR        = 2;

        [DllImport("user32.dll")] private static extern IntPtr SendMessage( IntPtr hWnd, int msg, int wParam, int lParam );

        protected override void OnGotFocus( EventArgs e )
        {
            base.OnGotFocus( e );
            // Принудительно ОЧИЩАЕМ флаг HIDEFOCUS (UIS_CLEAR = 2)
            // Это заставляет Windows рисовать рамку даже после клика мышью
            SendMessage( this.Handle, WM_CHANGEUISTATE, MAKE_LONG( UISF_HIDEFOCUS, UIS_SET | UIS_CLEAR ), 0 );
            SendMessage( this.Handle, WM_UPDATEUISTATE, MAKE_LONG( UISF_HIDEFOCUS, UIS_SET | UIS_CLEAR ), 0 );
        }
        private static int MAKE_LONG( int low, int high ) => (low & 0xffff) | ((high & 0xffff) << 16);
        #endregion

        protected override void OnDrawItem( DrawItemEventArgs e )
        {
            //base.OnDrawItem( e );

            var tabControl = this;
            var tabPage    = tabControl.TabPages[ e.Index ];

            var gr = e.Graphics;
            var rc = e.Bounds;

            #region [.background.]
            using var br = new SolidBrush( tabPage.BackColor );
            gr.FillRectangle( br, rc );
            #endregion

            #region [.draw icon image.]
            ImageList imageList;
            if ( (tabPage.ImageIndex != -1) && ((imageList = tabControl.ImageList) != null) )
            {
                var y_off = (rc.Height - imageList.ImageSize.Height) / 2;
                var y     = rc.Y + y_off;
                var x_off = y_off; // (rc.Width - imageList.ImageSize.Width) / 2;
                var x     = rc.X + x_off;
                imageList.Draw( gr, x, y, tabPage.ImageIndex );

                var off = x_off + imageList.ImageSize.Width;
                rc.X     += off;
                rc.Width -= off;
            }
            #endregion

            #region [.drawtab text.]
            var color = GetForecolorForTabPageText?.Invoke( tabPage ) ?? tabControl.ForeColor;
            using var fbr = new SolidBrush( color );
            //var color = (tabPage == parallelismTabPage) ? Brushes.DarkOliveGreen 
            //                                            : ((tabPage == otherTabPage) ? Brushes.DarkOrange 
            //                                                                         : ((tabPage == webProxyTabPage) ? Brushes.Blue : Brushes.Silver));

            using var sf = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center, Trimming = StringTrimming.None, FormatFlags = StringFormatFlags.NoWrap };
            //var fs = ((tabPage == moreTabPage) || ((e.State & DrawItemState.Selected) != DrawItemState.Selected)) ? FontStyle.Regular : FontStyle.Underline;
            //using var ft = new Font( tabPage.Font, fs );
            gr.DrawString( tabPage.Text, /*ft*/tabPage.Font, fbr, rc, sf );
            #endregion

            #region [.draw focus rect.]
            if ( (e.State & DrawItemState.Selected) == DrawItemState.Selected )
            {
                rc = e.Bounds; rc.Inflate( -3, -3 );
                ControlPaint.DrawFocusRectangle( gr, rc );
                //gr.DrawRectangle( Pens.Red, rc );
            }
            #endregion
        }
    }
}
