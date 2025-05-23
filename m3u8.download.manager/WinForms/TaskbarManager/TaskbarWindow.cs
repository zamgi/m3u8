﻿namespace System.Windows.Forms.Taskbar
{
    internal class TaskbarWindow : IDisposable
    {
        internal TabbedThumbnailProxyWindow TabbedThumbnailProxyWindow   { get; set; }
        internal ThumbnailToolbarProxyWindow ThumbnailToolbarProxyWindow { get; set; }
        internal bool EnableTabbedThumbnails  { get; set; }
        internal bool EnableThumbnailToolbars { get; set; }

        internal IntPtr UserWindowHandle { get; set; }
#if WPF
        internal UIElement WindowsControl { get; set; }
#endif

        private TabbedThumbnail _TabbedThumbnailPreview;
        internal TabbedThumbnail TabbedThumbnail
        {
            get => _TabbedThumbnailPreview;
            set
            {
                if ( _TabbedThumbnailPreview != null )
                {
                    throw (new InvalidOperationException( "LocalizedMessages.TaskbarWindowValueSet" ));
                }

                TabbedThumbnailProxyWindow = new TabbedThumbnailProxyWindow( value );
                _TabbedThumbnailPreview = value;
                _TabbedThumbnailPreview.TaskbarWindow = this;
            }
        }

        private ThumbnailToolBarButton[] _ThumbnailButtons;
        internal ThumbnailToolBarButton[] ThumbnailButtons
        {
            get => _ThumbnailButtons;
            set
            {
                _ThumbnailButtons = value;
                UpdateHandles();
            }
        }

        private void UpdateHandles()
        {
            foreach ( var button in _ThumbnailButtons )
            {
                button.WindowHandle = WindowToTellTaskbarAbout;
                button.AddedToTaskbar = false;
            }
        }


        // TODO: Verify the logic of this property. There are situations where this will throw InvalidOperationException when it shouldn't.
        internal IntPtr WindowToTellTaskbarAbout
        {
            get
            {
                if ( EnableThumbnailToolbars && !EnableTabbedThumbnails && (ThumbnailToolbarProxyWindow != null) )
                {
                    return (ThumbnailToolbarProxyWindow.WindowToTellTaskbarAbout);
                }
                else if ( !EnableThumbnailToolbars && EnableTabbedThumbnails && (TabbedThumbnailProxyWindow != null) )
                {
                    return (TabbedThumbnailProxyWindow.WindowToTellTaskbarAbout);
                }
                // Bug: What should happen when TabedThumbnailProxyWindow IS null, but it is enabled?
                // This occurs during the TabbedThumbnailProxyWindow constructor at line 31.   
                else if ( EnableTabbedThumbnails && EnableThumbnailToolbars && (TabbedThumbnailProxyWindow != null) )
                {
                    return (TabbedThumbnailProxyWindow.WindowToTellTaskbarAbout);
                }

                throw (new InvalidOperationException());
            }
        }

        internal void SetTitle( string title )
        {
            if ( TabbedThumbnailProxyWindow == null )
            {
                throw (new InvalidOperationException( "LocalizedMessages.TasbarWindowProxyWindowSet" ));
            }
            TabbedThumbnailProxyWindow.Text = title;
        }

        internal TaskbarWindow( IntPtr userWindowHandle, params ThumbnailToolBarButton[] buttons )
        {
            if ( userWindowHandle == IntPtr.Zero )
            {
                throw (new ArgumentException( "LocalizedMessages.CommonFileDialogInvalidHandle", "userWindowHandle" ));
            }
            if ( buttons == null || buttons.Length == 0 )
            {
                throw (new ArgumentException( "LocalizedMessages.TaskbarWindowEmptyButtonArray", "buttons" ));
            }

            // Create our proxy window
            ThumbnailToolbarProxyWindow = new ThumbnailToolbarProxyWindow( userWindowHandle, buttons ) { TaskbarWindow = this };

            // Set our current state
            EnableThumbnailToolbars = true;
            EnableTabbedThumbnails  = false;

            //
            ThumbnailButtons = buttons;
            UserWindowHandle = userWindowHandle;
#if WPF
            WindowsControl = null;
#endif
        }
#if WPF
        internal TaskbarWindow( System.Windows.UIElement windowsControl, params ThumbnailToolBarButton[] buttons )
        {
            if ( windowsControl == null )
            {
                throw (new ArgumentNullException( "windowsControl" ));
            }
            if ( buttons == null || buttons.Length == 0 )
            {
                throw (new ArgumentException( "LocalizedMessages.TaskbarWindowEmptyButtonArray", "buttons" ));
            }

            // Create our proxy window
            ThumbnailToolbarProxyWindow = new ThumbnailToolbarProxyWindow( windowsControl, buttons ) { TaskbarWindow = this };

            // Set our current state
            EnableThumbnailToolbars = true;
            EnableTabbedThumbnails  = false;

            ThumbnailButtons = buttons;
            UserWindowHandle = IntPtr.Zero;
            WindowsControl   = windowsControl;
        }
#endif
        internal TaskbarWindow( TabbedThumbnail preview )
        {
            if ( preview == null ) throw (new ArgumentNullException( "preview" ));

            // Create our proxy window
            // Bug: This is only called in this constructor.  Which will cause the property 
            // to fail if TaskbarWindow is initialized from a different constructor.
            TabbedThumbnailProxyWindow = new TabbedThumbnailProxyWindow( preview );

            // set our current state
            EnableThumbnailToolbars = false;
            EnableTabbedThumbnails = true;

            // copy values
            UserWindowHandle = preview.WindowHandle;
#if WPF
            WindowsControl = preview.WindowsControl;
#endif
            TabbedThumbnail = preview;
        }

        #region IDisposable Members
        ~TaskbarWindow() => Dispose( false );
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
                if ( _TabbedThumbnailPreview != null )
                {
                    _TabbedThumbnailPreview.Dispose();
                }
                _TabbedThumbnailPreview = null;

                if ( ThumbnailToolbarProxyWindow != null )
                {
                    ThumbnailToolbarProxyWindow.Dispose();
                }
                ThumbnailToolbarProxyWindow = null;

                if ( TabbedThumbnailProxyWindow != null )
                {
                    TabbedThumbnailProxyWindow.Dispose();
                }
                TabbedThumbnailProxyWindow = null;

                // Don't dispose the thumbnail buttons as they might be used in another window.
                // Setting them to null will indicate we don't need use anymore.
                _ThumbnailButtons = null;
            }
        }
        #endregion
    }
}
