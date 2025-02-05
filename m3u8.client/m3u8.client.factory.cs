using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Authentication;



#if NETCOREAPP
using System.Net.Security;
using System.Security.Authentication;
#endif

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ICollectionDebugView< T >
    {
        private ICollection< T > _Collection;
        public ICollectionDebugView( ICollection< T > collection ) => _Collection = collection ?? throw new ArgumentNullException( nameof(collection) );

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                var items = new T[ _Collection.Count ];
                _Collection.CopyTo( items, 0 );
                return (items);
            }
        }
    }

    /// <summary>
    /// LRU (least recently used) cache
    /// </summary>
    internal interface ILRUCache< T > : ICollection< T >, IReadOnlyCollection< T >
    {
        int Limit { get; set; }
        int Count_2 { get; }
        bool TryGetValue( T equalValue, out T actualValue );
    }

    /// <summary>
    /// 
    /// </summary>
    [DebuggerTypeProxy( typeof(ICollectionDebugView<>) )]
    [DebuggerDisplay("Count = {Count}")]
    internal sealed class LRUCache< T > : ILRUCache< T >, ICollection< T >, IReadOnlyCollection< T >
    {
        /// <summary>
        /// 
        /// </summary>
        private sealed class LinkedListNode_EqualityComparer : IEqualityComparer< LinkedListNode< T > >
        {
            private IEqualityComparer< T > _Comparer;
            public LinkedListNode_EqualityComparer( IEqualityComparer< T > comparer ) => _Comparer = comparer ?? EqualityComparer< T >.Default;

            public bool Equals( LinkedListNode< T > x, LinkedListNode< T > y ) => _Comparer.Equals( x.Value, y.Value );
            public int GetHashCode( LinkedListNode< T > obj ) => _Comparer.GetHashCode( obj.Value );
        }

        #region [.fields.]
        private HashSet< LinkedListNode< T > > _HashSet;
        private LinkedList< T >                _LinkedList;
        private int                            _Limit;
        #endregion

        #region [.ctor().]
        public LRUCache( int limit, IEqualityComparer< T > comparer = null )
        {
            this.Limit  = limit;
            _HashSet    = new HashSet< LinkedListNode< T > >( limit, new LinkedListNode_EqualityComparer( comparer ) );
            _LinkedList = new LinkedList< T >();
        }

        private LRUCache() { }
        public static LRUCache< T > CreateWithLimitMaxValue( int capacity, IEqualityComparer< T > comparer = null )
            => new LRUCache< T >()
            {
                Limit       = int.MaxValue,
                _HashSet    = new HashSet< LinkedListNode< T > >( capacity, new LinkedListNode_EqualityComparer( comparer ) ),
                _LinkedList = new LinkedList< T >()
            };
        #endregion

        public int Limit
        {
            get => _Limit;
            set
            {
                if ( value <= 0 ) throw (new ArgumentException( nameof(Limit) ));

                _Limit = value;
            }
        }
        public bool TryGetValue( T equalValue, out T actualValue )
        {
            if ( _HashSet.TryGetValue( ToNode( equalValue ), out var node ) )
            {
                MoveToFirst( node );

                actualValue = node.Value;
                return (true);
            }
            actualValue = default;
            return (false);
        }

        [M(O.AggressiveInlining)] private static LinkedListNode< T > ToNode( T item ) => new LinkedListNode< T >( item );
        [M(O.AggressiveInlining)] private void AddFirst( LinkedListNode< T > node )
        {
            _LinkedList.AddFirst( node );
            _HashSet   .Add( node );
        }
        [M(O.AggressiveInlining)] private void Remove( LinkedListNode< T > node )
        {
            _LinkedList.Remove( node );
            _HashSet   .Remove( node );
        }
        [M(O.AggressiveInlining)] private void MoveToFirst( LinkedListNode< T > node )
        {
            _LinkedList.Remove  ( node );
            _LinkedList.AddFirst( node );
        }

        public int Count => _LinkedList.Count;
        public int Count_2 => _LinkedList.Count;
        public bool IsReadOnly => false;

        public void Add( T item )
        {
            var temp = ToNode( item );
            
            if ( _HashSet.TryGetValue( temp, out var existsNode ) )
            {
                Remove( existsNode );
                AddFirst( temp );
            }
            else
            {
                AddFirst( temp );

                if ( _Limit < _HashSet.Count )
                {
                    Remove( _LinkedList.Last );
                }
            }
        }
        public bool Remove( T item )
        {
            var temp = ToNode( item );
            if ( _HashSet.TryGetValue( temp, out var existsNode ) )
            {
                Remove( existsNode );
                return (true);
            }
            return (false);
        }
        public bool Contains( T item ) => _HashSet.Contains( ToNode( item ) );
        public void Clear()
        {
            _HashSet   .Clear();
            _LinkedList.Clear();
        }

        public IEnumerator< T > GetEnumerator()
        {
            foreach ( var t in _LinkedList )
            {
                yield return (t);
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void CopyTo( T[] array, int arrayIndex )
        {
            foreach ( var t in _LinkedList )
            {
                array[ arrayIndex++ ] = t;
            }
        }
#if DEBUG
        public override string ToString() => $"Count: {Count}";
#endif
    }
    //--------------------------------------------------//

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
            public TimeSpan? Timeout { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class tuple_t
        {
            /// <summary>
            /// 
            /// </summary>
            public struct EqualityComparer : IEqualityComparer< tuple_t >
            {
                public bool Equals( tuple_t x, tuple_t y ) => (x.Timeout == y.Timeout);
                public int GetHashCode( tuple_t obj ) => obj.Timeout.GetHashCode();
            }

            private tuple_t( TimeSpan timeout ) => Timeout = timeout;
            public tuple_t( TimeSpan timeout, HttpClient httpClient ) => (Timeout, HttpClient, RefCount) = (timeout, httpClient, 0);

            public TimeSpan   Timeout    { get; }
            public HttpClient HttpClient { get; }
            public int        RefCount   { get; private set; }

            public int IncrementRefCount() => ++RefCount;
            public int DecrementRefCount() => --RefCount;

            public static tuple_t key( TimeSpan? timeout ) => new tuple_t( timeout.GetValueOrDefault( TimeSpan.Zero ) );
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

        private static HttpClient CreateHttpClient( in TimeSpan? timeout )
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

        public static (HttpClient, IDisposable) Get( in init_params ip ) => Get( ip.Timeout );
        public static (HttpClient, IDisposable) Get( in TimeSpan? timeout = null )
        {
            var key = tuple_t.key( timeout );
            lock ( _LRUCache )
            {
                if ( !_LRUCache.TryGetValue( key, out var t ) )
                {
                    t = new tuple_t( key.Timeout, CreateHttpClient( in timeout ) );
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
                return (t.HttpClient, new free_tuple( t ));
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

namespace m3u8
{
    using m3u8.infrastructure;

    /// <summary>
    /// 
    /// </summary>
    public static class m3u8_client_factory
    { 
        public static m3u8_client Create() => Create( HttpClientFactory_WithRefCount.Get() );
        public static m3u8_client Create( in (TimeSpan timeout, int attemptRequestCountByPart) t ) => Create( t.timeout, t.attemptRequestCountByPart );
        public static m3u8_client Create( in TimeSpan timeout, int attemptRequestCountByPart = 10 ) => Create( HttpClientFactory_WithRefCount.Get( timeout ), attemptRequestCountByPart );
        public static m3u8_client Create( in m3u8_client.init_params ip ) => Create( HttpClientFactory_WithRefCount.Get(), ip );

        private static m3u8_client Create( in (HttpClient httpClient, IDisposable) t, in m3u8_client.init_params ip ) => new m3u8_client( t, ip );
        private static m3u8_client Create( in (HttpClient httpClient, IDisposable) t, int attemptRequestCountByPart = 10 )
            => Create( t, new m3u8_client.init_params() { AttemptRequestCount = Math.Max( attemptRequestCountByPart, 1 ) } );

        public static void ForceClearAndDisposeAll() => HttpClientFactory_WithRefCount.ForceClearAndDisposeAll();
    }
}
