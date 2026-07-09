using System.Collections.Generic;
using System.Windows.Forms;

namespace m3u8.download.manager.UI.Forms
{
    public partial class LoggerForm : Form, m3u8_processor_next.ILogger
    {
        private int MAX_LINES = 1_000;
        private List<string> _ExistsLines;
        private List<string> _ExistsLines_4_Parts;
        private LoggerForm() => InitializeComponent();
        public LoggerForm( Form owner ) : this()
        {
            this.Owner = owner;
            _ExistsLines = new List<string>( MAX_LINES );
            _ExistsLines_4_Parts = new List<string>( MAX_LINES );
        }
        protected override void OnFormClosing( FormClosingEventArgs e )
        {
            e.Cancel = (e.CloseReason == CloseReason.UserClosing);
            base.OnFormClosing( e );
        }

        public /*async Task*/void Write( string msg )
        {
            if ( this.InvokeRequired )
            {
                var ar = this.BeginInvoke( () => Write( msg ) );
                ar.AsyncWaitHandle.WaitOne();
                //var ar = this.BeginInvoke( () => Write( msg ) );
                //await Task.Factory.FromAsync( ar, _ => { } );
            }
            else
            {
                AppendLineWithLinesProperty( logTextBox, _ExistsLines, msg, MAX_LINES );
            }
        }
        public void Write_4_Parts( string msg )
        {
            if ( this.InvokeRequired )
            {
                var ar = this.BeginInvoke( () => Write_4_Parts( msg ) );
                ar.AsyncWaitHandle.WaitOne();
            }
            else
            {
                AppendLineWithLinesProperty( logTextBox_4_Parts, _ExistsLines_4_Parts, msg, MAX_LINES );
            }
        }

        private static void AppendLineWithLinesProperty( TextBox textBox, List<string> existsLines, string newLine, int maxLines )
        {
            // Формируем новый список
            existsLines.Add( newLine );

            // Удаляем самые старые, если превышен лимит
            while ( maxLines < existsLines.Count )
            {
                existsLines.RemoveAt( 0 );
            }

            // Записываем обратно в TextBox
            textBox.Lines = existsLines.ToArray();

            // Прокрутка в конец
            //---if ( textBox.scr )
            textBox.SelectionStart = textBox.Text.Length;
            textBox.ScrollToCaret();
        }

        private void clearButton_Click( object sender, System.EventArgs e )
        {
            _ExistsLines.Clear();
            logTextBox.Text = null;
        }
        private void clearButton_4_Parts_Click( object sender, System.EventArgs e )
        {
            _ExistsLines_4_Parts.Clear();
            logTextBox_4_Parts.Text = null;
        }
    }
}
