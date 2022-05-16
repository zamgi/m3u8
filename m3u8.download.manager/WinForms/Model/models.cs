using System;
using System.Collections.Generic;
using System.Diagnostics;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.models
{
    /// <summary>
    /// 
    /// </summary>
    internal abstract class RowBase< T > where T : RowBase< T >
    {
        private ListModel< T > _Model;
        protected RowBase( ListModel< T > model )
        {
            _Model = model ?? throw (new ArgumentNullException( nameof(model) ));
            Id     = Guid.NewGuid();
        }

        public Guid Id { [M(O.AggressiveInlining)] get; }

        [M(O.AggressiveInlining)] public int GetVisibleIndex() => _Model.GetVisibleIndex( this );
    }

    /// <summary>
    /// 
    /// </summary>
    internal class ListModel< T > where T : RowBase< T >
    {
        /// <summary>
        /// 
        /// </summary>
        public enum CollectionChangedTypeEnum { Add, Remove, Clear, BulkUpdate, Sort }
        /// <summary>
        /// 
        /// </summary>
        public delegate void CollectionChangedEventHandler( CollectionChangedTypeEnum collectionChangedType, T item );

        /// <summary>
        /// 
        /// </summary>
        public delegate void RowPropertiesChangedEventHandler( T row, string propertyName );

        public event CollectionChangedEventHandler    CollectionChanged;
        public event RowPropertiesChangedEventHandler RowPropertiesChanged;

        private List< T > _Rows;
        private Dictionary< Guid, int > _RowsVisibleIndexes;
        private (bool InUpdate, int  RowsCount) _UpdtTup = (false, -1);
        protected RowPropertiesChangedEventHandler _Fire_RowPropertiesChangedEventHandler;        

        [M(O.AggressiveInlining)] private void Fire_RowPropertiesChanged( T row, string propertyName ) => RowPropertiesChanged?.Invoke( row, propertyName );

        public ListModel()
        {
            _Rows               = new List< T >();
            _RowsVisibleIndexes = new Dictionary< Guid, int >();
            _Fire_RowPropertiesChangedEventHandler = new RowPropertiesChangedEventHandler( this.Fire_RowPropertiesChanged );
        }

        public void BeginUpdate() => _UpdtTup = (true, this.RowsCount);
        public void EndUpdate()
        {
            var rowCount = _UpdtTup.RowsCount;
            _UpdtTup = (false, -1);
            if ( rowCount != this.RowsCount )
            {
                CollectionChanged?.Invoke( CollectionChangedTypeEnum.BulkUpdate, null );
            }
        }

        [M(O.AggressiveInlining)] protected T Add( T row )
        {
#if DEBUG
            Debug.Assert( row != null );
#endif
            if ( row == null ) throw (new ArgumentNullException( nameof(row) ));

            if ( _RowsVisibleIndexes.ContainsKey( row.Id ) )
            {
                throw (new InvalidOperationException( "Row already in list" ));
            }

            _RowsVisibleIndexes.Add( row.Id, _Rows.Count );
            _Rows.Add( row );
            if ( !_UpdtTup.InUpdate )
            {
                CollectionChanged?.Invoke( CollectionChangedTypeEnum.Add, row );
            }
            return (row);
        }
        [M(O.AggressiveInlining)] protected bool Remove( T row )
        {
#if DEBUG
            Debug.Assert( row != null );
#endif
            if ( row != null )
            {                
                var success = _Rows.Remove( row );

                #region [.re-calculate '_RowsVisibleIndexes'.]
                _RowsVisibleIndexes.Remove( row.Id );
                ReCalculateRowsVisibleIndexes();
                #endregion

                if ( !_UpdtTup.InUpdate )
                {
                    CollectionChanged?.Invoke( CollectionChangedTypeEnum.Remove, (success ? row : null) );
                }
                return (success);
            }
            return (false);
        }
        public void Clear()
        {
            var rowCount = _Rows.Count;
            _Rows.Clear();
            _RowsVisibleIndexes.Clear();
            OnAfterClear();
            if ( (rowCount != _Rows.Count) && !_UpdtTup.InUpdate )
            {
                CollectionChanged?.Invoke( CollectionChangedTypeEnum.Clear, null );
            }
        }
        protected virtual void OnAfterClear() { }
        public void Sort( Comparison< T > comparison )
        {
            _Rows.Sort( comparison );

            #region [.re-calculate '_RowsVisibleIndexes'.]
            ReCalculateRowsVisibleIndexes();
            #endregion

            if ( !_UpdtTup.InUpdate )
            {
                CollectionChanged?.Invoke( CollectionChangedTypeEnum.Sort, null );
            }
        }

        public void ChangeRowPosition( T row, int newIndex )
        {
            if ( row == null )                                 throw (new ArgumentNullException( nameof(row) ));
            if ( (newIndex < 0) || (_Rows.Count <= newIndex) ) throw (new ArgumentException( nameof(newIndex) ));

            var oldIndex = row.GetVisibleIndex();
            if ( oldIndex == -1 ) throw (new ArgumentException( nameof(row.GetVisibleIndex) ));

            if ( oldIndex != newIndex )
            {
                _Rows.RemoveAt( oldIndex );
                _Rows.Insert( newIndex, row );

                #region [.re-calculate '_RowsVisibleIndexes'.]
                ReCalculateRowsVisibleIndexes();
                #endregion

                if ( !_UpdtTup.InUpdate )
                {
                    CollectionChanged?.Invoke( CollectionChangedTypeEnum.Sort, null );
                }
            }
        }

        [M(O.AggressiveInlining)] public int GetVisibleIndex( RowBase< T > row ) => (_RowsVisibleIndexes.TryGetValue( row.Id, out var visibleIndex ) ? visibleIndex : -1);
        [M(O.AggressiveInlining)] private void ReCalculateRowsVisibleIndexes()
        {
            var visibleIndex = 0;
            foreach ( var row in _Rows )
            {
                _RowsVisibleIndexes[ row.Id ] = visibleIndex++;
            }
        }

        public int RowsCount { [M(O.AggressiveInlining)] get => _Rows.Count; }
        public T this[ int i ] { [M(O.AggressiveInlining)] get => _Rows[ i ]; }

        [M(O.AggressiveInlining)] public IEnumerable< T > GetRows() => _Rows;
    }
}
