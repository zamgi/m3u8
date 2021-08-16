using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class RowNumbersPainter : IDisposable
    {
        private DataGridView _DGV;
        private StringFormat _SF_RowNumbers;
        private Brush _SelBackBrush;
        private Brush _SelTextBrush;
        private Pen   _SelBorderPen;

        private Brush _BackBrush;
        private Brush _TextBrush;
        private Color _TextColor;
        private Pen   _BorderPen;

        private bool  _UseColumnsHoverHighlight;
        private Brush _BackBrushColumnHover;
        private Brush _BackBrushColumnHoverPushed;

        public static RowNumbersPainter Create( DataGridView dgv, bool useSelectedBackColor = true, bool useColumnsHoverHighlight = true )
            => new RowNumbersPainter( dgv, useSelectedBackColor, useColumnsHoverHighlight );

        private RowNumbersPainter( DataGridView dgv, bool useSelectedBackColor, bool useColumnsHoverHighlight )
        {
            _SF_RowNumbers = new StringFormat( StringFormatFlags.NoWrap ) { Trimming = StringTrimming.EllipsisCharacter, Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            _BackBrush = Brushes.LightGray;
            _TextBrush = Brushes.DimGray;
            _TextColor = Color.DimGray;
            _BorderPen = Pens.Silver;
            
            if ( useColumnsHoverHighlight )
            {
                _UseColumnsHoverHighlight = true;
                _BackBrushColumnHover       = new SolidBrush( Color.FromArgb( 224, 224, 224 ) ); // Brushes.Gainsboro;
                _BackBrushColumnHoverPushed = new SolidBrush( Color.FromArgb( 235, 235, 235 ) ); // Brushes.WhiteSmoke;
            }

            _DGV = dgv;
            if ( useSelectedBackColor )
            {
                _SelBackBrush = new SolidBrush( _DGV.DefaultCellStyle.SelectionBackColor );
                _SelBorderPen = new Pen       ( _DGV.DefaultCellStyle.SelectionForeColor );
                _SelTextBrush = new SolidBrush( _DGV.DefaultCellStyle.SelectionForeColor );
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

            _BackBrushColumnHover      ?.Dispose();
            _BackBrushColumnHoverPushed?.Dispose();

            _SelBackBrush?.Dispose();
            _SelBorderPen?.Dispose();
            _SelTextBrush?.Dispose();
        }

        private void DGV_CellPaintingRowNumbers_useSelectedBackColor( object sender, DataGridViewCellPaintingEventArgs e )
        {
            if ( 0 <= e.RowIndex ) //row numbers
            {
                if ( e.ColumnIndex < 0 ) //row numbers
                {
                    e.Handled = true;
                    var gr = e.Graphics;

                    Brush backBrush;
                    Pen borderPen;
                    Brush textBrush;
                    if ( e.State.IsSelected() )
                    {
                        backBrush = _SelBackBrush;
                        borderPen = _SelBorderPen;
                        textBrush = _SelTextBrush;
                    }
                    else
                    {
                        backBrush = _BackBrush;
                        borderPen = _BorderPen;
                        textBrush = _TextBrush;
                    }

                    gr.FillRectangle( backBrush, e.CellBounds );

                    var rc = e.CellBounds;
                    rc.Y++; rc.Height -= 3;
                    rc.Width -= 2;
                    gr.DrawRectangle( borderPen, rc );

                    gr.DrawString( (e.RowIndex + 1).ToString(), _DGV.Font, textBrush, e.CellBounds, _SF_RowNumbers );
                }
            }
            else //columns headers
            {
                DrawColumnsHeaders( e );
            }
        }
        private void DGV_CellPaintingRowNumbers( object sender, DataGridViewCellPaintingEventArgs e )
        {
            if ( 0 <= e.RowIndex ) //row numbers
            {
                if ( e.ColumnIndex < 0 ) //row numbers
                {
                    e.Handled = true;
                    var gr   = e.Graphics;
                    var font = _DGV.Font;

                    gr.FillRectangle( _BackBrush, e.CellBounds );

                    var rc  = e.CellBounds; rc.Height -= 2; rc.Width -= 2;
                    var pen = (e.State.IsSelected() ? Pens.DarkBlue : _BorderPen);
                    gr.DrawRectangle( pen, rc );

                    gr.DrawString( (e.RowIndex + 1).ToString(), font, _TextBrush, e.CellBounds, _SF_RowNumbers );
                }
            }
            else //columns headers
            {
                DrawColumnsHeaders( e );
            }
        }

        [M(O.AggressiveInlining)] private void DrawColumnsHeaders( DataGridViewCellPaintingEventArgs e )
        {
            e.Handled = true;
            var gr   = e.Graphics;
            var font = _DGV.Font;

            var br = (_UseColumnsHoverHighlight && e.CellBounds.Contains( _DGV.PointToClient( Control.MousePosition ) )
                                                && _DGV.IsColumnSortable( e.ColumnIndex )) 
                            ? ((Control.MouseButtons == MouseButtons.Left) ? _BackBrushColumnHoverPushed
                                                                           : _BackBrushColumnHover) 
                            : _BackBrush;
            gr.FillRectangle( br, e.CellBounds );

            var rc = e.CellBounds; rc.Height -= 2; rc.Width -= 2;
            gr.DrawRectangle( _BorderPen, rc );

            e.PaintContent( e.CellBounds );
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
                return (Path.GetFileNameWithoutExtension( Assembly.GetExecutingAssembly().Location/*.CodeBase*/ )); 
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
