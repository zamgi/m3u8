using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal static class BlinkManager
    {
        private static HashSet< TextBox > _BlinkedControls = new HashSet< TextBox >();

        public static async void FocusAndBlinkBackColor( this TextBox control, IBrush bgColor )
        {
            control.Focus();
            if ( _BlinkedControls.Add( control ) )
            {
                var bc = control.Background;
                control.Background = bgColor;
                await Task.Delay( 330 );
                control.Background = bc;
                _BlinkedControls.Remove( control );
                //Task.Delay( 330 ).ContinueWith( _ => control.BackColor = bc, TaskScheduler.FromCurrentSynchronizationContext() );
            }
        }
        public static void FocusAndBlinkBackColor( this TextBox control ) => control.FocusAndBlinkBackColor( Brushes.HotPink );
    }
}
