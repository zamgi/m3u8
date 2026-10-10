using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

using m3u8.download.manager.models;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static class DownloadRowsSerializer
    {
        /// <summary>
        /// 
        /// </summary>
        [DataContract] private sealed class DownloadRow_4_Serialize
        {
            public DownloadRow_4_Serialize() { }
            public DownloadRow_4_Serialize( DownloadRow r, ExternalProgRunnerStatusTypeEnum externalProgRunnerStatus )
            {
                CreatedOrStartedDateTime     = r.CreatedOrStartedDateTime;
                Url                          = r.Url;
                OutputDirectory              = r.OutputDirectory;
                IsLiveStream                 = r.IsLiveStream;
                LiveStreamMaxFileSizeInBytes = r.LiveStreamMaxFileSizeInBytes;
                RequestHeaders               = r.RequestHeaders;
                WebProxyInfo                 = new web_proxy_info_4_Serialize( r.WebProxyInfo );
                Timeout                      = r.Timeout;
                AttemptRequestCount          = r.AttemptRequestCount;
                Status                       = r.Status;
                DownloadBytesLength          = r.DownloadBytesLength;
                FinitaElapsed                = r.GetElapsed();
                ExternalProgRunnerStatus     = externalProgRunnerStatus;

                if ( r.IsLiveStream && !r.VeryFirstOutputFullFileName.IsNullOrEmpty() )
                {
                    var fn = Path.GetFileName( r.VeryFirstOutputFullFileName );
                    OutputFileName = fn.IsNullOrEmpty() ? r.VeryFirstOutputFullFileName : fn;
                }
                else
                {
                    OutputFileName = r.OutputFileName;
                }
            }
            [DataMember(Name="c")] public DateTime       CreatedOrStartedDateTime      { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="u")] public string         Url                           { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="f")] public string         OutputFileName                { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="d")] public string         OutputDirectory               { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="s")] public DownloadStatus Status                        { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="x")] public bool           IsLiveStream                  { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="y")] public long           LiveStreamMaxFileSizeInBytes  { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="r")] public IDictionary< string, string > RequestHeaders { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="w")] public web_proxy_info_4_Serialize    WebProxyInfo   { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="t")] public TimeSpan?      Timeout                       { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="a")] public int?           AttemptRequestCount           { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="b")] public long           DownloadBytesLength           { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="e")] public TimeSpan       FinitaElapsed                 { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="p")] public ExternalProgRunnerStatusTypeEnum ExternalProgRunnerStatus { [M(O.AggressiveInlining)] get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        [DataContract] private sealed class web_proxy_info_4_Serialize
        {
            public web_proxy_info_4_Serialize() { }
            public web_proxy_info_4_Serialize( in web_proxy_info t )
            {
                UseWebProxy = t.UseWebProxy;
                UrlType     = t.UrlType;
                Hostname    = t.Hostname;
                Port        = t.Port;
                Credentials = t.Credentials;
            }
            public web_proxy_info ToWebProxyInfo() => new web_proxy_info()
            {
                UseWebProxy = UseWebProxy,
                UrlType     = UrlType,
                Hostname    = Hostname,
                Port        = Port,
                Credentials = Credentials,
            };

            [DataMember(Name="u")] public bool UseWebProxy { get; set; }
            [DataMember(Name="t")] public WebProxyUrlEnumType UrlType { get; set; }
            [DataMember(Name="h")] public string Hostname { get; set; }
            [DataMember(Name="p")] public int?   Port     { get; set; }
            [DataMember(Name="c")] public (string Username, string Password) Credentials { get; set; }
        }

        public static string ToJSON( IEnumerable< DownloadRow > rows, Func< DownloadRow, ExternalProgRunnerStatusTypeEnum > get_ExternalProgRunnerStatus_Func )
            => rows.Select( r => new DownloadRow_4_Serialize( r, get_ExternalProgRunnerStatus_Func( r ) ) ).ToJSON();
        public static IEnumerable< DownloadRow_Definer_3 > FromJSON( string json )
        {
            if ( !json.IsNullOrWhiteSpace() )
            {
                try
                {
                    var rows = from r in Extensions.FromJSON< List< DownloadRow_4_Serialize > >( json )
                               let is_FinishedReplaced = (r.Status == DownloadStatus.FinishedReplaced)
                               let is_Finished         = r.Status.IsFinished()
                               select new DownloadRow_Definer_3()
                               {
                                   CreatedOrStartedDateTime     = r.CreatedOrStartedDateTime,
                                   Url                          = r.Url,
                                   RequestHeaders               = r.RequestHeaders, 
                                   WebProxyInfo                 = r.WebProxyInfo?.ToWebProxyInfo() ?? web_proxy_info.Empty,
                                   Timeout                      = r.Timeout,
                                   AttemptRequestCount          = r.AttemptRequestCount,
                                   OutputFileName               = r.OutputFileName, 
                                   OutputDirectory              = r.OutputDirectory, 
                                   Status                       = is_FinishedReplaced ? DownloadStatus.FinishedReplaced : DownloadStatus.Created/*r.Status*/,
                                   DownloadBytesLength          = is_FinishedReplaced ? r.DownloadBytesLength : null,
                                   FinitaElapsed                = is_Finished         ? r.FinitaElapsed       : null,
                                   IsLiveStream                 = r.IsLiveStream, 
                                   LiveStreamMaxFileSizeInBytes = r.LiveStreamMaxFileSizeInBytes,
                                   ExternalProgRunnerStatus     = r.ExternalProgRunnerStatus 
                               };
                    return (rows);
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }
            }
            return (Enumerable.Empty< DownloadRow_Definer_3 >());
        }

        public static string ToJSON( in web_proxy_info t ) => (new web_proxy_info_4_Serialize( t )).ToJSON();
        public static web_proxy_info FromJSON_2_WebProxyInfo( string json, bool suppressError = false )
        {
            if ( !json.IsNullOrWhiteSpace() )
            {
                try
                {
                    var webProxyInfo = Extensions.FromJSON< web_proxy_info_4_Serialize >( json );
                    return (webProxyInfo.ToWebProxyInfo());
                }
                catch ( Exception ex ) when ( suppressError )
                {
                    Debug.WriteLine( ex );
                }
            }
            return (web_proxy_info.Empty);
        }
    }
}
