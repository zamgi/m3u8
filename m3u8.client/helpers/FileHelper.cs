using System.IO;

namespace m3u8.helpers
{

    /// <summary>
    /// 
    /// </summary>
    internal static class FileHelper
    {
        public static FileStream File_Open4Write( string fileName, FileShare fileShare = /*FileShare.Read*/FileShare.Read | FileShare.Delete )
        {
            var fs = new FileStream( fileName, FileMode.OpenOrCreate, FileAccess.Write, fileShare ); //---var fs = File.OpenWrite( fileName );
            fs.SetLength( 0 );
            return (fs);
        }
        public static FileStream File_Open4Write_NoSetLength( string fileName, FileShare fileShare = /*FileShare.Read*/FileShare.Read | FileShare.Delete )
            => new FileStream( fileName, FileMode.OpenOrCreate, FileAccess.Write, fileShare ); //---var fs = File.OpenWrite( fileName );
        public static FileStream File_Open4Read( string fileName, FileShare fileShare = /*FileShare.Read*//*FileShare.Write |*/ FileShare.Read | FileShare.Delete )
            => new FileStream( fileName, FileMode.Open, FileAccess.Read, fileShare ); //---var fs = File.OpenRead( fileName );
    }
}
