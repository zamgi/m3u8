using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using m3u8.download.manager.controllers;
using m3u8.download.manager.models;
using m3u8.download.manager.Properties;

using _CollectionChangedTypeEnum_ = m3u8.download.manager.models.LogListModel.CollectionChangedTypeEnum;
using CellStyle = System.Windows.Forms.DataGridViewCellStyle;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class LogUC : UserControl
    {
        #region [.field's.]
        private RowNumbersPainter _RNP;
        private StringFormat      _SF;
        private CellStyle _Req_ReceivedCellStyle;
        private CellStyle _Req_CellStyleSmallFont_1;
        private CellStyle _Req_CellStyleSmallFont_2;
        private CellStyle _Req_ErrorCellStyle;
        private CellStyle _Req_ErrorCellStyleSmallFont_1;
        private CellStyle _Req_ErrorCellStyleSmallFont_2;

        private CellStyle _Rsp_ReceivedCellStyle;
        private CellStyle _Rsp_CellStyleSmallFont_1;
        private CellStyle _Rsp_CellStyleSmallFont_2;
        private CellStyle _Rsp_ErrorCellStyle;
        private CellStyle _Rsp_ErrorCellStyleSmallFont_1;
        private CellStyle _Rsp_ErrorCellStyleSmallFont_2;
        private CellStyle _DefaultCellStyle_4ResponseReceived;

        private ContextMenuStrip  _ContextMenu;
        private ToolStripMenuItem _ShowOnlyRequestRowsWithErrorsMenuItem;
        private bool              _ShowOnlyRequestRowsWithErrors;
        private ToolStripMenuItem _ScrollToLastRowMenuItem;
        private bool              _ScrollToLastRow;
        private SettingsPropertyChangeController _SettingsController;

        private LogListModel     _Model;
        private List_WithIndex< LogRow > _DGVRows;
        private ThreadSafeList< LogRow > _Model_CollectionChanged_AddChangedType_Buf;
        private Action< LogRow >  _RemoveRowAction;
        private HashSet< LogRow > _RemovedBeforeAddRows;
        private Action< LogRow > _InvalidateRowAction;
        private Action _Model_CollectionChanged_AddChangedType_Buf__UIRoutineAction;
        private Action< _CollectionChangedTypeEnum_, LogRow > _Model_CollectionChangedAction;        
        private bool _WasAdjustColumnsWidthSprain; //_VScrollBarVisible;

        private LogRowsHeightStorer _LogRowsHeightStorer;
        #endregion

        #region [.ctor().]
        public LogUC()
        {
            InitializeComponent();
            //----------------------------------------------//

            _RemoveRowAction = new Action< LogRow >( RemoveRow_UI );
            _InvalidateRowAction = new Action< LogRow >( InvalidateRow_UI );
            _Model_CollectionChangedAction = new Action< _CollectionChangedTypeEnum_, LogRow >( Model_CollectionChanged );
            _Model_CollectionChanged_AddChangedType_Buf__UIRoutineAction = new Action( () => Model_CollectionChanged_AddChangedType_Buf__UIRoutine() );

            _DGVRows = new List_WithIndex< LogRow >();
            _Model_CollectionChanged_AddChangedType_Buf = new ThreadSafeList< LogRow >();
            _RemovedBeforeAddRows = new HashSet< LogRow >();

            //----------------------------------------------//
            _RNP = RowNumbersPainter.Create( DGV, useSelectedBackColor: true, useColumnsHoverHighlight: false );
            _SF  = new StringFormat( StringFormatFlags.LineLimit ) { Trimming = StringTrimming.EllipsisCharacter, Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };

            //----------------------------------------------//
            var font   = DGV.Font;
            var alg    = DataGridViewContentAlignment.TopLeft;
            var wm     = DataGridViewTriState.True;
            var dcs    = DGV.DefaultCellStyle;
            var fc_suc = Color.DimGray;
            var bc_suc = dcs.BackColor;
            var fc_err = Color.Red;
            var bc_err = Color.Yellow;

            //----------------------------------------------//
            _DefaultCellStyle_4ResponseReceived = new CellStyle( dcs ) { WrapMode = wm, Alignment = alg, ForeColor = fc_suc };
            //----------------------------------------------//

            _Req_ReceivedCellStyle = new CellStyle( dcs ) { WrapMode = wm, Font = font, ForeColor = fc_suc, BackColor = bc_suc, Alignment = alg };
            _Rsp_ReceivedCellStyle = new CellStyle( dcs ) { WrapMode = wm, Font = font, ForeColor = fc_suc, BackColor = bc_suc, Alignment = alg };
            _Req_ErrorCellStyle    = new CellStyle( dcs ) { WrapMode = wm, Font = font, ForeColor = fc_err, BackColor = bc_err, Alignment = alg };
            _Rsp_ErrorCellStyle    = new CellStyle( dcs ) { WrapMode = wm, Font = font, ForeColor = fc_err, BackColor = bc_err, Alignment = alg };

            var smallFont_1 = new Font( font.FontFamily       , font.Size        * 4 / 5 );
            var smallFont_2 = new Font( smallFont_1.FontFamily, smallFont_1.Size * 4 / 5 );

            _Req_ErrorCellStyleSmallFont_1 = new CellStyle( dcs ) { WrapMode = wm, Font = smallFont_1, ForeColor = fc_err, BackColor = bc_err, Alignment = alg };
            _Rsp_ErrorCellStyleSmallFont_1 = new CellStyle( dcs ) { WrapMode = wm, Font = smallFont_1, ForeColor = fc_err, BackColor = bc_err, Alignment = alg };
            _Req_ErrorCellStyleSmallFont_2 = new CellStyle( dcs ) { WrapMode = wm, Font = smallFont_2, ForeColor = fc_err, BackColor = bc_err, Alignment = alg };
            _Rsp_ErrorCellStyleSmallFont_2 = new CellStyle( dcs ) { WrapMode = wm, Font = smallFont_2, ForeColor = fc_err, BackColor = bc_err, Alignment = alg };
            _Req_CellStyleSmallFont_1      = new CellStyle( dcs ) { WrapMode = wm, Font = smallFont_1, ForeColor = fc_suc, BackColor = bc_suc, Alignment = alg };
            _Rsp_CellStyleSmallFont_1      = new CellStyle( dcs ) { WrapMode = wm, Font = smallFont_1, ForeColor = fc_suc, BackColor = bc_suc, Alignment = alg };
            _Req_CellStyleSmallFont_2      = new CellStyle( dcs ) { WrapMode = wm, Font = smallFont_2, ForeColor = fc_suc, BackColor = bc_suc, Alignment = alg };
            _Rsp_CellStyleSmallFont_2      = new CellStyle( dcs ) { WrapMode = wm, Font = smallFont_2, ForeColor = fc_suc, BackColor = bc_suc, Alignment = alg };

            //----------------------------------------------//
            var st = SettingsPropertyChangeController.SettingsDefault;
            _ShowOnlyRequestRowsWithErrors = st.ShowOnlyRequestRowsWithErrors;
            _ShowOnlyRequestRowsWithErrorsMenuItem = new ToolStripMenuItem( "Show only request rows with errors", null, _ShowOnlyRequestRowsWithErrors_Click ) { Checked = _ShowOnlyRequestRowsWithErrors };

            _ScrollToLastRow = st.ScrollToLastRow;
            _ScrollToLastRowMenuItem = new ToolStripMenuItem( "Scroll to last row", null, _ScrollToLastRow_Click ) { Checked = _ScrollToLastRow };

            _ContextMenu = new ContextMenuStrip();
            _ContextMenu.Items.Add( _ShowOnlyRequestRowsWithErrorsMenuItem );
            _ContextMenu.Items.Add( _ScrollToLastRowMenuItem );
        }

        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();
                _RNP.Dispose();
                _SF.Dispose();
                DetachSettingsController();
                DetachLogRowsHeightStorer();
                DetachModel();
            }
            base.Dispose( disposing );
        }
        #endregion

        #region [.public.]        
        public bool ShowOnlyRequestRowsWithErrors
        {
            get => _ShowOnlyRequestRowsWithErrors;
            set
            {
                var st = (_SettingsController?.Settings ?? SettingsPropertyChangeController.SettingsDefault);
                if ( st.ShowOnlyRequestRowsWithErrors != value )
                {
                    st.ShowOnlyRequestRowsWithErrors = value;
                    AdjustColumnsWidthSprain();
                }
            }
        }
        public bool ScrollToLastRow
        {
            get => _ScrollToLastRow;
            set
            {
                var st = (_SettingsController?.Settings ?? SettingsPropertyChangeController.SettingsDefault);
                if ( st.ScrollToLastRow != value )
                {
                    st.ScrollToLastRow = value;
                    if ( value )
                    {
                        ScrollToLastRow_UI();
                    }
                }
            }
        }        
        public bool ShowResponseColumn
        {
            get => DGV_responseColumn.Visible;
            set => DGV_responseColumn.Visible = value;
        }

        public bool IsVerticalScrollBarVisible => (DGV.Controls.OfType< VScrollBar >().FirstOrDefault()?.Visible).GetValueOrDefault();
        public void AdjustColumnsWidthSprain() => DGV_Resize( null, null );
        public void AdjustRowsHeight()
        {
            const int DGV_ROWS_COUNT_MAX_THRESHOLD = 10_000 + 10;

            var endRowIndex = DGV.RowCount - 1;
            if ( (0 <= endRowIndex) && (endRowIndex <= DGV_ROWS_COUNT_MAX_THRESHOLD) )
            {
                DGV.SuspendDrawing();
                DGV.SuspendLayout();
                try
                {
                    DGV.AutoResizeRows( DataGridViewAutoSizeRowsMode.AllCells );
                }
                finally
                {
                    DGV.ResumeLayout( true );
                    DGV.ResumeDrawing();
                }
            }
        }
        public void AdjustRowsHeightAndColumnsWidthSprain()
        {
            AdjustRowsHeight();
            AdjustColumnsWidthSprain();
        }
        public void ClearSelection()
        {
            DGV.ClearSelection();
            DGV.CurrentCell = null;
        }
        public void SetFocus() => DGV.Focus();
        #endregion

        #region [.Model.]
        public void SetLogRowsHeightStorer( LogRowsHeightStorer logRowsHeightStorer )
        {
            DetachLogRowsHeightStorer();

            _LogRowsHeightStorer = logRowsHeightStorer ?? throw (new ArgumentNullException( nameof(logRowsHeightStorer) ));
            DGV.RowHeightChanged -= DGV_RowHeightChanged;
            DGV.RowHeightChanged += DGV_RowHeightChanged;
        }
        private void DetachLogRowsHeightStorer()
        {
            if ( _LogRowsHeightStorer != null )
            {
                DGV.RowHeightChanged -= DGV_RowHeightChanged;
                _LogRowsHeightStorer = null;
            }
        }

        public void SetSettingsController( SettingsPropertyChangeController sc )
        {
            DetachSettingsController();

            _SettingsController = sc ?? throw (new ArgumentNullException( nameof(sc) ));
            _SettingsController.SettingsPropertyChanged -= SettingsController_PropertyChanged;
            _SettingsController.SettingsPropertyChanged += SettingsController_PropertyChanged;

        }
        private void DetachSettingsController()
        {
            if ( _SettingsController != null )
            {
                _SettingsController.SettingsPropertyChanged -= SettingsController_PropertyChanged;
                _SettingsController = null;
            }
        }

        public void SetModel( LogListModel model )
        {
            DetachModel();

            if ( model != null )
            {
                _Model = model;
                model.CollectionChanged    -= Model_CollectionChanged;
                model.RowPropertiesChanged -= Model_RowPropertiesChanged;
                model.CollectionChanged    += Model_CollectionChanged;
                model.RowPropertiesChanged += Model_RowPropertiesChanged;
                
                this.Visible = true;
            }
            else
            {
                this.Visible = false;                
            }

            SetDataGridItems();            
        }
        private void DetachModel()
        {
            if ( _Model != null )
            {
                _Model.CollectionChanged -= Model_CollectionChanged;
                _Model.RowPropertiesChanged -= Model_RowPropertiesChanged;
                _Model = null;

                ClearDataGridItems_UI();
            }
        }
        
        private async void Model_CollectionChanged_AddChangedType_Buf__TaskRoutine()
        {
            const int COUNT_THRESHOLD = 100;
            const int TICK_THRESHOLD  = 500;

            var startTickCount = Environment.TickCount;
            var startCount     = _Model_CollectionChanged_AddChangedType_Buf.GetCount();
            for (; ; )
            {
                if ( (COUNT_THRESHOLD <= (_Model_CollectionChanged_AddChangedType_Buf.GetCount() - startCount)) || (TICK_THRESHOLD <= (Environment.TickCount - startTickCount)) )
                {
                    this.BeginInvoke( _Model_CollectionChanged_AddChangedType_Buf__UIRoutineAction );
                    return;
                }
                await Task.Delay( 1 ).CAX();
            }
        }
        private bool Model_CollectionChanged_AddChangedType_Buf__UIRoutine()
        {
            var suc = _Model_CollectionChanged_AddChangedType_Buf.TryGetAndClear( out var addRows );
            if ( suc )
            {
                var addRows_exclude_removed = addRows.Where( row => !_RemovedBeforeAddRows.Remove( row ) ).ToList( addRows.Count );
                var suc_2 = _DGVRows.AddRange( addRows_exclude_removed );
                if ( suc_2 )
                {
                    Set_DGV_RowCount();
                    AdjustColumnsWidthSprain_And_ScrollToLastRow();
                }
            }
            return (suc);
        }
        private void Model_CollectionChanged( _CollectionChangedTypeEnum_ changedType, LogRow row )
        {
            if ( this.InvokeRequired )
            {
                if ( changedType == _CollectionChangedTypeEnum_.Add )
                {
                    var cnt = _Model_CollectionChanged_AddChangedType_Buf.Add( row );
                    if ( cnt == 1 )
                    {
                        Task.Run( Model_CollectionChanged_AddChangedType_Buf__TaskRoutine );
                    }
                }
                else
                {
                    this.BeginInvoke( _Model_CollectionChangedAction, changedType, row );
                }                
            }
            else if ( !Model_CollectionChanged_AddChangedType_Buf__UIRoutine() )
            {
                switch ( changedType )
                {
                    case _CollectionChangedTypeEnum_.Add:
                        AddRow_UI( row );                        
                    break;

                    case _CollectionChangedTypeEnum_.Clear:
                        ClearDataGridItems_UI();
                        _WasAdjustColumnsWidthSprain = false;
                        Set_DGV_RowCount();
                    break;

                    case _CollectionChangedTypeEnum_.BulkUpdate:
                        SetDataGridItems();
                        AdjustColumnsWidthSprain_And_ScrollToLastRow();
                        DGV.ClearSelection();
                        ScrollToLastRow_UI();
                    break;
                }
            }
        }
        private void Model_RowPropertiesChanged( LogRow row, string propertyName )
        {
            if ( _ShowOnlyRequestRowsWithErrors && (row.RequestRowType == RequestRowTypeEnum.Success) && (propertyName == nameof(LogRow.RequestRowType)) )
            {
                Task.Delay( 250 ).ContinueWith( _ => this.BeginInvoke( _RemoveRowAction, row ) );
            }
            else
            {
                InvalidateRow_UI( row );
            }            
        }
        private void SettingsController_PropertyChanged( Settings settings, string propertyName )
        {
            switch ( propertyName )
            {
                case nameof(Settings.ShowOnlyRequestRowsWithErrors):
                    _ShowOnlyRequestRowsWithErrors = settings.ShowOnlyRequestRowsWithErrors;

                    _ShowOnlyRequestRowsWithErrorsMenuItem.Checked = _ShowOnlyRequestRowsWithErrors;
                    SetDataGridItems();
                break;

                case nameof(Settings.ScrollToLastRow):
                    _ScrollToLastRow = settings.ScrollToLastRow;

                    _ScrollToLastRowMenuItem.Checked = _ScrollToLastRow;
                    if ( _ScrollToLastRow )
                    {
                        ScrollToLastRow_UI();
                    }
                break;
            }
        }
        #endregion

        #region [.private.]
        private async void SetDataGridItems()
        {
            if ( _Model == null )
            {
                ClearDataGridItems_UI();
            }
            else
            {
                static IEnumerable< LogRow > GetRowsNotSuccess( IEnumerable< LogRow > rows ) => from row in rows 
                                                                                                where (row.RequestRowType != RequestRowTypeEnum.Success)
                                                                                                select row;

                var rows = _ShowOnlyRequestRowsWithErrors ? GetRowsNotSuccess( _Model.GetRows() )
                                                          : _Model.GetRows();
                SetDataGridItems_UI( rows );
            }
            
            SetRowsHeight();
            
            DGV.ClearSelection();

            await Task.Delay( 1 );
            AdjustColumnsWidthSprain_And_ScrollToLastRow();
        }
        private void ClearDataGridItems_UI()
        {
            _Model_CollectionChanged_AddChangedType_Buf.Clear();
            _RemovedBeforeAddRows.Clear();
            _DGVRows.Clear();

            Set_DGV_RowCount();
        }
        private void SetDataGridItems_UI( IEnumerable< LogRow > rows )
        {
            _Model_CollectionChanged_AddChangedType_Buf.Clear();
            _RemovedBeforeAddRows.Clear();
            _DGVRows.Replace( rows );

            Set_DGV_RowCount();
        }
        private void AddRow_UI( LogRow row )
        {
            if ( !_RemovedBeforeAddRows.Remove( row ) )
            {
                _DGVRows.Add( row );

                Set_DGV_RowCount();
                AdjustColumnsWidthSprain_And_ScrollToLastRow();
            }
        }
        private void RemoveRow_UI( LogRow row )
        {
            var suc = _DGVRows.Remove( row );
            if ( !suc )
            {
                _RemovedBeforeAddRows.Add( row );
            }
            else
            {
                Set_DGV_RowCount();
                DGV.Invalidate();
            }
        }
        private void InvalidateRow_UI( LogRow row )
        {
            if ( this.InvokeRequired )
            {
                this.BeginInvoke( _InvalidateRowAction, row );
            }
            else
            {
                var visibleIndex = _DGVRows.GetIndex( row );
                if ( (0 <= visibleIndex) && (visibleIndex < DGV.RowCount) )
                {
                    DGV.InvalidateRow( visibleIndex );
                }
            }
        }
        private void Set_DGV_RowCount()
        {
            if ( DGV.RowCount != _DGVRows.Count )
            {
                DGV.CellValueNeeded -= DGV_CellValueNeeded;
                DGV.CellFormatting  -= DGV_CellFormatting;
                try
                {
                    DGV.RowCount = _DGVRows.Count;
                }
                finally
                {
                    DGV.CellValueNeeded += DGV_CellValueNeeded;
                    DGV.CellFormatting  += DGV_CellFormatting;
                }
            }
        }
        private void SetRowsHeight()
        {
            if ( (0 < _DGVRows.Count) && (_LogRowsHeightStorer != null) && _LogRowsHeightStorer.TryGetStorerByModel( _Model, out var storer ) )
            {
                DGV.SuspendLayout();
                DGV.SuspendDrawing();
                try
                {
                    foreach ( var dgvRow in DGV.Rows.Cast< DataGridViewRow >() )
                    {
                        if ( storer.TryGetValue( dgvRow.Index, out var height ) && (dgvRow.Height != height) )
                        {
                            dgvRow.Height = height;
                        }
                    }
                }
                finally
                {
                    DGV.ResumeLayout( true );
                    DGV.ResumeDrawing();
                }
            }
        }
        private void AdjustColumnsWidthSprain_And_ScrollToLastRow()
        {
            if ( !_WasAdjustColumnsWidthSprain && this.IsVerticalScrollBarVisible )
            {
                _WasAdjustColumnsWidthSprain = true;
                AdjustColumnsWidthSprain();
            }
            if ( _ScrollToLastRow )
            {
                ScrollToLastRow_UI();
            }
        }
        private void ScrollToLastRow_UI()
        {
            if ( 0 < _DGVRows.Count )
            {
                DGV.SetFirstDisplayedScrollingRowIndex( _DGVRows.Count - 1 );
            }
        }

        private void _ShowOnlyRequestRowsWithErrors_Click( object sender, EventArgs e ) => this.ShowOnlyRequestRowsWithErrors = !this.ShowOnlyRequestRowsWithErrors;
        private void _ScrollToLastRow_Click( object sender, EventArgs e ) => this.ScrollToLastRow = !this.ScrollToLastRow;
        #endregion

        #region [.get cell-styles.]
        private const int MAX_TEXT_LENGTH = 2048;
        private const int TEXT_LENGTH_250 = 250;
        private const int TEXT_LENGTH_500 = 500;

        [M(O.AggressiveInlining)] private static string Get_4_RequestColumn__text( string errorText, int maxLength = MAX_TEXT_LENGTH ) => errorText.TrimIfLongest( maxLength );
        [M(O.AggressiveInlining)] private CellStyle     Get_4_RequestColumn__cellStyle( string text, int maxLength = MAX_TEXT_LENGTH )
        {
            var textLength = Math.Min( maxLength, text.Length );
            var cellStyle = ((TEXT_LENGTH_250 < textLength) ? ((TEXT_LENGTH_500 < textLength) ? _Req_CellStyleSmallFont_2 : _Req_CellStyleSmallFont_1) : null);
            return (cellStyle);
        }

        [M(O.AggressiveInlining)] private static string GetError_4_RequestColumn__text     ( string errorText, int maxLength = MAX_TEXT_LENGTH ) => errorText.TrimIfLongest( maxLength );
        [M(O.AggressiveInlining)] private CellStyle     GetError_4_RequestColumn__cellStyle( string errorText, int maxLength = MAX_TEXT_LENGTH )
        {
            var errorTextLength = Math.Min( maxLength, errorText.Length );
            var errorCellStyle = ((TEXT_LENGTH_250 < errorTextLength) ? ((TEXT_LENGTH_500 < errorTextLength) ? _Req_ErrorCellStyleSmallFont_2 : _Req_ErrorCellStyleSmallFont_1) : _Req_ErrorCellStyle);
            return (errorCellStyle);
        }

        [M(O.AggressiveInlining)] private static string GetError_4_ResponseColumn__text     ( string errorText, int maxLength = MAX_TEXT_LENGTH ) => errorText.TrimIfLongest( maxLength );
        [M(O.AggressiveInlining)] private CellStyle     GetError_4_ResponseColumn__cellStyle( string errorText, int maxLength = MAX_TEXT_LENGTH )
        {
            var errorTextLength = Math.Min( maxLength, errorText.Length );
            var errorCellStyle = ((TEXT_LENGTH_250 < errorTextLength) ? ((TEXT_LENGTH_500 < errorTextLength) ? _Rsp_ErrorCellStyleSmallFont_2 : _Rsp_ErrorCellStyleSmallFont_1) : _Rsp_ErrorCellStyle);
            return (errorCellStyle);
        }
        #endregion

        #region [.DGV events.]
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            DGV_Resize( null, null );
        }

        private int GetColumnsResizeDiff() => (DGV.RowHeadersVisible ? DGV.RowHeadersWidth : 0) + 
                                              (IsVerticalScrollBarVisible ? SystemInformation.VerticalScrollBarWidth : 0) + //3 + 
                                              ((DGV.BorderStyle != BorderStyle.None) ? SystemInformation.FixedFrameBorderSize.Width : 0);

        private void DGV_RowHeightChanged( object sender, DataGridViewRowEventArgs e ) => _LogRowsHeightStorer.StoreRowHeight( _Model, e.Row );
        private void DGV_ColumnWidthChanged( object sender, DataGridViewColumnEventArgs e )
        {
            DGV.ColumnWidthChanged -= DGV_ColumnWidthChanged;
            try
            {
                var w = (DGV.Width - GetColumnsResizeDiff());
                if ( this.ShowResponseColumn )
                {
                    DataGridViewColumn col_1, col_2;
                    if ( e.Column == DGV_responseColumn )
                    {
                        col_1 = DGV_responseColumn;
                        col_2 = DGV_requestColumn;
                    }
                    else //if ( e.Column == DGV_requestColumn )
                    {
                        col_1 = DGV_requestColumn;
                        col_2 = DGV_responseColumn;
                    }
                    
                    var cw = w - col_1.Width;
                    if ( col_2.MinimumWidth <= cw )
                    {
                        col_2.Width = cw;
                    }
                    else
                    {
                        col_2.Width = col_2.MinimumWidth;
                        col_1.Width = w - col_2.Width;
                    }
                }
                else
                {
                    DGV_requestColumn.Width = w;
                }

                AdjustRowsHeight();
            }
            finally
            {
                DGV.ColumnWidthChanged += DGV_ColumnWidthChanged;
            }
        }
        private void DGV_Resize( object sender, EventArgs e )
        {
            if ( this.FindForm()?.WindowState == FormWindowState.Minimized )
            {
                return;
            }

            DGV.ColumnWidthChanged -= DGV_ColumnWidthChanged;
            //try
            //{
                var w  = (DGV.Width - GetColumnsResizeDiff());
                if ( this.ShowResponseColumn )
                {
                    var cw = 1.0 * (DGV_requestColumn.Width + DGV_responseColumn.Width);
                    DGV_requestColumn .Width = (int) (w / (cw / Math.Max( 1, DGV_requestColumn .Width )));
                    DGV_responseColumn.Width = (int) (w / (cw / Math.Max( 1, DGV_responseColumn.Width )));
                }
                else
                {
                    DGV_requestColumn.Width = w;
                }

                if ( sender != null )
                {
                    AdjustRowsHeight();
                }
            //}
            //finally
            //{ 
                DGV.ColumnWidthChanged += DGV_ColumnWidthChanged;
            //}
        }
        private void DGV_MouseClick( object sender, MouseEventArgs e )
        {
            var hti = DGV.HitTest( e.X, e.Y );
            if ( (hti.RowIndex < 0) || (hti.ColumnIndex < 0) )
            {
                ClearSelection();
            }

            if ( e.Button == MouseButtons.Right )
            {
                _ContextMenu.Show( DGV, e.Location );
            }
        }
        private void DGV_CellValueNeeded( object sender, DataGridViewCellValueEventArgs e )
        {
            var row = _DGVRows[ e.RowIndex ];
            switch ( e.ColumnIndex )
            {
                case 0:
                    switch ( row.RequestRowType )
                    {
                        case RequestRowTypeEnum.None:
                        case RequestRowTypeEnum.Success:
                            e.Value = Get_4_RequestColumn__text( row.RequestText );
                        break;

                        case RequestRowTypeEnum.Error:
                            e.Value = GetError_4_RequestColumn__text( row.RequestText );
                        break;
                    }
                break;

                case 1:
                    switch ( row.RequestRowType )
                    {
                        case RequestRowTypeEnum.None:
                        case RequestRowTypeEnum.Success:
                            e.Value = Get_4_RequestColumn__text( row.ResponseText );
                        break;

                        case RequestRowTypeEnum.Error:
                            e.Value = GetError_4_ResponseColumn__text( row.ResponseText );
                        break;
                    }
                break;
            }
        }
        private void DGV_CellFormatting( object sender, DataGridViewCellFormattingEventArgs e )
        {
            var row = _DGVRows[ e.RowIndex ];
            CellStyle cs;
            switch ( e.ColumnIndex )
            {
                case 0:
                    switch ( row.RequestRowType )
                    {
                        case RequestRowTypeEnum.None:
                            cs = Get_4_RequestColumn__cellStyle( row.RequestText ); 
                        break;

                        case RequestRowTypeEnum.Success:
                            cs = _DefaultCellStyle_4ResponseReceived; 
                        break;

                        case RequestRowTypeEnum.Error:
                            cs = GetError_4_RequestColumn__cellStyle( row.RequestText ); 
                        break;

                        default: cs = null; break;
                    }
                break;

                case 1:
                    switch ( row.RequestRowType )
                    {
                        case RequestRowTypeEnum.None:
                            cs = Get_4_RequestColumn__cellStyle( row.ResponseText ); 
                        break;

                        case RequestRowTypeEnum.Success:
                            cs = _Rsp_ReceivedCellStyle; 
                        break;

                        case RequestRowTypeEnum.Error:
                            cs = (!row.ResponseText.IsNullOrEmpty() ? GetError_4_ResponseColumn__cellStyle( row.ResponseText ) : null); 
                        break;

                        default: cs = null; break;
                    }
                break;

                default: cs = null; break;
            }

            if ( cs != null )
            {
                e.CellStyle = cs;
                e.FormattingApplied = true;
            }
        }
        private void DGV_CellPainting( object sender, DataGridViewCellPaintingEventArgs e )
        {
            if ( 0 <= e.RowIndex )
            { 
                e.Handled = true;
                var isSelected = e.State.IsSelected();

                e.PaintBackground( e.CellBounds, isSelected );

                var cs   = e.CellStyle;
                var text = e.Value?.ToString();
                if ( !text.IsNullOrEmpty() )
                {
                    using ( var br = new SolidBrush( (isSelected ? cs.SelectionForeColor : cs.ForeColor) ) )
                    {
                        var rc = e.CellBounds;
                            rc.Inflate( -2, -2 );
                        e.Graphics.DrawString( text, cs.Font, br, rc, _SF );
                    }
                }
            }
        }        
        private void DGV_RowDividerDoubleClick( object sender, DataGridViewRowDividerDoubleClickEventArgs e )
        {
            e.Handled = true;

            var row = DGV.Rows[ e.RowIndex ];

            using ( var gr = Graphics.FromHwnd( DGV.Handle ) )
            {
                MeasureCellText( gr, row, 0, e.RowIndex, out var sz_0 );

                if ( this.ShowResponseColumn )
                {
                    MeasureCellText( gr, row, 1, e.RowIndex, out var sz_1 );

                    row.Height = (int) Math.Max( sz_0.Height, sz_1.Height );
                }
                else
                {
                    row.Height = (int) sz_0.Height;
                }
            }
        }

        [M(O.AggressiveInlining)] private void MeasureCellText( Graphics gr, DataGridViewRow row, int columnIndex, int rowIndex, out SizeF sz )
        {
            var text = row.Cells[ columnIndex ].Value?.ToString();

            var arg = new DataGridViewCellFormattingEventArgs( columnIndex, rowIndex, text, typeof(CellStyle), DGV.DefaultCellStyle );
            DGV_CellFormatting( DGV, arg );

            var font = arg.CellStyle.Font;
            sz = gr.MeasureString( text, font, DGV.Columns[ columnIndex ].Width, _SF );
        }
        #endregion
    }
}
