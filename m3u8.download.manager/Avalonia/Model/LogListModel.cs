using System.Collections.Generic;

namespace m3u8.download.manager.models
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LogListModel : ListModel< LogRow >
    {
        public LogListModel() { }
        public LogListModel( LogListModel o ) : base()
        {
            foreach ( var row in o.GetRows() )
            {
                base.Add( new LogRow( this, base._Fire_RowPropertiesChangedEventHandler, row ) );
            }
        }
        public LogListModel( IEnumerable<LogRow> rows ) : base()
        {
            foreach ( var row in rows )
            {
                base.Add( new LogRow( this, base._Fire_RowPropertiesChangedEventHandler, row ) );
            }
        }

        public LogRow AddEmptyRow() => base.Add( LogRow.CreateRequestFinish( " ", this, base._Fire_RowPropertiesChangedEventHandler ) );

        public LogRow AddRequestRow( string requestText ) => base.Add( LogRow.CreateRequest( requestText, this, base._Fire_RowPropertiesChangedEventHandler ) );
        public LogRow AddRequestRowSuccess( string requestText ) => base.Add( LogRow.CreateRequestSuccess( requestText, this, base._Fire_RowPropertiesChangedEventHandler ) );
        public LogRow AddRequestRowFinish( string requestText ) => base.Add( LogRow.CreateRequestFinish( requestText, this, base._Fire_RowPropertiesChangedEventHandler ) );

        public LogRow AddRequestErrorRow( string requestErrorText ) => base.Add( LogRow.CreateRequestError( requestErrorText, this, base._Fire_RowPropertiesChangedEventHandler ) );
        public LogRow AddResponseErrorRow( string requestText, string responseErrorText ) => base.Add( LogRow.CreateResponseError( requestText, responseErrorText, this, base._Fire_RowPropertiesChangedEventHandler ) );

        public LogRow AddRow( LogRow row ) => base.Add( new LogRow( this, base._Fire_RowPropertiesChangedEventHandler, row ) );

        public void RemoveRows( IEnumerable< LogRow > rows ) => base.RemoveRows_Internal( rows );              

    }
}
