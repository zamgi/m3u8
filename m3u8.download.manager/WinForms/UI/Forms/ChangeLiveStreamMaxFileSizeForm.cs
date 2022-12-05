using System;
using System.Windows.Forms;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class ChangeLiveStreamMaxFileSizeForm : Form
    {
        #region [.ctor().]
        internal static bool Show( IWin32Window owner, long liveStreamMaxFileSizeInBytes, out long outputLiveStreamMaxFileSizeInBytes )
        {
            using ( var f = new ChangeLiveStreamMaxFileSizeForm( liveStreamMaxFileSizeInBytes ) )
            {
                if ( f.ShowDialog( owner ) == DialogResult.OK )
                {
                    outputLiveStreamMaxFileSizeInBytes = f.LiveStreamMaxFileSizeInBytes;
                    return (true);
                }
            }
            outputLiveStreamMaxFileSizeInBytes = default;
            return (false);
        }

        public ChangeLiveStreamMaxFileSizeForm() => InitializeComponent();
        internal ChangeLiveStreamMaxFileSizeForm( long liveStreamMaxFileSizeInBytes ) : this() => this.LiveStreamMaxFileSizeInBytes = liveStreamMaxFileSizeInBytes;
        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();
            }
            base.Dispose( disposing );
        }
        #endregion

        #region [.public.]
        public int LiveStreamMaxFileSizeInMb
        {
            get => (int) liveStreamMaxSizeInMbNumUpDn.Value;
            set => liveStreamMaxSizeInMbNumUpDn.Value = Math.Max( liveStreamMaxSizeInMbNumUpDn.Minimum, Math.Min( liveStreamMaxSizeInMbNumUpDn.Maximum, value ) );
        }
        public long LiveStreamMaxFileSizeInBytes
        {
            get => this.LiveStreamMaxFileSizeInMb << 20;
            set => this.LiveStreamMaxFileSizeInMb = (int) (value >> 20);
        }
        #endregion

        #region [.override methods.]
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
        #endregion
    }
}
