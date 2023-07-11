using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ProgressBarUC : UserControl
    {
        public static readonly DirectProperty< ProgressBarUC, double> ValueProperty =
            AvaloniaProperty.RegisterDirect< ProgressBarUC, double >( nameof(Value), 
                (p) => p.Value, (p, v) => p.Value = v, defaultBindingMode: BindingMode.OneWay );

        #region [.field's.]
        private DockPanel dockPanel;
        private Border    leftBorder;
        #endregion

        #region [.ctor().]
        public ProgressBarUC()
        {
            this.InitializeComponent();

            dockPanel  = this.FindControl< DockPanel >( nameof(dockPanel)  ); dockPanel.PropertyChanged += dockPanel_PropertyChanged;
            leftBorder = this.FindControl< Border    >( nameof(leftBorder) ); 
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load( this );
        #endregion

        #region [.private.]
        private void dockPanel_PropertyChanged( object sender, AvaloniaPropertyChangedEventArgs e )
        {
            if ( (e.Property == Layoutable.BoundsProperty) ||
                 (e.Property == Layoutable.WidthProperty) 
               )
            {
                this.Value = _Value;
            }
        }

        [M(O.AggressiveInlining)] private static bool IsValid( in double d ) => (!double.IsNaN( d ) && (0 < d));
        private double GetTotalWidth_Reccurent( int deduction = 6 )
        {
            #region [.-1-.]
            var grid = dockPanel.GetVisualAncestors().OfType< Grid >().FirstOrDefault();
            if ( grid != null )
            {
                var aw = grid.ColumnDefinitions.FirstOrDefault()?.ActualWidth;
                if ( aw.HasValue && IsValid( aw.Value ) )
                {
                    return (aw.Value - deduction);
                }
            }
            #endregion

            #region [.-2-.]
            //---for ( IControl p = dockPanel; p != null; p = p.Parent )
            for ( Layoutable p = dockPanel; p != null; p = (Layoutable) p.GetVisualParent() )
            {
                if ( !double.IsNaN( p.Width ) )
                {
                    return (p.Width - deduction);
                }
            }
            return (double.NaN);
            #endregion
        }
        #endregion

        #region [.public.]
        private double _Value;
        public double Value
        {
            get => _Value;
            set
            {
                _Value = value;

                var totalWidth = GetTotalWidth_Reccurent();
                if ( IsValid( totalWidth ) )
                {
                    leftBorder.Width = value * (totalWidth / 100);
                }
                else
                {
                    leftBorder.Width = 0d;
                }
            }
        }
        #endregion
    }
}
