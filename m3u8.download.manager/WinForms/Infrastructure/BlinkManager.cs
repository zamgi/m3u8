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

        public static async void FocusAndBlinkBackColor( this Control control, Color bgColor )
        {
            control.Focus();
            if ( _BlinkedControls.Add( control ) )
            {
                var bc = control.BackColor;
                control.BackColor = bgColor;
                await Task.Delay( 330 );
                control.BackColor = bc;
                _BlinkedControls.Remove( control );
                //Task.Delay( 330 ).ContinueWith( _ => control.BackColor = bc, TaskScheduler.FromCurrentSynchronizationContext() );
            }
        }
        public static void FocusAndBlinkBackColor( this Control control ) => control.FocusAndBlinkBackColor( Color.HotPink );
    }
}
