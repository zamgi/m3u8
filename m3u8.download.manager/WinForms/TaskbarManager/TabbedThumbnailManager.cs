﻿using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.Taskbar
{
    /// <summary>
    /// Represents the main class for adding and removing tabbed thumbnails on the Taskbar
    /// for child windows and controls.
    /// </summary>
    public class TabbedThumbnailManager
    {
        /// <summary>
        /// Internal dictionary to keep track of the user's window handle and its 
        /// corresponding thumbnail preview objects.
        /// </summary>
        private readonly Dictionary<IntPtr, TabbedThumbnail> _TabbedThumbnailCache;
#if WPF
        private readonly Dictionary<UIElement, TabbedThumbnail> _TabbedThumbnailCacheWPF; // list for WPF controls
#endif
        /// <summary>
        /// Internal constructor that creates a new dictionary for keeping track of the window handles
        /// and their corresponding thumbnail preview objects.
        /// </summary>
        internal TabbedThumbnailManager()
        {
            _TabbedThumbnailCache = new Dictionary<IntPtr, TabbedThumbnail>();
#if WPF
            _TabbedThumbnailCacheWPF = new Dictionary<UIElement, TabbedThumbnail>();
#endif
        }

        /// <summary>
        /// Adds a new tabbed thumbnail to the taskbar.
        /// </summary>
        /// <param name="preview">Thumbnail preview for a specific window handle or control. The preview
        /// object can be initialized with specific properties for the title, bitmap, and tooltip.</param>
        /// <exception cref="System.ArgumentException">If the tabbed thumbnail has already been added</exception>
        public void AddThumbnailPreview( TabbedThumbnail preview )
        {
            if ( preview == null ) throw (new ArgumentNullException( "preview" ));

            // UI Element has a windowHandle of zero.
            if ( preview.WindowHandle == IntPtr.Zero )
            {
#if WPF
                if ( _TabbedThumbnailCacheWPF.ContainsKey( preview.WindowsControl ) )
                {
                    throw (new ArgumentException( "LocalizedMessages.ThumbnailManagerPreviewAdded", "preview" ));
                }
                _TabbedThumbnailCacheWPF.Add( preview.WindowsControl, preview );
#endif
            }
            else
            {
                // Regular control with a valid handle
                if ( _TabbedThumbnailCache.ContainsKey( preview.WindowHandle ) )
                {
                    throw (new ArgumentException( "LocalizedMessages.ThumbnailManagerPreviewAdded", "preview" ));
                }
                _TabbedThumbnailCache.Add( preview.WindowHandle, preview );
            }

            TaskbarWindowManager.AddTabbedThumbnail( preview );

            preview.InvalidatePreview(); // Note: Why this here?
        }

        /// <summary>
        /// Gets the TabbedThumbnail object associated with the given window handle
        /// </summary>
        /// <param name="windowHandle">Window handle for the control/window</param>
        /// <returns>TabbedThumbnail associated with the given window handle</returns>
        public TabbedThumbnail GetThumbnailPreview( IntPtr windowHandle )
        {
            if ( windowHandle == IntPtr.Zero ) throw (new ArgumentException( "LocalizedMessages.ThumbnailManagerInvalidHandle", "windowHandle" ));

            return _TabbedThumbnailCache.TryGetValue( windowHandle, out var thumbnail ) ? thumbnail : null;
        }

        /// <summary>
        /// Gets the TabbedThumbnail object associated with the given control
        /// </summary>
        /// <param name="control">Specific control for which the preview object is requested</param>
        /// <returns>TabbedThumbnail associated with the given control</returns>
        public TabbedThumbnail GetThumbnailPreview( Control control )
        {
            if ( control == null ) throw (new ArgumentNullException( "control" ));

            return GetThumbnailPreview( control.Handle );
        }
#if WPF
        /// <summary>
        /// Gets the TabbedThumbnail object associated with the given WPF Window
        /// </summary>
        /// <param name="windowsControl">WPF Control (UIElement) for which the preview object is requested</param>
        /// <returns>TabbedThumbnail associated with the given WPF Window</returns>
        public TabbedThumbnail GetThumbnailPreview( UIElement windowsControl )
        {
            if ( windowsControl == null ) throw (new ArgumentNullException( "windowsControl" ));

            return _TabbedThumbnailCacheWPF.TryGetValue( windowsControl, out var thumbnail ) ? thumbnail : null;
        }
#endif
        /// <summary>
        /// Remove the tabbed thumbnail from the taskbar.
        /// </summary>
        /// <param name="preview">TabbedThumbnail associated with the control/window that 
        /// is to be removed from the taskbar</param>
        public void RemoveThumbnailPreview( TabbedThumbnail preview )
        {
            if ( preview == null ) throw (new ArgumentNullException( "preview" ));

            if ( _TabbedThumbnailCache.ContainsKey( preview.WindowHandle ) )
            {
                RemoveThumbnailPreview( preview.WindowHandle );
            }
#if WPF
            else if ( _TabbedThumbnailCacheWPF.ContainsKey( preview.WindowsControl ) )
            {
                RemoveThumbnailPreview( preview.WindowsControl );
            }
#endif
        }

        /// <summary>
        /// Remove the tabbed thumbnail from the taskbar.
        /// </summary>
        /// <param name="windowHandle">TabbedThumbnail associated with the window handle that 
        /// is to be removed from the taskbar</param>
        public void RemoveThumbnailPreview( IntPtr windowHandle )
        {
            if ( !_TabbedThumbnailCache.ContainsKey( windowHandle ) ) throw (new ArgumentException( "LocalizedMessages.ThumbnailManagerControlNotAdded", "windowHandle" ));

            TaskbarWindowManager.UnregisterTab( _TabbedThumbnailCache[ windowHandle ].TaskbarWindow );

            _TabbedThumbnailCache.Remove( windowHandle );

            var taskbarWindow = TaskbarWindowManager.GetTaskbarWindow( windowHandle, TaskbarProxyWindowType.TabbedThumbnail );

            if ( taskbarWindow != null )
            {
                if ( TaskbarWindowManager._TaskbarWindowList.Contains( taskbarWindow ) )
                {
                    TaskbarWindowManager._TaskbarWindowList.Remove( taskbarWindow );
                }
                taskbarWindow.Dispose();
                taskbarWindow = null;
            }
        }

        /// <summary>
        /// Remove the tabbed thumbnail from the taskbar.
        /// </summary>
        /// <param name="control">TabbedThumbnail associated with the control that 
        /// is to be removed from the taskbar</param>
        public void RemoveThumbnailPreview( Control control )
        {
            if ( control == null ) throw (new ArgumentNullException( "control" ));

            var handle = control.Handle;

            RemoveThumbnailPreview( handle );
        }
#if WPF
        /// <summary>
        /// Remove the tabbed thumbnail from the taskbar.
        /// </summary>
        /// <param name="windowsControl">TabbedThumbnail associated with the WPF Control (UIElement) that 
        /// is to be removed from the taskbar</param>
        public void RemoveThumbnailPreview( UIElement windowsControl )
        {
            if ( windowsControl == null ) throw (new ArgumentNullException( "windowsControl" ));

            if ( !_TabbedThumbnailCacheWPF.ContainsKey( windowsControl ) )
            {
                throw (new ArgumentException( "LocalizedMessages.ThumbnailManagerControlNotAdded", "windowsControl" ));
            }

            TaskbarWindowManager.UnregisterTab( _TabbedThumbnailCacheWPF[ windowsControl ].TaskbarWindow );

            _TabbedThumbnailCacheWPF.Remove( windowsControl );

            var taskbarWindow = TaskbarWindowManager.GetTaskbarWindow( windowsControl, TaskbarProxyWindowType.TabbedThumbnail );

            if ( taskbarWindow != null )
            {
                if ( TaskbarWindowManager._taskbarWindowList.Contains( taskbarWindow ) )
                {
                    TaskbarWindowManager._taskbarWindowList.Remove( taskbarWindow );
                }
                taskbarWindow.Dispose();
                taskbarWindow = null;
            }
        }
#endif
        /// <summary>
        /// Sets the given tabbed thumbnail preview object as being active on the taskbar tabbed thumbnails list.
        /// Call this method to keep the application and the taskbar in sync as to which window/control
        /// is currently active (or selected, in the case of tabbed application).
        /// </summary>
        /// <param name="preview">TabbedThumbnail for the specific control/indow that is currently active in the application</param>
        /// <exception cref="System.ArgumentException">If the control/window is not yet added to the tabbed thumbnails list</exception>
        public void SetActiveTab( TabbedThumbnail preview )
        {
            if ( preview == null ) throw (new ArgumentNullException( "preview" ));

            if ( preview.WindowHandle != IntPtr.Zero )
            {
                if ( !_TabbedThumbnailCache.ContainsKey( preview.WindowHandle ) )
                {
                    throw (new ArgumentException( "LocalizedMessages.ThumbnailManagerPreviewNotAdded", "preview" ));
                }
                TaskbarWindowManager.SetActiveTab( _TabbedThumbnailCache[ preview.WindowHandle ].TaskbarWindow );
            }
#if WPF
            else if ( preview.WindowsControl != null )
            {
                if ( !_TabbedThumbnailCacheWPF.ContainsKey( preview.WindowsControl ) )
                {
                    throw (new ArgumentException( "LocalizedMessages.ThumbnailManagerPreviewNotAdded", "preview" ));
                }
                TaskbarWindowManager.SetActiveTab( _TabbedThumbnailCacheWPF[ preview.WindowsControl ].TaskbarWindow );
            }
#endif
        }

        /// <summary>
        /// Sets the given window handle as being active on the taskbar tabbed thumbnails list.
        /// Call this method to keep the application and the taskbar in sync as to which window/control
        /// is currently active (or selected, in the case of tabbed application).
        /// </summary>
        /// <param name="windowHandle">Window handle for the control/window that is currently active in the application</param>
        /// <exception cref="System.ArgumentException">If the control/window is not yet added to the tabbed thumbnails list</exception>
        public void SetActiveTab( IntPtr windowHandle )
        {
            if ( !_TabbedThumbnailCache.ContainsKey( windowHandle ) )
            {
                throw (new ArgumentException( "LocalizedMessages.ThumbnailManagerPreviewNotAdded", "windowHandle" ));
            }
            TaskbarWindowManager.SetActiveTab( _TabbedThumbnailCache[ windowHandle ].TaskbarWindow );
        }

        /// <summary>
        /// Sets the given Control/Form window as being active on the taskbar tabbed thumbnails list.
        /// Call this method to keep the application and the taskbar in sync as to which window/control
        /// is currently active (or selected, in the case of tabbed application).
        /// </summary>
        /// <param name="control">Control/Form that is currently active in the application</param>
        /// <exception cref="System.ArgumentException">If the control/window is not yet added to the tabbed thumbnails list</exception>
        public void SetActiveTab( Control control )
        {
            if ( control == null ) throw (new ArgumentNullException( "control" ));
            
            SetActiveTab( control.Handle );
        }
#if WPF
        /// <summary>
        /// Sets the given WPF window as being active on the taskbar tabbed thumbnails list.
        /// Call this method to keep the application and the taskbar in sync as to which window/control
        /// is currently active (or selected, in the case of tabbed application).
        /// </summary>
        /// <param name="windowsControl">WPF control that is currently active in the application</param>
        /// <exception cref="System.ArgumentException">If the control/window is not yet added to the tabbed thumbnails list</exception>
        public void SetActiveTab( UIElement windowsControl )
        {
            if ( windowsControl == null )
            {
                throw (new ArgumentNullException( "windowsControl" ));
            }

            if ( !_TabbedThumbnailCacheWPF.ContainsKey( windowsControl ) )
            {
                throw (new ArgumentException( "LocalizedMessages.ThumbnailManagerPreviewNotAdded", "windowsControl" ));
            }
            TaskbarWindowManager.SetActiveTab( _TabbedThumbnailCacheWPF[ windowsControl ].TaskbarWindow );
        }
#endif
        /// <summary>
        /// Determines whether the given preview has been added to the taskbar's tabbed thumbnail list.
        /// </summary>
        /// <param name="preview">The preview to locate on the taskbar's tabbed thumbnail list</param>
        /// <returns>true if the tab is already added on the taskbar; otherwise, false.</returns>
        public bool IsThumbnailPreviewAdded( TabbedThumbnail preview )
        {
            if ( preview == null ) throw (new ArgumentNullException( "preview" ));

            if ( (preview.WindowHandle != IntPtr.Zero) && _TabbedThumbnailCache.ContainsKey( preview.WindowHandle ) )
            {
                return (true);
            }
#if WPF
            else if ( (preview.WindowsControl != null) && _TabbedThumbnailCacheWPF.ContainsKey( preview.WindowsControl ) )
            {
                return (true);
            }
#endif
            return (false);
        }

        /// <summary>
        /// Determines whether the given window has been added to the taskbar's tabbed thumbnail list.
        /// </summary>
        /// <param name="windowHandle">The window to locate on the taskbar's tabbed thumbnail list</param>
        /// <returns>true if the tab is already added on the taskbar; otherwise, false.</returns>
        public bool IsThumbnailPreviewAdded( IntPtr windowHandle )
        {
            if ( windowHandle == IntPtr.Zero ) throw (new ArgumentException( "LocalizedMessages.ThumbnailManagerInvalidHandle", "windowHandle" ));

            return _TabbedThumbnailCache.ContainsKey( windowHandle );
        }

        /// <summary>
        /// Determines whether the given control has been added to the taskbar's tabbed thumbnail list.
        /// </summary>
        /// <param name="control">The preview to locate on the taskbar's tabbed thumbnail list</param>
        /// <returns>true if the tab is already added on the taskbar; otherwise, false.</returns>
        public bool IsThumbnailPreviewAdded( Control control )
        {
            if ( control == null ) throw (new ArgumentNullException( "control" ));

            return _TabbedThumbnailCache.ContainsKey( control.Handle );
        }
#if WPF
        /// <summary>
        /// Determines whether the given control has been added to the taskbar's tabbed thumbnail list.
        /// </summary>
        /// <param name="control">The preview to locate on the taskbar's tabbed thumbnail list</param>
        /// <returns>true if the tab is already added on the taskbar; otherwise, false.</returns>
        public bool IsThumbnailPreviewAdded( UIElement control )
        {
            if ( control == null ) throw (new ArgumentNullException( "control" ));

            return _TabbedThumbnailCacheWPF.ContainsKey( control );
        }
#endif
        /// <summary>
        /// Invalidates all the tabbed thumbnails. This will force the Desktop Window Manager
        /// to not use the cached thumbnail or preview or aero peek and request a new one next time.
        /// </summary>
        /// <remarks>This method should not be called frequently. 
        /// Doing so can lead to poor performance as new bitmaps are created and retrieved.</remarks>
        public void InvalidateThumbnails()
        {
            // Invalidate all the previews currently in our cache.
            // This will ensure we get updated bitmaps next time

            foreach ( var thumbnail in _TabbedThumbnailCache.Values )
            {
                TaskbarWindowManager.InvalidatePreview( thumbnail.TaskbarWindow );
                thumbnail.SetImage( IntPtr.Zero ); // TODO: Investigate this, and why it needs to be called.
            }
#if WPF
            foreach ( var thumbnail in _TabbedThumbnailCacheWPF.Values )
            {
                TaskbarWindowManager.InvalidatePreview( thumbnail.TaskbarWindow );
                thumbnail.SetImage( IntPtr.Zero );
            }
#endif
        }

        /// <summary>
        /// Clear a clip that is already in place and return to the default display of the thumbnail.
        /// </summary>
        /// <param name="windowHandle">The handle to a window represented in the taskbar. This has to be a top-level window.</param>
        public static void ClearThumbnailClip( IntPtr windowHandle ) => TaskbarList.Instance.SetThumbnailClip( windowHandle, IntPtr.Zero );

        /// <summary>
        /// Selects a portion of a window's client area to display as that window's thumbnail in the taskbar.
        /// </summary>
        /// <param name="windowHandle">The handle to a window represented in the taskbar. This has to be a top-level window.</param>
        /// <param name="clippingRectangle">Rectangle structure that specifies a selection within the window's client area,
        /// relative to the upper-left corner of that client area.
        /// <para>If this parameter is null, the clipping area will be cleared and the default display of the thumbnail will be used instead.</para></param>
        public void SetThumbnailClip( IntPtr windowHandle, Rectangle? clippingRectangle )
        {
            if ( clippingRectangle == null )
            {
                ClearThumbnailClip( windowHandle );
                return;
            }

            var rect = new NativeRect()
            {
                Left   = clippingRectangle.Value.Left,
                Top    = clippingRectangle.Value.Top,
                Right  = clippingRectangle.Value.Right,
                Bottom = clippingRectangle.Value.Bottom
            };

            var rectPtr = Marshal.AllocCoTaskMem( Marshal.SizeOf( rect ) );
            try
            {
                Marshal.StructureToPtr( rect, rectPtr, true );
                TaskbarList.Instance.SetThumbnailClip( windowHandle, rectPtr );
            }
            finally
            {
                Marshal.FreeCoTaskMem( rectPtr );
            }
        }

        /// <summary>
        /// Moves an existing thumbnail to a new position in the application's group.
        /// </summary>
        /// <param name="previewToChange">Preview for the window whose order is being changed. 
        /// This value is required, must already be added via AddThumbnailPreview method, and cannot be null.</param>
        /// <param name="insertBeforePreview">The preview of the tab window whose thumbnail that previewToChange is inserted to the left of. 
        /// This preview must already be added via AddThumbnailPreview. If this value is null, the previewToChange tab is added to the end of the list.
        /// </param>
        public static void SetTabOrder( TabbedThumbnail previewToChange, TabbedThumbnail insertBeforePreview )
        {
            if ( previewToChange == null ) throw (new ArgumentNullException( "previewToChange" ));

            var handleToReorder = previewToChange.TaskbarWindow.WindowToTellTaskbarAbout;

            if ( insertBeforePreview == null )
            {
                TaskbarList.Instance.SetTabOrder( handleToReorder, IntPtr.Zero );
            }
            else
            {
                var handleBefore = insertBeforePreview.TaskbarWindow.WindowToTellTaskbarAbout;
                TaskbarList.Instance.SetTabOrder( handleToReorder, handleBefore );
            }
        }
    }
}
