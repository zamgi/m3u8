using System;
using System.IO;
using System.Windows.Forms;

using m3u8.download.manager.Properties;
using m3u8.download.manager.ui.infrastructure;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class ChangeOutputFileForm : Form
    {
        #region [.ctor().]
        public ChangeOutputFileForm() => InitializeComponent();
        #endregion

        #region [.public.]
        public string OutputFileName
        {
            get => outputFileNameTextBox.Text.Trim();
            set
            {
                if ( outputFileNameTextBox.Text.Trim() != value )
                {
                    outputFileNameTextBox.Text = value;
                }
            }
        }
        #endregion

        #region [.override methods.]
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !base.DesignMode )
            {
                FormPositionStorer.LoadAllExcludeHeight( this, Settings.Default.ChangeOutputFileFormPositionJson, 200, 70 );
            }
        }
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            if ( !base.DesignMode )
            {
                Settings.Default.ChangeOutputFileFormPositionJson = FormPositionStorer.SaveOnlyPos( this );
                Settings.Default.SaveNoThrow();
            }
        }
        protected override void OnFormClosing( FormClosingEventArgs e )
        {
            base.OnFormClosing( e );

            if ( DialogResult == DialogResult.OK )
            {
                var outputFileName = OutputFileName;
                e.Cancel = (outputFileName.IsNullOrWhiteSpace() || (Path.GetExtension( outputFileName ) == outputFileName));
                if ( e.Cancel )
                {
                    outputFileNameTextBox.FocusAndBlinkBackColor();
                }
            }
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
        private void okButton_Click( object sender, EventArgs e )
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }
        private void cancelButton_Click( object sender, EventArgs e ) => this.Close();

        private void outputFileNameClearButton_Click( object sender, EventArgs e )
        {
            OutputFileName = null;
            outputFileNameTextBox.Focus();
        }
        #endregion
    }
}
