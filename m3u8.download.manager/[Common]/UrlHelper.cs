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
        
        public const string UriSchemeSocks5 = "socks5";
        /// <summary>
        /// 
        /// </summary>
        public enum WebProxyUrlEnumType
        {
            Http,
            Socks5,
        }
        /// <summary>
        /// 
        /// </summary>
        public readonly struct ParseWebProxyUrlResult
        {
            public Uri       Url     { get; init; }
            public string    Address { get; init; }
            public int?      Port    { get; init; }
            public WebProxyUrlEnumType UrlType { get; init; }
            public Exception Error   { get; init; }
        }
        public static bool TryParseWebProxyUrl( string webProxyUrlWithScheme, out ParseWebProxyUrlResult res )
        {
            if ( !webProxyUrlWithScheme.IsNullOrEmpty() )
            {
                try
                {
                    var url = new Uri( webProxyUrlWithScheme );
                    var is_http = (url.Scheme == Uri.UriSchemeHttp);
                    if ( !is_http && (url.Scheme != UriSchemeSocks5) )
                    {
                        throw (new ArgumentException( $"Only '{Uri.UriSchemeHttp}' and '{UriSchemeSocks5}' schemes are allowed.", nameof(url) ));
                    }
                    res = new ParseWebProxyUrlResult()
                    {
                        Url     = url,
                        Address = url.Host,
                        Port    = (url.Port != -1) ? url.Port : null,
                        UrlType = is_http ? WebProxyUrlEnumType.Http : WebProxyUrlEnumType.Socks5,
                    };
                    return (true);
                }
                catch ( Exception ex )
                {
                    res = new ParseWebProxyUrlResult() { Error = ex };
                    return (false);
                }
            }

            res = default;
            return (false);
        }
        public static string GetWebProxyAddressText( this in ParseWebProxyUrlResult res )
        {
            if ( (res.Url == null) || (res.Error != null) || res.Address.IsNullOrEmpty() )
            {
                return (null);
            }

            #region comm
            //var scheme = res.UrlType switch
            //{
            //    WebProxyUrlEnumType.Http => Uri.UriSchemeHttp,
            //    WebProxyUrlEnumType.Socks5 => UriSchemeSocks5,
            //    _ => res.Url.Scheme
            //};

            //var webProxyAddress = $"{scheme}://{res.Address}" + (res.Port.HasValue ? $":{res.Port.Value}" : null);
            //return (webProxyAddress); 
            #endregion

            return (GetWebProxyAddressText( res.UrlType, res.Address, res.Port ));
        }
        public static string GetWebProxyAddressText( WebProxyUrlEnumType urlType, string address, int? port )
        {
            if ( !address.IsNullOrWhiteSpace() )
            {
                var scheme = urlType switch
                {
                    WebProxyUrlEnumType.Http => Uri.UriSchemeHttp,
                    WebProxyUrlEnumType.Socks5 => UriSchemeSocks5,
                    _ => null
                };

                if ( scheme != null )
                {
                    var webProxyAddress = $"{scheme}://{address}" + (port.HasValue ? $":{port.Value}" : null);
                    return (webProxyAddress); 
                }
            }

            return (null);
        }

        public static bool TryParseHostnameAndPort( string hostnameAndPortRaw, out (string Address, int? Port, Exception Error) t, bool ignorePortErrors = true )
        {
            if ( !hostnameAndPortRaw.IsNullOrEmpty() )
            {
                try
                {
                    var i = hostnameAndPortRaw.LastIndexOf(':');
                    string address;
                    int? port;
                    if ( i != -1 )
                    {
                        var portText = hostnameAndPortRaw.Substring( i + 1 );
                        var portSuc  = int.TryParse( portText, out var portInt ) && (0 < portInt) && (portInt <= 0XFFFF);
                        if ( ignorePortErrors )
                        {
                            address = hostnameAndPortRaw.Substring( 0, i );
                            port    = portSuc ? portInt : null;
                        }
                        else
                        {
                            if ( portSuc )
                            {
                                port    = portInt;
                                address = hostnameAndPortRaw.Substring( 0, i );
                            }
                            else
                            {
                                port    = null;
                                address = null;
                            }
                        }
                    }
                    else
                    {
                        address = hostnameAndPortRaw;
                        port    = null;
                    }

                    t = (address, port, default);
                    return (address != null);
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
