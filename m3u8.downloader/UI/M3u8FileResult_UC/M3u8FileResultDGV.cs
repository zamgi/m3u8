using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using CellStyle = System.Windows.Forms.DataGridViewCellStyle;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class M3u8FileResultDGV : M3u8FileResultUCBase
    {
        /// <summary>
        /// 
        /// </summary>
        private struct RowHolder : IRowHolder
        {
            public RowHolder( DataGridViewRow row ) => Row = row;
            public DataGridViewRow Row { get; }
        }

        #region [.field's.]
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

        private ContextMenuStrip  _ContextMenu;
        private ToolStripMenuItem _ShowOnlyRequestRowsWithErrorsMenuItem;
        #endregion

        #region [.ctor().]
        public M3u8FileResultDGV()
        {
            InitializeComponent();
            //----------------------------------------------//

            _Req_ReceivedCellStyle = new CellStyle() { WrapMode = DataGridViewTriState.True, ForeColor = Color.DimGray };
            _Rsp_ReceivedCellStyle = new CellStyle() { WrapMode = DataGridViewTriState.True, ForeColor = Color.DimGray };
            _Req_ErrorCellStyle    = new CellStyle() { WrapMode = DataGridViewTriState.True, ForeColor = Color.Red, BackColor = Color.Yellow };
            _Rsp_ErrorCellStyle    = new CellStyle() { WrapMode = DataGridViewTriState.True, ForeColor = Color.Red, BackColor = Color.Yellow };

            var smallFont_1 = new Font( DGV.Font.FontFamily   , DGV.Font.Size * 4 / 5 );
            var smallFont_2 = new Font( smallFont_1.FontFamily, smallFont_1.Size * 4 / 5 );

            _Req_ErrorCellStyleSmallFont_1 = new CellStyle() { WrapMode = DataGridViewTriState.True, Font = smallFont_1, ForeColor = Color.Red, BackColor = Color.Yellow };
            _Rsp_ErrorCellStyleSmallFont_1 = new CellStyle() { WrapMode = DataGridViewTriState.True, Font = smallFont_1, ForeColor = Color.Red, BackColor = Color.Yellow };
            _Req_ErrorCellStyleSmallFont_2 = new CellStyle() { WrapMode = DataGridViewTriState.True, Font = smallFont_2, ForeColor = Color.Red, BackColor = Color.Yellow };
            _Rsp_ErrorCellStyleSmallFont_2 = new CellStyle() { WrapMode = DataGridViewTriState.True, Font = smallFont_2, ForeColor = Color.Red, BackColor = Color.Yellow };
            _Req_CellStyleSmallFont_1      = new CellStyle() { WrapMode = DataGridViewTriState.True, Font = smallFont_1 };
            _Rsp_CellStyleSmallFont_1      = new CellStyle() { WrapMode = DataGridViewTriState.True, Font = smallFont_1 };
            _Req_CellStyleSmallFont_2      = new CellStyle() { WrapMode = DataGridViewTriState.True, Font = smallFont_2 };
            _Rsp_CellStyleSmallFont_2      = new CellStyle() { WrapMode = DataGridViewTriState.True, Font = smallFont_2 };

            _ShowOnlyRequestRowsWithErrorsMenuItem = new ToolStripMenuItem( "Show only request rows with errors", null, _ShowOnlyRequestRowsWithErrors_Click ) { Checked = base.ShowOnlyRequestRowsWithErrors };
            _ContextMenu = new ContextMenuStrip();
            _ContextMenu.Items.Add( _ShowOnlyRequestRowsWithErrorsMenuItem );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            DGV_Resize( null, null );
        }
        #endregion

        #region [.DGV.]
        [M(O.AggressiveInlining)] private void ClearSelection()
        {
            DGV.ClearSelection();
            DGV.CurrentCell = null;
        }
        [M(O.AggressiveInlining)] private int AddRow( DataGridViewRow row )
        {
            var hasSelection = (DGV.CurrentCell != null);
            var rowIndex = DGV.Rows.Add( row );
            if ( !hasSelection && (DGV.CurrentCell != null) )
            {
                ClearSelection();
            }
            return (rowIndex);
        }
        [M(O.AggressiveInlining)] private int AddRow_Fast( DataGridViewRow row ) => DGV.Rows.Add( row );
        private int GetColumnsResizeDiff() => (DGV.RowHeadersVisible ? DGV.RowHeadersWidth : 0) + 
                                              (IsVerticalScrollBarVisible ? SystemInformation.VerticalScrollBarWidth : 0) + //3 + 
                                              ((DGV.BorderStyle != BorderStyle.None) ? SystemInformation.FixedFrameBorderSize.Width : 0);

        private enum RequestRowTypeEnum { None, Success, Error };
        [M(O.AggressiveInlining)] private static bool TryGetRequestRowType( DataGridViewRow row, out RequestRowTypeEnum rt )
        {
            if ( row.Tag == null )
            {
                rt = RequestRowTypeEnum.None;
                return (false);
            }
            rt = (RequestRowTypeEnum) row.Tag;
            return (true);
        }
        [M(O.AggressiveInlining)] private static void MarkAsRequestRow( DataGridViewRow row ) => row.Tag = RequestRowTypeEnum.Success;
        [M(O.AggressiveInlining)] private static void MarkAsRequestRowWithError( DataGridViewRow row ) => row.Tag = RequestRowTypeEnum.Error;
        [M(O.AggressiveInlining)] private static DataGridViewRow CreateRow( RequestRowTypeEnum rt = RequestRowTypeEnum.None )
        {
            var row = new DataGridViewRow();
            switch ( rt )
            {
                case RequestRowTypeEnum.Error  : MarkAsRequestRowWithError( row ); break;
                case RequestRowTypeEnum.Success: MarkAsRequestRow         ( row ); break;
            }
            return (row);
        }

        private void SetRowsVisiblity()
        {
            DGV.SuspendDrawing();
            try
            {
                if ( base.ShowOnlyRequestRowsWithErrors )
                {
                    foreach ( var row in DGV.Rows.Cast< DataGridViewRow >() )
                    {
                        if ( TryGetRequestRowType( row, out var rt ) )
                        {
                            row.Visible = (rt == RequestRowTypeEnum.Error);
                        }
                    }
                }
                else
                {
                    foreach ( var row in DGV.Rows.Cast< DataGridViewRow >() )
                    {
                        if ( TryGetRequestRowType( row, out var _ ) )
                        {
                            row.Visible = true;
                        }
                    }
                }
            }
            finally
            {
                DGV.ResumeDrawing();
            }
        }
        private void _ShowOnlyRequestRowsWithErrors_Click( object sender, EventArgs e ) => this.ShowOnlyRequestRowsWithErrors = !this.ShowOnlyRequestRowsWithErrors;

        private void DGV_ColumnWidthChanged( object sender, DataGridViewColumnEventArgs e )
        {
            DGV.ColumnWidthChanged -= DGV_ColumnWidthChanged;
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

                var w = (DGV.Width - GetColumnsResizeDiff());
                var cw = w - col_1.Width;
                if ( col_2.MinimumWidth <= cw )
                {
                    col_2.Width = cw;
                }
                else
                {
                    col_2.Width = col_2.MinimumWidth;
                    col_1.Width = w - DGV_responseColumn.Width;
                }

                AdjustRowsHeight();
            }
            DGV.ColumnWidthChanged += DGV_ColumnWidthChanged;
        }
        private void DGV_Resize( object sender, EventArgs e )
        {
            if ( this.FindForm()?.WindowState == FormWindowState.Minimized )
            {
                return;
            }

            DGV.ColumnWidthChanged -= DGV_ColumnWidthChanged;
            {
                var w  = 1.0 * (DGV.Width - GetColumnsResizeDiff());
                var cw = 1.0 * (DGV_requestColumn.Width + DGV_responseColumn.Width);
                DGV_requestColumn .Width = (int) (w / (cw / DGV_requestColumn.Width));
                DGV_responseColumn.Width = (int) (w / (cw / DGV_responseColumn.Width));

                if ( sender != null )
                {
                    AdjustRowsHeight();
                }
            }
            DGV.ColumnWidthChanged += DGV_ColumnWidthChanged;
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
        #endregion

        #region [.get cell-styles.]
        private const int MAX_TEXT_LENGTH = 2048;
        private const int TEXT_LENGTH_250 = 250;
        private const int TEXT_LENGTH_500 = 500;
        private (string text, CellStyle cellStyle) Get_4_RequestColumn( string text, int maxLength = MAX_TEXT_LENGTH )
        {
            text = text.TrimIfLongest( maxLength );
            var cellStyle = ((TEXT_LENGTH_250 < text.Length) ? ((TEXT_LENGTH_500 < text.Length) ? _Req_CellStyleSmallFont_2 : _Req_CellStyleSmallFont_1) : null);
            return (text, cellStyle);
        }
        private (string errorText, CellStyle errorCellStyle) GetError_4_RequestColumn( string errorText, int maxLength = MAX_TEXT_LENGTH )
        {
            errorText = errorText.TrimIfLongest( maxLength );
            var errorCellStyle = ((TEXT_LENGTH_250 < errorText.Length) ? ((TEXT_LENGTH_500 < errorText.Length) ? _Req_ErrorCellStyleSmallFont_2 : _Req_ErrorCellStyleSmallFont_1) : _Req_ErrorCellStyle);
            return (errorText, errorCellStyle);
        }
        private (string errorText, CellStyle errorCellStyle) GetError_4_RequestColumn( Exception ex ) => GetError_4_RequestColumn( ex.ToString() );
        private (string errorText, CellStyle errorCellStyle) GetError_4_ResponseColumn( string errorText, int maxLength = MAX_TEXT_LENGTH )
        {
            errorText = errorText.TrimIfLongest( maxLength );
            var errorCellStyle = ((TEXT_LENGTH_250 < errorText.Length) ? ((TEXT_LENGTH_500 < errorText.Length) ? _Rsp_ErrorCellStyleSmallFont_2 : _Rsp_ErrorCellStyleSmallFont_1) : _Rsp_ErrorCellStyle);
            return (errorText, errorCellStyle);
        }
        private (string errorText, CellStyle errorCellStyle) GetError_4_ResponseColumn( Exception ex ) => GetError_4_ResponseColumn( ex.ToString() );
        #endregion

        #region [.public override methods.]
        protected override void ShowOnlyRequestRowsWithErrors_OnChanged()
        {
            _ShowOnlyRequestRowsWithErrorsMenuItem.Checked = this.ShowOnlyRequestRowsWithErrors;
            SetRowsVisiblity();
        }
        public override bool IsVerticalScrollBarVisible => DGV.Controls.OfType< VScrollBar >().First().Visible;
        public override void AdjustColumnsWidthSprain() => DGV_Resize( null, null );
        public override void AdjustRowsHeight()
        {
            const int DGV_ROWS_COUNT_MAX_THRESHOLD = 10_000 + 10;

            var endRowIndex = DGV.RowCount - 1;
            if ( (0 <= endRowIndex) && (endRowIndex <= DGV_ROWS_COUNT_MAX_THRESHOLD) )
            {
                DGV.SuspendDrawing();
                DGV.SuspendLayout();
                {
                    DGV.AutoResizeRows( DataGridViewAutoSizeRowsMode.AllCells );
                }
                DGV.ResumeLayout( true );
                DGV.ResumeDrawing();

                /*const int DGV_ROWS_COUNT_MIN_THRESHOLD = 500;

                for ( var startRowIndex = Math.Max( 0, endRowIndex - DGV_ROWS_COUNT_MIN_THRESHOLD ); startRowIndex <= endRowIndex; endRowIndex-- )
                {
                    DGV.AutoResizeRow( endRowIndex, DataGridViewAutoSizeRowMode.AllCells );
                }*/
            }
        }
        public override void SetFocus() => DGV.Focus();
        public override void Clear() => DGV.Rows.Clear();

        public override void Output( m3u8_file_t m3u8File, IEnumerable< string > lines )
        {
            DGV.SuspendDrawing();
            DGV.SuspendLayout();
            {
                const int LINES_COUNT_MIN_THRESHOLD = 2000;

                Clear();
                if ( m3u8File.Parts.Count <= LINES_COUNT_MIN_THRESHOLD )
                {
                    foreach ( var line in lines )
                    {
                        AppendRequestText( line, false );
                    }
                }
                else
                {
                    foreach ( var line in lines )
                    {
                        AppendRequestText_Fast( line );
                    }
                }
                AppendEmptyLine();
                AppendRequestText( $" patrs count: {m3u8File.Parts.Count}" );
                ClearSelection();
                AdjustColumnsWidthSprain();
            }
            DGV.ResumeLayout( true );
            DGV.ResumeDrawing();
        }

        public override void AppendEmptyLine() => DGV.FirstDisplayedScrollingRowIndex = AddRow( CreateRow() );
        public override IRowHolder AppendRequestText( string requestText, bool ensureVisible = true )
        {
            var t = Get_4_RequestColumn( requestText );

            var row = CreateRow();
            row.Cells.Add( new DataGridViewTextBoxCell() { Value = t.text, Style = t.cellStyle } );

            var rowIndex = AddRow( row );
            if ( ensureVisible )
            {
                DGV.FirstDisplayedScrollingRowIndex = rowIndex;
            }
            DGV.AutoResizeRow( rowIndex, DataGridViewAutoSizeRowMode.AllCells );            
            return (new RowHolder( row ));
        }
        private IRowHolder AppendRequestText_Fast( string requestText )
        {
            var t = Get_4_RequestColumn( requestText );

            var row = CreateRow();
            row.Cells.Add( new DataGridViewTextBoxCell() { Value = t.text, Style = t.cellStyle } );

            AddRow_Fast( row );
            //---var rowIndex = AddRow( row );
            //---DGV.AutoResizeRow( rowIndex, DataGridViewAutoSizeRowMode.AllCells );            
            return (new RowHolder( row ));
        }

        public override void AppendRequestErrorText( Exception ex ) => _AppendRequestErrorText( GetError_4_RequestColumn( ex ) );
        public override void AppendRequestErrorText( string errorText ) => _AppendRequestErrorText( GetError_4_RequestColumn( errorText ) );
        private void _AppendRequestErrorText( (string errorText, CellStyle errorCellStyle) x )
        {
            var row = CreateRow( RequestRowTypeEnum.Error );
            row.Cells.Add( new DataGridViewTextBoxCell() { Value = x.errorText, Style = x.errorCellStyle } );

            var rowIndex = AddRow( row );
            DGV.FirstDisplayedScrollingRowIndex = rowIndex;
            DGV.AutoResizeRow( rowIndex, DataGridViewAutoSizeRowMode.AllCells );
        }        
        public override void AppendRequestAndResponseErrorText( string requestText, Exception responseError )
        {
            var t = Get_4_RequestColumn      ( requestText );
            var x = GetError_4_ResponseColumn( responseError );

            var row = CreateRow( RequestRowTypeEnum.Error );
            row.Cells.Add( new DataGridViewTextBoxCell() { Value = t.text     , Style = t.cellStyle      } );
            row.Cells.Add( new DataGridViewTextBoxCell() { Value = x.errorText, Style = x.errorCellStyle } );

            var rowIndex = AddRow( row );
            DGV.FirstDisplayedScrollingRowIndex = rowIndex;
            DGV.AutoResizeRow( rowIndex, DataGridViewAutoSizeRowMode.AllCells );
        }

        public override void SetResponseErrorText( IRowHolder holder, Exception ex )
        {
            var x = GetError_4_ResponseColumn( ex );
            var row = ((RowHolder) holder).Row;
            MarkAsRequestRowWithError( row );
            row.Visible = true;
            row.Cells[ 1 ] = new DataGridViewTextBoxCell() { Value = x.errorText, Style = x.errorCellStyle };
            DGV.AutoResizeRow( row.Index, DataGridViewAutoSizeRowMode.AllCells );
        }

        private static Action< DataGridViewRow > _SetRowInvisibleAction = new Action< DataGridViewRow >( row =>
        {
            try
            {
                row.Visible = false;
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        });
        public override void SetResponseReceivedText( IRowHolder holder, string receivedText )
        {
            var row = ((RowHolder) holder).Row;
            MarkAsRequestRow( row );
            //---row.Visible = !base.ShowOnlyRequestRowsWithErrors;
            var cell_1 = row.Cells[ 0 ];
            var cs = (cell_1.Style ?? DGV.Columns[ 0 ].DefaultCellStyle);
            cell_1.Style = (cs != null) 
                           ? new CellStyle( cs ) { ForeColor = _Req_ReceivedCellStyle.ForeColor } 
                           : _Req_ReceivedCellStyle;
            row.Cells[ 1 ] = new DataGridViewTextBoxCell() { Value = receivedText, Style = _Rsp_ReceivedCellStyle };
            DGV.AutoResizeRow( row.Index, DataGridViewAutoSizeRowMode.AllCells );

            if ( base.ShowOnlyRequestRowsWithErrors )
            {
                Task.Delay( 250 ).ContinueWith( _ => this.BeginInvoke( _SetRowInvisibleAction, row ) );
            }
        }
        #endregion
    }
}
