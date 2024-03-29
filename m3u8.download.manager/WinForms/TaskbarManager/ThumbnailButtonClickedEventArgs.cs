namespace System.Windows.Forms.Taskbar
{
    /// <summary>
    /// Event args for TabbedThumbnailButton.Click event
    /// </summary>
    public class ThumbnailButtonClickedEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a Event Args for the TabbedThumbnailButton.Click event
        /// </summary>
        /// <param name="windowHandle">Window handle for the control/window related to the event</param>
        /// <param name="button">Thumbnail toolbar button that was clicked</param>
        public ThumbnailButtonClickedEventArgs( IntPtr windowHandle, ThumbnailToolBarButton button )
        {
            ThumbnailButton = button;
            WindowHandle = windowHandle;
#if WPF
            WindowsControl = null;
#endif
        }
#if WPF
        /// <summary>
        /// Creates a Event Args for the TabbedThumbnailButton.Click event
        /// </summary>
        /// <param name="windowsControl">WPF Control (UIElement) related to the event</param>
        /// <param name="button">Thumbnail toolbar button that was clicked</param>
        public ThumbnailButtonClickedEventArgs( UIElement windowsControl, ThumbnailToolBarButton button )
        {
            ThumbnailButton = button;
            WindowHandle = IntPtr.Zero;
            WindowsControl = windowsControl;
        }
#endif
        /// <summary>
        /// Gets the Window handle for the specific control/window that is related to this event.
        /// </summary>
        /// <remarks>For WPF Controls (UIElement) the WindowHandle will be IntPtr.Zero. 
        /// Check the WindowsControl property to get the specific control associated with this event.</remarks>
        public IntPtr WindowHandle { get; private set; }
#if WPF
        /// <summary>
        /// Gets the WPF Control (UIElement) that is related to this event. This property may be null
        /// for non-WPF applications.
        /// </summary>
        public UIElement WindowsControl { get; private set; }
#endif
        /// <summary>
        /// Gets the ThumbnailToolBarButton that was clicked
        /// </summary>
        public ThumbnailToolBarButton ThumbnailButton { get; private set; }
    }
}
