using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

using m3u8.download.manager.controllers;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SettingsForm : Window
    {
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
        #endregion

        #region [.fields.]
        private DownloadController _DownloadController;
        #endregion

        #region [.ctor().]
        public SettingsForm()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        internal SettingsForm( DownloadController dc ) : this()
        {
            _DownloadController = dc ?? throw (new ArgumentNullException( nameof(dc) ));
            _DownloadController.IsDownloadingChanged -= DownloadController_IsDownloadingChanged;
            _DownloadController.IsDownloadingChanged += DownloadController_IsDownloadingChanged;

            DownloadController_IsDownloadingChanged( _DownloadController.IsDownloading );
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );

            attemptRequestCountByPartNUD                  = this.Find< NumericUpDown >( nameof(attemptRequestCountByPartNUD) );
            uniqueUrlsOnlyCheckBox                        = this.Find< CheckBox      >( nameof(uniqueUrlsOnlyCheckBox) );
            only4NotRunLabel1                             = this.Find< TextBlock     >( nameof(only4NotRunLabel1) );
            only4NotRunLabel2                             = this.Find< TextBlock     >( nameof(only4NotRunLabel2) );
            requestTimeoutByPartDTP                       = this.Find< TimePickerUC  >( nameof(requestTimeoutByPartDTP) );
            //requestTimeoutByPartDTP                       = this.Find< DatePicker    >( nameof(requestTimeoutByPartDTP) );
            outputFileExtensionTextBox                    = this.Find< TextBox       >( nameof(outputFileExtensionTextBox) );
            showOnlyRequestRowsWithErrorsCheckBox         = this.Find< CheckBox      >( nameof(showOnlyRequestRowsWithErrorsCheckBox) );
            showDownloadStatisticsInMainFormTitleCheckBox = this.Find< CheckBox      >( nameof(showDownloadStatisticsInMainFormTitleCheckBox) );

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
        #endregion

        #region [.public methods.]
        public bool Success { get; private set; }

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

        #region [.private methods.]
        private bool IsValid()
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

        private static string CorrectOutputFileExtension( string ext )
        {
            ext = ext?.Trim();
            if ( !ext.IsNullOrEmpty() && ext.HasFirstCharNotDot() )
            {
                ext = '.' + ext;
            }
            return (ext);
        }

        private void DownloadController_IsDownloadingChanged( bool isDownloading )
        {
            only4NotRunLabel1.IsVisible =
                only4NotRunLabel2.IsVisible = isDownloading;
        }
        #endregion
    }
}
