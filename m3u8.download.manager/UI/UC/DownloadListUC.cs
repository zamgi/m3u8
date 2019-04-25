using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using m3u8.download.manager.models;
using m3u8.download.manager.Properties;
using _CollectionChangedTypeEnum_ = m3u8.download.manager.models.DownloadListModel.CollectionChangedTypeEnum;
using CellStyle = System.Windows.Forms.DataGridViewCellStyle;
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
        public delegate void MouseClickRightButtonEventHandler( MouseEventArgs e, DownloadRow row );

        #region [.column index's.]
        private int URL_COLUMNINDEX             { [M(O.AggressiveInlining)] get => DGV_urlColumn.DisplayIndex;             }
        private int OUTPUTFILENAME_COLUMNINDEX  { [M(O.AggressiveInlining)] get => DGV_outputFileNameColumn.DisplayIndex;  }
        private int OUTPUTDIRECTORY_COLUMNINDEX { [M(O.AggressiveInlining)] get => DGV_outputDirectoryColumn.DisplayIndex; }
        private int STATUS_COLUMNINDEX          { [M(O.AggressiveInlining)] get => DGV_statusColumn.DisplayIndex;          }
        private int DOWNLOADINFO_COLUMNINDEX    { [M(O.AggressiveInlining)] get => DGV_downloadInfoColumn.DisplayIndex;    }
        #endregion

        #region [.field's.]
        public event SelectionChangedEventHandler      SelectionChanged;
        public event OutputFileNameClickEventHandler   OutputFileNameClick;
        public event MouseClickRightButtonEventHandler MouseClickRightButton;

        private DownloadListModel _Model;
        //private CellStyle _DefaultCellStyle;
        private CellStyle         _ErrorCellStyle;
        private CellStyle         _CanceledCellStyle;
        private CellStyle         _FinishedCellStyle;
        private RowNumbersPainter _RNP;
        private StringFormat      _SF;
        private bool              _UserMade_DGV_SelectionChanged;
        private SortInfo          _LastSortInfo;
        private Action            _RestoreSortIfNeedAction;
        private Timer             _CommonUpdateTimer;
        private Settings          _Settings;
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

            _RNP = RowNumbersPainter.Create( DGV, true );
            _SF  = new StringFormat( StringFormatFlags.NoWrap ) { Trimming = StringTrimming.EllipsisCharacter, Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
        }

        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();
                _RNP.Dispose();
                _SF.Dispose();
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
                            var visibleIndex = Math.Min( Math.Max( 0, selectedVisibleIndex ), DGV.RowCount - 1 ); // ((0 <= selectedVisibleIndex) && (selectedVisibleIndex < DGV.RowCount)) ? selectedVisibleIndex : (DGV.RowCount - 1); //0;
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
        [M(O.AggressiveInlining)] private static string GetDownloadInfoText( DownloadRow row )
        {
            const string HH_MM_SS = "hh\\:mm\\:ss";
            const string MM_SS    = "mm\\:ss";

            switch ( row.Status )
            {
                case DownloadStatus.Created: return ($"[created]: {row.CreatedDateTime.ToString( "HH:mm:ss  (yyyy.MM.dd)" )}");
                case DownloadStatus.Started: return ($"{row.GetElapsed().ToString( HH_MM_SS )}");
                case DownloadStatus.Wait   : return ($"(wait), ({row.GetElapsed().ToString( HH_MM_SS )})");
            }

            var ts           = row.GetElapsed();
            var percent      = ((0 < row.TotalParts) ? Convert.ToByte( (100.0 * row.SuccessDownloadParts) / row.TotalParts ).ToString() : "-");
            var elapsed      = ((1 < ts.TotalHours) ? ts.ToString( HH_MM_SS ) : (':' + ts.ToString( MM_SS )));
            var failedParts  = ((row.FailedDownloadParts != 0) ? $" (failed: {row.FailedDownloadParts})" : null);
            var downloadInfo = $"{row.SuccessDownloadParts} of {row.TotalParts}{failedParts}, {percent}%, ({elapsed})";
            
            #region [.speed.]
            var elapsedSeconds = ts.TotalSeconds;
            if ( (1_000 < row.DownloadBytesLength) && (2.5 <= elapsedSeconds) )
            {
                var speedText = default(string);
                //if ( totalBytesLength < 1_000   ) speedText = (row.DownloadBytesLength / elapsedSeconds).ToString("N2") + " bit/s";
                if ( row.DownloadBytesLength < 100_000 ) speedText = ((row.DownloadBytesLength / elapsedSeconds) /     1_000).ToString("N2") + " Kbit/s";
                else                                     speedText = ((row.DownloadBytesLength / elapsedSeconds) / 1_000_000).ToString("N1") + " Mbit/s";

                downloadInfo += $", [speed: {speedText}]";
            }
            #endregion

            return (downloadInfo);
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
                    d = x.CreatedDateTime.CompareTo( y.CreatedDateTime );
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
                var sh = new SortHelper() { _Comparison = (x, y) => x.CreatedDateTime.CompareTo( y.CreatedDateTime ) };
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
            if ( columnIndex == URL_COLUMNINDEX )
            {
                comparison = (x, y) => string.Compare( x.Url, y.Url, true );                
            }
            else if ( columnIndex == OUTPUTFILENAME_COLUMNINDEX )
            {
                comparison = (x, y) => string.Compare( x.OutputFileName, y.OutputFileName, true );
            }
            else if ( columnIndex == OUTPUTDIRECTORY_COLUMNINDEX )
            {
                comparison = (x, y) => string.Compare( x.OutputDirectory, y.OutputDirectory, true );
            }
            else if ( columnIndex == STATUS_COLUMNINDEX )
            {
                comparison = (x, y) => SortHelper.ToInt32( x.Status ).CompareTo( SortHelper.ToInt32( y.Status ) );
            }
            else if ( columnIndex == DOWNLOADINFO_COLUMNINDEX )
            {
                comparison = (x, y) => x.SuccessDownloadParts.CompareTo( y.SuccessDownloadParts );
            }
            else
            {
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
        private void CommonUpdateTimer_Tick( object sender, EventArgs e ) => DGV.Refresh();
        #endregion

        #region [.DGV.]
        private void DGV_CellValueNeeded( object sender, DataGridViewCellValueEventArgs e )
        {
            var row = _Model[ e.RowIndex ];
            #region comm
            //var idx = e.ColumnIndex;
            //     if ( idx == URL_COLUMNINDEX             ) e.Value = row.Url;
            //else if ( idx == OUTPUTFILENAME_COLUMNINDEX  ) e.Value = row.OutputFileName;
            //else if ( idx == OUTPUTDIRECTORY_COLUMNINDEX ) e.Value = row.OutputDirectory;
            //else if ( idx == STATUS_COLUMNINDEX          ) e.Value = row.Status;
            //else if ( idx == DOWNLOADINFO_COLUMNINDEX    ) e.Value = GetDownloadInfoText( row ); 
            #endregion

            switch ( e.ColumnIndex )
            {
                case 0: e.Value = row.Url;                    break;
                case 1: e.Value = row.OutputFileName;         break;
                case 2: e.Value = row.OutputDirectory;        break;
                case 3: e.Value = row.Status.ToString();      break;
                case 4: e.Value = GetDownloadInfoText( row ); break;
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
                var pt = Control.MousePosition;
                pt = DGV.PointToClient( pt );
                var hti = DGV.HitTest( pt.X, pt.Y );
                if ( hti.ColumnIndex == OUTPUTFILENAME_COLUMNINDEX )
                {
                    DGV.SetHandCursorIfNonHand();
                }
            }
        }
        private void DGV_CellMouseEnter( object sender, DataGridViewCellEventArgs e )
        {
            if ( (0 <= e.RowIndex) && (e.ColumnIndex == OUTPUTFILENAME_COLUMNINDEX) && 
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
            if ( !_UserMade_DGV_SelectionChanged && (0 <= e.RowIndex) && (e.ColumnIndex == OUTPUTFILENAME_COLUMNINDEX) )
            {
                var row = _Model[ e.RowIndex ];
                if ( !row.IsFinished() )
                {
                    OutputFileNameClick?.Invoke( row );
                }
            }
            _UserMade_DGV_SelectionChanged = false;
        }
        private void DGV_CellPainting( object sender, DataGridViewCellPaintingEventArgs e )
        {
            if ( (0 <= e.RowIndex) && (e.ColumnIndex == STATUS_COLUMNINDEX) )
            {
                e.Handled = true;
                var gr = e.Graphics;

                //-1- bottom & right lines -//
                var rc = e.CellBounds; rc.Width--; rc.Height--;
                gr.DrawLines( Pens.DimGray, new[] { new Point( rc.X    , rc.Bottom ), new Point( rc.Right, rc.Bottom ),
                                                    new Point( rc.Right, rc.Y      ), new Point( rc.Right, rc.Bottom ), } );
                //-2- fill background -//
                var backColor = e.State.IsSelected() ? e.CellStyle.SelectionBackColor : e.CellStyle.BackColor;
                using ( var br = new SolidBrush( backColor ) )
                {
                    gr.FillRectangle( br, rc );
                }

                //-3- fill background -//
                rc.Inflate( -1, -1 );
                gr.FillRectangle( Brushes.White, rc );

                //-4- status image -//
                var row = _Model[ e.RowIndex ];

                const int IMAGE_HEIGHT = 16;
                rc = e.CellBounds; 
                rc = new Rectangle( rc.X + 2, rc.Y + (rc.Height - IMAGE_HEIGHT) / 2, IMAGE_HEIGHT, IMAGE_HEIGHT );
                switch ( row.Status )
                {
                    case DownloadStatus.Created : gr.DrawImage( Resources.created , rc ); break;
                    case DownloadStatus.Started : gr.DrawImage( Resources.running , rc ); break;
                    case DownloadStatus.Running : gr.DrawImage( Resources.running , rc ); break;
                    case DownloadStatus.Paused  : gr.DrawImage( Resources.paused  , rc ); break;
                    case DownloadStatus.Wait    : gr.DrawImage( Resources.wait    , rc ); break;
                    case DownloadStatus.Canceled: gr.DrawImage( Resources.canceled, rc ); break;
                    case DownloadStatus.Finished: gr.DrawImage( Resources.finished, rc ); break;
                    case DownloadStatus.Error   : gr.DrawImage( Resources.error   , rc ); break;
                }
                //-5- status text -//
                rc = e.CellBounds; rc.Inflate( -22, 0 );
                gr.DrawString( row.Status.ToString(), DGV.Font, Brushes.Black, rc, _SF );
            }
        }
        private void DGV_MouseClick( object sender, MouseEventArgs e )
        {
            var ht = DGV.HitTest( e.X, e.Y );
            if ( 0 <= ht.RowIndex )
            {
                DGV.Rows[ ht.RowIndex ].Selected = true;

                if ( e.Button == MouseButtons.Right )
                {
                    MouseClickRightButton?.Invoke( e, GetSelectedDownloadRow() );
                }
            }
            else if ( (ht.Type == DataGridViewHitTestType.None) || (ht.Type == DataGridViewHitTestType.Cell) )
            {
                switch ( e.Button )
                {
                    case MouseButtons.Left:
                        DGV.ClearSelection();
                        //DGV.CurrentCell = null;
                    break;

                    case MouseButtons.Right:
                        MouseClickRightButton?.Invoke( e, null );
                    break;
                }
            }
        }
        private void DGV_ColumnHeaderMouseClick( object sender, DataGridViewCellMouseEventArgs e )
        {
            if ( e.Button == MouseButtons.Left )
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
        }
        #endregion
    }
}
