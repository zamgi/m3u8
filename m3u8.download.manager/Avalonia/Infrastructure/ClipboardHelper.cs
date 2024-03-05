using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using m3u8.download.manager.ipc;
using m3u8.download.manager.models;
using m3u8.download.manager.Properties;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static class ClipboardHelper
    {
        private const char CLIP_BRD__URL_REQ_HEAD_SEP_CHAR = '\t'; //'\u0001';
        public static async Task< IReadOnlyCollection< (string url, string requestHeaders) > > TryGetM3u8FileUrlsFromClipboardOrDefault( this Window window )
        {
            var t = await window.TryGetM3u8FileUrlsFromClipboard();
            return (t.success ? t.m3u8FileUrls : Array.Empty< (string url, string requestHeaders) >());
        }
        public static async Task< (bool success, IReadOnlyCollection< (string url, string requestHeaders) > m3u8FileUrls) > TryGetM3u8FileUrlsFromClipboard( this Window window )
        {
            var M3U8_EXTENSION_Q = Resources.M3U8_EXTENSION + '?';
            try
            {
                var text = (await window.Clipboard.GetTextAsync())?.Trim();
                if ( !text.IsNullOrEmpty() )
                {
                    var array = text.Split( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );
                    var hs    = new HashSet< string >( array.Length, StringComparer.InvariantCultureIgnoreCase );
                    var lst   = new List< (string url, string requestHeaders) >( array.Length );
                    foreach ( var a in array )
                    {
                        var s_row          = a.Trim();
                        var i              = s_row.IndexOf( CLIP_BRD__URL_REQ_HEAD_SEP_CHAR );
                        var url            = (i != -1) ? s_row.Substring( 0, i ) : s_row;
                        var requestHeaders = (i != -1) ? s_row.Substring( i + 1 ) : null;
                        if ( !BrowserIPC.ExtensionRequestHeader.Try2Dict( requestHeaders, out var dict ) || !dict.AnyEx() )
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
                    return (lst.Any(), lst);
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }

            return (false, default);
        }
        public static async Task< (bool success, IReadOnlyCollection< (string url, string requestHeaders) > m3u8FileUrls) > TryGetHttpUrlsFromClipboard( this Window window )
        {
            const string HTTP  = "http://";
            const string HTTPS = "https://";
            try
            {
                var text = (await window.Clipboard.GetTextAsync())?.Trim();
                if ( !text.IsNullOrEmpty() )
                {
                    var array = text.Split( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );
                    var hs    = new HashSet< string >( array.Length, StringComparer.InvariantCultureIgnoreCase );
                    var lst   = new List< (string url, string requestHeaders) >( array.Length );
                    foreach ( var a in array )
                    {
                        var s_row = a.Trim();
                        if ( s_row.StartsWith( HTTP, StringComparison.InvariantCultureIgnoreCase ) ||
                             s_row.StartsWith( HTTPS, StringComparison.InvariantCultureIgnoreCase ) )
                        {
                            var i = s_row.IndexOf( CLIP_BRD__URL_REQ_HEAD_SEP_CHAR );
                            var url = (i != -1) ? s_row.Substring( 0, i ) : s_row;
                            if ( hs.Add( url ) )
                            {
                                var requestHeaders = (i != -1) ? s_row.Substring( i + 1 ) : null;
                                if ( !BrowserIPC.ExtensionRequestHeader.Try2Dict( requestHeaders, out var dict ) || !dict.AnyEx() )
                                {
                                    requestHeaders = null;
                                }
                                lst.Add( (url, requestHeaders) );
                            }
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
            var txt = string.Join( "\r\n", rows.Select( r => r.RequestHeaders.AnyEx() ? $"{r.Url}{CLIP_BRD__URL_REQ_HEAD_SEP_CHAR}{BrowserIPC.ExtensionRequestHeader.ToJson( r.RequestHeaders )}" : r.Url ) );
            return (window.Clipboard.SetTextAsync( txt ));
        }
    }
}
