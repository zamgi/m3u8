using System;
using System.Collections.Generic;
using System.Linq;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.models
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class DownloadListModel : ListModel< DownloadRow >
    {
        private HashSet< string > _Urls;
        public DownloadListModel() => _Urls = new HashSet< string >( StringComparer.InvariantCultureIgnoreCase );

        public DownloadRow AddRow( in (string Url, string OutputFileName, string OutputDirectory) t )
        {
            var row = base.Add( new DownloadRow( in t, this ) );
            _Urls.Add( row.Url );
            return (row);
        }

        [M(O.AggressiveInlining)] public bool HasAnyFinished() => GetAllFinished().Any();
        [M(O.AggressiveInlining)] public IEnumerable< DownloadRow > GetAllFinished() => (from row in GetRows() where (row.Status == DownloadStatus.Finished) select row);
        
        [M(O.AggressiveInlining)] public bool ContainsUrl( string url ) => (!url.IsNullOrEmpty() && _Urls.Contains( url ));
        public bool RemoveRow( DownloadRow row )
        {
            var success = base.Remove( row );
            _Urls.Remove( (row?.Url ?? string.Empty) );
            return (success);
        }

        protected override void OnAfterClear() => _Urls.Clear();

        public IReadOnlyDictionary< DownloadStatus, int > GetStatisticsByAllStatus()
        {
            var values = (DownloadStatus[]) Enum.GetValues( typeof(DownloadStatus) );
            var dict = new Dictionary< DownloadStatus, int >( 11 /*values.Length*/ );
            foreach ( var v in values )
            {
                dict[ v ] = 0;
            }
            foreach ( var row in base.GetRows() )
            {
                dict[ row.Status ]++;
            }
            return (dict);
        }
        public bool TryGetSingleRunning( out DownloadRow singleRunningRow )
        {
            singleRunningRow = default;
            foreach ( var row in GetRows() )
            {
                if ( row.Status == DownloadStatus.Running )
                {
                    if ( singleRunningRow != null )
                    {
                        return (false); //not single running row
                    }
                    singleRunningRow = row;
                }
            }
            return (singleRunningRow != null);
        }
    }
}
