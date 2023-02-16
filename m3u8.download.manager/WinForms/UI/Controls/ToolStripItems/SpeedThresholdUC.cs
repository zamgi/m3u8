using System;
using System.Drawing;
using System.Windows.Forms;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class SpeedThresholdUC : UserControl
    {
        public SpeedThresholdUC()
        {
            InitializeComponent();

            this.SetStyle( ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true );
        }
        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();
            }
            base.Dispose( disposing );
        }


        public int SpeedThreshold_Value
        {
            get => Convert.ToInt32( speedThresholdNumericUpDownEx.Value );
            set => speedThresholdNumericUpDownEx.Value = value;
        }

        public event EventHandler SpeedThreshold_ValueChanged
        {
            add    => speedThresholdNumericUpDownEx.ValueChanged += value;
            remove => speedThresholdNumericUpDownEx.ValueChanged -= value;
        }
        public event EventHandler SpeedThreshold_TextChanged
        {
            add    => speedThresholdNumericUpDownEx.TextChanged += value;
            remove => speedThresholdNumericUpDownEx.TextChanged -= value;
        }

        protected override void OnPaint( PaintEventArgs e )
        {
            var gr = e.Graphics;
            var color = _HighlightBackColor.GetValueOrDefault( this.BackColor );

            using var br = new SolidBrush( color );
            gr.FillRectangle( br, this.DisplayRectangle );

            //using var br2 = new SolidBrush( this.l1.ForeColor );
            //gr.DrawString( this.l1.Text, this.l1.Font, br2, this.l1.Location );

            using var gr2 = Graphics.FromHwnd( this.l1.Handle );
            gr2.FillRectangle( br, this.l1.DisplayRectangle );

            using var br2 = new SolidBrush( this.l1.ForeColor );
            gr2.DrawString( this.l1.Text, this.l1.Font, br2, 1, 0 );
        }
        private Color? _HighlightBackColor;
        public Color HighlightBackColor
        {
            get => _HighlightBackColor.GetValueOrDefault( this.BackColor );
            set
            {
                if ( _HighlightBackColor != value )
                {
                    _HighlightBackColor = value;

                    this.l1.BackColor = _HighlightBackColor.GetValueOrDefault( this.BackColor );

                    //using var e = new PaintEventArgs( Graphics.FromHwnd( this.Handle ), Rectangle.Empty );
                    //OnPaint( e );
                    this.Invalidate();
                }
            }
        }
        
        public void PerformClick() => UC_Click( this, EventArgs.Empty );

        private void SpeedThresholdNumericUpDownEx_KeyDown( object sender, KeyEventArgs e )
        {
            if ( e.KeyCode == Keys.Enter )
            {
                UC_Click( sender, e );
            }
        }
        private void SpeedThresholdNumericUpDownEx_GotFocus( object sender, EventArgs e ) => this.Parent?.Refresh();
        private void UC_Click( object sender, EventArgs e )
        {
            var p = this.Parent;
            if ( p != null )
            {
                p.Focus();
                if ( p is ToolStripDropDownMenu toolStripDropDownMenu )
                {
                    speedThresholdNumericUpDownEx.Fire_ValueChanged();
                    toolStripDropDownMenu.Close();                    
                }
            }
        }
    }
}
