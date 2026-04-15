using System;
using System.Security.Policy;

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
        
        public static bool TryParseWebProxyUrl( string webProxyUrlWithScheme, out web_proxy_info webProxyInfo, out Exception error )
        {
            if ( !webProxyUrlWithScheme.IsNullOrEmpty() )
            {
                var suc = Uri.TryCreate( webProxyUrlWithScheme, UriKind.Absolute, out var url );
                if ( suc )
                {
                    try
                    {
                        url = new Uri( webProxyUrlWithScheme );
                        var is_http = (url.Scheme == Uri.UriSchemeHttp);
                        if ( !is_http && (url.Scheme != web_proxy_info.UriSchemeSocks5) )
                        {
                            throw (new ArgumentException( $"Only '{Uri.UriSchemeHttp}' and '{web_proxy_info.UriSchemeSocks5}' schemes are allowed.", nameof(webProxyUrlWithScheme) ));
                        }
                        webProxyInfo = new web_proxy_info()
                        {
                            //Url         = url,
                            Hostname    = url.Host,
                            Port        = (url.Port != -1) ? url.Port : null,
                            UrlType     = is_http ? WebProxyUrlEnumType.Http : WebProxyUrlEnumType.Socks5,
                            UseWebProxy = true,
                        };
                        error = default;
                        return (true);
                    }
                    catch ( Exception ex )
                    {
                        (webProxyInfo, error) = (/*default*/web_proxy_info.Empty, ex);
                        return (false);
                    }
                }
            }

            (webProxyInfo, error) = (/*default*/web_proxy_info.Empty, default);
            return (false);
        }
        //public static bool TryCreateWebProxyInfo( WebProxyUrlEnumType? urlType, string hostname, int? port, bool use, out web_proxy_info webProxyInfo )
        //{
        //    var suc = urlType.HasValue && !hostname.IsNullOrWhiteSpace();
        //    webProxyInfo = new web_proxy_info()
        //    { 
        //        Hostname    = hostname,
        //        Port        = port,
        //        UrlType     = urlType.GetValueOrDefault(),
        //        UseWebProxy = use,
        //    };
        //    return (suc);
        //}
        //public static string GetWebProxyAddressText( this in (web_proxy_info webProxyInfo, Exception error) res )
        //{
        //    if ( (res.webProxyInfo.Url == null) || (res.error != null) || res.webProxyInfo.Hostname.IsNullOrEmpty() )
        //    {
        //        return (null);
        //    }

        //    return (res.webProxyInfo.GetWebProxyAddressText());
        //}

        public static bool TryParseHostnameAndPort( string hostnameAndPortRaw, out (string Hostname, int? Port, Exception Error) t, bool ignorePortErrors = true )
        {
            if ( !hostnameAndPortRaw.IsNullOrEmpty() )
            {
                try
                {
                    var i = hostnameAndPortRaw.LastIndexOf(':');
                    string hostname;
                    int?   port;
                    if ( i != -1 )
                    {
                        var portText = hostnameAndPortRaw.Substring( i + 1 );
                        var portSuc  = int.TryParse( portText, out var portInt ) && (0 < portInt) && (portInt <= 0XFFFF);
                        if ( ignorePortErrors )
                        {
                            hostname = hostnameAndPortRaw.Substring( 0, i );
                            port     = portSuc ? portInt : null;
                        }
                        else
                        {
                            if ( portSuc )
                            {
                                port     = portInt;
                                hostname = hostnameAndPortRaw.Substring( 0, i );
                            }
                            else
                            {
                                port     = null;
                                hostname = null;
                            }
                        }
                    }
                    else
                    {
                        hostname = hostnameAndPortRaw;
                        port     = null;
                    }

                    t = (hostname, port, default);
                    return (hostname != null);
                }
                catch ( Exception ex )
                {
                    t = (default, default, ex);
                    return (false);
                }
            }

            t = default;
            return (false);
        }

        //private static bool TryParseUri( string addressRaw, out string address, out int port )
        //{
        //    if ( Uri.TryCreate( addressRaw, UriKind.RelativeOrAbsolute, out var uri ) )
        //    {
        //        try
        //        {
        //            address = uri.Host;
        //            port = uri.Port;
        //            return (true);
        //        }
        //        catch ( Exception ex )
        //        {
        //            Debug.WriteLine( ex );
        //        }
        //    }
        //    address = default;
        //    port = default;
        //    return (false);
        //}
    }
}
