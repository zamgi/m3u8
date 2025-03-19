using System.Drawing;

namespace System.Windows.Forms.Taskbar
{
    internal sealed class TabbedThumbnailProxyWindow : Form, IDisposable
    {
        internal TabbedThumbnailProxyWindow( TabbedThumbnail preview )
        {
            TabbedThumbnail = preview;
            Size = new Size( 1, 1 );

            if ( !string.IsNullOrEmpty( preview.Title ) )
            {
                Text = preview.Title;
            }
#if WPF
            if ( preview.WindowsControl != null )
            {
                WindowsControl = preview.WindowsControl;
            }
#endif
        }

        internal TabbedThumbnail TabbedThumbnail { get; private set; }
#if WPF
        internal UIElement WindowsControl { get; private set; }
#endif
        internal IntPtr WindowToTellTaskbarAbout => Handle;

        protected override void WndProc( ref Message m )
        {
            var handled = false;

            if ( TabbedThumbnail != null )
            {
                handled = TaskbarWindowManager.DispatchMessage( ref m, TabbedThumbnail.TaskbarWindow );
            }

            // If it's a WM_Destroy message, then also forward it to the base class (our native window)
            if ( (m.Msg == (int) WindowMessage.Destroy) ||
                 (m.Msg == (int) WindowMessage.NCDestroy) ||
                ((m.Msg == (int) WindowMessage.SystemCommand) && (((int) m.WParam) == TabbedThumbnailNativeMethods.ScClose)) )
            {
                base.WndProc( ref m );
            }
            else if ( !handled )
            {
                base.WndProc( ref m );
            }
        }

        #region IDisposable Members
        ~TabbedThumbnailProxyWindow() => Dispose( false );
        void IDisposable.Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                TabbedThumbnail?.Dispose();
                TabbedThumbnail = null;
#if WPF
                WindowsControl = null;
#endif
            }

            base.Dispose( disposing );
        }
        #endregion
    }
}