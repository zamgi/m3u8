using System;
using System.IO;
using System.Windows.Forms;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class ChangeOutputFileForm : Form
    {
        public ChangeOutputFileForm()
        {
            InitializeComponent();
        }

        protected override void OnFormClosing( FormClosingEventArgs e )
        {
            base.OnFormClosing( e );

            if ( DialogResult == DialogResult.OK )
            {
                var outputFileName = OutputFileName;
                e.Cancel = (outputFileName.IsNullOrWhiteSpace() || (Path.GetExtension( outputFileName ) == outputFileName));
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

        private void okButton_Click( object sender, EventArgs e )
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }
        private void cancelButton_Click( object sender, EventArgs e )
        {
            this.Close();
        }

        private void outputFileNameClearButton_Click( object sender, EventArgs e )
        {
            OutputFileName = null;
            outputFileNameTextBox.Focus();
        }

        public string OutputFileName
        {
            get { return (outputFileNameTextBox.Text.Trim()); }
            set
            {
                if ( outputFileNameTextBox.Text.Trim() != value )
                {
                    outputFileNameTextBox.Text = value;
                }
            }
        }
    }
}
