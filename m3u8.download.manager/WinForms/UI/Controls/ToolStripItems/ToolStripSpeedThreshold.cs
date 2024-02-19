using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ToolStripSpeedThreshold : ToolStripControlHost
    {
        public ToolStripSpeedThreshold() : base( new NumericUpDownUC() { _CaptionText = "Mbps" /*Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right*/ /*Dock = DockStyle.Fill*/ } ) { }
        public ToolStripSpeedThreshold( EventHandler speedThreshold_ValueChanged ) : this() => SpeedThreshold_ValueChanged += speedThreshold_ValueChanged;
        protected override void Dispose( bool disposing )
        {
            if ( disposing ) _Timer?.Dispose();
            base.Dispose( disposing );
        }

        public NumericUpDownUC GetSpeedThresholdUC() => (NumericUpDownUC) Control;

        public int _Value
        {
            get => GetSpeedThresholdUC()._Value;
            set => GetSpeedThresholdUC()._Value = value;
        }

        protected override void OnSubscribeControlEvents( Control c )
        {
            base.OnSubscribeControlEvents( c );

            var x = (NumericUpDownUC) c;
            x._TextChanged  += new EventHandler( OnTextChanged );
            x._ValueChanged += new EventHandler( OnValueChanged );
        }
        protected override void OnUnsubscribeControlEvents( Control c )
        {
            base.OnUnsubscribeControlEvents( c );

            var x = (NumericUpDownUC) c;
            x._TextChanged  -= new EventHandler( OnTextChanged );
            x._ValueChanged -= new EventHandler( OnValueChanged );
        }
        protected override void OnParentChanged( ToolStrip oldParent, ToolStrip newParent )
        {
            base.OnParentChanged( oldParent, newParent );

            if ( oldParent != null )
            {
                oldParent.MouseMove  -= Parent_MouseMove;
                oldParent.MouseClick -= Parent_MouseClick;
                oldParent.Paint      -= Parent_Paint;
            }

            if ( newParent != null )
            {
                newParent.MouseMove  -= Parent_MouseMove;
                newParent.MouseClick -= Parent_MouseClick;
                newParent.Paint      -= Parent_Paint;

                newParent.MouseMove  += Parent_MouseMove;
                newParent.MouseClick += Parent_MouseClick;
                newParent.Paint      += Parent_Paint;
            }
        }

        public event EventHandler SpeedThreshold_TextChanged;
        public event EventHandler SpeedThreshold_ValueChanged;

        private void OnTextChanged( object sender, EventArgs e )
        {
            SpeedThreshold_TextChanged?.Invoke( this, e );
            this.Invalidate();
        }
        private void OnValueChanged( object sender, EventArgs e )
        {
            SpeedThreshold_ValueChanged?.Invoke( this, e );
            this.Invalidate();
        }

        protected override void OnMouseEnter( EventArgs e ) => this.Parent.Invalidate();
        protected override void OnMouseLeave( EventArgs e ) => this.Parent.Invalidate();
        private void Parent_Paint( object sender, PaintEventArgs e )
        {
            var p  = (ToolStrip) sender;
            var dr = e.ClipRectangle;
            var rc = this.Bounds;
                rc.X     = 0;
                rc.Width = dr.Right;

            var pt           = Control.MousePosition;
            var screen_rc    = p.RectangleToScreen( rc );
            var is_Highlight = screen_rc.Contains( pt );
            var color        = is_Highlight ? Color.FromArgb( 255, Color.FromKnownColor( KnownColor.SkyBlue ) ) /*SystemColors.Highlight*/ : this.BackColor;

            GetSpeedThresholdUC().HighlightBackColor = color;

            using var br = new SolidBrush( color );
            e.Graphics.FillRectangle( br, rc );
        }
        private Timer _Timer;
        private static Timer CreateTimer( EventHandler timer_Tick, int interval = 50, bool enabled = true )
        {
            var timer = new Timer() { Interval = interval, Enabled = enabled };
                timer.Tick += timer_Tick;            
            return (timer);
        }

        private void Timer_Tick( object sender, EventArgs e )
        {
            this.Parent.Invalidate();
            if ( !this.Parent.Visible || !GetSelfFullRectInScreen().Contains( Control.MousePosition ) )
            {
                if ( _Timer.Enabled )
                {
                    _Timer.Stop();
                    Debug.WriteLine( "ToolStripSpeedThreshold => _Timer.Stop" );
                }
            }
        }

        private void Parent_MouseMove( object sender, MouseEventArgs e )
        {
            this.Parent.Invalidate();

            if ( _Timer == null )
            {
                _Timer = CreateTimer( Timer_Tick );
            }
            else if ( !_Timer.Enabled )
            {
                _Timer.Start();
                Debug.WriteLine( "ToolStripSpeedThreshold => _Timer.Start" );
            }
        }
        private void Parent_MouseClick( object sender, MouseEventArgs e )
        {
            if ( GetSelfFullRect().Contains( e.Location ) )
            {
                GetSpeedThresholdUC().PerformClick();
            }
        }
        private Rectangle GetSelfFullRect()
        {
            var p = this.GetCurrentParent();

            var dr = p.DisplayRectangle;
            var rc = this.Bounds;
                rc.X     = 0;
                rc.Width = dr.Right;
            return (rc);
        }
        private Rectangle GetSelfFullRectInScreen()
        {
            var p = this.GetCurrentParent();

            var dr = p.DisplayRectangle;
            var rc = this.Bounds;
                rc.X     = 0;
                rc.Width = dr.Right;
            var screen_rc = p.RectangleToScreen( rc );
            return (screen_rc);
        }

        protected override Padding DefaultMargin => Padding.Empty;
        //ADJUST size of content to all available size of ToolStripControlHost//
        /*
        public override Size Size 
        {
            get
            {
                if ( this.Parent is ToolStripDropDownMenu toolStripDropDownMenu )
                {
                    var rc = toolStripDropDownMenu.DisplayRectangle;
                    var sz = new Size( rc.Width - 1, rc.Height );

                        var x = GetSpeedThresholdUC();
                        x.Width = sz.Width;
                        x.Refresh();

                    return (sz);
                }
                return (base.Size);
            }
            set => base.Size = value;
        }
        */
    }
}
