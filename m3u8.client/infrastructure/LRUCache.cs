using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

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
}
