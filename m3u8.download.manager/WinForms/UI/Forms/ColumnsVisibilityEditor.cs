using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class ColumnsVisibilityEditor : Form
    {
        #region [.fields.]
        private RowNumbersPainter _RPN;
        private bool _HasChanges;
        private Dictionary< DataGridViewColumn, bool > _SaveColumnIsVisibleDict;
        #endregion

        #region [.ctor().]
        private ColumnsVisibilityEditor()
        {
            InitializeComponent();

            DGV.DefaultCellStyle = DefaultColors.DGV.Create_Suc( DGV.DefaultCellStyle );

            _RPN = RowNumbersPainter.Create( DGV, useSelectedBackColor: false );
        }
        internal ColumnsVisibilityEditor( IEnumerable< DataGridViewColumn > dataGridColumns, IEnumerable< DataGridViewColumn > immutableDataGridColumns = null ) : this() 
        {
            var immutableColumns = immutableDataGridColumns?.ToHashSet() ?? new HashSet< DataGridViewColumn >();
            _SaveColumnIsVisibleDict = new Dictionary< DataGridViewColumn, bool >();
            var rows = DGV.Rows;
            foreach ( var col in dataGridColumns )
            {                
                _SaveColumnIsVisibleDict[ col ] = col.Visible;

                var immutable = immutableColumns.Contains( col );
                var row = new DataGridViewRow();
                var is_visible_cell = new DataGridViewCheckBoxCell() { Value = col.Visible, Tag = (immutable ? null : col) };
                row.Cells.Add( is_visible_cell );
                row.Cells.Add( new DataGridViewTextBoxCell () { Value = col.HeaderText } );
                rows.Add( row );

                is_visible_cell.ReadOnly = immutable;              
            }
        }

        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();
                _RPN.Dispose();
            }
            base.Dispose( disposing );
        }
        #endregion

        #region [.public.]
        public bool Success => (DialogResult == DialogResult.OK);
        #endregion

        #region [.override methods.]
        //protected override void OnLoad( EventArgs e )
        //{
        //    base.OnLoad( e );
        //
        //    if ( !base.DesignMode )
        //    {
        //        FormPositionStorer.Load( this, Settings.Default.ColumnsVisibilityEditorPositionJson );
        //    }
        //}
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            //if ( !base.DesignMode )
            //{
            //    Settings.Default.ColumnsVisibilityEditorPositionJson = FormPositionStorer.SaveOnlyPos( this );
            //    Settings.Default.SaveNoThrow();
            //}

            if ( !Success && _HasChanges )
            {
                RollbackChanges();
            }
        }
        protected override void OnShown( EventArgs e )
        {
            base.OnShown( e );

            DGV_Resize( this, EventArgs.Empty );
        }
        protected override bool ProcessCmdKey( ref Message msg, Keys keyData )
        {
            switch ( keyData )
            {
                case Keys.Escape:
                    this.Close();
                    return (true);

                case Keys.Enter:
                    DialogResult = DialogResult.OK;
                    this.Close();
                    return (true);
            }
            return (base.ProcessCmdKey( ref msg, keyData ));
        }
        #endregion

        #region [.private methods.]
        private bool TryGetCell( int rowIndex, int columnIndex, out DataGridViewCell cell )
        {
            if ( (0 <= rowIndex) && (columnIndex == DGV_isVisibleColumn.DisplayIndex) )
            {
                cell = DGV.Rows[ rowIndex ].Cells[ columnIndex ];
                return (true);
            }
            cell = default;
            return (false);
        }
        private bool TryGetTargetColumn( int rowIndex, int columnIndex, out (DataGridViewColumn TargetColumn, DataGridViewCell Cell) t )
        {
            if ( TryGetCell( rowIndex, columnIndex, out var cell ) && (cell.Tag is DataGridViewColumn col) )
            {
                t = (col, cell);
                return (true);
            }
            t = default;
            return (false);
        }
        private bool TryGetTargetColumn( DataGridViewRow row, int columnIndex, out (DataGridViewColumn TargetColumn, DataGridViewCell Cell) t )
        {
            var cell = row.Cells[ columnIndex ];
            if ( (cell != null) && (cell.Tag is DataGridViewColumn col) )
            {
                t = (col, cell);
                return (true);
            }
            t = default;
            return (false);
        }
        private IEnumerable< DataGridViewColumn > GetTargetColumns()
        {
            var idx  = DGV_isVisibleColumn.DisplayIndex;
            for ( var i = DGV.RowCount - 1; 0 <= i; i-- )
            {
                if ( TryGetTargetColumn( i, idx, out var t ) )
                {
                    //var isVisible = Convert.ToBoolean( cell.Value );
                    yield return (t.TargetColumn);
                }
            }
        }
        private void SetHasChanges()
        {
            _HasChanges = false;
            foreach ( var col in GetTargetColumns() )
            {                
                if ( _SaveColumnIsVisibleDict.TryGetValue( col, out var isVisible ) && (col.Visible != isVisible) )
                {
                    _HasChanges = true;
                    break;
                }
            }

            #region [.change window title text.]
            var lastChar = this.Text.LastOrDefault();
            if ( _HasChanges )
            {
                if ( lastChar != '*' )
                {
                    this.Text += " *";
                }
            }
            else if ( lastChar == '*' )
            {
                this.Text = this.Text.Substring( 0, this.Text.Length - 2 );
            }
            #endregion
        }
        private void RollbackChanges()
        {
            foreach ( var p in _SaveColumnIsVisibleDict ) 
            {
                var (col, isVisible) = (p.Key, p.Value);
                col.Visible = isVisible;
            }
        }

        private void DGV_Resize( object sender, EventArgs e )
        {
            var vscrollBarVisible = DGV.Controls.OfType< VScrollBar >().First().Visible;
            DGV_columnNameColumn.Width = DGV.Width - DGV_isVisibleColumn.Width - DGV.RowHeadersWidth - 3 - (vscrollBarVisible ? SystemInformation.VerticalScrollBarWidth : 0);
        }
        private void DGV_ColumnWidthChanged( object sender, DataGridViewColumnEventArgs e )
        {
            if ( e.Column != DGV_columnNameColumn ) DGV_Resize( sender, EventArgs.Empty );
        }
        private void DGV_RowHeadersWidthChanged( object sender, EventArgs e ) => DGV_Resize( sender, EventArgs.Empty );

        private void DGV_CellPainting( object sender, DataGridViewCellPaintingEventArgs e )
        {
            if ( TryGetCell( e.RowIndex, e.ColumnIndex, out var cell ) && cell.ReadOnly )
            {
                e.Handled = true;
                e.PaintBackground( e.ClipBounds, (e.State & DataGridViewElementStates.Selected) != 0 );
                var is_checked    = Convert.ToBoolean( cell.Value );
                var checkBoxState = is_checked ? CheckBoxState.CheckedDisabled : CheckBoxState.UncheckedDisabled;
                var rc = e.CellBounds;
                var sz = CheckBoxRenderer.GetGlyphSize( e.Graphics, checkBoxState );
                var pt = new Point( rc.X + (rc.Width - sz.Width) / 2, rc.Y + (rc.Height - sz.Height) / 2 );
                CheckBoxRenderer.DrawCheckBox( e.Graphics, pt, checkBoxState );
            }
        }
        private void DGV_CellContentClick( object sender, DataGridViewCellEventArgs e )
        {
            if ( DGV.IsCurrentCellInEditMode )
            {
                DGV.CommitEdit( DataGridViewDataErrorContexts.Commit );
            }
        }
        private void DGV_CellValueChanged( object sender, DataGridViewCellEventArgs e )
        {
            if ( TryGetTargetColumn( e.RowIndex, e.ColumnIndex, out var t ) )
            {
                var isVisible = Convert.ToBoolean( t.Cell.Value );
                t.TargetColumn.Visible = isVisible;

                #region [.spread to selected rows.]
                var srs = DGV.SelectedRowsBuf/*DGV.SelectedRows.Cast< DataGridViewRow >()*/;
                if ( 0 < srs.Count )
                {
                    DGV.CellValueChanged -= DGV_CellValueChanged;
                    try
                    {
                        foreach ( var selRow in srs )
                        {
                            if ( (selRow.Index != e.RowIndex) && TryGetTargetColumn( selRow, e.ColumnIndex, out t ) )
                            {
                                t.Cell.Value = isVisible;
                                t.TargetColumn.Visible = isVisible;
                            }
                        }
                    }
                    finally
                    {
                        DGV.CellValueChanged += DGV_CellValueChanged;
                    }
                }
                #endregion

                SetHasChanges();
            }
        }

        private bool DGV_IsNeedSaveSelectionByMouseDown( MouseEventArgs e )
        {
            var ht = DGV.HitTest( e.X, e.Y );
            return ((0 <= ht.RowIndex) && (ht.ColumnIndex == DGV_isVisibleColumn.DisplayIndex));
        }
        #endregion
    }
}
