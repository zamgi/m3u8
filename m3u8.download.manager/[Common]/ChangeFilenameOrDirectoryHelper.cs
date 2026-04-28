using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using m3u8.download.manager.models;

namespace m3u8.download.manager.infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    internal static partial class ChangeFilenameOrDirectoryHelper
    {
        /// <summary>
        /// 
        /// </summary>
        private static class Set
        {
            /// <summary>
            /// 
            /// </summary>
            private static class EmptySet< T >
            {
                public static readonly ISet< T > Value = new HashSet< T >();
            }
            public static ISet< T > Empty< T >() => EmptySet< T >.Value;
        }       

        #region [.change OutputFileName & OutputDirectory.]
        public static async Task ChangeOutputFileName_Or_OutputDirectory( DownloadRow row, string outputFileName_or_outputDirectory, bool change_outputDirectory
            , ISet< string > externalProgQueue
            , Func< string, Task< bool > > askForOverwriteFunc
            , Func< string, Task > showErrorAction )
        {
            if ( outputFileName_or_outputDirectory.EqualIgnoreCase( change_outputDirectory ? row.OutputDirectory : row.OutputFileName ) )
            {
                return;
            }

            externalProgQueue ??= Set.Empty< string >();
            //----------------------------------------------------------//
            var prev_outputFullFileName = row.GetOutputFullFileName();
            var need_add = externalProgQueue.Remove( prev_outputFullFileName );

            string prev_outputFileName_or_outputDirectory;
            if ( change_outputDirectory )
            {
                prev_outputFileName_or_outputDirectory = row.OutputDirectory;
                row.SetOutputDirectory( outputFileName_or_outputDirectory );
            }
            else
            {
                prev_outputFileName_or_outputDirectory = row.OutputFileName;
                row.SetOutputFileName( outputFileName_or_outputDirectory );
            }
            var new_outputFullFileName = row.GetOutputFullFileName();

            if ( need_add ) externalProgQueue.Add( new_outputFullFileName );

            var res = await MoveFileByRename( row, prev_outputFullFileName, new_outputFullFileName, askForOverwriteFunc, showErrorAction );
            switch ( res )
            {
                //case MoveFileByRenameResultEnum.Postponed: break;
                case MoveFileByRenameResultEnum.Suc:
                    row.SaveVeryFirstOutputFullFileName( null );
                    break;
                case MoveFileByRenameResultEnum.Canceled:
                case MoveFileByRenameResultEnum.Fail:
                    //rollback
                    if ( change_outputDirectory )
                    {
                        row.SetOutputDirectory( prev_outputFileName_or_outputDirectory );
                    }
                    else
                    {
                        row.SetOutputFileName( prev_outputFileName_or_outputDirectory );
                    }
                    if ( need_add )
                    {
                        externalProgQueue.Remove( new_outputFullFileName );
                        externalProgQueue.Add( prev_outputFullFileName );
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private enum MoveFileByRenameModeEnum { OverwriteAsk, OverwriteSilent, SkipIfAlreadyExists }
        private enum MoveFileByRenameResultEnum { Postponed, Canceled, Suc, Fail }
        private static async Task< MoveFileByRenameResultEnum > MoveFileByRename( DownloadRow row, string prev_outputFullFileName, string new_outputFullFileName
            , Func< string, Task< bool > > askForOverwriteFunc
            , Func< string, Task > showErrorAction
            , MoveFileByRenameModeEnum mode = MoveFileByRenameModeEnum.OverwriteAsk )
        {
            if ( (!row.Status.IsRunningOrPaused() || FileHelper.IsSameDiskDrive( prev_outputFullFileName, new_outputFullFileName )) && File.Exists( prev_outputFullFileName ) )
            {
                switch ( mode )
                {
                    case MoveFileByRenameModeEnum.OverwriteSilent:
                        break;
                    case MoveFileByRenameModeEnum.OverwriteAsk:
                        if ( File.Exists( new_outputFullFileName ) )
                        {
                            if ( !await askForOverwriteFunc( new_outputFullFileName )
                                /*this.MessageBox_ShowQuestion( $"File '{new_outputFullFileName}' already exists. Overwrite ?", "Overwrite exists file" ) != DialogResult.Yes*/ )
                            {
                                return (MoveFileByRenameResultEnum.Canceled);
                            }
                        }
                        break;
                    case MoveFileByRenameModeEnum.SkipIfAlreadyExists:
                        if ( File.Exists( new_outputFullFileName ) )
                        {
                            return (MoveFileByRenameResultEnum.Canceled);
                        }
                        break;
                }

                if ( FileHelper.TryMoveFile_NoThrow( prev_outputFullFileName, new_outputFullFileName, out var error ) )
                {
                    return (MoveFileByRenameResultEnum.Suc);
                }
                else
                {
                    await showErrorAction( error.ToString() ); //this.MessageBox_ShowError( error.ToString(), "Move/Remane output file" );
                    return (MoveFileByRenameResultEnum.Fail);
                }
            }
            return (MoveFileByRenameResultEnum.Postponed);
        }
        #endregion
    }
}
