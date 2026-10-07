using System.Drawing;

namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class PictureBoxEx : PictureBox
    {
        protected override void OnPaint( PaintEventArgs e )
        {
            if ( !this.Enabled && (this.Image != null) )
            {
                ControlPaint.DrawImageDisabled(
                    e.Graphics,
                    this.Image,
                    0,
                    0,
                    this.BackColor
                );
            }
            else
            {
                base.OnPaint( e );
            }
        }
    }
}
