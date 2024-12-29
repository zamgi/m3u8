namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal static class DirectorySelectDialog
    {
        public static bool Show_Classic( IWin32Window owner, string initialDirectory, string descr, out string selectedDirectory ) 
            => SHBrowser.TrySelectPath( owner, initialDirectory, descr, out selectedDirectory );
        public static bool Show_AsFileSelectDialog( IWin32Window owner, string initialDirectory, string descr, out string selectedDirectory )
            => FileStyleFolderDialog.TryShowFileStyleFolderDialog( owner, initialDirectory, descr, out selectedDirectory );

        public static bool Show( IWin32Window owner, bool useDirectorySelectDialogModern, string initialDirectory, string descr, out string selectedDirectory )
            => useDirectorySelectDialogModern ? Show_AsFileSelectDialog( owner, initialDirectory, descr, out selectedDirectory )
                                              : Show_Classic( owner, initialDirectory, descr, out selectedDirectory );

        //public static bool Show( IWin32Window owner, string initialDirectory, string descr, out string selectedDirectory )
        //    => Settings.Default.UseDirectorySelectDialogModern ? Show_AsFileSelectDialog( owner, initialDirectory, descr, out selectedDirectory )
        //                                                       : Show_Classic( owner, initialDirectory, descr, out selectedDirectory );
    }
}
