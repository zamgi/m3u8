using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using m3u8.download.manager.controllers;
using SETTINGS = m3u8.download.manager.controllers.SettingsPropertyChangeController;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FileNameExcludesWordsEditor : Window
    {
        /// <summary>
        /// 
        /// </summary>
        private sealed class Fake_t : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private int _ViewIndex;
            public int    ViewIndex 
            { 
                get => _ViewIndex; 
                set
                {
                    if ( _ViewIndex != value )
                    {
                        _ViewIndex = value;
                        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( nameof(ViewIndex) ) );
                    }
                }
            }
            public string Text { get; set; }
            public override string ToString() => Text;
        }

        #region [.fields.]
        private DataGrid DGV;
        private TextBox  filterTextBox;
        private Button   clearFilterButton;
        private DataGridCollectionView _DGVRows;

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
            var items = (from s in excludesWords select new Fake_t() { Text = s }).ToList();
            DGV.Items = _DGVRows = new DataGridCollectionView( items );
            _DGVRows.AddNew();
            _DGVRows.CommitNew();
            //_DGVRows.CollectionChanged += _DGVRows_CollectionChanged;
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );

            DGV = this.FindControl< DataGrid >( nameof(DGV) );
            DGV.LoadingRow += DGV_LoadingRow;
            DGV.Styles.Add( GlobalStyles.Dark );

            filterTextBox     = this.Find< TextBox >( nameof(filterTextBox) );
            clearFilterButton = this.Find< Button  >( nameof(clearFilterButton) ); clearFilterButton.Click += clearFilterButton_Click;

            this.Find< Button >( "okButton"     ).Click += (s, e) => OkButtonProcess();
            this.Find< Button >( "cancelButton" ).Click += (s, e) => this.Close();

            filterTextBox_SubscribeDisposable = filterTextBox.GetObservable( TextBox.TextProperty ).Subscribe( filterTextBox_TextChanged );
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
        public ICollection< string > GetFileNameExcludesWords()
        {
            var lst = new List< string >( _DGVRows.TotalItemCount );
            foreach ( var t in _DGVRows.SourceCollection.Cast< Fake_t >() )
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

        private void _DGVRows_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            //if ( e.Action == NotifyCollectionChangedAction. )
        }
        private void DGV_LoadingRow( object sender, DataGridRowEventArgs e )
        {
            var index = e.Row.GetIndex();

            var t = (Fake_t) _DGVRows[ index ];
            t.ViewIndex = index + 1;

            #region comm.
            //Debug.WriteLine( $"index: {index}" );

            //----e.Row.BeginInit();

            //bool isVisible;
            //if ( _LastFilterText.IsNullOrEmpty() )
            //{
            //    isVisible = true;
            //}
            //else
            //{
            //    var value = _ExcludesWords[ index ];
            //    isVisible = (value.IndexOf( _LastFilterText, StringComparison.InvariantCultureIgnoreCase ) != -1);

            //}
            //if ( e.Row.IsVisible )
            //{
            /*
            e.Row.Header = new DataGridRowHeader()
            {
                Content = new DataGridCell()
                {
                    Content = new TextBlock() { Text = $"{(index + 1)}.", Foreground = Brushes.Red, Background = Brushes.LightGray },
                },
            };
            */
            //}
            //e.Row.IsVisible = isVisible;

            //e.Row.InvalidateArrange();
            //e.Row.InvalidateMeasure();
            //e.Row.InvalidateVisual();
            //e.Row.EndInit();
            #endregion
        }

        private void clearFilterButton_Click( object sender, RoutedEventArgs e ) => filterTextBox.Text = null;

        private string _LastFilterText;
        private async void filterTextBox_TextChanged( string value )
        {
            var text = value?.Trim();
            Debug.WriteLine( $"filterTextBox_TextChanged: '{text}'" );

            if ( _LastFilterText != text )
            {
                await Task.Delay( 250 );
                _LastFilterText = text;
                filterTextBox_TextChanged( text );
                return;
            }

            await Dispatcher.UIThread.InvokeAsync( () => processFilter( text ) );
        }
        private void processFilter( string filterText )
        {
            Debug.WriteLine( $"processFilter: '{filterText}'" );
            var isEmpty = filterText.IsNullOrEmpty();

            clearFilterButton.IsVisible = !isEmpty;

            //if ( _DGVRows.IsAddingNew || _DGVRows.IsEditingItem )
            //{
            //    return;
            //}

            if ( isEmpty )
            {
                _DGVRows.Filter = null;
            }
            else
            {
                _DGVRows.Filter = ( t ) =>
                {
                    var s = t.ToString();
                    return ((s != null) && (s.IndexOf( filterText, StringComparison.InvariantCultureIgnoreCase ) != -1));
                };
            }
        }
        #endregion
    }
}
