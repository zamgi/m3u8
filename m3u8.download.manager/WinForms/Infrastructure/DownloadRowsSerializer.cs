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
            public DownloadRow_4_Serialize( DownloadRow r )
            {
                CreatedOrStartedDateTime     = r.CreatedOrStartedDateTime;
                Url                          = r.Url;
                OutputDirectory              = r.OutputDirectory;
                IsLiveStream                 = r.IsLiveStream;
                LiveStreamMaxFileSizeInBytes = r.LiveStreamMaxFileSizeInBytes;
                RequestHeaders               = r.RequestHeaders;

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
            [DataMember(Name="c")] public DateTime CreatedOrStartedDateTime     { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="u")] public string   Url                          { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="f")] public string   OutputFileName               { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="d")] public string   OutputDirectory              { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="s")] public DownloadStatus Status                 { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="x")] public bool     IsLiveStream                 { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="y")] public long     LiveStreamMaxFileSizeInBytes { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="r")] public IDictionary< string, string > RequestHeaders { [M(O.AggressiveInlining)] get; set; }
        }

        public static string ToJSON( IEnumerable< DownloadRow > rows ) => rows.Select( r => new DownloadRow_4_Serialize( r ) ).ToJSON();
        public static IEnumerable< (DateTime CreatedOrStartedDateTime, string Url, IDictionary< string, string > RequestHeaders, string OutputFileName, string OutputDirectory, DownloadStatus Status, bool IsLiveStream, long LiveStreamMaxFileSizeInBytes) > FromJSON( string json )
        {
            if ( !json.IsNullOrWhiteSpace() )
            {
                try
                {
                    var rows = Extensions.FromJSON< DownloadRow_4_Serialize[] >( json ).Select( r => 
                        (r.CreatedOrStartedDateTime, r.Url, r.RequestHeaders, r.OutputFileName, r.OutputDirectory, DownloadStatus.Created/*r.Status*/, r.IsLiveStream, r.LiveStreamMaxFileSizeInBytes) );
                    return (rows);
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }
            }
            return (Enumerable.Empty< (DateTime CreatedOrStartedDateTime, string Url, IDictionary< string, string > RequestHeaders, string OutputFileName, string OutputDirectory, DownloadStatus Status, bool IsLiveStream, long LiveStreamMaxFileSizeInBytes) >());
        }
    }
}
