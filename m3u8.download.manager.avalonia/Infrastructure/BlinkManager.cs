using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal static class BlinkManager
    {
        private static HashSet< TemplatedControl > _BlinkedControls = new HashSet< TemplatedControl >();

        public static async void FocusAndBlinkBackColor( this TemplatedControl control, IBrush bgColor )
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
        public static void FocusAndBlinkBackColor( this TemplatedControl control ) => control.FocusAndBlinkBackColor( Brushes.HotPink );
    }
}
