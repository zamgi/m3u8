using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia;
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
        private bool _HasChanges;
        #endregion

        #region [.ctor().]
        public RequestHeadersEditor() => this.InitializeComponent();
        //internal RequestHeadersEditor( DataGrid targetDGV ) : this() 
        //{
        //    _SaveColumnIsVisibleDict = new Dictionary< DataGridColumn, bool >( targetDGV.Columns.Count );
        //    var items = new List< WordItem >( targetDGV.Columns.Count );
        //    foreach ( var col in targetDGV.Columns )
        //    {
        //        items.Add( new WordItem( col, isVisibleAlways: col.CellStyleClasses.Contains( "visible_always_sign" ) ) );
        //        _SaveColumnIsVisibleDict[ col ] = col.IsVisible;
        //    }
        //    DGV.ItemsSource = _DGVRows = new DataGridCollectionView( items );
        //}

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );

            DGV = this.FindControl< DataGrid >( nameof(DGV) );
            DGV.LoadingRow         += DGV_LoadingRow;
            DGV.CellPointerPressed += DGV_CellPointerPressed;

            if ( DGV.Columns.OfType< DataGridCheckBoxColumn >().FirstOrDefault()?.Header is TextBlock tb )
            {
                tb.Text = "\u2713";
                tb.PointerPressed += Tb_PointerPressed;
            }
            DGV.ItemsSource = _DGVRows = new DataGridCollectionView( Array.Empty< RequestHeader >() );
        }

        private void Tb_PointerPressed( object sender, PointerPressedEventArgs e )
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
                case Key.Escape:
                    if ( !_HasChanges )
                    { 
                        e.Handled = true;
                        //this.Close(); 
                    }
                return;

                case Key.Enter: //Ok
                    //if ( OkButtonProcess() )
                    //{
                    //    e.Handled = true;
                    //    return;
                    //}
                break;

                case Key.Space:
                    if ( DGV.IsFocused )
                    {
                        TryCheckByKey();
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
            else
            {
                OnRequestHeadersCountChanged?.Invoke( 0, 0 );
            }

            //DGV_Resize( null, EventArgs.Empty );
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

        #region [.private methods.]
        private void SetHasChanges()
        {
            _HasChanges = false;
            //foreach ( var rh in _DGVRows.SourceCollection.Cast< RequestHeader >() )
            //{
            //    if ( _SaveColumnIsVisibleDict.TryGetValue( rh.Column, out var isVisible ) && (rh.IsVisible != isVisible) )
            //    {
            //        _HasChanges = true;
            //        break;
            //    }
            //}

            //var lastChar = this.Title.LastOrDefault();
            //if ( _HasChanges )
            //{
            //    if ( lastChar != '*' )
            //    {
            //        this.Title += " *";
            //    }
            //}
            //else if ( lastChar == '*' )
            //{
            //    this.Title = this.Title.Substring( 0, this.Title.Length - 2 );
            //}
        }
        private void TryCheckByKey()
        {
            if ( TryGetCheckBox( DGV.SelectedItem, out var checkBox ) )
            {
                checkBox.IsChecked = !checkBox.IsChecked.GetValueOrDefault();
                SetHasChanges();
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
            foreach ( var w in _DGVRows.SourceCollection.Cast< RequestHeader >() )
            {
                if ( TryGetCheckBox( w, out var checkBox ) )
                {
                    //var brush = w.IsVisibleAlways ? Brushes.DimGray : Brushes.Black;
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

        private void DGV_CellPointerPressed( object sender, DataGridCellPointerPressedEventArgs e )
        {
            var p = e.PointerPressedEventArgs.GetCurrentPoint( null );
            if ( p.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed ) return;
            if ( !(e.Column.GetCellContent( e.Row ) is CheckBox checkBox) ) return;

            //---var trb = checkBox.TransformedBounds;
            var trb = checkBox.GetTransformedBounds();
            if ( trb.HasValue && trb.Value.Contains( p.Position ) )
            {
                checkBox.IsChecked = !checkBox.IsChecked.GetValueOrDefault();
                SetHasChanges();
            }
        }
        private void DGV_LoadingRow( object sender, DataGridRowEventArgs e )
        {
            var index = e.Row.GetIndex();

            var w = (RequestHeader) _DGVRows[ index ];
            w.ViewOrderNumber = index + 1;
        }
        #endregion
    }
}
