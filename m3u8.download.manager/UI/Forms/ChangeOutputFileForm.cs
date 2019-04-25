using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using m3u8.download.manager.infrastructure;
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
                value = value?.Trim();
                if ( outputFileNameTextBox.Text.Trim() != value )
                {
                    outputFileNameTextBox.TextChanged -= outputFileNameTextBox_TextChanged;
                    outputFileNameTextBox.Text = value;
                    outputFileNameTextBox.TextChanged += outputFileNameTextBox_TextChanged;
                }
            }
        }

        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();
                CancelAndDispose_Cts_outputFileNameTextBox_TextChanged();
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
                var fn = FileNameCleaner.GetOutputFileName( this.OutputFileName );
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
        
        private CancellationTokenSource _Cts_outputFileNameTextBox_TextChanged;
        private void CancelAndDispose_Cts_outputFileNameTextBox_TextChanged()
        {
            if ( _Cts_outputFileNameTextBox_TextChanged != null )
            {
                _Cts_outputFileNameTextBox_TextChanged.Cancel();
                _Cts_outputFileNameTextBox_TextChanged.Dispose();
                _Cts_outputFileNameTextBox_TextChanged = null;
            }
        }
        private void outputFileNameTextBox_TextChanged( object sender, EventArgs e )
        {
            CancelAndDispose_Cts_outputFileNameTextBox_TextChanged();

            _Cts_outputFileNameTextBox_TextChanged = new CancellationTokenSource();
            Task.Delay( 1_000, _Cts_outputFileNameTextBox_TextChanged.Token )
                .ContinueWith( async t =>
                {
                    if ( t.IsCanceled )
                        return;

                    await FileNameCleaner.SetOutputFileName_Async( this.OutputFileName,
                                                                   outputFileName => this.OutputFileName = outputFileName,
                                                                   millisecondsDelay: 150 );
                }, TaskScheduler.FromCurrentSynchronizationContext() );
        }
        #endregion
    }
}
