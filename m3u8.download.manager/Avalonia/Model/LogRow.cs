using System;
using System.ComponentModel;

using _RowPropertiesChanged_ = m3u8.download.manager.models.LogListModel.RowPropertiesChangedEventHandler;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.models
{
    /// <summary>
    /// 
    /// </summary>
    public enum RequestRowTypeEnum { None, Success, Error, Finish, RequestHeader };

    /// <summary>
    /// 
    /// </summary>
    public sealed class LogRow : RowBase< LogRow >, INotifyPropertyChanged
    {
        private _RowPropertiesChanged_ _RowPropertiesChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        [M(O.AggressiveInlining)] private void Fire_PropertyChanged_Events( string propertyName )
        {
            _RowPropertiesChanged?.Invoke( this, propertyName );
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        internal LogRow( LogListModel model, _RowPropertiesChanged_ rowPropertiesChanged ) : base( model )
        {
            _RowPropertiesChanged = rowPropertiesChanged ?? throw (new ArgumentNullException( nameof(rowPropertiesChanged) ));
            RequestText = ResponseText = string.Empty;
        }
        internal LogRow( LogListModel model, _RowPropertiesChanged_ rowPropertiesChanged, LogRow r ) : base( model )
        {
            _RowPropertiesChanged = rowPropertiesChanged ?? throw (new ArgumentNullException( nameof(rowPropertiesChanged) ));
            RequestRowType        = r.RequestRowType;
            RequestText           = r.RequestText;
            ResponseText          = r.ResponseText;
            AttemptRequestNumber  = r.AttemptRequestNumber;
        }

        internal static LogRow CreateRequest( string requestText, LogListModel model, _RowPropertiesChanged_ rowPropertiesChanged )
            => new LogRow( model, rowPropertiesChanged ) { RequestText = requestText };

        internal static LogRow CreateRequestHeader( string requestText, LogListModel model, _RowPropertiesChanged_ rowPropertiesChanged )
            => new LogRow( model, rowPropertiesChanged ) { RequestRowType = RequestRowTypeEnum.RequestHeader, RequestText = requestText };

        internal static LogRow CreateRequest( string requestText, string responseText, LogListModel model, _RowPropertiesChanged_ rowPropertiesChanged )
            => new LogRow( model, rowPropertiesChanged ) { RequestText = requestText, ResponseText = responseText };        
        internal static LogRow CreateRequestSuccess( string requestText, LogListModel model, _RowPropertiesChanged_ rowPropertiesChanged )
            => new LogRow( model, rowPropertiesChanged ) { RequestRowType = RequestRowTypeEnum.Success, RequestText = requestText };
        internal static LogRow CreateRequestFinish( string requestText, LogListModel model, _RowPropertiesChanged_ rowPropertiesChanged )
            => new LogRow( model, rowPropertiesChanged ) { RequestRowType = RequestRowTypeEnum.Finish, RequestText = requestText };

        internal static LogRow CreateRequestError( string requestErrorText, LogListModel model, _RowPropertiesChanged_ rowPropertiesChanged )
            => new LogRow( model, rowPropertiesChanged ) { RequestRowType = RequestRowTypeEnum.Error, RequestText = requestErrorText };

        internal static LogRow CreateResponseError( string requestText, string responseErrorText, LogListModel model, _RowPropertiesChanged_ rowPropertiesChanged )
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
                Fire_PropertyChanged_Events( nameof(AttemptRequestNumber) );
            }
        }
        [M(O.AggressiveInlining)] public void SetResponse( string responseText, int? attemptRequestNumber = null )
        {
            ResponseText = responseText;
            Fire_PropertyChanged_Events( nameof(ResponseText) );

            if ( attemptRequestNumber.HasValue && (AttemptRequestNumber != attemptRequestNumber.Value) )
            {
                AttemptRequestNumber = attemptRequestNumber.Value;
                Fire_PropertyChanged_Events( nameof( AttemptRequestNumber ) );
            }
        }
        [M(O.AggressiveInlining)] public void SetResponseSuccess( string responseText )
        {
            ResponseText   = responseText;
            RequestRowType = RequestRowTypeEnum.Success;

            Fire_PropertyChanged_Events( nameof(ResponseText)   );
            Fire_PropertyChanged_Events( nameof(RequestRowType) );
        }
        [M(O.AggressiveInlining)] public void SetResponseError( string responseErrorText, int? attemptRequestNumber = null )
        {
            ResponseText   = responseErrorText;
            RequestRowType = RequestRowTypeEnum.Error;

            Fire_PropertyChanged_Events( nameof(ResponseText)   );
            Fire_PropertyChanged_Events( nameof(RequestRowType) );

            if ( attemptRequestNumber.HasValue && (AttemptRequestNumber != attemptRequestNumber.Value) )
            {
                AttemptRequestNumber = attemptRequestNumber.Value;
                Fire_PropertyChanged_Events( nameof(AttemptRequestNumber) );
            }
        }
        public void Append2RequestText( string append2RequestText )
        {
            RequestText += append2RequestText;
            //RequestRowType = RequestRowTypeEnum.Success;

            Fire_PropertyChanged_Events( nameof(RequestRowType) );
        }
    }
}
