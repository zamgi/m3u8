using System.Linq;

using Avalonia.Media;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static class FontHelper
    {
        public static bool TryGetMonospace( out FontFamily fontFamily )
        {
            fontFamily = (from f in FontManager.Current.SystemFonts
                          where (f.Name.EqualIgnoreCase( "Courier New"      )) || //Windows
                                (f.Name.EqualIgnoreCase( "Consolas"         )) || //Windows
                                (f.Name.EqualIgnoreCase( "Book"             )) || //Linux
                                (f.Name.EqualIgnoreCase( "DejaVu Sans Mono" ))    //Linux
                          select f
                         ).FirstOrDefault();
            return (fontFamily != null);
        }
    }
}
