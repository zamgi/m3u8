﻿using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace m3u8.downloader
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
        private Form     _FirstAppForm;
        private string   _FirstAppFormText;
        private string   _SpeedText;
        private bool     _IsInWaitingForOtherAppInstanceFinished;
        #endregion

        #region [.ctor().]
        private WaitBannerUC()
        {
            InitializeComponent();

            _StartDateTime    = DateTime.Now;
            _FirstAppForm     = Application.OpenForms.Cast< Form >().First();
            _FirstAppFormText = _FirstAppForm.Text;
        }
        
        protected override void Dispose( bool disposing )
        {
            if ( disposing && (components != null) )
            {  
                components.Dispose();
            }
            base.Dispose( disposing );

            _FirstAppForm.Text = _FirstAppFormText;
        }
        #endregion

        #region [.private methods.]
        private void WaitBannerUC_Load( object sender, EventArgs e ) => fuskingTimer.Enabled = true;
        private void cancelButton_Click( object sender, EventArgs e )
        {
            cancelButton.Enabled = false;

            _CancellationTokenSource.Cancel();
        }
        private void fuskingTimer_Tick( object sender, EventArgs e )
        {
            const string HH_MM_SS = "hh\\:mm\\:ss"; // "hh\\:mm\\:ss\\.f";
            const string MM_SS    = "mm\\:ss";

            fuskingTimer.Interval = 200;

            var ts = DateTime.Now - _StartDateTime;

            captionLabel .Text    = (_IsInWaitingForOtherAppInstanceFinished ? "...waiting for other app-instance finished..." : $"{_CaptionText}{_PercentSteps}%");
            progressLabel.Text    = $"{_CurrentSteps} of {_TotalSteps}";            
            elapsedLabel .Text    = '(' + ts.ToString( HH_MM_SS ) + ')';
            speedLabel   .Text    = (_SpeedText.IsNullOrEmpty() ? null : ('[' + _SpeedText + ']'));
            speedLabel   .Visible = !_SpeedText.IsNullOrEmpty();

            indicatorPictureBox.Image   = BitmapHolder.IndicatorI.Next();
            indicatorPictureBox.Visible = true;

            //----------------------------------------------------------------------//
            var elapsed = ((1 < ts.TotalHours) ? ts.ToString( HH_MM_SS ) : (':' + ts.ToString( MM_SS )));

            _FirstAppForm.Text = (_IsInWaitingForOtherAppInstanceFinished 
                                 ? $"(wait), ({elapsed})" : $"{_PercentSteps}%, ({elapsed}){(_SpeedText.IsNullOrEmpty() ? null : $", {_SpeedText}")}");
        }
        #endregion

        #region [.public methods.]
        public void SetTotalSteps( int totalSteps ) => _TotalSteps = totalSteps;
        public void IncreaseSteps( string speedText )
        {            
            _CurrentSteps++;
            _PercentSteps = Convert.ToByte( (100.0 * _CurrentSteps) / _TotalSteps );
            _SpeedText    = speedText;

            _IsInWaitingForOtherAppInstanceFinished = false;
        }

        public void WaitingForOtherAppInstanceFinished() => _IsInWaitingForOtherAppInstanceFinished = true;

        public static WaitBannerUC Create( Control parent, CancellationTokenSource cts, string captionText = CAPTION_TEXT )
        {
            if ( parent == null ) throw (new ArgumentNullException( nameof(parent) ));
            if ( cts    == null ) throw (new ArgumentNullException( nameof(cts) ));
            //------------------------------------------------------------------------//

            parent.SuspendDrawing();
            try
            {
                var uc = new WaitBannerUC() { _CancellationTokenSource = cts, _CaptionText = captionText };            
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
