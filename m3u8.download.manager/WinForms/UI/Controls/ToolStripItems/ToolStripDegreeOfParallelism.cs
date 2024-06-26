using System;
using System.Drawing;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ToolStripDegreeOfParallelism : ToolStripNumericUpDownUC
    {
        public ToolStripDegreeOfParallelism( EventHandler onValueChanged, decimal minValue = 1M, decimal maxValue = 10_000M, int decimalPlaces = 0 )
            : base( captionText: "(dop)", onValueChanged, minValue, maxValue, decimalPlaces ) { }

        public override Color HighlightBackColor => Color.FromArgb( 255, Color.FromKnownColor( KnownColor.SkyBlue ) ); /*SystemColors.Highlight*/
    }
}
