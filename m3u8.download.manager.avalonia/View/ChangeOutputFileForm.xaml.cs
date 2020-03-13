using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using m3u8.download.manager.infrastructure;
using m3u8.download.manager.models;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ChangeOutputFileForm : Window
    {
        #region [.markup fields.]
        private TextBox outputFileNameTextBox;

        private IDisposable outputFileNameTextBox_SubscribeDisposable;
        #endregion

        #region [.fields.]
        private FileNameCleaner.Processor _FNCP;
        #endregion

        #region [.ctor().]
        public ChangeOutputFileForm()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        internal ChangeOutputFileForm( DownloadRow row ) : this()
        {
            (Row, this.OutputFileName) = (row, row.OutputFileName);

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var fn = this.OutputFileName;
                var idx = fn.IndexOf( '.' );
                outputFileNameTextBox.SelectionStart =
                        outputFileNameTextBox.SelectionEnd = (idx != -1) ? idx : (fn?.Length).GetValueOrDefault( 0 );
            });
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );

            outputFileNameTextBox = this.Find< TextBox >( nameof(outputFileNameTextBox) );
            this.Find< Button >( "outputFileNameClearButton" ).Click += outputFileNameClearButton_Click;
            this.Find< Button >( "okButton"                  ).Click += okButton_Click;
            this.Find< Button >( "cancelButton"              ).Click += cancelButton_Click;

            _FNCP = new FileNameCleaner.Processor( outputFileNameTextBox, () => this.OutputFileName, outputFileName => this.OutputFileName = outputFileName );

            outputFileNameTextBox_SubscribeDisposable = outputFileNameTextBox.GetObservable( TextBox.TextProperty ).Subscribe( outputFileNameTextBox_TextChanged );
        }

        protected override void OnOpened( EventArgs e )
        {
            base.OnOpened( e );

            outputFileNameTextBox.Focus();
        }
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            outputFileNameTextBox_SubscribeDisposable.Dispose_NoThrow();
            _FNCP.Dispose_NoThrow();
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
        private bool _IsTurnOff__outputFileNameTextBox_TextChanged;

        private bool IsValid()
        {
            var fn = FileNameCleaner.GetOutputFileName( this.OutputFileName );
            if ( fn.IsNullOrWhiteSpace() || (Path.GetExtension( fn ) == fn) )
            {                
                outputFileNameTextBox.FocusAndBlinkBackColor();
                return (false);
            }
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
        private void okButton_Click( object sender, EventArgs e ) => OkButtonProcess();
        private void cancelButton_Click( object sender, EventArgs e ) => this.Close();

        private void outputFileNameClearButton_Click( object sender, EventArgs e )
        {
            this.OutputFileName = null;
            outputFileNameTextBox.Focus();
        }
        private void outputFileNameTextBox_TextChanged( string value )
        {
            if ( !_IsTurnOff__outputFileNameTextBox_TextChanged )
            {
                _FNCP.FileNameTextBox_TextChanged();
            }
        }
        #endregion

        #region [.public methods.]
        public DownloadRow Row { get; }
        public bool Success { get; private set; }
        public string OutputFileName
        {
            get => outputFileNameTextBox?.Text?.Trim();
            set
            {
                value = value?.Trim();
                if ( (outputFileNameTextBox != null) && (outputFileNameTextBox.Text?.Trim() != value) )
                {
                    //---outputFileNameTextBox.TextChanged -= outputFileNameTextBox_TextChanged;
                    _IsTurnOff__outputFileNameTextBox_TextChanged = true;
                    outputFileNameTextBox.Text = value;
                    _IsTurnOff__outputFileNameTextBox_TextChanged = false;
                    //---outputFileNameTextBox.TextChanged += outputFileNameTextBox_TextChanged;
                }
            }
        }
        #endregion
    }
}
