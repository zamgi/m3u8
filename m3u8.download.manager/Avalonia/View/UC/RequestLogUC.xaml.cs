using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
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
        private DataGrid_SelectRect_Extension< LogRow > _SelectRectExtension;
        #endregion

        #region [.ctor().]
        public RequestLogUC()
        {
            AvaloniaXamlLoader.Load( this );

            DGV = this.FindControl< DataGrid >( nameof(DGV) );
            if ( FontHelper.TryGetMonospace( out var fontFamily ) ) DGV.FontFamily = fontFamily;

            _SelectRectExtension = new DataGrid_SelectRect_Extension< LogRow >( this, DGV, this.FindControl< Rectangle >( "selectRect" ) );
        }
        #endregion

        #region [.Model.]
        private void SetDataGridItems()
        {
            if ( _Model == null )
            {
                DGV.ItemsSource = null;
            }
            else 
            {
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

            _SelectRectExtension.SetModel( model );
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
