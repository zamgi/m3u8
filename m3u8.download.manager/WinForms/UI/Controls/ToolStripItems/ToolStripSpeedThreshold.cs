using System;
using System.Drawing;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ToolStripSpeedThreshold : ToolStripNumericUpDownUC
    {
        public ToolStripSpeedThreshold( EventHandler speedThreshold_ValueChanged, decimal minValue = 0.01M, decimal maxValue = 1_000_000M, int decimalPlaces = 2 )
            : base( captionText: "Mbps", speedThreshold_ValueChanged, minValue, maxValue, decimalPlaces ) { }

        public override Color HighlightBackColor => Color.FromArgb( 255, Color.FromKnownColor( KnownColor.SkyBlue ) ); /*SystemColors.Highlight*/
    }
}
