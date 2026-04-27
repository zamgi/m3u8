using System;
using System.Net;
using System.Net.Http;

using m3u8.infrastructure;

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    public interface i_m3u8_client_factory
    {
        i_m3u8_client Create( IWebProxy webProxy = null );
        i_m3u8_client Create( in (IWebProxy webProxy, TimeSpan timeout, int attemptRequestCountByPart) t );
        i_m3u8_client Create( /*IWebProxy webProxy,*/ in TimeSpan timeout, int attemptRequestCountByPart = 10 );
        i_m3u8_client Create( IWebProxy webProxy, in TimeSpan timeout, int attemptRequestCountByPart = 10 );
        i_m3u8_client Create( in i_m3u8_client.init_params ip );
    }

    /// <summary>
    /// 
    /// </summary>
    public enum m3u8_client_factory_enum_type
    {
        HttpClient,
        HttpMessageInvoker,
    }

    /// <summary>
    /// 
    /// </summary>
    public static class m3u8_client_factory_maker
    {
        public static i_m3u8_client_factory get( m3u8_client_factory_enum_type m3u8_client_factory_type )
        => m3u8_client_factory_type switch
        {
            m3u8_client_factory_enum_type.HttpClient         => m3u8_client_factory__with_HttpClient.Inst,
            m3u8_client_factory_enum_type.HttpMessageInvoker => m3u8_client_factory__with_HttpMessageInvoker.Inst,
            _ => throw new ArgumentException( m3u8_client_factory_type.ToString() )
        };

        public static void ForceClearAndDisposeAll()
        {
            HttpClientFactory_WithRefCount.ForceClearAndDisposeAll();
            HttpInvokerFactory_WithRefCount.ForceClearAndDisposeAll();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class m3u8_client_factory__with_HttpClient : i_m3u8_client_factory
    {
        public static i_m3u8_client_factory Inst { get; } = new m3u8_client_factory__with_HttpClient();
        private m3u8_client_factory__with_HttpClient() { }

        public i_m3u8_client Create( IWebProxy webProxy = null ) => Create( HttpClientFactory_WithRefCount.Get( webProxy ) );
        public i_m3u8_client Create( in (IWebProxy webProxy, TimeSpan timeout, int attemptRequestCountByPart) t ) => Create( t.webProxy, t.timeout, t.attemptRequestCountByPart );
        public i_m3u8_client Create( /*IWebProxy webProxy,*/ in TimeSpan timeout, int attemptRequestCountByPart = 10 ) => Create( webProxy: null, timeout, attemptRequestCountByPart );
        public i_m3u8_client Create( IWebProxy webProxy, in TimeSpan timeout, int attemptRequestCountByPart = 10 ) => Create( HttpClientFactory_WithRefCount.Get( webProxy, timeout ), attemptRequestCountByPart );
        public i_m3u8_client Create( in i_m3u8_client.init_params ip ) => Create( HttpClientFactory_WithRefCount.Get( ip.WebProxy ), ip );

        private static m3u8_client Create( in (HttpClient httpClient, IWebProxy webProxy, IDisposable) t, in i_m3u8_client.init_params ip ) => new m3u8_client( t, ip );
        private static m3u8_client Create( in (HttpClient httpClient, IWebProxy webProxy, IDisposable) t, int attemptRequestCountByPart = 10 )
            => Create( t, new i_m3u8_client.init_params() { AttemptRequestCount = Math.Max( attemptRequestCountByPart, 1 ) } );
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class m3u8_client_factory__with_HttpMessageInvoker : i_m3u8_client_factory
    {
        public static i_m3u8_client_factory Inst { get; } = new m3u8_client_factory__with_HttpMessageInvoker();
        private m3u8_client_factory__with_HttpMessageInvoker() { }

        public i_m3u8_client Create( IWebProxy webProxy = null ) => Create( HttpInvokerFactory_WithRefCount.Get( webProxy ) );
        public i_m3u8_client Create( in (IWebProxy webProxy, TimeSpan timeout, int attemptRequestCountByPart) t ) => Create( t.webProxy, t.timeout, t.attemptRequestCountByPart );
        public i_m3u8_client Create( /*IWebProxy webProxy,*/ in TimeSpan timeout, int attemptRequestCountByPart = 10 ) => Create( webProxy: null, timeout, attemptRequestCountByPart );
        public i_m3u8_client Create( IWebProxy webProxy, in TimeSpan timeout, int attemptRequestCountByPart = 10 ) => Create( HttpInvokerFactory_WithRefCount.Get( webProxy/*, timeout*/ ), attemptRequestCountByPart );
        public i_m3u8_client Create( in i_m3u8_client.init_params ip ) => Create( HttpInvokerFactory_WithRefCount.Get( ip.WebProxy ), ip );

        private static m3u8_client_v2 Create( in (HttpMessageInvoker httpInvoker, IWebProxy webProxy, IDisposable) t, in i_m3u8_client.init_params ip ) => new m3u8_client_v2( t, ip );
        private static m3u8_client_v2 Create( in (HttpMessageInvoker httpInvoker, IWebProxy webProxy, IDisposable) t, int attemptRequestCountByPart = 10 )
            => Create( t, new i_m3u8_client.init_params() { AttemptRequestCount = Math.Max( attemptRequestCountByPart, 1 ) } );
    }

    ///// <summary>
    ///// 
    ///// </summary>
    //public static class m3u8_client_factory
    //{ 
    //    public static m3u8_client Create( IWebProxy webProxy = null ) => Create( HttpClientFactory_WithRefCount.Get( webProxy ) );
    //    public static m3u8_client Create( in (IWebProxy webProxy, TimeSpan timeout, int attemptRequestCountByPart) t ) => Create( t.webProxy, t.timeout, t.attemptRequestCountByPart );
    //    public static m3u8_client Create( /*IWebProxy webProxy,*/ in TimeSpan timeout, int attemptRequestCountByPart = 10 ) => Create( webProxy: null, timeout, attemptRequestCountByPart );
    //    public static m3u8_client Create( IWebProxy webProxy, in TimeSpan timeout, int attemptRequestCountByPart = 10 ) => Create( HttpClientFactory_WithRefCount.Get( webProxy, timeout ), attemptRequestCountByPart );
    //    public static m3u8_client Create( in init_params ip ) => Create( HttpClientFactory_WithRefCount.Get( ip.WebProxy ), ip );

    //    private static m3u8_client Create( in (HttpClient httpClient, IWebProxy webProxy, IDisposable) t, in init_params ip ) => new m3u8_client( t, ip );
    //    private static m3u8_client Create( in (HttpClient httpClient, IWebProxy webProxy, IDisposable) t, int attemptRequestCountByPart = 10 )
    //        => Create( t, new init_params() { AttemptRequestCount = Math.Max( attemptRequestCountByPart, 1 ) } );

    //    public static void ForceClearAndDisposeAll() => HttpClientFactory_WithRefCount.ForceClearAndDisposeAll();
    //}
}
