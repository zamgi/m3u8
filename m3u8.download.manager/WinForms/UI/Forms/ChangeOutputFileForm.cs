using System;
using System.IO;
using System.Windows.Forms;

using m3u8.download.manager.models;
using m3u8.download.manager.ui.infrastructure;
using _SC_ = m3u8.download.manager.controllers.SettingsPropertyChangeController;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class ChangeOutputFileForm : Form
    {
        #region [.field's.]
        private FileNameCleaner4UI.Processor _FNCP;
        private _SC_ _SC;
        #endregion

        #region [.ctor().]
        internal static bool TryChangeOutputFile( IWin32Window owner, DownloadRow row, _SC_ sc, out string outputFileName )
        {
            using ( var f = new ChangeOutputFileForm( row, sc ) )
            {
                if ( (f.ShowDialog( owner ) == DialogResult.OK) &&
                     FileNameCleaner4UI.TryGetOutputFileName( f.OutputFileName, out outputFileName )
                   )
                {
                    return (true);
                }
            }
            outputFileName = default;
            return (false);
        }

        public ChangeOutputFileForm()
        {
            InitializeComponent();

            _FNCP = new FileNameCleaner4UI.Processor( outputFileNameTextBox, () => this.OutputFileName, outputFileName => this.OutputFileName = outputFileName );
        }
        internal ChangeOutputFileForm( DownloadRow row, _SC_ sc ) : this()
        {
            (Row, this.OutputFileName) = (row, row.OutputFileName);
            _SC = sc;

            set_outputFileNameTextBox_Selection_Position( this.OutputFileName );
            _FNCP.FileNameTextBox_TextChanged( outputFileName => set_outputFileNameTextBox_Selection_Position( outputFileName ) );
        }
        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();
                _FNCP.Dispose(); //CancelAndDispose_Cts_outputFileNameTextBox_TextChanged();
            }
            base.Dispose( disposing );
        }
        #endregion

        #region [.public.]
        public DownloadRow Row { get; }
        public string OutputFileName
        {
            get => outputFileNameTextBox.Text.Trim();
            set
            {
                value = value?.Trim();
                if ( outputFileNameTextBox.Text.Trim() != value )
                {
                    outputFileNameTextBox.TextChanged -= outputFileNameTextBox_TextChanged;
                    outputFileNameTextBox.Text = value;
                    outputFileNameTextBox.TextChanged += outputFileNameTextBox_TextChanged;
                }
            }
        }
        #endregion

        #region [.override methods.]
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !base.DesignMode && (_SC != null) )
            {
                FormPositionStorer.LoadAllExcludeHeight( this, _SC.Settings.ChangeOutputFileFormPositionJson, 200, 70 );
            }
        }
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            if ( !base.DesignMode && (_SC != null) )
            {
                _SC.Settings.ChangeOutputFileFormPositionJson = FormPositionStorer.SaveOnlyPos( this );
                _SC.SaveNoThrow_IfAnyChanged();
            }
        }
        protected override void OnFormClosing( FormClosingEventArgs e )
        {
            base.OnFormClosing( e );

            if ( DialogResult == DialogResult.OK )
            {
                var fn = FileNameCleaner4UI.GetOutputFileName( this.OutputFileName );
                if ( fn.IsNullOrWhiteSpace() || (Path.GetExtension( fn ) == fn) )
                {
                    e.Cancel = true;
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
            this.OutputFileName = null;
            outputFileNameTextBox.Focus();
        }
        private void outputFileNameTextBox_TextChanged( object sender, EventArgs e ) => _FNCP.FileNameTextBox_TextChanged();

        private void set_outputFileNameTextBox_Selection_Position( string outputFileName )
        {
            if ( !outputFileName.IsNullOrEmpty() )
            {
                var idx = outputFileName.IndexOf( '.' );
                outputFileNameTextBox.SelectionStart  = ((idx != -1) ? idx : outputFileName.Length);
                outputFileNameTextBox.SelectionLength = 0;
            }
        }
        #endregion
    }
}
