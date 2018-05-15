using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class FileNameExcludesWordsEditor : Form
    {
        private StringFormat _SF;

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

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && (components != null) )
            {
                components.Dispose();
                _SF.Dispose();
            }
            base.Dispose( disposing );
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
        protected override void OnShown( EventArgs e )
        {
            base.OnShown( e );

            DGV_Resize( null, null );
        }

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
                var pen = ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected) ? Pens.DarkBlue : Pens.Silver;
                e.Graphics.DrawRectangle( pen, rect );

                var text = ((DataGridView) sender).Rows[ e.RowIndex ].IsNewRow ? "*" : (e.RowIndex + 1).ToString();
                e.Graphics.DrawString( text, this.Font, Brushes.Black, e.CellBounds, _SF );
            }            
        }


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

        private void clearFilterButton_Click( object sender, EventArgs e ) => filterTextBox.Text = null;

        private string _LastFilterText;
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
    }
}
