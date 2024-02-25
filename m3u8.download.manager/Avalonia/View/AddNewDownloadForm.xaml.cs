using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using m3u8.download.manager.controllers;
using m3u8.download.manager.infrastructure;
using m3u8.download.manager.models;

using _SC_ = m3u8.download.manager.controllers.SettingsPropertyChangeController;
using _AvaColor_   = Avalonia.Media.Color;
using _AvaBrushes_ = Avalonia.Media.Brushes;

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

        private TextBlock patternOutputFileNameLabelCaption;
        private TextBlock patternOutputFileNameLabel;
        private NumericUpDown patternOutputFileNameNumUpDn;

        private CheckBox isLiveStreamCheckBox;
        private TextBlock liveStreamMaxSizeInMbTextBlock;
        private NumericUpDown liveStreamMaxSizeInMbNumUpDn;

        private IDisposable m3u8FileUrlTextBox_SubscribeDisposable;
        private IDisposable outputFileNameTextBox_SubscribeDisposable;

        private Button startDownloadButton;
        private Button laterDownloadButton;

        private TabItem              mainTabItem;
        private TabItem              requestHeadersTabItem;
        private RequestHeadersEditor requestHeadersEditor;        
        #endregion

        #region [.fields.]
        private LogListModel _Model;
        private bool _DownloadLater;
        private _SC_ _SC;
        private DownloadListModel _DownloadListModel;
        private FileNameCleaner4UI.Processor _FNCP;
        private bool _WasFocusSet2outputFileNameTextBoxAfterFirstChanges;
        private (int n, int total) _SeriesInfo;
        private OutputFileNamePatternProcessor _OutputFileNamePatternProcessor;
        private bool _IsInEditMode;
        #endregion

        #region [.ctor().]
        public AddNewDownloadForm()
        {
            AvaloniaXamlLoader.Load( this );

            m3u8FileUrlTextBox     = this.Find< TextBox >( nameof(m3u8FileUrlTextBox) );
            outputFileNameTextBox  = this.Find< TextBox >( nameof(outputFileNameTextBox) );
            outputDirectoryTextBox = this.Find< TextBox >( nameof(outputDirectoryTextBox) );
            logUC                  = this.Find< RequestLogUC >( nameof(logUC) );
            mainTabItem            = this.Find< TabItem      >( nameof(mainTabItem) );
            requestHeadersTabItem  = this.Find< TabItem      >( nameof(requestHeadersTabItem) );
            requestHeadersEditor   = this.Find< RequestHeadersEditor >( nameof(requestHeadersEditor) ); 
            requestHeadersEditor.OnRequestHeadersCountChanged += requestHeadersEditor_OnRequestHeadersCountChanged;
            requestHeadersEditor.OnPageDown_N_PageUp          += () => OnKeyDown( new KeyEventArgs() { Key = Key.PageDown, KeyModifiers = KeyModifiers.Control } );

            patternOutputFileNameLabelCaption = this.Find< TextBlock     >( nameof(patternOutputFileNameLabelCaption) );
            patternOutputFileNameLabel        = this.Find< TextBlock     >( nameof(patternOutputFileNameLabel) );
            patternOutputFileNameNumUpDn      = this.Find< NumericUpDown >( nameof(patternOutputFileNameNumUpDn) ); patternOutputFileNameNumUpDn.ValueChanged += patternOutputFileNameNumUpDn_ValueChanged;

            isLiveStreamCheckBox           = this.Find< CheckBox      >( nameof(isLiveStreamCheckBox) );
            liveStreamMaxSizeInMbTextBlock = this.Find< TextBlock     >( nameof(liveStreamMaxSizeInMbTextBlock) );
            liveStreamMaxSizeInMbNumUpDn   = this.Find< NumericUpDown >( nameof(liveStreamMaxSizeInMbNumUpDn) );

            this.Find< Button >( "outputFileNameClearButton"   ).Click += outputFileNameClearButton_Click;
            this.Find< Button >( "outputFileNameSelectButton"  ).Click += outputFileNameSelectButton_Click;
            this.Find< Button >( "outputDirectorySelectButton" ).Click += outputDirectorySelectButton_Click;
            this.Find< Button >( "loadM3u8FileContentButton"   ).Click += loadM3u8FileContentButton_Click;
            startDownloadButton = this.Find< Button >( "startDownloadButton" ); startDownloadButton.Click += startDownloadButton_Click;
            laterDownloadButton = this.Find< Button >( "laterDownloadButton" ); laterDownloadButton.Click += laterDownloadButton_Click;
            isLiveStreamCheckBox.Click += isLiveStreamCheckBox_Click;

            _FNCP = new FileNameCleaner4UI.Processor( outputFileNameTextBox, () => this.OutputFileName, setOutputFileName );

            m3u8FileUrlTextBox_SubscribeDisposable    = m3u8FileUrlTextBox   .GetObservable( TextBox.TextProperty ).Subscribe( m3u8FileUrlTextBox_TextChanged );
            outputFileNameTextBox_SubscribeDisposable = outputFileNameTextBox.GetObservable( TextBox.TextProperty ).Subscribe( outputFileNameTextBox_TextChanged );
#if DEBUG
            this.AttachDevTools();
#endif
        }
        private AddNewDownloadForm( MainVM vm
            , DownloadRow row
            , OutputFileNamePatternProcessor outputFileNamePatternProcessor ) : this()
        {
            _IsInEditMode = true;
            this.DataContext = new AddNewDownloadFormVM( this );

            _SC                = vm.SettingsController;
            _DownloadListModel = vm.DownloadController?.Model;
            _OutputFileNamePatternProcessor = outputFileNamePatternProcessor;
            requestHeadersEditor.SetRequestHeaders( row.RequestHeaders );

            this.OutputFileName               = row.OutputFileName;
            this.OutputDirectory              = row.OutputDirectory;
            this.IsLiveStream                 = row.IsLiveStream;
            this.LiveStreamMaxFileSizeInBytes = row.LiveStreamMaxFileSizeInBytes;            

            #region [.if setted outputFileName.]
            //before 'this.M3u8FileUrl = m3u8FileUrl;'
            Process_use_OutputFileNamePatternProcessor_on_Init();
            #endregion

            _IsTurnOff__m3u8FileUrlTextBox_TextChanged = true;
            this.M3u8FileUrl = row.Url;
            _IsTurnOff__m3u8FileUrlTextBox_TextChanged = false;
            _WasFocusSet2outputFileNameTextBoxAfterFirstChanges = row.Url.IsNullOrWhiteSpace();

            _Model = new LogListModel();
            logUC.SetModel( _Model );
        }
        private AddNewDownloadForm( MainVM vm
            , string m3u8FileUrl
            , IDictionary< string, string > requestHeaders
            , OutputFileNamePatternProcessor outputFileNamePatternProcessor
            , in (int n, int total)? seriesInfo = null ) : this()
        {
            this.DataContext = new AddNewDownloadFormVM( this );

            _SC                = vm.SettingsController;
            _DownloadListModel = vm.DownloadController?.Model;
            _OutputFileNamePatternProcessor = outputFileNamePatternProcessor;
            requestHeadersEditor.SetRequestHeaders( requestHeaders );

            #region [.if setted outputFileName.]
            //before 'this.M3u8FileUrl = m3u8FileUrl;'
            Process_use_OutputFileNamePatternProcessor_on_Init();
            #endregion

            this.M3u8FileUrl = m3u8FileUrl;
            this.OutputDirectory = _SC.Settings.OutputFileDirectory;
            this.LiveStreamMaxFileSizeInMb = _SC.Settings.LiveStreamMaxSingleFileSizeInMb;
            //this.IsLiveStream              = _SC.Settings.IsLiveStream;
            _WasFocusSet2outputFileNameTextBoxAfterFirstChanges = m3u8FileUrl.IsNullOrWhiteSpace();

            _Model = new LogListModel();
            logUC.SetModel( _Model );

            #region [.seriesInfo.]
            if ( seriesInfo.HasValue )
            {
                var x = seriesInfo.Value;
                this.Title += $" ({x.n} of {x.total})";
            }
            _SeriesInfo = seriesInfo.GetValueOrDefault( (1, 1) );
            #endregion
        }
        public void Dispose()
        {
            _FNCP.Dispose_NoThrow();
            m3u8FileUrlTextBox_SubscribeDisposable.Dispose_NoThrow();
            outputFileNameTextBox_SubscribeDisposable.Dispose_NoThrow();
        }
        #endregion

        internal static AddNewDownloadForm Show( MainVM vm
            , string m3u8FileUrl
            , IDictionary< string, string > requestHeaders
            , OutputFileNamePatternProcessor outputFileNamePatternProcessor
            , in (int n, int total)? seriesInfo = null ) => new AddNewDownloadForm( vm, m3u8FileUrl, requestHeaders, outputFileNamePatternProcessor, seriesInfo );
        internal static AddNewDownloadForm Edit( MainVM vm
            , DownloadRow row
            , OutputFileNamePatternProcessor outputFileNamePatternProcessor )
        {
            var f = new AddNewDownloadForm( vm, row, outputFileNamePatternProcessor )
            {
                Icon  = new WindowIcon( ResourceLoader._GetResource_( "/Resources/edit.png" ) ),
                Title = $"Edit, / '{row.OutputFileName}' /",
            };
            f.startDownloadButton.Content = "Ok";
            f.laterDownloadButton.Content = "Cancel";
            f.Opened += (_, _) => f.setFocus2outputFileNameTextBox_Core();
            return (f);
        }

        #region [.override.]
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

            Dispose();

            if ( this.Success )
            {
                _SC.Settings.OutputFileDirectory             = this.OutputDirectory;
                _SC.Settings.LiveStreamMaxSingleFileSizeInMb = this.LiveStreamMaxFileSizeInMb;
                _SC.Settings.IsLiveStream                    = this.IsLiveStream;
                _SC.SaveNoThrow_IfAnyChanged();
            }
        }
        protected override void OnClosing( WindowClosingEventArgs e )
        {
            base.OnClosing( e );

            if ( this.IsWaitBannerShown() )
            {
                e.Cancel = true;
                return;
            }
        }
        protected async override void OnKeyDown( KeyEventArgs e )
        {
            switch ( e.Key )
            {
                case Key.Escape:
                    //if ( !requestHeadersTabItem.IsSelected )
                    //{
                        e.Handled = true;
                        this.Close();
                        return;
                    //}
                    //break;

                case Key.Enter: //StartDownload
                    var button = (this.GetTemplateFocusTarget() as Button);
                    if ( (button == null) || !button.IsFocused )
                    {
                        var downloadLater = ((e.KeyModifiers & KeyModifiers.Alt)     == KeyModifiers.Alt ||
                                             (e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control ||
                                             (e.KeyModifiers & KeyModifiers.Shift)   == KeyModifiers.Shift);
                        if ( await StartDownloadRoutine( downloadLater ) )
                        {
                            e.Handled = true;
                            return;
                        }
                    }
                    break;

                case Key.PageDown:
                case Key.PageUp:
                    if ( (e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control )
                    {
                        if ( mainTabItem.IsSelected ) requestHeadersTabItem.IsSelected = true;
                        else mainTabItem.IsSelected = true;
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
        private bool _IsTurnOff__outputFileNameTextBox_TextChanged;
        private bool _IsTurnOff__m3u8FileUrlTextBox_TextChanged;

        private void setFocus2outputFileNameTextBox_Core( string outputFileName = null )
        {
            outputFileNameTextBox.Focus();
            var i = (outputFileName ?? outputFileNameTextBox.Text ?? string.Empty).LastIndexOf( '.' );
            if ( i != -1 )
            {
                outputFileNameTextBox.SelectionStart = outputFileNameTextBox.SelectionEnd = i;
            }
        }
        private void setFocus2outputFileNameTextBox()
        {
            if ( outputFileNameTextBox != null )
            {
                if ( !_WasFocusSet2outputFileNameTextBoxAfterFirstChanges )
                {
                    setFocus2outputFileNameTextBox_Core(); //outputFileNameTextBox.Focus();
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
            //var _ = await this.StorageProvider.SaveFilePickerAsync( new Avalonia.Platform.Storage.FilePickerSaveOptions()
            //{
            //    SuggestedFileName      = FileNameCleaner4UI.GetOutputFileName( this.OutputFileName ),
            //    SuggestedStartLocation = this.OutputDirectory,
            //    //Directory        = this.OutputDirectory,
            //    DefaultExtension = _SC.Settings.OutputFileExtension,
            //    //InitialFileName  = FileNameCleaner4UI.GetOutputFileName( this.OutputFileName )
            //});

            var sfd = new SaveFileDialog() { Directory = this.OutputDirectory,
                DefaultExtension = _SC.Settings.OutputFileExtension,
                InitialFileName = FileNameCleaner4UI.GetOutputFileName( this.OutputFileName ),
                /*AddExtension     = true,*/
            };
            {
                var fileName = await sfd.ShowAsync( this );
                if ( !fileName.IsNullOrWhiteSpace() )
                {
                    var outputFullFileName = fileName;
                    this.OutputFileName = Path.GetFileName( outputFullFileName );
                    this.OutputDirectory = Path.GetDirectoryName( outputFullFileName );
                }
            }
        }
        private async void outputDirectorySelectButton_Click( object sender, RoutedEventArgs e )
        {
            var d = new OpenFolderDialog() { Directory = this.OutputDirectory, Title = "Select Output directory" };
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
            if ( _IsTurnOff__m3u8FileUrlTextBox_TextChanged ) return;

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

            await FileNameCleaner4UI.SetOutputFileNameByUrl_Async( m3u8FileUrlText, setOutputFileName, TEXTBOX_MILLISECONDS_DELAY );

            setFocus2outputFileNameTextBox();
        }

        private void setOutputFileName( string outputFileName ) => this.OutputFileName = outputFileName;
        private void outputFileNameTextBox_TextChanged( string value )
        {
            if ( _IsTurnOff__outputFileNameTextBox_TextChanged ) return;
            if ( _OutputFileNamePatternProcessor == null ) return; //then call from '.ctor()'

            Process_use_OutputFileNamePatternProcessor();
        }
        #endregion

        #region [.Start Download.]
        private bool IsWaitBannerShown() => !this.IsEnabled;

        private async Task< bool > IsValid()
        {
            if ( this.M3u8FileUrl.IsNullOrWhiteSpace() )
            {
                m3u8FileUrlTextBox.FocusAndBlinkBackColor();
                return (false);
            }
            else
            if ( !_IsInEditMode && _SC.Settings.UniqueUrlsOnly && (_DownloadListModel?.ContainsUrl( this.M3u8FileUrl )).GetValueOrDefault() )
            {
                await Extensions.MessageBox_ShowError( $"Url already exists in list:\n '{this.M3u8FileUrl}'\n", this.Title );
                m3u8FileUrlTextBox.FocusAndBlinkBackColor();
                return (false);
            }
            else
            if ( this.GetOutputFileName_Internal().IsNullOrWhiteSpace() )
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
                var t = await DownloadController.GetFileTextContent( x.m3u8FileUrl, this.GetRequestHeaders(), _SC.Settings.RequestTimeoutByPart, cts ); //all possible exceptions are thrown within inside
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

        private void isLiveStreamCheckBox_Click( object sender, RoutedEventArgs e )
        {
            var isLiveStream = this.IsLiveStream;

            isLiveStreamCheckBox.Foreground = isLiveStream ? new SolidColorBrush( _AvaColor_.FromArgb( 255, 70, 70, 70 ) ) : _AvaBrushes_.Silver; //isLiveStream ? Color.FromArgb( 70, 70, 70 ) : Color.Silver;
            liveStreamMaxSizeInMbNumUpDn.IsVisible = liveStreamMaxSizeInMbTextBlock.IsVisible = isLiveStream;
            //this.mainLayoutPanel.Height = isLiveStream ? 90 : 60;
        }

        private void Process_use_OutputFileNamePatternProcessor_on_Init()
        {
            if ( _OutputFileNamePatternProcessor.TryGet_Patterned_Last_OutputFileName( out var t ) )
            {
                m3u8FileUrlTextBox_SubscribeDisposable.Dispose(); m3u8FileUrlTextBox_SubscribeDisposable = null;
                this.OutputFileName = t.Patterned_Last_OutputFileName;
                patternOutputFileNameLabel.Text = t.Last_OutputFileName_As_Pattern; patternOutputFileNameLabel.SetValue( ToolTip.TipProperty, t.Last_OutputFileName_As_Pattern );
                patternOutputFileNameNumUpDn.Value = t.Last_OutputFileName_Num;
                patternOutputFileNameLabelCaption.IsVisible = patternOutputFileNameLabel.IsVisible = patternOutputFileNameNumUpDn.IsVisible = true;

                if ( !this.M3u8FileUrl.IsNullOrWhiteSpace() )
                {
                    this.Opened += (_, _) => setFocus2outputFileNameTextBox_Core( t.Patterned_Last_OutputFileName );
                }
            }

            //TEMP
#if DEBUG
            //else //if ( !this.M3u8FileUrl.IsNullOrWhiteSpace() )
            //{
            //    m3u8FileUrlTextBox_SubscribeDisposable.Dispose(); m3u8FileUrlTextBox_SubscribeDisposable = null;
            //    this.Opened += (_, _) =>
            //    {
            //        var txt = "Last_OutputFileName_Num - Last_OutputFileName_Num - Last_OutputFileName_Num - **.txt";
            //        this.OutputFileName = txt;
            //        setFocus2outputFileNameTextBox_Core( txt );
            //        outputFileNameTextBox_TextChanged( txt );
            //    };
            //}
#endif
        }
        private void Process_use_OutputFileNamePatternProcessor()
        {
            var outputFileName = this.OutputFileName;
            var has = _OutputFileNamePatternProcessor.HasPatternChar( outputFileName );
            if ( has )
            {
                patternOutputFileNameLabel.Text = outputFileName; patternOutputFileNameLabel.SetValue( ToolTip.TipProperty, outputFileName );
                patternOutputFileNameNumUpDn.Value = _OutputFileNamePatternProcessor.IsEqualPattern( outputFileName ) ? _OutputFileNamePatternProcessor.Last_OutputFileName_Num : 1;
                patternOutputFileNameNumUpDn_ValueChanged( null, null );
            }
            patternOutputFileNameLabelCaption.IsVisible = patternOutputFileNameLabel.IsVisible = patternOutputFileNameNumUpDn.IsVisible = has;
        }
        private void patternOutputFileNameNumUpDn_ValueChanged( object sender, RoutedEventArgs e )
        {
            _OutputFileNamePatternProcessor.Set_Last_OutputFileName_Num( (int) patternOutputFileNameNumUpDn.Value );

            if ( _OutputFileNamePatternProcessor.HasLast_OutputFileName_As_Pattern )
            {
                this.OutputFileName = _OutputFileNamePatternProcessor.Get_Patterned_Last_OutputFileName();
            }
        }

        private void requestHeadersEditor_OnRequestHeadersCountChanged( int requestHeadersCount, int enabledCount )
            => requestHeadersTabItem.Header = (requestHeadersCount == enabledCount) ? $"request headers ({requestHeadersCount})" : $"request headers ({enabledCount} of {requestHeadersCount})";
        #endregion

        #region [.public methods.]
        public bool Success { get; private set; }
        public bool AutoStartDownload => !_DownloadLater;
        public string M3u8FileUrl
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
        //public  string GetOutputFileName( char? skipChar = null ) => FileNameCleaner4UI.GetOutputFileName( this.OutputFileName, skipChar );
        public string GetOutputFileName()
        {
            var outputFileName_1 = GetOutputFileName_Internal();
            var outputFileName_2 = _OutputFileNamePatternProcessor.Process( outputFileName_1 );
            return (outputFileName_2);
        }       
        public string GetOutputDirectory() => this.OutputDirectory;
        public IDictionary< string, string > GetRequestHeaders() => requestHeadersEditor.GetRequestHeaders();
        public  bool   IsLiveStream
        { 
            get => isLiveStreamCheckBox.IsChecked.GetValueOrDefault();
            set => isLiveStreamCheckBox.IsChecked = value;
        }
        public int     LiveStreamMaxFileSizeInMb
        {
            get => (int) liveStreamMaxSizeInMbNumUpDn.Value;
            set => liveStreamMaxSizeInMbNumUpDn.Value = Math.Max( liveStreamMaxSizeInMbNumUpDn.Minimum, Math.Min( liveStreamMaxSizeInMbNumUpDn.Maximum, value ) );
        }
        public long    LiveStreamMaxFileSizeInBytes
        {
            get => this.LiveStreamMaxFileSizeInMb << 20;
            set => this.LiveStreamMaxFileSizeInMb = (int) (value >> 20);
        }

        private string GetOutputFileName_Internal() => FileNameCleaner4UI.GetOutputFileName( this.OutputFileName, _OutputFileNamePatternProcessor.PatternChar );
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
