using System.Drawing;

namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal class BorderlessDateTimePicker : DateTimePicker
    {
        protected override void WndProc( ref Message m )
        {
            const int WM_PAINT = 0x0F;

            base.WndProc( ref m );

            if ( m.Msg == WM_PAINT )
            {
                using ( var gr = Graphics.FromHwnd( this.Handle ) )
                {
                    // Draw over the default border with the parent's background color
                    const int BORDER_THICKNESS = 2;
                    using ( var pen = new Pen( this.Parent.BackColor, BORDER_THICKNESS ) )
                    {
                        gr.DrawRectangle( pen, 0, 0, this.Width, this.Height );
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class BorderDateTimePicker : BorderlessDateTimePicker
    {
        public Color BorderColor    { get; set; } = Color.Silver;
        public float BorderThikness { get; set; } = 1;

        protected override void WndProc( ref Message m )
        {
            const int WM_PAINT = 0x0F;

            base.WndProc( ref m );

            if ( m.Msg == WM_PAINT )
            {
                using ( var gr = Graphics.FromHwnd( this.Handle ) )
                {
                    // Draw over the default border with the parent's background color
                    var bt = this.BorderThikness;
                    using ( var pen = new Pen( this.BorderColor, bt ) )
                    {
                        gr.DrawRectangle( pen, 0, 0, this.Width - bt, this.Height - bt );
                    }
                }
            }
        }
    }
}