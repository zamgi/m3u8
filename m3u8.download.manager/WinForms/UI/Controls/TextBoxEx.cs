using System.Windows.Forms;

using m3u8.download.manager.infrastructure;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class TextBoxEx : TextBox
    {
        private const int WM_PASTE = 0x0302;
        protected override void WndProc( ref Message m )
        {
            base.WndProc( ref m );

            if ( m.Msg == WM_PASTE )
            {
                var text         = Clipboard.GetText();
                var text_trimmed = PathnameCleaner.CleanPathnameAndFilename( text?.Trim() ); //---text?.Trim().Replace( '\r', ' ' ).Replace( '\n', ' ' ).Replace( '\t', ' ' );
                if ( text != text_trimmed )
                {
                    this.Text = text_trimmed;
                }
            }
        }
    }
}
