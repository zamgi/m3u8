using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using m3u8.download.manager.controllers;
using SETTINGS = m3u8.download.manager.controllers.SettingsPropertyChangeController;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FileNameExcludesWordsEditor : Window
    {
        #region [.fields.]
        private DataGrid DGV;
        private ObservableCollection< string > _DGVRows;
        #endregion

        #region [.ctor().]
        public FileNameExcludesWordsEditor()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        internal FileNameExcludesWordsEditor( IEnumerable< string > excludesWords ) : this() 
        {
            _DGVRows = new ObservableCollection< string >( excludesWords );            
            DGV.Items = _DGVRows;
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );

            DGV = this.FindControl< DataGrid >( nameof(DGV) );

            this.Find< Button >( "okButton"     ).Click += (s, e) => OkButtonProcess();
            this.Find< Button >( "cancelButton" ).Click += (s, e) => this.Close();
        }

        protected override void OnKeyDown( KeyEventArgs e )
        {
            switch ( e.Key )
            {
                case Key.Escape:
                    e.Handled = true;
                    this.Close(); 
                return;

                case Key.Enter: //Ok
                    if ( OkButtonProcess() )
                    {
                        e.Handled = true;
                        return;
                    }
                break;
            }

            base.OnKeyDown( e );
        }
        #endregion

        #region [.public methods.]
        public bool Success { get; private set; }
        
        public ICollection< string > GetFileNameExcludesWords() => _DGVRows;
        //{
        //    var data = new List< string >( DGV.RowCount - 1 );
        //    var rows = DGV.Rows;
        //    for ( var i = DGV.RowCount - 1; 0 <= i; i--  )
        //    {
        //        var row = rows[ i ];
        //        if ( !row.IsNewRow )
        //        {
        //            data.Add( row.Cells[ 0 ].Value?.ToString() );
        //        }
        //    }
        //    return (data);
        //}
        #endregion

        #region [.private methods.]
        private bool OkButtonProcess()
        {
            this.Success = true;
            this.Close();
            return (true);
        }


        #endregion
    }
}
