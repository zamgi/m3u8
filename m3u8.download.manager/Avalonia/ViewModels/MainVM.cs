using System;
using System.ComponentModel;

using m3u8.download.manager.controllers;
using m3u8.download.manager.models;
using m3u8.download.manager.Properties;
using m3u8.download.manager.ui;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class MainVM : IDisposable, INotifyPropertyChanged
    {
        public MainVM( MainWindow mainWindow, Settings settings, m3u8_client_next_factory_enum_type m3u8_client_next_factory_type )
        {
            SettingsController = new SettingsPropertyChangeController( settings );

            ReceivedAndWritedPartsProcessor = new ReceivedAndWritedPartsProcessor( AssemblyInfoHelper.AppDataFolder );

            DownloadListModel  = new DownloadListModel();
            DownloadListModel.CollectionChanged += DownloadListModel_CollectionChanged;
            DownloadController = new DownloadController( DownloadListModel, SettingsController, m3u8_client_next_factory_type, ReceivedAndWritedPartsProcessor );

            UndoModel          = new UndoModel( DownloadListModel );
            OutputFileNamePatternProcessor = new OutputFileNamePatternProcessor();

            AddCommand                              = new AddCommand( this, OutputFileNamePatternProcessor );
            EditCommand                             = new EditCommand( this, mainWindow, OutputFileNamePatternProcessor );
            ChangeSettingsParams4DownloadRowCommand = new ChangeSettingsParams4DownloadRowCommand( this, mainWindow, OutputFileNamePatternProcessor );
            ParallelismCommand                      = new ParallelismCommand( this );
            SettingsCommand                         = new SettingsCommand( this );
            WebProxyCommand                         = new WebProxyCommand( this );
            AboutCommand                            = new AboutCommand();
            FileNameExcludesWordsEditorCommand      = new FileNameExcludesWordsEditorCommand( this );
            ColumnsVisibilityEditorCommand          = new ColumnsVisibilityEditorCommand( mainWindow );
            CollectGarbageCommand                   = new CollectGarbageCommand();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void DownloadListModel_CollectionChanged( ListModel< DownloadRow >.CollectionChangedTypeEnum changedType, DownloadRow _ ) => PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( nameof(ItemsCount) ) );

        public void Dispose()
        {
            DownloadListModel.CollectionChanged -= DownloadListModel_CollectionChanged;
            SettingsController.Dispose_NoThrow();
            DownloadController.Dispose_NoThrow();
        }

        public IReceivedAndWritedPartsProcessor ReceivedAndWritedPartsProcessor { get; }
        public DownloadListModel                DownloadListModel               { get; }
        public DownloadController               DownloadController              { get; }
        public SettingsPropertyChangeController SettingsController              { get; }
        public UndoModel                        UndoModel                       { get; }
        public OutputFileNamePatternProcessor   OutputFileNamePatternProcessor  { get; }

        public AddCommand                              AddCommand                              { get; }
        public EditCommand                             EditCommand                             { get; }
        public ChangeSettingsParams4DownloadRowCommand ChangeSettingsParams4DownloadRowCommand { get; }
        public ParallelismCommand                      ParallelismCommand                      { get; }
        public SettingsCommand                         SettingsCommand                         { get; }
        public WebProxyCommand                         WebProxyCommand                         { get; }
        public AboutCommand                            AboutCommand                            { get; }
        public FileNameExcludesWordsEditorCommand      FileNameExcludesWordsEditorCommand      { get; }
        public ColumnsVisibilityEditorCommand          ColumnsVisibilityEditorCommand          { get; }
        public CollectGarbageCommand                   CollectGarbageCommand                   { get; }

        public string ItemsCount => $"{DownloadListModel.RowsCount} Items";
    }
}
