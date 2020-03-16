using m3u8.download.manager.ui;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class AddNewDownloadFormVM
    {
        public AddNewDownloadFormVM( AddNewDownloadForm window )
            => CloseWindowCommand = new CloseWindowCommand( window );

        public CloseWindowCommand CloseWindowCommand { get; }
    }
}
