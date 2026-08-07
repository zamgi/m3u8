using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using m3u8.download.manager.controllers;
using m3u8.download.manager.models;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static class ClipboardHelper
    {
        private const string HTTP  = "http://";
        private const string HTTPS = "https://";

        private static bool IsValidUrl( string url ) => (url != null) && (url.StartsWith( HTTP , StringComparison.InvariantCultureIgnoreCase ) ||
                                                                          url.StartsWith( HTTPS, StringComparison.InvariantCultureIgnoreCase ));
        private static DownloadRow_Definer_3 Create_DownloadRow_Definer_3( string url, SettingsPropertyChangeController sc )
        {
            var (timeout, attemptRequestCountByPart) = sc.GetCreateM3u8ClientParams();
            var r = new DownloadRow_Definer_3()
            {
                Url = url,
                RequestHeaders           = null,
                OutputDirectory          = sc.OutputFileDirectory,
                OutputFileName           = null,
                CreatedOrStartedDateTime = DateTime.Now,
                Status                   = DownloadStatus.Created,
                AttemptRequestCount      = attemptRequestCountByPart,
                Timeout                  = timeout,
                WebProxyInfo             = sc.GetDefaultWebProxyInfo(),
                IsLiveStream             = false,
                LiveStreamMaxFileSizeInBytes = 0,
            };
            return (r);
        }

        //public static bool TryGetM3u8FileUrlsFromClipboard( out IReadOnlyCollection< DownloadRow_Definer_3 > m3u8FileUrls, SettingsPropertyChangeController sc )
        //{
        //    var M3U8_EXTENSION_Q = Resources.M3U8_EXTENSION + '?';
        //    try
        //    {
        //        var text = Clipboard.GetText( TextDataFormat.Text )?.Trim();
        //        if ( text.IsNullOrEmpty() ) text = Clipboard.GetText( TextDataFormat.UnicodeText )?.Trim();
                
        //        if ( !text.IsNullOrEmpty() )
        //        {
        //            var ignoreHostHeader = sc.IgnoreHostHttpHeader;

        //            var lines = text.Split( ['\r', '\n'], StringSplitOptions.RemoveEmptyEntries );
        //            var hs    = new HashSet< string >( lines.Length, StringComparer.InvariantCultureIgnoreCase );
        //            var lst   = new List< DownloadRow_Definer_3 >( lines.Length );
        //            foreach ( var a in lines )
        //            {
        //                var row_json = a.Trim();
        //                var r = DownloadRowsSerializer.FromJSON( row_json ).FirstOrDefault();
        //                if ( r == null ) r = Create_DownloadRow_Definer_3( url: row_json, sc );
        //                if ( ignoreHostHeader && r.RequestHeaders.AnyEx() ) r.RequestHeaders.Remove( HttpHeaderHelper.HEADER_HOST );

        //                if ( r.Url.EndsWith_Ex( Resources.M3U8_EXTENSION, StringComparison.InvariantCultureIgnoreCase ) && hs.Add( r.Url ) )
        //                {
        //                    lst.Add( r );
        //                }
        //                else
        //                {
        //                    var i = r.Url.IndexOf( M3U8_EXTENSION_Q, StringComparison.InvariantCultureIgnoreCase );
        //                    if ( (10 < i) && hs.Add( r.Url ) )
        //                    {
        //                        lst.Add( r );
        //                    }
        //                }
        //                #region comm
        //                //if ( r != null )
        //                //{
        //                //    if ( ignoreHostHeader && r.RequestHeaders.AnyEx() ) r.RequestHeaders.Remove( HttpHeaderHelper.HEADER_HOST );

        //                //    if ( r.Url.EndsWith_Ex( Resources.M3U8_EXTENSION, StringComparison.InvariantCultureIgnoreCase ) && hs.Add( r.Url ) )
        //                //    {
        //                //        lst.Add( r );
        //                //    }
        //                //    else
        //                //    {
        //                //        var i = r.Url.IndexOf( M3U8_EXTENSION_Q, StringComparison.InvariantCultureIgnoreCase );
        //                //        if ( (10 < i) && hs.Add( r.Url ) )
        //                //        {
        //                //            lst.Add( r );
        //                //        }
        //                //    }
        //                //} 
        //                #endregion
        //            }
        //            m3u8FileUrls = lst;
        //            return (m3u8FileUrls.Any());
        //        }
        //    }
        //    catch ( Exception ex )
        //    {
        //        Debug.WriteLine( ex );
        //    }

        //    m3u8FileUrls = default;
        //    return (false);
        //}

        public static IReadOnlyCollection< DownloadRow_Definer_3 > TryGetHttpUrlsFromClipboardOrDefault( SettingsPropertyChangeController sc ) => (TryGetHttpUrlsFromClipboard( out var m3u8FileUrls, sc ) ? m3u8FileUrls : Array.Empty< DownloadRow_Definer_3 >());
        public static bool TryGetHttpUrlsFromClipboard( out IReadOnlyCollection< DownloadRow_Definer_3 > urls, SettingsPropertyChangeController sc )
        {
            try
            {
                var text = Clipboard.GetText( TextDataFormat.Text )?.Trim();
                if ( text.IsNullOrEmpty() ) text = Clipboard.GetText( TextDataFormat.UnicodeText )?.Trim();

                if ( !text.IsNullOrEmpty() )
                {
                    var ignoreHostHeader = sc.IgnoreHostHttpHeader;
                    //var (timeout, attemptRequestCountByPart) = sc.GetCreateM3u8ClientParams();
                    //var webProxyInfo        = sc.GetDefaultWebProxyInfo();
                    //var outputFileDirectory = sc.OutputFileDirectory;
                    //---------------------------------------------------------------------//

                    var lines = text.Split( ['\r', '\n'], StringSplitOptions.RemoveEmptyEntries );
                    var hs    = new HashSet< string >( lines.Length, StringComparer.InvariantCultureIgnoreCase );
                    var lst   = new List< DownloadRow_Definer_3 >( lines.Length );
                    foreach ( var a in lines )
                    {
                        var row_json = a.Trim();
                        var r = DownloadRowsSerializer.FromJSON( row_json ).FirstOrDefault();
                        if ( r == null ) r = Create_DownloadRow_Definer_3( url: row_json, sc );
                        if ( IsValidUrl( r.Url ) && hs.Add( r.Url ) )
                        {
                            if ( ignoreHostHeader && r.RequestHeaders.AnyEx() ) r.RequestHeaders.Remove( HttpHeaderHelper.HEADER_HOST );
                            lst.Add( r );
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
            var txt = string.Join( "\r\n", DownloadRowsSerializer.ToJSON( rows ) );
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
                        if ( ignoreHostHeader && HttpHeaderHelper.IsHeader_Host( name ) ) continue;
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
