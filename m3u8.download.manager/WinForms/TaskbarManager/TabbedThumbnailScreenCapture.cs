﻿using System.Drawing;
using System.IO;
#if WPF
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

namespace System.Windows.Forms.Taskbar
{
    /// <summary>
    /// Helper class to capture a control or window as System.Drawing.Bitmap
    /// </summary>
    public static class TabbedThumbnailScreenCapture
    {
        /// <summary>
        /// Captures a screenshot of the specified window at the specified
        /// bitmap size. <para/>NOTE: This method will not accurately capture controls
        /// that are hidden or obstructed (partially or completely) by another control (e.g. hidden tabs,
        /// or MDI child windows that are obstructed by other child windows/forms).
        /// </summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="bitmapSize">The requested bitmap size.</param>
        /// <returns>A screen capture of the window.</returns>        
        public static Bitmap GrabWindowBitmap( IntPtr windowHandle, System.Drawing.Size bitmapSize )
        {
            if ( bitmapSize.Height <= 0 || bitmapSize.Width <= 0 ) return (null);

            var windowDC = IntPtr.Zero;

            try
            {
                windowDC = TabbedThumbnailNativeMethods.GetWindowDC( windowHandle );

                TabbedThumbnailNativeMethods.GetClientSize( windowHandle, out var realWindowSize );

                if ( realWindowSize == System.Drawing.Size.Empty )
                {
                    realWindowSize = new System.Drawing.Size( 200, 200 );
                }

                var size = (bitmapSize == System.Drawing.Size.Empty) ? realWindowSize : bitmapSize;

                Bitmap targetBitmap = null;
                try
                {
                    targetBitmap = new Bitmap( size.Width, size.Height );

                    using ( var targetGr = Graphics.FromImage( targetBitmap ) )
                    {
                        var targetDC = targetGr.GetHdc();
                        uint operation = 0x00CC0020 /*SRCCOPY*/;

                        var ncArea = WindowUtilities.GetNonClientArea( windowHandle );

                        var success = TabbedThumbnailNativeMethods.StretchBlt(
                            targetDC, 0, 0, targetBitmap.Width, targetBitmap.Height,
                            windowDC, ncArea.Width, ncArea.Height, realWindowSize.Width,
                            realWindowSize.Height, operation );

                        targetGr.ReleaseHdc( targetDC );

                        if ( !success ) return (null);

                        return targetBitmap;
                    }
                }
                catch
                {
                    targetBitmap?.Dispose(); 
                    throw;
                }
            }
            finally
            {
                if ( windowDC != IntPtr.Zero )
                {
                    TabbedThumbnailNativeMethods.ReleaseDC( windowHandle, windowDC );
                }
            }
        }
#if WPF
        /// <summary>
        /// Grabs a snapshot of a WPF UIElement and returns the image as Bitmap.
        /// </summary>
        /// <param name="element">Represents the element to take the snapshot from.</param>
        /// <param name="dpiX">Represents the X DPI value used to capture this snapshot.</param>
        /// <param name="dpiY">Represents the Y DPI value used to capture this snapshot.</param>
        /// <param name="width">The requested bitmap width.</param>
        /// <param name="height">The requested bitmap height.</param>
        /// <returns>Returns the bitmap (PNG format).</returns>
        public static Bitmap GrabWindowBitmap( UIElement element, int dpiX, int dpiY, int width, int height )
        {
            // Special case for HwndHost controls
            var host = element as HwndHost;
            if ( host != null )
            {
                var handle = host.Handle;
                return GrabWindowBitmap( handle, new System.Drawing.Size( width, height ) );
            }

            var bounds = VisualTreeHelper.GetDescendantBounds( element );

            // create the renderer.
            if ( bounds.Height == 0 || bounds.Width == 0 )
            {
                return (null);    // 0 sized element. Probably hidden
            }

            var rendertarget = new RenderTargetBitmap( (int) (bounds.Width * dpiX / 96.0),
             (int) (bounds.Height * dpiY / 96.0), dpiX, dpiY, PixelFormats.Default );

            var dv = new DrawingVisual();
            using ( var ctx = dv.RenderOpen() )
            {
                var vb = new VisualBrush( element );
                ctx.DrawRectangle( vb, null, new Rect( new System.Windows.Point(), bounds.Size ) );
            }

            rendertarget.Render( dv );

            BitmapEncoder bmpe = new PngBitmapEncoder();
            bmpe.Frames.Add( BitmapFrame.Create( rendertarget ) );

            Bitmap bmp;
            // Create a MemoryStream with the image.            
            using ( var fl = new MemoryStream() )
            {
                bmpe.Save( fl );
                fl.Position = 0;
                bmp = new Bitmap( fl );
            }

            return (Bitmap) bmp.GetThumbnailImage( width, height, null, IntPtr.Zero );
        }
#endif
        /// <summary>
        /// Resizes the given bitmap while maintaining the aspect ratio.
        /// </summary>
        /// <param name="originalHBitmap">Original/source bitmap</param>
        /// <param name="newWidth">Maximum width for the new image</param>
        /// <param name="maxHeight">Maximum height for the new image</param>
        /// <param name="resizeIfWider">If true and requested image is wider than the source, the new image is resized accordingly.</param>
        internal static Bitmap ResizeImageWithAspect( IntPtr originalHBitmap, int newWidth, int maxHeight, bool resizeIfWider )
        {
            var originalBitmap = Bitmap.FromHbitmap( originalHBitmap );
            try
            {
                if ( resizeIfWider && originalBitmap.Width <= newWidth )
                {
                    newWidth = originalBitmap.Width;
                }

                var newHeight = originalBitmap.Height * newWidth / originalBitmap.Width;

                if ( newHeight > maxHeight ) // Height resize if necessary
                {
                    newWidth = originalBitmap.Width * maxHeight / originalBitmap.Height;
                    newHeight = maxHeight;
                }

                // Create the new image with the sizes we've calculated
                return ((Bitmap) originalBitmap.GetThumbnailImage( newWidth, newHeight, null, IntPtr.Zero ));
            }
            finally
            {
                originalBitmap.Dispose();
            }
        }
    }
}
