using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal struct SortInfo
    {
        /// <summary>
        /// 
        /// </summary>
        [DataContract]
        private struct _4Json_
        {
            [DataMember(EmitDefaultValue=false)]
            public int?       ColumnIndex { get; set; }

            [DataMember(EmitDefaultValue=false)]
            public SortOrder? Order       { get; set; }
        }

        public int?       ColumnIndex { get; private set; }
        public SortOrder? Order       { get; private set; }

        public bool       HasSorting => (ColumnIndex.HasValue && Order.HasValue);
        public bool       TryGetSorting( out int columnIndex, out SortOrder order )
        {
            columnIndex = ColumnIndex.GetValueOrDefault();
            order       = Order      .GetValueOrDefault();
            return (HasSorting);
        }

        public void SetSortOrderAndSaveCurrent( int columnIndex )
        {
            if ( !this.ColumnIndex.HasValue || (this.ColumnIndex.Value != columnIndex) )
            {
                this.ColumnIndex = columnIndex;
                this.Order       = SortOrder.Ascending;
            }
            else if ( this.Order == SortOrder.Ascending )
            {
#if DEBUG
                Debug.Assert( (this.ColumnIndex == columnIndex) );
#endif
                this.Order = SortOrder.Descending;
            }
            else
            {
                this.ColumnIndex = null;
                this.Order       = null;
            }
        }
        public string ToJson() => (new _4Json_() { ColumnIndex = ColumnIndex, Order = Order }).ToJSON();
        public static SortInfo FromJson( string json )
        {
            try
            {
                var _ = Extensions.FromJSON<_4Json_>( json );
                return (new SortInfo() { ColumnIndex = _.ColumnIndex, Order = _.Order });
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
                return (default);
            }
        }
    }
}
