using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    internal sealed class FormData
    {
        [DataMember(Name="r")]  public Rectangle       Rect        { get; set; }
        [DataMember(Name="ws")] public FormWindowState WindowState { get; set; }

        public FormData()
        {
            Rect        = default;
            WindowState = FormWindowState.Normal;
        }
        internal FormData( Rectangle r, FormWindowState ws ) : this()
        {
            Rect        = r;
            WindowState = ws;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class FormPositionStorer
    {
        private const int MIN_HEIGHT = 200;
        private const int MIN_WIDTH  = 250;

        private static void CorrectPosition( this Form form, int? minWidth = null, int? minHeight = null )
        {
            Rectangle workingArea = Screen.GetWorkingArea( form );
            if ( form.Top < workingArea.Top )
            {
                form.Top = workingArea.Top;
            }
            if ( form.Left < workingArea.Left )
            {
                form.Left = workingArea.Left;
            }
            if ( workingArea.Bottom < form.Bottom )
            {
                form.Height = workingArea.Bottom - form.Top;
            }
            if ( workingArea.Right < form.Right )
            {
                form.Width = workingArea.Right - form.Left;
            }

            var min_height = minHeight.GetValueOrDefault( MIN_HEIGHT );
            if ( form.Height < min_height )
            {
                form.Top    -= min_height - form.Height;
                form.Height += min_height - form.Height;
                form.CorrectPosition( minWidth, minHeight );
            }

            var min_width = minWidth.GetValueOrDefault( MIN_WIDTH );
            if ( form.Width < min_width )
            {
                form.Left  -= min_width - form.Width;
                form.Width += min_width - form.Width;
                form.CorrectPosition( minWidth, minHeight );
            }
        }

        public static void LoadOnlyHeight( Form form, string formPositionJson )
        {
            if ( string.IsNullOrWhiteSpace( formPositionJson ) ) return;

            try
            {
                //---------------------------------------------------------------------------//
                var d = Extensions.FromJSON< FormData >( formPositionJson );
                //---------------------------------------------------------------------------//

                #region [.Form position.]
                if ( d.WindowState == FormWindowState.Normal )
                {
                    //form.Bounds = d.Rect;
                    form.Bounds = new Rectangle( form.Bounds.Location, new Size( form.Bounds.Width, d.Rect.Height ) );
                }
                else
                {
                    form.SetBounds( d.Rect.X, d.Rect.Y, d.Rect.Width, d.Rect.Height, BoundsSpecified.Height );
                }
                if ( d.WindowState != FormWindowState.Minimized )
                {
                    form.WindowState = d.WindowState;
                }

                if ( form.WindowState == FormWindowState.Normal )
                {
                    form.CorrectPosition();
                }
                #endregion
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        public static void LoadAllExcludeHeight( Form form, string formPositionJson, int? minWidth = null, int? minHeight = null )
        {
            if ( string.IsNullOrWhiteSpace( formPositionJson ) ) return;

            try
            {
                //---------------------------------------------------------------------------//
                var d = Extensions.FromJSON< FormData >( formPositionJson );
                //---------------------------------------------------------------------------//

                #region [.Form position.]
                if ( d.WindowState == FormWindowState.Normal )
                {
                    //form.Bounds = d.Rect;
                    form.Bounds = new Rectangle( form.Bounds.Location, new Size( d.Rect.Width, form.Bounds.Height ) );
                }
                else
                {
                    form.SetBounds( d.Rect.X, d.Rect.Y, d.Rect.Width, d.Rect.Height, BoundsSpecified.Location | BoundsSpecified.Width );
                }
                if ( d.WindowState != FormWindowState.Minimized )
                {
                    form.WindowState = d.WindowState;
                }

                if ( form.WindowState == FormWindowState.Normal )
                {
                    form.CorrectPosition( minWidth, minHeight );
                }
                #endregion
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        public static void Load( Form form, string formPositionJson )
        {
            if ( string.IsNullOrWhiteSpace( formPositionJson ) ) return;

            try
            {
                //---------------------------------------------------------------------------//
                var d = Extensions.FromJSON< FormData >( formPositionJson ); //---var d = JsonConvert.DeserializeObject< FormData >( formPositionJson );
                //---------------------------------------------------------------------------//

                #region [.Form position.]
                if ( d.WindowState == FormWindowState.Normal )
                {
                    form.Bounds = d.Rect;
                }
                else
                {
                    form.SetBounds( d.Rect.X, d.Rect.Y, d.Rect.Width, d.Rect.Height, BoundsSpecified.All );
                }
                if ( d.WindowState != FormWindowState.Minimized )
                {
                    form.WindowState = d.WindowState;
                }
                if ( form.WindowState == FormWindowState.Normal )
                {
                    form.CorrectPosition();
                }
                #endregion
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }

        public static string Save( Form form, int? height = null )
        {
            try
            {
                var rc = ((form.WindowState == FormWindowState.Normal) ? form.Bounds : form.RestoreBounds);
                if ( height.HasValue )
                {
                    rc.Height = height.Value;
                }
                var d = new FormData( rc, form.WindowState );

                //------------------------------------------------------------------//
                var json = d.ToJSON();
                return (json);
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
                return (null);
            }
        }
    }
}
