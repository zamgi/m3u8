using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class M3u8FileResultTextBox : M3u8FileResultUCBase
    {
        #region [.fields.]
        private const string FAILED_TEXT_LINE = "\r\n FAILED-------------------------------------------------------------------------------------------------------------------------FAILED \r\n";
        #endregion

        #region [.ctor().]
        public M3u8FileResultTextBox() => InitializeComponent();
        #endregion

        #region [.public override methods.]
        public override bool IsVerticalScrollBarVisible => true;
        public override void AdjustColumnsWidthSprain() { }
        public override void AdjustRowsHeight() { }
        public override void SetFocus() => textBox.Focus();
        public override void Clear()
        {
            textBox.Clear();
            textBox.ForeColor = Color.FromKnownColor( KnownColor.WindowText );
        }

        public override void Output( m3u8_file_t m3u8File, IEnumerable< string > lines )
        {
            textBox.Lines = lines.ToArray();
            textBox.AppendText( $"\r\n\r\n patrs count: {m3u8File.Parts.Count}\r\n" );
        }

        public override void AppendEmptyLine() => textBox.AppendText( Environment.NewLine );
        public override IRowHolder AppendRequestText( string requestText, bool ensureVisible = true )
        {            
            textBox.AppendText( requestText );
            if ( requestText.IsNullOrEmpty() || (requestText.Last() != '\n') )
            {
                textBox.AppendText( Environment.NewLine );
            }
            return (null);
        }

        public override void AppendRequestErrorText( Exception ex ) => AppendRequestErrorText( ex.ToString() );
        public override void AppendRequestErrorText( string errorText )
        {
            textBox.ForeColor = Color.Red;
            textBox.AppendText( FAILED_TEXT_LINE );
            textBox.AppendText( errorText.Trim( '\r', '\n' ) );
            textBox.AppendText( FAILED_TEXT_LINE );
        }        
        public override void AppendRequestAndResponseErrorText( string requestText, Exception responseError )
        {
            AppendRequestText( requestText );
            AppendRequestErrorText( responseError );
        }

        public override void SetResponseErrorText( IRowHolder holder, Exception ex ) => AppendRequestErrorText( ex );
        public override void SetResponseReceivedText( IRowHolder holder, string receivedText ) { }
        #endregion
    }
}
