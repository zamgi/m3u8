using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using m3u8.download.manager.models;
using m3u8.download.manager.Properties;
using _CollectionChangedTypeEnum_ = m3u8.download.manager.models.DownloadListModel.CollectionChangedTypeEnum;
using CellStyle   = System.Windows.Forms.DataGridViewCellStyle;
using HitTestInfo = System.Windows.Forms.DataGridView.HitTestInfo;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class DownloadListUC : UserControl
    {
        /// <summary>
        /// 
        /// </summary>
        public delegate void SelectionChangedEventHandler( DownloadRow row );
        /// <summary>
        /// 
        /// </summary>
        public delegate void OutputFileNameClickEventHandler( DownloadRow row );
        /// <summary>
        /// 
        /// </summary>
        public delegate void OutputDirectoryClickEventHandler( DownloadRow row );
        /// <summary>
        /// 
        /// </summary>
        public delegate void LiveStreamMaxFileSizeClickEventHandler( DownloadRow row );
        /// <summary>
        /// 
        /// </summary>
        public delegate void MouseClickRightButtonEventHandler( MouseEventArgs e, DownloadRow selectedRow, bool outOfGridArea );
        /// <summary>
        /// 
        /// </summary>
        public delegate void UpdatedSingleRunningRowEventHandler( DownloadRow row );
        /// <summary>
        /// 
        /// </summary>
        public delegate bool IsDrawCheckMarkDelegate( DownloadRow row );

        #region [.column index's.]
        private const int OUTPUTFILENAME_COLUMN_INDEX            = 0;
        private const int OUTPUTDIRECTORY_COLUMN_INDEX           = 1;
        private const int STATUS_COLUMN_INDEX                    = 2;
        private const int DOWNLOAD_PROGRESS_COLUMN_INDEX         = 3;
        private const int DOWNLOAD_TIME_COLUMN_INDEX             = 4;
        private const int APPROX_REMAINED_TIME_COLUMN_INDEX      = 5;
        private const int DOWNLOAD_SPEED_COLUMN_INDEX            = 6;
        private const int DOWNLOAD_BYTES_COLUMN_INDEX            = 7;
        private const int APPROX_REMAINED_BYTES_COLUMN_INDEX     = 8;
        private const int APPROX_TOTAL_BYTES_COLUMN_INDEX        = 9;
        private const int IS_LIVE_STREAM_COLUMN_INDEX            = 10;
        private const int LIVE_STREAM_MAX_FILE_SIZE_COLUMN_INDEX = 11;
        private const int URL_COLUMN_INDEX                       = 12;
        #endregion

        #region [.field's.]
        public event SelectionChangedEventHandler           SelectionChanged;
        public event OutputFileNameClickEventHandler        OutputFileNameClick;
        public event OutputDirectoryClickEventHandler       OutputDirectoryClick;
        public event LiveStreamMaxFileSizeClickEventHandler LiveStreamMaxFileSizeClick;
        public event MouseClickRightButtonEventHandler      MouseClickRightButton;
        public event UpdatedSingleRunningRowEventHandler    UpdatedSingleRunningRow;
        public event EventHandler                           DoubleClickEx;
        public event IsDrawCheckMarkDelegate                IsDrawCheckMark;

        private DownloadListModel _Model;
        //private CellStyle _DefaultCellStyle;
        private CellStyle         _ErrorCellStyle;
        private CellStyle         _CanceledCellStyle;
        private CellStyle         _FinishedCellStyle;
        private RowNumbersPainter _RNP;
        private StringFormat      _SF_Left;
        private StringFormat      _SF_Center;
        private StringFormat      _SF_Right;
        private bool              _UserMade_DGV_SelectionChanged;
        private SortInfo          _LastSortInfo;
        private Action            _RestoreSortIfNeedAction;
        private Timer             _CommonUpdateTimer;
        private Settings          _Settings;
        private ContextMenuStrip  _ColumnsContextMenu;
        #endregion

        #region [.ctor().]
        public DownloadListUC()
        {
            InitializeComponent();
            //----------------------------------------//

            _Settings = Settings.Default;
            _LastSortInfo = SortInfo.FromJson( _Settings.LastSortInfoJson );

            _RestoreSortIfNeedAction = new Action( RestoreSortIfNeed );
            _CommonUpdateTimer = new Timer() { Interval = 1_000, Enabled = false };
            _CommonUpdateTimer.Tick += CommonUpdateTimer_Tick;

            _ErrorCellStyle    = new CellStyle( DGV.DefaultCellStyle )
            {
                ForeColor          = Color.Red,
                //BackColor          = Color.Red,
                SelectionForeColor = Color.White,
                SelectionBackColor = Color.Red,
                //Font = new Font( DGV.Font, FontStyle.Bold )
            };
            _CanceledCellStyle = new CellStyle( DGV.DefaultCellStyle )
            {
                ForeColor          = Color.DimGray,
                SelectionForeColor = Color.WhiteSmoke,
                //Font               = new Font( DGV.Font, FontStyle.Strikeout )

                //ForeColor          = Color.Red,
                //BackColor          = Color.Yellow,
                //SelectionForeColor = Color.Yellow,
                //SelectionBackColor = Color.Red,
            };
            _FinishedCellStyle = new CellStyle( DGV.DefaultCellStyle ) { Font = new Font( DGV.Font, FontStyle.Bold ) };

            _RNP = RowNumbersPainter.Create( DGV );
            _SF_Left   = new StringFormat( StringFormatFlags.NoWrap ) { Trimming = StringTrimming.EllipsisCharacter, Alignment = StringAlignment.Near  , LineAlignment = StringAlignment.Center };
            _SF_Center = new StringFormat( StringFormatFlags.NoWrap ) { Trimming = StringTrimming.EllipsisCharacter, Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            _SF_Right  = new StringFormat( StringFormatFlags.NoWrap ) { Trimming = StringTrimming.EllipsisCharacter, Alignment = StringAlignment.Far   , LineAlignment = StringAlignment.Center };
            //----------------------------------------//

            CreateColumnsContextMenu();            
        }
        private void CreateColumnsContextMenu()
        {
            _ColumnsContextMenu = new ContextMenuStrip();
            EventHandler menuItemClick = (s, e) =>
            {
                var item = (ToolStripMenuItem) s;
                ((DataGridViewColumn) item.Tag).Visible = item.Checked;
            };
            foreach ( var col in DGV.Columns.Cast< DataGridViewColumn >() )
            {
                _ColumnsContextMenu.Items.Add( new ToolStripMenuItem( col.HeaderText, null, menuItemClick ) { CheckOnClick = true, Tag = col, Enabled = (col.Index != OUTPUTFILENAME_COLUMN_INDEX) } );
            }
            //-------------------------------------------------//

            _ColumnsContextMenu.Items.Add( new ToolStripSeparator() );
            EventHandler resetMenuItemClick = (s, e) =>
            {
                foreach ( var item in _ColumnsContextMenu.Items.OfType< ToolStripMenuItem >() )
                {
                    if ( item.Tag is DataGridViewColumn col )
                    {
                        col.Visible      = true;
                        col.DisplayIndex = col.Index;
                        col.Width        = Convert.ToInt32( col.FillWeight );                        
                    }
                }
            };
            _ColumnsContextMenu.Items.Add( new ToolStripMenuItem( "Reset all columns", null, resetMenuItemClick ) );
            //-------------------------------------------------//

            _ColumnsContextMenu.Opening += (s, e) =>
            {
                foreach ( var item in _ColumnsContextMenu.Items.OfType< ToolStripMenuItem >() )
                {
                    if ( item.Tag is DataGridViewColumn col )
                    {
                        item.Checked = col.Visible;
                    }
                }
            };
        }

        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();
                _RNP       .Dispose();
                _SF_Left   .Dispose();
                _SF_Center .Dispose();
                _SF_Right  .Dispose();
            }
            base.Dispose( disposing );
        }
        #endregion

        #region [.Model.]
        public DownloadRow GetSelectedDownloadRow()
        {
            DataGridViewRow dtrow;
            switch ( DGV.SelectedRows.Count )
            {
                case 0: 
                    return (null);
                case 1:
                    dtrow = DGV.SelectedRows[ 0 ];
                    break;
                default:
                    dtrow = DGV.SelectedRows.Cast< DataGridViewRow >().OrderBy( r => r.Index ).FirstOrDefault();
                    break;
            }
            var row = ((dtrow != null) && (dtrow.Index < _Model.RowsCount)) ? _Model[ dtrow.Index ] : null;
            return (row);
        }
        public IReadOnlyList< DownloadRow > GetSelectedDownloadRows()
        {
            var srs = DGV.SelectedRows;
            var lst = new List< DownloadRow >( srs.Count );
            foreach ( var dtrow in srs.Cast< DataGridViewRow >().OrderBy( r => r.Index ) )
            {
                if ( dtrow.Index < _Model.RowsCount )
                {
                    lst.Add( _Model[ dtrow.Index ] );
                }
            }
            return (lst);
        }
        public int GetSelectedDownloadRowsCount() => DGV.SelectedRows.Count;
        //{
        //    var cnt = 0;
        //    foreach ( var dtrow in DGV.SelectedRows.Cast< DataGridViewRow >() )
        //    {
        //        if ( dtrow.Index < _Model.RowsCount )
        //        {
        //            cnt++;
        //        }
        //    }
        //    return (cnt);
        //}
        public async Task SelectDownloadRowDelay( DownloadRow row, int millisecondsDelay = 100 ) 
        {
            await Task.Delay( millisecondsDelay );
            SelectDownloadRowInternal( row );
        }
        public bool SelectDownloadRow( DownloadRow row ) => SelectDownloadRowInternal( row );
        [M(O.AggressiveInlining)] private void SelectOneRow( int rowIndex )
        {
            DGV.ClearSelection();
            DGV.Rows[ rowIndex ].Selected = true;
        }
        private bool SelectDownloadRowInternal( DownloadRow row, bool callAfterSort = false )
        {
            if ( row != null )
            {
                var visibleIndex = row.GetVisibleIndex();
                if ( (0 <= visibleIndex) && (visibleIndex < DGV.RowCount) )
                {
                    SelectOneRow( visibleIndex );
                    if ( !callAfterSort )
                    {
                        _UserMade_DGV_SelectionChanged = false;
                    }
                    return (true);
                }
            }
            return (false);
        }
        private void SelectDownloadRows( IReadOnlyList< DownloadRow > rows, DownloadRow focusedRow )
        {
            DGV.ClearSelection();

            var visibleIndex = focusedRow.GetVisibleIndex();
            if ( (0 <= visibleIndex) && (visibleIndex < DGV.RowCount) )
            {
                DGV.CurrentCell = DGV.Rows[ visibleIndex ].Cells[ 0 ];
            }
            foreach ( var row in rows )
            {
                visibleIndex = row.GetVisibleIndex();
                if ( (0 <= visibleIndex) && (visibleIndex < DGV.RowCount) )
                {
                    DGV.Rows[ visibleIndex ].Selected = true;
                    _UserMade_DGV_SelectionChanged = false;
                }
            }
        }
        public bool HasFocus => (DGV.Focused || this.Focused);

        public DownloadListModel Model => _Model;
        public void SetModel( DownloadListModel model )
        {
            DetachModel();

            _Model = model ?? throw (new ArgumentNullException( nameof(model) ));
            _Model.CollectionChanged    -= Model_CollectionChanged;
            _Model.CollectionChanged    += Model_CollectionChanged;
            _Model.RowPropertiesChanged -= Model_RowPropertiesChanged;
            _Model.RowPropertiesChanged += Model_RowPropertiesChanged;
            Model_CollectionChanged( _CollectionChangedTypeEnum_.Add, null );
        }
        private void DetachModel()
        {
            if ( _Model != null )
            {
                _Model.CollectionChanged    -= Model_CollectionChanged;
                _Model.RowPropertiesChanged -= Model_RowPropertiesChanged;
                _Model = null;

                DGV.CellValueNeeded -= DGV_CellValueNeeded;
                DGV.CellFormatting  -= DGV_CellFormatting;
                try
                {
                    DGV.RowCount = 0;
                }
                finally
                {
                    DGV.CellValueNeeded += DGV_CellValueNeeded;
                    DGV.CellFormatting  += DGV_CellFormatting;
                }
            }
        }

        private void Model_CollectionChanged( _CollectionChangedTypeEnum_ collectionChangedType, DownloadRow row )
        {
            if ( this.InvokeRequired )
            {
                this.BeginInvoke( Model_CollectionChanged, collectionChangedType, row );
                return;
            }

            switch ( collectionChangedType )
            {
                case _CollectionChangedTypeEnum_.Sort:
                    DGV.Refresh();
                break;

                case _CollectionChangedTypeEnum_.Add:
                {
                    var v = _UserMade_DGV_SelectionChanged;
                    DGV.RowCount = _Model.RowsCount;
                    RestoreSortIfNeed();
                    if ( v != _UserMade_DGV_SelectionChanged )
                    {
                        _UserMade_DGV_SelectionChanged = false;
                    }
                    _CommonUpdateTimer.Enabled = true;
                }
                break;

                case _CollectionChangedTypeEnum_.Remove:
                case _CollectionChangedTypeEnum_.Clear:
                case _CollectionChangedTypeEnum_.BulkUpdate:
                {
                    #region [.save selected row.]
                    var selectedVisibleIndex = (DGV.SelectedRows.Cast< DataGridViewRow >().FirstOrDefault()?.Index).GetValueOrDefault( -1 );
                    #endregion

                    DGV.CellValueNeeded  -= DGV_CellValueNeeded;
                    DGV.CellFormatting   -= DGV_CellFormatting;
                    DGV.SelectionChanged -= DGV_SelectionChanged;
                    DGV.CellPainting     -= DGV_CellPainting;
                    try
                    {
                        DGV.RowCount = _Model.RowsCount;
                        RestoreSortIfNeed();
                    }
                    finally
                    {
                        DGV.CellValueNeeded  += DGV_CellValueNeeded;
                        DGV.CellFormatting   += DGV_CellFormatting;
                        DGV.SelectionChanged += DGV_SelectionChanged;
                        DGV.CellPainting     += DGV_CellPainting;
                    }

                    #region [.restore selected row.]
                    try
                    {
                        var hasRows = (0 < DGV.RowCount);
                        _CommonUpdateTimer.Enabled = hasRows;
                        if ( hasRows )
                        {
                            var visibleIndex = Math.Min( Math.Max( 0, selectedVisibleIndex ), DGV.RowCount - 1 ); // ((0 <= selectedVisibleIndex) && (selectedVisibleIndex < DGV.RowCount)) ? selectedVisibleIndex : (DGV.RowCount - 1); //0;
                            SelectOneRow( visibleIndex );
                        }
                        else
                        {                            
                            SelectionChanged?.Invoke( null );
                        }
                    }
                    catch ( Exception ex)
                    {
                        Debug.WriteLine( ex );
                    }
                    #endregion

                    DGV.Invalidate();
                }
                break;
            }
        }
        private void Model_RowPropertiesChanged( DownloadRow row, string propertyName )
        {
            var visibleIndex = row.GetVisibleIndex();
            if ( (0 <= visibleIndex) && (visibleIndex < DGV.RowCount) )
            {                
                DGV.InvalidateRow( visibleIndex );

                if ( propertyName == nameof(DownloadRow.Status) )
                {
                    RestoreSortIfNeed();
                }
            }
        }

        internal IEnumerable< DataGridViewColumn > GetDataGridColumns() => DGV.Columns.Cast< DataGridViewColumn >();
        internal IEnumerable< DataGridViewColumn > GetAlwaysVisibleDataGridColumns() => new[] { DGV_outputFileNameColumn };
        #endregion

        #region [.private methods.]
        private const string CREATED_DT = "HH:mm:ss  (yyyy.MM.dd)";
        private const string HH_MM_SS   = "hh\\:mm\\:ss";
        private const string MM_SS      = "mm\\:ss";

        [M(O.AggressiveInlining)] public  static string GetDownloadInfoText( DownloadRow row )
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
            //var failedParts  = ((row.FailedDownloadParts != 0) ? $" (failed: {row.FailedDownloadParts})" : null);
            var downloadInfo = $"{percent}%, ({elapsed})";
            
            #region [.speed.]
            if ( !st.IsPaused() )
            {                
                var elapsedSeconds = ts.TotalSeconds;
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

        [M(O.AggressiveInlining)] private static bool TryGetDownloadProgress( DownloadRow row, out double part, out string progressText )
        {
            var st = row.Status;
            switch ( st )
            {
                case DownloadStatus.Created:
                case DownloadStatus.Started:
                case DownloadStatus.Wait   :
                    part         = default;
                    progressText = null;
                    return (false);

                default:
                    string percentText;
                    if ( 0 < row.TotalParts )
                    {
                        part        = (1.0 * row.SuccessDownloadParts) / row.TotalParts;                        
                        var percent = (row.TotalParts <= (row.SuccessDownloadParts + row.FailedDownloadParts)) ? 100 : Extensions.Min( (byte) (100 * part), 99 );
                        percentText = percent.ToString();
                    }
                    else if ( st == DownloadStatus.Canceled ) //not-started
                    {
                        part         = default;
                        progressText = null;
                        return (false);
                    }
                    else
                    {
                        part        = 0;
                        percentText = "-";
                    }

                    var failedParts = ((row.FailedDownloadParts != 0) ? $" (failed: {row.FailedDownloadParts})" : null);
                    progressText = $"{percentText}%  ({row.SuccessDownloadParts} of {row.TotalParts}{failedParts})";
                    return (true);
            }
        }
        [M(O.AggressiveInlining)] private static string GetDownloadTimeText( DownloadRow row )
        {
            if ( row.Status == DownloadStatus.Created )
            {
                return (row.CreatedOrStartedDateTime.ToString( CREATED_DT ));
            }
            return (row.GetElapsed().ToString( HH_MM_SS ));
        }
        [M(O.AggressiveInlining)] private static string GetApproxRemainedTimeText( DownloadRow row )
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
        [M(O.AggressiveInlining)] private static string GetDownloadSpeedText( DownloadRow row )
        {
            if ( !row.Status.IsPaused() )
            {                
                var elapsedSeconds = row.GetElapsed().TotalSeconds;
                var downloadBytes  = row.GetDownloadBytesLengthAfterLastRun();
                if ( (1_024 < downloadBytes) && (2.5 <= elapsedSeconds) )
                {
                    return (Extensions.GetSpeedText( downloadBytes, elapsedSeconds, row.GetInstantaneousSpeedInMbps() ));
                }
            }
            return (string.Empty);
        }
        [M(O.AggressiveInlining)] private static string GetDisplaySizeText( long size )
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
        [M(O.AggressiveInlining)] private static string GetApproxRemainedBytesText( DownloadRow row )
        {
            var size = row.GetApproxRemainedBytes();
            return (size.HasValue ? GetDisplaySizeText( size.Value ) : string.Empty);
        }
        [M(O.AggressiveInlining)] private static string GetApproxTotalBytesText( DownloadRow row )
        {
            var size = row.GetApproxTotalBytes();
            return (size.HasValue ? GetDisplaySizeText( size.Value ) : string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        private struct SortHelper
        {
            private Comparison< DownloadRow > _Comparison;
            private int _Coeff;

            private int Comparison( DownloadRow x, DownloadRow y )
            {
                var d = _Comparison( x, y );
                if ( d == 0 )
                {
                    d = x.CreatedOrStartedDateTime.CompareTo( y.CreatedOrStartedDateTime );
                }
                else
                {
                    d *= _Coeff;
                }
                return (d);
            }

            public static Comparison< DownloadRow > CreateComparison( Comparison< DownloadRow > comparison, int coeff )
            {
                var sh = new SortHelper() { _Comparison = comparison, _Coeff = coeff };
                return (sh.Comparison);
            }
            public static Comparison< DownloadRow > CreateDefaultComparison()
            {
                var sh = new SortHelper() { _Comparison = (x, y) => x.CreatedOrStartedDateTime.CompareTo( y.CreatedOrStartedDateTime ), _Coeff = 1 };
                return (sh.Comparison);
            }

            [M(O.AggressiveInlining)] public static int ToInt32( DownloadStatus status )
            {
                switch ( status )
                {
                    case DownloadStatus.Running:  return (0);
                    case DownloadStatus.Started:  return (1);
                    case DownloadStatus.Paused:   return (2);
                    case DownloadStatus.Wait:     return (3);
                    case DownloadStatus.Canceled: return (4);
                    case DownloadStatus.Error:    return (5);
                    case DownloadStatus.Finished: return (6);
                    case DownloadStatus.Created:  return (7);

                    default: return (int.MaxValue);
                }
            }

        }

        private async void RestoreSortIfNeed()
        {
            if ( this.InvokeRequired )
            {
                this.BeginInvoke( _RestoreSortIfNeedAction );
                return;
            }

            if ( !_LastSortInfo.TryGetSorting( out var columnIndex, out var sortOrder ) )
            {
                DGV.ClearHeaderSortGlyphDirection();
                return;
            }

            #region [.get comparison routine.]
            var comparison = default(Comparison< DownloadRow >);
            switch ( columnIndex )
            {
                case OUTPUTFILENAME_COLUMN_INDEX:
                    comparison = (x, y) => string.Compare( x.OutputFileName, y.OutputFileName, true );
                break;

                case OUTPUTDIRECTORY_COLUMN_INDEX:
                    comparison = (x, y) => string.Compare( x.OutputDirectory, y.OutputDirectory, true );
                break;

                case STATUS_COLUMN_INDEX:
                    comparison = (x, y) => SortHelper.ToInt32( x.Status ).CompareTo( SortHelper.ToInt32( y.Status ) );
                break;

                case DOWNLOAD_PROGRESS_COLUMN_INDEX:
                    comparison = (x, y) => x.SuccessDownloadParts.CompareTo( y.SuccessDownloadParts );
                break;

                case DOWNLOAD_BYTES_COLUMN_INDEX:
                    comparison = (x, y) => x.DownloadBytesLength.CompareTo( y.DownloadBytesLength );
                break;

                case APPROX_REMAINED_BYTES_COLUMN_INDEX:
                    comparison = (x, y) => x.GetApproxRemainedBytes().CompareTo( y.GetApproxRemainedBytes() );
                break;

                case APPROX_TOTAL_BYTES_COLUMN_INDEX:
                    comparison = (x, y) => x.GetApproxTotalBytes().CompareTo( y.GetApproxTotalBytes() );
                break;

                case URL_COLUMN_INDEX:
                    comparison = (x, y) => string.Compare( x.Url, y.Url, true );
                break;

                case IS_LIVE_STREAM_COLUMN_INDEX:
                    comparison = (x, y) => x.IsLiveStream.CompareTo( y.IsLiveStream );
                break;
                case LIVE_STREAM_MAX_FILE_SIZE_COLUMN_INDEX:
                    comparison = (x, y) => x.LiveStreamMaxFileSizeInBytes.CompareTo( y.LiveStreamMaxFileSizeInBytes );
                break;

                case DOWNLOAD_TIME_COLUMN_INDEX:
                case APPROX_REMAINED_TIME_COLUMN_INDEX:
                case DOWNLOAD_SPEED_COLUMN_INDEX:
                    //---comparison = (x, y) => x..CompareTo( y. );
                    return;
               
                default:
                    DGV.ClearHeaderSortGlyphDirection();
                    throw (new NotImplementedException( $"columnIndex: {columnIndex}" ));
            }
            #endregion

            var coeff = ((sortOrder == SortOrder.Ascending) ? 1 : -1);
            var row   = GetSelectedDownloadRow();

            _Model.Sort( SortHelper.CreateComparison( comparison, coeff ) );

            DGV.SetHeaderSortGlyphDirection( columnIndex, sortOrder );
            await Task.Delay( 1 );
            SelectDownloadRowInternal( row, true );
        }
        private void CommonUpdateTimer_Tick( object sender, EventArgs e )
        {
            DGV.Refresh();
            //---------------------------------------------//

            if ( _Model.TryGetSingleRunning( out var singleRunningRow ) )
            {
                UpdatedSingleRunningRow?.Invoke( singleRunningRow );
            }
        }
        #endregion

        #region [.DGV.]
        private void DGV_CellValueNeeded( object sender, DataGridViewCellValueEventArgs e )
        {
            var row = _Model[ e.RowIndex ];
            switch ( e.ColumnIndex )
            {
                case OUTPUTFILENAME_COLUMN_INDEX       : e.Value = row.OutputFileName;                                    break;
                case OUTPUTDIRECTORY_COLUMN_INDEX      : e.Value = row.OutputDirectory;                                   break;
                case STATUS_COLUMN_INDEX               : e.Value = row.Status.ToString()/*$"               {row.Status}"*/;break;
                case DOWNLOAD_PROGRESS_COLUMN_INDEX    : 
                    //e.Value = new string( ' ', 30 );
                    e.Value = TryGetDownloadProgress( row, out _, out var progressText ) ? progressText : string.Empty/*new string( ' ', 30 )*/;
                    break;
                case DOWNLOAD_TIME_COLUMN_INDEX            : e.Value = GetDownloadTimeText       ( row );                     break;
                case APPROX_REMAINED_TIME_COLUMN_INDEX     : e.Value = GetApproxRemainedTimeText ( row );                     break;
                case DOWNLOAD_SPEED_COLUMN_INDEX           : e.Value = GetDownloadSpeedText      ( row );                     break;
                case DOWNLOAD_BYTES_COLUMN_INDEX           : e.Value = GetDisplaySizeText        ( row.DownloadBytesLength ); break;
                case APPROX_REMAINED_BYTES_COLUMN_INDEX    : e.Value = GetApproxRemainedBytesText( row );                     break;
                case APPROX_TOTAL_BYTES_COLUMN_INDEX       : e.Value = GetApproxTotalBytesText   ( row );                     break;
                case URL_COLUMN_INDEX                      : e.Value = row.Url;                                               break;
                case IS_LIVE_STREAM_COLUMN_INDEX           : e.Value = row.IsLiveStream ? "YES" : "-"; break;
                case LIVE_STREAM_MAX_FILE_SIZE_COLUMN_INDEX: e.Value = row.IsLiveStream ? $"{row.GetLiveStreamMaxFileSizeInMb()} mb" : "-"; break;
            }
        }
        private void DGV_CellFormatting( object sender, DataGridViewCellFormattingEventArgs e )
        {
            var row = _Model[ e.RowIndex ];
            switch ( row.Status )
            {
                case DownloadStatus.Error:
                    e.CellStyle         = _ErrorCellStyle;
                    e.FormattingApplied = true;
                break;

                case DownloadStatus.Canceled:
                    e.CellStyle         = _CanceledCellStyle;
                    e.FormattingApplied = true;
                break;

                case DownloadStatus.Finished:
                    e.CellStyle         = _FinishedCellStyle;
                    e.FormattingApplied = true;
                break;
            }
        }
        
        private void DGV_SelectionChanged( object sender, EventArgs e ) //---private void DGV_CurrentCellChanged( object sender, EventArgs e )
        {
            _UserMade_DGV_SelectionChanged = true;
            var selectedDownloadRow = this.GetSelectedDownloadRow();
            SelectionChanged?.Invoke( selectedDownloadRow );

            if ( (selectedDownloadRow != null) && !selectedDownloadRow.IsFinished() )
            {
                var pt = DGV.PointToClient( Control.MousePosition );
                var ht = DGV.HitTest( pt.X, pt.Y );
                switch ( ht.ColumnIndex )
                {
                    case OUTPUTFILENAME_COLUMN_INDEX:
                    case OUTPUTDIRECTORY_COLUMN_INDEX:
                    case LIVE_STREAM_MAX_FILE_SIZE_COLUMN_INDEX:
                        DGV.SetHandCursorIfNonHand();
                        break;
                }
            }
        }
        /*private DataGridViewRow _PrevCurrentRow;
        private void DGV_CurrentCellChanged( object sender, EventArgs e )
        {
            var cs = DGV.CurrentCell;
            Debug.WriteLine( $"DGV.CurrentCell.RowIndex: {cs?.RowIndex.ToString() ?? "NULL"}" );

            if ( cs != null )
            {
                var row = DGV.Rows[ cs.RowIndex ];
                if ( row != _PrevCurrentRow )
                {
                    if ( _PrevCurrentRow != null )
                    {
                        _PrevCurrentRow.DefaultCellStyle = null;
                    }
                    row.DefaultCellStyle = _CurrentRowCellStyle;
                    _PrevCurrentRow = row;

                    DGV_SelectionChanged( sender, e );
                }
            }
        }
        //*/
        private void DGV_CellMouseEnter( object sender, DataGridViewCellEventArgs e )
        {
            [M(O.AggressiveInlining)] bool is_need_change_cursor( int columnIndex )
            {
                switch ( columnIndex )
                {
                    case OUTPUTFILENAME_COLUMN_INDEX:
                    case OUTPUTDIRECTORY_COLUMN_INDEX:
                        return (true);

                    case LIVE_STREAM_MAX_FILE_SIZE_COLUMN_INDEX:
                        return (_Model[ e.RowIndex ].IsLiveStream);
                    default: 
                        return (false);
                }
            };

            if ( (0 <= e.RowIndex) && is_need_change_cursor( e.ColumnIndex ) && 
                 DGV.Rows[ e.RowIndex ].Selected /*&& !_Model[ e.RowIndex ].IsFinished()*/ )
            {
                DGV.SetHandCursorIfNonHand();
            }
            else
            {
                DGV.SetDefaultCursorIfHand();
            }
        }
        private void DGV_CellMouseLeave( object sender, DataGridViewCellEventArgs e ) => DGV.SetDefaultCursorIfHand();
        private void DGV_CellMouseMove( object sender, DataGridViewCellMouseEventArgs e )
        {
            if ( (e.Button == MouseButtons.None) && (0 <= e.RowIndex) && (e.ColumnIndex == OUTPUTFILENAME_COLUMN_INDEX) )
            {
                var row = _Model[ e.RowIndex ];
                if ( row.IsLiveStream )
                {
                    var rc = GetIsLiveStreamImageRect( DGV.GetCellDisplayRectangle( e.ColumnIndex, e.RowIndex, cutOverflow: true ) );
                    var pt = DGV.PointToClient( Control.MousePosition );
                    if ( rc.Contains( pt /*e.Location*/ ) )
                    {
                        //toolTip.ShowAlways = true;
                        var f = this.FindForm();
                        toolTip.Show( $"Is Live Stream, (max single output file size: {row.GetLiveStreamMaxFileSizeInMb()} mb)", f, f.PointToClient( DGV.PointToScreen( pt ) ), duration: 1_500 );
                    }
                }
            }
        }
        private void DGV_CellClick( object sender, DataGridViewCellEventArgs e )
        {
            if ( !_UserMade_DGV_SelectionChanged && (0 <= e.RowIndex) && (DGV.SelectedRows.Count == 1) )
            {
                switch ( e.ColumnIndex )
                {
                    case OUTPUTFILENAME_COLUMN_INDEX:
                    {   
                        var row = _Model[ e.RowIndex ];
                        if ( row.IsLiveStream )
                        {
                            var rc = GetIsLiveStreamImageRect( DGV.GetCellDisplayRectangle( e.ColumnIndex, e.RowIndex, cutOverflow: true ) );
                            var pt = DGV.PointToClient( Control.MousePosition );
                            if ( rc.Contains( pt ) )
                            {
                                LiveStreamMaxFileSizeClick?.Invoke( row );
                                break;
                            }
                        }
                        
                        if ( !row.IsFinished() )
                        {
                            OutputFileNameClick?.Invoke( row );
                        }
                    }
                    break;

                    case OUTPUTDIRECTORY_COLUMN_INDEX:
                    {
                        var row = _Model[ e.RowIndex ];
                        if ( !row.IsFinished() )
                        {
                            OutputDirectoryClick?.Invoke( row );
                        }
                    }
                    break;

                    case LIVE_STREAM_MAX_FILE_SIZE_COLUMN_INDEX:
                    {
                        var row = _Model[ e.RowIndex ];
                        if ( row.IsLiveStream )
                        {
                            LiveStreamMaxFileSizeClick?.Invoke( row );
                        }
                    }
                    break;
                }
            }
            _UserMade_DGV_SelectionChanged = false;
        }

        [M(O.AggressiveInlining)] private static void CellPaintRoutine( DataGridViewCellPaintingEventArgs e )
        {
            e.Handled = true;
            var gr = e.Graphics;

            #region [.-1- bottom & right lines.]
            var rc = e.CellBounds; rc.Width--; rc.Height--;
            gr.DrawLines( Pens.DimGray, new[] { new Point( rc.X    , rc.Bottom ), new Point( rc.Right, rc.Bottom ),
                                                new Point( rc.Right, rc.Y      ), new Point( rc.Right, rc.Bottom ), } );
            #endregion
            #region [.-2- fill background.]
            var backColor = (e.State.IsSelected() ? e.CellStyle.SelectionBackColor : e.CellStyle.BackColor);
            using ( var br = new SolidBrush( backColor ) )
            {
                gr.FillRectangle( br, rc );
            }
            #endregion

            #region [.-3- fill background.]            
            rc.Inflate( -1, -1 );
            gr.FillRectangle( Brushes.White, rc );
            #endregion
        }
        private void DGV_CellPainting( object sender, DataGridViewCellPaintingEventArgs e )
        {
            if ( e.RowIndex < 0 ) return;

            if ( e.ColumnIndex == STATUS_COLUMN_INDEX )
            {
                CellPaintRoutine( e );

                #region [.-4- status image.]
                var row = _Model[ e.RowIndex ];
                var img = default(Image);
                switch ( row.Status )
                {
                    case DownloadStatus.Created : img = Resources.created ; break;
                    case DownloadStatus.Started : img = Resources.running ; break;
                    case DownloadStatus.Running : img = Resources.running ; break;
                    case DownloadStatus.Paused  : img = Resources.paused  ; break;
                    case DownloadStatus.Wait    : img = Resources.wait    ; break;
                    case DownloadStatus.Canceled: img = Resources.canceled; break;
                    case DownloadStatus.Finished: img = Resources.finished; break;
                    case DownloadStatus.Error   : img = Resources.error   ; break;
                }

                var gr = e.Graphics;
                Rectangle rc;
                if ( img != null )
                {
                    const int IMAGE_HEIGHT = 16;
                    rc = e.CellBounds;
                    rc = new Rectangle( rc.X + 2, rc.Y + (rc.Height - IMAGE_HEIGHT) / 2, IMAGE_HEIGHT, IMAGE_HEIGHT );
                    gr.DrawImage( img, rc );
                }
                #endregion

                #region [.-5- status text.]
                rc = e.CellBounds; rc.Inflate( -22, 0 );
                gr.DrawString( row.Status.ToString(), DGV.Font, Brushes.Black, rc, _SF_Left );
                #endregion

                #region [.-6- draw-check-mark.]
                if ( (IsDrawCheckMark != null) && IsDrawCheckMark( row ) )
                {
                    rc = e.CellBounds; //rc.Inflate( -2, 0 );
                    gr.DrawString( "\u2713", DGV.Font, Brushes.Green, rc, _SF_Right );
                }
                #endregion
            }
            else if ( e.ColumnIndex == DOWNLOAD_PROGRESS_COLUMN_INDEX )
            {
                CellPaintRoutine( e );

                #region [.progress-bar-text.]
                var row  = _Model[ e.RowIndex ];
                var has  = TryGetDownloadProgress( row, out var part, out var progressText );

                var gr = e.Graphics;
                var rc = e.CellBounds;
                StringFormat sf;
                if ( has )
                {
                    //rc.Inflate( -22, 0 );
                    sf = _SF_Center;
                    if ( 0 < part )
                    {
                        var progressBar = rc;
                        progressBar.Inflate( -3, -3 );
                        progressBar.Height--;
                        progressBar.Width = Convert.ToInt32( (progressBar.Width - 1) * part );
                        using ( var br = new SolidBrush( Color.LightBlue ) )
                        {
                            gr.FillRectangle( br, progressBar );
                        }
                    }
                }
                else
                {
                    rc.Inflate( -10, 0 );
                    sf           = _SF_Left;
                    progressText = "-";
                }
                gr.DrawString( progressText, DGV.Font, Brushes.Black, rc, sf );
                #endregion
            }
            else if ( e.ColumnIndex == OUTPUTFILENAME_COLUMN_INDEX )
            {
                #region [.IsLiveStream image in output-filename.]
                var row = _Model[ e.RowIndex ];
                if ( row.IsLiveStream )
                {
                    e.Handled = true;
                    e.Paint( e.ClipBounds, DataGridViewPaintParts.All );

                    var rc = GetIsLiveStreamImageRect( e.CellBounds );
                    e.Graphics.DrawImage( Resources.live_stream, rc );
                }
                #endregion
            }
        }
        [M(O.AggressiveInlining)] private static Rectangle GetIsLiveStreamImageRect( in Rectangle cellClipBounds )
        {
            const int IMAGE_HEIGHT = 16;
            var rc = new Rectangle( cellClipBounds.Right - IMAGE_HEIGHT - 5, cellClipBounds.Y + (cellClipBounds.Height - IMAGE_HEIGHT) / 2, IMAGE_HEIGHT, IMAGE_HEIGHT );
            return (rc);
        }
        private void DGV_ColumnHeaderMouseClick( object sender, DataGridViewCellMouseEventArgs e )
        {
            if ( (e.Button == MouseButtons.Left) && DGV.IsColumnSortable( e.ColumnIndex ) )
            {
                _LastSortInfo.SetSortOrderAndSaveCurrent( e.ColumnIndex );
                if ( !_LastSortInfo.HasSorting )
                {
                    _Model.Sort( SortHelper.CreateDefaultComparison() );
                }

                RestoreSortIfNeed();

                _Settings.LastSortInfoJson = _LastSortInfo.ToJson();
                _Settings.SaveNoThrow();
            }
            else if ( e.Button == MouseButtons.Right )
            {
                var displayIndex = DGV.Columns[ e.ColumnIndex ].DisplayIndex;
                var widthBefore  = DGV.Columns.Cast< DataGridViewColumn >().Where( c => c.Visible && c.DisplayIndex < displayIndex ).Sum( c => c.Width );
                var pt = new Point( widthBefore + e.X - DGV.HorizontalScrollingOffset, e.Y );
                _ColumnsContextMenu.Show( DGV, pt );
            }
        }

        #region [.draw select rows rect.]
        private void DGV_StartDrawSelectRect( object sender, EventArgs e ) => _CommonUpdateTimer.Enabled = false;
        private void DGV_EndDrawSelectRect( object sender, EventArgs e ) => _CommonUpdateTimer.Enabled = true;
        #endregion

        private HitTestInfo _DGV_MouseDown_HitTestInfo = HitTestInfo.Nowhere;
        private Point       _DGV_MouseDown_ButtonLeft_Location;
        private void DGV_MouseDown( object sender, MouseEventArgs e )
        {
            _DGV_MouseDown_HitTestInfo = DGV.HitTest( e.X, e.Y ); 
            if ( e.Button == MouseButtons.Left )
            {
                _DGV_MouseDown_ButtonLeft_Location = e.Location;
            }
        }
        private void DGV_MouseClick( object sender, MouseEventArgs e )
        {
            var ht = DGV.HitTest( e.X, e.Y );
            if ( 0 <= ht.RowIndex )
            {
                var allowed = (_DGV_MouseDown_HitTestInfo.RowIndex == ht.RowIndex) &&
                              ((_DGV_MouseDown_HitTestInfo.Type == DataGridViewHitTestType.Cell) || 
                               (_DGV_MouseDown_HitTestInfo.Type == DataGridViewHitTestType.RowHeader));
                if ( allowed )
                {
                    if ( e.Button == MouseButtons.Right )
                    {
                        //select actual row if current selected only one-or-zero row
                        if ( DGV.SelectedRows.Count <= 1 )
                        {
                            SelectOneRow( ht.RowIndex );
                        }

                        MouseClickRightButton?.Invoke( e, GetSelectedDownloadRow(), outOfGridArea: false );
                    }
                }
            }
            else if ( (ht.Type == DataGridViewHitTestType.None) || (ht.Type == DataGridViewHitTestType.Cell) )
            {
                switch ( e.Button )
                {
                    case MouseButtons.Right:
                        MouseClickRightButton?.Invoke( e, GetSelectedDownloadRow(), outOfGridArea: true );
                    break;
                }
            }
        }
        private void DGV_DoubleClick( object sender, EventArgs e ) => DoubleClickEx?.Invoke( sender, e );

        #region [.DragDrop rows.]
        /// <summary>
        /// 
        /// </summary>
        private sealed class DRAGDROP_ROWS_FORMAT_TYPE
        {
            public DRAGDROP_ROWS_FORMAT_TYPE( IReadOnlyList< DownloadRow > selectedRows, DownloadRow focusedRow ) => (Rows, FocusedRow) = (selectedRows, focusedRow);
            public IReadOnlyList< DownloadRow > Rows { get; }
            public DownloadRow FocusedRow { get; }
        }
        private void DGV_MouseMove( object sender, MouseEventArgs e )
        {
            if ( e.Button != MouseButtons.Left ) return;
            var ht = DGV.HitTest( e.X, e.Y );
            if ( (ht.RowIndex < 0) || (ht.ColumnIndex < 0) ) return;

            if ( _DGV_MouseDown_HitTestInfo.Type != DataGridViewHitTestType.Cell ) return;

            const int MOVE_DELTA = 5;
            if ( Math.Abs( _DGV_MouseDown_ButtonLeft_Location.X - e.X ) < MOVE_DELTA &&
                 Math.Abs( _DGV_MouseDown_ButtonLeft_Location.Y - e.Y ) < MOVE_DELTA ) return;
            //-----------------------------------------------------//

            var rows = GetSelectedDownloadRows();
            if ( !rows.Any() ) return;

            #region [.create DragDrop DataObject.]
            var focusedRow = _Model[ DGV.CurrentCell.RowIndex ];

            var fileNames = rows.SelectMany( r => r.GetOutputFullFileNames() ).ToList( rows.Count ).ToArray();

            var dataObj = new DataObject();

            var hasExistsFiles = fileNames.Any( fn => File.Exists( fn ) );            
            if ( hasExistsFiles ) dataObj.SetData( DataFormats.FileDrop, fileNames );
            
            dataObj.SetDataEx( new DRAGDROP_ROWS_FORMAT_TYPE( rows, focusedRow ) );
            #endregion

            _DragOver_RowIndex = null;
            DGV.AllowDrop = true;
            DGV.DragOver     += DGV_DragOver;
            DGV.DragDrop     += DGV_DragDrop;
            DGV.CellPainting += DGV_DragDrop_CellPainting;
            DGV.SetDoubleBuffered( false );
            try
            {
                var dragDropEffect = DGV.DoDragDrop( dataObj, DragDropEffects.Move | DragDropEffects.Copy );
                if ( dragDropEffect == DragDropEffects.None )
                {
                    DGV.Invalidate();
                }
            }
            finally
            {
                DGV.DragOver     -= DGV_DragOver;
                DGV.DragDrop     -= DGV_DragDrop;
                DGV.CellPainting -= DGV_DragDrop_CellPainting;
                DGV.AllowDrop = false;
                DGV.SetDoubleBuffered( true );
            }
        }
        private void DGV_DragDrop( object sender, DragEventArgs e )
        {
            if ( e.Data.TryGetData< DRAGDROP_ROWS_FORMAT_TYPE >( out var ddrf ) )
            {
                var pt = DGV.PointToClient( new Point( e.X, e.Y ) );
                var ht = DGV.HitTest( pt.X, pt.Y );
                if (( 0 <= ht.RowIndex ) && (ddrf.Rows[ 0 ].GetVisibleIndex() != ht.RowIndex))
                {
                    var suc = _Model.ChangeRowsPosition( ddrf.Rows, ht.RowIndex );
                    if ( suc )
                    {
                        SelectDownloadRows( ddrf.Rows, ddrf.FocusedRow );

                        e.Effect = e.AllowedEffect;
                        return;
                    }
                }
            }
            e.Effect = DragDropEffects.None;
        }

        private int? _DragOver_RowIndex;
        private void DGV_DragOver( object sender, DragEventArgs e )
        {
            var pt = DGV.PointToClient( new Point( e.X, e.Y ) );
            var ht = DGV.HitTest( pt.X, pt.Y );

            if ( (0 <= ht.RowIndex) && e.Data.TryGetData< DRAGDROP_ROWS_FORMAT_TYPE >( out var ddrf ) && (ddrf.Rows[ 0 ].GetVisibleIndex() != ht.RowIndex) )
            {
                e.Effect = e.AllowedEffect;

                if ( _DragOver_RowIndex != ht.RowIndex )
                {
                    _DragOver_RowIndex = ht.RowIndex;
                    DGV.Invalidate();
                }
                DGV.ScrollIfNeed( in pt );
                return;
            }

            e.Effect = DragDropEffects.None;
            if ( _DragOver_RowIndex.HasValue )
            {
                _DragOver_RowIndex = null;
                DGV.Invalidate();
            }
            DGV.ScrollIfNeed( in pt );
        }
        private void DGV_DragDrop_CellPainting( object sender, DataGridViewCellPaintingEventArgs e )
        {
            if ( e.RowIndex == _DragOver_RowIndex )
            {
                e.Handled = true;

                var rc = DGV.GetRowDisplayRectangle( e.RowIndex, true );
                rc.Width = DGV.RowHeadersWidth + DGV.Columns.Cast< DataGridViewColumn >().Where( c => c.Visible ).Sum( c => c.Width );

                var color = Color.FromArgb( 75, DGV.DefaultCellStyle.SelectionBackColor ); // Color.Blue );
                using ( var br = new HatchBrush( HatchStyle.ForwardDiagonal, color, Color.Transparent ) )
                {
                    e.Graphics.FillRectangle( br, rc );
                }
                using ( var pen = new Pen( color, 2.0f ) )
                {
                    rc.Inflate( -1, -1 );
                    e.Graphics.DrawRectangle( pen, rc );
                }

                #region comm. other draw methods.
                //using ( var pen = new Pen( Color.OrangeRed, 2.0f ) )
                //{
                //    e.Graphics.DrawRectangle( pen, rc );
                //}

                //using ( var br = new SolidBrush( Color.FromArgb( 90, Color.Black ) ) )
                //{
                //    e.Graphics.FillRectangle( br, rc );
                //} 
                #endregion
            }
        }
        #endregion

        private void DGV_DataError( object sender, DataGridViewDataErrorEventArgs e ) => Debug.WriteLine( $"{nameof(DGV_DataError)}::'{e.Context}'; [row={e.RowIndex}:col={e.ColumnIndex}] => '{e.Exception}'" );
        #endregion
    }
}
