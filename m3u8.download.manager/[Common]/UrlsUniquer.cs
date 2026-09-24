using System;
using System.Collections.Generic;

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
            _Urls.TryGetValue( url, out var cnt );
            _Urls[ url ] = cnt + 1;
        }
        public void Remove( string url )
        {
            if ( _Urls.TryGetValue( url, out var cnt ) )
            {
                if ( 1 < cnt ) _Urls[ url ] = cnt - 1;
                else _Urls.Remove( url );
            }
        }
        public bool Contains( string url ) => _Urls.ContainsKey( url );
        public void Clear() => _Urls.Clear();
    }
}
