using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using m3u8.download.manager.ipc;
using m3u8.download.manager.models;
using m3u8.download.manager.Properties;

using static System.Net.Mime.MediaTypeNames;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static class ClipboardHelper
    {
        private const char CLIP_BRD__URL_REQ_HEAD_SEP_CHAR = '\t'; //'\u0001';
        public static bool TryGetM3u8FileUrlsFromClipboard( out IReadOnlyCollection< (string url, string requestHeaders) > m3u8FileUrls, bool ignoreHostHeader )
        {
            var M3U8_EXTENSION_Q = Resources.M3U8_EXTENSION + '?';
            try
            {
                var text = Clipboard.GetText( TextDataFormat.Text )?.Trim();
                if ( text.IsNullOrEmpty() ) text = Clipboard.GetText( TextDataFormat.UnicodeText )?.Trim();
                
                if ( !text.IsNullOrEmpty() )
                {
                    var array = text.Split( [ '\r', '\n' ], StringSplitOptions.RemoveEmptyEntries );
                    var hs  = new HashSet< string >( array.Length, StringComparer.InvariantCultureIgnoreCase );
                    var lst = new List< (string url, string requestHeaders) >( array.Length );
                    foreach ( var a in array )
                    {
                        var s_row          = a.Trim();
                        var i              = s_row.IndexOf( CLIP_BRD__URL_REQ_HEAD_SEP_CHAR );
                        var url            = (i != -1) ? s_row.Substring( 0, i ) : s_row;
                        var requestHeaders = (i != -1) ? s_row.Substring( i + 1 ) : null;
                        if ( !BrowserIPC.ExtensionRequestHeader.Try2Dict( requestHeaders, out var dict, ignoreHostHeader ) || !dict.AnyEx() )
                        {
                            requestHeaders = null;
                        }

                        if ( url.EndsWith( Resources.M3U8_EXTENSION, StringComparison.InvariantCultureIgnoreCase ) && hs.Add( url ) )
                        {
                            lst.Add( (url, requestHeaders) );
                        }
                        else
                        {
                            i = url.IndexOf( M3U8_EXTENSION_Q, StringComparison.InvariantCultureIgnoreCase );
                            if ( (10 < i) && hs.Add( url ) )
                            {
                                lst.Add( (url, requestHeaders) );
                            }
                        }
                    }
                    m3u8FileUrls = lst;
                    return (m3u8FileUrls.Any());
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }

            m3u8FileUrls = default;
            return (false);
        }
        public static IReadOnlyCollection< (string url, string requestHeaders) > TryGetM3u8FileUrlsFromClipboardOrDefault( bool ignoreHostHeader ) => (TryGetM3u8FileUrlsFromClipboard( out var m3u8FileUrls, ignoreHostHeader ) ? m3u8FileUrls : Array.Empty< (string url, string requestHeaders) >());
        public static bool TryGetHttpUrlsFromClipboard( out IReadOnlyCollection< (string url, string requestHeaders) > urls, bool ignoreHostHeader )
        {
            const string HTTP  = "http://";
            const string HTTPS = "https://";
            try
            {
                var text = Clipboard.GetText( TextDataFormat.Text )?.Trim();
                if ( text.IsNullOrEmpty() ) text = Clipboard.GetText( TextDataFormat.UnicodeText )?.Trim();

                if ( !text.IsNullOrEmpty() )
                {
                    var array = text.Split( [ '\r', '\n' ], StringSplitOptions.RemoveEmptyEntries );
                    var hs  = new HashSet< string >( array.Length, StringComparer.InvariantCultureIgnoreCase );
                    var lst = new List< (string url, string requestHeaders) >( array.Length );
                    foreach ( var a in array )
                    {
                        var s_row = a.Trim();
                        if ( s_row.StartsWith( HTTP , StringComparison.InvariantCultureIgnoreCase ) ||
                             s_row.StartsWith( HTTPS, StringComparison.InvariantCultureIgnoreCase ) )
                        {
                            var i = s_row.IndexOf( CLIP_BRD__URL_REQ_HEAD_SEP_CHAR );
                            var url = (i != -1) ? s_row.Substring( 0, i ) : s_row;
                            if ( hs.Add( url ) )
                            {
                                var requestHeaders = (i != -1) ? s_row.Substring( i + 1 ) : null;
                                if ( !BrowserIPC.ExtensionRequestHeader.Try2Dict( requestHeaders, out var dict, ignoreHostHeader ) || !dict.AnyEx() )
                                {
                                    requestHeaders = null;
                                }
                                lst.Add( (url, requestHeaders) );
                            }
                        }
                    }
                    urls = lst;
                    return (urls.Any());
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }

            urls = default;
            return (false);
        }        
        public static void CopyUrlsToClipboard( IEnumerable< DownloadRow > rows )
        {
            var txt = string.Join( "\r\n", rows.Select( r => r.RequestHeaders.AnyEx() ? $"{r.Url}{CLIP_BRD__URL_REQ_HEAD_SEP_CHAR}{BrowserIPC.ExtensionRequestHeader.ToJson( r.RequestHeaders )}" : r.Url ) );
            Clipboard.SetText( txt, TextDataFormat.UnicodeText );
        }

        public static bool TryGetHeadersFromClipboard( out IDictionary< string, string > headers, bool ignoreHostHeader )
        {
            const char COLON = ':';
            const char TAB   = '\t';
            try
            {
                var text = Clipboard.GetText( TextDataFormat.Text )?.Trim();
                if ( text.IsNullOrEmpty() ) text = Clipboard.GetText( TextDataFormat.UnicodeText )?.Trim();

                if ( !text.IsNullOrEmpty() )
                {
                    var array = text.Split( [ '\r', '\n' ], StringSplitOptions.RemoveEmptyEntries );
                    var dict  = new Dictionary< string, string >( array.Length, StringComparer.InvariantCultureIgnoreCase );
                    var anyOf = new[] { COLON, TAB };

                    foreach ( var a in array )
                    {
                        var s_row = a.Trim();
                        var i     = s_row.IndexOfAny( anyOf ); if ( i == -1 ) break;
                        var name  = s_row.Substring( 0,  i ).Trim(); if ( name.IsNullOrEmpty() ) break;
                        var value = s_row.Substring( i + 1 ).Trim();

                        dict[ name ] = value;
                    }
                    headers = dict;
                    return (dict.Any());
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }

            headers = default;
            return (false);
        }
        public static void CopyHeadersToClipboard( IDictionary< string, string > headers )
        {
            const char COLON = ':';

            var buf = new StringBuilder();
            foreach ( var p in headers )
            {
                buf.Append( p.Key ).Append( COLON ).Append( p.Value ).AppendLine();
            }
            var txt = buf.ToString();
            Clipboard.SetText( txt, TextDataFormat.UnicodeText );
        }
    }
}
