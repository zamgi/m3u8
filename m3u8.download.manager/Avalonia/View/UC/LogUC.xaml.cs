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
        private DataGrid DGV;
        private SettingsPropertyChangeController _SettingsController;
        private bool     _ShowOnlyRequestRowsWithErrors;
        private CheckBox _ShowOnlyRequestRowsWithErrorsMenuItemCheckBox;

        private LogListModel _Model;
        private ObservableCollection_WithIndex< LogRow > _DGVRows;
        private ThreadSafeList< LogRow > _Model_CollectionChanged_AddChangedType_Buf;
        #endregion

        #region [.ctor().]
        public LogUC()
        {
            this.InitializeComponent();

            _DGVRows = new ObservableCollection_WithIndex< LogRow >();
            _Model_CollectionChanged_AddChangedType_Buf = new ThreadSafeList< LogRow >();
            DGV = this.FindControl< DataGrid >( nameof(DGV) );
            DGV.Items = _DGVRows.List;

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
        private static IEnumerable< LogRow > GetRowsNotSuccess( IEnumerable< LogRow > rows ) => from row in rows
                                                                                                where (row.RequestRowType != RequestRowTypeEnum.Success)
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
                await Dispatcher.UIThread.InvokeAsync( _DGVRows.Clear );
            }
            else 
            {
                var rows = _ShowOnlyRequestRowsWithErrors ? GetRowsNotSuccess( _Model.GetRows() )
                                                          : _Model.GetRows(); //GetRowsNotNone( _Model.GetRows() );
                await Dispatcher.UIThread.InvokeAsync( () => _DGVRows.Replace( rows ) );
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

                _Model_CollectionChanged_AddChangedType_Buf.Clear();
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

        private async void Model_CollectionChanged_Buf__TaskRoutine()
        {
            const int COUNT_THRESHOLD = 100;
            const int TICK_THRESHOLD  = 500;

            var startTickCount = Environment.TickCount;
            var startCount     = _Model_CollectionChanged_AddChangedType_Buf.GetCount();
            for (; ; )
            {
                if ( (COUNT_THRESHOLD <= (_Model_CollectionChanged_AddChangedType_Buf.GetCount() - startCount)) || (TICK_THRESHOLD <= (Environment.TickCount - startTickCount)) )
                {
                    Dispatcher.UIThread.Post( () => Process_Model_CollectionChanged_Buf() );
                    return;
                }
                await Task.Delay( 1 );
            }
        }
        private bool Process_Model_CollectionChanged_Buf()
        {
            var suc = _Model_CollectionChanged_AddChangedType_Buf.TryGetAndClear( out var addRows );
            if ( suc )
            {
                _DGVRows.AddRange( addRows );
                ScrollToLastRowInternal();
            }
            return (suc);
        }
        private async void Model_CollectionChanged( _CollectionChangedTypeEnum_ changedType )
        {
            if ( !Dispatcher.UIThread.CheckAccess() && (changedType == _CollectionChangedTypeEnum_.Add) )
            {
                var row = _Model.GetRows().Last();
                var cnt = _Model_CollectionChanged_AddChangedType_Buf.Add( row );
                if ( cnt == 1 )
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run( Model_CollectionChanged_Buf__TaskRoutine );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                return;
            }
            else if ( !Process_Model_CollectionChanged_Buf() )
            {
                switch ( changedType )
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
