using System;
using System.ComponentModel;

using m3u8.download.manager.controllers;
using m3u8.download.manager.models;
using m3u8.download.manager.ui;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class MainVM : IDisposable, INotifyPropertyChanged
    {
        public MainVM( MainWindow mainWindow )
        {
            SettingsController = new SettingsPropertyChangeController();

            DownloadListModel  = new DownloadListModel();
            DownloadListModel.CollectionChanged += DownloadListModel_CollectionChanged;
            DownloadController = new DownloadController( DownloadListModel, SettingsController );

            AddCommand                         = new AddCommand( this );
            ParallelismCommand                 = new ParallelismCommand( this );
            SettingsCommand                    = new SettingsCommand( this );
            AboutCommand                       = new AboutCommand();
            FileNameExcludesWordsEditorCommand = new FileNameExcludesWordsEditorCommand( this );
            ColumnsVisibilityEditorCommand     = new ColumnsVisibilityEditorCommand( mainWindow );
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void DownloadListModel_CollectionChanged( ListModel< DownloadRow >.CollectionChangedTypeEnum collectionChangedType ) => PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( nameof(ItemsCount) ) );

        public void Dispose()
        {
            DownloadListModel.CollectionChanged -= DownloadListModel_CollectionChanged;
            SettingsController.Dispose_NoThrow();
            DownloadController.Dispose_NoThrow();
        }

        public DownloadListModel                DownloadListModel  { get; }
        public DownloadController               DownloadController { get; }
        public SettingsPropertyChangeController SettingsController { get; }

        public AddCommand                         AddCommand                         { get; }
        public ParallelismCommand                 ParallelismCommand                 { get; }
        public SettingsCommand                    SettingsCommand                    { get; }
        public AboutCommand                       AboutCommand                       { get; }
        public FileNameExcludesWordsEditorCommand FileNameExcludesWordsEditorCommand { get; }
        public ColumnsVisibilityEditorCommand     ColumnsVisibilityEditorCommand     { get; }

        public string ItemsCount => $"{DownloadListModel.RowsCount} Items";
    }
}
