using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Input.Platform;

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

        //public static async Task< IReadOnlyCollection< DownloadRow_Definer_3 > > TryGetM3u8FileUrlsFromClipboardOrDefault( this Window window, SettingsPropertyChangeController sc )
        //{
        //    var M3U8_EXTENSION_Q = Resources.M3U8_EXTENSION + '?';
        //    try
        //    {
        //        var text = (await window.Clipboard.TryGetTextAsync().CAX())?.Trim();
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
        //            }
        //            return (lst);
        //        }
        //    }
        //    catch ( Exception ex )
        //    {
        //        Debug.WriteLine( ex );
        //    }
        //    return (Array.Empty< DownloadRow_Definer_3 >());
        //}

        public static async Task< IReadOnlyCollection< DownloadRow_Definer_3 > > TryGetHttpUrlsFromClipboardOrDefault( this Window window, SettingsPropertyChangeController sc )
        {
            var (suc, urls) = await window.TryGetHttpUrlsFromClipboard( sc ).CAX();
            return (suc ?  urls : Array.Empty< DownloadRow_Definer_3 >());
        }
        public static async Task< (bool success, IReadOnlyCollection< DownloadRow_Definer_3 > urls) > TryGetHttpUrlsFromClipboard( this Window window, SettingsPropertyChangeController sc )
        {
            try
            {
                var text = (await window.Clipboard.TryGetTextAsync().CAX())?.Trim();
                if ( !text.IsNullOrEmpty() )
                {
                    var ignoreHostHeader = sc.IgnoreHostHttpHeader;
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
                    return (lst.Any(), lst);
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
            return (false, default);
        }        
        public static Task CopyToClipboard( this Window window, IEnumerable< DownloadRow > rows )
        {
            var txt = string.Join( "\r\n", DownloadRowsSerializer.ToJSON( rows ) );
            return (window.Clipboard.SetTextAsync( txt ));
        }

        public static Task CopyToClipboard( this Window window, string txt ) => window.Clipboard.SetTextAsync( txt );
        public static Task< string > GetFromClipboard( this Window window ) => window.Clipboard.TryGetTextAsync();

        public static async Task< (bool success, IDictionary< string, string > headers) > TryGetHeadersFromClipboard( this Window window, bool ignoreHostHeader )
        {
            const char COLON = ':';
            const char TAB   = '\t';
            try
            {
                var text = (await window.Clipboard.TryGetTextAsync().CAX())?.Trim();
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
                    return (dict.Any(), headers: dict);
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }

            return (false, default);
        }
        public static Task CopyHeadersToClipboard( this Window window, IDictionary< string, string > headers )
        {
            const char COLON = ':';

            var buf = new StringBuilder();
            foreach ( var p in headers )
            {
                buf.Append( p.Key ).Append( COLON ).Append( p.Value ).AppendLine();
            }
            var txt = buf.ToString();
            return (window.Clipboard.SetTextAsync( txt ));
        }
    }
}
