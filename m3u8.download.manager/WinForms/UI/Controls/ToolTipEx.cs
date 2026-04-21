using System.Drawing;

namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ToolTipEx : IDisposable
    {
        private IWin32Window _Window;
        private ToolTip      _ToolTip;
        public ToolTipEx() => _ToolTip = new ToolTip();
        public void Dispose() => _ToolTip.Dispose();

        public void Show( IWin32Window window, string text, Point point, int duration )
        {
            _Window = window;
            _ToolTip.Show( text, window, point, duration );
        }
        public void Hide()
        {
            _ToolTip.Hide( _Window );
            _Window = null;
        }
        public void HideIfShown()
        {
            if ( _Window != null )
            {
                Hide();
            }
        }

        public bool IsShown => (_Window != null);
    }
}
