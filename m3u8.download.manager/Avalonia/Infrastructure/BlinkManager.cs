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

        private static async void FocusAndBlinkBackColor_Routine( this TemplatedControl control, IBrush bgColor, bool setFocus, int millisecondsDelay )
        {
            if ( setFocus ) control.Focus();
            if ( _BlinkedControls.Add( control ) )
            {
                var bc = control.Background;
                control.Background = bgColor;
                await Task.Delay( millisecondsDelay );
                control.Background = bc;
                _BlinkedControls.Remove( control );
                //Task.Delay( 330 ).ContinueWith( _ => control.BackColor = bc, TaskScheduler.FromCurrentSynchronizationContext() );
            }
        }
        public static void BlinkBackColor( this TemplatedControl control, IBrush bgColor, int millisecondsDelay = 330 ) => control.FocusAndBlinkBackColor_Routine( bgColor, setFocus: false, millisecondsDelay );
        public static void FocusAndBlinkBackColor( this TemplatedControl control, IBrush bgColor, int millisecondsDelay = 330 ) => control.FocusAndBlinkBackColor_Routine( bgColor, setFocus: true, millisecondsDelay );
        public static void FocusAndBlinkBackColor( this TemplatedControl control ) => control.FocusAndBlinkBackColor( Brushes.HotPink );
    }
}
