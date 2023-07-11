﻿using System;
using System.Linq;

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
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
        public delegate void MouseClickRightButtonEventHandler( Point pt, DownloadRow row );

        #region [.field's.]
        public event DownloadRowEventHandler           SelectionChanged;
        public event DownloadRowEventHandler           OutputFileNameClick;
        public event DownloadRowEventHandler           OutputDirectoryClick;
        public event DownloadRowEventHandler           LiveStreamMaxFileSizeClick;
        public event MouseClickRightButtonEventHandler MouseClickRightButton;
        public event EventHandler                      DoubleClickEx;
#pragma warning disable CS0067
        public event DownloadRowEventHandler           UpdatedSingleRunningRow;
#pragma warning restore CS0067
        //---public event EventHandler                      EnterKeyDown;

        private DataGrid DGV;
        private DownloadListModel _Model;
        #endregion

        #region [.ctor().]
        public DownloadListUC()  => this.InitializeComponent();
        private void InitializeComponent()//__PREV()
        {
            AvaloniaXamlLoader.Load( this );

            DGV = this.FindControl< DataGrid >( nameof(DGV) );
            DGV.SelectionChanged   += DGV_SelectionChanged;
            DGV.CellPointerPressed += DGV_CellPointerPressed;
            DGV.DoubleTapped       += (s, e) => DoubleClickEx?.Invoke( s, e );
            DGV.PointerPressed     += DGV_PointerPressed;
            //---DGV.KeyDown            += DGV_KeyDown;

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

            //this.Styles.Add_NoThrow( GlobalStyles.Dark );
            //foreach ( var style in this.Styles )
            //{
            //    DGV.Styles.Add_NoThrow( style );
            //}            
        }
        #endregion

        #region [.DGV events.]
        private void DGV_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            var selectedDownloadRow = this.GetSelectedDownloadRow();
            SelectionChanged?.Invoke( selectedDownloadRow );

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
            //        break;
            //    }
            //} 
            #endregion
        }
        private void DGV_PointerPressed( object sender, PointerPressedEventArgs e )
        {
            var p = e.GetCurrentPoint( this/*null*/ );
            if ( p.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed )
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
        private void DGV_CellPointerPressed( object sender, DataGridCellPointerPressedEventArgs e )
        {
            const int OutputFileName_Column_DisplayIndex        = 0;
            const int OutputDirectory_Column_DisplayIndex       = 1;
            const int LiveStreamMaxFileSize_Column_DisplayIndex = 11;

            var p = e.PointerPressedEventArgs.GetCurrentPoint( this/*null*/ );
            switch ( p.Properties.PointerUpdateKind ) //e.PointerPressedEventArgs.MouseButton )
            {
                case PointerUpdateKind.LeftButtonPressed: //MouseButton.Left:
                    var columnDisplayIndex = e.Column.DisplayIndex;
                    switch ( columnDisplayIndex )
                    {
                        case OutputFileName_Column_DisplayIndex:
                        case OutputDirectory_Column_DisplayIndex:
                        case LiveStreamMaxFileSize_Column_DisplayIndex:
                        {
                            bool is_valid( int rowIndex_, DownloadRow selectedDownloadRow_ ) => ((0 <= rowIndex_) && (rowIndex_ < _Model.RowsCount) && (_Model[ rowIndex_ ] == selectedDownloadRow_));

                            var selectedDownloadRow = this.GetSelectedDownloadRow();
                            var rowIndex            = e.Row.GetIndex();
                            if ( is_valid( rowIndex, selectedDownloadRow ) )
                            {
                                var evnt = default(DownloadRowEventHandler);
                                switch ( columnDisplayIndex )
                                {                                    
                                    case OutputDirectory_Column_DisplayIndex: evnt = OutputDirectoryClick; break;
                                    case OutputFileName_Column_DisplayIndex:
                                        if ( (e.PointerPressedEventArgs.Source is Image img) && (img.Name == "live_stream") && _Model[ rowIndex ].IsLiveStream )
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
                                    e.PointerPressedEventArgs.Pointer.Capture( null );
                                    e.PointerPressedEventArgs.Handled = true;
                                    evnt( selectedDownloadRow );
                                    
                                    //Dispatcher.UIThread.Post( () => evnt( selectedDownloadRow ) );
                                }
                            }
                        }
                        break;
                    }
                break;

                case PointerUpdateKind.RightButtonPressed: //MouseButton.Right:
                    {
                        var evnt = MouseClickRightButton;
                        if ( evnt != null )
                        {
                            //---var row = (e.PointerPressedEventArgs.Source as IControl)?.GetSelfAndVisualAncestors().OfType< DataGridRow >().FirstOrDefault();
                            var row = (e.PointerPressedEventArgs.Source as Control)?.GetSelfAndVisualAncestors().OfType< DataGridRow >().FirstOrDefault();
                            if ( row != null )
                            {
                                DGV.SelectedIndex = row.GetIndex();
                            }

                            e.PointerPressedEventArgs.Pointer.Capture( null );
                            e.PointerPressedEventArgs.Handled = true;
                            evnt( p.Position, this.GetSelectedDownloadRow() );
                        }
                    }
                break;
            }
        }
        /*--NOT WORKING--
        private void DGV_KeyDown( object sender, KeyEventArgs e )
        {
            if ( (e.Key == Key.Enter) && (e.KeyModifiers == KeyModifiers.None) )
            {
                var ev = EnterKeyDown;
                if ( ev != null )
                {
                    e.Handled = true;
                    ev( sender, e );
                }
            }
        }*/
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
                //---DGV.Items = null;
                DGV.ItemsSource = null;
            }
            else
            {
                //---DGV.Items = new DataGridCollectionView( _Model.GetRows() );
                DGV.ItemsSource = new DataGridCollectionView( _Model.GetRows() );
            }
        }

        internal DownloadRow GetSelectedDownloadRow() => (DGV.SelectedItem as DownloadRow);
        internal bool SelectDownloadRow( DownloadRow row ) => SelectDownloadRowInternal( row );
        private bool SelectDownloadRowInternal( DownloadRow row ) //---, bool callAfterSort = false )
        {
            if ( row != null )
            {
                DGV.SelectedItem = row;
                return (true);

                #region comm.
                //var visibleIndex = row.GetVisibleIndex();
                //if ( (0 <= visibleIndex) && (visibleIndex < DGV.RowCount) )
                //{
                //    var dtrow = DGV.Rows[ visibleIndex ];
                //    if ( dtrow.Selected )
                //    {
                //        SelectionChanged?.Invoke( row );
                //    }
                //    else
                //    {
                //        dtrow.Selected = true;
                //    }
                //    if ( !callAfterSort )
                //    {
                //        _UserMade_DGV_SelectionChanged = false;
                //    }
                //    return (true);
                //} 
                #endregion
            }
            return (false);
        }
        internal bool HasFocus => (DGV.IsFocused || this.IsFocused);

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
        }
        private void DetachModel()
        {
            if ( _Model != null )
            {
                _Model.CollectionChanged -= Model_CollectionChanged;
                //_Model.RowPropertiesChanged -= Model_RowPropertiesChanged;
                _Model = null;

                //---DGV.Items = null;
                DGV.ItemsSource = null;
            }
        }

        private void Model_CollectionChanged( _CollectionChangedTypeEnum_ changedType, DownloadRow _ )
        {
            switch ( changedType )
            {
                case _CollectionChangedTypeEnum_.Sort:
                    DGV.InvalidateVisual();
                break;

                case _CollectionChangedTypeEnum_.Add:
                    SetDataGridItems();
                break;

                case _CollectionChangedTypeEnum_.Add_Bulk:
                case _CollectionChangedTypeEnum_.Remove:
                case _CollectionChangedTypeEnum_.Remove_Bulk:
                case _CollectionChangedTypeEnum_.Clear:
                //case _CollectionChangedTypeEnum_.BulkUpdate:
                    SetDataGridItems();
                    DGV.InvalidateVisual();
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
                    var speedText = Extensions.GetSpeedText( downloadBytes, elapsedSeconds, row.GetInstantaneousSpeedInMbps() );
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
                    return (Extensions.GetSpeedText( downloadBytes, elapsedSeconds, row.GetInstantaneousSpeedInMbps() ));
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
