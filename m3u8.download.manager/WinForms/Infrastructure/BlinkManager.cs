using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal static class BlinkManager
    {
        private static HashSet< Control > _BlinkedControls = new HashSet< Control >();

        private static async void FocusAndBlinkBackColor_Routine( this Control control, Color bgColor, bool setFocus, int millisecondsDelay )
        {
            if( setFocus ) control.Focus();
            if ( _BlinkedControls.Add( control ) )
            {
                var bc = control.BackColor;
                control.BackColor = bgColor;
                await Task.Delay( millisecondsDelay );
                control.BackColor = bc;
                _BlinkedControls.Remove( control );
                //Task.Delay( millisecondsDelay ).ContinueWith( _ => control.BackColor = bc, TaskScheduler.FromCurrentSynchronizationContext() );
            }
        }
        public static void BlinkBackColor( this Control control, Color bgColor, int millisecondsDelay = 330 ) => control.FocusAndBlinkBackColor_Routine( bgColor, setFocus: false, millisecondsDelay );
        public static void FocusAndBlinkBackColor( this Control control, Color bgColor, int millisecondsDelay = 330 ) => control.FocusAndBlinkBackColor_Routine( bgColor, setFocus: true, millisecondsDelay );
        public static void FocusAndBlinkBackColor( this Control control ) => control.FocusAndBlinkBackColor( Color.HotPink );
    }
}
