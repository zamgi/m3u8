using System.Drawing;
using System.Windows.Forms;

namespace m3u8.downloader.ui
{
    /// <summary>
    /// Good Draw panel on transparent parent user-control
    /// </summary>
    //[System.ComponentModel.DesignerCategory("Code")]
    internal sealed class PanelEx : Panel
    {
        public PanelEx()
        {
            SetStyle( ControlStyles.UserPaint    | ControlStyles.ResizeRedraw |
                      ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true );
        }

        protected override void OnPaint( PaintEventArgs e )
        {
            using ( var sbr = new SolidBrush( BackColor ) )
            {
                e.Graphics.FillRectangle( sbr, ClientRectangle );
            }
            e.Graphics.DrawRectangle( Pens.Black, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1 );
        }
    }
}
