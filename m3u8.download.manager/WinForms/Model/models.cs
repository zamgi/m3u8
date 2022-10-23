﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if DEBUG
using System.Diagnostics;
#endif

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
        public void ChangeRowsPosition( IReadOnlyList< T > moveRows, int newPivotIndex )
        {
            if ( !moveRows.AnyEx() )
            {
                return;
            }
            if ( moveRows.Count == 1 )
            {
                ChangeRowPosition( moveRows[ 0 ], newPivotIndex );
                return;
            }
#if DEBUG
            Debug.Assert( moveRows.SequenceEqual( moveRows.OrderBy( r => r.GetVisibleIndex() ) ) );
            Debug.Assert( moveRows.Distinct().Count() == moveRows.Count );
            Debug.Assert( moveRows.All( row => _Rows.Contains( row ) ) );
#endif
            moveRows = moveRows.OrderBy( r => r.GetVisibleIndex() ).ToList( moveRows.Count );

            var moveRowsIdx = new List< (T move_row, int move_idx) >( moveRows.Count );
            var moveRowsHs  = new HashSet< T >( moveRows.Count );
            var first_row_vis_idx = moveRows[ 0 ].GetVisibleIndex();
            var move_down = (first_row_vis_idx < newPivotIndex);
            if ( move_down )
            {
                first_row_vis_idx = moveRows[ moveRows.Count - 1 ].GetVisibleIndex();
            }
            foreach ( var row in moveRows )
            {
                moveRowsIdx.Add( (row, newPivotIndex + (row.GetVisibleIndex() - first_row_vis_idx)) );
                moveRowsHs.Add( row );
            }

            var new_rows_order = new UniqueList< T >( _Rows.Count );
            static void add_remainder< X >( UniqueList< X > lst, IEnumerator< X > e )
            {
                lst.Add( e.Current );
                for ( ; e.MoveNext(); )
                {
                    lst.Add( e.Current );
                }
            };
            static void add_remainder_if_exists< X >( UniqueList< X > lst, IEnumerator< (X move_row, int move_idx) > e )
            {
                if ( e.Current.move_row != null )
                {
                    lst.Add( e.Current.move_row );
                    for ( ; e.MoveNext(); )
                    {
                        lst.Add( e.Current.move_row );
                    }
                }
            };

            using var rows_e = _Rows.Where( row => !moveRowsHs.Contains( row ) ).GetEnumerator();
            using var moveRows_e = moveRowsIdx.GetEnumerator();
            if ( rows_e.MoveNext() && moveRows_e.MoveNext() )
            {
                for ( var idx = 0; ; idx++ )
                {
                    var leave_current_idx_condition = (idx < moveRows_e.Current.move_idx);
                    if ( leave_current_idx_condition )
                    {
                        new_rows_order.Add( rows_e.Current );
                        if ( !rows_e.MoveNext() )
                        {
                            break;
                        }
                    }
                    else
                    {
                        new_rows_order.Add( moveRows_e.Current.move_row );
                        if ( !moveRows_e.MoveNext() )
                        {
                            add_remainder( new_rows_order, rows_e );
                            break;

                        }
                    }
                }

                add_remainder_if_exists( new_rows_order, moveRows_e );
            }
            else
            {
                new_rows_order.AddRange( moveRows );
            }

            _Rows.Replace( new_rows_order );

            #region [.re-calculate '_RowsVisibleIndexes'.]
            ReCalculateRowsVisibleIndexes();
            #endregion

            if ( !_UpdtTup.InUpdate )
            {
                CollectionChanged?.Invoke( CollectionChangedTypeEnum.Sort, null );
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

    /// <summary>
    /// 
    /// </summary>
    internal sealed class UniqueList< T > : IEnumerable< T >
    {
        private List< T > _List;
        private HashSet< T > _HS;
        public UniqueList( int capacity, IEqualityComparer< T > comparer = null )
        {
            _List = new List< T >( capacity );
            _HS   = new HashSet< T >( capacity, comparer ?? EqualityComparer< T >.Default );
        }
        public void Add( T t )
        {
            if ( _HS.Add( t ) )
            {
                _List.Add( t );
            }
        }

        public IReadOnlyList< T > List => _List;
        public void AddRange( IEnumerable< T > seq )
        {
            foreach ( var t in seq )
            {
                Add( t );
            }
        }
        public IEnumerator< T > GetEnumerator()
        {
            foreach ( var t in _List )
            {
                yield return (t);
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
