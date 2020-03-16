namespace m3u8.download.manager.models
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LogListModel : ListModel< LogRow >
    {
        public LogRow AddEmptyRow() => base.Add( LogRow.CreateRequestFinish( " ", this, base._Fire_RowPropertiesChangedEventHandler ) );

        public LogRow AddRequestRow( string requestText ) => base.Add( LogRow.CreateRequest( requestText, this, base._Fire_RowPropertiesChangedEventHandler ) );
        public LogRow AddRequestRowSuccess( string requestText ) => base.Add( LogRow.CreateRequestSuccess( requestText, this, base._Fire_RowPropertiesChangedEventHandler ) );
        public LogRow AddRequestRowFinish( string requestText ) => base.Add( LogRow.CreateRequestFinish( requestText, this, base._Fire_RowPropertiesChangedEventHandler ) );

        public LogRow AddRequestErrorRow( string requestErrorText ) => base.Add( LogRow.CreateRequestError( requestErrorText, this, base._Fire_RowPropertiesChangedEventHandler ) );
        public LogRow AddResponseErrorRow( string requestText, string responseErrorText ) => base.Add( LogRow.CreateResponseError( requestText, responseErrorText, this, base._Fire_RowPropertiesChangedEventHandler ) );
    }
}
