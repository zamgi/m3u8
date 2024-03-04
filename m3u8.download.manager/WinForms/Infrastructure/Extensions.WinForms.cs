using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static partial class Extensions//_WinForms
    {
        public static void MessageBox_ShowInformation( this IWin32Window owner, string text, string caption ) => MessageBox.Show( owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information );
        public static void MessageBox_ShowError( this IWin32Window owner, string text, string caption ) => MessageBox.Show( owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error );
        public static void MessageBox_ShowError( this Exception ex, string caption ) => MessageBox_ShowError( ex.ToString(), caption );
        public static void MessageBox_ShowError( string text, string caption )
        {
            var form = Application.OpenForms.Cast< Form >().FirstOrDefault();
            if ( (form != null) && !form.IsDisposed && form.IsHandleCreated )
            {
                form.MessageBox_ShowError( text, caption );
            }
            else
            {
                MessageBox.Show( text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error );
            }            
        }
        public static DialogResult MessageBox_ShowQuestion( this IWin32Window owner, string text, string caption
            , MessageBoxButtons buttons = MessageBoxButtons.YesNo, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1 )
            => MessageBox.Show( owner, text, caption, buttons, MessageBoxIcon.Question, defaultButton );

        public static void SetEnabledAllChildControls( this Control parentControl, bool enabled ) => parentControl.Controls.Cast< Control >().ToList().ForEach( c => c.Enabled = enabled );
        public static bool IsSelected( this TabPage tabPage )
        {
            var tabControl = (TabControl) tabPage.Parent;
            if ( tabControl != null )
            {
                var selIdx = tabControl.SelectedIndex;
                if ( selIdx != -1 )
                {
                    return (tabControl.TabPages[ selIdx ] == tabPage);
                }
            }
            return (false);
        }
        public static void SetCursor( this SplitContainer splitContainer, Cursor cursor )
        {
            if ( splitContainer.Cursor != cursor )
            {
                var saved_cursor = splitContainer.Cursor;
                splitContainer.Cursor = cursor;
                foreach ( var c in splitContainer.Panel1.Controls.Cast< Control >() ) c.Cursor = saved_cursor;
                foreach ( var c in splitContainer.Panel2.Controls.Cast< Control >() ) c.Cursor = saved_cursor;
            }
        }

        [M(O.AggressiveInlining)] public static void SetRowInvisible( this DataGridView DGV, int index )
        {
#if DEBUG
            if ( (index < 0) || (DGV.RowCount <= index) )
            {
                return;
            }
#endif
            try
            {
                DGV.Rows[ index ].Visible = false;
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        [M(O.AggressiveInlining)] public static void ClearHeaderSortGlyphDirection( this DataGridView DGV ) => DGV.SetHeaderSortGlyphDirection( -1, SortOrder.None );
        [M(O.AggressiveInlining)] public static void SetHeaderSortGlyphDirection( this DataGridView DGV, int columnIndex, SortOrder sortOrder )
        {
            foreach ( var c in DGV.Columns.Cast< DataGridViewColumn >() )
            {
                c.HeaderCell.SortGlyphDirection = ((c.Index == columnIndex) ? sortOrder : SortOrder.None);
            }
        }
        [M(O.AggressiveInlining)] public static void SetFirstDisplayedScrollingRowIndex( this DataGridView DGV, int rowIndex )
        {
            try
            {
                DGV.FirstDisplayedScrollingRowIndex = rowIndex;
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        [M(O.AggressiveInlining)] public static int GetFirstDisplayedScrollingRowIndex( this DataGridView DGV, int defVal = 0 )
        {
            try
            {
                return (DGV.FirstDisplayedScrollingRowIndex);
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
                return (defVal);
            }
        }
        [M(O.AggressiveInlining)] public static void SetHandCursorIfNonHand( this DataGridView DGV )
        {
            if ( DGV.Cursor != Cursors.Hand )
            {
                DGV.Cursor = Cursors.Hand;
            }
        }
        [M(O.AggressiveInlining)] public static void SetDefaultCursorIfHand( this DataGridView DGV )
        {
            if ( DGV.Cursor == Cursors.Hand )
            {
                DGV.Cursor = Cursors.Default;
            }
        }
        [M(O.AggressiveInlining)] public static bool IsSelected( this DataGridViewElementStates state ) => ((state & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected);
        public static void SetForeColor4ParentOnly< T >( this Control parent, Color foreColor ) where T : Control
        {
            if ( parent is T )
            {
                var fc = parent.ForeColor;
                parent.ForeColor = foreColor;
                foreach ( var child in parent.Controls.Cast< Control >() )
                {
                    child.ForeColor = fc;
                }
            }
            foreach ( var child in parent.Controls.Cast< Control >() )
            {
                SetForeColor4ParentOnly< T >( child, foreColor );
            }
        }

        public static bool TryGetData< T >( this IDataObject d, out T t )
        {
            if ( d.GetData( typeof(T) ) is T x )
            {
                t = x;
                return (true);
            }
            t = default;
            return (false);
        }
        public static void SetDataEx< T >( this IDataObject d, T t ) => d.SetData( typeof(T), t );
        //public static T GetData< T >( this IDataObject d ) => (d.GetData( typeof(T) ) is T t) ? t : default;

        [M(O.AggressiveInlining)] public static bool IsColumnSortable( this DataGridView dgv, int columnIndex )
            => /*(0 <= columnIndex) && */ (columnIndex < 0) || (dgv.Columns[ columnIndex ].SortMode != DataGridViewColumnSortMode.NotSortable);

        public static void SetDoubleBuffered< T >( this T t, bool doubleBuffered ) where T : Control
        {          
            //Control.DoubleBuffered = true;
            var field = typeof(T).GetProperty( "DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance );
            field?.SetValue( t, doubleBuffered );
        }
    }
}
