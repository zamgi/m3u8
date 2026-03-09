using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static class ErrorDialog
    {
        private static bool _DontShowAgaing;

        public static void Show( string caption, UnhandledExceptionEventArgs e, bool showQuitButton = false, string continueButtonText = "Ok" )
            => Show( caption, (Exception) e.ExceptionObject, showQuitButton, continueButtonText );
        public static void Show( string caption, Exception ex, bool showQuitButton = false, string continueButtonText = "Ok" )
        {
            if ( _DontShowAgaing )
            {
                Debug.Write( $"{caption}: {ex}" );
                return;
            }

            static IWin32Window get_mainform_or_null()
            {
                var form = Application.OpenForms.Cast< Form >().FirstOrDefault();
                return (((form != null) && !form.IsDisposed && form.IsHandleCreated) ? form : null);
            }

            using ( var d = new ThreadExceptionDialog( ex ) { Text = caption /*AutoSize = true*//*, FormBorderStyle = FormBorderStyle.Sizable*/ } )
            {
                var dontShowAgaingCheckBox = new CheckBox() { Text = "Dont show againg", AutoSize = true, AutoEllipsis = true, Cursor = Cursors.Hand };
                d.Controls.Add( dontShowAgaingCheckBox );
                dontShowAgaingCheckBox.BringToFront();

                var quitButton = d.Controls.OfType< Button >().Where( b => (b.Text == "&Quit") || (b.Text == "Quit") ).FirstOrDefault();
                if ( quitButton != null )
                {
                    quitButton.Visible = showQuitButton;
                    var x = Math.Max( quitButton.Left, d.Width - dontShowAgaingCheckBox.Width - 20 );
                    dontShowAgaingCheckBox.Location = showQuitButton ? new Point( x, quitButton.Top - dontShowAgaingCheckBox.Height - 5 )
                                                                     : new Point( x, (int) (quitButton.Top + (quitButton.Height - dontShowAgaingCheckBox.Height) / 2.0) );

                    var label = d.Controls.OfType< Label >().FirstOrDefault();
                    if ( label?.Text != null )
                    {
                        const string PART_OF_CAPTION_ABOUT_BUTTONS = " If you click Continue, the application will ignore this error and attempt to continue. If you click Quit, the application will close immediately.";
                        label.Text = label.Text.Replace( PART_OF_CAPTION_ABOUT_BUTTONS, string.Empty );
                    }
                }
                else
                {
                    dontShowAgaingCheckBox.Location = new Point( d.Width  - dontShowAgaingCheckBox.Width  - 20, 
                                                                 d.Height - dontShowAgaingCheckBox.Height - 80 );
                }

                var continueButton = d.Controls.OfType< Button >().Where( b => (b.Text == "&Continue") || (b.Text == "Continue") ).FirstOrDefault();
                if ( continueButton != null ) continueButton.Text = continueButtonText;               

                var res = d.ShowDialog( get_mainform_or_null() );
                if ( dontShowAgaingCheckBox.Checked )
                {
                    _DontShowAgaing = true;
                }
                if ( res == DialogResult.Abort )
                {
                    Application.Exit();
                }                
            }
        }
    }
}
