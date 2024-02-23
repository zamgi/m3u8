using System.Collections.Generic;

namespace m3u8.download.manager.models
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class LogListModel : ListModel< LogRow >
    {
        public LogListModel() { }
        public LogListModel( LogListModel o ) : base() 
        {
            base.BeginUpdate();
            foreach ( var row in o.GetRows() )
            {
                base.Add( new LogRow( this, base._Fire_RowPropertiesChangedEventHandler, row ) );
            }
            base.EndUpdate();
        }
        public LogListModel( IEnumerable< LogRow > rows ) : base()
        {
            base.BeginUpdate();
            foreach ( var row in rows )
            {
                base.Add( new LogRow( this, base._Fire_RowPropertiesChangedEventHandler, row ) );
            }
            base.EndUpdate();
        }

        public LogRow AddEmptyRow() => base.Add( new LogRow( this, base._Fire_RowPropertiesChangedEventHandler ) );
        public LogRow AddRequestRow( string requestText ) => base.Add( LogRow.CreateRequest( requestText, this, base._Fire_RowPropertiesChangedEventHandler ) );
        public LogRow AddRequestHeaderRow( string requestText ) => base.Add( LogRow.CreateRequestHeader( requestText, this, base._Fire_RowPropertiesChangedEventHandler ) );
        public LogRow AddRequestRow( string requestText, string responseText ) => base.Add( LogRow.CreateRequest( requestText, responseText, this, base._Fire_RowPropertiesChangedEventHandler ) );
        public LogRow AddRequestErrorRow( string requestErrorText ) => base.Add( LogRow.CreateRequestError( requestErrorText, this, base._Fire_RowPropertiesChangedEventHandler ) );
        public LogRow AddResponseErrorRow( string requestText, string responseErrorText ) => base.Add( LogRow.CreateResponseError( requestText, responseErrorText, this, base._Fire_RowPropertiesChangedEventHandler ) );

        public LogRow AddRow( LogRow row ) => base.Add( new LogRow( this, base._Fire_RowPropertiesChangedEventHandler, row ) );

        public void RemoveRows( IReadOnlyList< LogRow > rows ) => base.RemoveRows_Internal( rows );
    }
}
