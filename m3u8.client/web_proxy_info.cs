using System;
using System.Diagnostics;
using System.Net;

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    public enum WebProxyUrlEnumType
    {
        Socks5,
        Http,
    }

    /// <summary>
    /// 
    /// </summary>
    public/*internal*/ readonly struct web_proxy_info
    {
        public const string UriSchemeSocks5 = "socks5";
        
        required public bool UseWebProxy { get; init; }
        required public WebProxyUrlEnumType UrlType { get; init; }
        required public string Hostname { get; init; }
        required public int?   Port     { get; init; }
        public (string Username, string Password) Credentials { get; init; }

        public bool HasCredentials => !string.IsNullOrWhiteSpace( Credentials.Username ) || !string.IsNullOrWhiteSpace( Credentials.Password );
        public string GetWebProxyAddressText() => GetWebProxyAddressText( UrlType, Hostname, Port );
        public string GetWebProxyAddressText( WebProxyUrlEnumType urlType ) => GetWebProxyAddressText( urlType, Hostname, Port );
        public string GetWebProxyAddressTextIfUsed() => UseWebProxy ? GetWebProxyAddressText() : null;
        public ICredentials GetCredentialsIfUsed() => HasCredentials ? new CredentialsImpl( Credentials ) : null;
        public string ToText() => GetWebProxyAddressText() + $", (use={UseWebProxy})" + (HasCredentials ? $", ({Credentials.Username}:{Credentials.Password})" : null);
        public IWebProxy CreateWebProxyIfUsed( bool suppressError = false )
        {
            if ( UseWebProxy )
            {
                if ( suppressError )
                {
                    try
                    {
                        return (new WebProxy( GetWebProxyAddressText() ) { Credentials = GetCredentialsIfUsed() });
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( ex );
                    }
                }
                else
                {
                    return (new WebProxy( GetWebProxyAddressText() ) { Credentials = GetCredentialsIfUsed() });
                }
            }
            return (null);
        }
        public IWebProxy CreateWebProxy() => new WebProxy( GetWebProxyAddressText() ) { Credentials = GetCredentialsIfUsed() };
        public IWebProxy CreateWebProxy( string webProxyAddressText ) => new WebProxy( webProxyAddressText ) { Credentials = GetCredentialsIfUsed() };

        public static string GetWebProxyAddressText( WebProxyUrlEnumType urlType, string hostname, int? port )
        {
            if ( !string.IsNullOrWhiteSpace( hostname ) )
            {
                var scheme = urlType switch
                {
                    WebProxyUrlEnumType.Http   => Uri.UriSchemeHttp,
                    WebProxyUrlEnumType.Socks5 => UriSchemeSocks5,
                    _ => null
                };

                if ( scheme != null )
                {
                    var webProxyAddress = $"{scheme}://{hostname}" + (port.HasValue ? $":{port.Value}" : null);
                    return (webProxyAddress);
                }
            }

            return (null);
        }

        public static web_proxy_info Empty { get; } = new web_proxy_info() { UseWebProxy = false, Hostname = default, Port = default, UrlType = WebProxyUrlEnumType.Socks5 };

        public override string ToString() => ToText();
    }

    /// <summary>
    /// 
    /// </summary>
    internal readonly struct CredentialsImpl : ICredentials
    {
        public CredentialsImpl( in (string Username, string Password) t ) => Credentials = t;
        public (string Username, string Password) Credentials { get; }
        public NetworkCredential GetCredential( Uri uri, string authType ) => new NetworkCredential( Credentials.Username, Credentials.Password );
    }
}
