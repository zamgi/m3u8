using System.Drawing;
using System.Windows.Forms;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal static class DefaultColors
    {
        public static class DGV
        {
            private static Pen _GridLinesPen;
            public static Color GridLinesColor = Color.FromKnownColor( KnownColor.ControlDark );
            public static Pen   GridLinesPen   = (_GridLinesPen ??= new Pen( GridLinesColor ));

            private static Brush _SelectionBackBrush2_Suc;
            public static Color ForeColor_Suc          => Color.DimGray;
            public static Color BackColor_Suc          => Color.White;
            public static Color SelectionForeColor_Suc => Color.White;// FromArgb( 57, 57, 57 );
            public static Color SelectionBackColor_Suc => Color.CadetBlue;// LightSkyBlue;
            public static Brush SelectionBackBrush_Suc => Brushes.CadetBlue;// LightSkyBlue;
            public static Brush SelectionBackBrush2_Suc => (_SelectionBackBrush2_Suc ??= new SolidBrush( Color.FromArgb( 0x0, 0x80, 0x80 ) ));
            public static Pen   SelectionBackPen_Suc   => Pens.CadetBlue;// LightSkyBlue;

            public static Color ForeColor_Err          => Color.OrangeRed;
            public static Color BackColor_Err          => Color.LightYellow;
            public static Color SelectionForeColor_Err => Color.Red;
            public static Color SelectionBackColor_Err => Color.Khaki;

            public static DataGridViewCellStyle Create_Suc( DataGridViewCellStyle cs ) 
                => new DataGridViewCellStyle( cs ) { ForeColor = ForeColor_Suc, BackColor = BackColor_Suc, SelectionForeColor = SelectionForeColor_Suc, SelectionBackColor = SelectionBackColor_Suc };

            public static DataGridViewCellStyle Create_Err( DataGridViewCellStyle cs )
                => new DataGridViewCellStyle( cs ) { ForeColor = ForeColor_Err, BackColor = BackColor_Err, SelectionForeColor = SelectionForeColor_Err, SelectionBackColor = SelectionBackColor_Err };
        }
    }
}
