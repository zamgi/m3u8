namespace m3u8.download.manager.ipc
{
    ///// <summary>
    ///// 
    ///// </summary>
    //internal abstract class UrlInputParamsBase
    //{
    //    public abstract bool isGroupByAudioVideo { get; }
    //    public bool autoStartDownload { get; init; }
    //}

    /// <summary>
    /// 
    /// </summary>
    internal readonly struct UrlInputParams
    {
        public bool isGroupByAudioVideo   { get; init; }

        public string videoUrl            { get; init; }
        public string videoRequestHeaders { get; init; }
        public string audioUrl            { get; init; }
        public string audioRequestHeaders { get; init; }


        public string m3u8FileUrl    { get; init; }
        public string requestHeaders { get; init; }


        public bool autoStartDownload { get; init; }


        public static UrlInputParams Create( string url, string requestHeaders, bool autoStartDownload ) => new UrlInputParams()
        {
            m3u8FileUrl       = url,
            requestHeaders    = requestHeaders,
            autoStartDownload = autoStartDownload,
        };
        public static UrlInputParams Create( in (string url, string requestHeaders, bool autoStartDownload) t ) => new UrlInputParams()
        {
            m3u8FileUrl       = t.url,
            requestHeaders    = t.requestHeaders,
            autoStartDownload = t.autoStartDownload,
        };


#if DEBUG
        public override string ToString() => isGroupByAudioVideo ? $"isGroupByAudioVideo=true" : $"{m3u8FileUrl}";
#endif
    }
}
