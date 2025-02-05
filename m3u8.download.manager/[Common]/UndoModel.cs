using System;
using System.Collections.Generic;
using System.Linq;

using _CollectionChangedTypeEnum_ = m3u8.download.manager.models.ListModel< m3u8.download.manager.models.DownloadRow >.CollectionChangedTypeEnum;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.models
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class UndoModel : IDisposable
    {
        private readonly DownloadListModel    _ShadowRowsModel;
        private readonly Queue< DownloadRow > _UndoList;
        private readonly DownloadListModel    _DownloadListModel;
        public UndoModel( DownloadListModel downloadListModel )
        {
            _ShadowRowsModel = new DownloadListModel();
            _UndoList        = new Queue< DownloadRow >();
            _DownloadListModel = downloadListModel;
            _DownloadListModel.CollectionChanged += _DownloadListModel_CollectionChanged;
        }

        public event Action UndoChanged;
        [M(O.AggressiveInlining)] private void Fire_UndoChanged() => UndoChanged?.Invoke();

        public void Dispose()
        {
            _DownloadListModel.CollectionChanged -= _DownloadListModel_CollectionChanged;
            _UndoList.Clear();
            _ShadowRowsModel.Clear();
        }
        private void _DownloadListModel_CollectionChanged( _CollectionChangedTypeEnum_ changedType, DownloadRow row )
        {
            switch ( changedType )
            {
                case _CollectionChangedTypeEnum_.Remove:
                    if ( _UndoList.AddIf( row ) ) Fire_UndoChanged();
                    break;

                //case _CollectionChangedTypeEnum_.BulkUpdate:
                case _CollectionChangedTypeEnum_.Remove_Bulk:
                    var existsRows = _DownloadListModel.GetRows();
                    var undoRows   = _ShadowRowsModel.GetRows().Except( existsRows ).ToList();
                    if ( _UndoList.Add( undoRows ) ) Fire_UndoChanged();
                    _ShadowRowsModel.RemoveRows( undoRows );
                    break;

                case _CollectionChangedTypeEnum_.Clear:
                    if ( _UndoList.Replace( _ShadowRowsModel.GetRows() ) ) Fire_UndoChanged();
                    _ShadowRowsModel.Clear();
                    break;

                case _CollectionChangedTypeEnum_.Add_Bulk:
                case _CollectionChangedTypeEnum_.Sort:
                    _ShadowRowsModel.Replace( _DownloadListModel.GetRows() );
                    break;

                case _CollectionChangedTypeEnum_.Add:
                    _ShadowRowsModel.AddRowIf( row );
                    break;
            }
        }

        public bool TryUndo( out DownloadRow row )
#if NETCOREAPP
        {
            if ( _UndoList.TryDequeue( out row ) )
            {
                Fire_UndoChanged();
                return (true);
            }
            return (false);
        }
#else
        {
            if ( 0 < _UndoList.Count )
            {
                row = _UndoList.Dequeue();
                Fire_UndoChanged();
                return (true);
            }
            row = null; 
            return (false);
        }
#endif
        public bool HasUndo => (0 < _UndoList.Count);
        public int  UndoCount => _UndoList.Count;
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class UndoModel_Extensions
    {
        public static void Replace( this DownloadListModel downloadListModel, IEnumerable< DownloadRow > rows )
        {
            downloadListModel.Clear();
            downloadListModel.AddRowsIf( rows );
        }
        public static void AddRowsIf( this DownloadListModel downloadListModel, IEnumerable< DownloadRow > rows )
        {
            if ( rows != null )
            {
                foreach ( var row in rows )
                {
                    downloadListModel.AddRowIf( row );
                }
            }
        }
        public static void AddRowIf( this DownloadListModel downloadListModel, DownloadRow row )
        {
            if ( (row != null) && !downloadListModel.ContainsRow( row ) )
            {
                downloadListModel.AddRow( row );
            }
        }

        public static bool Replace< T >( this Queue< T > queue, IEnumerable< T > seq )
        {
            queue.Clear();
            queue.Add( seq );
            return (true);
        }
        public static bool Add< T >( this Queue< T > queue, IEnumerable< T > seq )
        {
            bool suc; 
            if ( suc = seq.AnyEx() )
            {
                foreach ( var t in seq )
                {
                    queue.Enqueue( t );
                }
            }
            return (suc);
        }
        public static bool AddIf< T >( this Queue< T > queue, T t )
        {
            bool suc;
            if ( suc = (t != null) )
            {
                queue.Enqueue( t );
            }
            return (suc);
        }
    }
}
