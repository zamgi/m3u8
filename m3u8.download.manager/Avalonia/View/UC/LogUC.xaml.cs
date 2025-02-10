using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using m3u8.download.manager.models;
using m3u8.download.manager.Properties;

using _CollectionChangedTypeEnum_ = m3u8.download.manager.models.LogListModel.CollectionChangedTypeEnum;
using _SC_ = m3u8.download.manager.controllers.SettingsPropertyChangeController;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LogUC : UserControl
    {
        #region [.field's.]
        private DataGrid DGV;
        private _SC_     _SettingsController;
        private bool     _ShowOnlyRequestRowsWithErrors;
        private CheckBox _ShowOnlyRequestRowsWithErrorsMenuItemCheckBox;
        private bool     _ScrollToLastRow;
        private CheckBox _ScrollToLastRowMenuItemCheckBox;

        private LogListModel _Model;
        private ObservableCollection_WithIndex< LogRow > _DGVRows;
        private ThreadSafeList< LogRow > _Model_CollectionChanged_AddChangedType_Buf;
        private HashSet< LogRow > _RemovedBeforeAddRows;
        private DataGrid_SelectRect_Extension< LogRow > _SelectRectExtension;
        #endregion

        #region [.ctor().]
        public LogUC()
        {
            AvaloniaXamlLoader.Load( this );

            _DGVRows = new ObservableCollection_WithIndex< LogRow >();
            _Model_CollectionChanged_AddChangedType_Buf = new ThreadSafeList< LogRow >();
            _RemovedBeforeAddRows = new HashSet< LogRow >();

            DGV = this.FindControl< DataGrid >( nameof(DGV) );
            DGV.ItemsSource = _DGVRows.List;
            if ( FontHelper.TryGetMonospace( out var fontFamily ) ) DGV.FontFamily = fontFamily;

            _ShowOnlyRequestRowsWithErrorsMenuItemCheckBox = this.FindControl< CheckBox >( nameof(_ShowOnlyRequestRowsWithErrorsMenuItemCheckBox) );
            this.FindControl< MenuItem >( "_ShowOnlyRequestRowsWithErrorsMenuItem" ).Click += _ShowOnlyRequestRowsWithErrorsMenuItem_Click;
            _ScrollToLastRowMenuItemCheckBox = this.FindControl< CheckBox >( nameof(_ScrollToLastRowMenuItemCheckBox) );
            this.FindControl< MenuItem >( "_ScrollToLastRowMenuItem" ).Click += _ScrollToLastRowMenuItem_Click;

            _SelectRectExtension = new DataGrid_SelectRect_Extension< LogRow >( this, DGV, this.FindControl< Rectangle >( "selectRect" ) );

            //this.Styles.Add( GlobalStyles.Dark );

            var st = _SC_.SettingsDefault;
            _ShowOnlyRequestRowsWithErrorsMenuItemCheckBox.IsChecked = _ShowOnlyRequestRowsWithErrors = st.ShowOnlyRequestRowsWithErrors;
            _ScrollToLastRowMenuItemCheckBox              .IsChecked = _ScrollToLastRow               = st.ScrollToLastRow; 
        }
        #endregion

        #region [.Model.]
        internal void SetSettingsController( _SC_ sc )
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

        internal void SetModel( LogListModel model )
        {
            if ( _Model == model ) return;

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

            _SelectRectExtension.SetModel( model );
        }
        private void DetachModel()
        {
            if ( _Model != null )
            {
                _Model.CollectionChanged    -= Model_CollectionChanged;
                _Model.RowPropertiesChanged -= Model_RowPropertiesChanged;
                _Model = null;

                ClearDataGridItems_UI();
            }
        }

        private async void SetDataGridItems()
        {
            if ( _Model == null )
            {
                await Dispatcher.UIThread.InvokeAsync( ClearDataGridItems_UI );
            }
            else 
            {
                static IEnumerable< LogRow > GetRowsNotSuccess( IEnumerable< LogRow > rows ) => from row in rows
                                                                                                where (row.RequestRowType != RequestRowTypeEnum.Success)
                                                                                                select row;

                var rows = _ShowOnlyRequestRowsWithErrors ? GetRowsNotSuccess( _Model.GetRows() )
                                                          : _Model.GetRows(); //GetRowsNotNone( _Model.GetRows() );
                await Dispatcher.UIThread.InvokeAsync( () => SetDataGridItems_UI( rows ) );
                ScrollToLastRow_Routine();
            }
        }
        private void ClearDataGridItems_UI()
        {
            _Model_CollectionChanged_AddChangedType_Buf.Clear();
            _RemovedBeforeAddRows.Clear();
            _DGVRows.Clear();
        }
        private void SetDataGridItems_UI( IEnumerable< LogRow > rows )
        {            
            _Model_CollectionChanged_AddChangedType_Buf.Clear();
            _RemovedBeforeAddRows.Clear();
            _DGVRows.Replace( rows );
        }

        private async void Model_CollectionChanged_AddChangedType_Buf__TaskRoutine()
        {
            const int COUNT_THRESHOLD = 100;
            const int TICK_THRESHOLD  = 500;

            var startTickCount = Environment.TickCount;
            var startCount     = _Model_CollectionChanged_AddChangedType_Buf.GetCount();
            for (; ; )
            {
                if ( (COUNT_THRESHOLD <= (_Model_CollectionChanged_AddChangedType_Buf.GetCount() - startCount)) || (TICK_THRESHOLD <= (Environment.TickCount - startTickCount)) )
                {
                    Dispatcher.UIThread.Post( () => Model_CollectionChanged_AddChangedType_Buf__UIRoutine() );
                    return;
                }
                await Task.Delay( 1 ).CAX();
            }
        }
        private bool Model_CollectionChanged_AddChangedType_Buf__UIRoutine()
        {
            var suc = _Model_CollectionChanged_AddChangedType_Buf.TryGetAndClear( out var addRows );
            if ( suc )
            {
                var addRows_exclude_removed = addRows.Where( row => !_RemovedBeforeAddRows.Remove( row ) ).ToList( addRows.Count );
                var suc_2 = _DGVRows.AddRange( addRows_exclude_removed );
                if ( suc_2 )
                {
                    ScrollToLastRow_Routine(); //ScrollToLastRow_UI(); //
                }
            }
            return (suc);
        }
        private async void Model_CollectionChanged( _CollectionChangedTypeEnum_ changedType, LogRow row )
        {
            var checkAccess = Dispatcher.UIThread.CheckAccess();
            if ( !checkAccess && (changedType == _CollectionChangedTypeEnum_.Add) )
            {
                var cnt = _Model_CollectionChanged_AddChangedType_Buf.Add( row );
                if ( cnt == 1 )
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run( Model_CollectionChanged_AddChangedType_Buf__TaskRoutine );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                return;
            }
            else
            {
                if ( !checkAccess )
                {
                    await Dispatcher.UIThread.InvokeAsync( () => Model_CollectionChanged( changedType, row ) );
                }
                else if ( !Model_CollectionChanged_AddChangedType_Buf__UIRoutine() )
                {
                    switch ( changedType )
                    {
                        case _CollectionChangedTypeEnum_.Add:
                            await Dispatcher.UIThread.InvokeAsync( () => AddRow_UI( row ) );
                            //ScrollToLastRow_Routine();
                            break;

                        //case _CollectionChangedTypeEnum_.BulkUpdate:
                        case _CollectionChangedTypeEnum_.Add_Bulk:
                        case _CollectionChangedTypeEnum_.Remove_Bulk:
                        case _CollectionChangedTypeEnum_.Remove:
                        case _CollectionChangedTypeEnum_.Clear:
                            SetDataGridItems();
                            break;
                    }
                }
            }
        }
        private void Model_RowPropertiesChanged( LogRow row, string propertyName )
        {
            if ( _ShowOnlyRequestRowsWithErrors && (row.RequestRowType == RequestRowTypeEnum.Success) && (propertyName == nameof(LogRow.RequestRowType)) )
            {
                Task.Delay( 250 ).ContinueWith( _ => Dispatcher.UIThread.Post( () => RemoveRow_UI( row ), DispatcherPriority.MaxValue ) );
            }
        }
        private async void AddRow_UI( LogRow row )
        {
            if ( !_RemovedBeforeAddRows.Remove( row ) )
            {
                _DGVRows.Add( row );

                if ( _ScrollToLastRow )
                {
                    await ScrollToLastRow_UI();
                }
            }
        }
        private void RemoveRow_UI( LogRow row )
        {
            var suc = _DGVRows.Remove( row );
            if ( !suc )
            {
                _RemovedBeforeAddRows.Add( row );
            }
        }

        private async Task ScrollToLastRow_UI()
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
        private void ScrollToLastRow_Routine()
        {
            if ( _ScrollToLastRow )
            {
                Dispatcher.UIThread.Post( async () => await ScrollToLastRow_UI() );
            }
        }

        private void SettingsController_PropertyChanged( Settings settings, string propertyName )
        {
            switch ( propertyName )
            {
                case nameof(Settings.ShowOnlyRequestRowsWithErrors):
                    _ShowOnlyRequestRowsWithErrors = settings.ShowOnlyRequestRowsWithErrors;

                    _ShowOnlyRequestRowsWithErrorsMenuItemCheckBox.IsChecked = _ShowOnlyRequestRowsWithErrors;
                    SetDataGridItems();
                    break;

                case nameof(Settings.ScrollToLastRow):
                    _ScrollToLastRow = settings.ScrollToLastRow;

                    _ScrollToLastRowMenuItemCheckBox.IsChecked = _ScrollToLastRow;
                    ScrollToLastRow_Routine();
                    break;
            }
        }
        private void _ShowOnlyRequestRowsWithErrorsMenuItem_Click( object sender, RoutedEventArgs e ) => this.ShowOnlyRequestRowsWithErrors = !this.ShowOnlyRequestRowsWithErrors;
        private void _ScrollToLastRowMenuItem_Click( object sender, RoutedEventArgs e ) => this.ScrollToLastRow = !this.ScrollToLastRow;
        #endregion

        #region [.public.]
        public bool ShowOnlyRequestRowsWithErrors
        {
            get => _ShowOnlyRequestRowsWithErrors;
            set => (_SettingsController?.Settings ?? _SC_.SettingsDefault).ShowOnlyRequestRowsWithErrors = value;
        }
        public bool ScrollToLastRow
        {
            get => _ScrollToLastRow;
            set => (_SettingsController?.Settings ?? _SC_.SettingsDefault).ScrollToLastRow = value;
        }        
        public void ClearSelection() => DGV.SelectedItem = null;
        #endregion
    }
}
