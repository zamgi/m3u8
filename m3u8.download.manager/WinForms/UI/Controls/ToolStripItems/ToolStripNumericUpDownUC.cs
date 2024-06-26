using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal abstract class ToolStripNumericUpDownUC : ToolStripControlHost
    {
        public ToolStripNumericUpDownUC( string captionText, decimal minValue, decimal maxValue, int decimalPlaces ) 
            : base( new NumericUpDownUC() { _CaptionText = captionText, _MinValue = minValue, _MaxValue = maxValue, _DecimalPlaces = decimalPlaces } ) { }
        public ToolStripNumericUpDownUC( string captionText, EventHandler onValueChanged, decimal minValue, decimal maxValue, int decimalPlaces ) 
            : this( captionText, minValue, maxValue, decimalPlaces ) => _OnValueChanged += onValueChanged;
        protected override void Dispose( bool disposing )
        {
            if ( disposing ) _Timer?.Dispose();
            base.Dispose( disposing );
        }

        public NumericUpDownUC GetNumericUpDownUC() => (NumericUpDownUC) Control;

        public decimal _Value
        {
            get => GetNumericUpDownUC()._Value;
            set => GetNumericUpDownUC()._Value = value;
        }
        public decimal _MinValue
        {
            get => GetNumericUpDownUC()._MinValue;
            set => GetNumericUpDownUC()._MinValue = value;
        }
        public decimal _MaxValue
        {
            get => GetNumericUpDownUC()._MaxValue;
            set => GetNumericUpDownUC()._MaxValue = value;
        }

        public abstract Color HighlightBackColor { get; }

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

        public event EventHandler _OnTextChanged;
        public event EventHandler _OnValueChanged;

        private void OnTextChanged( object sender, EventArgs e )
        {
            _OnTextChanged?.Invoke( this, e );
            this.Invalidate();
        }
        private void OnValueChanged( object sender, EventArgs e )
        {
            _OnValueChanged?.Invoke( this, e );
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
            var color        = is_Highlight ? this.HighlightBackColor : this.BackColor;

            GetNumericUpDownUC().HighlightBackColor = color;

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
                    Debug.WriteLine( "ToolStripNumericUpDownUC => _Timer.Stop" );
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
                Debug.WriteLine( "ToolStripNumericUpDownUC => _Timer.Start" );
            }
        }
        private void Parent_MouseClick( object sender, MouseEventArgs e )
        {
            if ( GetSelfFullRect().Contains( e.Location ) )
            {
                GetNumericUpDownUC().PerformClick();
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
