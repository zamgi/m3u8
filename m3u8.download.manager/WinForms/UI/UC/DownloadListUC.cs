using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        public delegate void MouseClickRightButtonEventHandler( MouseEventArgs e, DownloadRow row );
        /// <summary>
        /// 
        /// </summary>
        public delegate void UpdatedSingleRunningRowEventHandler( DownloadRow row );

        #region [.column index's.]
        private const int OUTPUTFILENAME_COLUMN_INDEX        = 0;
        private const int OUTPUTDIRECTORY_COLUMN_INDEX       = 1;
        private const int STATUS_COLUMN_INDEX                = 2;
        private const int DOWNLOAD_PROGRESS_COLUMN_INDEX     = 3;
        private const int DOWNLOAD_TIME_COLUMN_INDEX         = 4;
        private const int APPROX_REMAINED_TIME_COLUMN_INDEX  = 5;
        private const int DOWNLOAD_SPEED_COLUMN_INDEX        = 6;
        private const int DOWNLOAD_BYTES_COLUMN_INDEX        = 7;
        private const int APPROX_REMAINED_BYTES_COLUMN_INDEX = 8;
        private const int APPROX_TOTAL_BYTES_COLUMN_INDEX    = 9;
        private const int URL_COLUMN_INDEX                   = 10;
        #endregion

        #region [.field's.]
        public event SelectionChangedEventHandler        SelectionChanged;
        public event OutputFileNameClickEventHandler     OutputFileNameClick;
        public event OutputDirectoryClickEventHandler    OutputDirectoryClick;
        public event MouseClickRightButtonEventHandler   MouseClickRightButton;
        public event UpdatedSingleRunningRowEventHandler UpdatedSingleRunningRow;

        private DownloadListModel _Model;
        //private CellStyle _DefaultCellStyle;
        private CellStyle         _ErrorCellStyle;
        private CellStyle         _CanceledCellStyle;
        private CellStyle         _FinishedCellStyle;
        private RowNumbersPainter _RNP;
        private StringFormat      _SF_Left;
        private StringFormat      _SF_Center;
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
                SelectionForeColor = Color.White,
                SelectionBackColor = Color.Red,
            };
            _CanceledCellStyle = new CellStyle( DGV.DefaultCellStyle )
            {
                ForeColor          = Color.DimGray,
                SelectionForeColor = Color.WhiteSmoke,
            };
            _FinishedCellStyle = new CellStyle( DGV.DefaultCellStyle ) { Font = new Font( DGV.Font, FontStyle.Bold ) };

            _RNP = RowNumbersPainter.Create( DGV );
            _SF_Left   = new StringFormat( StringFormatFlags.NoWrap ) { Trimming = StringTrimming.EllipsisCharacter, Alignment = StringAlignment.Near  , LineAlignment = StringAlignment.Center };
            _SF_Center = new StringFormat( StringFormatFlags.NoWrap ) { Trimming = StringTrimming.EllipsisCharacter, Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
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
            }
            base.Dispose( disposing );
        }
        #endregion

        #region [.Model.]
        public DownloadRow GetSelectedDownloadRow()
        {
            var dtrow = DGV.SelectedRows.Cast< DataGridViewRow >().FirstOrDefault();
            var row   = ((dtrow != null) && (dtrow.Index < _Model.RowsCount)) ? _Model[ dtrow.Index ] : null;
            return (row);
        }
        public bool SelectDownloadRow( DownloadRow row ) => SelectDownloadRowInternal( row );
        private bool SelectDownloadRowInternal( DownloadRow row, bool callAfterSort = false )
        {
            if ( row != null )
            {
                var visibleIndex = row.GetVisibleIndex();
                if ( (0 <= visibleIndex) && (visibleIndex < DGV.RowCount) )
                {
                    var dtrow = DGV.Rows[ visibleIndex ];
                    if ( dtrow.Selected )
                    {
                        SelectionChanged?.Invoke( row );
                    }
                    else
                    {
                        dtrow.Selected = true;
                    }
                    if ( !callAfterSort )
                    {
                        _UserMade_DGV_SelectionChanged = false;
                    }
                    return (true);
                }
            }
            return (false);
        }
        public bool HasFocus => (DGV.Focused || this.Focused);

        public void SetModel( DownloadListModel model )
        {
            DetachModel();

            _Model = model ?? throw (new ArgumentNullException( nameof(model) ));
            _Model.CollectionChanged    -= Model_CollectionChanged;
            _Model.CollectionChanged    += Model_CollectionChanged;
            _Model.RowPropertiesChanged -= Model_RowPropertiesChanged;
            _Model.RowPropertiesChanged += Model_RowPropertiesChanged;
            Model_CollectionChanged( _CollectionChangedTypeEnum_.Add );
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

        private void Model_CollectionChanged( _CollectionChangedTypeEnum_ collectionChangedType )
        {
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
                            var visibleIndex = Math.Min( Math.Max( 0, selectedVisibleIndex ), DGV.RowCount - 1 );
                            var dtrow = DGV.Rows[ visibleIndex ];
                            if ( dtrow.Selected )
                            {
                                var row = _Model[ visibleIndex ];
                                SelectionChanged?.Invoke( row );
                            }
                            else
                            {
                                dtrow.Selected = true;
                            }
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
            var failedParts  = ((row.FailedDownloadParts != 0) ? $" (failed: {row.FailedDownloadParts})" : null);
            var downloadInfo = $"{percent}%, ({elapsed})";
            
            #region [.speed.]
            if ( !st.IsPaused() )
            {                
                var elapsedSeconds = ts.TotalSeconds;
                var downloadBytes  = row.GetDownloadBytesLengthAfterLastRun();
                if ( (1_000 < downloadBytes) && (2.5 <= elapsedSeconds) )
                {
                    var speedText = default(string);
                    if ( downloadBytes < 100_000 ) speedText = ((downloadBytes / elapsedSeconds) /     1_000).ToString("N2") + " Kbit/s";
                    else                           speedText = ((downloadBytes / elapsedSeconds) / 1_000_000).ToString("N1") + " Mbit/s";

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
                if ( (1_000 < downloadBytes) && (2.5 <= elapsedSeconds) )
                {
                    var speedText = default(string);
                    if ( downloadBytes < 100_000 ) speedText = ((downloadBytes / elapsedSeconds) /     1_000).ToString("N2") + " Kbit/s";
                    else                           speedText = ((downloadBytes / elapsedSeconds) / 1_000_000).ToString("N1") + " Mbit/s";
                    return (speedText);
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

            const float KILOBYTE = 1024;
            const float MEGABYTE = KILOBYTE * KILOBYTE;
            const float GIGABYTE = MEGABYTE * KILOBYTE;

            if ( GIGABYTE < size )
                return ((size / GIGABYTE).ToString("N2") + " GB");
            if ( MEGABYTE < size )
                return ((size / MEGABYTE).ToString("N2") + " MB");
            if ( KILOBYTE < size )
                return ((size / KILOBYTE).ToString("N2") + " KB");
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
                var sh = new SortHelper() { _Comparison = (x, y) => x.CreatedOrStartedDateTime.CompareTo( y.CreatedOrStartedDateTime ) };
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
                case STATUS_COLUMN_INDEX               : e.Value = $"               {row.Status}";                        break;
                case DOWNLOAD_PROGRESS_COLUMN_INDEX    : e.Value = new string( ' ', 30 );                                 break;
                case DOWNLOAD_TIME_COLUMN_INDEX        : e.Value = GetDownloadTimeText       ( row );                     break;
                case APPROX_REMAINED_TIME_COLUMN_INDEX : e.Value = GetApproxRemainedTimeText ( row );                     break;
                case DOWNLOAD_SPEED_COLUMN_INDEX       : e.Value = GetDownloadSpeedText      ( row );                     break;
                case DOWNLOAD_BYTES_COLUMN_INDEX       : e.Value = GetDisplaySizeText        ( row.DownloadBytesLength ); break;
                case APPROX_REMAINED_BYTES_COLUMN_INDEX: e.Value = GetApproxRemainedBytesText( row );                     break;
                case APPROX_TOTAL_BYTES_COLUMN_INDEX   : e.Value = GetApproxTotalBytesText   ( row );                     break;
                case URL_COLUMN_INDEX                  : e.Value = row.Url;                                               break;
            }
        }
        private void DGV_CellFormatting( object sender, DataGridViewCellFormattingEventArgs e )
        {
            var row = _Model[ e.RowIndex ];
            switch ( row.Status )
            {
                case DownloadStatus.Error:
                    e.CellStyle = _ErrorCellStyle;
                    e.FormattingApplied = true;
                break;

                case DownloadStatus.Canceled:
                    e.CellStyle = _CanceledCellStyle;
                    e.FormattingApplied = true;
                break;

                case DownloadStatus.Finished:
                    e.CellStyle = _FinishedCellStyle;
                    e.FormattingApplied = true;
                break;
            }
        }
        private void DGV_SelectionChanged( object sender, EventArgs e )
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
                        DGV.SetHandCursorIfNonHand();
                    break;
                }
            }
        }
        private void DGV_CellMouseEnter( object sender, DataGridViewCellEventArgs e )
        {
            if ( (0 <= e.RowIndex) && ((e.ColumnIndex == OUTPUTFILENAME_COLUMN_INDEX) || (e.ColumnIndex == OUTPUTDIRECTORY_COLUMN_INDEX)) && 
                 DGV.Rows[ e.RowIndex ].Selected && !_Model[ e.RowIndex ].IsFinished() )
            {
                DGV.SetHandCursorIfNonHand();
            }
            else
            {
                DGV.SetDefaultCursorIfHand();
            }
        }
        private void DGV_CellMouseLeave( object sender, DataGridViewCellEventArgs e ) => DGV.SetDefaultCursorIfHand();
        private void DGV_CellClick( object sender, DataGridViewCellEventArgs e )
        {
            if ( !_UserMade_DGV_SelectionChanged && (0 <= e.RowIndex) )
            {
                switch ( e.ColumnIndex )
                {
                    case OUTPUTFILENAME_COLUMN_INDEX:
                    {    var row = _Model[ e.RowIndex ];
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
                }
            }
            _UserMade_DGV_SelectionChanged = false;
        }

        [M(O.AggressiveInlining)] private static void CellPaintRoutine( DataGridViewCellPaintingEventArgs e )
        {
            e.Handled = true;
            var gr = e.Graphics;

            //-1- bottom & right lines -//
            var rc = e.CellBounds; rc.Width--; rc.Height--;
            gr.DrawLines( Pens.DimGray, new[] { new Point( rc.X    , rc.Bottom ), new Point( rc.Right, rc.Bottom ),
                                                new Point( rc.Right, rc.Y      ), new Point( rc.Right, rc.Bottom ), } );
            //-2- fill background -//
            var backColor = (e.State.IsSelected() ? e.CellStyle.SelectionBackColor : e.CellStyle.BackColor);
            using ( var br = new SolidBrush( backColor ) )
            {
                gr.FillRectangle( br, rc );
            }

            //-3- fill background -//
            rc.Inflate( -1, -1 );
            gr.FillRectangle( Brushes.White, rc );
        }
        private void DGV_CellPainting( object sender, DataGridViewCellPaintingEventArgs e )
        {
            if ( e.RowIndex < 0 ) return;

            if ( e.ColumnIndex == STATUS_COLUMN_INDEX )
            {
                CellPaintRoutine( e );

                //-4- status image -//
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
                const int IMAGE_HEIGHT = 16;
                var rc = e.CellBounds; 
                rc = new Rectangle( rc.X + 2, rc.Y + (rc.Height - IMAGE_HEIGHT) / 2, IMAGE_HEIGHT, IMAGE_HEIGHT );                
                if ( img != null )
                {
                    gr.DrawImage( img, rc );
                }

                //-5- status text -//
                rc = e.CellBounds; rc.Inflate( -22, 0 );
                gr.DrawString( row.Status.ToString(), DGV.Font, Brushes.Black, rc, _SF_Left );
            }
            else if ( e.ColumnIndex == DOWNLOAD_PROGRESS_COLUMN_INDEX )
            {
                CellPaintRoutine( e );

                //-1- progress-text -//
                var row  = _Model[ e.RowIndex ];
                var has  = TryGetDownloadProgress( row, out var part, out var progressText );

                var gr = e.Graphics;
                var rc = e.CellBounds;
                StringFormat sf;
                if ( has )
                {
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
            }
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

        private HitTestInfo _DGV_MouseDown_HitTestInfo;
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
                    DGV.Rows[ ht.RowIndex ].Selected = true;

                    if ( e.Button == MouseButtons.Right )
                    {
                        MouseClickRightButton?.Invoke( e, GetSelectedDownloadRow() );
                    }
                }
            }
            else if ( (ht.Type == DataGridViewHitTestType.None) || (ht.Type == DataGridViewHitTestType.Cell) )
            {
                switch ( e.Button )
                {
                    case MouseButtons.Left:
                        if ( _DGV_MouseDown_HitTestInfo.Type == DataGridViewHitTestType.None )
                        {
                            DGV.ClearSelection();
                        }
                    break;

                    case MouseButtons.Right:
                        MouseClickRightButton?.Invoke( e, null );
                    break;
                }
            }
        }

        private void DGV_MouseMove( object sender, MouseEventArgs e )
        {
            if ( e.Button != MouseButtons.Left ) return;
            var row = GetSelectedDownloadRow();
            if ( row == null ) return;

            var ht = DGV.HitTest( e.X, e.Y );
            if ( (ht.RowIndex < 0) || (ht.ColumnIndex < 0) ) return;

            if ( _DGV_MouseDown_HitTestInfo.Type != DataGridViewHitTestType.Cell ) return;

            const int MOVE_DELTA = 5;
            if ( Math.Abs( _DGV_MouseDown_ButtonLeft_Location.X - e.X ) < MOVE_DELTA &&
                 Math.Abs( _DGV_MouseDown_ButtonLeft_Location.Y - e.Y ) < MOVE_DELTA ) return;
            //-----------------------------------------------------//

            _DragOver_RowIndex = null;
            DGV.AllowDrop = true;
            DGV.DragOver += DGV_DragOver;
            DGV.DragDrop += DGV_DragDrop;
            DGV.CellPainting += DGV_DragDrop_CellPainting;
            try
            {
                var dragDropEffect = DGV.DoDragDrop( row, DragDropEffects.Move );
                if ( dragDropEffect == DragDropEffects.Move )
                {
                    SelectDownloadRow( row );
                }
                else
                {
                    DGV.Invalidate();
                }
            }
            finally
            {
                DGV.DragOver -= DGV_DragOver;
                DGV.DragDrop -= DGV_DragDrop;
                DGV.CellPainting -= DGV_DragDrop_CellPainting;
                DGV.AllowDrop = false;
            }
        }
        private void DGV_DragDrop( object sender, DragEventArgs e )
        {
            if ( e.Data.GetData( typeof(DownloadRow) ) is DownloadRow row )
            {
                var pt = DGV.PointToClient( new Point( e.X, e.Y ) );
                var ht = DGV.HitTest( pt.X, pt.Y );
                if ( (0 <= ht.RowIndex) && (row.GetVisibleIndex() != ht.RowIndex) )
                {
                    _Model.ChangeRowPosition( row, ht.RowIndex );
                    e.Effect = e.AllowedEffect;
                    return;
                }
            }
            e.Effect = DragDropEffects.None;
        }

        private int? _DragOver_RowIndex;
        private void DGV_DragDrop_CellPainting( object sender, DataGridViewCellPaintingEventArgs e )
        {
            if ( e.RowIndex == _DragOver_RowIndex )
            {
                e.Handled = true;

                var rc = DGV.GetRowDisplayRectangle( e.RowIndex, true );
                rc.Width = DGV.RowHeadersWidth + DGV.Columns.Cast< DataGridViewColumn >().Where( c => c.Visible ).Sum( c => c.Width );

                var color = Color.FromArgb( 75, DGV.DefaultCellStyle.SelectionBackColor );
                using ( var br = new HatchBrush( HatchStyle.ForwardDiagonal, color, Color.Transparent ) )
                {
                    e.Graphics.FillRectangle( br, rc );
                }
                using ( var pen = new Pen( color, 2.0f ) )
                {
                    rc.Inflate( -1, -1 );
                    e.Graphics.DrawRectangle( pen, rc );
                }
            }
        }
        private void DGV_DragOver( object sender, DragEventArgs e )
        {
            var pt = DGV.PointToClient( new Point( e.X, e.Y ) );
            var ht = DGV.HitTest( pt.X, pt.Y );

            if ( (0 <= ht.RowIndex) && (e.Data.GetData( typeof(DownloadRow) ) is DownloadRow row) && (row.GetVisibleIndex() != ht.RowIndex) )
            {
                e.Effect = e.AllowedEffect;

                if ( _DragOver_RowIndex != ht.RowIndex )
                {
                    _DragOver_RowIndex = ht.RowIndex;
                    DGV.Invalidate();
                }
                ScrollIfNeed( in pt );
                return;
            }            

            e.Effect = DragDropEffects.None;
            if ( _DragOver_RowIndex.HasValue )
            {
                _DragOver_RowIndex = null;
                DGV.Invalidate();
            }
            ScrollIfNeed( in pt );
        }
        private void DGV_DataError( object sender, DataGridViewDataErrorEventArgs e )
            => Debug.WriteLine( $"{nameof(DGV_DataError)}::'{e.Context}'; [row={e.RowIndex}:col={e.ColumnIndex}] => '{e.Exception}'" );

        private DateTime _LastScrollDateTime;
        private void ScrollIfNeed( in Point pt )
        {
            const int SCROLL_DELAY_IN_MILLISECONDS = 200;

            if ( ShouldScrollUp( in pt ) )
            {
                if ( (0 < DGV.FirstDisplayedScrollingRowIndex) && (TimeSpan.FromMilliseconds( SCROLL_DELAY_IN_MILLISECONDS ) < (DateTime.Now - _LastScrollDateTime)) )
                {
                    DGV.FirstDisplayedScrollingRowIndex--;
                    _LastScrollDateTime = DateTime.Now;
                }
            }
            else
            if ( ShouldScrollDown( in pt ) )
            {
                if ( (DGV.FirstDisplayedScrollingRowIndex < (DGV.RowCount - 1)) && (TimeSpan.FromMilliseconds( SCROLL_DELAY_IN_MILLISECONDS ) < (DateTime.Now - _LastScrollDateTime)) )
                {
                    DGV.FirstDisplayedScrollingRowIndex++;
                    _LastScrollDateTime = DateTime.Now;
                }
            }
        }
        [M(O.AggressiveInlining)] private bool ShouldScrollUp( in Point pt )
        {
            return pt.Y > DGV.ColumnHeadersHeight
                && pt.Y < DGV.ColumnHeadersHeight + 15
                && pt.X >= 0
                && pt.X <= DGV.Bounds.Width;
        }
        [M(O.AggressiveInlining)] private bool ShouldScrollDown( in Point pt )
        {
            var bounds = DGV.Bounds;
            return pt.Y > bounds.Height - 15
                && pt.Y < bounds.Height
                && pt.X >= 0
                && pt.X <= bounds.Width;
        }
        #endregion
    }
}
