using System.IO;

namespace System.Windows.Forms.Taskbar
{
    /// <summary>A file in the Shell Namespace</summary>
    public class ShellFile : ShellObject
    {
        internal ShellFile( string path )
        {
            // Get the absolute path
            var absPath = ShellHelper.GetAbsolutePath( path );

            // Make sure this is valid
            if ( !File.Exists( absPath ) )
            {
                throw new FileNotFoundException( $"LocalizedMessages.FilePathNotExist: '{path}'." );
            }

            ParsingName = absPath;
        }

        internal ShellFile( IShellItem2 shellItem ) => nativeShellItem = shellItem;

        /// <summary>The path for this file</summary>
        public virtual string Path => ParsingName;

        /// <summary>Constructs a new ShellFile object given a file path</summary>
        /// <param name="path">The file or folder path</param>
        /// <returns>ShellFile object created using given file path.</returns>
        public static ShellFile FromFilePath( string path ) => new ShellFile( path );
    }
}