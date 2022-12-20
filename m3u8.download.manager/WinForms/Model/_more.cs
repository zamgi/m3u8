using System.Collections.Generic;

using m3u8.download.manager.controllers;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.models
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ThreadSafeList< T >
    {
        private List< T > _List;
        public ThreadSafeList() => _List = new List< T >();
        public int Add( T t )
        {
            lock ( _List )
            {
                _List.Add( t );
                return (_List.Count);
            }
        }
        public void Clear()
        {
            lock ( _List )
            {
                _List.Clear();
            }
        }
        public int GetCount()
        {
            lock ( _List )
            {
                return (_List.Count);
            }
        }
        public bool TryGetAndClear( out IList< T > seq )
        {
            lock ( _List ) 
            {
                if ( 0 < _List.Count )
                {
                    seq = _List.ToArray();
                    _List.Clear();
                    return (true);
                }
            }
            seq = default;
            return (false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class List_WithIndex< T >
    {
        private List< T > _List;
        private Dictionary< T, int > _Dict;
        public List_WithIndex()
        {
            _List = new List< T >();
            _Dict = new Dictionary< T, int >();
        }

        public void Add( T t ) 
        {
            _Dict[ t ] = _List.Count;
            _List.Add( t );
        }
        public void AddRange( IEnumerable< T > seq )
        {
            foreach ( var t in seq )
            {
                Add( t );
            }
        }
        public void Replace( IEnumerable< T > seq )
        {
            Clear();
            if ( seq != null )
            {
                foreach ( var t in seq )
                {
                    Add( t );
                }
            }
        }
        public bool Remove( T t )
        {
            var suc = _Dict.RemoveEx( t, out var idx );
            if ( suc )
            {
                _List.RemoveAt( idx );
                ReCalcRowsIndexes();
            }
            return (suc);
        }
        [M(O.AggressiveInlining)] private void ReCalcRowsIndexes()
        {            
            for ( int i = _List.Count - 1; 0 <= i; i-- )
            {
                _Dict[ _List[ i ] ] = i;
            }
        }
        public void Clear()
        {
            _List.Clear();
            _Dict.Clear();
        }
        public int Count => _List.Count;
        public T this[ int index ] => _List[ index ];
        public int GetIndex( T t ) => _Dict.TryGetValue( t, out var idx ) ? idx : -1;
    }
}
