using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using m3u8.download.manager.models;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class LogRowsHeightStorer
    {
        private Dictionary< LogListModel, Dictionary< int, int > > _Dict;
        public LogRowsHeightStorer() => _Dict = new Dictionary< LogListModel, Dictionary< int, int > >();

        public void StoreRowHeight( LogListModel model, DataGridViewRow row )
        {
            if ( !_Dict.TryGetValue( model, out var modelDict ) )
            {
                modelDict = new Dictionary< int, int >();
                _Dict.Add( model, modelDict );
            }

            modelDict[ row.Index ] = row.Height;
        }
        public bool TryGetStorerByModel( LogListModel model, out IReadOnlyDictionary< int, int > dict )
        {
            if ( _Dict.TryGetValue( model, out var modelDict ) )
            {
                dict = modelDict;
                return (true);
            }
            dict = null;
            return (false);
        }
        public bool ContainsModel( LogListModel model ) => _Dict.ContainsKey( model );

        public void LeaveOnly( IEnumerable< LogListModel > models )
        {
            var hs = models?.ToHashSet();
            if ( hs.AnyEx() )
            {
                var existsModels = _Dict.Keys.ToArrayEx();
                foreach ( var m in existsModels )
                {
                    if ( !hs.Contains( m ) )
                    {
                        _Dict.Remove( m );
                    }
                }
            }
            else
            {
                _Dict.Clear();
            }
        }
        public void Clear() => _Dict.Clear();
    }
}
