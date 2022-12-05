using System;
using System.Drawing;
using System.Windows.Forms;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ToolStripSpeedThreshold : ToolStripControlHost
    {
        public ToolStripSpeedThreshold() : base( new SpeedThresholdUC() { /*Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right*/ /*Dock = DockStyle.Fill*/ } ) { }
        public ToolStripSpeedThreshold( EventHandler speedThreshold_ValueChanged ) : this() => SpeedThreshold_ValueChanged += speedThreshold_ValueChanged;

        public SpeedThresholdUC GetSpeedThresholdUC() => (SpeedThresholdUC) Control;

        public int Value
        {
            get => GetSpeedThresholdUC().SpeedThreshold_Value;
            set => GetSpeedThresholdUC().SpeedThreshold_Value = value;
        }

        protected override void OnSubscribeControlEvents( Control c )
        {
            base.OnSubscribeControlEvents( c );

            var x = (SpeedThresholdUC) c;
            x.SpeedThreshold_TextChanged += new EventHandler( OnTextChanged );
            x.SpeedThreshold_ValueChanged += new EventHandler( OnValueChanged );
        }
        protected override void OnUnsubscribeControlEvents( Control c )
        {
            base.OnUnsubscribeControlEvents( c );

            var x = (SpeedThresholdUC) c;
            x.SpeedThreshold_TextChanged -= new EventHandler( OnTextChanged );
            x.SpeedThreshold_ValueChanged -= new EventHandler( OnValueChanged );
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

        protected override void OnPaint( PaintEventArgs e )
        {
            var _ToolStripSpeedThreshold = this;

            var p = _ToolStripSpeedThreshold.GetCurrentParent();

            using var gr = Graphics.FromHwnd( p.Handle );

            var dr = p.DisplayRectangle;
            var rc = _ToolStripSpeedThreshold.Bounds;
                rc.X     = 0;
                rc.Width = dr.Right;

            //p.BackColor = Color.Red;
            //gr.FillRectangle( Brushes.Red, rc );

            using var br = new SolidBrush( _ToolStripSpeedThreshold.BackColor );
            gr.FillRectangle( br, rc );

            base.OnPaint( e );
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
