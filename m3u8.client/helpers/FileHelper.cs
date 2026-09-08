using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
#if WINDOWS
using System.Runtime.InteropServices;
#endif

namespace m3u8.helpers
{
    /// <summary>
    /// 
    /// </summary>
    internal static class FileHelper
    {
        /*| FileShare.Delete*/ /* => хуй знает зачем я это напихал: что бы мувить файл что ли (в т.ч. на другой диск?) => если он есть => можно тупо удалить перпемувить файл и хвост потеряется (а загрузка как бэ будет идти) */
        public static FileStream File_Open4Write( string fileName, FileShare fileShare = /*FileShare.Read*/FileShare.Read /*| FileShare.Delete*/ )
        {
            var fs = new FileStream( fileName, FileMode.OpenOrCreate, FileAccess.Write, fileShare ); //---var fs = File.OpenWrite( fileName );
            fs.SetLength( 0 );
            return (fs);
        }
        public static FileStream File_Open4Write_NoSetLength( string fileName, FileShare fileShare = /*FileShare.Read*/FileShare.Read /*| FileShare.Delete*/ )
            => new FileStream( fileName, FileMode.OpenOrCreate, FileAccess.Write, fileShare ); //---var fs = File.OpenWrite( fileName );
        public static FileStream File_Open4Read( string fileName, FileShare fileShare = /*FileShare.Read*//*FileShare.Write |*/ FileShare.Read /*| FileShare.Delete*/ )
            => new FileStream( fileName, FileMode.Open, FileAccess.Read, fileShare ); //---var fs = File.OpenRead( fileName );
        //-------------------------------------------------------------------------------------------------------------------------//

#if WINDOWS
        [DllImport("kernel32.dll", EntryPoint="DeleteFileW", CharSet=CharSet.Unicode, SetLastError=true)] [return: MarshalAs(UnmanagedType.Bool)]  private static extern bool DeleteFileW( string lpFileName );
        [DllImport("kernel32.dll", EntryPoint="MoveFileExW", CharSet=CharSet.Unicode, SetLastError=true)] private static extern bool MoveFileExW( string lpExistingFileName, string lpNewFileName, uint dwFlags );

        private static (bool suc, int errorCode) MoveFileEx( string fileName, string newFileName )
        {
            // Флаг заставляет ядро заменить существующий файл, если он уже есть
            const uint MOVEFILE_REPLACE_EXISTING = 0x00000001;

            var suc = MoveFileExW( fileName, newFileName, MOVEFILE_REPLACE_EXISTING );
            return (suc, suc ? 0 : Marshal.GetLastWin32Error());
        }
#endif
        private static bool EqualIgnoreCase( this string s1, string s2 ) => (string.Compare( s1, s2, true ) == 0);
        public static bool DeleteFile_NoThrow( string fileName )
        {
#if WINDOWS
            var suc = DeleteFileW( fileName );
            return (suc);
#else
            try
            {
                File.Delete( fileName );
                return (true);
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
                return (false);
            }
#endif
        }
        public static bool TryMoveFile_NoThrow( string sourceFileName, string destFileName, out Exception error )
        {
            if ( sourceFileName == destFileName )
            {
                error = default;
                return (true);
            }

            try
            {
#if NETCOREAPP
#if WINDOWS
                var t = MoveFileEx( sourceFileName, destFileName );
                if ( !t.suc )
                {
                    error = new Win32Exception( t.errorCode );
                    return (false);
                }
#else
                File.Move( sourceFileName, destFileName, overwrite: true );
#endif
#else
                if ( !sourceFileName.EqualIgnoreCase( destFileName ) )
                {
                    FileHelper.DeleteFile_NoThrow( destFileName );
                }
                File.Move( sourceFileName, destFileName );
#endif
                error = default;
                return (true);
            }
            catch ( Exception ex )
            {
                error = ex;
                return (false);
            }
        }
        public static bool TryMoveFile_NoThrow( IFileWriterHolder fwh, string sourceFileName, string destFileName, out Exception error )
        {
            var suc = (fwh != null) ? fwh.TryMoveFile( sourceFileName, destFileName, out error )
                                    : TryMoveFile_NoThrow( sourceFileName, destFileName, out error );
            return (suc);
        }
        //-------------------------------------------------------------------------------------------------------------------------//

        /*public static IFileWriterHolder CreateFileWriter( string fileName, bool setLength2Zero, out FileStream fs )
        {
            fs = new FileStream( fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete );
            if ( setLength2Zero ) fs.SetLength( 0 );
            var fsShadowLock = new FileStream( fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
            try
            {
                var fw = new FileWriterHolder( fs, fsShadowLock );
                return (fw);
            }
            catch ( Exception ex ) 
            {
                Debug.WriteLine( ex );
                fs?.Dispose();
                fsShadowLock?.Dispose();

                throw;
            }
        }
        //*/
        public static IFileWriterHolder CreateFileWriterHolder( string fileName ) => new FileWriterHolder( fileName );
    }

    /// <summary>
    /// 
    /// </summary>
    internal interface IFileWriterHolder : IDisposable
    {
        string FileName { get; }
        bool IsOpened { get; }
        FileStream Open( bool setLength2Zero );
        bool TryMoveFile( string destFileName, out Exception error );
        bool TryMoveFile( string sourceFileName, string destFileName, out Exception error );
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class FileWriterHolder : IFileWriterHolder
    {
        //private readonly FileStream _Fs;
        //private FileStream _FsShadowLock;
        //public FileWriterHolder( FileStream fs, FileStream fsShadowLock )
        //{
        //    _Fs = fs;
        //    _FsShadowLock = fsShadowLock;
        //}
        private FileStream __fs__;
        private FileStream __fsShadowLock__;
        private string __fileName__;
        private readonly object _Lock;
        public FileWriterHolder( string fileName )
        {
            if ( string.IsNullOrWhiteSpace( fileName ) ) throw (new ArgumentNullException( nameof(fileName) ));
            __fileName__ = fileName;
            _Lock = new object();
        }
        public void Dispose()
        {
            var fs = get_Fs();
            if ( fs != null )
            {
                fs.Dispose(); 
                set_Fs( null );

                var fsShadowLock = get_FsShadowLock();
                if ( fsShadowLock != null )
                {
                    fsShadowLock.Dispose(); 
                    set_FsShadowLock( null );
                }
            }
        }

        #region [.accsess to __fs__, __fsShadowLock__, __fileName__.]
        private FileStream get_Fs()
        {
            lock ( _Lock )
            {
                return (__fs__);
            }
        }
        private void set_Fs( FileStream fs )
        {
            lock ( _Lock )
            {
                __fs__ = fs;
            }
        }

        private FileStream get_FsShadowLock()
        {
            lock ( _Lock )
            {
                return (__fsShadowLock__);
            }
        }
        private void set_FsShadowLock( FileStream fs )
        {
            lock ( _Lock )
            {
                __fsShadowLock__ = fs;
            }
        }

        private string get__FileName()
        {
            lock ( _Lock )
            {
                return (__fileName__);
            }
        }
        private void set__FileName( string fileName )
        {
            lock ( _Lock )
            {
                __fileName__ = fileName;
            }
        }
        #endregion

        public string FileName => get__FileName();
        public bool IsOpened => (get_Fs() != null);

        public FileStream Open( bool setLength2Zero )
        {
            Dispose();

            var fn = get__FileName();
            var fs = new FileStream( fn, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete );
            set_Fs( fs );
            if ( setLength2Zero ) fs.SetLength( 0 );

            set_FsShadowLock( new FileStream( fn, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) );

            return (fs);
        }
        public bool TryMoveFile( string destFileName, out Exception error )
        {
            Debug.Assert( get_Fs() != null );

            get_FsShadowLock().Dispose();
            var fn = get__FileName();
            var suc = FileHelper.TryMoveFile_NoThrow( fn, destFileName, out error );
            if ( suc ) { fn = destFileName; set__FileName( fn );  }
            set_FsShadowLock( new FileStream( fn, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) );
            return (suc);
        }
        public bool TryMoveFile( string sourceFileName, string destFileName, out Exception error )
        {
            var fn = get__FileName();
            Debug.Assert( !IsOpened || string.Compare( fn, sourceFileName, true ) == 0 );

            if ( !IsOpened || (string.Compare( fn, sourceFileName, true ) != 0) )
            {
                return FileHelper.TryMoveFile_NoThrow( sourceFileName, destFileName, out error );
            }

            return TryMoveFile( destFileName, out error );
        }

        public override string ToString() => $"opened={IsOpened}" + (IsOpened ? $", fn='{FileName}'" : null);
    }
}
