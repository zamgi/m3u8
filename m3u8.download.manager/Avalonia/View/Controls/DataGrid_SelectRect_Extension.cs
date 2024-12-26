using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;

using _Timer_ = System.Timers.Timer;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal static class DataGrid_SelectRect_Extension
    {
        public static DataGrid_SelectRect_Extension< T > Create< T >( Visual topVisual, DataGrid dgv, Rectangle selectRect
            , DataGrid_SelectRect_Extension< T >.Allow_Begin_SelectRect_Delegate allow_Begin_SelectRect = null, IReadOnlyList< T > model = null )
            => new DataGrid_SelectRect_Extension< T >( topVisual, dgv, selectRect, allow_Begin_SelectRect, model );
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class DataGrid_SelectRect_Extension< T >
    {
        public delegate bool Allow_Begin_SelectRect_Delegate( DataGridCellPointerPressedEventArgs e );

        #region [.field's.]
        private DataGrid           DGV;
        private Rectangle          _SelectRect;
        private Visual             _TopVisual;
        private _Timer_            _ScrollIfNeedTimer;
        private IReadOnlyList< T > _Model;
        private Allow_Begin_SelectRect_Delegate _Allow_Begin_SelectRect;
        private DataGrid_ScrollHelper< T >      _ScrollHelper;

        public DataGrid_SelectRect_Extension( Visual topVisual, DataGrid dgv, Rectangle selectRect, Allow_Begin_SelectRect_Delegate allow_Begin_SelectRect = null, IReadOnlyList< T > model = null )
        {
            _TopVisual  = topVisual  ?? throw (new ArgumentNullException( nameof(topVisual) ));
            DGV         = dgv        ?? throw (new ArgumentNullException( nameof(dgv) ));
            _SelectRect = selectRect ?? throw (new ArgumentNullException( nameof(selectRect) ));
            _Allow_Begin_SelectRect = allow_Begin_SelectRect ?? new Allow_Begin_SelectRect_Delegate( e => false );
            _ScrollHelper = DataGrid_ScrollHelper.Create( DGV, model );
            SetModel( model );

            var scrollIfNeedTimer_Elapsed_Action = new Action( _ScrollIfNeedTimer_Elapsed );
            _ScrollIfNeedTimer = new _Timer_() { Interval = ScrollDelayInMilliseconds, AutoReset = true, Enabled = false };
            _ScrollIfNeedTimer.Elapsed += async (_, _) => await Dispatcher.UIThread.InvokeAsync( scrollIfNeedTimer_Elapsed_Action );
        }
        #endregion

        public bool IsInDrawSelectRect => _IsPointerPressed;
        public void SetModel( IReadOnlyList< T > model )
        {
            _Model = model;
            _ScrollHelper.SetModel( _Model );

            DGV.CellPointerPressed -= DGV_CellPointerPressed;
            DGV.PointerPressed     -= DGV_PointerPressed;

            if ( _Model != null )
            {
                DGV.CellPointerPressed += DGV_CellPointerPressed;
                DGV.PointerPressed     += DGV_PointerPressed;
            }
        }

        #region [.DGV events.]
        private ScrollBar __VScrollBar__;
        private ScrollBar _VScrollBar => __VScrollBar__ ??= _SelectRect.GetVisualParent().GetVisualDescendants().OfType< ScrollBar >().First( b => b.Orientation == Orientation.Vertical );
        private double GetScrollY() => _VScrollBar.IsVisible ? _VScrollBar.Value : 0;


        /// <summary>
        /// 
        /// </summary>
        private enum MoveDirectionEnum { __UNDEFINE__, Up, Down }

        private bool   _IsPointerPressed;
        private bool   _IsPre_Begin_SelectRect;
        private Point  _Pre_Begin_SelectRect_Point;
        private Point  _Press_Pos;
        private (Point Pos, MoveDirectionEnum MoveDirection)? _LastMove;
        private Rect   _Grid_Bounds;
        private double _ScrollPos_Y;
        private Thickness _ParentLayoutMargin;

        private void DGV_CellPointerPressed( object sender, DataGridCellPointerPressedEventArgs e )
        {
            _IsPre_Begin_SelectRect = false;

            var pargs = e.PointerPressedEventArgs;
            var p     = pargs.GetCurrentPoint( _TopVisual );
            switch ( p.Properties.PointerUpdateKind )
            {
                case PointerUpdateKind.LeftButtonPressed:
                    if ( !pargs.Handled && _Allow_Begin_SelectRect( e ) )
                    {
                        //---Begin_SelectRect( pargs, p.Position );
                        _IsPre_Begin_SelectRect = true;
                        Pre_Begin_SelectRect( pargs, p.Position );
                    }
                    break;
            }
        }
        private async void DGV_PointerPressed( object sender, PointerPressedEventArgs e )
        {
            if ( e.Handled ) return;

            var p = e.GetCurrentPoint( _TopVisual );
            switch ( p.Properties.PointerUpdateKind )
            {
                case PointerUpdateKind.LeftButtonPressed:
                    var columnHeader = (e.Source as Control)?.GetSelfAndVisualAncestors().OfType< DataGridColumnHeader >().FirstOrDefault();
                    if ( columnHeader == null )
                    {
                        Begin_SelectRect( e, p.Position );
                    }
                    else if ( DGV.Columns[ 0 ]?.Header == columnHeader.Content )
                    {
                        DGV.SelectedItems.Clear();
                        await Task.Delay( 100 );
                        DGV.SelectAll();
                    }
                    break;
            }
        }

        private void DGV_PointerMoved( object sender, PointerEventArgs e )
        {
            const double MOVE_THRSHOLD = 3;
            if ( _IsPre_Begin_SelectRect )
            {
                var pt = e.GetPosition( _TopVisual );
                if ( MOVE_THRSHOLD < Math.Abs( pt.X - _Pre_Begin_SelectRect_Point.X ) || 
                     MOVE_THRSHOLD < Math.Abs( pt.Y - _Pre_Begin_SelectRect_Point.Y ) 
                   )
                {
                    _IsPre_Begin_SelectRect = false;
                    Begin_SelectRect( pt );
                }
            }
            else if ( _IsPointerPressed )
            {
                Move_SelectRect( e.GetPosition( _TopVisual ) );
            }
        }
        private void DGV_PointerReleased( object sender, PointerReleasedEventArgs e )
        {
            if ( _IsPre_Begin_SelectRect )
            {
                _IsPre_Begin_SelectRect = false;
            }
            /*else*/ if ( _IsPointerPressed )
            {
                End_SelectRect( e );
            }
        }

        private void Pre_Begin_SelectRect( PointerPressedEventArgs e, in Point pt )
        {
            DGV.PointerMoved    -= DGV_PointerMoved;
            DGV.PointerReleased -= DGV_PointerReleased;

            DGV.PointerMoved    += DGV_PointerMoved;
            DGV.PointerReleased += DGV_PointerReleased;
            //---e.Handled = true;

            _Pre_Begin_SelectRect_Point = pt;
        }
        private void Begin_SelectRect( PointerPressedEventArgs e, in Point pt )
        {
            DGV.PointerMoved    -= DGV_PointerMoved;
            DGV.PointerReleased -= DGV_PointerReleased;

            DGV.PointerMoved    += DGV_PointerMoved;
            DGV.PointerReleased += DGV_PointerReleased;
            e.Handled = true;

            Begin_SelectRect( pt );
        }
        private void Begin_SelectRect( Point pt )
        {
            DGV.SelectedItems.Clear();

            var vp = _SelectRect.GetVisualParent();
            _Grid_Bounds = vp.GetTransformedBounds().Value.Bounds;
            if ( vp is Layoutable parentLayout )
            {
                _ParentLayoutMargin = parentLayout.Margin;                
                pt = new Point( pt.X - _ParentLayoutMargin.Left, pt.Y - _ParentLayoutMargin.Top );
            }
            _Press_Pos         = pt;
            _ScrollPos_Y       = GetScrollY();
            _SelectRect.Margin = new Thickness( pt.X, pt.Y, _Grid_Bounds.Width - pt.X, _Grid_Bounds.Height - pt.Y );
            Debug.WriteLine( _SelectRect.Margin );

            _SelectRect.IsVisible = true;
            _IsPointerPressed     = true;

            _ScrollIfNeedTimer.Interval = ScrollDelayInMilliseconds;
            _ScrollIfNeedTimer.Enabled  = true;
        }
        private void End_SelectRect( /*PointerReleasedEventArgs*/RoutedEventArgs e )
        {
            _ScrollIfNeedTimer.Enabled = false;

            DGV.PointerMoved    -= DGV_PointerMoved;
            DGV.PointerReleased -= DGV_PointerReleased;
            e.Handled = true;

            Process_SelectRect();

            _LastMove             = null;
            _ParentLayoutMargin   = default;
            _IsPointerPressed     = false;
            _SelectRect.IsVisible = false;
        }
        private void Move_SelectRect( Point pt )
        {
            Debug.Assert( _IsPointerPressed );

            var y_scroll = GetScrollY() - _ScrollPos_Y;

            pt = new Point( pt.X - _ParentLayoutMargin.Left, pt.Y - _ParentLayoutMargin.Top );

            #region [.core-of-meaning.]
            var y0 = _Press_Pos.Y - y_scroll;
            var x  = Math.Min( pt.X, _Press_Pos.X );
            var y  = Math.Min( pt.Y, y0 );
            var width  = Math.Abs( pt.X - _Press_Pos.X );
            var height = Math.Abs( pt.Y - y0 );

            var moveDirection  = (pt.Y < _Press_Pos.Y) ? MoveDirectionEnum.Up : MoveDirectionEnum.Down;
            _LastMove          = (pt, moveDirection);
            _SelectRect.Margin = new Thickness( x, y, _Grid_Bounds.Width - (x + width), _Grid_Bounds.Height - (y + height) );
            #endregion

            Process_SelectRect();
        }
        private void Process_SelectRect()
        {
            Debug.Assert( _IsPointerPressed );
            if ( !_LastMove.HasValue ) return;

            var selectRect_Bounds = _SelectRect.GetBoundsByTopAncestor( _Grid_Bounds.Width ); if ( selectRect_Bounds.Height <= 0 ) return;

            var selItems = DGV.SelectedItems;
            var dgrows   = DGV.GetVisualDescendants().OfType< DataGridRow >().Where( r => r.IsVisible ).ToList( _Model.Count );
            var range    = (min: int.MaxValue, max: -1);
            foreach ( var dgrow in dgrows )
            {
                var bounds = dgrow.GetBoundsByTopAncestor(); if ( (bounds.Height <= 0) || (bounds.Width <= 0) ) continue;

                var rowIndex = dgrow.Index; if ( !IsValidRowIndex( rowIndex ) ) continue;
                var row      = _Model[ rowIndex ];

                if ( bounds.Intersects( selectRect_Bounds ) )
                {
                    range.min = Math.Min( range.min, rowIndex );
                    range.max = Math.Max( range.max, rowIndex );
                    
                    selItems.Add( row );
                }
                else
                {
                    selItems.Remove( row );
                }
            }

            switch ( _LastMove.Value.MoveDirection )
            {
                case MoveDirectionEnum.Up  : if ( range.min != int.MaxValue ) DGV.SelectedIndex = range.min; break;
                case MoveDirectionEnum.Down: if ( range.max !=  - 1         ) DGV.SelectedIndex = range.max; break;
            }
            for ( var i = range.min; i <= range.max; i++ )
            {
                selItems.Add( _Model[ i ] );
            }
        }

        [M(O.AggressiveInlining)] private bool IsValidRowIndex( int rowIndex ) => ((0 <= rowIndex) && (rowIndex < _Model.Count));
        #endregion


        #region [.Scroll if need to point.]
        public int ScrollDelayInMilliseconds { get => _ScrollHelper.ScrollDelayInMilliseconds; set => _ScrollHelper.ScrollDelayInMilliseconds = value; }

        private void _ScrollIfNeedTimer_Elapsed()
        {
            if ( _IsPointerPressed )
            {
                if ( _LastMove.HasValue && _ScrollHelper.Scroll2ViewIfNeed_UseScrollDelay( _LastMove.Value.Pos ) )
                {
                    Process_SelectRect();
                }
            }
            else if ( _ScrollIfNeedTimer.Enabled )
            {
                _ScrollIfNeedTimer.Enabled = false;
            }
        }
        #endregion
    }
}
