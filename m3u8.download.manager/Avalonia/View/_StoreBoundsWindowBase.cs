using System;
using System.Diagnostics;

using Avalonia;
using Avalonia.Controls;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class StoreBoundsWindowBase : Window
    {
        private PixelPoint _Position;
        protected override void OnClosing( WindowClosingEventArgs e )
        {
            _Position = this.Position;

            base.OnClosing( e );
        }

        protected void RestoreBounds( string json )
        {
            if ( json.IsNullOrEmpty() ) return;
            try
            {
                var (x, y, width, height, state) = Extensions.FromJSON<(int x, int y, double width, double height, WindowState state)>( json );
                switch ( state )
                {
                    case WindowState.Maximized: this.WindowState = WindowState.Maximized; break;
                    //case WindowState.Minimized: goto default;
                    default:
                        this.WindowState = WindowState.Normal;
                        if ( (double.Epsilon < Math.Abs( width )) && (double.Epsilon < Math.Abs( height )) ) //---if ( (width != default) && (height != default) )
                        {
                            this.Position = new PixelPoint( Math.Max( -10, x ), Math.Max( -10, y ) );
                            //this.ClientSize = new Size( width, height );
                            this.Width = width;
                            this.Height = height;
                        }
                        break;
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        protected (int x, int y, double width, double height, WindowState state) GetBounds() => (x: _Position.X, y: _Position.Y, width: this.Width, height: this.Height, state: this.WindowState);
    }
}
