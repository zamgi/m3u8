using System;
using System.Collections.Generic;
#if NETCOREAPP
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#endif

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.models
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class UrlsUniquer
    {
        private Dictionary< string, int > _Urls;
        public UrlsUniquer() => _Urls = new Dictionary< string, int >( StringComparer.InvariantCultureIgnoreCase );
        public void Add( string url )
        {
#if NETCOREAPP
            ref var cnt = ref CollectionsMarshal.GetValueRefOrAddDefault( _Urls, url, out _/*var exists*/ );
            cnt++;
#else
            _Urls.TryGetValue( url, out var cnt );
            _Urls[ url ] = cnt + 1;
#endif
        }
        public void Remove( string url )
        {
#if NETCOREAPP
            ref var cnt = ref CollectionsMarshal.GetValueRefOrNullRef( _Urls, url );
            if ( !Unsafe.IsNullRef( ref cnt ) )
            {
                if ( --cnt <= 0 ) _Urls.Remove( url );
            }
#else
            if ( _Urls.TryGetValue( url, out var cnt ) )
            {
                if ( 1 < cnt ) _Urls[ url ] = cnt - 1;
                else _Urls.Remove( url );
            }
#endif
        }
        public bool Contains( string url ) => _Urls.ContainsKey( url );
        public void Clear() => _Urls.Clear();
    }
}
