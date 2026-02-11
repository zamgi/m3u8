using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class WaitBannerUC : UserControl
    {
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
        private WaitBannerUC()
        {
            InitializeComponent();

            _StartDateTime = DateTime.Now;
            //_FirstAppForm     = Application.OpenForms.Cast< Form >().First();
            //_FirstAppFormText = _FirstAppForm.Text;
        }
        
        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {  
                components?.Dispose();
            }
            base.Dispose( disposing );

            //_FirstAppForm.Text = _FirstAppFormText;
        }
        #endregion

        #region [.private methods.]
        private void WaitBannerUC_Load( object sender, EventArgs e ) => timer.Enabled = true;
        private void cancelButton_Click( object sender, EventArgs e )
        {
            cancelButton.Enabled = false;

            _CancellationTokenSource.Cancel();
        }
        private void timer_Tick( object sender, EventArgs e )
        {
            const string HH_MM_SS = "hh\\:mm\\:ss";

            timer.Interval = 200;

            var ts = DateTime.Now - _StartDateTime;
            if ( _VisibleDelayInMilliseconds.HasValue )
            {
                if ( _VisibleDelayInMilliseconds.Value <= ts.Duration().TotalMilliseconds )
                {
                    this.Visible = true;
                    _VisibleDelayInMilliseconds = null;
                }
            }
            captionLabel .Text    = (_IsInWaitingForOtherAppInstanceFinished ? "...waiting for other app-instance finished..." : $"{_CaptionText}{_PercentSteps}%");
            progressLabel.Text    = $"{_CurrentSteps} of {_TotalSteps}";            
            elapsedLabel .Text    = '(' + ts.ToString( HH_MM_SS ) + ')';
            speedLabel   .Text    = (_SpeedText.IsNullOrEmpty() ? null : ('[' + _SpeedText + ']'));
            speedLabel   .Visible = !_SpeedText.IsNullOrEmpty();

            indicatorPictureBox.Image   = BitmapHolder.IndicatorI.Next();
            indicatorPictureBox.Visible = true;
        }
        #endregion

        #region [.public.]
        public void SetTotalSteps( int totalSteps ) => _TotalSteps = totalSteps;
        public void IncreaseSteps( string speedText = null )
        {            
            _CurrentSteps++;
            _PercentSteps = Convert.ToByte( (100.0 * _CurrentSteps) / _TotalSteps );
            _SpeedText    = speedText;

            _IsInWaitingForOtherAppInstanceFinished = false;
        }

        private bool _IsInWaitingForOtherAppInstanceFinished;
        public void WaitingForOtherAppInstanceFinished() => _IsInWaitingForOtherAppInstanceFinished = true;

        public void SetCaptionText( string captionText ) => _CaptionText = captionText;

        public static WaitBannerUC Create( Control parent, CancellationTokenSource cts, int visibleDelayInMilliseconds ) => Create( parent, cts, CAPTION_TEXT, visibleDelayInMilliseconds );
        public static WaitBannerUC Create( Control parent, CancellationTokenSource cts, string captionText = CAPTION_TEXT, int? visibleDelayInMilliseconds = null )
        {
            if ( parent == null ) throw (new ArgumentNullException( nameof(parent) ));
            if ( cts    == null ) throw (new ArgumentNullException( nameof(cts) ));
            //------------------------------------------------------------------------//

            parent.SuspendDrawing();
            try
            {
                var uc = new WaitBannerUC()
                {
                    _CancellationTokenSource    = cts,
                    _CaptionText                = captionText,
                    _VisibleDelayInMilliseconds = visibleDelayInMilliseconds,
                };
                if ( visibleDelayInMilliseconds.HasValue )
                {
                    uc.Visible = false;
                    uc.timer.Interval = Math.Min( 100, visibleDelayInMilliseconds.Value );
                    uc.timer.Enabled  = true;
                }
                uc.captionLabel.Text = captionText;            
                parent.Controls.Add( uc );
                uc.BringToFront();
                uc.Anchor = AnchorStyles.None;
                uc.Location = new Point( (parent.ClientSize.Width - uc.Size.Width) >> 1, (parent.ClientSize.Height - uc.Size.Height) >> 1 );
                Application.DoEvents();
                return (uc);                
            }
            finally
            {
                parent.ResumeDrawing();
            }
        }
        #endregion
    }
}
