using System;

using _RowPropertiesChanged_ = m3u8.download.manager.models.LogListModel.RowPropertiesChangedEventHandler;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.models
{
    /// <summary>
    /// 
    /// </summary>
    internal enum RequestRowTypeEnum { None, Success, Error, RequestHeader };

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
            RequestRowType        = other.RequestRowType;
            RequestText           = other.RequestText;
            ResponseText          = other.ResponseText;
            AttemptRequestNumber  = other.AttemptRequestNumber;
        }

        internal static LogRow CreateRequest( string requestText, /*LogListModel*/ListModel< LogRow > model, _RowPropertiesChanged_ rowPropertiesChanged )
            => new LogRow( model, rowPropertiesChanged ) { RequestText = requestText };

        internal static LogRow CreateRequestHeader( string requestText, /*LogListModel*/ListModel< LogRow > model, _RowPropertiesChanged_ rowPropertiesChanged )
            => new LogRow( model, rowPropertiesChanged ) { RequestRowType = RequestRowTypeEnum.RequestHeader, RequestText = requestText };

        internal static LogRow CreateRequest( string requestText, string responseText, /*LogListModel*/ListModel< LogRow > model, _RowPropertiesChanged_ rowPropertiesChanged )
            => new LogRow( model, rowPropertiesChanged ) { RequestText = requestText, ResponseText = responseText };

        internal static LogRow CreateRequestError( string requestErrorText, /*LogListModel*/ListModel< LogRow > model, _RowPropertiesChanged_ rowPropertiesChanged )
            => new LogRow( model, rowPropertiesChanged ) { RequestRowType = RequestRowTypeEnum.Error, RequestText = requestErrorText };

        internal static LogRow CreateResponseError( string requestText, string responseErrorText, /*LogListModel*/ListModel< LogRow > model, _RowPropertiesChanged_ rowPropertiesChanged )
            => new LogRow( model, rowPropertiesChanged ) { RequestRowType = RequestRowTypeEnum.Error, RequestText  = requestText, ResponseText = responseErrorText };


        public RequestRowTypeEnum RequestRowType       { [M(O.AggressiveInlining)] get; private set; }
        public string             RequestText          { [M(O.AggressiveInlining)] get; private set; }
        public string             ResponseText         { [M(O.AggressiveInlining)] get; private set; }
        public int?               AttemptRequestNumber { [M(O.AggressiveInlining)] get; private set; }

        [M(O.AggressiveInlining)] public void SetAttemptRequestNumber( int attemptRequestNumber )
        {
            if ( AttemptRequestNumber != attemptRequestNumber )
            {
                AttemptRequestNumber = attemptRequestNumber;
                _RowPropertiesChanged.Invoke( this, nameof(AttemptRequestNumber) );
            }
        }
        [M(O.AggressiveInlining)] public void SetResponse( string responseText, int? attemptRequestNumber = null )
        {
            ResponseText = responseText;
            _RowPropertiesChanged.Invoke( this, nameof(ResponseText/*RequestRowType*/) );

            if ( attemptRequestNumber.HasValue && (AttemptRequestNumber != attemptRequestNumber.Value) )
            {
                AttemptRequestNumber = attemptRequestNumber.Value;
                _RowPropertiesChanged.Invoke( this, nameof(AttemptRequestNumber) );
            }
        }
        [M(O.AggressiveInlining)] public void SetResponseSuccess( string responseText )
        {
            ResponseText   = responseText;
            RequestRowType = RequestRowTypeEnum.Success;

            _RowPropertiesChanged.Invoke( this, nameof(RequestRowType) );
        }
        [M(O.AggressiveInlining)] public void SetResponseError( string responseErrorText, int? attemptRequestNumber = null )
        {
            ResponseText   = responseErrorText;
            RequestRowType = RequestRowTypeEnum.Error;

            _RowPropertiesChanged.Invoke( this, nameof(RequestRowType) );

            if ( attemptRequestNumber.HasValue && (AttemptRequestNumber != attemptRequestNumber.Value) )
            {
                AttemptRequestNumber = attemptRequestNumber.Value;
                _RowPropertiesChanged.Invoke( this, nameof(AttemptRequestNumber) );
            }
        }
        public void Append2RequestText( string append2RequestText )
        {
            RequestText += append2RequestText;
            //RequestRowType = RequestRowTypeEnum.Success;

            _RowPropertiesChanged.Invoke( this, nameof(RequestRowType) );
        }
    }
}
