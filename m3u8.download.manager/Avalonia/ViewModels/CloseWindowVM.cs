using Avalonia.Controls;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class CloseWindowVM
    {
        public CloseWindowVM( Window window ) => CloseWindowCommand = new CloseWindowCommand( window );
        public CloseWindowCommand CloseWindowCommand { get; }
    }
}
