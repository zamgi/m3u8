using System.Drawing;
using System.Windows.Forms;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ToolStripStatusLabelEx : ToolStripStatusLabel
    {
        public ToolStripStatusLabelEx() { }

        protected override void OnPaint( PaintEventArgs e )
        {
            //---base.OnPaint( e );

            //using var br = new SolidBrush( this.BackColor );
            //e.Graphics.FillRectangle( br, e.ClipRectangle );

            TextRenderer.DrawText( e.Graphics, this.Text, this.Font,
                e.ClipRectangle, this.ForeColor, Color.Transparent,
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter );
        }

        /// <summary>
        /// 
        /// </summary>
        public sealed class Color2ColorTransitionProcessor : Color2ColorTransition.ProcessorBase
        {
            private ToolStripItem _ToolStripItem;
            public Color2ColorTransitionProcessor( ToolStripItem toolStripItem ) => _ToolStripItem = toolStripItem;

            public void Run( string message, KnownColor startColor, int millisecondsDelay, int stepCount = 100 )
                => base.Run( message, Color.FromKnownColor( startColor ), _ToolStripItem.BackColor, millisecondsDelay, stepCount );

            protected override void StartAction( string message, Color startColor )
            {
                _ToolStripItem.Text      = message;
                _ToolStripItem.ForeColor = startColor;
            }
            protected override void ProgressAction( Color color ) => _ToolStripItem.ForeColor = color;
            protected override void EndAction() => _ToolStripItem.Text = null;
        }
    }
}
