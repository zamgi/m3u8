using System.IO;

namespace System.Windows.Forms.Taskbar
{
    /// <summary>A folder in the Shell Namespace</summary>
    public class ShellFileSystemFolder : ShellFolder
    {
        internal ShellFileSystemFolder()
        {
            // Empty
        }

        internal ShellFileSystemFolder( IShellItem2 shellItem ) => nativeShellItem = shellItem;

        /// <summary>The path for this Folder</summary>
        public virtual string Path => ParsingName;

        /// <summary>Constructs a new ShellFileSystemFolder object given a folder path</summary>
        /// <param name="path">The folder path</param>
        /// <remarks>ShellFileSystemFolder created from the given folder path.</remarks>
        public static ShellFileSystemFolder FromFolderPath( string path )
        {
            // Get the absolute path
            var absPath = ShellHelper.GetAbsolutePath( path );

            // Make sure this is valid
            if ( !Directory.Exists( absPath ) )
            {
                throw new DirectoryNotFoundException( $"LocalizedMessages.FilePathNotExist: '{path}'." );
            }

            var folder = new ShellFileSystemFolder();
            try
            {
                folder.ParsingName = absPath;
                return folder;
            }
            catch
            {
                folder.Dispose();
                throw;
            }
        }
    }
}