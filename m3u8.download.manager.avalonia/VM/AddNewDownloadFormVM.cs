using m3u8.download.manager.ui;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class AddNewDownloadFormVM
    {
        public AddNewDownloadFormVM( AddNewDownloadForm window )
        {
            //StartDownloadCommand = new StartDownloadCommand();
            CloseWindowCommand   = new CloseWindowCommand( window );
        }

        //public StartDownloadCommand StartDownloadCommand { get; }
        public CloseWindowCommand   CloseWindowCommand   { get; }
    }
}
