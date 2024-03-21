using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;

using ReactiveUI;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ColumnsVisibilityEditor : Window
    {
        /// <summary>
        /// 
        /// </summary>
        private sealed class WordItem : ReactiveObject
        {
            public WordItem( DataGridColumn col, bool isVisibleAlways )
            {
                Column = col;
                Text   = col.GetTextEx();
                IsVisibleAlways = isVisibleAlways;
                if ( isVisibleAlways )
                {
                    IsVisible = true;
                    //Text += " (always visible)";
                }
            }

            private int _ViewIndex;
            public int ViewOrderNumber
            { 
                get => _ViewIndex; 
                set => this.RaiseAndSetIfChanged( ref _ViewIndex, value );
            }

            public bool IsVisible 
            { 
                get => Column.IsVisible;
                set
                {
                    if ( IsVisibleAlways && !value ) value = true;
                    if ( Column.IsVisible != value )
                    {
                        Column.IsVisible = value;
                        this.RaisePropertyChanged( nameof(IsVisible) );
                    }
                }
            }
            public bool IsVisibleAlways { get; }
            public DataGridColumn Column { get; }
            public string Text{ get; }
            //public string ToolTip => IsVisibleAlways ? "Visible Always" : string.Empty;

            public override string ToString() => Text;
        }

        #region [.fields.]
        private DataGrid DGV;
        private DataGridCollectionView _DGVRows;
        private bool _HasChanges;
        private Dictionary< DataGridColumn, bool > _SaveColumnIsVisibleDict;
        #endregion

        #region [.ctor().]
        public ColumnsVisibilityEditor()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        internal ColumnsVisibilityEditor( DataGrid targetDGV ) : this() 
        {
            _SaveColumnIsVisibleDict = new Dictionary< DataGridColumn, bool >( targetDGV.Columns.Count );
            var items = new List< WordItem >( targetDGV.Columns.Count );
            foreach ( var col in targetDGV.Columns )
            {
                items.Add( new WordItem( col, isVisibleAlways: col.CellStyleClasses.Contains( "visible_always_sign" ) ) );
                _SaveColumnIsVisibleDict[ col ] = col.IsVisible;
            }
            DGV.ItemsSource = _DGVRows = new DataGridCollectionView( items );
            DGV.PointerPressed     += DGV_PointerPressed;
            DGV.CellPointerPressed += DGV_CellPointerPressed;

            DataGrid_SelectRect_Extension.Create( this, DGV, this.FindControl< Rectangle >( "selectRect" ), e => e.Column.DisplayIndex == 0, items );
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );

            DGV = this.FindControl< DataGrid >( nameof(DGV) );
            DGV.LoadingRow += DGV_LoadingRow;
            //---DGV.Styles.Add( GlobalStyles.Dark );

            this.Find< Button >( "okButton"     ).Click += (s, e) => OkButtonProcess();
            this.Find< Button >( "cancelButton" ).Click += (s, e) => this.Close();
        }

        protected override void OnOpened( EventArgs e )
        {
            base.OnOpened( e );

            CorrectCheckBoxesStyle();
            DGV.Focus();
        }
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            //Dispose();
            if ( !Success && _HasChanges )
            {
                RollbackChanges();
            }
        }
        protected override void OnKeyDown( KeyEventArgs e )
        {
            switch ( e.Key )
            {
                case Key.Escape:
                    if ( !_HasChanges )
                    { 
                        e.Handled = true;
                        this.Close(); 
                    }
                return;

                case Key.Enter: //Ok
                    if ( OkButtonProcess() )
                    {
                        e.Handled = true;
                        return;
                    }
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
        protected override async void OnPropertyChanged( AvaloniaPropertyChangedEventArgs e )
        {
            if ( (e.Property == Window.WindowStateProperty) && ((WindowState) e.NewValue == WindowState.Minimized) )
            {
                var state = (WindowState) e.OldValue;
                await Task.Delay( 1 );
                this.WindowState = state;
            }
            base.OnPropertyChanged( e );
        }
        #endregion

        #region [.public methods.]
        public bool Success { get; private set; }
        #endregion

        #region [.private methods.]
        private bool OkButtonProcess()
        {
            this.Success = true;
            this.Close();
            return (true);
        }
        private void SetHasChanges()
        {
            _HasChanges = false;
            foreach ( var w in _DGVRows.SourceCollection.Cast< WordItem >() )
            {
                if ( _SaveColumnIsVisibleDict.TryGetValue( w.Column, out var isVisible ) && (w.IsVisible != isVisible) )
                {
                    _HasChanges = true;
                    break;
                }
            }

            var lastChar = this.Title.LastOrDefault();
            if ( _HasChanges )
            {
                if ( lastChar != '*' )
                {
                    this.Title += " *";
                }
            }
            else if ( lastChar == '*' )
            {
                this.Title = this.Title.Substring( 0, this.Title.Length - 2 );
            }
        }
        private void RollbackChanges()
        {
            foreach ( var (col, isVisible) in _SaveColumnIsVisibleDict ) 
            {
                col.IsVisible = isVisible;
            }
        }
        private void TryCheckByKey()
        {
            var selItem = DGV.SelectedItem;
            if ( !IsAllowChange_DGV_SelectedItem( selItem ) ) return;
            if ( !TryGetCheckBox( selItem, out var checkBox ) ) return;

            checkBox.IsChecked = !checkBox.IsChecked.GetValueOrDefault();
            SetHasChanges();
        }
        private bool IsAllowChange_DGV_SelectedItem( object selItem ) => ((selItem is WordItem w) && !w.IsVisibleAlways);
        private bool TryGetCheckBox( object selItem, out CheckBox checkBox )
        {
            checkBox = DGV.Columns.Select( col => (col.GetCellContent( selItem ) is CheckBox checkBox) ? checkBox : null )
                                  .FirstOrDefault( checkBox => checkBox != null );
            return (checkBox != null);
        }
        private void CorrectCheckBoxesStyle()
        {
            //need for Visual-Form-Designer
            if ( _DGVRows == null ) return;

            foreach ( var w in _DGVRows.SourceCollection.Cast< WordItem >() )
            {
                if ( TryGetCheckBox( w, out var checkBox ) )
                {
                    var brush = w.IsVisibleAlways ? Brushes.DimGray : Brushes.Black;
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

        private void DGV_LoadingRow( object sender, DataGridRowEventArgs e )
        {
            var index = e.Row.GetIndex();

            var w = (WordItem) _DGVRows[ index ];
            w.ViewOrderNumber = index + 1;
        }
        private async void DGV_PointerPressed( object sender, PointerPressedEventArgs e )
        {
            var p = e.GetCurrentPoint( this/*null*/ );
            switch ( p.Properties.PointerUpdateKind )
            {
                case PointerUpdateKind.LeftButtonPressed:
                    var columnHeader = (e.Source as Control)?.GetSelfAndVisualAncestors().OfType< DataGridColumnHeader >().FirstOrDefault();
                    if ( columnHeader == null )
                    {
                        DGV.SelectedItems.Clear();
                    }
                    else if ( DGV.Columns[ 0 ]?.Header == columnHeader.Content )
                    {
                        DGV.SelectedItems.Clear();
                        await Task.Delay( 100 );
                        DGV.SelectAll();
                    }
                    break;

                //case PointerUpdateKind.RightButtonPressed:
                //    e.Pointer.Capture( null );
                //    e.Handled = true;
                //
                //    //---open_mainContextMenu();
                //    break;
            }
        }
        private void DGV_CellPointerPressed( object sender, DataGridCellPointerPressedEventArgs e )
        {
            var p = e.PointerPressedEventArgs.GetCurrentPoint( null );
            if ( p.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed ) return;
            if ( !(e.Column.GetCellContent( e.Row ) is CheckBox checkBox) ) return;
            if ( !IsAllowChange_DGV_SelectedItem( DGV.SelectedItem ) ) return;

            //---var trb = checkBox.TransformedBounds;
            var trb = checkBox.GetTransformedBounds();
            if ( trb.HasValue && trb.Value.Contains( p.Position ) )
            {
                //---checkBox.IsChecked = !checkBox.IsChecked.GetValueOrDefault();

                var isChecked = !checkBox.IsChecked.GetValueOrDefault();
                var selItems = DGV.SelectedItems.Cast< WordItem >();
                foreach ( var rh in selItems )
                {
                    rh.IsVisible = isChecked;
                }
                SetHasChanges();
            }
        }
        #endregion
    }
}
