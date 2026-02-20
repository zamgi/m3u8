namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static partial class HttpHeaderHelper
    {
        public const string HEADER_HOST = "Host";
        public static bool IsHeader_Host( string s ) => (string.Compare( s, HEADER_HOST, true ) == 0);
    }
}
