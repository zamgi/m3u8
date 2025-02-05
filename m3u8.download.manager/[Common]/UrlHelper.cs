using System;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static partial class UrlHelper
    {
        public static bool TryGetM3u8FileUrl( string m3u8FileUrlText, out (Uri m3u8FileUrl, Exception error) t )
        {
            try
            {
                var m3u8FileUrl = new Uri( m3u8FileUrlText );
                if ( (m3u8FileUrl.Scheme != Uri.UriSchemeHttp) && (m3u8FileUrl.Scheme != Uri.UriSchemeHttps) )
                {
                    throw (new ArgumentException( $"Only '{Uri.UriSchemeHttp}' and '{Uri.UriSchemeHttps}' schemes are allowed.", nameof(m3u8FileUrl) ));
                }
                t = (m3u8FileUrl, null);
                return (true);
            }
            catch ( Exception ex )
            {
                t = (null, ex);
                return (false);
            }
        }
    }
}
