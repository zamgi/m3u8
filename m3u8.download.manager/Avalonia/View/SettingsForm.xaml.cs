using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;

using m3u8.download.manager.controllers;
using _Timer_ = System.Timers.Timer;
using SETTINGS = m3u8.download.manager.controllers.SettingsPropertyChangeController;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SettingsForm : Window
    {
        /// <summary>
        /// 
        /// </summary>
        public enum SettingsTabEnum
        {
            Parallelism,
            Other,
            //More,
        }

        #region [.parallelism.]
        #region [.markup fields.]
        private NumericUpDown maxDegreeOfParallelismNUD;
        private CheckBox      useCrossDownloadInstanceParallelismCheckBox;
        private CheckBox      useMaxCrossDownloadInstanceCheckBox;
        private TextBlock     maxCrossDownloadInstanceLabel1;
        private TextBlock     maxCrossDownloadInstanceLabel2;
        private NumericUpDown maxCrossDownloadInstanceNUD;
        private CheckBox      isUnlimMaxSpeedThresholdCheckBox;
        private TextBlock     maxSpeedThresholdLabel;
        private NumericUpDown maxSpeedThresholdNUD;
        #endregion

        #region [.private methods.]
        private void InitializeComponent_4_parallelism()
        {
            maxDegreeOfParallelismNUD                   = this.Find< NumericUpDown >( nameof(maxDegreeOfParallelismNUD) );
            useCrossDownloadInstanceParallelismCheckBox = this.Find< CheckBox      >( nameof(useCrossDownloadInstanceParallelismCheckBox) );
            useMaxCrossDownloadInstanceCheckBox         = this.Find< CheckBox      >( nameof(useMaxCrossDownloadInstanceCheckBox) );
            maxCrossDownloadInstanceLabel1              = this.Find< TextBlock     >( nameof(maxCrossDownloadInstanceLabel1) );
            maxCrossDownloadInstanceLabel2              = this.Find< TextBlock     >( nameof(maxCrossDownloadInstanceLabel2) );
            maxCrossDownloadInstanceNUD                 = this.Find< NumericUpDown >( nameof(maxCrossDownloadInstanceNUD) );
            isUnlimMaxSpeedThresholdCheckBox            = this.Find< CheckBox      >( nameof(isUnlimMaxSpeedThresholdCheckBox) );
            maxSpeedThresholdLabel                      = this.Find< TextBlock     >( nameof(maxSpeedThresholdLabel) );
            maxSpeedThresholdNUD                        = this.Find< NumericUpDown >( nameof(maxSpeedThresholdNUD) );

            useMaxCrossDownloadInstanceCheckBox.Click += useMaxCrossDownloadInstanceCheckBox_Click;
            isUnlimMaxSpeedThresholdCheckBox   .Click += isUnlimMaxSpeedThresholdCheckBox_Click;
        }
        #endregion

        #region [.public methods.]
        public int MaxDegreeOfParallelism
        {
            get => Math.Min( SETTINGS.MAX_DEGREE_OF_PARALLELISM, Convert.ToInt32( maxDegreeOfParallelismNUD.Value ) );
            set => maxDegreeOfParallelismNUD.Value = Math.Min( SETTINGS.MAX_DEGREE_OF_PARALLELISM, value );
        }
        public bool UseCrossDownloadInstanceParallelism
        {
            get => useCrossDownloadInstanceParallelismCheckBox.IsChecked.GetValueOrDefault();
            set => useCrossDownloadInstanceParallelismCheckBox.IsChecked = value;
        }

        public int? MaxCrossDownloadInstance => (useMaxCrossDownloadInstanceCheckBox.IsChecked.GetValueOrDefault() ? Convert.ToInt32( maxCrossDownloadInstanceNUD.Value ) : ((int?) null));
        public int MaxCrossDownloadInstanceSaved => Convert.ToInt32( maxCrossDownloadInstanceNUD.Value );
        public void SetMaxCrossDownloadInstance( int? maxCrossDownloadInstance, int maxCrossDownloadInstanceSaved )
        {
            maxCrossDownloadInstanceNUD.Value = maxCrossDownloadInstance.GetValueOrDefault( maxCrossDownloadInstanceSaved );
            useMaxCrossDownloadInstanceCheckBox.IsChecked = maxCrossDownloadInstance.HasValue;
            useMaxCrossDownloadInstanceCheckBox_Click( null, null ); // useMaxCrossDownloadInstanceCheckBox, new RoutedEventArgs() );
        }

        public decimal? MaxSpeedThresholdInMbps => (!isUnlimMaxSpeedThresholdCheckBox.IsChecked.GetValueOrDefault() ? MaxSpeedThresholdInMbpsSaved : null);
        public decimal MaxSpeedThresholdInMbpsSaved => Math.Max( 0.01M, maxSpeedThresholdNUD.Value.GetValueOrDefault() );
        public void SetMaxSpeedThresholdInMbps( decimal? maxSpeedThresholdInMbps, decimal maxSpeedThresholdInMbpsSaved )
        {
            maxSpeedThresholdNUD.Value = Math.Max( 0.01M, maxSpeedThresholdInMbps.GetValueOrDefault( maxSpeedThresholdInMbpsSaved ) );
            isUnlimMaxSpeedThresholdCheckBox.IsChecked = !maxSpeedThresholdInMbps.HasValue;
            isUnlimMaxSpeedThresholdCheckBox_Click( null, null );
        }
        #endregion

        #region [.private methods.]
        private void DownloadController_IsDownloadingChanged_4_parallelism( bool isDownloading )
        {
            useMaxCrossDownloadInstanceCheckBox.IsEnabled =
                useCrossDownloadInstanceParallelismCheckBox.IsEnabled = !isDownloading;
        }
        private void useMaxCrossDownloadInstanceCheckBox_Click( object sender, RoutedEventArgs e )
        {
            var isChecked = useMaxCrossDownloadInstanceCheckBox.IsChecked.GetValueOrDefault();

            maxCrossDownloadInstanceLabel1.Foreground =
                maxCrossDownloadInstanceLabel2.Foreground = (isChecked ? this.Foreground : Brushes.DarkGray);

            maxCrossDownloadInstanceNUD.IsEnabled = isChecked;
        }
        private void isUnlimMaxSpeedThresholdCheckBox_Click( object sender, RoutedEventArgs e )
        {
            var isChecked = isUnlimMaxSpeedThresholdCheckBox.IsChecked.GetValueOrDefault();

            maxSpeedThresholdLabel.Foreground = (!isChecked ? this.Foreground : Brushes.DarkGray);
            maxSpeedThresholdNUD.IsEnabled = !isChecked;
        }
        #endregion
        #endregion

        #region [.other.]
        #region [.markup fields.]
        private NumericUpDown attemptRequestCountByPartNUD;
        private CheckBox      uniqueUrlsOnlyCheckBox;
        private TextBlock     only4NotRunLabel1;
        private TextBlock     only4NotRunLabel2;
        //private DatePicker    requestTimeoutByPartDTP;
        private TimePickerUC  requestTimeoutByPartDTP;
        private TextBox       outputFileExtensionTextBox;
        private CheckBox      showOnlyRequestRowsWithErrorsCheckBox;
        private CheckBox      showDownloadStatisticsInMainFormTitleCheckBox;
        private CheckBox      showAllDownloadsCompleted_NotificationCheckBox;
        private TextBlock     currentMemoryTextBlock;
        #endregion

        #region [.fields.]
        private _Timer_ _GetTotalMemoryTimer;
        #endregion

        #region [.private methods.]
        private void InitializeComponent_4_other()
        {
            attemptRequestCountByPartNUD                   = this.Find< NumericUpDown >( nameof(attemptRequestCountByPartNUD) );
            uniqueUrlsOnlyCheckBox                         = this.Find< CheckBox      >( nameof(uniqueUrlsOnlyCheckBox) );
            only4NotRunLabel1                              = this.Find< TextBlock     >( nameof(only4NotRunLabel1) );
            only4NotRunLabel2                              = this.Find< TextBlock     >( nameof(only4NotRunLabel2) );
            requestTimeoutByPartDTP                        = this.Find< TimePickerUC  >( nameof(requestTimeoutByPartDTP) );
            //requestTimeoutByPartDTP                        = this.Find< DatePicker    >( nameof(requestTimeoutByPartDTP) );
            outputFileExtensionTextBox                     = this.Find< TextBox       >( nameof(outputFileExtensionTextBox) );
            showOnlyRequestRowsWithErrorsCheckBox          = this.Find< CheckBox      >( nameof(showOnlyRequestRowsWithErrorsCheckBox) );
            showDownloadStatisticsInMainFormTitleCheckBox  = this.Find< CheckBox      >( nameof(showDownloadStatisticsInMainFormTitleCheckBox) );
            showAllDownloadsCompleted_NotificationCheckBox = this.Find< CheckBox      >( nameof(showAllDownloadsCompleted_NotificationCheckBox) );
            currentMemoryTextBlock                         = this.Find< TextBlock     >( nameof(currentMemoryTextBlock) );

            this.Find< Button >( "collectGarbageButton" ).Click += collectGarbageButton_Ckick;
        }

        private bool IsValid_4_other()
        {
            if ( this.OutputFileExtension.IsNullOrEmpty() )
            {
                outputFileExtensionTextBox.FocusAndBlinkBackColor();
                return (false);
            }
            if ( requestTimeoutByPartDTP.Value <= TimeSpan.Zero )
            {
                requestTimeoutByPartDTP.FocusAndBlinkBackColor();
                return (false);
            }
            return (true);
        }

        private static string CorrectOutputFileExtension( string ext )
        {
            ext = ext?.Trim();
            if ( !ext.IsNullOrEmpty() && ext.HasFirstCharNotDot() )
            {
                ext = '.' + ext;
            }
            return (ext);
        }

        private void DownloadController_IsDownloadingChanged_4_other( bool isDownloading )
        {
            only4NotRunLabel1.IsVisible =
                only4NotRunLabel2.IsVisible = isDownloading;
        }

        private void getTotalMemoryTimer_Elapsed()
        {
            CollectGarbageCommand.Collect_Garbage( out var totalMemoryBytes );
            currentMemoryTextBlock.Text = $"Current Memory: {GetTotalMemoryFormatText( totalMemoryBytes )}.";
        }
        private static string GetTotalMemoryFormatText( long totalMemoryBytes ) => $"{(totalMemoryBytes / (1024.0 * 1024)):N2} MB";
        private /*async*/ void collectGarbageButton_Ckick( object s, RoutedEventArgs e )
        {
            var btn = (Button) s;
            btn.Content = "...";
            btn.IsEnabled = false;

            CollectGarbageCommand.Collect_Garbage( out var totalMemoryBytes );

            var text        = GetTotalMemoryFormatText( totalMemoryBytes );
            var toolTipText = $"Collect Garbage. Total Memory: {text}.";
           
            btn.Content   = text;
            btn.IsEnabled = true;
            btn.SetValue( ToolTip.TipProperty, toolTipText );
        }
        #endregion

        #region [.public methods.]
        public int      AttemptRequestCountByPart
        {
            get => Convert.ToInt32( attemptRequestCountByPartNUD.Value );
            set => attemptRequestCountByPartNUD.Value = value;
        }
        public TimeSpan RequestTimeoutByPart
        {
            //get => requestTimeoutByPartDTP.Value.TimeOfDay; // TimeSpan.FromTicks( (requestTimeoutByPartDTP.Value.TimeOfDay - requestTimeoutByPartDTP.MinDate.Date).Ticks );
            //set => requestTimeoutByPartDTP.Value = requestTimeoutByPartDTP.MinDate.Date + value;

            get => requestTimeoutByPartDTP.Value; // requestTimeoutByPartDTP.DisplayDate.TimeOfDay;
            set => requestTimeoutByPartDTP.Value = value;// requestTimeoutByPartDTP.DisplayDate = requestTimeoutByPartDTP.DisplayDate.Date + value;

            //TimeSpan.TryParse( requestTimeoutByPartDTP.Text, out var ts ) ? ts : (TimeSpan?) null
        }
        public bool     ShowOnlyRequestRowsWithErrors
        {
            get => showOnlyRequestRowsWithErrorsCheckBox.IsChecked.GetValueOrDefault();
            set => showOnlyRequestRowsWithErrorsCheckBox.IsChecked = value;
        }
        public bool     ShowDownloadStatisticsInMainFormTitle
        {
            get => showDownloadStatisticsInMainFormTitleCheckBox.IsChecked.GetValueOrDefault();
            set => showDownloadStatisticsInMainFormTitleCheckBox.IsChecked = value;
        }
        public bool     ShowAllDownloadsCompleted_Notification
        {
            get => showAllDownloadsCompleted_NotificationCheckBox.IsChecked.GetValueOrDefault();
            set => showAllDownloadsCompleted_NotificationCheckBox.IsChecked = value;
        }
        public bool     UniqueUrlsOnly
        {
            get => uniqueUrlsOnlyCheckBox.IsChecked.GetValueOrDefault();
            set => uniqueUrlsOnlyCheckBox.IsChecked = value;
        }
        public string   OutputFileExtension
        {
            get => CorrectOutputFileExtension( outputFileExtensionTextBox.Text );
            set => outputFileExtensionTextBox.Text = CorrectOutputFileExtension( value );
        }
        #endregion
        #endregion

        #region [.fields.]
        private DownloadController _DownloadController;
        #endregion

        #region [.ctor().]
        public SettingsForm()
        {
            AvaloniaXamlLoader.Load( this );
            this.InitializeComponent_4_parallelism();
            this.InitializeComponent_4_other();
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        internal SettingsForm( MainVM vm, SettingsTabEnum? settingsTab = default ) : this()
        {
            _DownloadController = vm?.DownloadController ?? throw (new ArgumentNullException( nameof(vm.DownloadController) ));

            #region [.parallelism.]
            _DownloadController.IsDownloadingChanged -= DownloadController_IsDownloadingChanged_4_parallelism;
            _DownloadController.IsDownloadingChanged += DownloadController_IsDownloadingChanged_4_parallelism;

            DownloadController_IsDownloadingChanged_4_parallelism( _DownloadController.IsDownloading );
            #endregion

            #region [.other.]
            _DownloadController.IsDownloadingChanged -= DownloadController_IsDownloadingChanged_4_other;
            _DownloadController.IsDownloadingChanged += DownloadController_IsDownloadingChanged_4_other;

            DownloadController_IsDownloadingChanged_4_other( _DownloadController.IsDownloading );

            var getTotalMemoryTimer_Elapsed_Action = new Action( getTotalMemoryTimer_Elapsed );
            _GetTotalMemoryTimer = new _Timer_() { Interval = 1_000, AutoReset = true, Enabled = true };
            _GetTotalMemoryTimer.Elapsed += async (_, _) => await Dispatcher.UIThread.InvokeAsync( getTotalMemoryTimer_Elapsed_Action );
            getTotalMemoryTimer_Elapsed();
            #endregion

            if ( settingsTab.HasValue )
            {
                var parallelismTabItem = this.Find< TabItem >( "parallelismTabItem" );
                var otherTabItem       = this.Find< TabItem >( "otherTabItem" );
                switch ( settingsTab )
                {
                    case SettingsTabEnum.Parallelism: ((TabControl) parallelismTabItem.Parent).SelectedItem = parallelismTabItem; break;
                    case SettingsTabEnum.Other      : ((TabControl) otherTabItem      .Parent).SelectedItem = otherTabItem;       break;
                }
            }
        }

        private void InitializeComponent()
        {
            this.Find< Button >( "okButton"     ).Click += (s, e) => OkButtonProcess();
            this.Find< Button >( "cancelButton" ).Click += (s, e) => this.Close();
        }

        protected override void OnOpened( EventArgs e )
        {
            base.OnOpened( e );

            this.Find< Button >( "collectGarbageButton" ).Focus();
        }
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            _GetTotalMemoryTimer.Enabled = false;
            _DownloadController.IsDownloadingChanged -= DownloadController_IsDownloadingChanged_4_parallelism;
            _DownloadController.IsDownloadingChanged -= DownloadController_IsDownloadingChanged_4_other;
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
        protected override async void OnPropertyChanged( AvaloniaPropertyChangedEventArgs e )
        {
            if ( (e.Property == Window.WindowStateProperty) && ((WindowState) e.NewValue == WindowState.Minimized) )
            {
                var state = (WindowState) e.OldValue;
                await Task.Delay( 1 );
                this.WindowState = state;
            }
            base.OnPropertyChanged( e );
        }
        #endregion

        #region [.public methods.]
        public bool Success { get; private set; }
        #endregion

        #region [.private methods.]
        private bool OkButtonProcess()
        {
            if ( IsValid_4_other() )
            {
                this.Success = true;
                this.Close();
                return (true);
            }
            return (false);
        }
        #endregion
    }
}
