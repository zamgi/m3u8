using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class WaitBannerUC : UserControl, IWaitBannerMarker
    {
        private const string CAPTION_TEXT = "...executing...";

        #region [.field's.]
        private CancellationTokenSource _CancellationTokenSource;
        private string   _CaptionText;
        private DateTime _StartDateTime;
        private int      _CurrentSteps;
        private int      _TotalSteps;
        private int      _PercentSteps;
        private Form     _FirstAppForm;
        private string   _FirstAppFormText;
        private string   _SpeedText;
        #endregion

        #region [.ctor().]
        private WaitBannerUC()
        {
            InitializeComponent();

            _StartDateTime = DateTime.Now;
            _FirstAppForm  = Application.OpenForms.Cast< Form >().First();
            _FirstAppFormText = _FirstAppForm.Text;
        }
        
        protected override void Dispose( bool disposing )
        {
            if ( disposing && (components != null) )
            {  
                components.Dispose();
            }
            base.Dispose( disposing );

            //---SetMainFormCursor( Cursors.Default );
            //---Application.DoEvents();
            _FirstAppForm.Text = _FirstAppFormText;
        }
        #endregion

        private void WaitBannerUC_Load( object sender, EventArgs e ) => fuskingTimer.Enabled = true;
        private void cancelButton_Click( object sender, EventArgs e )
        {
            cancelButton.Enabled = false;

            _CancellationTokenSource.Cancel();
        }
        private void fuskingTimer_Tick( object sender, EventArgs e )
        {
            fuskingTimer.Interval = 200;

            captionLabel .Text = $"{_CaptionText}{_PercentSteps}%";
            progressLabel.Text = $"{_CurrentSteps} of {_TotalSteps}";
            elapsedLabel .Text = '(' + (DateTime.Now - _StartDateTime).ToString( "hh\\:mm\\:ss" /*---/ "hh\\:mm\\:ss\\.f" /---*/ ) + ')';
            if ( !_SpeedText.IsNullOrEmpty() )
            {
                speedLabel.Text = '[' + _SpeedText + ']';
                speedLabel.Visible = true;
            }
            else
            {
                speedLabel.Text = null;
                speedLabel.Visible = false;
            }

            indicatorPictureBox.Image   = BitmapHolder.IndicatorI.Next();
            indicatorPictureBox.Visible = true;

            //----------------------------------------------------------------------//
            _FirstAppForm.Text = (_FirstAppForm.WindowState == FormWindowState.Minimized)
                                ? ($"{_PercentSteps}%, {elapsedLabel.Text}" + (!_SpeedText.IsNullOrEmpty() ? $", {_SpeedText}" : null ))
                                : _FirstAppFormText;
        }

        public void SetTotalSteps( int totalSteps ) => _TotalSteps = totalSteps;
        public void IncreaseSteps( string speedText )
        {            
            _CurrentSteps++;
            _PercentSteps = Convert.ToByte( (100.0 * _CurrentSteps) / _TotalSteps );
            _SpeedText    = speedText;
        }


        public static WaitBannerUC Create( Control parent, CancellationTokenSource cts, string captionText = CAPTION_TEXT )
        {
            if ( parent == null ) throw (new ArgumentNullException( nameof(parent) ));
            if ( cts    == null ) throw (new ArgumentNullException( nameof(cts) ));
            //------------------------------------------------------------------------//

            var uc = new WaitBannerUC() { _CancellationTokenSource = cts, _CaptionText = captionText };            
            uc.captionLabel.Text = captionText;
            parent.Controls.Add( uc );
            uc.BringToFront();
            uc.Anchor = AnchorStyles.None;
            uc.Location = new Point( (parent.ClientSize.Width - uc.Size.Width) >> 1, (parent.ClientSize.Height - uc.Size.Height) >> 1 );
            Application.DoEvents();
            return (uc);
        }
    }
}
