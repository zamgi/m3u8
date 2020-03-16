using m3u8.download.manager.controllers;
using m3u8.download.manager.models;
using m3u8.download.manager.ui;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class MainVM
    {
        public MainVM( MainWindow mainWindow )
        {
            SettingsController = new SettingsPropertyChangeController();

            DownloadListModel  = new DownloadListModel();
            DownloadController = new DownloadController( DownloadListModel, SettingsController );

            AddCommand         = new AddCommand( this );
            ParallelismCommand = new ParallelismCommand( this );
            SettingsCommand    = new SettingsCommand( this );
            AboutCommand       = new AboutCommand();
            FileNameExcludesWordsEditorCommand = new FileNameExcludesWordsEditorCommand( this );
        }

        public DownloadListModel                DownloadListModel  { get; }
        public DownloadController               DownloadController { get; }
        public SettingsPropertyChangeController SettingsController { get; }

        public AddCommand         AddCommand         { get; }
        public ParallelismCommand ParallelismCommand { get; }
        public SettingsCommand    SettingsCommand    { get; }
        public AboutCommand       AboutCommand       { get; }
        public FileNameExcludesWordsEditorCommand FileNameExcludesWordsEditorCommand { get; }
    }
}
