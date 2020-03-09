using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace m3u8.download.manager.ui
{
    public sealed class WaitBannerForm : Window
    {
        #region [.field's.]
        private Timer     _Timer;
        private Button    cancelButton;
        private TextBlock captionLabel;
        private TextBlock progressLabel;
        private TextBlock elapsedLabel;
        private TextBlock speedLabel;
        private Image     indicatorImage;
        #endregion

        #region [.field's.]
        private const string CAPTION_TEXT = "...executing...";

        private CancellationTokenSource _CancellationTokenSource;
        private string   _CaptionText;
        private DateTime _StartDateTime;
        private int      _CurrentSteps;
        private int      _TotalSteps;
        private int      _PercentSteps;
        private string   _SpeedText;
        private int?     _VisibleDelayInMilliseconds;        
        #endregion

        #region [.ctor().]
        public WaitBannerForm()
        {
            this.InitializeComponent();

            _StartDateTime = DateTime.Now;
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );

            cancelButton   = this.Find< Button    >( nameof(cancelButton) );
            captionLabel   = this.Find< TextBlock >( nameof(captionLabel) );
            progressLabel  = this.Find< TextBlock >( nameof(progressLabel) );
            elapsedLabel   = this.Find< TextBlock >( nameof(elapsedLabel) );
            speedLabel     = this.Find< TextBlock >( nameof(speedLabel) );
            indicatorImage = this.Find< Image     >( nameof(indicatorImage) );

            cancelButton.Click += cancelButton_Click;
            _Timer = new Timer( (state) => Dispatcher.UIThread.Post( Timer_Tick ), null, 2500, 200 );
        }
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            _Timer.Dispose_NoThrow();
        }
        #endregion

        #region [.private methods.]
        private void cancelButton_Click( object sender, EventArgs e )
        {
            cancelButton.IsEnabled = false;

            _CancellationTokenSource.Cancel();
        }
        private void Timer_Tick()
        {
            try
            {
                const string HH_MM_SS = "hh\\:mm\\:ss";

                var ts = DateTime.Now - _StartDateTime;
                if ( _VisibleDelayInMilliseconds.HasValue )
                {
                    if ( _VisibleDelayInMilliseconds.Value <= ts.Duration().TotalMilliseconds )
                    {
                        this.IsVisible = true;
                        _VisibleDelayInMilliseconds = null;
                    }
                }
                captionLabel .Text      = (_IsInWaitingForOtherAppInstanceFinished ? "...waiting for other app-instance finished..." : $"{_CaptionText}{_PercentSteps}%");
                progressLabel.Text      = $"{_CurrentSteps} of {_TotalSteps}";            
                elapsedLabel .Text      = '(' + ts.ToString( HH_MM_SS ) + ')';
                speedLabel   .Text      = (_SpeedText.IsNullOrEmpty() ? null : ('[' + _SpeedText + ']'));
                speedLabel   .IsVisible = !_SpeedText.IsNullOrEmpty();

                indicatorImage.Source    = BitmapHolder.IndicatorI.Next();
                indicatorImage.IsVisible = true;
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        #endregion

        #region [.public.]
        public void SetTotalSteps( int totalSteps ) => _TotalSteps = totalSteps;
        public void IncreaseSteps( string speedText )
        {            
            _CurrentSteps++;
            _PercentSteps = Convert.ToByte( (100.0 * _CurrentSteps) / _TotalSteps );
            _SpeedText    = speedText;

            _IsInWaitingForOtherAppInstanceFinished = false;
        }

        private bool _IsInWaitingForOtherAppInstanceFinished;
        public void WaitingForOtherAppInstanceFinished() => _IsInWaitingForOtherAppInstanceFinished = true;

        public static WaitBannerForm Create( Window owner, CancellationTokenSource cts, int visibleDelayInMilliseconds ) => Create( owner, cts, CAPTION_TEXT, visibleDelayInMilliseconds );
        public static WaitBannerForm Create( Window owner, CancellationTokenSource cts, string captionText = CAPTION_TEXT, int? visibleDelayInMilliseconds = null )
        {
            if ( owner == null ) throw (new ArgumentNullException( nameof(owner) ));
            if ( cts   == null ) throw (new ArgumentNullException( nameof(cts) ));
            //------------------------------------------------------------------------//

            //parent.SuspendDrawing();
            //try
            //{
                var form = new WaitBannerForm()
                {
                    _CancellationTokenSource    = cts,
                    _CaptionText                = captionText,
                    _VisibleDelayInMilliseconds = visibleDelayInMilliseconds,
                    Owner                       = owner,
                };
                form.captionLabel.Text = captionText;

                /*
                if ( visibleDelayInMilliseconds.HasValue )
                {
                    await Task.Delay( visibleDelayInMilliseconds.Value );
                    //form.IsVisible = false;
                    //form._Timer.Interval = Math.Min( 100, visibleDelayInMilliseconds.Value );
                    //form._Timer.Enabled  = true;
                }
                form.Show();
                */

                //parent.Controls.Add( form );
                //form.BringToFront();
                //form.Anchor = AnchorStyles.None;
                //form.Location = new Point( (parent.Width - form.Width) / 2, (parent.Height - form.Height) / 2 );
                //Application.DoEvents();
                return (form);

            //}
            //finally
            //{
            //    parent.ResumeDrawing();
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class WaitBannerFormHolder : IDisposable
        {
            private WaitBannerForm _Form;
            public WaitBannerFormHolder( WaitBannerForm form, int? visibleDelayInMilliseconds )
            {
                _Form = form;

                if ( visibleDelayInMilliseconds.HasValue )
                {
                    void show_NoThrow()
                    {
                        try
                        {
                            _Form?.Show();
                        }
                        catch ( Exception ex )
                        {
                            Debug.WriteLine( ex );
                        }
                    };

                    Task.Run( async () =>
                    {
                        await Task.Delay( visibleDelayInMilliseconds.Value );
                        await Dispatcher.UIThread.InvokeAsync( show_NoThrow );
                    });
                }
                else
                {
                    _Form.Show();
                }
            }
            public void Dispose()
            {
                if ( _Form != null )
                {
                    try
                    {
                        _Form.Close();
                        _Form = null;
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( ex );
                    }
                }
            }
        }
        public static IDisposable CreateAndShow( Window owner, CancellationTokenSource cts, string captionText = CAPTION_TEXT, int? visibleDelayInMilliseconds = null )
        {
            if ( owner == null ) throw (new ArgumentNullException( nameof(owner) ));
            if ( cts   == null ) throw (new ArgumentNullException( nameof(cts) ));
            //------------------------------------------------------------------------//

            var form = new WaitBannerForm()
            {
                _CancellationTokenSource    = cts,
                _CaptionText                = captionText,
                _VisibleDelayInMilliseconds = visibleDelayInMilliseconds,
                Owner                       = owner,
            };
            form.captionLabel.Text = captionText;

            return (new WaitBannerFormHolder( form, visibleDelayInMilliseconds ));
        }
        #endregion
    }
}
