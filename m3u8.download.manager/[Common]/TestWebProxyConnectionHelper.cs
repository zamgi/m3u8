using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using m3u8.infrastructure;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static partial class TestWebProxyConnectionHelper
    {
        private static async Task< (HttpResponseMessage resp, string webProxyAddressText) > TestConnection_Routine__use_HttpClient(
            web_proxy_info webProxyInfo, CancellationToken ct, Action< string > changeWebProxyAddressAction )
        {
            const string TEST_URL = "https://google.com";

            //var timeout  = TimeSpan.FromSeconds( 10 ); //var (timeout, _) = _SC.GetCreateM3u8ClientParams();
            try
            {
                var webProxyAddressText = webProxyInfo.GetWebProxyAddressText();
                var webProxy = webProxyInfo.CreateWebProxy( webProxyAddressText );
                var (hc, _, d) = HttpClientFactory_WithRefCount.Get( webProxy/*, timeout*/ );
                using ( d )
                {
                    changeWebProxyAddressAction( webProxyAddressText );
                    //await Task.Delay( 5000 );
                    var resp = await hc.GetAsync( TEST_URL, HttpCompletionOption.ResponseHeadersRead, ct );
                    return (resp, webProxyAddressText);
                }
            }
            catch ( OperationCanceledException ) when ( ct.IsCancellationRequested )
            {
                throw;
            }
            catch ( Exception /*ex*/ ) when ( !webProxyInfo.UseWebProxy && (webProxyInfo.UrlType == default) )
            {
                foreach ( var urlType in Enum.GetValues( typeof(WebProxyUrlEnumType) ).Cast< WebProxyUrlEnumType >().Where( x => x != default ) )
                {
                    try
                    {
                        var webProxyAddressText = webProxyInfo.GetWebProxyAddressText( urlType );
                        var webProxy = webProxyInfo.CreateWebProxy( webProxyAddressText );
                        var (hc, _, d) = HttpClientFactory_WithRefCount.Get( webProxy/*, timeout*/ );
                        using ( d )
                        {
                            changeWebProxyAddressAction( webProxyAddressText );
                            //await Task.Delay( 1000 );
                            var resp = await hc.GetAsync( TEST_URL, HttpCompletionOption.ResponseHeadersRead, ct );
                            return (resp, webProxyAddressText);
                        }
                    }
                    catch ( OperationCanceledException ) when ( ct.IsCancellationRequested )
                    {
                        throw;
                    }
                    catch ( Exception /*inner_ex*/ )
                    {
                        ;
                    }
                }

                throw;
            }
        }

        private static async Task< (HttpResponseMessage resp, string webProxyAddressText) > TestConnection_Routine__use_HttpInvoker(
            web_proxy_info webProxyInfo, CancellationToken ct, Action< string > changeWebProxyAddressAction )
        {
            const string TEST_URL = "https://google.com";

            //var timeout = TimeSpan.FromSeconds( 10 ); //var (timeout, _) = _SC.GetCreateM3u8ClientParams();
            var timeout = i_m3u8_client.init_params.DEFAULT_TIMEOUT;
            try
            {
                var webProxyAddressText = webProxyInfo.GetWebProxyAddressText();
                var webProxy = webProxyInfo.CreateWebProxy( webProxyAddressText );
                var (hi, _, d) = HttpInvokerFactory_WithRefCount.Get( webProxy/*, timeout*/ );
                using ( d )
                {
                    changeWebProxyAddressAction( webProxyAddressText );
                    //await Task.Delay( 5000 );
                    using var req = new HttpRequestMessage( HttpMethod.Get, TEST_URL );
                    var resp = await hi.SendAsync_Ex( req, timeout, ct );
                    return (resp, webProxyAddressText);
                }
            }
            catch ( OperationCanceledException ) when ( ct.IsCancellationRequested )
            {
                throw;
            }
            catch ( Exception /*ex*/ ) when ( !webProxyInfo.UseWebProxy && (webProxyInfo.UrlType == default) )
            {
                foreach ( var urlType in Enum.GetValues( typeof(WebProxyUrlEnumType) ).Cast< WebProxyUrlEnumType >().Where( x => x != default ) )
                {
                    try
                    {
                        var webProxyAddressText = webProxyInfo.GetWebProxyAddressText( urlType );
                        var webProxy = webProxyInfo.CreateWebProxy( webProxyAddressText );
                        var (hi, _, d) = HttpInvokerFactory_WithRefCount.Get( webProxy/*, timeout*/ );
                        using ( d )
                        {
                            changeWebProxyAddressAction( webProxyAddressText );
                            //await Task.Delay( 1000 );
                            using var req = new HttpRequestMessage( HttpMethod.Get, TEST_URL );
                            var resp = await hi.SendAsync_Ex( req, timeout, ct );
                            return (resp, webProxyAddressText);
                        }
                    }
                    catch ( OperationCanceledException ) when ( ct.IsCancellationRequested )
                    {
                        throw;
                    }
                    catch ( Exception /*inner_ex*/ )
                    {
                        ;
                    }
                }

                throw;
            }
        }


        public static m3u8_client_next_factory_enum_type m3u8_client_next_factory_type { get; set; }
        public static Task< (HttpResponseMessage resp, string webProxyAddressText) > TestConnection_Routine(
            web_proxy_info webProxyInfo, CancellationToken ct, Action<string  > changeWebProxyAddressAction )
        {
            switch ( m3u8_client_next_factory_type )
            {
                case m3u8_client_next_factory_enum_type.HttpClient:
                    return TestConnection_Routine__use_HttpClient( webProxyInfo, ct, changeWebProxyAddressAction );

                case m3u8_client_next_factory_enum_type.HttpMessageInvoker:
                    return TestConnection_Routine__use_HttpInvoker( webProxyInfo, ct, changeWebProxyAddressAction );

                default:
                    throw (new ArgumentException( m3u8_client_next_factory_type.ToString() ));
            }
        }
    }
}
