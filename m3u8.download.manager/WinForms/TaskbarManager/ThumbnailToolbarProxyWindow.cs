﻿namespace System.Windows.Forms.Taskbar
{
    /// <summary>
    /// 
    /// </summary>
    internal class ThumbnailToolbarProxyWindow : NativeWindow, IDisposable
    {
        private ThumbnailToolBarButton[] _ThumbnailButtons;
        private readonly IntPtr _InternalWindowHandle;
#if WPF
        internal System.Windows.UIElement WindowsControl { get; set; }
#endif
        internal IntPtr WindowToTellTaskbarAbout => (_InternalWindowHandle != IntPtr.Zero) ? _InternalWindowHandle : Handle;
        internal TaskbarWindow TaskbarWindow { get; set; }

        internal ThumbnailToolbarProxyWindow( IntPtr windowHandle, ThumbnailToolBarButton[] buttons )
        {
            if ( windowHandle == IntPtr.Zero )
            {
                throw (new ArgumentException( "LocalizedMessages.CommonFileDialogInvalidHandle", "windowHandle" ));
            }
            if ( (buttons != null) && (buttons.Length == 0) )
            {
                throw (new ArgumentException( "LocalizedMessages.ThumbnailToolbarManagerNullEmptyArray", "buttons" ));
            }

            _InternalWindowHandle = windowHandle;
            _ThumbnailButtons     = buttons;

            // Set the window handle on the buttons (for future updates)
            Array.ForEach( _ThumbnailButtons, new Action<ThumbnailToolBarButton>( UpdateHandle ) );

            // Assign the window handle (coming from the user) to this native window
            // so we can intercept the window messages sent from the taskbar to this window.
            AssignHandle( windowHandle );
        }
#if WPF
        internal ThumbnailToolbarProxyWindow( System.Windows.UIElement windowsControl, ThumbnailToolBarButton[] buttons )
        {
            if ( windowsControl == null ) throw (new ArgumentNullException( "windowsControl" ));
            if ( (buttons != null) && (buttons.Length == 0) )
            {
                throw (new ArgumentException( "LocalizedMessages.ThumbnailToolbarManagerNullEmptyArray", "buttons" ));
            }

            _internalWindowHandle = IntPtr.Zero;
            WindowsControl = windowsControl;
            _thumbnailButtons = buttons;

            // Set the window handle on the buttons (for future updates)
            Array.ForEach( _thumbnailButtons, new Action<ThumbnailToolBarButton>( UpdateHandle ) );
        }
#endif
        private void UpdateHandle( ThumbnailToolBarButton button )
        {
            button.WindowHandle   = _InternalWindowHandle;
            button.AddedToTaskbar = false;
        }

        protected override void WndProc( ref Message m )
        {
            var handled = TaskbarWindowManager.DispatchMessage( ref m, TaskbarWindow );

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
        /// <summary>
        /// 
        /// </summary>
        ~ThumbnailToolbarProxyWindow() => Dispose( false );

        /// <summary>
        /// Release the native objects.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        public void Dispose( bool disposing )
        {
            if ( disposing )
            {
                // Dispose managed resources

                // Don't dispose the thumbnail buttons
                // as they might be used in another window.
                // Setting them to null will indicate we don't need use anymore.
                _ThumbnailButtons = null;
            }
        }
        #endregion
    }
}
