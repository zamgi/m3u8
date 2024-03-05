using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class RequestHeadersEditor : UserControl
    {
        /// <summary>
        /// 
        /// </summary>
        public delegate void RequestHeadersCountChangedEventHandler( int requestHeadersCount, int enabledCount );

        /// <summary>
        /// 
        /// </summary>
        private sealed class RequestHeader
        {
            public RequestHeader( string name, string description, int? max_name_length = null )
            {
                Name        = name;
                DisplayName = name;
                Description = description;
                if ( description != null )
                {
                    var indent = new string( ' ', 2 + (max_name_length.GetValueOrDefault( name.Length ) - name.Length) );
                    DisplayName += indent + description;
                }
            }
            public RequestHeader( string name )
            {
                Name        = name;
                DisplayName = name;
            }

            public string Name        { get; }
            public string Description { get; }
            public string DisplayName { get; }
            public override string ToString() => DisplayName;
        }

        #region [.fields.]
        private const int CHECKED_CELL_IDX = 0;
        private const int KEY_CELL_IDX     = 1;
        private const int VALUE_CELL_IDX   = 2;

        private RowNumbersPainter _RPN;
        private List< RequestHeader > _AllRequestHeaders4DropDown;
        #endregion

        #region [.ctor().]
        public/*private*/ RequestHeadersEditor()
        {
            InitializeComponent();

            DGV.DefaultCellStyle = DefaultColors.DGV.Create_Suc( DGV.DefaultCellStyle );

            _RPN = RowNumbersPainter.Create( DGV, useSelectedBackColor: false );

            SetRequestHeaders( null );
        }
        public RequestHeadersEditor( Dictionary< string, string > requestHeaders ) : this() => SetRequestHeaders( requestHeaders );

        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();
                _RPN.Dispose();
            }
            base.Dispose( disposing );
        }
        #endregion

        #region [.public.]
        public event RequestHeadersCountChangedEventHandler OnRequestHeadersCountChanged;
        private void Fire_OnRequestHeadersCountChanged() => OnRequestHeadersCountChanged?.Invoke( DGV.RowCount - 1, GetEnabledCount() );

        private static IEnumerable< (string name, string descr) > GetRegularRequestHeader()
        {
            //System.Net.HttpRequestHeader.Connection
            //var rh = new System.Net.Http.Headers.HttpRequestHeaders();

            yield return ("A-IM"               , "Acceptable instance-manipulations for the request.");
            yield return ("Accept"             , "Media type(s) that is/are acceptable for the response. See Content negotiation.");
            yield return ("Accept-Charset"     , "Character sets that are acceptable.");
            yield return ("Accept-Datetime"    , "Acceptable version in time.");
            yield return ("Accept-Encoding"    , "List of acceptable encodings. See HTTP compression.");
            yield return ("Accept-Language"    , "List of acceptable human languages for response. See Content negotiation.");
            yield return ("Access-Control-Request-Method", "Initiates a request for cross-origin resource sharing with Origin (below).");
            yield return ("Access-Control-Request-Headers", "Initiates a request for cross-origin resource sharing with Origin (below).");
            yield return ("Authorization"      , "Authentication credentials for HTTP authentication.");
            yield return ("Cache-Control"      , "Used to specify directives that must be obeyed by all caching mechanisms along the request-response chain.");
            yield return ("Connection"         , "Control options for the current connection and list of hop-by-hop request fields. Must not be used with HTTP/2.");
            yield return ("Content-Encoding"   , "The type of encoding used on the data. See HTTP compression.");
            yield return ("Content-Length"     , "The length of the request body in octets (8-bit bytes).");
            yield return ("Content-MD5"        , "A Base64-encoded binary MD5 sum of the content of the request body.");
            yield return ("Content-Type"       , "The Media type of the body of the request (used with POST and PUT requests).");
            yield return ("Cookie"             , "An HTTP cookie previously sent by the server with Set-Cookie (below).");
            yield return ("Date"               , "The date and time at which the message was originated (in \"HTTP-date\" format as defined by RFC 9110: HTTP Semantics, section 5.6.7 \"Date/Time Formats\").");
            yield return ("Expect"             , "Indicates that particular server behaviors are required by the client.");
            yield return ("Forwarded"          , "Disclose original information of a client connecting to a web server through an HTTP proxy.");
            yield return ("From"               , "The email address of the user making the request.");
            yield return ("Host"               , "The domain name of the server (for virtual hosting), and the TCP port number on which the server is listening. The port number may be omitted if the port is the standard port for the service requested. Mandatory since HTTP/1.1. If the request is generated directly in HTTP/2, it should not be used.");
            yield return ("HTTP2-Settings"     , "A request that upgrades from HTTP/1.1 to HTTP/2 MUST include exactly one HTTP2-Settings header field. The HTTP2-Settings header field is a connection-specific header field that includes parameters that govern the HTTP/2 connection, provided in anticipation of the server accepting the request to upgrade.");
            yield return ("If-Match"           , "Only perform the action if the client supplied entity matches the same entity on the server. This is mainly for methods like PUT to only update a resource if it has not been modified since the user last updated it.");
            yield return ("If-Modified-Since"  , "Allows a 304 Not Modified to be returned if content is unchanged.");
            yield return ("If-None-Match"      , "Allows a 304 Not Modified to be returned if content is unchanged, see HTTP ETag.");
            yield return ("If-Range"           , "If the entity is unchanged, send me the part(s) that I am missing; otherwise, send me the entire new entity.");
            yield return ("If-Unmodified-Since", "Only send the response if the entity has not been modified since a specific time.");
            yield return ("Max-Forwards"       , "Limit the number of times the message can be forwarded through proxies or gateways.");
            yield return ("Origin"             , "Initiates a request for cross-origin resource sharing (asks server for Access-Control-* response fields).");
            yield return ("Pragma"             , "Implementation-specific fields that may have various effects anywhere along the request-response chain.");
            yield return ("Prefer"             , "Allows client to request that certain behaviors be employed by a server while processing a request.");
            yield return ("Proxy-Authorization", "Authorization credentials for connecting to a proxy.");
            yield return ("Range"              , "Request only part of an entity. Bytes are numbered from 0. See Byte serving.");
            yield return ("Referer"            , "This is the address of the previous web page from which a link to the currently requested page was followed. (The word \"referrer\" has been misspelled in the RFC as well as in most implementations to the point that it has become standard usage and is considered correct terminology)");
            yield return ("TE"                 , "The transfer encodings the user agent is willing to accept: the same values as for the response header field Transfer-Encoding can be used, plus the \"trailers\" value (related to the \"chunked\" transfer method) to notify the server it expects to receive additional fields in the trailer after the last, zero-sized, chunk. Only trailers is supported in HTTP/2.");
            yield return ("Trailer"            , "The Trailer general field value indicates that the given set of header fields is present in the trailer of a message encoded with chunked transfer coding.");
            yield return ("Transfer-Encoding"  , "The form of encoding used to safely transfer the entity to the user. Currently defined methods are: chunked, compress, deflate, gzip, identity. Must not be used with HTTP/2.");
            yield return ("User-Agent"         , "The user agent string of the user agent.");
            yield return ("Upgrade"            , "Ask the server to upgrade to another protocol. Must not be used in HTTP/2.");
            yield return ("Via"                , "Informs the server of proxies through which the request was sent.");
            yield return ("Warning"            , "A general warning about possible problems with the entity body.");

            yield return ("Upgrade-Insecure-Requests", "Tells a server which (presumably in the middle of a HTTP -> HTTPS migration) hosts mixed content that the client would prefer redirection to HTTPS and can handle Content-Security-Policy: upgrade-insecure-requests Must not be used with HTTP/2");
            yield return ("X-Requested-With"         , "Mainly used to identify Ajax requests (most JavaScript frameworks send this field with value of XMLHttpRequest); also identifies Android apps using WebView");
            yield return ("DNT"                      , "Requests a web application to disable their tracking of a user. This is Mozilla's version of the X-Do-Not-Track header field (since Firefox 4.0 Beta 11). Safari and IE9 also have support for this field. On March 7, 2011, a draft proposal was submitted to IETF. The W3C Tracking Protection Working Group is producing a specification.");
            yield return ("X-Forwarded-For"          , "A de facto standard for identifying the originating IP address of a client connecting to a web server through an HTTP proxy or load balancer. Superseded by Forwarded header.");
            yield return ("X-Forwarded-Host"         , "A de facto standard for identifying the original host requested by the client in the Host HTTP request header, since the host name and/or port of the reverse proxy (load balancer) may differ from the origin server handling the request. Superseded by Forwarded header.");
            yield return ("X-Forwarded-Proto"        , "A de facto standard for identifying the originating protocol of an HTTP request, since a reverse proxy (or a load balancer) may communicate with a web server using HTTP even if the request to the reverse proxy is HTTPS. An alternative form of the header (X-ProxyUser-Ip) is used by Google clients talking to Google servers. Superseded by Forwarded header.");
            yield return ("Front-End-Https"          , "Non-standard header field used by Microsoft applications and load-balancers");
            yield return ("X-Http-Method-Override"   , "Requests a web application to override the method specified in the request (typically POST) with the method given in the header field (typically PUT or DELETE). This can be used when a user agent or firewall prevents PUT or DELETE methods from being sent directly (this is either a bug in the software component, which ought to be fixed, or an intentional configuration, in which case bypassing it may be the wrong thing to do).");
            yield return ("X-ATT-DeviceId"           , "Allows easier parsing of the MakeModel/Firmware that is usually found in the User-Agent String of AT&T Devices");
            yield return ("X-Wap-Profile"            , "Links to an XML file on the Internet with a full description and details about the device currently connecting. In the example to the right is an XML file for an AT&T Samsung Galaxy S2.");
            yield return ("Proxy-Connection"         , "Implemented as a misunderstanding of the HTTP specifications. Common because of mistakes in implementations of early HTTP versions. Has exactly the same functionality as standard Connection field. Must not be used with HTTP/2.");
            yield return ("X-UIDH"                   , "Server-side deep packet inspection of a unique ID identifying customers of Verizon Wireless; also known as \"perma-cookie\" or \"supercookie\"");
            yield return ("X-Csrf-Token"             , "Used to prevent cross-site request forgery. Alternative header names are: X-CSRFToken and X-XSRF-TOKEN");
            yield return ("X-Request-ID"             , "Correlates HTTP requests between a client and server.");
            yield return ("X-Correlation-ID"         , "Correlates HTTP requests between a client and server.");
            yield return ("Correlation-ID"           , "Correlates HTTP requests between a client and server.");
            yield return ("Save-Data"                , "The Save-Data client hint request header available in Chrome, Opera, and Yandex browsers lets developers deliver lighter, faster applications to users who opt-in to data saving mode in their browser.");
            yield return ("Sec-GPC"                  , "The Sec-GPC (Global Privacy Control) request header indicates whether the user consents to a website or service selling or sharing their personal information with third parties.");
        }
        public void SetRequestHeaders( IDictionary< string, string > requestHeaders )
        {
            var max_name_length = GetRegularRequestHeader().Max( t => t.name.Length );
            if ( requestHeaders.AnyEx() )
            {
                max_name_length = Math.Max( max_name_length, requestHeaders.Keys.Max( k => k.Length ) );
            }

            _AllRequestHeaders4DropDown = new List< RequestHeader >( 100 );            
            var hs_allRequestHeaders4DropDown = new HashSet< string >( 100 );
            foreach ( var (name, descr)  in GetRegularRequestHeader() )
            {
                if ( hs_allRequestHeaders4DropDown.Add( name ) )
                {
                    _AllRequestHeaders4DropDown.Add( new RequestHeader( name/*, descr, max_name_length*/ ) );
                }
            }
            DGV_keyColumn.DropDownWidth = 250; // 750;

            if ( requestHeaders.AnyEx() )
            {
                foreach ( var key in requestHeaders.Keys )
                {
                    if ( hs_allRequestHeaders4DropDown.Add( key ) )
                    {
                        _AllRequestHeaders4DropDown.Add( new RequestHeader( key ) );
                    }
                }
            }
            _AllRequestHeaders4DropDown.Sort( (x, y) => string.Compare( x.DisplayName, y.DisplayName, true ) );

            keyBindingSource.DataSource = _AllRequestHeaders4DropDown;

            //-------------------------------------------------//

            if ( requestHeaders.AnyEx() )
            {
                //DGV.RowsAdded -= DGV_RowsAdded;
                //try
                //{
                var rows = DGV.Rows;
                rows.Clear();
                foreach ( var p in requestHeaders )
                {
                    rows.Add( true, p.Key, p.Value );
                }
                //}
                //finally
                //{
                //    DGV.RowsAdded += DGV_RowsAdded;
                //}
            }
            else
            {
                OnRequestHeadersCountChanged?.Invoke( DGV.RowCount - 1, 0 );
            }

            DGV_Resize( null, EventArgs.Empty );
        }
        private void AppendRequestHeaders( IDictionary< string, string > requestHeaders )
        {            
            if ( !requestHeaders.AnyEx() )
            {
                return;
            }

            var hs_allRequestHeaders4DropDown = new HashSet< string >( 100 ) { _AllRequestHeaders4DropDown.Select( rh => rh.Name ) };

            var cnt = _AllRequestHeaders4DropDown.Count;
            foreach ( var key in requestHeaders.Keys )
            {
                if ( hs_allRequestHeaders4DropDown.Add( key ) )
                {
                    _AllRequestHeaders4DropDown.Add( new RequestHeader( key ) );
                }
            }
            //-------------------------------------------------//

            if ( cnt != _AllRequestHeaders4DropDown.Count )
            {
                _AllRequestHeaders4DropDown.Sort( (x, y) => string.Compare( x.DisplayName, y.DisplayName, true ) );
                keyBindingSource.DataSource = _AllRequestHeaders4DropDown;
            }
            //-------------------------------------------------//

            var rows = DGV.Rows;
            foreach ( var p in requestHeaders )
            {
                rows.Add( true, p.Key, p.Value );
            }
            //-------------------------------------------------//

            DGV_Resize( null, EventArgs.Empty );
        }
        public IDictionary< string, string > GetRequestHeaders()
        {
            //var dict = new Dictionary< string, string >( DGV.RowCount - 1, StringComparer.InvariantCultureIgnoreCase );
            var dict = new SortedDictionary< string, string >( StringComparer.InvariantCultureIgnoreCase );
            var rows = DGV.Rows;
            for ( var i = DGV.RowCount - 1; 0 <= i; i--  )
            {
                var row = rows[ i ];
                if ( !row.IsNewRow )
                {
                    var isChecked = bool.TryParse( row.Cells[ CHECKED_CELL_IDX ].Value?.ToString(), out var b ) && b; if ( !isChecked ) continue;
                    var key   = row.Cells[ KEY_CELL_IDX   ].Value?.ToString().Trim(); if ( key.IsNullOrWhiteSpace() ) continue;
                    var value = row.Cells[ VALUE_CELL_IDX ].Value?.ToString().Trim(); //if ( value.IsNullOrWhiteSpace() ) continue;
                    dict[ key ] = value;
                }
            }
            return (dict);
        }
        public bool InEditMode => DGV.IsCurrentCellInEditMode;

        private int GetEnabledCount()
        {
            var rows = DGV.Rows;
            var enabledCount = 0;
            for ( var i = DGV.RowCount - 1; 0 <= i; i--  )
            {
                var row = rows[ i ];
                if ( !row.IsNewRow )
                {
                    var isChecked = bool.TryParse( row.Cells[ CHECKED_CELL_IDX ].Value?.ToString(), out var b ) && b; 
                    if ( isChecked )
                    {
                        var key = row.Cells[ KEY_CELL_IDX ].Value?.ToString().Trim();
                        if ( !key.IsNullOrWhiteSpace() )
                        {
                            enabledCount++;
                        }
                    }
                }
            }
            return (enabledCount);
        }
        #endregion

        #region [.private methods.]
        private void DGV_Resize( object sender, EventArgs e )
        {
            var vscrollBarVisible = DGV.Controls.OfType< VScrollBar >().First().Visible;
            DGV_valueColumn.Width = DGV.Width - (DGV_keyColumn.Width + DGV_checkedColumn.Width) - DGV.RowHeadersWidth 
                                    - (vscrollBarVisible ? SystemInformation.VerticalScrollBarWidth : 0)
                                    - ((DGV.BorderStyle != BorderStyle.None) ? SystemInformation.FixedFrameBorderSize.Width : SystemInformation.BorderSize.Width);
        }
        private void DGV_ColumnWidthChanged( object sender, DataGridViewColumnEventArgs e )
        {
            if ( e.Column != DGV_valueColumn ) DGV_Resize( sender, EventArgs.Empty );
        }
        private void DGV_RowHeadersWidthChanged( object sender, EventArgs e ) => DGV_Resize( sender, EventArgs.Empty );
        private void DGV_CellValueChanged( object sender, DataGridViewCellEventArgs e )
        {
            if ( e.RowIndex < 0 ) return;

            if ( e.ColumnIndex == DGV_checkedColumn.DisplayIndex )
            {                
                var isChecked = Convert.ToBoolean( DGV.Rows[ e.RowIndex ].Cells[ e.ColumnIndex ].Value );

                #region [.spread to selected rows.]
                var srs = DGV.SelectedRowsBuf/*DGV.SelectedRows.Cast< DataGridViewRow >()*/;
                if ( 0 < srs.Count )
                {
                    DGV.CellValueChanged -= DGV_CellValueChanged;
                    try
                    {
                        foreach ( var selRow in srs )
                        {
                            if ( selRow.Index != e.RowIndex )
                            {
                                selRow.Cells[ e.ColumnIndex ].Value = isChecked;
                            }
                        }
                    }
                    finally
                    {
                        DGV.CellValueChanged += DGV_CellValueChanged;
                    }
                }
                #endregion
            }

            Fire_OnRequestHeadersCountChanged();
        }
        private void DGV_CellContentClick( object sender, DataGridViewCellEventArgs e )
        {
            if ( DGV.IsCurrentCellInEditMode )
            {
                DGV.CommitEdit( DataGridViewDataErrorContexts.Commit );
            }
        }
        private bool DGV_IsNeedSaveSelectionByMouseDown( MouseEventArgs e )
        {
            var ht = DGV.HitTest( e.X, e.Y );
            return ((0 <= ht.RowIndex) && (ht.ColumnIndex == DGV_checkedColumn.DisplayIndex));
        }
        private void DGV_ColumnHeaderMouseClick( object sender, DataGridViewCellMouseEventArgs e )
        {
            if ( (e.ColumnIndex == DGV_checkedColumn.DisplayIndex) && (0 < DGV.RowCount) )
            {
                var row_0 = DGV.Rows[ 0 ]; if ( row_0.IsNewRow ) return;
                var isChecked = !Convert.ToBoolean( row_0.Cells[ e.ColumnIndex ].Value );

                DGV.CellValueChanged -= DGV_CellValueChanged;
                try
                {
                    var rows = DGV.Rows;
                    for ( var i = DGV.RowCount - 1; 0 <= i; i--  )
                    {
                        var row = rows[ i ];
                        if ( !row.IsNewRow )
                        {
                            row.Cells[ e.ColumnIndex ].Value = isChecked;
                        }
                    }
                }
                finally
                {
                    DGV.CellValueChanged += DGV_CellValueChanged;
                }

                if ( DGV.IsCurrentCellInEditMode )
                {
                    DGV.EndEdit();
                }

                Fire_OnRequestHeadersCountChanged();
            }
        }
        private void DGV_RowsAdded( object sender, DataGridViewRowsAddedEventArgs e )
        {
            var cell = DGV.Rows[ e.RowIndex ].Cells[ CHECKED_CELL_IDX/*DGV_checkedColumn.DisplayIndex*/ ];
            if ( cell.Value == null )
            {
                cell.Value = true;
            }

            Fire_OnRequestHeadersCountChanged();
        }
        private void DGV_RowsRemoved( object sender, DataGridViewRowsRemovedEventArgs e ) => Fire_OnRequestHeadersCountChanged();
        /*private void DGV_CellToolTipTextNeeded( object sender, DataGridViewCellToolTipTextNeededEventArgs e )
        {
            if ( 0 <= e.RowIndex )
            {
                switch ( e.ColumnIndex )
                {
                    case CHECKED_CELL_IDX: e.ToolTipText = "enable/disable"; break;
                    case KEY_CELL_IDX    : e.ToolTipText = DGV.Rows[ e.RowIndex ].Cells[ e.ColumnIndex ].Value?.ToString(); break;
                    case VALUE_CELL_IDX  : e.ToolTipText = DGV.Rows[ e.RowIndex ].Cells[ e.ColumnIndex ].Value?.ToString(); break;
                }
            }
        }
        //*/
        private void DGV_KeyDown( object sender, KeyEventArgs e )
        {
            switch ( e.KeyCode )
            {
                case Keys.Delete:
                    foreach ( var cell in DGV.SelectedCells.Cast< DataGridViewCell >() )
                    {
                        if ( cell.ColumnIndex != CHECKED_CELL_IDX ) cell.Value = null;
                    }
                    break;

                case Keys.V:
                    if ( e.Control ) goto case Keys.Insert;
                    break;
                case Keys.Insert:
                    if ( ClipboardHelper.TryGetHeadersFromClipboard( out var headers ) )
                    {
                        AppendRequestHeaders( headers );
                    }
                    break;
            }
        }

        private void DGV_EditingControlShowing( object sender, DataGridViewEditingControlShowingEventArgs e )
        {
            if ( e.Control is DataGridViewComboBoxEditingControl cbec )
            {
                cbec.DropDownStyle = ComboBoxStyle.DropDown;

                if ( cbec.EditingControlDataGridView.Rows[ cbec.EditingControlRowIndex ].IsNewRow )
                {
                    cbec.Text = null;
                }

                if ( _DataGridViewComboBoxEditingControl != cbec )
                {
                    _DataGridViewComboBoxEditingControl = cbec;
                    cbec.PreviewKeyDown -= Cbec_PreviewKeyDown;
                    cbec.PreviewKeyDown += Cbec_PreviewKeyDown;
                    cbec.KeyDown -= Cbec_KeyDown;
                    cbec.KeyDown += Cbec_KeyDown;
                }
            }
            else if ( e.Control is ComboBox cb )
            {
                cb.DropDownStyle = ComboBoxStyle.DropDown;
            }
        }
        
        private DataGridViewComboBoxEditingControl _DataGridViewComboBoxEditingControl;
        private void Cbec_KeyDown( object sender, KeyEventArgs e )
        {
            if ( (e.KeyCode == Keys.Enter) && (_DataGridViewComboBoxEditingControl != null) && !_DataGridViewComboBoxEditingControl.Text.IsNullOrWhiteSpace() && (_DataGridViewComboBoxEditingControl.SelectedIndex == -1) )
            {
                var txt = _DataGridViewComboBoxEditingControl.Text;
                if ( _AllRequestHeaders4DropDown.FirstOrDefault( r => r.Name == txt ) == null )
                {
                    _AllRequestHeaders4DropDown.Add( new RequestHeader( txt ) );
                    keyBindingSource.DataSource = _AllRequestHeaders4DropDown;

                    _DataGridViewComboBoxEditingControl.DataSource = null;
                    _DataGridViewComboBoxEditingControl.DataSource = keyBindingSource;
                    _DataGridViewComboBoxEditingControl.SelectedIndex = _AllRequestHeaders4DropDown.Count - 1;
                    if ( DGV.IsCurrentCellInEditMode )
                    {
                        DGV.CommitEdit( DataGridViewDataErrorContexts.Commit );
                        DGV.EndEdit();
                    }

                    Fire_OnRequestHeadersCountChanged();
                }
            }
        }
        private void Cbec_PreviewKeyDown( object sender, PreviewKeyDownEventArgs e )
        {
            if ( (e.KeyCode == Keys.Enter) && (_DataGridViewComboBoxEditingControl != null) && !_DataGridViewComboBoxEditingControl.Text.IsNullOrWhiteSpace() && (_DataGridViewComboBoxEditingControl.SelectedIndex == -1) )
            {
                e.IsInputKey = true;
            }
        }

        private void DGV_DataError( object sender, DataGridViewDataErrorEventArgs e )
        {
            if ( (0 <= e.RowIndex) && (e.ColumnIndex == DGV_keyColumn.DisplayIndex) )
            {
                DGV[ e.ColumnIndex, e.RowIndex ].Value = null;
            }
        }
        #endregion

        #region [.filter.]
        private void clearFilterButton_Click( object sender, EventArgs e ) => filterTextBox.Text = null;

        private string _LastFilterText;
        private async void filterTextBox_TextChanged( object sender, EventArgs e )
        {
            var text = filterTextBox.Text.Trim();
            if ( _LastFilterText != text )
            {
                await Task.Delay( 250 );
                _LastFilterText = text;                
                filterTextBox_TextChanged( sender, e );
                return;
            }

            var isEmpty = text.IsNullOrEmpty();

            clearFilterButton.Visible = !isEmpty;

            DGV.SuspendDrawing();
            try
            {
                if ( isEmpty )
                {
                    foreach ( var row in DGV.Rows.Cast< DataGridViewRow >() )
                    {
                        row.Visible = true;
                    }
                }
                else
                {
                    foreach ( var row in DGV.Rows.Cast< DataGridViewRow >() )
                    {
                        if ( !row.IsNewRow )
                        {
                            var key   = row.Cells[ KEY_CELL_IDX   ].Value?.ToString();
                            var value = row.Cells[ VALUE_CELL_IDX ].Value?.ToString();
                            row.Visible = key.ContainsIgnoreCase( text ) || value.ContainsIgnoreCase( text );
                        }
                    }
                }
            }
            finally
            {
                DGV.ResumeDrawing();
            }
        }
        #endregion
    }
}
