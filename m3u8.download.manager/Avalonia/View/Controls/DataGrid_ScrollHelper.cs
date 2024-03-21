using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal static class DataGrid_ScrollHelper
    {
        public static DataGrid_ScrollHelper< T > Create< T >( DataGrid dgv, IReadOnlyList< T > model ) => new DataGrid_ScrollHelper< T >( dgv, model );
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class DataGrid_ScrollHelper< T >
    {
        private DataGrid DGV;
        private IReadOnlyList< T > _Model;
        public DataGrid_ScrollHelper( DataGrid dgv, IReadOnlyList< T > model ) => (DGV, _Model) = (dgv, model);

        public void SetModel( IReadOnlyList< T > model ) => _Model = model;


        private double? _DGV_ColumnHeadersHeight;
        private double DGV_ColumnHeadersHeight
        {
            [M(O.AggressiveInlining)] get
            {
                if ( !_DGV_ColumnHeadersHeight.HasValue )
                {
                    _DGV_ColumnHeadersHeight = DGV.GetVisualDescendants().OfType< DataGridColumnHeader >().Select( c => c.Bounds.Height ).First( h => h != 0 ); //DGV.ColumnHeadersHeight;
                }
                return (_DGV_ColumnHeadersHeight.Value);
            }
        }
        
        [M(O.AggressiveInlining)] private static bool TryGetTopDataGridRowIndex( DataGrid dgv, out int rowIndex )
        {
            var seq_1 = dgv.GetVisualDescendants().OfType< DataGridRow >().Where( r => r.IsVisible ).Select( r => (row: r, idx: r.GetIndex(), r.Bounds) )
                           .OrderBy( t => t.idx ).ThenBy( t => t.Bounds.Top )
                           .ToList() 
                           ;

            var seq_2 = seq_1.Select( (t, i) => (t, d: (i < seq_1.Count - 1) ? (seq_1[ i + 1 ].idx - seq_1[ i ].idx) : 0) )
#if DEBUG
                             .ToList()
#endif
                             ;
            #region [.descr.]
            /*
+		[0]	({Avalonia.Controls.DataGridRow},  0, {0,   0, 1244, 31} )
+		[1]	({Avalonia.Controls.DataGridRow},  6, {0, -24, 1244, 31} )
+		[2]	({Avalonia.Controls.DataGridRow},  7, {0,   7, 1244, 31} ) (+) need get first non-negative-top after negative-top dgrow (not first in this list)
+		[3]	({Avalonia.Controls.DataGridRow},  8, {0,  38, 1244, 31} )
+		[4]	({Avalonia.Controls.DataGridRow},  9, {0,  69, 1244, 31} )
+		[5]	({Avalonia.Controls.DataGridRow}, 10, {0, 100, 1244, 31} )
+		[6]	({Avalonia.Controls.DataGridRow}, 11, {0, 131, 1244, 31} )
+		[7]	({Avalonia.Controls.DataGridRow}, 12, {0, 162, 1244, 31} )
             */
            #endregion

            var ft = seq_2.FirstOrDefault( t => t.d == 1 );
            if ( ft.t.row != null )
            {
                rowIndex = ft.t.idx;// - 1;
                return (true);
            }
            rowIndex = default;
            return (false);
        }
        [M(O.AggressiveInlining)] private static bool TryGetBottomDataGridRowIndex( DataGrid dgv, out int rowIndex )
        {
            var seq_1 = dgv.GetVisualDescendants().OfType< DataGridRow >().Where( r => r.IsVisible ).Select( r => (row: r, idx: r.GetIndex(), r.Bounds) )
                           .OrderByDescending( t => t.idx ).ThenByDescending( t => t.Bounds.Top )
                           .ToList()
                           ;

            var seq_2 = seq_1.Select( (t, i) => (t, d: (i < seq_1.Count - 1) ? (seq_1[ i ].idx - seq_1[ i + 1 ].idx) : 0) )
#if DEBUG
                             .ToList()
#endif
                             ;

            var ft = seq_2.FirstOrDefault( t => t.d == 1 );
            if ( ft.t.row != null )
            {
                rowIndex = ft.t.idx;// + 1;
                return (true);
            }
            rowIndex = default;
            return (false);
        }


        #region [.Scroll if need to point.]
        private const int SCROLL_DELAY_IN_MILLISECONDS = 20;
        public int ScrollDelayInMilliseconds { get; set; } = SCROLL_DELAY_IN_MILLISECONDS;


        private DateTime _LastScrollDateTime;
        [M(O.AggressiveInlining)] private bool IsTimeElapsed4Scroll() => (TimeSpan.FromMilliseconds( ScrollDelayInMilliseconds ) < (DateTime.Now - _LastScrollDateTime));
        [M(O.AggressiveInlining)] public bool MakeScroll_IfNeed_UseScrollDelay( in Point pt ) => MakeScroll_IfNeed( pt, true );
        [M(O.AggressiveInlining)] public bool MakeScroll_IfNeed_Now( in Point pt ) => MakeScroll_IfNeed( pt, false );
        public bool MakeScroll_IfNeed( in Point pt, bool useScrollDelay )
        {
            if ( _Model == null ) return (false);

            if ( IsNeed_ScrollDown( pt ) )
            {
                var last_idx = _Model.Count - 1;
                if ( TryGetBottomDataGridRowIndex( DGV, out var bottomIdx ) && (bottomIdx <= last_idx) && (!useScrollDelay || IsTimeElapsed4Scroll()) )
                {
                    if ( bottomIdx < last_idx ) bottomIdx++;
                    DGV.ScrollIntoView( _Model[ bottomIdx ], null );
                    _LastScrollDateTime = DateTime.Now;
                    return (true);
                }
            }            
            else if ( IsNeed_ScrollUp( pt ) )
            {
                if ( TryGetTopDataGridRowIndex( DGV, out var topIdx ) && (0 <= topIdx) && (!useScrollDelay || IsTimeElapsed4Scroll()) )
                {
                    if ( 0 < topIdx ) topIdx--;
                    DGV.ScrollIntoView( _Model[ topIdx ], null );
                    _LastScrollDateTime = DateTime.Now;
                    return (true);
                }
            }
            return (false);
        }        


        private const int HEIGHT_THRESHOLD = 15;
        [M(O.AggressiveInlining)] private bool IsNeed_ScrollUp( in Point pt ) => (pt.Y < (this.DGV_ColumnHeadersHeight + HEIGHT_THRESHOLD)) && (0 <= pt.X);
        [M(O.AggressiveInlining)] private bool IsNeed_ScrollDown( in Point pt ) => ((DGV.Bounds.Height - HEIGHT_THRESHOLD) < pt.Y) && (0 <= pt.X);
        #endregion
    }
}
