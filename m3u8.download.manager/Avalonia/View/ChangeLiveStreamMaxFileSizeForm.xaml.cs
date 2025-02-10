using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

using m3u8.download.manager.models;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ChangeLiveStreamMaxFileSizeForm : Window
    {
        #region [.markup fields.]
        private NumericUpDown liveStreamMaxSizeInMbNumUpDn;
        #endregion

        #region [.ctor().]
        public ChangeLiveStreamMaxFileSizeForm()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        internal ChangeLiveStreamMaxFileSizeForm( DownloadRow row ) : this() => (Row, this.LiveStreamMaxFileSizeInBytes) = (row, row.LiveStreamMaxFileSizeInBytes);
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );

            liveStreamMaxSizeInMbNumUpDn = this.Find< NumericUpDown >( nameof(liveStreamMaxSizeInMbNumUpDn) );
            this.Find< Button >( "okButton"     ).Click += (s, e) => OkButtonProcess();
            this.Find< Button >( "cancelButton" ).Click += (s, e)  => this.Close();
        }

        protected override void OnOpened( EventArgs e )
        {
            base.OnOpened( e );

            liveStreamMaxSizeInMbNumUpDn.Focus();
        }
        protected override void OnKeyDown( KeyEventArgs e )
        {
            switch ( e.Key )
            {
                case Key.Escape:
                    e.Handled = true;
                    this.Close(); 
                    return;

                case Key.Enter: //Ok
                    if ( OkButtonProcess() )
                    {
                        e.Handled = true;
                        return;
                    }
                    break;
            }

            base.OnKeyDown( e );
        }
        #endregion

        #region [.private methods.]
        private bool IsValid()
        {
            //var fn = FileNameCleaner.GetOutputFileName( this.OutputFileName );
            //if ( fn.IsNullOrWhiteSpace() || (Path.GetExtension( fn ) == fn) )
            //{                
            //    outputFileNameTextBox.FocusAndBlinkBackColor();
            //    return (false);
            //}
            return (true);
        }
        private bool OkButtonProcess()
        {
            if ( IsValid() )
            {
                this.Success = true;
                this.Close();
                return (true);
            }
            return (false);
        }
        #endregion

        #region [.public methods.]
        public DownloadRow Row { get; }
        public bool Success { get; private set; }
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
    }
}
