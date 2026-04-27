using System;
using System.Net;
using System.Net.Http;

using m3u8.infrastructure;

using _init_params_ = m3u8.i_m3u8_client_next.init_params;

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    internal interface i_m3u8_client_next_factory
    {
        i_m3u8_client_next Create( IWebProxy webProxy = null );
        i_m3u8_client_next Create( IWebProxy webProxy, in (TimeSpan timeout, int attemptRequestCountByPart) t );
        i_m3u8_client_next Create( IWebProxy webProxy, in TimeSpan timeout, int attemptRequestCountByPart = 10 );
        i_m3u8_client_next Create( in _init_params_ ip );
    }

    /// <summary>
    /// 
    /// </summary>
    internal enum m3u8_client_next_factory_enum_type
    {
        HttpClient,
        HttpMessageInvoker,
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class m3u8_client_next_factory_maker
    {
        public static i_m3u8_client_next_factory get( m3u8_client_next_factory_enum_type m3u8_client_next_factory_type )
        => m3u8_client_next_factory_type switch
        {
            m3u8_client_next_factory_enum_type.HttpClient         => m3u8_client_next_factory__with_HttpClient.Inst,
            m3u8_client_next_factory_enum_type.HttpMessageInvoker => m3u8_client_next_factory__with_HttpMessageInvoker.Inst,
            _ => throw new ArgumentException( m3u8_client_next_factory_type.ToString() )
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
    internal sealed class m3u8_client_next_factory__with_HttpClient : i_m3u8_client_next_factory
    {
        public static i_m3u8_client_next_factory Inst { get; } = new m3u8_client_next_factory__with_HttpClient();
        private m3u8_client_next_factory__with_HttpClient() { }

        public i_m3u8_client_next Create( IWebProxy webProxy = null ) => Create( HttpClientFactory_WithRefCount.Get( webProxy ) );
        public i_m3u8_client_next Create( IWebProxy webProxy, in (TimeSpan timeout, int attemptRequestCountByPart) t ) => Create( webProxy, t.timeout, t.attemptRequestCountByPart );
        public i_m3u8_client_next Create( IWebProxy webProxy, in TimeSpan timeout, int attemptRequestCountByPart = 10 ) => Create( HttpClientFactory_WithRefCount.Get( webProxy, timeout ), attemptRequestCountByPart );
        public i_m3u8_client_next Create( in _init_params_ ip ) => Create( HttpClientFactory_WithRefCount.Get( ip.WebProxy ), ip );

        private static m3u8_client_next Create( in (HttpClient httpClient, IWebProxy webProxy, IDisposable) t, in _init_params_ ip ) => new m3u8_client_next( t, ip );
        private static m3u8_client_next Create( in (HttpClient httpClient, IWebProxy webProxy, IDisposable) t, int attemptRequestCountByPart = 10 )
            => Create( t, new _init_params_() { AttemptRequestCount = Math.Max( attemptRequestCountByPart, 1 ) } );
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class m3u8_client_next_factory__with_HttpMessageInvoker : i_m3u8_client_next_factory
    {
        public static i_m3u8_client_next_factory Inst { get; } = new m3u8_client_next_factory__with_HttpMessageInvoker();
        private m3u8_client_next_factory__with_HttpMessageInvoker() { }

        public i_m3u8_client_next Create( IWebProxy webProxy = null ) => Create( HttpInvokerFactory_WithRefCount.Get( webProxy ) );
        public i_m3u8_client_next Create( IWebProxy webProxy, in (TimeSpan timeout, int attemptRequestCountByPart) t ) => Create( webProxy, t.timeout, t.attemptRequestCountByPart );
        public i_m3u8_client_next Create( IWebProxy webProxy, in TimeSpan timeout, int attemptRequestCountByPart = 10 ) => Create( HttpInvokerFactory_WithRefCount.Get( webProxy/*, timeout*/ ), attemptRequestCountByPart );
        public i_m3u8_client_next Create( in _init_params_ ip ) => Create( HttpInvokerFactory_WithRefCount.Get( ip.WebProxy ), ip );

        private static m3u8_client_next_v2 Create( in (HttpMessageInvoker httpInvoker, IWebProxy webProxy, IDisposable) t, in _init_params_ ip ) => new m3u8_client_next_v2( t, ip );
        private static m3u8_client_next_v2 Create( in (HttpMessageInvoker httpInvoker, IWebProxy webProxy, IDisposable) t, int attemptRequestCountByPart = 10 )
            => Create( t, new _init_params_() { AttemptRequestCount = Math.Max( attemptRequestCountByPart, 1 ) } );
    }

    ///// <summary>
    ///// 
    ///// </summary>
    //internal static class m3u8_client_next_factory
    //{
    //    public static m3u8_client_next Create( IWebProxy webProxy = null ) => Create( HttpClientFactory_WithRefCount.Get( webProxy ) );
    //    public static m3u8_client_next Create( IWebProxy webProxy, in (TimeSpan timeout, int attemptRequestCountByPart) t ) => Create( webProxy, t.timeout, t.attemptRequestCountByPart );
    //    public static m3u8_client_next Create( IWebProxy webProxy, in TimeSpan timeout, int attemptRequestCountByPart = 10 ) => Create( HttpClientFactory_WithRefCount.Get( webProxy, timeout ), attemptRequestCountByPart );
    //    public static m3u8_client_next Create( in _init_params_ ip ) => Create( HttpClientFactory_WithRefCount.Get( ip.WebProxy ), ip );

    //    private static m3u8_client_next Create( in (HttpClient httpClient, IWebProxy webProxy, IDisposable) t, in _init_params_ ip ) => new m3u8_client_next( t, ip );
    //    private static m3u8_client_next Create( in (HttpClient httpClient, IWebProxy webProxy, IDisposable) t, int attemptRequestCountByPart = 10 )
    //        => Create( t, new _init_params_() { AttemptRequestCount = Math.Max( attemptRequestCountByPart, 1 ) } );

    //    public static void ForceClearAndDisposeAll() => HttpClientFactory_WithRefCount.ForceClearAndDisposeAll();
    //}
}
