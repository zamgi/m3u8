using System.Drawing;

namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ButtonWithDotsText : Button
    {
        protected override void OnPaint( PaintEventArgs e )
        {
            base.OnPaint( e );

            var rc = e.ClipRectangle;
            using var sf = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
            e.Graphics.DrawString( "...", this.Font, Brushes.Black, rc, sf );
            if ( this.Focused )
            {
                rc.Inflate( -1, -1 );
                ControlPaint.DrawFocusRectangle( e.Graphics, rc );
            }
        }
    }
}
