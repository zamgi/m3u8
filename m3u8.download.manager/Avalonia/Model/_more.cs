using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

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
    internal sealed class ObservableCollection_Ex< T > : ObservableCollection< T >
    {
        private bool _InUpdate;
        public void StartBulkUpdate() => _InUpdate = true;
        public void EndBulkUpdate()
        {
            _InUpdate = false;
            OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
        }
        protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
        {
            if ( !_InUpdate )
            {
                base.OnCollectionChanged( e );
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class ObservableCollection_WithIndex< T >
    {
        private ObservableCollection_Ex< T > _List;
        private Dictionary< T, int > _Dict;
        public ObservableCollection_WithIndex()
        {
            _List = new ObservableCollection_Ex< T >();
            _Dict = new Dictionary< T, int >();
        }
        public ObservableCollection_WithIndex( IEnumerable< T > seq ) : this() => AddRange( seq );

        public ObservableCollection< T > List => _List;

        public void Add( T t ) 
        {
            _Dict[ t ] = _List.Count;
            _List.Add( t );
        }
        public void AddRange( IEnumerable< T > seq )
        {
            if ( seq.AnyEx() )
            {
                _List.StartBulkUpdate();
                {
                    foreach ( var t in seq )
                    {
                        Add( t );
                    }
                }
                _List.EndBulkUpdate();
            }
        }
        public void Replace( IEnumerable< T > seq )
        {
            _List.StartBulkUpdate();
            {
                Clear();
                if ( seq.AnyEx() )
                {
                    foreach ( var t in seq )
                    {
                        Add( t );
                    }
                }
            }
            _List.EndBulkUpdate();
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
            _Dict.Clear();
            _List.Clear();
        }
        public int Count => _List.Count;
        public T this[ int index ] => _List[ index ];
        public int GetIndex( T t ) => _Dict.TryGetValue( t, out var idx ) ? idx : -1;
    }
}
