using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FileNameExcludesWordsEditor : Window, IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        private sealed class WordItem : ReactiveObject
        {
            public WordItem() { }
            public WordItem( string text ) => Text = text;

            private int _ViewIndex;
            public int ViewOrderNumber
            { 
                get => _ViewIndex; 
                set => this.RaiseAndSetIfChanged( ref _ViewIndex, value );
            }

            public string Text { get; set; }
            #region comm.
            //private string _Text;
            //public string Text
            //{
            //    get => _Text;
            //    set => this.RaiseAndSetIfChanged( ref _Text, value );
            //} 
            #endregion

            public override string ToString() => Text;
        }

        #region [.fields.]
        private DataGrid DGV;
        private TextBox  filterTextBox;
        private Button   clearFilterButton;
        private DataGridCollectionView _DGVRows;
        private bool _HasChanges;

        private IDisposable filterTextBox_SubscribeDisposable;
        #endregion

        #region [.ctor().]
        public FileNameExcludesWordsEditor()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        internal FileNameExcludesWordsEditor( IReadOnlyCollection< string > excludesWords ) : this() 
        {
            var items = (from s in excludesWords select new WordItem( s )).ToList( excludesWords.Count );
            //---DGV.Items = _DGVRows = new DataGridCollectionView( items );
            DGV.ItemsSource = _DGVRows = new DataGridCollectionView( items );
            _DGVRows.CollectionChanged += (s, e) => SetHasChanges();
            _DGVRows.PropertyChanged   += (s, e) => SetHasChanges( (e.PropertyName == nameof(_DGVRows.IsEditingItem) && !_DGVRows.IsEditingItem) );
        }
        public void Dispose() => filterTextBox_SubscribeDisposable.Dispose_NoThrow();
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );

            DGV = this.FindControl< DataGrid >( nameof(DGV) );
            DGV.LoadingRow += DGV_LoadingRow;
            //---DGV.Styles.Add( GlobalStyles.Dark );

            filterTextBox     = this.Find< TextBox >( nameof(filterTextBox) );
            clearFilterButton = this.Find< Button  >( nameof(clearFilterButton) ); clearFilterButton.Click += clearFilterButton_Click;

            this.Find< Button >( "addNewRowButton" ).Click += addNewRowToDGV;
            this.Find< Button >( "okButton"        ).Click += (s, e) => OkButtonProcess();
            this.Find< Button >( "cancelButton"    ).Click += (s, e) => this.Close();

            filterTextBox_SubscribeDisposable = filterTextBox.GetObservable( TextBox.TextProperty ).Subscribe( filterTextBox_TextChanged );
        }

        protected override void OnOpened( EventArgs e )
        {
            base.OnOpened( e );

            DGV.Focus();
        }
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            filterTextBox_SubscribeDisposable.Dispose_NoThrow();
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

                case Key.Delete:
                    e.Handled = TryDeleteSelectedItemsFromDGV();
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
        public ICollection< string > GetFileNameExcludesWords()
        {
            var lst = new List< string >( _DGVRows.TotalItemCount );
            foreach ( var t in _DGVRows.SourceCollection.Cast< WordItem >() )
            {
                if ( !t.Text.IsNullOrWhiteSpace() )
                {
                    lst.Add( t.Text );
                }
            }
            return (lst);
        }
        #endregion

        #region [.private methods.]
        private bool OkButtonProcess()
        {
            this.Success = true;
            this.Close();
            return (true);
        }
        private bool TryDeleteSelectedItemsFromDGV()
        {
            if ( _DGVRows.IsAddingNew || _DGVRows.IsEditingItem )
            {
                return (false);
            }

            object[] to_array( IList selectedItems )
            {
                var count = selectedItems.Count;
                if ( 0 < count )
                {
                    var _array = new object[ count ];
                    for ( count--; 0 <= count; count-- )
                    {
                        _array[ count ] = selectedItems[ count ];
                    }
                    return (_array);
                }
                return (null);
            };

            var array = to_array( DGV.SelectedItems );
            var has = array.AnyEx();
            if ( has )
            {
                foreach ( var t in array )
                {
                    _DGVRows.Remove( t );
                }
                var i = 0;
                foreach ( var t in _DGVRows )
                {
                    ((WordItem) t).ViewOrderNumber = ++i;
                }
            }
            return (has);
        }
        private void SetHasChanges( bool hasChanges = true )
        {
            _HasChanges |= hasChanges;
            if ( hasChanges && (this.Title.LastOrDefault() != '*') )
            {
                this.Title += " *";
            }
        }

        private void addNewRowToDGV( object sender, RoutedEventArgs e )
        {
            if ( !_DGVRows.IsAddingNew && !_DGVRows.IsEditingItem )
            {
                var t = _DGVRows.AddNew();
                _DGVRows.CommitNew();
                try
                {
                    DGV.ScrollIntoView( t, DGV.Columns[ 1 ] );
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }
            }
        }
        private void DGV_LoadingRow( object sender, DataGridRowEventArgs e )
        {
            var index = e.Row.GetIndex();

            var t = (WordItem) _DGVRows[ index ];
            t.ViewOrderNumber = index + 1;
        }


        private string _LastFilterText;
        private async void filterTextBox_TextChanged( string value )
        {
            var text = value?.Trim();
            if ( _LastFilterText != text )
            {
                await Task.Delay( 250 );
                _LastFilterText = text;
                filterTextBox_TextChanged( text );
                return;
            }

            #region [.main routine.]
            var isEmpty = text.IsNullOrEmpty();

            clearFilterButton.IsVisible = !isEmpty;

            if ( _DGVRows != null )
            {
                if ( isEmpty )
                {
                    _DGVRows.Filter = null;
                }
                else
                {
                    _DGVRows.Filter = (t) => t.ToString().ContainsIgnoreCase( text );
                }
            }
            #endregion            
        }
        private void clearFilterButton_Click( object sender, RoutedEventArgs e ) => filterTextBox.Text = null;
        #endregion
    }
}
