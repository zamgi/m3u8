using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class RowNumbersPainter : IDisposable
    {
        private DataGridView _DGV;
        private StringFormat _SF_RowNumbers;
        private Brush _BackBrush;
        private Pen   _RectPen;
        private Brush _TextBrush;

        public static RowNumbersPainter Create( DataGridView dgv, bool useSelectedBackColor = false ) => new RowNumbersPainter( dgv, useSelectedBackColor );

        private RowNumbersPainter( DataGridView dgv, bool useSelectedBackColor )
        {
            _SF_RowNumbers = new StringFormat( StringFormatFlags.NoWrap ) { Trimming = StringTrimming.EllipsisCharacter, Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            _DGV = dgv;
            if ( useSelectedBackColor )
            {
                _BackBrush = new SolidBrush( _DGV.DefaultCellStyle.SelectionBackColor );
                _RectPen   = new Pen       ( _DGV.DefaultCellStyle.SelectionForeColor );
                _TextBrush = new SolidBrush( _DGV.DefaultCellStyle.SelectionForeColor );
                _DGV.CellPainting += DGV_CellPaintingRowNumbers_useSelectedBackColor;
            }
            else
            {
                _DGV.CellPainting += DGV_CellPaintingRowNumbers;
            }
        }
        public void Dispose()
        {
            _DGV.CellPainting -= DGV_CellPaintingRowNumbers_useSelectedBackColor;
            _DGV.CellPainting -= DGV_CellPaintingRowNumbers;
            _SF_RowNumbers.Dispose();

            _BackBrush?.Dispose();
            _RectPen  ?.Dispose();
            _TextBrush?.Dispose();
        }

        private void DGV_CellPaintingRowNumbers_useSelectedBackColor( object sender, DataGridViewCellPaintingEventArgs e )
        {
            if ( (0 <= e.RowIndex) && (e.ColumnIndex < 0) ) //row numbers
            {
                e.Handled = true;
                var gr = e.Graphics;

                Brush backBrush;
                Pen   rectPen;
                Brush textBrush;
                if ( e.State.IsSelected() )
                {
                    //var isFocused = (_DGV.CurrentCell.OwningRow.Index == e.RowIndex); // ((e.PaintParts & DataGridViewPaintParts.Focus) == DataGridViewPaintParts.Focus);

                    backBrush = _BackBrush; // Brushes.SkyBlue;
                    rectPen   = _RectPen;   // Pens.DeepSkyBlue;
                    textBrush = _TextBrush; // Brushes.WhiteSmoke;
                }
                else
                {
                    backBrush = Brushes.LightGray;
                    rectPen   = Pens.Silver;
                    textBrush = Brushes.DimGray;
                }

                gr.FillRectangle( backBrush, e.CellBounds );

                var rc = e.CellBounds;
                    rc.Y++; rc.Height -= 3;
                    rc.Width -= 2;
                gr.DrawRectangle( rectPen, rc );

                gr.DrawString( (e.RowIndex + 1).ToString(), _DGV.Font, textBrush, e.CellBounds, _SF_RowNumbers );
            }
        }
        private void DGV_CellPaintingRowNumbers( object sender, DataGridViewCellPaintingEventArgs e )
        {
            if ( (0 <= e.RowIndex) && (e.ColumnIndex < 0) ) //row numbers
            {
                e.Handled = true;
                var gr = e.Graphics;
                gr.FillRectangle( Brushes.LightGray, e.CellBounds );

                var rc  = e.CellBounds; rc.Height -= 2; rc.Width -= 2;
                var pen = e.State.IsSelected() ? Pens.DarkBlue : Pens.Silver;
                gr.DrawRectangle( pen, rc );

                gr.DrawString( (e.RowIndex + 1).ToString(), _DGV.Font, Brushes.DimGray, e.CellBounds, _SF_RowNumbers );
            } 
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal struct SortInfo
    {
        /// <summary>
        /// 
        /// </summary>
        [DataContract]
        private struct _4Json_
        {
            [DataMember(EmitDefaultValue=false)]
            public int?       ColumnIndex { get; set; }

            [DataMember(EmitDefaultValue=false)]
            public SortOrder? Order       { get; set; }
        }

        public int?       ColumnIndex { get; private set; }
        public SortOrder? Order       { get; private set; }

        public bool       HasSorting => (ColumnIndex.HasValue && Order.HasValue);
        public bool       TryGetSorting( out int columnIndex, out SortOrder order )
        {
            columnIndex = ColumnIndex.GetValueOrDefault();
            order       = Order      .GetValueOrDefault();
            return (HasSorting);
        }

        public void SetSortOrderAndSaveCurrent( int columnIndex )
        {
            if ( !this.ColumnIndex.HasValue || (this.ColumnIndex.Value != columnIndex) )
            {
                this.ColumnIndex = columnIndex;
                this.Order       = SortOrder.Ascending;
            }
            else if ( this.Order == SortOrder.Ascending )
            {
#if DEBUG
                Debug.Assert( (this.ColumnIndex == columnIndex) );
#endif
                this.Order = SortOrder.Descending;
            }
            else
            {
                this.ColumnIndex = null;
                this.Order       = null;
            }
        }
        public string ToJson() => (new _4Json_() { ColumnIndex = ColumnIndex, Order = Order }).ToJSON();
        public static SortInfo FromJson( string json )
        {
            try
            {
                var _ = Extensions.FromJSON<_4Json_>( json );
                return (new SortInfo() { ColumnIndex = _.ColumnIndex, Order = _.Order });
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
                return (default);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class BlinkManager
    {
        private static HashSet< Control > _BlinkedControls = new HashSet< Control >();

        public static async void FocusAndBlinkBackColor( this Control control, Color bgColor )
        {
            control.Focus();
            if ( _BlinkedControls.Add( control ) )
            {
                var bc = control.BackColor;
                control.BackColor = bgColor;
                await Task.Delay( 330 );
                control.BackColor = bc;
                _BlinkedControls.Remove( control );
                //Task.Delay( 330 ).ContinueWith( _ => control.BackColor = bc, TaskScheduler.FromCurrentSynchronizationContext() );
            }
        }
        public static void FocusAndBlinkBackColor( this Control control ) => control.FocusAndBlinkBackColor( Color.HotPink );
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class AssemblyInfoHelper
    {
        public static string AssemblyTitle
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof(AssemblyTitleAttribute), false );
                if ( 0 < attributes.Length )
                {
                    var titleAttribute = (AssemblyTitleAttribute) attributes[ 0 ];
                    if ( !string.IsNullOrEmpty( titleAttribute.Title ) )
                    {
                        return (titleAttribute.Title);
                    }
                }
                return (Path.GetFileNameWithoutExtension( Assembly.GetExecutingAssembly().CodeBase )); 
            }
        }
        public static string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string AssemblyDescription
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof(AssemblyDescriptionAttribute), false );
                if ( 0 < attributes.Length )
                {
                    return ((AssemblyDescriptionAttribute) attributes[ 0 ]).Description; 
                }
                return (string.Empty);
            }
        }
        public static string AssemblyProduct
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof(AssemblyProductAttribute), false );
                if ( 0 < attributes.Length )
                {
                    return ((AssemblyProductAttribute) attributes[ 0 ]).Product;
                }
                return (string.Empty); 
            }
        }
        public static string AssemblyCopyright
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof(AssemblyCopyrightAttribute), false );
                if ( 0 < attributes.Length )
                {
                    return ((AssemblyCopyrightAttribute) attributes[ 0 ]).Copyright;
                }
                return (string.Empty); 
            }
        }
        public static string AssemblyCompany
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof(AssemblyCompanyAttribute), false );
                if ( 0 < attributes.Length )
                {
                    return ((AssemblyCompanyAttribute) attributes[ 0 ]).Company;
                }
                return (string.Empty); 
            }
        }
        public static string AssemblyLastWriteTime => File.GetLastWriteTime( Assembly.GetExecutingAssembly().Location ).ToString( "dd.MM.yyyy HH:mm" );
    }
}
