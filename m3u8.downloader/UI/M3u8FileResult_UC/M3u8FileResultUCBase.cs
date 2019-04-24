using System;
using System.Collections.Generic;
using System.Windows.Forms;

using m3u8.Properties;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    internal enum DownloadLogUITypeEnum
    {
        TextBoxUIType,
        GridViewUIType,
    }

    /// <summary>
    /// 
    /// </summary>
    internal interface IRowHolder { }

    /// <summary>
    /// 
    /// </summary>
    internal abstract class M3u8FileResultUCBase : UserControl
    {
        #region [.ctor().]
        protected M3u8FileResultUCBase() => _ShowOnlyRequestRowsWithErrors = Settings.Default.ShowOnlyRequestRowsWithErrors;
        #endregion

        #region [.public props.]
        private bool _ShowOnlyRequestRowsWithErrors;
        public bool ShowOnlyRequestRowsWithErrors
        {
            get => _ShowOnlyRequestRowsWithErrors;
            set
            {
                if ( _ShowOnlyRequestRowsWithErrors != value )
                {
                    _ShowOnlyRequestRowsWithErrors = value;
                    Settings.Default.ShowOnlyRequestRowsWithErrors = value;
                    Settings.Default.SaveNoThrow();

                    ShowOnlyRequestRowsWithErrors_OnChanged();
                }
            }
        }
        protected virtual void ShowOnlyRequestRowsWithErrors_OnChanged() { }
        #endregion

        #region [.abstract methods.]
        public abstract bool IsVerticalScrollBarVisible { get; }
        public abstract void Clear();
        public abstract void SetFocus();
        public abstract void AdjustColumnsWidthSprain();
        public abstract void AdjustRowsHeight();

        public abstract void Output( m3u8_file_t m3u8File, IEnumerable< string > lines );

        public abstract void AppendEmptyLine();
        public abstract void AppendRequestAndResponseErrorText( string requestText, Exception responseError );
        public abstract void AppendRequestErrorText( Exception ex );
        public abstract void AppendRequestErrorText( string errorText );
        public abstract IRowHolder AppendRequestText( string requestText, bool ensureVisible = true );
        public abstract void SetResponseErrorText( IRowHolder holder, Exception ex );
        public abstract void SetResponseReceivedText( IRowHolder holder, string receivedText );
        #endregion
    }
}