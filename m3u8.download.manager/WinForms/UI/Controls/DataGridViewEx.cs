using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class DataGridViewEx : DataGridView
    {
        public DataGridViewEx() : base()
        {
            this.SetDoubleBuffered( true );

            _ScrollIfNeedTimer = new Timer() { Interval = ScrollDelayInMilliseconds, Enabled = false };
            _ScrollIfNeedTimer.Tick += ScrollIfNeedTimer_Tick;
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class DataGridViewRow_Comparer : IComparer< DataGridViewRow >
        {
            public static DataGridViewRow_Comparer Inst { get; } = new DataGridViewRow_Comparer();
            private DataGridViewRow_Comparer() { }
            public int Compare( DataGridViewRow x, DataGridViewRow y ) => (x.Index - y.Index);
        }

        public event EventHandler StartDrawSelectRect;
        public event EventHandler EndDrawSelectRect;

        #region [.draw select rows rect.]
        private bool _DrawSelectRect;
        private Point _SelectLocation;
        private int _SelectVerticalScrollingOffset;
        private Rectangle _SelectRect;
        private SortedSet< DataGridViewRow > _SelectedRows;
        private Timer _ScrollIfNeedTimer;

        protected override void OnMouseDown( MouseEventArgs e )
        {
            base.OnMouseDown( e );

            if ( this.HitTest( e.X, e.Y ).Type == DataGridViewHitTestType.None )
            {
                this.ClearSelection();
                //this.CurrentCell = null;

                _DrawSelectRect = (e.Button == MouseButtons.Left);
                _SelectVerticalScrollingOffset = this.VerticalScrollingOffset;
                _SelectLocation = e.Location;
                _SelectRect = Rectangle.Empty;
                _SelectedRows?.Clear();
                StartDrawSelectRect?.Invoke( this, EventArgs.Empty );

                _ScrollIfNeedTimer.Interval = ScrollDelayInMilliseconds;
                _ScrollIfNeedTimer.Enabled  = true;
            }
            else 
            {
                _DrawSelectRect = false;
            }
        }
        protected override void OnMouseUp( MouseEventArgs e )
        {
            base.OnMouseUp( e );

            if ( _DrawSelectRect )
            {
                _ScrollIfNeedTimer.Enabled = false;
                _DrawSelectRect = false;
                _SelectedRows?.Clear();
                this.Invalidate();
                EndDrawSelectRect?.Invoke( this, EventArgs.Empty );                
            }
        }
        protected override void OnMouseMove( MouseEventArgs e )
        {            
            base.OnMouseMove( e );

            ProcessDrawSelectRect( e.X, e.Y );
        }
        private void ProcessDrawSelectRect( int X, int Y )
        {
            if ( _DrawSelectRect )
            {
                #region [.1.]
                var selectedRows = new SortedSet< DataGridViewRow >( DataGridViewRow_Comparer.Inst );
                for ( int i = Math.Max( 0, this.FirstDisplayedScrollingRowIndex ), len = this.RowCount, disp_i = 0, disp_count = this.DisplayedRowCount( true ); i < len; i++ )
                {
                    var row = this.Rows[ i ];
                    if ( row.Displayed )
                    {
                        disp_i++;
                        if ( _SelectRect.IntersectsWith( this.GetRowDisplayRectangle( i, false ) ) )
                        {
                            selectedRows.Add( row );
                        }
                    }
                    else if ( disp_count <= disp_i )
                    {
                        break;
                    }                    
                }
                for ( var i = this.SelectedRows.Count - 1; 0 <= i; i-- )
                {
                    var row = this.SelectedRows[ i ];
                    if ( !row.Displayed /*&& row.Visible*/ && row.Selected )
                    {
                        selectedRows.Add( row );
                    }
                }
                #region comm. prev.
                /*
                var selectedRows = new SortedSet< DataGridViewRow >( DataGridViewRow_Comparer.Inst );
                for ( int y = _SelectRect.Y, end_y = _SelectRect.Bottom; y < end_y; y += 10 )
                {
                    var hti = this.HitTest( _SelectRect.X, y );
                    if ( hti.Type == DataGridViewHitTestType.Cell )
                    {
                        var row = this.Rows[ hti.RowIndex ];
                        selectedRows.Add( row );
                    }
                }
                //*/ 
                #endregion

                if ( !_SelectedRows.IsEqual( selectedRows ) )
                {
                    this.ClearSelection();
                    if ( 0 < selectedRows.Count )
                    {
                        var move_to_up = (Y < _SelectLocation.Y);
                        var row_min = selectedRows.Min;
                        var row_max = selectedRows.Max;

                        this.CurrentCell = (move_to_up ? row_min : row_max).Cells[ 0 ];
                        for ( int i = row_min.Index, end_i = row_max.Index; i <= end_i; i++ )
                        {
                            var row = this.Rows[ i ];
                            if ( row.Visible )
                            {
                                row.Selected = true;
                            }
                        }
                    }
                    _SelectedRows = selectedRows;
                    _SelectRect   = Rectangle.Empty;
                    this.Invalidate();
                    this.Update();
                }
                #endregion

                #region [.2.]
                using ( var gr = Graphics.FromHwnd( this.Handle ) )
                {
                    gr.DrawXORRectangle( _SelectRect, Color.DimGray );

                    var y0 = _SelectLocation.Y - (this.VerticalScrollingOffset - _SelectVerticalScrollingOffset);
                    var x = Math.Min( X, _SelectLocation.X );
                    var y = Math.Min( Y, y0 );
                    _SelectRect = new Rectangle( x, y, Math.Abs( X - _SelectLocation.X ), Math.Abs( Y - y0 ) );
                    gr.DrawXORRectangle( _SelectRect, Color.DimGray );
                }
                #endregion
            }
        }
        private void ScrollIfNeedTimer_Tick( object sender, EventArgs e )
        {
            var pt = this.PointToClient( Control.MousePosition );
            if ( this.ScrollIfNeed( in pt ) )
            {
                ProcessDrawSelectRect( pt.X, pt.Y );
            }
        }
        #endregion

        #region [.Scroll if need to point.]
        private const int SCROLL_DELAY_IN_MILLISECONDS = 10;
        public int ScrollDelayInMilliseconds { get; set; } = SCROLL_DELAY_IN_MILLISECONDS;

        private DateTime _LastScrollDateTime;
        public bool ScrollIfNeed( in Point pt )
        {
            if ( ShouldScrollUp( in pt ) )
            {
                if ( (0 < this.FirstDisplayedScrollingRowIndex) && (TimeSpan.FromMilliseconds( ScrollDelayInMilliseconds ) < (DateTime.Now - _LastScrollDateTime)) )
                {
                    this.FirstDisplayedScrollingRowIndex--;
                    _LastScrollDateTime = DateTime.Now;
                    return (true);
                }
            }
            else if ( ShouldScrollDown( in pt ) )
            {
                if ( (this.FirstDisplayedScrollingRowIndex < (this.RowCount - 1)) && (TimeSpan.FromMilliseconds( ScrollDelayInMilliseconds ) < (DateTime.Now - _LastScrollDateTime)) )
                {
                    this.FirstDisplayedScrollingRowIndex++;
                    _LastScrollDateTime = DateTime.Now;
                    return (true);
                }
            }
            return (false);
        }
        [M(O.AggressiveInlining)] private bool ShouldScrollUp( in Point pt )
        {
            var columnHeadersHeight = this.ColumnHeadersHeight;
            return //pt.Y > columnHeadersHeight && //---remove column-up-edge---//
                   pt.Y < columnHeadersHeight + 15
                && pt.X >= 0
                && pt.X <= this.Bounds.Width;
        }
        [M(O.AggressiveInlining)] private bool ShouldScrollDown( in Point pt )
        {
            var bounds = this.Bounds;
            return pt.Y > bounds.Height - 15
                && pt.Y < bounds.Height
                && pt.X >= 0
                && pt.X <= bounds.Width;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class Gdi32
    {
        /// <summary>
        /// 
        /// </summary>
        private enum DrawingMode : int
        {
            R2_BLACK           = 1,   /*  0       */
            R2_NOTMERGEPEN     = 2,   /* DPon     */
            R2_MASKNOTPEN      = 3,   /* DPna     */
            R2_NOTCOPYPEN      = 4,   /* PN       */
            R2_MASKPENNOT      = 5,   /* PDna     */
            R2_NOT             = 6,   /* Dn       */
            R2_XORPEN          = 7,   /* DPx      */
            R2_NOTMASKPEN      = 8,   /* DPan     */
            R2_MASKPEN         = 9,   /* DPa      */
            R2_NOTXORPEN       = 10,  /* DPxn     */
            R2_NOP             = 11,  /* D        */
            R2_MERGENOTPEN     = 12,  /* DPno     */
            R2_COPYPEN         = 13,  /* P        */
            R2_MERGEPENNOT     = 14,  /* PDno     */
            R2_MERGEPEN        = 15,  /* DPo      */
            R2_WHITE           = 16,  /*  1       */
        }

        private const string GDI32_DLL = "gdi32.dll";
        [DllImport(GDI32_DLL)] private static extern bool Rectangle( IntPtr hDC, int left, int top, int right, int bottom );
        [DllImport(GDI32_DLL)] private static extern int SetROP2( IntPtr hDC, /*int*/ DrawingMode fnDrawMode );
        [DllImport(GDI32_DLL)] private static extern bool MoveToEx( IntPtr hDC, int x, int y, ref Point p );
        [DllImport(GDI32_DLL)] private static extern bool LineTo( IntPtr hdc, int x, int y );
        [DllImport(GDI32_DLL)] private static extern IntPtr CreatePen( int fnPenStyle, int nWidth, int crColor );
        [DllImport(GDI32_DLL)] private static extern IntPtr SelectObject( IntPtr hDC, IntPtr hObj );
        [DllImport(GDI32_DLL)] private static extern bool DeleteObject( IntPtr hObj );

        [M(O.AggressiveInlining)] private static int ArgbToRGB( int rgb ) => ((rgb >> 16 & 0x0000FF) | (rgb & 0x00FF00) | (rgb << 16 & 0xFF0000));
        public static void DrawXORRectangle( this Graphics graphics, in Rectangle rc, Color color, int penWidth = 2 )
        {
            IntPtr hDC = graphics.GetHdc();
            IntPtr hPen = CreatePen( 0, penWidth, ArgbToRGB( color.ToArgb() ) );
            SelectObject( hDC, hPen );
            SetROP2( hDC, DrawingMode.R2_NOTXORPEN );
            Rectangle( hDC, rc.Left, rc.Top, rc.Right, rc.Bottom );
            DeleteObject( hPen );
            graphics.ReleaseHdc( hDC );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class Extensions
    {
        public static void SetDoubleBuffered< T >( this T t, bool doubleBuffered ) where T : Control
        {          
            //Control.DoubleBuffered = true;
            var field = typeof(T).GetProperty( "DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance );
            field?.SetValue( t, doubleBuffered );
        }

        [M(O.AggressiveInlining)] public static bool IsEqual< T >( this ISet< T > first, ISet< T > second )
        {
            if ( first == null )
            {
                return (second == null) || (second.Count == 0);
            }
            if ( second == null )
            {
                return (first.Count == 0);
            }
            if ( first.Count != second.Count )
            {
                return (false);
            }
            //return (first.SequenceEqual( second ));
            //return (!first.Except( second ).Any() /*&& !second.Except( first ).Any()*/);
            foreach ( var t in first )
            {
                if ( !second.Contains( t ) )
                {
                    return (false);
                }
            }
            return (true);
        }
    }
}
