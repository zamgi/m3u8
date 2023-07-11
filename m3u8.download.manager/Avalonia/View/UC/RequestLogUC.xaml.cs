using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using m3u8.download.manager.models;
using _CollectionChangedTypeEnum_ = m3u8.download.manager.models.LogListModel.CollectionChangedTypeEnum;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RequestLogUC : UserControl
    {
        #region [.field's.]
        private DataGrid     DGV;
        private LogListModel _Model;
        #endregion

        #region [.ctor().]
        public RequestLogUC()
        {
            this.InitializeComponent();

            DGV = this.FindControl< DataGrid >( nameof(DGV) );

            //this.Styles.Add( GlobalStyles.Dark );
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load( this );
        #endregion

        #region [.Model.]
        private void SetDataGridItems()
        {
            if ( _Model == null )
            {
                //---DGV.Items = null;
                DGV.ItemsSource = null;
            }
            else 
            {
                //---DGV.Items = new DataGridCollectionView( _Model.GetRows() ); 
                DGV.ItemsSource = new DataGridCollectionView( _Model.GetRows() );
            }
        }

        internal void SetModel( LogListModel model )
        {
            DetachModel();

            if ( model != null )
            {
                _Model = model;
                _Model.CollectionChanged -= Model_CollectionChanged;
                _Model.CollectionChanged += Model_CollectionChanged;
            }

            SetDataGridItems();
        }
        private void DetachModel()
        {
            if ( _Model != null )
            {
                _Model.CollectionChanged -= Model_CollectionChanged;
                _Model = null;

                //---DGV.Items = null;
                DGV.ItemsSource = null;
            }
        }

        private void Model_CollectionChanged( _CollectionChangedTypeEnum_ changedType, LogRow _ )
        {
            switch ( changedType )
            {
                case _CollectionChangedTypeEnum_.Add:
                case _CollectionChangedTypeEnum_.Add_Bulk:
                //case _CollectionChangedTypeEnum_.BulkUpdate:
                case _CollectionChangedTypeEnum_.Remove:
                case _CollectionChangedTypeEnum_.Remove_Bulk:
                case _CollectionChangedTypeEnum_.Clear:
                    SetDataGridItems();
                break;
            }
        }
        #endregion

        #region [.public.]
        public void ClearSelection() => DGV.SelectedItem = null;
        public async Task ScrollToLastRow()
        {
            if ( (_Model != null) && (0 < _Model.RowsCount) )
            {
                DGV.SelectedItem = null;
                await Task.Delay( 1 );
                try
                {
                    DGV.ScrollIntoView( _Model[ _Model.RowsCount - 1 ], DGV.Columns[ 0 ] );
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }
            }
        }
        #endregion
    }
}
