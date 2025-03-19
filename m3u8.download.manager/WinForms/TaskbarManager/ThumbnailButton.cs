﻿using System.Drawing;

namespace System.Windows.Forms.Taskbar
{
    /// <summary>
    /// Represents a taskbar thumbnail button in the thumbnail toolbar.
    /// </summary>
    public sealed class ThumbnailToolBarButton : IDisposable
    {
        private static uint NEXT_ID = 101;
        private ThumbButton _Win32ThumbButton;

        /// <summary>
        /// The event that occurs when the taskbar thumbnail button
        /// is clicked.
        /// </summary>
        public event EventHandler<ThumbnailButtonClickedEventArgs> Click;

        // Internal bool to track whether we should be updating the taskbar 
        // if any of our properties change or if it's just an internal update
        // on the properties (via the constructor)
        private readonly bool internalUpdate = false;

        /// <summary>
        /// Initializes an instance of this class
        /// </summary>
        /// <param name="icon">The icon to use for this button</param>
        /// <param name="tooltip">The tooltip string to use for this button.</param>
        public ThumbnailToolBarButton( Icon icon, string tooltip )
        {
            // Start internal update (so we don't accidently update the taskbar
            // via the native API)
            internalUpdate = true;

            // Set our id
            Id = NEXT_ID;

            // increment the ID
            if ( NEXT_ID == int.MaxValue )
                NEXT_ID = 101; // our starting point
            else
                NEXT_ID++;

            // Set user settings
            Icon = icon;
            Tooltip = tooltip;

            // Defaults
            Enabled = true;

            // Create a native 
            _Win32ThumbButton = new ThumbButton();

            // End our internal update
            internalUpdate = false;
        }

        #region Public properties

        /// <summary>
        /// Gets thumbnail button's id.
        /// </summary>
        internal uint Id { get; set; }

        private Icon icon;
        /// <summary>
        /// Gets or sets the thumbnail button's icon.
        /// </summary>
        public Icon Icon
        {
            get => icon;
            set
            {
                if ( icon != value )
                {
                    icon = value;
                    UpdateThumbnailButton();
                }
            }
        }

        private string tooltip;
        /// <summary>
        /// Gets or sets the thumbnail button's tooltip.
        /// </summary>
        public string Tooltip
        {
            get => tooltip;
            set
            {
                if ( tooltip != value )
                {
                    tooltip = value;
                    UpdateThumbnailButton();
                }
            }
        }

        private bool visible = true;
        /// <summary>
        /// Gets or sets the thumbnail button's visibility. Default is true.
        /// </summary>
        public bool Visible
        {
            get => (Flags & ThumbButtonOptions.Hidden) == 0;
            set
            {
                if ( visible != value )
                {
                    visible = value;

                    if ( value )
                    {
                        Flags &= ~(ThumbButtonOptions.Hidden);
                    }
                    else
                    {
                        Flags |= ThumbButtonOptions.Hidden;
                    }

                    UpdateThumbnailButton();
                }

            }
        }

        private bool enabled = true;
        /// <summary>
        /// Gets or sets the thumbnail button's enabled state. If the button is disabled, it is present, 
        /// but has a visual state that indicates that it will not respond to user action. Default is true.
        /// </summary>
        public bool Enabled
        {
            get => (Flags & ThumbButtonOptions.Disabled) == 0;
            set
            {
                if ( value != enabled )
                {
                    enabled = value;

                    if ( value )
                    {
                        Flags &= ~(ThumbButtonOptions.Disabled);
                    }
                    else
                    {
                        Flags |= ThumbButtonOptions.Disabled;
                    }

                    UpdateThumbnailButton();
                }
            }
        }

        private bool dismissOnClick;
        /// <summary>
        /// Gets or sets the property that describes the behavior when the button is clicked. 
        /// If set to true, the taskbar button's flyout will close immediately. Default is false.
        /// </summary>
        public bool DismissOnClick
        {
            get => (Flags & ThumbButtonOptions.DismissOnClick) == 0;
            set
            {
                if ( value != dismissOnClick )
                {
                    dismissOnClick = value;

                    if ( value )
                    {
                        Flags |= ThumbButtonOptions.DismissOnClick;
                    }
                    else
                    {
                        Flags &= ~(ThumbButtonOptions.DismissOnClick);
                    }

                    UpdateThumbnailButton();
                }
            }
        }

        private bool isInteractive = true;
        /// <summary>
        /// Gets or sets the property that describes whether the button is interactive with the user. Default is true.
        /// </summary>
        /// <remarks>
        /// Non-interactive buttons don't display any hover behavior nor do they raise click events.
        /// They are intended to be used as status icons. This is mostly similar to being not Enabled, 
        /// but the image is not desaturated.
        /// </remarks>
        public bool IsInteractive
        {
            get => (Flags & ThumbButtonOptions.NonInteractive) == 0;
            set
            {
                if ( value != isInteractive )
                {
                    isInteractive = value;

                    if ( value )
                    {
                        Flags &= ~(ThumbButtonOptions.NonInteractive);
                    }
                    else
                    {
                        Flags |= ThumbButtonOptions.NonInteractive;
                    }

                    UpdateThumbnailButton();
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Native flags enum (used when creating the native button)
        /// </summary>
        internal ThumbButtonOptions Flags { get; set; }

        /// <summary>
        /// Native representation of the thumbnail button
        /// </summary>
        internal ThumbButton Win32ThumbButton
        {
            get
            {
                _Win32ThumbButton.Id    = Id;
                _Win32ThumbButton.Tip   = Tooltip;
                _Win32ThumbButton.Icon  = Icon != null ? Icon.Handle : IntPtr.Zero;
                _Win32ThumbButton.Flags = Flags;

                _Win32ThumbButton.Mask = ThumbButtonMask.THB_FLAGS;
                if ( Tooltip != null )
                {
                    _Win32ThumbButton.Mask |= ThumbButtonMask.Tooltip;
                }
                if ( Icon != null )
                {
                    _Win32ThumbButton.Mask |= ThumbButtonMask.Icon;
                }

                return (_Win32ThumbButton);
            }
        }

        /// <summary>
        /// The window manager should call this method to raise the public click event to all
        /// the subscribers.
        /// </summary>
        /// <param name="taskbarWindow">Taskbar Window associated with this button</param>
        internal void FireClick( TaskbarWindow taskbarWindow )
        {
            var evnt = Click;
            if ( (evnt != null) && (taskbarWindow != null) )
            {
                if ( taskbarWindow.UserWindowHandle != IntPtr.Zero )
                {
                    evnt( this, new ThumbnailButtonClickedEventArgs( taskbarWindow.UserWindowHandle, this ) );
                }
#if WPF
                else if ( taskbarWindow.WindowsControl != null )
                {
                    evnt( this, new ThumbnailButtonClickedEventArgs( taskbarWindow.WindowsControl, this ) );
                }
#endif
            }
        }

        /// <summary>
        /// Handle to the window to which this button is for (on the taskbar).
        /// </summary>
        internal IntPtr WindowHandle { get; set; }

        /// <summary>
        /// Indicates if this button was added to the taskbar. If it's not yet added,
        /// then we can't do any updates on it.
        /// </summary>
        internal bool AddedToTaskbar { get; set; }

        internal void UpdateThumbnailButton()
        {
            if ( internalUpdate || !AddedToTaskbar ) return;

            // Get the array of thumbnail buttons in native format
            ThumbButton[] nativeButtons = { Win32ThumbButton };

            var hr = TaskbarList.Instance.ThumbBarUpdateButtons( WindowHandle, 1, nativeButtons );

            if ( !CoreErrorHelper.Succeeded( hr ) ) throw (new ShellException( hr ));
        }

        #endregion

        #region IDisposable Members
        ~ThumbnailToolBarButton() => Dispose( false );
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
                Icon.Dispose();
                tooltip = null;
            }
        }
        #endregion
    }

}
