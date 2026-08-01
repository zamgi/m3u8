using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using m3u8.helpers;
using m3u8.infrastructure;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.client__v2
{
    /// <summary>
    /// 
    /// </summary>
    internal interface IReceivedAndWritedPartsStorer : IDisposable
    {
        string StoredFileName    { get; }
        string OutputFileName    { get; }
        string Url               { get; }
        int    M3u8FilePartCount { get; }
        int    LastReceivedAndWritedPartOrderNumber { get; }
        long   OutputFileStreamPosition             { get; }
        Task Store( int lastReceivedAndWritedPartOrderNumber, long outputFileStreamPosition, CancellationToken ct );
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class ReceivedAndWritedPartsStorer : IReceivedAndWritedPartsStorer, IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public sealed class _Dummy_ : IReceivedAndWritedPartsStorer
        {
            public static _Dummy_ Inst { get; } = new _Dummy_();
            private _Dummy_() { }
            public string StoredFileName => null;
            public string OutputFileName => null;
            public string Url => null;
            public int M3u8FilePartCount => -1;
            public int LastReceivedAndWritedPartOrderNumber => -1;
            public long OutputFileStreamPosition => -1L;

            public void Dispose() { }
            public Task Store( int lastReceivedAndWritedPartOrderNumber, long outputFileStreamPosition, CancellationToken ct ) => Task.CompletedTask;
        }

        private static byte[] _EnvironmentNewLine;
        static ReceivedAndWritedPartsStorer() => _EnvironmentNewLine = Encoding.UTF8.GetBytes( Environment.NewLine );

        private FileStream _Fs;        
        private byte[] _Utf8Buf;
        private long _StartPos4Write;
        public ReceivedAndWritedPartsStorer( string storedfileName, FileStream fs, string url, string outputFileName, int m3u8FilePartCount,
            (int lastReceivedAndWritedPartOrderNumber, long outputFileStreamPosition) restored = default )
        {
            StoredFileName = storedfileName;
            OutputFileName = outputFileName;
            _Fs = fs;
            Url = url;
            M3u8FilePartCount = m3u8FilePartCount;            

            _Utf8Buf = new byte[ 128 ];

            LastReceivedAndWritedPartOrderNumber = restored.lastReceivedAndWritedPartOrderNumber;
            OutputFileStreamPosition             = restored.outputFileStreamPosition;
        }
        public void Dispose() => _Fs.Dispose();

        public string StoredFileName    { get; }
        public string OutputFileName    { get; }
        public string Url               { get; }
        public int    M3u8FilePartCount { get; }
        public int    LastReceivedAndWritedPartOrderNumber { get; private set; }
        public long   OutputFileStreamPosition             { get; private set; }

        public async Task Store( int lastReceivedAndWritedPartOrderNumber, long outputFileStreamPosition, CancellationToken ct )
        {
            LastReceivedAndWritedPartOrderNumber = lastReceivedAndWritedPartOrderNumber;
            OutputFileStreamPosition             = outputFileStreamPosition;

            if ( _StartPos4Write == 0 )
            {
                _Fs.SetLength(0);

                await _Fs.WriteAsync( Url + Environment.NewLine, ct ).CAX();
                await _Fs.WriteAsync( OutputFileName + Environment.NewLine, ct ).CAX();
                await _Fs.WriteAsync( M3u8FilePartCount.ToString() + Environment.NewLine, ct ).CAX();
                await _Fs.FlushAsync( ct ).CAX();
                _StartPos4Write = _Fs.Position;
            }
            else
            {
                _Fs.Seek( _StartPos4Write, SeekOrigin.Begin );
            }            

            await _Fs.WriteAsync( Write2Buf( lastReceivedAndWritedPartOrderNumber, _Utf8Buf ), ct ).CAX();
            await _Fs.WriteAsync( _EnvironmentNewLine, ct ).CAX();

            await _Fs.WriteAsync( Write2Buf( outputFileStreamPosition, _Utf8Buf ), ct ).CAX();
            await _Fs.WriteAsync( _EnvironmentNewLine, ct ).CAX();

            await _Fs.FlushAsync( ct ).CAX();
        }

        public override string ToString() => $"{StoredFileName}, LastReceivedAndWritedPartOrderNumber = {LastReceivedAndWritedPartOrderNumber}, OutputFileStreamPosition = {OutputFileStreamPosition}";

        private unsafe static (byte[] buf, int count) Write2Buf( int value, byte[] buf )
        {
            Debug.Assert( 64 <= buf.Length );

            Span<char> char_buf = stackalloc char[ 32 ];
            var charsWritten = Value2Chars( value, char_buf/*, out var charsWritten*/ );
            //var ok = Utf16Formatter.TryFormat( value, char_buf, out var charsWritten );
            //if ( !ok ) throw (new FormatException( "Не удалось отформатировать число" ));

            ref char first = ref char_buf[ 0 ];
            char* char_ptr = (char*) Unsafe.AsPointer( ref first );

            fixed ( byte* buf_ptr = &buf[ 0 ] )
            {
                var utf8_buf_len = Encoding.UTF8.GetBytes( char_ptr, charsWritten, buf_ptr, buf.Length );
                return (buf, utf8_buf_len);
            }
        }
        private unsafe static (byte[] buf, int count) Write2Buf( long value, byte[] buf )
        {
            Debug.Assert( 128 <= buf.Length );

            Span<char> char_buf = stackalloc char[ 64 ];
            var charsWritten = Value2Chars( value, char_buf/*, out var charsWritten*/ );
            //var ok = Utf16Formatter.TryFormat( value, char_buf, out var charsWritten );
            //if ( !ok ) throw (new FormatException( "Не удалось отформатировать число" ));

            ref char first = ref char_buf[ 0 ];
            char* char_ptr = (char*) Unsafe.AsPointer( ref first );

            fixed ( byte* buf_ptr = &buf[ 0 ] )
            {
                var utf8_buf_len = Encoding.UTF8.GetBytes( char_ptr, charsWritten, buf_ptr, buf.Length );
                return (buf, utf8_buf_len);
            }
        }
        private static int Value2Chars( int value, Span<char> buffer/*, out int charsWritten*/ )
        {
            Debug.Assert( 32 <= buffer.Length );
            //Span<char> buffer = stackalloc char[ 32 ];
            int pos = 0;

            int v = value;
            if ( v == 0 )
            {
                buffer[ pos++ ] = '0';
            }
            else
            {
                var negative = (v < 0);
                if ( negative )
                {
                    buffer[ pos++ ] = '-';
                    // для int.MinValue нужно аккуратно: Math.Abs не сработает
                    if ( v == int.MinValue )
                    {
                        // спецслучай: пишем "2147483648" вручную
                        const string minValueStr = "2147483648";
                        foreach ( var c in minValueStr ) buffer[ pos++ ] = c;
                        v = 0; // чтобы цикл ниже не сработал
                    }
                    else
                    {
                        v = -v;
                    }
                }

                // Пишем цифры в обратном порядке во временный стек символов, потом разворачиваем
                // либо сразу в буфер с конца — для простоты покажу вариант с обратным заполнением
                int start = pos;
                while ( v > 0 )
                {
                    int digit = v % 10;
                    buffer[ pos++ ] = (char) ('0' + digit);
                    v /= 10;
                }
                // Разворот
                int left = start, right = pos - 1;
                while ( left < right )
                {
                    char tmp = buffer[ left ];
                    buffer[ left ] = buffer[ right ];
                    buffer[ right ] = tmp;
                    left++;
                    right--;
                }
            }

            //---charsWritten = pos;
            return (pos);
            // Дальше можно использовать buffer.Slice(0, charsWritten)
        }
        private static int Value2Chars( long value, Span<char> buffer/*, out int charsWritten*/ )
        {
            Debug.Assert( 64 <= buffer.Length );
            //Span<char> buffer = stackalloc char[ 32 ];
            int pos = 0;

            long v = value;
            if ( v == 0 )
            {
                buffer[ pos++ ] = '0';
            }
            else
            {
                var negative = (v < 0);
                if ( negative )
                {
                    buffer[ pos++ ] = '-';
                    // для long.MinValue нужно аккуратно: Math.Abs не сработает
                    if ( v == long.MinValue )
                    {
                        // спецслучай: пишем "9223372036854775808" вручную
                        const string minValueStr = "9223372036854775808";
                        foreach ( var c in minValueStr ) buffer[ pos++ ] = c;
                        v = 0; // чтобы цикл ниже не сработал
                    }
                    else
                    {
                        v = -v;
                    }
                }

                // Пишем цифры в обратном порядке во временный стек символов, потом разворачиваем
                // либо сразу в буфер с конца — для простоты покажу вариант с обратным заполнением
                int start = pos;
                while ( v > 0 )
                {
                    long digit = v % 10;
                    buffer[ pos++ ] = (char) ('0' + digit);
                    v /= 10;
                }
                // Разворот
                int left = start, right = pos - 1;
                while ( left < right )
                {
                    char tmp = buffer[ left ];
                    buffer[ left ] = buffer[ right ];
                    buffer[ right ] = tmp;
                    left++;
                    right--;
                }
            }

            //---charsWritten = pos;
            return (pos);
            // Дальше можно использовать buffer.Slice(0, charsWritten)
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal interface IReceivedAndWritedPartsProcessor : IDisposable
    {
        IReceivedAndWritedPartsStorer CreateStorer(
              in m3u8_file_t m3u8File, string outputFileName, bool outputDirectoryExists, long outputFileStreamLength /*FileStream outputFileStream*/
            , out (bool has, m3u8_file_t new_m3u8File, long outputFileStreamPosition) restored );
        bool TryRestore( Uri address, string outputFileName
            , out (int totalPartsCount, int lastReceivedAndWritedPartOrderNumber, long outputFileStreamPosition) restored );
        bool TryRestoreOutputFileNameByAddress( string address, out string outputFileName, out string outputDirectory );
        bool TryDeleteStorerFile( string address );
        bool TryDeleteAllStorerFiles();
        bool TryCalcStats( out int storeFilesCount, out long storeFilesSize );
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class ReceivedAndWritedPartsProcessor : IReceivedAndWritedPartsProcessor
    {
        /// <summary>
        /// 
        /// </summary>
        public sealed class _Dummy_ : IReceivedAndWritedPartsProcessor
        {
            public static _Dummy_ Inst { get; } = new _Dummy_();
            private _Dummy_() { }
            public void Dispose() { }
            IReceivedAndWritedPartsStorer IReceivedAndWritedPartsProcessor.CreateStorer( in m3u8_file_t m3u8File, string outputFileName, bool outputDirectoryExists, long outputFileStreamLength, out (bool has, m3u8_file_t new_m3u8File, long outputFileStreamPosition) restored )
            {
                restored = default;
                return (ReceivedAndWritedPartsStorer._Dummy_.Inst);
            }
            public bool TryRestore( Uri address, string outputFileName, out (int totalPartsCount, int lastReceivedAndWritedPartOrderNumber, long outputFileStreamPosition) restored )
            {
                restored = default;
                return (false);
            }
            public bool TryRestoreOutputFileNameByAddress( string address, out string outputFileName, out string outputDirectory )
            {
                outputFileName = outputDirectory = default;
                return (false);
            }
            public bool TryDeleteStorerFile( string address ) => false;
            public bool TryDeleteAllStorerFiles() => false;
            public bool TryCalcStats( out int storeFilesCount, out long storeFilesSize )
            {
                storeFilesCount = default; storeFilesSize = default;
                return (false);
            }
        }

        private SHA1 _Sha1;
        private string _DirectoryLocation4StoreFiles; // [_DirectoryLocation4StoreFiles] = C:\ProgramData\zamgi\m3u8.download.manager
        public ReceivedAndWritedPartsProcessor( string directoryLocation4StoreFiles )
        {
            _DirectoryLocation4StoreFiles = directoryLocation4StoreFiles;
            _Sha1 = SHA1.Create();
        }
        public void Dispose() => _Sha1.Dispose();

        public IReceivedAndWritedPartsStorer CreateStorer( 
              in m3u8_file_t m3u8File, string outputFileName, bool outputDirectoryExists, long outputFileStreamLength /*FileStream outputFileStream*/
            , out (bool has, m3u8_file_t new_m3u8File, long outputFileStreamPosition) restored )
        {
            restored = default;
            if ( !TryGetStoredFileName( m3u8File.BaseAddress, out var ffn, out var normalizedAddress ) ) return (ReceivedAndWritedPartsStorer._Dummy_.Inst);

            int  lastReceivedAndWritedPartOrderNumber;
            long outputFileStreamPosition;

            if ( outputDirectoryExists && (0 < outputFileStreamLength) && TryReadStoredFile( ffn, normalizedAddress, out var sfi )
                 && (m3u8File.Parts.Count == sfi.TotalPartsCount)
                 && (sfi.LastReceivedAndWritedPartOrderNumber < m3u8File.Parts.Count)
                 && (sfi.OutputFileStreamPosition <= outputFileStreamLength)
               )
            {
                lastReceivedAndWritedPartOrderNumber = sfi.LastReceivedAndWritedPartOrderNumber;
                outputFileStreamPosition             = sfi.OutputFileStreamPosition;

                var new_parts = new List< m3u8_part_ts >( m3u8File.Parts.Count - (lastReceivedAndWritedPartOrderNumber + 1) );
                    new_parts.AddRange( m3u8File.Parts.SkipWhile( p => p.OrderNumber <= lastReceivedAndWritedPartOrderNumber ) );
                var new_m3u8File = m3u8_file_t.From( m3u8File, new_parts.AsReadOnly() );

                restored = (has: true, new_m3u8File, outputFileStreamPosition);
            }
            #region comm.
            //if ( outputDirectoryExists && File.Exists( ffn ) )
            //{
            //    using ( var sr = File.OpenText( ffn ) )
            //    {
            //        var addressTextStored = sr.ReadLine();
            //        if ( addressTextStored == normalizedAddress )
            //        {
            //            var totalPartsCountText = sr.ReadLine();
            //            if ( int.TryParse( totalPartsCountText, out var totalPartsCount ) && (m3u8File.Parts.Count == totalPartsCount) )
            //            {
            //                var lastReceivedAndWritedPartOrderNumberText = sr.ReadLine();
            //                if ( int.TryParse( lastReceivedAndWritedPartOrderNumberText, out lastReceivedAndWritedPartOrderNumber ) && 
            //                     (lastReceivedAndWritedPartOrderNumber < m3u8File.Parts.Count) 
            //                   )
            //                {
            //                    var outputFileStreamPositionText = sr.ReadLine();
            //                    if ( long.TryParse( outputFileStreamPositionText, out outputFileStreamPosition ) && 
            //                        (outputFileStreamPosition <= outputFileStreamLength) 
            //                       )
            //                    {
            //                        var new_parts = new List< m3u8_part_ts__v2 >( m3u8File.Parts.Count - (lastReceivedAndWritedPartOrderNumber + 1) );
            //                            new_parts.AddRange( m3u8File.Parts.SkipWhile( p => p.OrderNumber <= lastReceivedAndWritedPartOrderNumber ) );
            //                        var new_m3u8File = m3u8_file_t__v2.From( m3u8File, new_parts.AsReadOnly() );

            //                        //(new_m3u8File, outputFileStreamPosition)
            //                        restored = (has: true, new_m3u8File, outputFileStreamPosition);
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            #endregion
            else
            {
                lastReceivedAndWritedPartOrderNumber = 0;
                outputFileStreamPosition             = 0;

                if ( !Directory.Exists( _DirectoryLocation4StoreFiles ) )
                {
                    Directory.CreateDirectory( _DirectoryLocation4StoreFiles );
                }
            }

            var fs = FileHelper.File_Open4Write_NoSetLength( ffn );
            var x = new ReceivedAndWritedPartsStorer( ffn, fs, normalizedAddress, outputFileName, m3u8File.Parts.Count, (lastReceivedAndWritedPartOrderNumber, outputFileStreamPosition) );
            return (x);
        }
        public bool TryRestore( Uri address, string outputFileName
            , out (int totalPartsCount, int lastReceivedAndWritedPartOrderNumber, long outputFileStreamPosition) restored )
        {
            if ( TryGetStoredFileName( address, out var ffn, out var normalizedAddress ) &&
                 File.Exists( ffn ) && File.Exists( outputFileName ) 
               )
            {
                using ( var fs = FileHelper.File_Open4Read( outputFileName ) )
                {
                    if ( TryReadStoredFile( ffn, normalizedAddress, out var sfi, checkFileOnExists: false )
                         //&& (sfi.LastReceivedAndWritedPartOrderNumber < sfi.TotalPartsCount)
                         && (sfi.OutputFileStreamPosition == fs.Length) )
                    {
                        restored = (sfi.TotalPartsCount, sfi.LastReceivedAndWritedPartOrderNumber, sfi.OutputFileStreamPosition);
                        return (true);
                    }
                }
            }

            restored = default;
            return (false);
        }
        public bool TryRestoreOutputFileNameByAddress( string address, out string outputFileName, out string outputDirectory )
        {
            if ( TryGetStoredFileName( address, out var ffn, out var normalizedAddress ) && 
                 TryReadStoredFile( ffn, normalizedAddress, out var sfi )
               )
            {
                outputFileName  = Path.GetFileName( sfi.OutputFileName );
                outputDirectory = Path.GetDirectoryName( sfi.OutputFileName );
                return (true);
            }

            outputFileName = outputDirectory = default;
            return (false);
        }

        /// <summary>
        /// 
        /// </summary>
        private readonly struct StoredFileInfo
        {
            required public string Url { get; init; }
            required public string OutputFileName { get; init; }
            required public int    TotalPartsCount { get; init; }
            required public int    LastReceivedAndWritedPartOrderNumber { get; init; }
            required public long   OutputFileStreamPosition { get; init; }
            //required public long   OutputFileSize { get; init; }
        }
        private static bool TryReadStoredFile( string storedFileName, string normalizedAddress, out StoredFileInfo sfi, bool checkFileOnExists = true )
        {
            sfi = default;
            if ( !checkFileOnExists || File.Exists( storedFileName ) ) 
            using ( var sr = File.OpenText( storedFileName ) )
            {
                var addressTextStored = sr.ReadLine();
                if ( addressTextStored != normalizedAddress ) return (false);

                var outputFileName = sr.ReadLine();
                if ( outputFileName.IsNullOrWhiteSpace() ) return (false);

                var totalPartsCountText = sr.ReadLine();
                if ( !int.TryParse( totalPartsCountText, out var totalPartsCount ) ) return (false);

                var lastReceivedAndWritedPartOrderNumberText = sr.ReadLine();
                if ( !int.TryParse( lastReceivedAndWritedPartOrderNumberText, out var lastReceivedAndWritedPartOrderNumber ) ||
                     !(lastReceivedAndWritedPartOrderNumber < totalPartsCount) ) return (false);

                var outputFileStreamPositionText = sr.ReadLine();
                if ( !long.TryParse( outputFileStreamPositionText, out var outputFileStreamPosition ) ) return (false);

                sfi = new StoredFileInfo() { Url = addressTextStored, OutputFileName = outputFileName, TotalPartsCount = totalPartsCount,
                    LastReceivedAndWritedPartOrderNumber = lastReceivedAndWritedPartOrderNumber, OutputFileStreamPosition = outputFileStreamPosition/*, OutputFileSize = fs.Length*/ };
                return (true);
            }
            return (false);
        }

        public bool TryDeleteStorerFile( string address )
        {
            if ( TryGetStoredFileName( address, out var ffn, out _ ) && File.Exists( ffn ) )
            {
                try
                {
                    File.Delete( ffn );
                    return (true);
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }
            }
            return (false);
        }
        public bool TryDeleteAllStorerFiles()
        {
            var suc = !_DirectoryLocation4StoreFiles.IsNullOrEmpty() && Directory.Exists( _DirectoryLocation4StoreFiles );
            if ( suc )
            {
                foreach ( var fn in Directory.EnumerateFiles( _DirectoryLocation4StoreFiles, '*' + DEF_STORE_FILE_EXTENSION, SearchOption.TopDirectoryOnly ) )
                {
                    try
                    {
                        File.Delete( fn );                        
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( ex );
                    }
                }
            }
            return (suc);
        }
        public bool TryCalcStats( out int storeFilesCount, out long storeFilesSize )
        {
            storeFilesCount = 0;
            storeFilesSize  = 0;

            var suc = !_DirectoryLocation4StoreFiles.IsNullOrEmpty() && Directory.Exists( _DirectoryLocation4StoreFiles );
            if ( suc )
            {
                foreach ( var fn in Directory.EnumerateFiles( _DirectoryLocation4StoreFiles, '*' + DEF_STORE_FILE_EXTENSION, SearchOption.TopDirectoryOnly ) )
                {
                    storeFilesCount++;
                    try
                    {
                        var size = new FileInfo( fn ).Length;
                        storeFilesSize += size;
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( ex );
                    }
                }
            }
            return (suc);
        }


        private const string DEF_STORE_FILE_EXTENSION = ".txt";
        [M(O.AggressiveInlining)] private bool TryGetStoredFileName( Uri address, out string storedFileName, out string normalizedAddress ) 
            => TryGetStoredFileName( address?.AbsoluteUri/*ToString()*/, out storedFileName, out normalizedAddress );
        private bool TryGetStoredFileName( string address, out string storedFileName, out string normalizedAddress )
        {
            if ( _DirectoryLocation4StoreFiles.IsNullOrEmpty() || address.IsNullOrEmpty() )
            {
                storedFileName = default;
                normalizedAddress = default;
                return (false);
            }

            Debug.Assert( address == new Uri( address ).AbsoluteUri );

            normalizedAddress = address.ToUpperInvariant();
            var addressBytes = Encoding.UTF8.GetBytes( normalizedAddress );
            //
            byte[] hash;
            lock ( _Sha1 )
            {
                hash = _Sha1.ComputeHash( addressBytes );
            }            
            var fn = ByteArrayToHex( hash );
            storedFileName = Path.Combine( _DirectoryLocation4StoreFiles, fn + DEF_STORE_FILE_EXTENSION );
            return (true);
        }
        private unsafe static string ByteArrayToHex( byte[] bytes ) // => string.Concat( hash.Select( b => b.ToString( "X2" ) ) );
        {
            //if ( bytes == null || bytes.Length == 0 ) return (string.Empty);

            const string HEX_DIGITS = "0123456789ABCDEF";

            Span<char> char_buf = stackalloc char[ bytes.Length * 2 ];
            for ( var i = 0; i < bytes.Length; i++ )
            {
                var b = bytes[ i ];
                char_buf[ i * 2     ] = HEX_DIGITS[ b >> 4   ];
                char_buf[ i * 2 + 1 ] = HEX_DIGITS[ b & 0x0F ];
            }
#if NETCOREAPP
            var s = new string( char_buf );
#else
            ref char r = ref char_buf[ 0 ];
            char* ptr = (char*) Unsafe.AsPointer( ref r );
            var s = new string( ptr, 0, char_buf.Length );
#endif
            return (s);

            /*
            var buf = new StringBuilder( bytes.Length * 2 );
            foreach ( var b in bytes )
            {
                buf.Append( b.ToString( "X2" ) );
            }
            return (buf.ToString());
            //*/
        }
    }
}
