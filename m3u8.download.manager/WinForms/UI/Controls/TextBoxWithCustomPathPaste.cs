using System.Windows.Forms;

using m3u8.download.manager.infrastructure;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class TextBoxWithCustomPathPaste : TextBoxWithBorder
    {
        public TextBoxWithCustomPathPaste() { }


        private const int WM_PASTE   = 0x0302;
        #region comm. prev.
        /*
        protected override void WndProc( ref Message m )
        {
            base.WndProc( ref m );

            if ( m.Msg == WM_PASTE )
            {
                var text = Clipboard.GetText();
                var text_trimmed = PathnameCleaner.CleanPathnameAndFilename( text?.Trim() ); //---text?.Trim().Replace( '\r', ' ' ).Replace( '\n', ' ' ).Replace( '\t', ' ' );
                if ( text != text_trimmed )
                {
                    this.Text = text_trimmed;
                }
            }
        }
        //*/
        #endregion
        protected override void WndProc( ref Message m )
        {
            switch ( m.Msg )
            {
                case WM_PASTE: 
                    var text         = Clipboard.GetText();
                    var text_trimmed = PathnameCleaner.CleanPathnameAndFilename( text?.Trim() );

                    var t = (Start: this.SelectionStart, Length: this.SelectionLength);
                    var old_text = this.Text;
                    var new_text = old_text.Substring( 0, t.Start ) + text_trimmed + old_text.Substring( t.Start + t.Length );
                    if ( new_text != old_text )
                    {
                        this.Text = new_text;
                        this.SelectionStart = t.Start + text_trimmed.Length;
                    }
                    break;

                default:
                    base.WndProc( ref m );
                    break;
            }
        }
    }
}
