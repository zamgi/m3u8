using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace m3u8.downloader.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class WaitBannerUC_v2 : UserControl, IWaitBannerMarker
    {
        private const string CAPTION_TEXT = "...executing...";

        //---private EventWaitHandle _CancelWaitHandle;
        private CancellationTokenSource _CancellationTokenSource;
        private string   _CaptionText;
        private DateTime _StartDateTime;

        public WaitBannerUC_v2()
        {
            InitializeComponent();
            _StartDateTime = DateTime.Now;

            SetStyle( ControlStyles.SupportsTransparentBackColor, true );
            SetStyle( ControlStyles.Opaque, true );
            this.BackColor = Color.Transparent;
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                //cp.ExStyle = cp.ExStyle | 0x20;
                return (cp);
            }
        }
        //protected override void OnPaint( PaintEventArgs e )
        //{
        //    var g = e.Graphics;
        //    var bounds = new Rectangle( 0, 0, this.Width - 1, this.Height - 1 );

        //    Color frmColor = this.Parent.BackColor;
        //    Brush bckColor = default(Brush);

        //    #region commented
        //    /*var alpha = (m_opacity * 255) / 100;
        //    if ( drag )
        //    {
        //        Color dragBckColor = default( Color );

        //        if ( BackColor != Color.Transparent )
        //        {
        //            int Rb = BackColor.R * alpha / 255 + frmColor.R * (255 - alpha) / 255;
        //            int Gb = BackColor.G * alpha / 255 + frmColor.G * (255 - alpha) / 255;
        //            int Bb = BackColor.B * alpha / 255 + frmColor.B * (255 - alpha) / 255;
        //            dragBckColor = Color.FromArgb( Rb, Gb, Bb );
        //        }
        //        else
        //        {
        //            dragBckColor = frmColor;
        //        }

        //        alpha = 255;
        //        bckColor = new SolidBrush( Color.FromArgb( alpha, dragBckColor ) );
        //    }
        //    else
        //    {
        //        bckColor = new SolidBrush( Color.FromArgb( alpha, this.BackColor ) );
        //    }*/ 
        //    #endregion

        //    bckColor = new SolidBrush( Color.FromArgb( 0, this.BackColor ) );

        //    if ( this.BackColor != Color.Transparent /*| drag*/ )
        //    {
        //        g.FillRectangle( bckColor, bounds );
        //    }

        //    bckColor.Dispose();
        //    g.Dispose();
        //    base.OnPaint( e );
        //}
        protected override void OnBackColorChanged( EventArgs e )
        {
            this.Parent?.Invalidate( this.Bounds, true );
            base.OnBackColorChanged( e );
        }
        protected override void OnParentBackColorChanged( EventArgs e )
        {
            this.Invalidate();
            base.OnParentBackColorChanged( e );
        }


        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && (components != null) )
            {  
                components.Dispose();
            }
            base.Dispose( disposing );

            //---SetMainFormCursor( Cursors.Default );
            //---Application.DoEvents();
        }
        private void WaitBannerUC_Load( object sender, EventArgs e )
        {
            fuskingTimer.Enabled = true;
        }
        private void fuskingTimer_Tick( object sender, EventArgs e )
        {
            fuskingTimer.Interval = 200;

            var ts = (DateTime.Now - _StartDateTime);
            captionLabel.Text = _CaptionText + Environment.NewLine + '(' + ts.ToString( "hh\\:mm\\:ss" /*---/ "hh\\:mm\\:ss\\.f" /---*/ ) + ')';

            indicatorPictureBox.Image = BitmapHolder.IndicatorI.Next();
            indicatorPictureBox.Visible = true;            
        }

        public static WaitBannerUC_v2 Create( CancellationTokenSource cts )
        {
            return (Create( Application.OpenForms.Cast< Form >().FirstOrDefault(), cts ));
        }
        public static WaitBannerUC_v2 Create( Control parent, CancellationTokenSource cts, string captionText = CAPTION_TEXT )
        {
            if ( parent == null ) throw (new ArgumentNullException( nameof(parent) ));
            if ( cts    == null ) throw (new ArgumentNullException( nameof(cts)    ));

            captionText = captionText ?? CAPTION_TEXT;
            var uc = new WaitBannerUC_v2()
            {
                _CancellationTokenSource = cts,
                _CaptionText             = captionText,
            };
            uc.captionLabel.Text = captionText;
            uc.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top; //uc.Anchor = AnchorStyles.None;
            uc.Location = Point.Empty; //uc.Location = new Point( (parent.ClientSize.Width - uc.Size.Width) >> 1, (parent.ClientSize.Height - uc.Size.Height) >> 1 );
            uc.Size = parent.ClientSize;
            parent.Controls.Add( uc );
            uc.BringToFront();
            //---SetMainFormCursor( Cursors.AppStarting );
            Application.DoEvents();
            return (uc);
        }

        /*public static WaitBannerUC Create( EventWaitHandle cancelWaitHandle )
        {
            return (Create( Application.OpenForms.Cast< Form >().FirstOrDefault(), cancelWaitHandle ));
        }
        public static WaitBannerUC Create( Control parent, EventWaitHandle cancelWaitHandle )
        {
            return (Create( parent, CAPTION_TEXT, cancelWaitHandle ));
        }
        private static WaitBannerUC Create( Control parent, string captionText, EventWaitHandle cancelWaitHandle )
        {
            if ( cancelWaitHandle == null ) throw (new ArgumentNullException( nameof(cancelWaitHandle) ));

            var uc = new WaitBannerUC()
            {
                _CancelWaitHandle = cancelWaitHandle,
                _CaptionText      = captionText
            };
            uc.captionLabel.Text = captionText;
            uc.Anchor = AnchorStyles.None;
            uc.Location = new Point( (parent.ClientSize.Width - uc.Size.Width) >> 1, (parent.ClientSize.Height - uc.Size.Height) >> 1 );
            parent.Controls.Add( uc );
            uc.BringToFront();
            SetMainFormCursor( Cursors.AppStarting );
            Application.DoEvents();
            return (uc);
        }*/

        private static void SetMainFormCursor( Cursor cursor )
        {
            var mainForm = Application.OpenForms.Cast< Form >().FirstOrDefault();
            if ( mainForm != null )
            {
                mainForm.Cursor = cursor;
            }
        }

        private void cancelButton_Click( object sender, EventArgs e )
        {
            cancelButton.Enabled = false;

            _CancellationTokenSource?.Cancel();
            //---_CancelWaitHandle?.Set();
        }
    }
}
