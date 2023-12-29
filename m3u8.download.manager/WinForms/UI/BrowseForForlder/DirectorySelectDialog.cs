//#if !(NETCOREAPP)
//using Microsoft.WindowsAPICodePack.Dialogs;
//using m3u8.download.manager.Properties;
//#endif
using m3u8.download.manager.Properties;

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

        public static bool Show( IWin32Window owner, string initialDirectory, string descr, out string selectedDirectory )
            => Settings.Default.UseDirectorySelectDialogModern ? Show_AsFileSelectDialog( owner, initialDirectory, descr, out selectedDirectory )
                                                               : Show_Classic( owner, initialDirectory, descr, out selectedDirectory );
//#if NETCOREAPP
//        public static bool Show( IWin32Window owner, string initialDirectory, string descr, out string selectedDirectory ) => Show_Classic( owner, initialDirectory, descr, out selectedDirectory );
//        public static bool Show_AsFileSelectDialog( IWin32Window owner, string initialDirectory, string descr, out string selectedDirectory ) => Show_Classic( owner, initialDirectory, descr, out selectedDirectory );
//#else
//        public static bool Show( IWin32Window owner, string initialDirectory, string descr, out string selectedDirectory )
//            => Settings.Default.UseDirectorySelectDialogModern ? Show_AsFileSelectDialog( owner, initialDirectory, descr, out selectedDirectory )
//                                                               : Show_Classic( owner, initialDirectory, descr, out selectedDirectory );
//        public static bool Show_AsFileSelectDialog( IWin32Window owner, string initialDirectory, string descr, out string selectedDirectory )
//        {
//            using ( var dlg = new CommonOpenFileDialog()
//            {
//                InitialDirectory = initialDirectory,
//                IsFolderPicker   = true,
//                RestoreDirectory = true,
//            })
//            {
//                if ( !string.IsNullOrWhiteSpace( descr ) ) dlg.Title = descr;
//                var r = (owner != null) ? dlg.ShowDialog( owner.Handle ) : dlg.ShowDialog();
//                if ( r == CommonFileDialogResult.Ok )
//                {
//                    selectedDirectory = dlg.FileName;
//                    return (true);
//                }
//            }
//            selectedDirectory = null;
//            return (false);
//        }
//#endif
    }
}
