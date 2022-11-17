using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                CreatedOrStartedDateTime = r.CreatedOrStartedDateTime;
                Url                      = r.Url;
                OutputFileName           = r.OutputFileName;
                OutputDirectory          = r.OutputDirectory;
            }
            [DataMember(Name="c")] public DateTime CreatedOrStartedDateTime { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="u")] public string   Url                      { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="f")] public string   OutputFileName           { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="d")] public string   OutputDirectory          { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="s")] public DownloadStatus Status             { [M(O.AggressiveInlining)] get; set; }
        }

        public static string ToJSON( IEnumerable< DownloadRow > rows ) => rows.Select( r => new DownloadRow_4_Serialize( r ) ).ToJSON();
        public static IEnumerable< (DateTime CreatedOrStartedDateTime, string Url, string OutputFileName, string OutputDirectory, DownloadStatus Status) > FromJSON( string json )
        {
            if ( !json.IsNullOrWhiteSpace() )
            {
                try
                {
                    var rows = Extensions.FromJSON< DownloadRow_4_Serialize[] >( json ).Select( r => (r.CreatedOrStartedDateTime, r.Url, r.OutputFileName, r.OutputDirectory, DownloadStatus.Created/*r.Status*/) );
                    return (rows);
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }
            }
            return (Enumerable.Empty< (DateTime CreatedOrStartedDateTime, string Url, string OutputFileName, string OutputDirectory, DownloadStatus Status) >());
        }
    }
}
