using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using m3u8.download.manager.controllers;
using m3u8.download.manager.infrastructure;
using m3u8.download.manager.models;
using m3u8.download.manager.Properties;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class AddNewDownloadForm : Window, IDisposable
    {
        #region [.markup fields.]
        private TextBox m3u8FileUrlTextBox;
        private TextBox outputFileNameTextBox;
        private TextBox outputDirectoryTextBox;
        private RequestLogUC logUC;

        private IDisposable m3u8FileUrlTextBox_SubscribeDisposable;
        private IDisposable outputFileNameTextBox_SubscribeDisposable;
        #endregion

        #region [.fields.]
        private LogListModel      _Model;
        private bool              _DownloadLater;
        private Settings          _Settings;
        private DownloadListModel _DownloadListModel;
        private FileNameCleaner.Processor _FNCP;
        private bool _WasFocusSet2outputFileNameTextBoxAfterFirstChanges;
        private (int n, int total) _SeriesInfo;
        #endregion

        #region [.ctor().]
        public AddNewDownloadForm()
        {
            AvaloniaXamlLoader.Load( this );
            m3u8FileUrlTextBox     = this.Find< TextBox >( nameof(m3u8FileUrlTextBox)     ); 
            outputFileNameTextBox  = this.Find< TextBox >( nameof(outputFileNameTextBox)  ); 
            outputDirectoryTextBox = this.Find< TextBox >( nameof(outputDirectoryTextBox) );
            logUC                  = this.Find< RequestLogUC >( nameof(logUC) );

            this.Find< Button >( "outputFileNameClearButton"   ).Click += outputFileNameClearButton_Click;
            this.Find< Button >( "outputFileNameSelectButton"  ).Click += outputFileNameSelectButton_Click;
            this.Find< Button >( "outputDirectorySelectButton" ).Click += outputDirectorySelectButton_Click;
            this.Find< Button >( "startDownloadButton"         ).Click += startDownloadButton_Click;
            this.Find< Button >( "laterDownloadButton"         ).Click += laterDownloadButton_Click;
            this.Find< Button >( "loadM3u8FileContentButton"   ).Click += loadM3u8FileContentButton_Click;


            _FNCP = new FileNameCleaner.Processor( outputFileNameTextBox, () => this.OutputFileName, setOutputFileName );

            m3u8FileUrlTextBox_SubscribeDisposable    = m3u8FileUrlTextBox   .GetObservable( TextBox.TextProperty ).Subscribe( m3u8FileUrlTextBox_TextChanged    );
            outputFileNameTextBox_SubscribeDisposable = outputFileNameTextBox.GetObservable( TextBox.TextProperty ).Subscribe( outputFileNameTextBox_TextChanged );
#if DEBUG
            this.AttachDevTools();
#endif
        }
        internal AddNewDownloadForm( MainVM vm, string m3u8FileUrl, (int n, int total)? seriesInfo = null ) : this()
        {
            this.DataContext = new AddNewDownloadFormVM( this );

            _Settings          = vm.SettingsController.Settings;
            _DownloadListModel = vm.DownloadController?.Model;
            
            this.M3u8FileUrl     = m3u8FileUrl;
            this.OutputDirectory = _Settings.OutputFileDirectory;
            _WasFocusSet2outputFileNameTextBoxAfterFirstChanges = m3u8FileUrl.IsNullOrWhiteSpace();

            _Model = new LogListModel();
            logUC.SetModel( _Model );
            
            if ( seriesInfo.HasValue )
            {
                var x = seriesInfo.Value;
                this.Title += $" ({x.n} of {x.total})";
            }
            _SeriesInfo = seriesInfo.GetValueOrDefault( (1, 1) );
        }
        public void Dispose()
        {
            _FNCP.Dispose_NoThrow();
            m3u8FileUrlTextBox_SubscribeDisposable.Dispose_NoThrow();
            outputFileNameTextBox_SubscribeDisposable.Dispose_NoThrow();
        }

        protected async override void OnOpened( EventArgs e )
        {
            base.OnOpened( e );

            if ( this.M3u8FileUrl.IsNullOrEmpty() )
            {
                m3u8FileUrlTextBox.Focus();
            }
            else
            {
                outputFileNameTextBox.Focus();
            }

            //STRANGE BUG (?) (IN AVALONIA) AFTER UPGRADE FROM v0.9.9 TO v0.9.10
            this.Width++;
            await Task.Delay( 1 );
            this.Width--;
        }
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            m3u8FileUrlTextBox_SubscribeDisposable   .Dispose_NoThrow();
            outputFileNameTextBox_SubscribeDisposable.Dispose_NoThrow();
            _FNCP.Dispose_NoThrow();

            if ( this.Success )
            {
                _Settings.OutputFileDirectory = this.OutputDirectory;
                _Settings.SaveNoThrow();
            }
        }
        protected async override void OnKeyDown( KeyEventArgs e )
        {
            switch ( e.Key )
            {
                case Key.Escape:
                    e.Handled = true;
                    this.Close(); 
                return;

                case Key.Enter: //StartDownload
                    var button = (this.GetTemplateFocusTarget() as Button);
                    if ( (button == null) || !button.IsFocused )
                    {
                        var downloadLater = ( (e.KeyModifiers & KeyModifiers.Alt    ) == KeyModifiers.Alt     ||
                                              (e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control ||
                                              (e.KeyModifiers & KeyModifiers.Shift  ) == KeyModifiers.Shift);
                        if ( await StartDownloadRoutine( downloadLater ) )
                        {
                            e.Handled = true;
                            return;
                        }
                    }
                break;
            }

            base.OnKeyDown( e );
        }
        #endregion

        #region [.text-boxes.]
        private const int TEXTBOX_MILLISECONDS_DELAY = 150;
        private string _Last_m3u8FileUrlText;
        private string _LastManualInputed_outputFileNameText;
        private bool   _IsTurnOff__outputFileNameTextBox_TextChanged;

        private void setFocus2outputFileNameTextBox()
        {            
            if ( outputFileNameTextBox != null )
            {
                if ( !_WasFocusSet2outputFileNameTextBoxAfterFirstChanges )
                {
                    outputFileNameTextBox.Focus();
                    _WasFocusSet2outputFileNameTextBoxAfterFirstChanges = outputFileNameTextBox.IsFocused;
                }
            }
        }

        private void outputFileNameClearButton_Click( object sender, RoutedEventArgs e )
        {
            this.OutputFileName = null;
            outputFileNameTextBox.Focus();
        }
        private async void outputFileNameSelectButton_Click( object sender, RoutedEventArgs e )
        {
            var sfd = new SaveFileDialog() { Directory        = this.OutputDirectory,
                                             DefaultExtension = _Settings.OutputFileExtension,
                                             InitialFileName  = FileNameCleaner.GetOutputFileName( this.OutputFileName ),
                                             /*AddExtension     = true,*/ };
            {
                var fileName = await sfd.ShowAsync( this );
                if ( !fileName.IsNullOrWhiteSpace() )
                {
                    var outputFullFileName = fileName;
                    this.OutputFileName  = Path.GetFileName( outputFullFileName );
                    this.OutputDirectory = Path.GetDirectoryName( outputFullFileName );
                }
            }
        }
        private async void outputDirectorySelectButton_Click( object sender, RoutedEventArgs e )
        {
            var d = new OpenFolderDialog() { Directory = this.OutputDirectory,
                                             Title     = "Select Output directory" };
            {
                var directory = await d.ShowAsync( this );
                if ( !directory.IsNullOrWhiteSpace() )
                {
                    this.OutputDirectory = directory;
                }
            }
        }
        private async void m3u8FileUrlTextBox_TextChanged( string value )
        {
            var m3u8FileUrlText = this.M3u8FileUrl;
            if ( (_Last_m3u8FileUrlText == m3u8FileUrlText) && !this.OutputFileName.IsNullOrWhiteSpace() )
            {
                return;
            }
            if ( !_LastManualInputed_outputFileNameText.IsNullOrWhiteSpace() )
            {
                return;
            }
            _Last_m3u8FileUrlText = m3u8FileUrlText;

            await FileNameCleaner.SetOutputFileNameByUrl_Async( m3u8FileUrlText, setOutputFileName, TEXTBOX_MILLISECONDS_DELAY );

            setFocus2outputFileNameTextBox();
        }

        private void setOutputFileName( string outputFileName ) => this.OutputFileName = outputFileName;
        private void outputFileNameTextBox_TextChanged( string value )
        {
            if ( !_IsTurnOff__outputFileNameTextBox_TextChanged )
            {
                _FNCP.FileNameTextBox_TextChanged( outputFileName => _LastManualInputed_outputFileNameText = outputFileName );
            }
        }
        #endregion

        #region [.Start Download.]
        private async Task< bool > IsValid()
        {
            if ( this.M3u8FileUrl.IsNullOrWhiteSpace() )
            {
                m3u8FileUrlTextBox.FocusAndBlinkBackColor();
                return (false);
            }
            else
            if ( _Settings.UniqueUrlsOnly && ( _DownloadListModel?.ContainsUrl( this.M3u8FileUrl )).GetValueOrDefault() )
            {
                await Extensions.MessageBox_ShowError( $"Url already exists in list:\n '{this.M3u8FileUrl}'\n", this.Title );
                m3u8FileUrlTextBox.FocusAndBlinkBackColor();
                return (false);
            }
            else
            if ( this.GetOutputFileName().IsNullOrWhiteSpace() )
            {
                outputFileNameTextBox.FocusAndBlinkBackColor();
                return (false);
            }
            else
            if ( this.GetOutputDirectory().IsNullOrWhiteSpace() )
            {
                outputDirectoryTextBox.FocusAndBlinkBackColor();
                return (false);
            }

            return (true);
        }
        private async Task< bool > StartDownloadRoutine( bool downloadLater )
        {
            if ( await IsValid() )
            {
                _DownloadLater = downloadLater;
                this.Success = true;
                this.Close();
                return (true);
            }
            return (false);
        }
        private async void startDownloadButton_Click( object sender, RoutedEventArgs e ) => await StartDownloadRoutine( downloadLater: false );
        private async void laterDownloadButton_Click( object sender, RoutedEventArgs e ) => await StartDownloadRoutine( downloadLater: true );
        private async void loadM3u8FileContentButton_Click( object sender, RoutedEventArgs e )
        {
            //logPanel.Visible = true;

            _Model.Clear();
            #region [.url.]
            if ( !Extensions.TryGetM3u8FileUrl( this.M3u8FileUrl, out var x ) )
            {
                _Model.AddRequestErrorRow( x.error.ToString() );
                logUC.ClearSelection();
                return;
            }
            #endregion

            this.IsEnabled = false;
            await Task.Delay( millisecondsDelay: 250 );

            using ( var cts = new CancellationTokenSource() )
            using ( WaitBannerForm.CreateAndShow( this, cts, visibleDelayInMilliseconds: 1_500 ) )
            {
//await Task.Delay( 10_000 );
                var t = await DownloadController.GetFileTextContent( x.m3u8FileUrl, cts ); //all possible exceptions are thrown within inside
                if ( cts.IsCancellationRequested )
                {
                    ;
                }
                else if ( t.error != null )
                {
                    _Model.AddRequestErrorRow( t.error.ToString() );
                }
                else
                {
                    _Model.Output( in t.m3u8File );
                }
                await logUC.ScrollToLastRow();
            }
            this.IsEnabled = true;
        }
        #endregion

        #region [.public methods.]
        public bool   Success { get; private set; }
        public bool   AutoStartDownload => !_DownloadLater;
        public  string M3u8FileUrl
        {
            get => m3u8FileUrlTextBox.Text?.Trim();
            private set
            {
                value = value?.Trim();
                if ( m3u8FileUrlTextBox.Text?.Trim() != value )
                {
                    m3u8FileUrlTextBox.Text = value;
                }
            }
        }
        public  string GetOutputFileName() => FileNameCleaner.GetOutputFileName( this.OutputFileName );
        public  string GetOutputDirectory() => this.OutputDirectory;

        private string OutputFileName
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
        private string OutputDirectory
        {
            get => outputDirectoryTextBox?.Text?.Trim();
            set
            {
                value = value?.Trim();
                if ( (outputDirectoryTextBox != null) && outputDirectoryTextBox.Text?.Trim() != value )
                {
                    outputDirectoryTextBox.Text = value;
                }
            }
        }
        #endregion
    }
}
