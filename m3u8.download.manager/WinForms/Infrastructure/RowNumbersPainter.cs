using System;
using System.Drawing;
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
        //private Color _TextColor;
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
            //_TextColor = Color.DimGray;
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
                _DGV.CellPainting += DGV_CellPaintingRowNumbers_UseSelectedBackColor;
            }
            else
            {
                _DGV.CellPainting += DGV_CellPaintingRowNumbers;
            }
        }
        public void Dispose()
        {
            _DGV.CellPainting -= DGV_CellPaintingRowNumbers_UseSelectedBackColor;
            _DGV.CellPainting -= DGV_CellPaintingRowNumbers;

            _SF_RowNumbers.Dispose();

            _BackBrushColumnHover      ?.Dispose();
            _BackBrushColumnHoverPushed?.Dispose();

            _SelBackBrush?.Dispose();
            _SelBorderPen?.Dispose();
            _SelTextBrush?.Dispose();
        }

        private void DGV_CellPaintingRowNumbers_UseSelectedBackColor( object sender, DataGridViewCellPaintingEventArgs e )
        {
            if ( 0 <= e.RowIndex ) //row numbers
            {
                if ( e.ColumnIndex < 0 ) //row numbers
                {
                    e.Handled = true;
                    var gr = e.Graphics;

                    Brush backBrush;
                    Pen   borderPen;
                    Brush textBrush;
                    bool  drawCircle;
                    if ( e.State.IsSelected() )
                    {
                        //var isFocused = (_DGV.CurrentCell.OwningRow.Index == e.RowIndex); // ((e.PaintParts & DataGridViewPaintParts.Focus) == DataGridViewPaintParts.Focus);

                        backBrush = _SelBackBrush;
                        borderPen = _SelBorderPen;
                        textBrush = _SelTextBrush;

                        drawCircle = (1 < _DGV.SelectedRows.Count) && (_DGV.CurrentCell.RowIndex == e.RowIndex);
                    }
                    else
                    {
                        backBrush = _BackBrush; // Brushes.LightGray;
                        borderPen = _BorderPen; // Pens.Silver;
                        textBrush = _TextBrush; // Brushes.DimGray;
                        drawCircle = false;
                    }

                    gr.FillRectangle( backBrush, e.CellBounds );

                    var rc = e.CellBounds;
                    rc.Y++; rc.Height -= 3;
                    rc.Width -= 2;
                    gr.DrawRectangle( borderPen, rc );

                    if ( drawCircle )
                    {
                        var cb = e.CellBounds;

                        const float VERGE = 15;
                        var circle_rc = new RectangleF( cb.X + cb.Width / 2.0f - VERGE / 2, cb.Y + cb.Height / 2.0f - VERGE / 2, VERGE, VERGE );
                        gr.FillEllipse( Brushes.Blue, circle_rc );

                        circle_rc.Inflate( 0.5f, 0.5f );
                        gr.DrawEllipse( Pens.White, circle_rc );
                    }

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
}
