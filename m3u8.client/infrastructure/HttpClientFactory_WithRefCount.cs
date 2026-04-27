using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;

#if NETCOREAPP
using System.Net.Security;
#endif

namespace m3u8.infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    internal static class HttpClientFactory_WithRefCount
    {
        /// <summary>
        /// 
        /// </summary>
        public struct init_params
        {
            public TimeSpan? Timeout  { get; set; }
            public IWebProxy WebProxy { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class tuple_t
        {
            /// <summary>
            /// 
            /// </summary>
            public sealed class EqualityComparer : IEqualityComparer< tuple_t >
            {
                public EqualityComparer() { }
                public bool Equals( tuple_t x, tuple_t y ) => (x.Timeout == y.Timeout) && (x.WebProxy.GetAddressUri() == y.WebProxy.GetAddressUri());
                public int GetHashCode( tuple_t t ) => t.Timeout.GetHashCode() ^ (t.WebProxy.GetAddressUri()?.GetHashCode() ?? 0);
            }

            private tuple_t( IWebProxy webProxy, TimeSpan timeout ) => (WebProxy, Timeout) = (webProxy, timeout);
            public tuple_t( in (HttpClient httpClient, IWebProxy WebProxy) x, TimeSpan timeout ) => (HttpClient, WebProxy, Timeout, RefCount) = (x.httpClient, x.WebProxy, timeout, 0);

            public TimeSpan   Timeout    { get; }
            public HttpClient HttpClient { get; }
            public IWebProxy  WebProxy   { get; }
            public int        RefCount   { get; private set; }

            public int IncrementRefCount() => ++RefCount;
            public int DecrementRefCount() => --RefCount;

            public static tuple_t key( IWebProxy webProxy, TimeSpan? timeout ) => new tuple_t( webProxy, timeout.GetValueOrDefault( TimeSpan.Zero ) );
#if DEBUG
            public override string ToString() => $"{Timeout}, (ref: {RefCount})";
#endif
        }

        private static ILRUCache< tuple_t > _LRUCache;

        static HttpClientFactory_WithRefCount() => _LRUCache = LRUCache< tuple_t >.CreateWithLimitMaxValue( 0x10, new tuple_t.EqualityComparer() );

        /// <summary>
        /// 
        /// </summary>
        private struct free_tuple : IDisposable
        {
            private tuple_t _tuple;
            public free_tuple( tuple_t t ) => _tuple = t ?? throw (new ArgumentNullException( nameof(t) ));

            public void Dispose()
            {
                if ( _tuple != null )
                {
                    HttpClientFactory_WithRefCount.Release( _tuple );
                    _tuple = null;
                }
            }
        }

        private static (HttpClient httpClient, IWebProxy webProxy) CreateHttpClient( IWebProxy webProxy, in TimeSpan? timeout )
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

                var h = new SocketsHttpHandler() 
                { 
                    AutomaticDecompression = DecompressionMethods.All,
                    Proxy = webProxy
                };
                h.SslOptions.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                //set_Protocol( h.SslOptions, SslProtocols.Tls   );
                //set_Protocol( h.SslOptions, SslProtocols.Tls11 );
                set_Protocol( h.SslOptions, SslProtocols.Tls12 );
                set_Protocol( h.SslOptions, SslProtocols.Tls13 );
#pragma warning disable CS0618
                set_Protocol( h.SslOptions, SslProtocols.Ssl2  );
                set_Protocol( h.SslOptions, SslProtocols.Ssl3  );
#pragma warning restore CS0618
                if ( _timeout.HasValue )
                {
                    h.ConnectTimeout = _timeout.Value;
                }
                return (h);
            };
            
            /*
            var handler = new HttpClientHandler() 
            { 
                AutomaticDecompression = DecompressionMethods.All, 
                ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                Proxy = webProxy,
            };
            //*/
            var handler    = CreateSocketsHttpHandler( timeout );
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
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                    Proxy = webProxy,
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
            return (httpClient, webProxy);
        }

        public static (HttpClient, IWebProxy, IDisposable) Get( in init_params ip ) => Get( ip.WebProxy, ip.Timeout );
        public static (HttpClient, IWebProxy, IDisposable) Get( IWebProxy webProxy, in TimeSpan? timeout = null )
        {
            var key = tuple_t.key( webProxy, timeout );
            lock ( _LRUCache )
            {
                if ( !_LRUCache.TryGetValue( key, out var t ) )
                {
                    t = new tuple_t( CreateHttpClient( webProxy, in timeout ), key.Timeout );
                    _LRUCache.Add( t );
                }
                t.IncrementRefCount();
                //-----------------------------------------------//
                var removed = (from et in _LRUCache.Skip( 1 )
                               where (et.RefCount <= 0)
                               select et
                              ).ToArray();
                foreach ( var et in removed )
                {
                    _LRUCache.Remove( et );
                    et.HttpClient.Dispose();
                }
                //-----------------------------------------------//
                return (t.HttpClient, t.WebProxy, new free_tuple( t ));
            }
        }

        private static void Release( tuple_t t )
        {
            lock ( _LRUCache )
            {
                var refCount = t.DecrementRefCount();
                if ( (refCount <= 0) && (1 < _LRUCache.Count_2) )
                {
                    var first_t = _LRUCache.First();
                    if ( t != first_t )
                    {
                        _LRUCache.Remove( t );
                        t.HttpClient.Dispose();
                    }
                }
            }
        }

        public static void ForceClearAndDisposeAll()
        {
            lock ( _LRUCache )
            {
                foreach ( var t in _LRUCache )
                {
                    t.HttpClient.Dispose();
                }
                _LRUCache.Clear();
            }
        }
#if M3U8_CLIENT_TESTS
        public static bool Any() => _LRUCache.Any();
        public static (TimeSpan timeout, HttpClient hc, int refCount) GetTop()
        {
            var t = _LRUCache.First();
            return (t.Timeout, t.HttpClient, t.RefCount);
        }
#endif
    }
}
