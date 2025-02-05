using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Xunit;
using m3u8.infrastructure;
using System.Net.Security;
using System.Net;
using System.Security.Authentication;

namespace m3u8.client.tests
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class m3u8_client_tests : IDisposable
    {
        #region [.ctor().]
        private HttpClient _HttpClient;
        private Uri        _M3u8Url;

        public m3u8_client_tests()
        {
            const string SETTINGS_JSON_FILE_NAME = "settings.json";

            IConfigurationRoot configuration;
            try
            {
                configuration = new ConfigurationBuilder()
                                .AddJsonFile( SETTINGS_JSON_FILE_NAME )
                                .Build();
            }
            catch ( FileNotFoundException )
            {
                Debug.WriteLine( $"'{SETTINGS_JSON_FILE_NAME}' not found in project folder." );
                return;
            }

            var m3u8_url = configuration[ "m3u8_url" ];
            if ( m3u8_url.IsNullOrWhiteSpace() )
            {
                Debug.WriteLine( $"'{nameof(m3u8_url)}' not configured in '{SETTINGS_JSON_FILE_NAME}'." );
                return;
            }
            if ( !Uri.TryCreate( m3u8_url, UriKind.RelativeOrAbsolute, out _M3u8Url ) )
            {
                Debug.WriteLine( $"Wrong value of '{nameof( m3u8_url )}' in '{SETTINGS_JSON_FILE_NAME}'." );
                return;
            }

            _HttpClient = CreateHttpClient();
        }
        public void Dispose() => _HttpClient?.Dispose();

        private static HttpClient CreateHttpClient( in TimeSpan? timeout = null )
        {
#if NETCOREAPP
            SocketsHttpHandler CreateSocketsHttpHandler( in TimeSpan? _timeout )
            {
                static void set_Protocol( SslClientAuthenticationOptions sslOptions, SslProtocols protocol )
                {
                    try
                    {
                        sslOptions.EnabledSslProtocols |= protocol;
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( ex );
                    }
                };

                var h = new SocketsHttpHandler() { AutomaticDecompression = DecompressionMethods.All };
                h.SslOptions.RemoteCertificateValidationCallback = ( sender, certificate, chain, sslPolicyErrors ) => true;
                //set_Protocol( h.SslOptions, SslProtocols.Tls   );
                //set_Protocol( h.SslOptions, SslProtocols.Tls11 );
                set_Protocol( h.SslOptions, SslProtocols.Tls12 );
                set_Protocol( h.SslOptions, SslProtocols.Tls13 );
#pragma warning disable CS0618
                set_Protocol( h.SslOptions, SslProtocols.Ssl2 );
                set_Protocol( h.SslOptions, SslProtocols.Ssl3 );
#pragma warning restore CS0618
                if ( _timeout.HasValue )
                {
                    h.ConnectTimeout = _timeout.Value;
                }
                return (h);
            };

            var handler = CreateSocketsHttpHandler( timeout );
            var httpClient = new HttpClient( handler, true );
#else
            HttpClientHandler CreateHttpClientHandler( /*in TimeSpan? _timeout*/ )
            {
                static void set_Protocol( HttpClientHandler h, SslProtocols protocol )
                {
                    try
                    {
                        h.SslProtocols |= protocol;
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( ex );
                    }
                };

                var h = new HttpClientHandler() 
                { 
                    ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true, 
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip 
                };

                //set_Protocol( h, SslProtocols.Tls   );
                //set_Protocol( h, SslProtocols.Tls11 );
                set_Protocol( h, SslProtocols.Tls12 );
                set_Protocol( h, SslProtocols.Tls13 );
#pragma warning disable CS0618
                set_Protocol( h, SslProtocols.Ssl2 );
                set_Protocol( h, SslProtocols.Ssl3 );
#pragma warning restore CS0618
                //if ( _timeout.HasValue )
                //{
                //    h.ConnectTimeout = _timeout.Value;
                //}
                return (h);
            };

            var handler    = CreateHttpClientHandler( /*timeout*/ );
            var httpClient = new HttpClient( handler, true );
#endif
            if ( timeout.HasValue )
            {
                httpClient.Timeout = timeout.Value;
            }
            return (httpClient);
        }
        #endregion

        #region [.Tests.]
        private static void _Assert_( in m3u8_file_t m3u8_file  )
        {
            Assert.NotNull( m3u8_file.BaseAddress );
            Assert.True   ( !m3u8_file.RawText.IsNullOrWhiteSpace() );
            Assert.NotNull( m3u8_file.Parts );
            Assert.True   ( 0 < m3u8_file.Parts.Count );
        }
        private static void _Assert_( in m3u8_part_ts m3u8_part )
        {
            Assert.Null   ( m3u8_part.Error );            
            Assert.NotNull( m3u8_part.Bytes );
            Assert.True   ( 0 < m3u8_part.Bytes.Length );
        }

        [Fact] public async Task DownloadFile()
        {
#if GITHUB_SPECIAL
            try
            {
#endif
            using ( var mc = new m3u8_client( _HttpClient, default ) )
            {
                var m3u8_file = await mc.DownloadFile( _M3u8Url );

                _Assert_( m3u8_file );
            }
#if GITHUB_SPECIAL
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
#endif
        }

        [Fact] public async Task DownloadPart()
        {
#if GITHUB_SPECIAL
            try
            {
#endif
            using ( var mc = new m3u8_client( _HttpClient, default ) )
            {
                var m3u8_file = await mc.DownloadFile( _M3u8Url );

                _Assert_( m3u8_file );

                var m3u8_part = m3u8_file.Parts.First();
                var downloaded_m3u8_part = await mc.DownloadPart( m3u8_part, m3u8_file.BaseAddress );

                _Assert_( downloaded_m3u8_part );
            }
#if GITHUB_SPECIAL
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
#endif
        }

        [Fact] public void ClientFactory()
        {
            Assert.True( !HttpClientFactory_WithRefCount.Any() );

            var timeout_1 = TimeSpan.FromSeconds( 1 );
            var timeout_2 = TimeSpan.FromSeconds( 7 );
            using ( var mc_1_1 = m3u8_client_factory.Create( timeout_1 ) )
            {
                Assert.True( HttpClientFactory_WithRefCount.Any() );

                var t_1 = HttpClientFactory_WithRefCount.GetTop();
                Assert.True( (t_1.hc == mc_1_1.HttpClient) && (t_1.timeout == timeout_1) && (t_1.refCount == 1) );

                using ( var mc_1_2 = m3u8_client_factory.Create( timeout_1 ) )
                {
                    t_1 = HttpClientFactory_WithRefCount.GetTop();
                    Assert.True( (t_1.hc == mc_1_2.HttpClient) && (t_1.timeout == timeout_1) && (t_1.refCount == 2) );

                    Assert.True( mc_1_1.HttpClient == mc_1_2.HttpClient );

                    using ( var mc_2_1 = m3u8_client_factory.Create( timeout_2 ) )
                    using ( var mc_2_2 = m3u8_client_factory.Create( timeout_2 ) )
                    {
                        Assert.True( mc_2_1.HttpClient == mc_2_2.HttpClient );
                        Assert.True( mc_1_1.HttpClient != mc_2_2.HttpClient );

                        var t_2 = HttpClientFactory_WithRefCount.GetTop();
                        Assert.True( (t_2.hc == mc_2_2.HttpClient) && (t_2.timeout == timeout_2) && (t_2.refCount == 2) );
                    }

                    var t_3 = HttpClientFactory_WithRefCount.GetTop();
                    Assert.True( (t_3.timeout == timeout_2) && (t_3.refCount == 0) );
                }
            }

            var t_4 = HttpClientFactory_WithRefCount.GetTop();
            Assert.True( (t_4.timeout == timeout_2) && (t_4.refCount == 0) );

            using ( var mc_2_3 = m3u8_client_factory.Create( timeout_2 ) )
            {
                var t_5 = HttpClientFactory_WithRefCount.GetTop();
                Assert.True( (t_5.hc == mc_2_3.HttpClient) && (t_5.timeout == timeout_2) && (t_5.refCount == 1) );
            }

            var t_6 = HttpClientFactory_WithRefCount.GetTop();
            Assert.True( (t_6.timeout == timeout_2) && (t_6.refCount == 0) );

            HttpClientFactory_WithRefCount.ForceClearAndDisposeAll();
            Assert.True( !HttpClientFactory_WithRefCount.Any() );
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class Extensions
    {
        public static bool IsNullOrEmpty( this string s ) => string.IsNullOrEmpty( s );
        public static bool IsNullOrWhiteSpace( this string s ) => string.IsNullOrWhiteSpace( s );
    }
}
