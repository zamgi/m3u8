﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;

using ReactiveUI;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RequestHeadersEditor : UserControl
    {
        /// <summary>
        /// 
        /// </summary>
        public delegate void RequestHeadersCountChangedEventHandler( int requestHeadersCount, int enabledCount );

        /// <summary>
        /// 
        /// </summary>
        private sealed class RequestHeader : ReactiveObject
        {
            public RequestHeader() => IsChecked = true;
            public RequestHeader( string name, string description, int? max_name_length = null )
            {
                Name        = name;
                DisplayName = name;
                Description = description;
                if ( description != null )
                {
                    var indent = new string( ' ', 2 + (max_name_length.GetValueOrDefault( name.Length ) - name.Length) );
                    DisplayName += indent + description;
                }
            }
            public RequestHeader( string name, string value, bool isChecked )
            {
                Name        = name;                
                DisplayName = name;
                Value       = value;
                IsChecked   = isChecked;
            }

            private int _ViewIndex;
            public int ViewOrderNumber
            {
                get => _ViewIndex;
                set => this.RaiseAndSetIfChanged( ref _ViewIndex, value );
            }

            private bool _IsChecked;
            public bool IsChecked
            {
                get => _IsChecked;
                set => this.RaiseAndSetIfChanged( ref _IsChecked, value );
            }

            private string _Value;
            public string Value
            {
                get => _Value;
                set => this.RaiseAndSetIfChanged( ref _Value, value );
            }

            private string _Name;
            public string Name
            {
                get => _Name;
                set => this.RaiseAndSetIfChanged( ref _Name, value );
            }

            public string Description { get; }
            public string DisplayName { get; }
#if DEBUG
            public override string ToString() => DisplayName;
#endif
        }

        #region [.fields.]
        private DataGrid DGV;
        private DataGridCollectionView _DGVRows;

        private ContextMenu mainContextMenu;
        private MenuItem    addRowMenuItem;
        private MenuItem    deleteRowMenuItem;
        #endregion

        #region [.ctor().]
        public RequestHeadersEditor() => this.InitializeComponent();

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );

            mainContextMenu   = this.Find_Ex< ContextMenu >( nameof(mainContextMenu) );
            addRowMenuItem    = mainContextMenu.Find_MenuItem( nameof(addRowMenuItem)    ); addRowMenuItem   .Click += addRowMenuItem_Click;
            deleteRowMenuItem = mainContextMenu.Find_MenuItem( nameof(deleteRowMenuItem) ); deleteRowMenuItem.Click += deleteRowMenuItem_Click;

            DGV = this.FindControl< DataGrid >( nameof(DGV) );
            DGV.LoadingRow         += DGV_LoadingRow;            
            DGV.RowEditEnded       += DGV_RowEditEnded;
            DGV.PointerPressed     += DGV_PointerPressed;
            DGV.CellPointerPressed += DGV_CellPointerPressed;

            if ( DGV.Columns.OfType< DataGridCheckBoxColumn >().FirstOrDefault()?.Header is TextBlock textBlock )
            {
                textBlock.PointerPressed += DGV_CheckBoxColumn_TextBlock_PointerPressed;
            }
            DGV.ItemsSource = _DGVRows = new DataGridCollectionView( new List< RequestHeader >() );
        }

        protected override void OnLoaded( RoutedEventArgs e )
        {
            base.OnLoaded( e );

            CorrectCheckBoxesStyle();
            DGV.Focus();
        }
        protected override void OnKeyDown( KeyEventArgs e )
        {
            switch ( e.Key )
            {
                case Key.Space:
                    if ( DGV.IsFocused )
                    {
                        TryCheckByKey();
                    }
                break;

                case Key.Insert:
                    if ( DGV.IsFocused )
                    {
                        addRowMenuItem_Click( null, EventArgs.Empty );
                    }
                break;

                case Key.Delete:
                    if ( DGV.IsFocused )
                    {
                        deleteRowMenuItem_Click( null, EventArgs.Empty );
                    }
                break;
            }

            base.OnKeyDown( e );
        }
        #endregion

        #region [.public methods.]
        public event RequestHeadersCountChangedEventHandler OnRequestHeadersCountChanged;
        private void Fire_OnRequestHeadersCountChanged() => OnRequestHeadersCountChanged?.Invoke( _DGVRows.Count, GetEnabledCount() );

        public void SetRequestHeaders( IDictionary< string, string > requestHeaders )
        {
            //var max_name_length = GetRegularRequestHeader().Max( t => t.name.Length );
            //if ( requestHeaders.AnyEx() )
            //{
            //    max_name_length = Math.Max( max_name_length, requestHeaders.Keys.Max( k => k.Length ) );
            //}

            //_AllRequestHeaders4DropDown = new List< RequestHeader >( 100 );            
            //var hs_allRequestHeaders4DropDown = new HashSet< string >( 100 );
            //foreach ( var (name, descr)  in GetRegularRequestHeader() )
            //{
            //    if ( hs_allRequestHeaders4DropDown.Add( name ) )
            //    {
            //        _AllRequestHeaders4DropDown.Add( new RequestHeader( name/*, descr, max_name_length*/ ) );
            //    }
            //}
            //DGV_keyColumn.DropDownWidth = 250; // 750;

            //if ( requestHeaders.AnyEx() )
            //{
            //    foreach ( var key in requestHeaders.Keys )
            //    {
            //        if ( hs_allRequestHeaders4DropDown.Add( key ) )
            //        {
            //            _AllRequestHeaders4DropDown.Add( new RequestHeader( key ) );
            //        }
            //    }
            //}
            //_AllRequestHeaders4DropDown.Sort( (x, y) => string.Compare( x.DisplayName, y.DisplayName, true ) );

            //keyBindingSource.DataSource = _AllRequestHeaders4DropDown;

            //-------------------------------------------------//

            if ( requestHeaders.AnyEx() )
            {
                var items = new List< RequestHeader >( requestHeaders.Count );
                foreach ( var p in requestHeaders.OrderBy( p => p.Key ) )
                {
                    items.Add( new RequestHeader( p.Key, p.Value, isChecked: true ) );
                }
                DGV.ItemsSource = _DGVRows = new DataGridCollectionView( items );
            }
            Fire_OnRequestHeadersCountChanged();
        }
        public IDictionary< string, string > GetRequestHeaders()
        {
            var dict = new SortedDictionary< string, string >( StringComparer.InvariantCultureIgnoreCase );
            foreach ( var rh in _DGVRows.SourceCollection.Cast< RequestHeader >() )
            {
                if ( rh.IsChecked && !rh.Name.IsNullOrWhiteSpace() )
                {
                    dict[ rh.Name.Trim() ] = rh.Value?.Trim();
                }
            }
            return (dict);
        }

        private int GetEnabledCount()
        {
            var enabledCount = 0;
            foreach ( var rh in _DGVRows.SourceCollection.Cast< RequestHeader >() )
            {
                if ( rh.IsChecked && !rh.Name.IsNullOrWhiteSpace() )
                {
                    enabledCount++;
                }
            }
            return (enabledCount);
        }
        #endregion

        #region [.private methods & DGV.]
        private void TryCheckByKey()
        {
            if ( TryGetCheckBox( DGV.SelectedItem, out var checkBox ) )
            {
                checkBox.IsChecked = !checkBox.IsChecked.GetValueOrDefault();
                Fire_OnRequestHeadersCountChanged();
            }
        }
        private bool TryGetCheckBox( object selItem, out CheckBox checkBox )
        {
            checkBox = DGV.Columns.Select( col => (col.GetCellContent( selItem ) is CheckBox checkBox) ? checkBox : null )
                                  .FirstOrDefault( checkBox => checkBox != null );
            return (checkBox != null);
        }
        private void CorrectCheckBoxesStyle()
        {
            var brush = Brushes.Black;
            foreach ( var rh in _DGVRows.SourceCollection.Cast< RequestHeader >() )
            {
                if ( TryGetCheckBox( rh, out var checkBox ) )
                {
                    //var brush = rh.IsVisibleAlways ? Brushes.DimGray : Brushes.Black;
                    foreach ( var vis in checkBox.GetVisualDescendants() )
                    {
                        switch ( vis )
                        {
                            case Path   p: p.Fill        = brush; break;
                            case Border b: b.BorderBrush = brush; break;
                        }                        
                    }

                }
            }
        }

        private void DGV_PointerPressed( object sender, PointerPressedEventArgs e )
        {
            var p = e.GetCurrentPoint( this/*null*/ );
            if ( p.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed )
            {
                e.Pointer.Capture( null );
                e.Handled = true;

                open_mainContextMenu();
            }
        }
        private void DGV_CellPointerPressed( object sender, DataGridCellPointerPressedEventArgs e )
        {
            var p = e.PointerPressedEventArgs.GetCurrentPoint( null );
            switch ( p.Properties.PointerUpdateKind )
            {
                case PointerUpdateKind.RightButtonPressed: open_mainContextMenu(); return;
                case PointerUpdateKind.LeftButtonPressed: break;
                default: return;
            }
            if ( !(e.Column.GetCellContent( e.Row ) is CheckBox checkBox) ) return;

            var bounds = checkBox.GetTransformedBounds();
            if ( bounds.HasValue && bounds.Value.Contains( p.Position ) )
            {
                var isChecked = !checkBox.IsChecked.GetValueOrDefault();

                var selItems = DGV.SelectedItems.Cast< RequestHeader >();
                foreach ( var rh in selItems )
                {
                    rh.IsChecked = isChecked;
                }
            }
        }
        private void DGV_LoadingRow( object sender, DataGridRowEventArgs e )
        {
            var index = e.Row.GetIndex();

            var w = (RequestHeader) _DGVRows[ index ];
            w.ViewOrderNumber = index + 1;
        }
        private void DGV_RowEditEnded( object sender, DataGridRowEditEndedEventArgs e )
        {
            if ( e.EditAction == DataGridEditAction.Commit )
            {
                Fire_OnRequestHeadersCountChanged();
            }
        }

        private void DGV_CheckBoxColumn_TextBlock_PointerPressed( object sender, PointerPressedEventArgs e )
        {
            var rhs = _DGVRows.SourceCollection.Cast< RequestHeader >();

            var row_0 = rhs.FirstOrDefault(); if ( row_0 == null ) return;
            var isChecked = !row_0.IsChecked;

            //DGV.CellValueChanged -= DGV_CellValueChanged;
            //try
            //{
                foreach ( var rh in rhs )
                {
                    rh.IsChecked = isChecked;
                }
            //}
            //finally
            //{
            //    DGV.CellValueChanged += DGV_CellValueChanged;
            //}

            //if ( DGV.IsCurrentCellInEditMode )
            //{
            //    DGV.EndEdit();
            //}

            Fire_OnRequestHeadersCountChanged();
        }
        #endregion

        #region [.context menu.]
        private void open_mainContextMenu()
        {
            deleteRowMenuItem.IsEnabled = (DGV.SelectedItem != null);

            mainContextMenu.Open( DGV );
        }

        private void addRowMenuItem_Click( object sender, EventArgs e ) 
        {
            if ( !_DGVRows.IsAddingNew && !_DGVRows.IsEditingItem )
            {
                try
                {
                    var t = _DGVRows.AddNew();
                    _DGVRows.CommitNew();

                    DGV.ScrollIntoView( t, DGV.Columns[ 1 ] );
                    //_DGVRows.EditItem( t );
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }
            }
        }
        private void deleteRowMenuItem_Click( object sender, EventArgs e ) 
        {
            if ( _DGVRows.IsAddingNew   ) _DGVRows.CommitNew();
            if ( _DGVRows.IsEditingItem ) _DGVRows.CommitEdit();

            try
            {
                var selItems = DGV.SelectedItems.Cast< RequestHeader >().ToList( DGV.SelectedItems.Count );
                foreach ( var rh in selItems )
                {
                    _DGVRows.Remove( rh );
                }
                _DGVRows.CommitEdit();
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
            Fire_OnRequestHeadersCountChanged();
        }
        #endregion
    }
}
