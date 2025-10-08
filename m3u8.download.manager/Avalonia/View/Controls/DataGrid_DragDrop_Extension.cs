using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
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
    internal static class DataGrid_DragDrop_Extension
    {
        public static DataGrid_DragDrop_Extension< T > Create< T >( 
              DataGrid  dgv
            , Rectangle selectRect
            , DataGrid_DragDrop_Extension< T >.Allow_Begin_DragDrop_Delegate               allow_Begin_DragDrop
            , DataGrid_DragDrop_Extension< T >.GetFileNames_4_DragDropFilesFormat_Delegate getFileNames_4_DragDropFilesFormat
            , DataGrid_DragDrop_Extension< T >.GetIndexOf_Delegate                         getIndexOf
            , DataGrid_DragDrop_Extension< T >.ChangeRowPosition_Delegate                  changeRowPosition
            , IReadOnlyList< T > model = null ) where T : class 
            => new DataGrid_DragDrop_Extension< T >( new DataGrid_DragDrop_Extension< T >.InitParams()
            {
                dgv                                = dgv,
                selectRect                         = selectRect,
                model                              = model,
                allow_Begin_DragDrop               = allow_Begin_DragDrop,
                getFileNames_4_DragDropFilesFormat = getFileNames_4_DragDropFilesFormat,
                getIndexOf                         = getIndexOf,
                changeRowPosition                  = changeRowPosition,
            });
        public static DataGrid_DragDrop_Extension< T > Create< T >( in DataGrid_DragDrop_Extension< T >.InitParams ip ) where T : class => new DataGrid_DragDrop_Extension< T >( ip );
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class DataGrid_DragDrop_Extension< T > where T : class
    {
        public delegate bool                  Allow_Begin_DragDrop_Delegate( DataGridCellPointerPressedEventArgs e );
        public delegate IEnumerable< string > GetFileNames_4_DragDropFilesFormat_Delegate( T t );
        public delegate int                   GetIndexOf_Delegate( T t );
        public delegate void                  ChangeRowPosition_Delegate( T t, int oldIndex, int newIndex );
        
        /// <summary>
        /// 
        /// </summary>
        public struct InitParams
        {
            public DataGrid                                    dgv                                { get; set; }
            public Rectangle                                   selectRect                         { get; set; }
            public IReadOnlyList< T >                          model                              { get; set; }
            public Allow_Begin_DragDrop_Delegate               allow_Begin_DragDrop               { get; set; }
            public GetFileNames_4_DragDropFilesFormat_Delegate getFileNames_4_DragDropFilesFormat { get; set; }
            public GetIndexOf_Delegate                         getIndexOf                         { get; set; }
            public ChangeRowPosition_Delegate                  changeRowPosition                  { get; set; }            
        }

        #region [.field's.]
        private DataGrid  DGV;
        private Visual    _TopVisual;
        private Rectangle _SelectRect;
        private IReadOnlyList< T > _Model;
        private Allow_Begin_DragDrop_Delegate               _Allow_Begin_DragDrop;
        private GetFileNames_4_DragDropFilesFormat_Delegate _GetFileNames_4_DragDropFilesFormat;
        private GetIndexOf_Delegate                         _GetIndexOf;
        private ChangeRowPosition_Delegate                  _ChangeRowPosition;
        private _Timer_                                     _ScrollIfNeedTimer;
        private DataGrid_ScrollHelper< T >                  _ScrollHelper;        

        public DataGrid_DragDrop_Extension( in InitParams ip )
        {
            DGV                                 = ip.dgv               ?? throw (new ArgumentNullException( nameof(ip.dgv) ));
            _SelectRect                         = ip.selectRect        ?? throw (new ArgumentNullException( nameof(ip.selectRect) ));
            _GetIndexOf                         = ip.getIndexOf        ?? throw (new ArgumentNullException( nameof(ip.getIndexOf) ));
            _ChangeRowPosition                  = ip.changeRowPosition ?? throw (new ArgumentNullException( nameof(ip.changeRowPosition) ));
            _Allow_Begin_DragDrop               = ip.allow_Begin_DragDrop               ?? new Allow_Begin_DragDrop_Delegate( e => e.Column.DisplayIndex != 0 );
            _GetFileNames_4_DragDropFilesFormat = ip.getFileNames_4_DragDropFilesFormat ?? new GetFileNames_4_DragDropFilesFormat_Delegate( _ => null );
            _ScrollHelper = DataGrid_ScrollHelper.Create( DGV, ip.model );
            SetModel( ip.model );

            var scrollIfNeedTimer_Elapsed_Action = new Action( _ScrollIfNeedTimer_Elapsed );
            _ScrollIfNeedTimer = new _Timer_() { Interval = ScrollDelayInMilliseconds, AutoReset = true, Enabled = false };
            _ScrollIfNeedTimer.Elapsed += async (_, _) => await Dispatcher.UIThread.InvokeAsync( scrollIfNeedTimer_Elapsed_Action );

            DGV.AttachedToVisualTree += (_, e) => _TopVisual = (Visual) e.Root;
        }
        #endregion

        public bool IsInDragDrop => _IsInDragDrop;
        public void SetModel( IReadOnlyList< T > model )
        {
            _Model = model;
            _ScrollHelper.SetModel( _Model );

            DGV.CellPointerPressed -= DGV_CellPointerPressed;
            if ( _Model != null )
            {
                DGV.CellPointerPressed += DGV_CellPointerPressed;
            }
        }

        #region [.DGV events.]
        ///// <summary>
        ///// 
        ///// </summary>
        //private sealed class DRAGDROP_ROWS_FORMAT_TYPE__PREV
        //{
        //    public DRAGDROP_ROWS_FORMAT_TYPE__PREV( IReadOnlyList< T > selectedRows, T focusedRow, int focusedRowIndex ) 
        //        => (Rows, FocusedRow, FocusedRowIndex) = (selectedRows, focusedRow, focusedRowIndex);
        //    public IReadOnlyList< T > Rows { get; }
        //    public T   FocusedRow      { get; }
        //    public int FocusedRowIndex { get; }
        //}
        /// <summary>
        /// 
        /// </summary>
        private sealed class DRAGDROP_ROWS_FORMAT_TYPE
        {
            private DRAGDROP_ROWS_FORMAT_TYPE() { }
            public DRAGDROP_ROWS_FORMAT_TYPE( int focusedRowIndex ) => FocusedRowIndex = focusedRowIndex;
            public int FocusedRowIndex { get; private set; }
            public T Get_Last_DragDrop_FocusedRow() => _Last_DragDrop_FocusedRow;


            public const string DataFormatIdentifier = "DRAGDROP-ROWS-FORMAT-TYPE";
            private static T _Last_DragDrop_FocusedRow;
            public static string ToJSON( T focusedRow, int focusedRowIndex )
            {
                _Last_DragDrop_FocusedRow = focusedRow;
                return (JsonSerializer.Serialize( new DRAGDROP_ROWS_FORMAT_TYPE( focusedRowIndex ) ));
            }
            public static bool TryFromJSON( string json, out DRAGDROP_ROWS_FORMAT_TYPE drft )
            {
                try
                {
                    drft = JsonSerializer.Deserialize< DRAGDROP_ROWS_FORMAT_TYPE >( json );
                    return (true);
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                    drft = default;
                    return (false);
                }
            }
        }

        private bool  _IsInDragDrop;
        private Point _Press_Pos;
        private Point _LastMove_Pos;
        private void DGV_CellPointerPressed( object sender, DataGridCellPointerPressedEventArgs e )
        {
            var pargs = e.PointerPressedEventArgs;
            if ( !pargs.Handled )
            {
                var p = e.PointerPressedEventArgs.GetCurrentPoint( _TopVisual );
                if ( (p.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) &&
                     _Allow_Begin_DragDrop( e )
                    )
                {
                    Prepare_DoDragDrop( e.PointerPressedEventArgs, p.Position );
                }
            }
        }
        private void DGV_PointerMoved( object sender, PointerEventArgs e ) => TryBegin_DoDragDrop( e );
        private void DGV_PointerReleased( object sender, PointerReleasedEventArgs e ) => End_DoDragDrop( e );

        private void Prepare_DoDragDrop( PointerPressedEventArgs e, in Point pt )
        {
            _Press_Pos = pt;

            DGV.PointerMoved    -= DGV_PointerMoved;
            DGV.PointerReleased -= DGV_PointerReleased;

            DGV.PointerMoved    += DGV_PointerMoved;
            DGV.PointerReleased += DGV_PointerReleased;
            e.Handled = true;
        }
        //private async void TryBegin_DoDragDrop__PREV( PointerEventArgs e )
        //{
        //    var pt = e.GetCurrentPoint( _TopVisual ).Position;

        //    const int MOVE_DELTA = 5;
        //    if ( Math.Abs( _Press_Pos.X - pt.X ) < MOVE_DELTA &&
        //         Math.Abs( _Press_Pos.Y - pt.Y ) < MOVE_DELTA ) return; 
        //    //-----------------------------------------------------//

        //    var rows = DGV.GetSelectedDownloadRows< T >();
        //    if ( !rows.Any() ) return;

        //    var focusedRow = DGV.GetSelectedDownloadRow< T >();
        //    if ( focusedRow == null ) return;

        //    #region [.create DragDrop DataObject.]
        //    var dataObj = new DataObject();

        //    var fileNames = rows.Select( r => _GetFileNames_4_DragDropFilesFormat( r ) ).SelectMany( _ => _ ).Where( fn => fn != null ).ToList( rows.Count ).ToArray();
        //    var hasExistsFiles = fileNames.Any( File.Exists );
        //    if ( hasExistsFiles ) dataObj.Set( DataFormats.Files, fileNames );            

        //    dataObj.Set( new DRAGDROP_ROWS_FORMAT_TYPE( rows, focusedRow, _GetIndexOf( focusedRow ) ) );
        //    #endregion

        //    DGV.PointerReleased -= DGV_PointerReleased;
        //    DGV.PointerMoved    -= DGV_PointerMoved;
        //    e.Handled = true;

        //    _ScrollIfNeedTimer.Interval = ScrollDelayInMilliseconds;
        //    _ScrollIfNeedTimer.Enabled  = true;

        //    DGV.SetValue( DragDrop.AllowDropProperty, true );
        //    DGV.AddHandler( DragDrop.DragOverEvent, DGV_DragOver );
        //    DGV.AddHandler( DragDrop.DropEvent    , DGV_DragDrop );
        //    try
        //    {
        //        _IsInDragDrop = true;
        //        var res = await DragDrop.DoDragDrop( e, dataObj, DragDropEffects.Move | DragDropEffects.Copy );
        //        if ( res != DragDropEffects.None )
        //        {
        //            ((DataGridCollectionView) DGV.ItemsSource).Refresh();
        //            //DGV.InvalidateVisual();
        //        }
        //        Debug.WriteLine( res.ToString() );
        //    }
        //    finally
        //    {
        //        _IsInDragDrop = false;
        //        DGV.SetValue( DragDrop.AllowDropProperty, false );
        //        DGV.RemoveHandler( DragDrop.DragOverEvent, DGV_DragOver );
        //        DGV.RemoveHandler( DragDrop.DropEvent    , DGV_DragDrop );
        //    }
        //    _ScrollIfNeedTimer.Enabled = false;
        //    End_DoDragDrop( e );
        //}        
        private Rect _Grid_Bounds;
        private int  _Last_Selected_RowIndex = -1;
        private async void TryBegin_DoDragDrop( PointerEventArgs e )
        {
            var pt = e.GetCurrentPoint( _TopVisual ).Position;

            const int MOVE_DELTA = 5;
            if ( Math.Abs( _Press_Pos.X - pt.X ) < MOVE_DELTA &&
                 Math.Abs( _Press_Pos.Y - pt.Y ) < MOVE_DELTA ) return; 
            //-----------------------------------------------------//

            var rows = DGV.GetSelectedDownloadRows< T >();
            if ( !rows.Any() ) return;

            var focusedRow = DGV.GetSelectedDownloadRow< T >();
            if ( focusedRow == null ) return;

            #region [.create DragDrop DataTransfer.]
            var dataTrans = new DataTransfer();

            var fileNames = rows.Select( r => _GetFileNames_4_DragDropFilesFormat( r ) ).SelectMany( _ => _ ).Where( fn => fn != null ).ToList( rows.Count );
            var hasExistsFiles = fileNames.Any( File.Exists );
            if ( hasExistsFiles ) 
            {
                var storageProvider = TopLevel.GetTopLevel( _TopVisual )?.StorageProvider;
                if ( storageProvider != null ) 
                {
                    var storageFiles = new List< IStorageFile >( fileNames.Count );
                    foreach ( var fn in fileNames )
                    {
                        var storageFile = await storageProvider.TryGetFileFromPathAsync( fn );
                        if ( storageFile != null ) storageFiles.Add( storageFile );
                    }
                    foreach ( var sf in storageFiles )
                    {
                        dataTrans.Add( DataTransferItem.CreateFile( sf ) );
                    }
                }                                
            }

            dataTrans.Add( DataTransferItem.Create( DataFormat.CreateStringApplicationFormat( DRAGDROP_ROWS_FORMAT_TYPE.DataFormatIdentifier ),
                                                    DRAGDROP_ROWS_FORMAT_TYPE.ToJSON( /*rows, */ focusedRow, _GetIndexOf( focusedRow ) ) ) );
            #endregion

            DGV.PointerReleased -= DGV_PointerReleased;
            DGV.PointerMoved    -= DGV_PointerMoved;
            e.Handled = true;

            _ScrollIfNeedTimer.Interval = ScrollDelayInMilliseconds;
            _ScrollIfNeedTimer.Enabled  = true;

            DGV.SetValue( DragDrop.AllowDropProperty, true );
            DGV.AddHandler( DragDrop.DragOverEvent, DGV_DragOver );
            DGV.AddHandler( DragDrop.DropEvent    , DGV_DragDrop );

            var vp = _SelectRect.GetVisualParent();
            _Grid_Bounds = vp.GetTransformedBounds().Value.Bounds;

            try
            {
                _IsInDragDrop = true;
                var res = await DragDrop.DoDragDropAsync( e, dataTrans, DragDropEffects.Move | DragDropEffects.Copy );
                if ( res != DragDropEffects.None )
                {
                    ((DataGridCollectionView) DGV.ItemsSource).Refresh();
                    //DGV.InvalidateVisual();
                }
                Debug.WriteLine( res.ToString() );
            }
            finally
            {
                _IsInDragDrop = false;
                DGV.SetValue( DragDrop.AllowDropProperty, false );
                DGV.RemoveHandler( DragDrop.DragOverEvent, DGV_DragOver );
                DGV.RemoveHandler( DragDrop.DropEvent    , DGV_DragDrop );
            }
            _ScrollIfNeedTimer.Enabled = false;
            End_DoDragDrop( e );
        }
        private void End_DoDragDrop( PointerEventArgs e )
        {
            DGV.PointerMoved    -= DGV_PointerMoved;
            DGV.PointerReleased -= DGV_PointerReleased;

            _Press_Pos    = default;
            _LastMove_Pos = default;

            e.Handled = true;
            _SelectRect.IsVisible   = false;
            _Last_Selected_RowIndex = -1;
        }

        //private void DGV_DragOver__PREV( object sender, DragEventArgs e )
        //{
        //    if ( e.Data.TryGet< DRAGDROP_ROWS_FORMAT_TYPE >( out var drft ) )
        //    {
        //        var pt = e.GetPosition( _TopVisual );

        //        var pt_4_scroll = _LastMove_Pos = e.GetPosition( DGV );
        //        _ScrollHelper.Scroll2ViewIfNeed_Now( pt_4_scroll );

        //        var oldIndex = drft.FocusedRowIndex;
        //        if ( oldIndex != -1 )
        //        {
        //            foreach ( var dgrow in DGV.GetVisualDescendants().OfType< DataGridRow >() )
        //            {
        //                if ( !dgrow.IsVisible ) continue;
        //                var bounds = dgrow.GetBoundsByTopAncestor(); if ( (bounds.Height <= 0) || (bounds.Width <= 0) ) continue;

        //                if ( bounds.Contains( pt ) )
        //                {
        //                    var newIndex = dgrow.Index;
        //                    if ( (newIndex != -1) && (oldIndex != newIndex) )
        //                    {
        //                        return;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    e.DragEffects = DragDropEffects.None;
        //}
        //private Rectangle _Last_DragOver_Rectangle;
        private void DGV_DragOver( object sender, DragEventArgs e )
        {
            var json = e.DataTransfer.TryGetValue( DataFormat.CreateStringApplicationFormat( DRAGDROP_ROWS_FORMAT_TYPE.DataFormatIdentifier ) );
            if ( DRAGDROP_ROWS_FORMAT_TYPE.TryFromJSON( json, out var drft ) )
            {
                //if ( _Last_DragOver_Rectangle != null ) _Last_DragOver_Rectangle.Fill = Brushes.CadetBlue;

                var pt = e.GetPosition( _TopVisual );

                var pt_4_scroll = _LastMove_Pos = e.GetPosition( DGV );
                _ScrollHelper.Scroll2ViewIfNeed_Now( pt_4_scroll );

                var oldIndex = drft.FocusedRowIndex;
                if ( oldIndex != -1 )
                {
                    foreach ( var dgrow in DGV.GetVisualDescendants().OfType< DataGridRow >() )
                    {
                        if ( !dgrow.IsVisible ) continue;
                        var bounds = dgrow.GetBoundsByTopAncestor(); if ( (bounds.Height <= 0) || (bounds.Width <= 0) ) continue;

                        if ( bounds.Contains( pt ) )
                        {
                            var newIndex = dgrow.Index;
                            if ( (newIndex != -1) && (oldIndex != newIndex) )
                            {
                                if ( _Last_Selected_RowIndex != newIndex )
                                {
                                    _Last_Selected_RowIndex = newIndex;

                                    //bounds = dgrow.Bounds;
                                    //var b = dgrow.Bounds, y_offset = b.Height;
                                    const int Y_OFFSET = 42; //?
                                    _SelectRect.Margin = new Thickness( bounds.Left,
                                                                        bounds.Top - Y_OFFSET,
                                                                        _Grid_Bounds.Width  - bounds.Right,
                                                                        _Grid_Bounds.Height - bounds.Bottom + Y_OFFSET );
                                    //_SelectRect.Fill      = Brushes.Red;
                                    _SelectRect.IsVisible = true;
                                    Debug.WriteLine( _SelectRect.Margin );
                                }

                                //var rect = dgrow.FindDescendantOfType< Rectangle >();
                                //if ( rect?.Name == "BackgroundRectangle" )
                                //{
                                //    _Last_DragOver_Rectangle = rect;
                                //    rect.Fill = Brushes.Red;
                                //}
                                return;
                            }
                        }
                    }
                }
            }

            e.DragEffects = DragDropEffects.None;
            _SelectRect.IsVisible   = false;
            _Last_Selected_RowIndex = -1;
        }
        //private void DGV_DragDrop__PREV( object sender, DragEventArgs e )
        //{
        //    if ( e.Data.TryGet< DRAGDROP_ROWS_FORMAT_TYPE >( out var drft ) )
        //    {
        //        var pt       = e.GetPosition( _TopVisual );
        //        var oldIndex = drft.FocusedRowIndex;
        //        if ( oldIndex != -1 )
        //        {
        //            foreach ( var dgrow in DGV.GetVisualDescendants().OfType< DataGridRow >() )
        //            {
        //                if ( !dgrow.IsVisible ) continue;
        //                var bounds = dgrow.GetBoundsByTopAncestor(); if ( (bounds.Height <= 0) || (bounds.Width <= 0) ) continue;

        //                if ( bounds.Contains( pt ) )
        //                {
        //                    var newIndex = dgrow.Index;
        //                    if ( (newIndex != -1) && (oldIndex != newIndex) )
        //                    {
        //                        _ChangeRowPosition( oldIndex, newIndex, drft.FocusedRow );

        //                        Debug.WriteLine( $"DGV_DragDrop: {drft}" );
        //                        return;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    e.DragEffects = DragDropEffects.None;
        //}        
        private void DGV_DragDrop( object sender, DragEventArgs e )
        {
            var json = e.DataTransfer.TryGetValue( DataFormat.CreateStringApplicationFormat( DRAGDROP_ROWS_FORMAT_TYPE.DataFormatIdentifier ) );
            if ( DRAGDROP_ROWS_FORMAT_TYPE.TryFromJSON( json, out var drft ) )
            {
                var pt       = e.GetPosition( _TopVisual );
                var oldIndex = drft.FocusedRowIndex;
                if ( oldIndex != -1 )
                {
                    foreach ( var dgrow in DGV.GetVisualDescendants().OfType< DataGridRow >() )
                    {
                        if ( !dgrow.IsVisible ) continue;
                        var bounds = dgrow.GetBoundsByTopAncestor(); if ( (bounds.Height <= 0) || (bounds.Width <= 0) ) continue;

                        if ( bounds.Contains( pt ) )
                        {
                            var newIndex = dgrow.Index;
                            if ( (newIndex != -1) && (oldIndex != newIndex) )
                            {
                                _ChangeRowPosition( drft.Get_Last_DragDrop_FocusedRow(), oldIndex, newIndex );

                                Debug.WriteLine( $"DGV_DragDrop: {drft}" );
                                return;
                            }
                        }
                    }
                }
            }

            e.DragEffects = DragDropEffects.None;            
        }
        #endregion

        #region [.Scroll if need to point.]
        public int ScrollDelayInMilliseconds { get => _ScrollHelper.ScrollDelayInMilliseconds; set => _ScrollHelper.ScrollDelayInMilliseconds = value; }

        private void _ScrollIfNeedTimer_Elapsed()
        {
            if ( _IsInDragDrop )
            {
                _ScrollHelper.Scroll2ViewIfNeed_UseScrollDelay( _LastMove_Pos );
            }
            else if ( _ScrollIfNeedTimer.Enabled )
            {
                _ScrollIfNeedTimer.Enabled = false;
            }
        }
        #endregion
    }
}
