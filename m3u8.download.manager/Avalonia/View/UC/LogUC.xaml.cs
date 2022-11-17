using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using m3u8.download.manager.controllers;
using m3u8.download.manager.models;
using m3u8.download.manager.Properties;
using _CollectionChangedTypeEnum_ = m3u8.download.manager.models.LogListModel.CollectionChangedTypeEnum;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LogUC : UserControl
    {
        #region [.field's.]
        private DataGrid     DGV;
        private ObservableCollection< LogRow > _DGVRows;
        private LogListModel _Model;
        private SettingsPropertyChangeController _SettingsController;
        private bool     _ShowOnlyRequestRowsWithErrors;
        private CheckBox _ShowOnlyRequestRowsWithErrorsMenuItemCheckBox;
        #endregion

        #region [.ctor().]
        public LogUC()
        {
            this.InitializeComponent();

            _DGVRows = new ObservableCollection< LogRow >();
            DGV = this.FindControl< DataGrid >( nameof(DGV) );
            DGV.Items = _DGVRows;

            _ShowOnlyRequestRowsWithErrorsMenuItemCheckBox = this.FindControl< CheckBox >( nameof(_ShowOnlyRequestRowsWithErrorsMenuItemCheckBox) );
            this.FindControl< MenuItem >( "_ShowOnlyRequestRowsWithErrorsMenuItem" ).Click += _ShowOnlyRequestRowsWithErrorsMenuItem_Click;

            //this.Styles.Add( GlobalStyles.Dark );

            _ShowOnlyRequestRowsWithErrors = SettingsPropertyChangeController.SettingsDefault.ShowOnlyRequestRowsWithErrors;
            _ShowOnlyRequestRowsWithErrorsMenuItemCheckBox.IsChecked = _ShowOnlyRequestRowsWithErrors;
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load( this );
        #endregion

        #region [.Model.]
        private static IEnumerable< LogRow > GetRowsDenyNone( IEnumerable< LogRow > rows ) => from row in rows
                                                                                              where (row.RequestRowType != RequestRowTypeEnum.None)
                                                                                              select row;
        private static IEnumerable< LogRow > GetRowsErrorOrFinish( IEnumerable< LogRow > rows ) => from row in rows
                                                                                                   where (row.RequestRowType == RequestRowTypeEnum.Error) ||
                                                                                                         (row.RequestRowType == RequestRowTypeEnum.Finish)
                                                                                                   select row;
        private async Task ScrollToLastRow()
        {
            if ( (_Model != null) && (0 < _Model.RowsCount) )
            {
                DGV.SelectedItem = null;
                await Task.Delay( 1 );
                try
                {
                    if ( (_Model != null) && (0 < _Model.RowsCount) )
                    {
                        DGV.ScrollIntoView( _Model[ _Model.RowsCount - 1 ], DGV.Columns[ 0 ] );
                    }
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }
            }
        }
        private void ScrollToLastRowInternal() => Dispatcher.UIThread.Post( async () => await ScrollToLastRow() );

        private async void SetDataGridItems()
        {
            if ( _Model == null )
            {
                await Dispatcher.UIThread.InvokeAsync( () => _DGVRows.Clear() );
            }
            else 
            {
                var rows = _ShowOnlyRequestRowsWithErrors ? GetRowsErrorOrFinish( _Model.GetRows() )
                                                          : _Model.GetRows(); //GetRowsNotNone( _Model.GetRows() );
                var new_DGVRows = new ObservableCollection< LogRow >( rows );
                await Dispatcher.UIThread.InvokeAsync( () => DGV.Items = _DGVRows = new_DGVRows );
                ScrollToLastRowInternal();
            }
        }

        internal void SetModel( LogListModel model )
        {
            DetachModel();

            if ( model != null )
            {
                _Model = model;
                _Model.CollectionChanged    -= Model_CollectionChanged;
                _Model.RowPropertiesChanged -= Model_RowPropertiesChanged;
                _Model.CollectionChanged    += Model_CollectionChanged;
                _Model.RowPropertiesChanged += Model_RowPropertiesChanged;
            }

            SetDataGridItems();
        }
        private void DetachModel()
        {
            if ( _Model != null )
            {
                _Model.CollectionChanged    -= Model_CollectionChanged;
                _Model.RowPropertiesChanged -= Model_RowPropertiesChanged;
                _Model = null;

                _DGVRows.Clear();
                //DGV.Items = null;
            }
        }

        internal void SetSettingsController( SettingsPropertyChangeController sc )
        {
            DetachSettingsController();

            _SettingsController = sc ?? throw (new ArgumentNullException( nameof(sc) ));
            _SettingsController.SettingsPropertyChanged -= SettingsController_PropertyChanged;
            _SettingsController.SettingsPropertyChanged += SettingsController_PropertyChanged;

        }
        private void DetachSettingsController()
        {
            if ( _SettingsController != null )
            {
                _SettingsController.SettingsPropertyChanged -= SettingsController_PropertyChanged;
                _SettingsController = null;
            }
        }

        private async void Model_CollectionChanged( _CollectionChangedTypeEnum_ collectionChangedType )
        {
            switch ( collectionChangedType )
            {
                case _CollectionChangedTypeEnum_.Add:
                    var row = _Model.GetRows().Last();
                    await Dispatcher.UIThread.InvokeAsync( () => _DGVRows.Add( row ) );
                    ScrollToLastRowInternal();
                break;

                case _CollectionChangedTypeEnum_.BulkUpdate:
                case _CollectionChangedTypeEnum_.Clear:
                    SetDataGridItems();
                break;
            }
        }
        private void Model_RowPropertiesChanged( LogRow row, string propertyName )
        {
            if ( _ShowOnlyRequestRowsWithErrors && (row.RequestRowType == RequestRowTypeEnum.Success) && (propertyName == nameof(LogRow.RequestRowType)) )
            {
                Task.Delay( 250 ).ContinueWith( _ => Dispatcher.UIThread.Post( () => _DGVRows.Remove( row ) ) );
            }
        }
        private void SettingsController_PropertyChanged( Settings settings, string propertyName )
        {
            if ( propertyName == nameof(Settings.ShowOnlyRequestRowsWithErrors) )
            {
                _ShowOnlyRequestRowsWithErrors = settings.ShowOnlyRequestRowsWithErrors;

                _ShowOnlyRequestRowsWithErrorsMenuItemCheckBox.IsChecked = _ShowOnlyRequestRowsWithErrors;
                SetDataGridItems();
            }
        }
        private void _ShowOnlyRequestRowsWithErrorsMenuItem_Click( object sender, RoutedEventArgs e ) => this.ShowOnlyRequestRowsWithErrors = !this.ShowOnlyRequestRowsWithErrors;
        #endregion

        #region [.public.]
        public bool ShowOnlyRequestRowsWithErrors
        {
            get => _ShowOnlyRequestRowsWithErrors;
            set => (_SettingsController?.Settings ?? SettingsPropertyChangeController.SettingsDefault).ShowOnlyRequestRowsWithErrors = value;
        }
        public void ClearSelection() => DGV.SelectedItem = null;
        #endregion
    }
}
