﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using m3u8.Properties;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class FileNameExcludesWordsEditor : Form
    {
        #region [.fields.]
        private StringFormat _SF;
        private string       _LastFilterText;
        #endregion

        #region [.ctor().]
        private FileNameExcludesWordsEditor()
        {
            InitializeComponent();

            _SF = new StringFormat( StringFormatFlags.NoWrap ) { Trimming = StringTrimming.EllipsisCharacter, Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        }
        public FileNameExcludesWordsEditor( IEnumerable< string > excludesWords ) : this()
        {
            var rows = DGV.Rows;
            foreach ( var w in excludesWords )
            {
                var row = new DataGridViewRow();                
                row.Cells.Add( new DataGridViewTextBoxCell() { Value = w } );
                rows.Add( row );
            }
        }

        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();
                _SF.Dispose();
            }
            base.Dispose( disposing );
        }
        #endregion

        #region [.override methods.]
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !base.DesignMode )
            {
                FormPositionStorer.Load( this, Settings.Default.FileNameExcludesWordsEditorPositionJson );
            }
        }
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            if ( !base.DesignMode )
            {
                Settings.Default.FileNameExcludesWordsEditorPositionJson = FormPositionStorer.Save( this );
                Settings.Default.SaveNoThrow();
            }
        }
        protected override void OnShown( EventArgs e )
        {
            base.OnShown( e );

            DGV_Resize( null, null );
        }
        protected override bool ProcessCmdKey( ref Message msg, Keys keyData )
        {
            if ( !DGV.IsCurrentCellInEditMode )
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
            }
            return (base.ProcessCmdKey( ref msg, keyData ));
        }
        #endregion

        #region [.private methods.]
        private void DGV_Resize( object sender, EventArgs e )
        {
            var vscrollBarVisible = DGV.Controls.OfType< VScrollBar >().First().Visible;
            DGV_excludesWordsColumn.Width = DGV.Width - DGV.RowHeadersWidth - 3 - (vscrollBarVisible ? SystemInformation.VerticalScrollBarWidth : 0);
        }
        private void DGV_CellPainting( object sender, DataGridViewCellPaintingEventArgs e )
        {
            if ( (0 <= e.RowIndex) && (e.ColumnIndex < 0) )
            {
                e.Handled = true;
                e.Graphics.FillRectangle( Brushes.LightGray, e.CellBounds );

                var rect = e.CellBounds; rect.Height -= 2; rect.Width -= 2;
                var pen  = ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected) ? Pens.DarkBlue : Pens.Silver;
                e.Graphics.DrawRectangle( pen, rect );

                var text = (e.RowIndex + 1).ToString(); // DGV.Rows[ e.RowIndex ].IsNewRow ? "*" : (e.RowIndex + 1).ToString();
                e.Graphics.DrawString( text, this.Font, Brushes.Black, e.CellBounds, _SF );
            }            
        }

        private void clearFilterButton_Click( object sender, EventArgs e ) => filterTextBox.Text = null;

        private async void filterTextBox_TextChanged( object sender, EventArgs e )
        {
            var text = filterTextBox.Text.Trim();

            if ( _LastFilterText != text )
            {
                _LastFilterText = text;
                await Task.Delay( 250 );
                filterTextBox_TextChanged( sender, e );
                return;
            }

            var isEmpty = text.IsNullOrEmpty();

            clearFilterButton.Enabled = !isEmpty;

            //var cm = BindingContext[ DGV.DataSource ] as CurrencyManager;
            //cm?.SuspendBinding();
            DGV.SuspendDrawing();
            try
            {
                if ( isEmpty )
                {
                    foreach ( var row in DGV.Rows.Cast< DataGridViewRow >() )
                    {
                        row.Visible = true;
                    }
                }
                else
                {
                    foreach ( var row in DGV.Rows.Cast< DataGridViewRow >() )
                    {
                        if ( !row.IsNewRow )
                        {
                            var value = row.Cells[ 0 ].Value?.ToString();
                            row.Visible = ((value != null) && (value.IndexOf( text, StringComparison.InvariantCultureIgnoreCase ) != -1));
                        }
                    }
                }
            }
            finally
            {
                //cm?.ResumeBinding();
                DGV.ResumeDrawing();
            }
        }
        #endregion

        #region [.public methods.]
        public string[] GetFileNameExcludesWords()
        {
            var data = new string[ DGV.RowCount - 1 ];
            var rows = DGV.Rows;
            for ( int i = DGV.RowCount - 1, j = 0; 0 <= i; i--  )
            {
                var row = rows[ i ];
                if ( !row.IsNewRow )
                {
                    data[ j++ ] = row.Cells[ 0 ].Value?.ToString();
                }
            }
            return (data);
        }
        #endregion
    }
}
