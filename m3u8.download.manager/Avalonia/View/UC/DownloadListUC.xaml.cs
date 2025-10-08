using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;

using m3u8.download.manager.models;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;
using _CollectionChangedTypeEnum_ = m3u8.download.manager.models.DownloadListModel.CollectionChangedTypeEnum;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class DownloadListUC : UserControl
    {
        /// <summary>
        /// 
        /// </summary>
        public delegate void DownloadRowEventHandler( DownloadRow row );
        /// <summary>
        /// 
        /// </summary>
        public delegate void MouseClickRightButtonEventHandler( in Point pt, DownloadRow row );
        /// <summary>
        /// 
        /// </summary>
        public delegate void MouseClickColumnHeaderEventHandler( in Point pt, PointerUpdateKind pointerKind, DataGridColumnHeader columnHeader );

        #region [.field's.]
        public event DownloadRowEventHandler            SelectionChanged;
        public event DownloadRowEventHandler            OutputFileNameClick;
        public event DownloadRowEventHandler            OutputDirectoryClick;
        public event DownloadRowEventHandler            LiveStreamMaxFileSizeClick;
        public event MouseClickRightButtonEventHandler  MouseClickRightButton;
        public event MouseClickColumnHeaderEventHandler MouseClickColumnHeader;
        public event EventHandler                       DoubleClickEx;
#pragma warning disable CS0067
        public event DownloadRowEventHandler            UpdatedSingleRunningRow;
#pragma warning restore CS0067

        private DataGrid DGV;
        private DownloadListModel _Model;
        private DataGrid_SelectRect_Extension< DownloadRow > _SelectRectExtension;
        private DataGrid_DragDrop_Extension< DownloadRow >   _DragDropExtension;
        private ContextMenu _ColumnsContextMenu;
        #endregion

        #region [.ctor().]
        public DownloadListUC() => this.InitializeComponent();
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );

            DGV = this.FindControl< DataGrid >( nameof(DGV) );
            DGV.SelectionChanged   += DGV_SelectionChanged;
            DGV.CellPointerPressed += DGV_CellPointerPressed;
            DGV.DoubleTapped       += (s, e) => DoubleClickEx?.Invoke( s, e );
            DGV.PointerPressed     += DGV_PointerPressed;
            //---DGV.KeyDown            += DGV_KeyDown;

            _SelectRectExtension = DataGrid_SelectRect_Extension.Create< DownloadRow >(
                  this
                , DGV
                , this.FindControl< Rectangle >( "selectRect" )
                , e => /*(e.Column.DisplayIndex == 0) &&*/ !e.PointerPressedEventArgs.KeyModifiers.HasFlag( KeyModifiers.Control ) /*#_Column_DisplayIndex*/ );

            _DragDropExtension = DataGrid_DragDrop_Extension.Create< DownloadRow >( 
                  DGV
                , this.FindControl< Rectangle >( "dndRect" )
                , e => /*(e.Column.DisplayIndex == 0) &&*/ e.PointerPressedEventArgs.KeyModifiers.HasFlag( KeyModifiers.Control ) /*e.Column.DisplayIndex != 0*/ /*#_Column_DisplayIndex*/
                , r => r.GetOutputFullFileNames()
                , r => _Model.GetVisibleIndex( r )
                , (r, oldIndex, newIndex) => _Model.ChangeRowPosition( r, newIndex ) );
            //------------------------------------------------------------------//

            CreateColumnsContextMenu();

            #region comm.
            /*DGV.AddHandler(
                InputElement.PointerPressedEvent,
                (s, e) =>
                {
                    var p = e.GetCurrentPoint( null );
                    Debug.WriteLine( $"PointerPressedEvent: {p.Position}" );
                    if ( p.Properties.IsRightButtonPressed )
                    {
                        var coll = (e.Source as IControl)?.GetSelfAndVisualAncestors().OfType< DataGridColumnHeader >().FirstOrDefault();
                        if ( coll != null )
                        {
                            e.Pointer.Capture( null );
                            e.Handled = true;
                        }
                    }
                },
                handledEventsToo: true );
            */
            #endregion

            #region comm.
            //this.Styles.Add_NoThrow( GlobalStyles.Dark );
            //foreach ( var style in this.Styles ) DGV.Styles.Add_NoThrow( style );
            #endregion
        }
        private void CreateColumnsContextMenu()
        {
            _ColumnsContextMenu = this.Find_Ex< ContextMenu >( "columnsContextMenu" );

            EventHandler< RoutedEventArgs > menuItemClick = (s, _) =>
            {
                var item = (MenuItem) s;                
                var ch   = (CheckBox) item.Icon;
                var col  = (DataGridColumn) item.Tag;

                DGV.ScrollIntoView( null, DGV.Columns[ 0 ] );

                ch.IsChecked = !ch.IsChecked.GetValueOrDefault();
                try
                {
                    col.IsVisible = ch.IsChecked.GetValueOrDefault();
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }
            };
            EventHandler< RoutedEventArgs > checkBoxClick = (s, _) =>
            {
                var ch   = (CheckBox) s;
                var item = (MenuItem) ch.Parent;
                var col  = (DataGridColumn) item.Tag;

                DGV.ScrollIntoView( null, DGV.Columns[ 0 ] );

                ch.IsChecked = !ch.IsChecked.GetValueOrDefault();
                try
                {
                    col.IsVisible = ch.IsChecked.GetValueOrDefault();
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }

                _ColumnsContextMenu.Close();
            };
            foreach ( var col in DGV.Columns.Cast< DataGridColumn >() )
            {
                var enabled = !col.CellStyleClasses.Contains( "visible_always_sign" );
                var txt = (col.Header as TextBlock)?.Text;
                var ch  = new CheckBox();
                if ( enabled ) ch.Click += checkBoxClick;
                var mi  = new MenuItem() { Icon = ch, Header = txt, IsEnabled = enabled, Tag = col };
                if ( enabled ) mi.Click += menuItemClick;
                _ColumnsContextMenu.Items.Add( mi );
            }
            //-------------------------------------------------//

            _ColumnsContextMenu.Items.Add( new Separator() );
            var rmi = new MenuItem() { Header = "Reset all columns" };
            rmi.Click += (_, _) =>
            {
                foreach ( var item in _ColumnsContextMenu.Items.OfType< MenuItem >() )
                {
                    if ( !(item.Tag is DataGridColumn col) ) continue;

                    col.IsVisible = true;
                    foreach ( var cls in col.CellStyleClasses )
                    {
                        var i = cls.IndexOf( '=' ); if ( i == -1 ) continue;
                        var n = cls.Substring( 0, i );
                        var v = cls.Substring( i + 1 );
                        switch ( n )
                        {
                            case "w":
                                if ( v.TryParse2DataGridLength( out var gridLength, col.Width.DisplayValue ) )
                                {
                                    col.Width = gridLength;
                                }
                                break;
                            case "v": case "vis":
                                if ( bool.TryParse( v, out var b ) )
                                {
                                    col.IsVisible = b;
                                }
                                break;
                            case "di":
                                if ( int.TryParse( v, out i ) && (0 <= i) )
                                {
                                    col.DisplayIndex = i;
                                }
                                break;
                        }
                    }
                }
            };
            _ColumnsContextMenu.Items.Add( rmi );
            //-------------------------------------------------//

            _ColumnsContextMenu.Opened += (_, _) =>
            {
                foreach ( var item in _ColumnsContextMenu.Items.OfType< MenuItem >() )
                {
                    if ( (item.Tag is DataGridColumn col) && (item.Icon is CheckBox ch) )
                    {
                        ch.IsChecked = col.IsVisible;
                    }
                }
            };
        }
        #endregion

        #region [.DGV events.]
        private void DGV_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            var selectedDownloadRow = this.GetSelectedDownloadRow();
            SelectionChanged?.Invoke( selectedDownloadRow );

            if ( 1 < DGV.SelectedItems.Count )
            {
                foreach ( var row in _Model.GetRows() )
                {
                    row.IsFocusedAndSelected = (selectedDownloadRow == row);
                }
            }
            else
            {
                foreach ( var row in _Model.GetRows() )
                {
                    row.IsFocusedAndSelected = false;
                }
            }

            #region comm.
            //if ( (selectedDownloadRow != null) && !selectedDownloadRow.IsFinished() )
            //{
            //    var pt = DGV.PointToClient( Control.MousePosition );
            //    var ht = DGV.HitTest( pt.X, pt.Y );
            //    switch ( ht.ColumnIndex )
            //    {
            //        case OUTPUTFILENAME_COLUMN_INDEX:
            //        case OUTPUTDIRECTORY_COLUMN_INDEX:
            //            DGV.SetHandCursorIfNonHand();
            //            break;
            //    }
            //} 
            #endregion
        }
        private async void DGV_PointerPressed( object sender, PointerPressedEventArgs e )
        {
            DataGridColumnHeader columnHeader;

            var p = e.GetCurrentPoint( this/*null*/ );
            switch ( p.Properties.PointerUpdateKind )
            {
                case PointerUpdateKind.LeftButtonPressed:
                    columnHeader = (e.Source as Control)?.GetSelfAndVisualAncestors().OfType< DataGridColumnHeader >().FirstOrDefault();
                    if ( columnHeader == null )
                    {
                        DGV.SelectedItems.Clear();
                    }
                    else if ( DGV.Columns[ 0 ]?.Header == columnHeader.Content )
                    {
                        DGV.SelectedItems.Clear();
                        await Task.Delay( 100 );
                        DGV.SelectAll();
                    }
                    else
                    {
                        var evnt = MouseClickColumnHeader;
                        if ( evnt != null )
                        {
                            e.Pointer.Capture( null );
                            e.Handled = true;
                            evnt( p.Position, p.Properties.PointerUpdateKind, columnHeader );
                        }
                    }
                    break;

                case PointerUpdateKind.RightButtonPressed:
                    columnHeader = (e.Source as Control)?.GetSelfAndVisualAncestors().OfType< DataGridColumnHeader >().FirstOrDefault();
                    if ( columnHeader != null )
                    {
                        _ColumnsContextMenu.Open( this );

                        #region comm. MouseClickColumnHeader.
                        //var evnt = MouseClickColumnHeader;
                        //if ( evnt != null )
                        //{
                        //    e.Pointer.Capture( null );
                        //    e.Handled = true;
                        //    evnt( p.Position, p.Properties.PointerUpdateKind, columnHeader );
                        //}
                        #endregion
                    }
                    else
                    {
                        const int COLUMN_HEIGHT_THRESHOLD = 15;
                        var columnHeadersHeight = DGV.GetVisualDescendants().OfType< DataGridColumnHeader >().Select( c => c.Bounds.Height ).First( h => h != 0 );
                        if ( p.Position.Y <= (columnHeadersHeight + COLUMN_HEIGHT_THRESHOLD) )
                        {
                            _ColumnsContextMenu.Open( this );
                        }
                        else
                        {
                            var evnt = MouseClickRightButton;
                            if ( evnt != null )
                            {
                                e.Pointer.Capture( null );
                                e.Handled = true;
                                evnt( p.Position, row: null/*this.GetSelectedDownloadRow()*/ );
                            }
                        }
                    }
                    break;
            }
        }
        private void DGV_CellPointerPressed( object sender, DataGridCellPointerPressedEventArgs e )
        {
            const int OutputFileName_Column_DisplayIndex        = 1;
            const int OutputDirectory_Column_DisplayIndex       = 2;
            const int LiveStreamMaxFileSize_Column_DisplayIndex = 12;

            var pargs = e.PointerPressedEventArgs;
            var p     = pargs.GetCurrentPoint( this );
            switch ( p.Properties.PointerUpdateKind )
            {
                case PointerUpdateKind.LeftButtonPressed:
                    var columnDisplayIndex = e.Column.DisplayIndex;
                    switch ( columnDisplayIndex )
                    {
                        case OutputFileName_Column_DisplayIndex:
                        case OutputDirectory_Column_DisplayIndex:
                        case LiveStreamMaxFileSize_Column_DisplayIndex:
                        {
                            if ( DGV.SelectedItems.Count != 1 ) break;
                            var km = pargs.KeyModifiers;
                            if ( km.HasFlag( KeyModifiers.Control ) || km.HasFlag( KeyModifiers.Shift ) || km.HasFlag( KeyModifiers.Alt ) ) break;
                                
                            var selectedDownloadRow = this.GetSelectedDownloadRow();
                            var rowIndex            = e.Row.Index;
                            if ( IsValidRowIndex( rowIndex, selectedDownloadRow ) )
                            {
                                var evnt = default(DownloadRowEventHandler);
                                switch ( columnDisplayIndex )
                                {                                    
                                    case OutputDirectory_Column_DisplayIndex: evnt = OutputDirectoryClick; break;
                                    case OutputFileName_Column_DisplayIndex:
                                        if ( (pargs.Source is Image img) && (img.Name == "live_stream") && _Model[ rowIndex ].IsLiveStream )
                                        {
                                            evnt = LiveStreamMaxFileSizeClick;
                                        }
                                        else
                                        {
                                            evnt = OutputFileNameClick;
                                        }
                                        break;
                                    case LiveStreamMaxFileSize_Column_DisplayIndex:
                                        if ( _Model[ rowIndex ].IsLiveStream )
                                        {
                                            evnt = LiveStreamMaxFileSizeClick;
                                        }
                                        break;
                                }
                                if ( evnt != null )
                                {
                                    pargs.Pointer.Capture( null );
                                    pargs.Handled = true;
                                    evnt( selectedDownloadRow );
                                    
                                    //Dispatcher.UIThread.Post( () => evnt( selectedDownloadRow ) );
                                }
                            }
                        }
                        break;
                    }
                    break;

                case PointerUpdateKind.RightButtonPressed:
                    {
                        var evnt = MouseClickRightButton;
                        if ( evnt != null )
                        {
                            if ( DGV.SelectedItems.Count <= 1 )
                            {
                                var row = (pargs.Source as Control)?.GetSelfAndVisualAncestors().OfType< DataGridRow >().FirstOrDefault();
                                if ( row != null )
                                {
                                    DGV.SelectedIndex = row.Index;
                                }
                            }
                            #region comm. other var.
                            /*
                            var row = (pargs.Source as Control)?.GetSelfAndVisualAncestors().OfType< DataGridRow >().FirstOrDefault();
                            if ( row != null )
                            {
                                var rowIdx = row.GetIndex();
                                if ( DGV.SelectedIndex != rowIdx )
                                {
                                    var selRows = this.GetSelectedDownloadRows();
                                    DGV.SelectedIndex = rowIdx;
                                    foreach ( var selRow in selRows )
                                    {
                                        DGV.SelectedItems.Add( selRow );
                                    }
                                }
                            }
                            //*/
                            #endregion

                            pargs.Pointer.Capture( null );
                            pargs.Handled = true;
                            evnt( p.Position, this.GetSelectedDownloadRow() );
                        }
                    }
                    break;
            }
        }
        #endregion

        #region [.Model.]
        internal string GetColumnsInfoJson() => ColumnsInfoSerializer.ToJSON( DGV.Columns );
        internal void RestoreColumnsInfoFromJson( string json ) => ColumnsInfoSerializer.Restore_NoThrow( DGV.Columns, json );

        private async void SetDataGridItems()
        {
            if ( !Dispatcher.UIThread.CheckAccess() )
            {
                await Dispatcher.UIThread.InvokeAsync( SetDataGridItems );
                return;
            }

            if ( _Model == null )
            {
                DGV.ItemsSource = null;
            }
            else
            {
                DGV.ItemsSource = new DataGridCollectionView( _Model.GetRows() );
            }
        }
        private bool IsValidRowIndex( int rowIndex, DownloadRow selectedRow ) => ((0 <= rowIndex) && (rowIndex < _Model.RowsCount) && (_Model[ rowIndex ] == selectedRow));

        internal DownloadRow GetSelectedDownloadRow() => (DGV.SelectedItem as DownloadRow);
        internal IReadOnlyList< DownloadRow > GetSelectedDownloadRows()
        {
            var srs = DGV.SelectedItems;
            var lst = new List< DownloadRow >( srs.Count );
            foreach ( var row in srs.Cast< DownloadRow >() )
            {
                lst.Add( row );
            }
            return (lst);
        }
        internal /*async Task*/void SelectDownloadRow( DownloadRow row, bool scrollIntoView = true )
        {
            if ( row != null )
            {
                DGV.SelectedItem = row;
                if ( scrollIntoView )
                {
                    //await Task.Delay( 100 );
                    //DGV.ScrollIntoView( row, null );

                    Task.Delay( 10 ).ContinueWith( _ => DGV.ScrollIntoView( row, null ), TaskScheduler.FromCurrentSynchronizationContext() );
                }
            }
        }
        internal bool HasFocus => this.IsFocused_SelfOrDescendants();

        internal void SetModel( DownloadListModel model )
        {
            if ( _Model == model ) return;

            DetachModel();

            _Model = model ?? throw (new ArgumentNullException( nameof(model) ));
            _Model.CollectionChanged -= Model_CollectionChanged;
            _Model.CollectionChanged += Model_CollectionChanged;
            //_Model.RowPropertiesChanged -= Model_RowPropertiesChanged;
            //_Model.RowPropertiesChanged += Model_RowPropertiesChanged;

            //---Model_CollectionChanged( _CollectionChangedTypeEnum_.Add );
            SetDataGridItems();

            _SelectRectExtension.SetModel( model );
            _DragDropExtension  .SetModel( model );
        }
        private void DetachModel()
        {
            if ( _Model != null )
            {
                _Model.CollectionChanged -= Model_CollectionChanged;
                //_Model.RowPropertiesChanged -= Model_RowPropertiesChanged;
                _Model = null;

                DGV.ItemsSource = null;
            }
        }

        private void Model_CollectionChanged( _CollectionChangedTypeEnum_ changedType, DownloadRow row )
        {
            switch ( changedType )
            {
                case _CollectionChangedTypeEnum_.Sort:
                    DGV.InvalidateVisual();
                    break;

                case _CollectionChangedTypeEnum_.Add:
                    SetDataGridItems();
                    SelectDownloadRow( row );
                    break;

                case _CollectionChangedTypeEnum_.Add_Bulk:
                case _CollectionChangedTypeEnum_.Clear:
                    SetDataGridItems();
                    DGV.InvalidateVisual();
                    break;

                case _CollectionChangedTypeEnum_.Remove:
                case _CollectionChangedTypeEnum_.Remove_Bulk:
                    #region [.save selected row.]
                    var selRow = this.GetSelectedDownloadRow();
                    var dgrow  = DGV.GetVisualDescendants().OfType< DataGridRow >().First( r => r.DataContext == selRow );
                    var selVisibleIndex = (dgrow?.Index).GetValueOrDefault( -1 );

                    //if ( DGV.ItemsSource is DataGridCollectionView dcv )
                    //{
                    //    var prev_ItemCount = dcv.ItemCount;
                    //    selVisibleIndex -= (prev_ItemCount - _Model.RowsCount);
                    //}
                    #endregion

                    SetDataGridItems();
                    DGV.InvalidateVisual();

                    #region [.restore selected row.]
                    try
                    {
                        var rowCount = _Model.RowsCount;
                        var hasRows = (0 < rowCount);
                        if ( hasRows )
                        {
                            var visibleIndex = Math.Min( Math.Max( 0, selVisibleIndex ), rowCount - 1 );
                            SelectDownloadRow( _Model[ visibleIndex ] );
                        }
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( ex );
                    }
                    #endregion
                    break;
            }
        }
        //private void Model_RowPropertiesChanged( DownloadRow row, string propertyName )
        //{
        //    //---DGV.InvalidateVisual();

        //    #region comm.
        //    //var visibleIndex = row.GetVisibleIndex();
        //    //if ( (0 <= visibleIndex) && (visibleIndex < DGV.RowCount) )
        //    //{
        //    //    DGV.InvalidateRow( visibleIndex );

        //    //    if ( propertyName == nameof(DownloadRow.Status) )
        //    //    {
        //    //        RestoreSortIfNeed();
        //    //    }
        //    //} 
        //    #endregion
        //}

        internal DataGrid DataGrid => DGV;
        #endregion

        #region [.static methods for view.]
        private const string CREATED_DT = "HH:mm:ss  (yyyy.MM.dd)";
        private const string HH_MM_SS   = "hh\\:mm\\:ss";
        private const string MM_SS      = "mm\\:ss";

        [M(O.AggressiveInlining)] internal static string GetDownloadInfoText( DownloadRow row )
        {
            var st = row.Status;
            switch ( st )
            {
                case DownloadStatus.Created: return ($"[created]: {row.CreatedOrStartedDateTime.ToString( CREATED_DT )}");
                case DownloadStatus.Started: return ($"{row.GetElapsed().ToString( HH_MM_SS )}");
                case DownloadStatus.Wait   : return ($"(wait), ({row.GetElapsed().ToString( HH_MM_SS )})");
            }

            var ts           = row.GetElapsed();
            var elapsed      = ((1 < ts.TotalHours) ? ts.ToString( HH_MM_SS ) : (':' + ts.ToString( MM_SS )));
            var percent      = ((0 < row.TotalParts) ? Convert.ToByte( (100.0 * row.SuccessDownloadParts) / row.TotalParts ).ToString() : "-");
            var downloadInfo = $"{percent}%, ({elapsed})";
            //var failedParts  = ((row.FailedDownloadParts != 0) ? $" (failed: {row.FailedDownloadParts})" : null);            

            #region [.speed.]
            if ( !st.IsPaused() )
            {
                var elapsedSeconds = row.GetElapsed4SpeedMeasurement().TotalSeconds;
                var downloadBytes  = row.GetDownloadBytesLengthAfterLastRun();
                if ( (1_024 < downloadBytes) && (2.5 <= elapsedSeconds) )
                {
                    var speedText = Extensions.GetSpeedText( downloadBytes, elapsedSeconds, row.GetInstantSpeedInMbps() );
                    downloadInfo += $", [{speedText}]";
                }
            }
            #endregion

            return (downloadInfo);
        }

        [M(O.AggressiveInlining)] private static bool TryGetDownloadProgressText( DownloadRow row, out string progressText )
        {
            var st = row.Status;
            switch ( st )
            {
                case DownloadStatus.Created:
                case DownloadStatus.Started:
                case DownloadStatus.Wait   :
                    progressText = null;
                    return (false);

                default:
                    string percentText;
                    if ( 0 < row.TotalParts )
                    {
                        var part    = (1.0 * row.SuccessDownloadParts) / row.TotalParts;
                        var percent = (row.TotalParts <= (row.SuccessDownloadParts + row.FailedDownloadParts)) ? 100 : Extensions.Min( (byte) (100 * part), 99 );
                        percentText = percent.ToString();
                    }
                    else if ( st == DownloadStatus.Canceled ) //not-started
                    {
                        progressText = null;
                        return (false);
                    }
                    else
                    {
                        percentText = "-";
                    }

                    var failedParts = ((row.FailedDownloadParts != 0) ? $" (failed: {row.FailedDownloadParts})" : null);
                    progressText = $"{percentText}%  ({row.SuccessDownloadParts} of {row.TotalParts}{failedParts})";
                    return (true);
            }
        }
        [M(O.AggressiveInlining)] private static bool TryGetDownloadProgressPartValue( DownloadRow row, out double part )
        {
            var st = row.Status;
            switch ( st )
            {
                case DownloadStatus.Created:
                case DownloadStatus.Started:
                case DownloadStatus.Wait   :
                    part = 0d;
                    return (false);

                default:
                    if ( 0 < row.TotalParts )
                    {
                        part = 100 * ((1.0 * row.SuccessDownloadParts) / row.TotalParts);
                    }
                    else if ( st == DownloadStatus.Canceled ) //not-started
                    {
                        part = 0d;
                        return (false);
                    }
                    else
                    {
                        part = 0d;
                    }
                    return (true);
            }
        }
        [M(O.AggressiveInlining)] internal static string GetProgressText( DownloadRow row ) => (TryGetDownloadProgressText( row, out var progressText ) ? progressText : "-");
        [M(O.AggressiveInlining)] internal static double GetProgressPartValue( DownloadRow row )=> (TryGetDownloadProgressPartValue( row, out var part ) ? part : 0d);

        [M(O.AggressiveInlining)] internal static string GetDownloadTimeText( DownloadRow row )
        {
            if ( row.Status == DownloadStatus.Created )
            {
                return (row.CreatedOrStartedDateTime.ToString( CREATED_DT ));
            }
            return (row.GetElapsed().ToString( HH_MM_SS ));
        }
        [M(O.AggressiveInlining)] internal static string GetApproxRemainedTimeText( DownloadRow row )
        {
            if ( row.Status == DownloadStatus.Running)
            {
                var totalBytes = row.GetApproxTotalBytes();
                if ( totalBytes.HasValue )
                {
                    var elapsedSeconds = row.GetElapsed().TotalSeconds;
                    var downloadBytes  = row.GetDownloadBytesLengthAfterLastRun();                    
                    if ( (1_000 < downloadBytes) && (2.5 <= elapsedSeconds) )
                    {
                        var remainedBytes = totalBytes.Value - (row.DownloadBytesLength - downloadBytes);
                        var remainedTime  = TimeSpan.FromSeconds( (remainedBytes - downloadBytes) * (elapsedSeconds / downloadBytes) );
                        return (remainedTime.ToString( HH_MM_SS ));
                    }
                }
            }
            return (string.Empty);
        }
        [M(O.AggressiveInlining)] internal static string GetDownloadSpeedText( DownloadRow row )
        {
            if ( !row.Status.IsPaused() )
            {                
                var elapsedSeconds = row.GetElapsed4SpeedMeasurement().TotalSeconds;
                var downloadBytes  = row.GetDownloadBytesLengthAfterLastRun();
                if ( (1_024 < downloadBytes) && (2.5 <= elapsedSeconds) )
                {
                    return (Extensions.GetSpeedText( downloadBytes, elapsedSeconds, row.GetInstantSpeedInMbps() ));
                }
            }
            return (string.Empty);
        }
        [M(O.AggressiveInlining)] internal static string GetDisplaySizeText( long size )
        {
            if ( size == 0 )
            {
                return ("-");
            }

            static string to_text( float f ) => f.ToString( (f == Math.Ceiling( f )) ? "N0" : "N2" );

            const float KILOBYTE = 1024;
            const float MEGABYTE = KILOBYTE * KILOBYTE;
            const float GIGABYTE = MEGABYTE * KILOBYTE;

            if ( GIGABYTE < size )
                return (to_text( size / GIGABYTE ) + " GB");
            if ( MEGABYTE < size )
                return (to_text( size / MEGABYTE) + " MB");
            if ( KILOBYTE < size )
                return (to_text( size / KILOBYTE ) + " KB");
            return ((size / KILOBYTE).ToString("N1") + " KB");
        }
        [M(O.AggressiveInlining)] internal static string GetApproxRemainedBytesText( DownloadRow row )
        {
            var size = row.GetApproxRemainedBytes();
            return (size.HasValue ? GetDisplaySizeText( size.Value ) : string.Empty);
        }
        [M(O.AggressiveInlining)] internal static string GetApproxTotalBytesText( DownloadRow row )
        {
            var size = row.GetApproxTotalBytes();
            return (size.HasValue ? GetDisplaySizeText( size.Value ) : string.Empty);
        }
        #endregion
    }
}
