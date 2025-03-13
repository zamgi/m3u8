using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using m3u8.download.manager.ui.infrastructure;
using _SC_ = m3u8.download.manager.controllers.SettingsPropertyChangeController;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class FileNameExcludesWordsEditor : Form
    {
        #region [.fields.]
        private RowNumbersPainter _RPN;
        private _SC_ _SC;
        #endregion

        #region [.ctor().]
        public static bool TryEdit( IReadOnlyCollection< string > excludesWords, _SC_ sc, out IList< string > resultExcludesWords )
        {
            using ( var f = new FileNameExcludesWordsEditor( excludesWords, sc ) )
            {
                if ( f.ShowDialog() == DialogResult.OK )
                {
                    resultExcludesWords = f.GetFileNameExcludesWords();
                    return (true);
                }
            }
            resultExcludesWords = default;
            return (false);
        }

        private FileNameExcludesWordsEditor()
        {
            InitializeComponent();

            DGV.DefaultCellStyle = DefaultColors.DGV.Create_Suc( DGV.DefaultCellStyle );

            _RPN = RowNumbersPainter.Create( DGV, useSelectedBackColor: false );
        }
        public FileNameExcludesWordsEditor( IEnumerable< string > excludesWords, _SC_ sc ) : this()
        {
            _SC = sc;

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
                _RPN.Dispose();
            }
            base.Dispose( disposing );
        }
        #endregion

        #region [.public.]
        public IList< string > GetFileNameExcludesWords()
        {
            var data = new List< string >( DGV.RowCount - 1 );
            var rows = DGV.Rows;
            for ( var i = DGV.RowCount - 1; 0 <= i; i--  )
            {
                var row = rows[ i ];
                if ( !row.IsNewRow )
                {
                    data.Add( row.Cells[ 0 ].Value?.ToString() );
                }
            }
            return (data);
        }
        #endregion

        #region [.override methods.]
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !base.DesignMode && (_SC != null) )
            {
                FormPositionStorer.Load( this, _SC.Settings.FileNameExcludesWordsEditorPositionJson );
            }
        }
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            if ( !base.DesignMode && (_SC != null) )
            {
                _SC.Settings.FileNameExcludesWordsEditorPositionJson = FormPositionStorer.SaveOnlyPos( this );
                _SC.SaveNoThrow_IfAnyChanged();
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

                    case Keys.V | Keys.Control:
                        if ( DGV.Focused )
                        {
                            var text = Clipboard.GetText( TextDataFormat.Text )?.Trim();
                            if ( text.IsNullOrEmpty() ) text = Clipboard.GetText( TextDataFormat.UnicodeText )?.Trim();

                            if ( !text.IsNullOrEmpty() )
                            {
                                var rows = text.Split( ['\r', '\n'], StringSplitOptions.RemoveEmptyEntries );
                                foreach ( var row in rows )
                                {
                                    var line = row.Trim();
                                    if ( !line.IsNullOrEmpty() )
                                    {
                                        var n = DGV.Rows.Add( line );
                                        DGV.FirstDisplayedScrollingRowIndex = n;
                                    }
                                }
                                return (true);
                            }
                        }
                        break;
                }
            }
            return (base.ProcessCmdKey( ref msg, keyData ));
        }
        #endregion

        #region [.private methods.]
        private void DGV_Resize( object sender, EventArgs e )
        {
            var vscrollBarVisible = DGV.Controls.OfType< VScrollBar >().First().Visible;
            DGV_excludesWordsColumn.Width = DGV.Width - DGV.RowHeadersWidth //- 3 
                - (vscrollBarVisible ? SystemInformation.VerticalScrollBarWidth : 0)
                - ((DGV.BorderStyle != BorderStyle.None) ? SystemInformation.FixedFrameBorderSize.Width : SystemInformation.BorderSize.Width);
        }

        private void clearFilterButton_Click( object sender, EventArgs e ) => filterTextBox.Text = null;

        private string _LastFilterText;
        private async void filterTextBox_TextChanged( object sender, EventArgs e )
        {
            var text = filterTextBox.Text.Trim();
            if ( _LastFilterText != text )
            {
                await Task.Delay( 250 );
                _LastFilterText = text;                
                filterTextBox_TextChanged( sender, e );
                return;
            }

            var isEmpty = text.IsNullOrEmpty();

            clearFilterButton.Visible = !isEmpty;

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
                            row.Visible = (row.Cells[ 0 ].Value?.ToString()).ContainsIgnoreCase( text );
                        }
                    }
                }
            }
            finally
            {
                DGV.ResumeDrawing();
            }
        }
        #endregion
    }
}
