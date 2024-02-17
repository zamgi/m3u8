using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using m3u8.download.manager.controllers;
using SETTINGS = m3u8.download.manager.controllers.SettingsPropertyChangeController;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ParallelismForm : Window
    {
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

        #region [.fields.]
        private DownloadController _DownloadController;
        #endregion

        #region [.ctor().]
        public ParallelismForm()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        internal ParallelismForm( DownloadController dc ) : this() 
        {
            _DownloadController = dc ?? throw (new ArgumentNullException( nameof(dc) ));
            _DownloadController.IsDownloadingChanged -= DownloadController_IsDownloadingChanged;
            _DownloadController.IsDownloadingChanged += DownloadController_IsDownloadingChanged;

            DownloadController_IsDownloadingChanged( _DownloadController.IsDownloading );
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );

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

            this.Find< Button >( "okButton"     ).Click += (s, e) => OkButtonProcess();
            this.Find< Button >( "cancelButton" ).Click += (s, e) => this.Close();
        }

        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            _DownloadController.IsDownloadingChanged -= DownloadController_IsDownloadingChanged;
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
        
        public int  MaxDegreeOfParallelism
        {
            get => Math.Min( SETTINGS.MAX_DEGREE_OF_PARALLELISM, Convert.ToInt32( maxDegreeOfParallelismNUD.Value ) );
            set => maxDegreeOfParallelismNUD.Value = Math.Min( SETTINGS.MAX_DEGREE_OF_PARALLELISM, value );
        }
        public bool UseCrossDownloadInstanceParallelism
        {
            get => useCrossDownloadInstanceParallelismCheckBox.IsChecked.GetValueOrDefault();
            set => useCrossDownloadInstanceParallelismCheckBox.IsChecked = value;
        }

        public int? MaxCrossDownloadInstance      => (useMaxCrossDownloadInstanceCheckBox.IsChecked.GetValueOrDefault() ? Convert.ToInt32( maxCrossDownloadInstanceNUD.Value ) : ((int?) null));
        public int  MaxCrossDownloadInstanceSaved => Convert.ToInt32( maxCrossDownloadInstanceNUD.Value );
        public void SetMaxCrossDownloadInstance( int? maxCrossDownloadInstance, int maxCrossDownloadInstanceSaved )
        {
            maxCrossDownloadInstanceNUD.Value = maxCrossDownloadInstance.GetValueOrDefault( maxCrossDownloadInstanceSaved );
            useMaxCrossDownloadInstanceCheckBox.IsChecked = maxCrossDownloadInstance.HasValue;
            useMaxCrossDownloadInstanceCheckBox_Click( null, null ); // useMaxCrossDownloadInstanceCheckBox, new RoutedEventArgs() );
        }

        public double? MaxSpeedThresholdInMbps => (!isUnlimMaxSpeedThresholdCheckBox.IsChecked.GetValueOrDefault() ? MaxSpeedThresholdInMbpsSaved : null);
        public double MaxSpeedThresholdInMbpsSaved => Math.Max( 0.1, (double) maxSpeedThresholdNUD.Value );
        public void SetMaxSpeedThresholdInMbps( double? maxSpeedThresholdInMbps, double maxSpeedThresholdInMbpsSaved )
        {
            maxSpeedThresholdNUD.Value = Math.Max( 0.1M, (decimal) maxSpeedThresholdInMbps.GetValueOrDefault( maxSpeedThresholdInMbpsSaved ) );
            isUnlimMaxSpeedThresholdCheckBox.IsChecked = !maxSpeedThresholdInMbps.HasValue;
            isUnlimMaxSpeedThresholdCheckBox_Click( null, null );
        }
        #endregion

        #region [.private methods.]
        private bool OkButtonProcess()
        {
            this.Success = true;
            this.Close();
            return (true);
        }

        private void DownloadController_IsDownloadingChanged( bool isDownloading )
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
            maxSpeedThresholdNUD  .IsEnabled  = !isChecked;
        }
        #endregion
    }
}
