using System;

using _RowPropertiesChanged_ = m3u8.download.manager.models.LogListModel.RowPropertiesChangedEventHandler;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.models
{
    /// <summary>
    /// 
    /// </summary>
    internal enum RequestRowTypeEnum { None, Success, Error };

    /// <summary>
    /// 
    /// </summary>
    internal sealed class LogRow : RowBase< LogRow >
    {
        private _RowPropertiesChanged_ _RowPropertiesChanged;
        internal LogRow( /*LogListModel*/ListModel< LogRow > model, _RowPropertiesChanged_ rowPropertiesChanged ) : base( model )
        {
            _RowPropertiesChanged = rowPropertiesChanged ?? throw (new ArgumentNullException( nameof(rowPropertiesChanged) ));
            RequestText = ResponseText = string.Empty;
        }
        internal LogRow( /*LogListModel*/ListModel< LogRow > model, _RowPropertiesChanged_ rowPropertiesChanged, LogRow other ) : base( model )
        {
            _RowPropertiesChanged = rowPropertiesChanged ?? throw (new ArgumentNullException( nameof(rowPropertiesChanged) ));
            RequestRowType = other.RequestRowType;
            RequestText    = other.RequestText;
            ResponseText   = other.ResponseText;            
        }

        internal static LogRow CreateRequest( string requestText, /*LogListModel*/ListModel< LogRow > model, _RowPropertiesChanged_ rowPropertiesChanged )
            => new LogRow( model, rowPropertiesChanged ) { RequestText = requestText };

        internal static LogRow CreateRequestError( string requestErrorText, /*LogListModel*/ListModel< LogRow > model, _RowPropertiesChanged_ rowPropertiesChanged )
            => new LogRow( model, rowPropertiesChanged ) { RequestRowType = RequestRowTypeEnum.Error, RequestText = requestErrorText };

        internal static LogRow CreateResponseError( string requestText, string responseErrorText, /*LogListModel*/ListModel< LogRow > model, _RowPropertiesChanged_ rowPropertiesChanged )
            => new LogRow( model, rowPropertiesChanged ) { RequestRowType = RequestRowTypeEnum.Error, RequestText  = requestText, ResponseText = responseErrorText };


        public string RequestText  { [M(O.AggressiveInlining)] get; private set; }
        public string ResponseText { [M(O.AggressiveInlining)] get; private set; }

        [M(O.AggressiveInlining)] public void SetResponseSuccess( string responseText )
        {
            ResponseText   = responseText;
            RequestRowType = RequestRowTypeEnum.Success;

            _RowPropertiesChanged.Invoke( this, nameof(RequestRowType) );
        }
        [M(O.AggressiveInlining)] public void SetResponseError( string responseErrorText )
        {
            ResponseText   = responseErrorText;
            RequestRowType = RequestRowTypeEnum.Error;

            _RowPropertiesChanged.Invoke( this, nameof(RequestRowType) );
        }
        public void Append2RequestText( string append2RequestText )
        {
            RequestText += append2RequestText;
            //RequestRowType = RequestRowTypeEnum.Success;

            _RowPropertiesChanged.Invoke( this, nameof(RequestRowType) );
        }

        public RequestRowTypeEnum RequestRowType { [M(O.AggressiveInlining)] get; private set; }
    }
}
