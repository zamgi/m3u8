using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

using m3u8.download.manager.infrastructure;

using _Resources_ = m3u8.download.manager.Properties.Resources;

namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal class ComboBoxEx : ComboBox
    {
        /// <summary>
        /// 
        /// </summary>
        private static class WinApi
        {
            public delegate bool EnumWindowsProc( IntPtr hWnd, IntPtr lParam );

            /// <summary>
            /// 
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public RECT( in Rectangle rc ) => new RECT() { left = rc.Left, top = rc.Top, right = rc.Right, bottom = rc.Bottom };

                public int left;
                public int top;
                public int right;
                public int bottom;

                public int Height => bottom - top;
                public int Width => right - left;
            }

            /// <summary>
            /// 
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int x;
                public int y;
            }

            private const string USER32_DLL = "user32.dll";
            [DllImport(USER32_DLL)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool EnumChildWindows( IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam );
            [DllImport(USER32_DLL)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool GetClientRect( IntPtr hWnd, out RECT rc );
            [DllImport(USER32_DLL)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool GetCursorPos( out POINT pt );
            [DllImport(USER32_DLL)] public static extern int MapWindowPoints( IntPtr hWndFrom, IntPtr hWndTo, ref POINT pt, int cPoints );
            [DllImport(USER32_DLL)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool InvalidateRect( [In] IntPtr hWnd, [In] IntPtr rect, [In] int bErase );
            [DllImport(USER32_DLL)][return: MarshalAs(UnmanagedType.Bool)] public static extern bool InvalidateRect( [In] IntPtr hWnd, [In] ref RECT rect, [In] int bErase );
            [DllImport(USER32_DLL)] public static extern IntPtr WindowFromDC( [In] IntPtr hDC );
            [DllImport(USER32_DLL)] public static extern IntPtr SetCursor( [In] IntPtr hCursor );

            public const int WM_PASTE       = 0x0302;
            public const int EM_SETSEL      = 0x00B1;
            public const int WM_PAINT       = 0x000F;
            public const int WM_MOUSEMOVE   = 0x0200;
            public const int WM_LBUTTONDOWN = 0x0201;
            public const int WM_LBUTTONUP   = 0x0202;
            //public const int MK_LBUTTON     = 0x0001;

            public static POINT GetMousePos( IntPtr hWnd, in Point mousePt )
            {
                //var suc = GetCursorPos( out var pt ); Debug.Assert( suc );
                var pt = new POINT() { x = mousePt.X, y = mousePt.Y };
                var res = MapWindowPoints( IntPtr.Zero, hWnd, ref pt, 1 );
                return (pt);
            }
            public static bool InvalidateRect( IntPtr hWnd, bool eraseBackground = true ) => InvalidateRect( hWnd, IntPtr.Zero, bErase: eraseBackground ? 1 : 0 );
            public static bool InvalidateRect( IntPtr hWnd, in Rectangle rc, bool eraseBackground = true )
            {
                var rect = new RECT( rc );
                return (InvalidateRect( hWnd, ref rect, bErase: eraseBackground ? 1 : 0 ));
            }
            public static ushort LOWORD( int x ) => (ushort) x;
            public static ushort HIWORD( int x ) => (ushort) (x >> 16);
            public static int GET_X_LPARAM( IntPtr lp ) => ((int) (short) LOWORD( lp.ToInt32() ));
            public static int GET_Y_LPARAM( IntPtr lp ) => ((int) (short) HIWORD( lp.ToInt32() ));
            public static (int x, int y) GET_XY_LPARAM( IntPtr lp ) => (GET_X_LPARAM( lp ), GET_Y_LPARAM( lp ));
        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler OnClearMenuItemClick;
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler ClearButtonClick;

        public const int DEFAULT_MaxDropDownItems = 25;

        #region [.field's.]
        private bool             _SkipOnSelectedValueChanged;
        private StringCollection _ItemsValues;
        private int              _ItemsValuesMaxCount;
        #endregion

        #region [.ctor().]
        protected ComboBoxEx( StringCollection itemsValues, int itemsValuesMaxCount, Action< StringCollection > setItemsValues2OutIfNullAction, bool allowClearHistoryMenu = true )
        {
            if ( this.DesignMode ) return;

            this.FlatStyle        = FlatStyle.Flat;
            this.MaxDropDownItems = DEFAULT_MaxDropDownItems;
            this.DrawMode         = DrawMode.OwnerDrawFixed;

            _ItemsValues         = itemsValues ?? new StringCollection();
            _ItemsValuesMaxCount = itemsValuesMaxCount;
            if ( itemsValues == null ) setItemsValues2OutIfNullAction( _ItemsValues );

            #region [.context-menu.]
            if ( allowClearHistoryMenu )
            {
                var contextMenu = new ContextMenuStrip();
                    contextMenu.SuspendLayout();
                    contextMenu.Opening += ContextMenu_Opening;
                var sortMenuItem = new ToolStripMenuItem() { Text = "Sort history", Image = _Resources_.sort_asc };
                sortMenuItem.Click += new EventHandler( SortMenuItem_Click );
                var clearMenuItem = new ToolStripMenuItem() { Text = "Clear history", Image = _Resources_.delete };
                    clearMenuItem.Click += new EventHandler( ClearMenuItem_Click );
                    contextMenu.Items.AddRange( [ sortMenuItem, clearMenuItem ] );
                this.ContextMenuStrip = contextMenu;
                    contextMenu.ResumeLayout( false );
            }
            #endregion

            Refill();
        }
        #endregion

        #region [.EditNativeWindow. kill-obtruding-text-selection-in-edit-of-combobox & more.]
        /// <summary>
        /// 
        /// </summary>
        private sealed class EditNativeWindow : NativeWindow
        {
            private ComboBoxEx _Parent;
            public EditNativeWindow( ComboBoxEx parent ) => _Parent = parent;
            public static void _AssignHandle_( ComboBoxEx parent, IntPtr hWnd ) => (new EditNativeWindow( parent )).AssignHandle( hWnd );

            protected override void WndProc( ref Message m )
            {
                switch ( m.Msg )
                {
                    case WinApi.WM_PASTE: 
                        var text         = Clipboard.GetText();
                        var text_trimmed = PathnameCleaner.CleanPathnameAndFilename( text?.Trim() );

                        var t = (Start: _Parent.SelectionStart, Length: _Parent.SelectionLength);
                        var old_text = _Parent.Text;
                        var new_text = old_text.Substring( 0, t.Start ) + text_trimmed + old_text.Substring( t.Start + t.Length );
                        if ( new_text != old_text )
                        {
                            _Parent.Text = new_text;
                            _Parent.SelectionStart = t.Start + text_trimmed.Length;

                            return;
                        }
                        break;

                    case WinApi.EM_SETSEL:
                        if ( !_Parent.Focused && (m.LParam != IntPtr.Zero) )
                        {
                            m.LParam = IntPtr.Zero;
                        }
                        break;

                    case WinApi.WM_PAINT:
                        if ( _Parent.Text.IsNullOrEmpty() )
                        {
                            if ( !_Parent.PlaceHolderText.IsNullOrEmpty() )
                            {
                                base.WndProc( ref m );

                                DrawPlaceHolderText( m.HWnd );
                                m.Result = IntPtr.Zero;
                                return;
                            }
                        }
                        else if ( _Parent.DrawClearButton )
                        {
                            base.WndProc( ref m );

                            DrawClearButton( m.HWnd );
                            m.Result = IntPtr.Zero;
                            return;
                        }
                        break;

                    case WinApi.WM_MOUSEMOVE:
                        if ( _Parent.DrawClearButton && !_Parent.Text.IsNullOrEmpty() )
                        {
                            var pt = WinApi.GetMousePos( m.HWnd, Control.MousePosition );
                            var rc = GetClearButtonRect( m.HWnd );

                            var inClearButton = rc.Contains( pt.x, pt.y );
                            if ( inClearButton != _LastMouseMove_InClearButton )
                            {
                                _LastMouseMove_InClearButton = inClearButton;
                                WinApi.InvalidateRect( m.HWnd );
                            }
                            if ( inClearButton )
                            {
                                _Parent.Cursor = Cursors.Arrow;
                            }
                            //_Parent.Cursor = inClearButton ? Cursors.Arrow : Cursors.IBeam;
                        }
                        break;

                    case WinApi.WM_LBUTTONDOWN:
                        if ( _Parent.DrawClearButton && !_Parent.Text.IsNullOrEmpty() )
                        {
                            var pt = WinApi.GetMousePos( m.HWnd, Control.MousePosition );
                            var rc = GetClearButtonRect( m.HWnd );

                            _IsPushed_ClearButton = rc.Contains( pt.x, pt.y );
                            if ( _IsPushed_ClearButton )
                            {
                                if ( !_Parent.Focused )
                                {
                                    _Parent.Focus();
                                }

                                base.WndProc( ref m );

                                WinApi.InvalidateRect( m.HWnd );
                                m.Result = IntPtr.Zero;
                                return;
                            }
                        }
                        break;

                    case WinApi.WM_LBUTTONUP:
                        if ( _IsPushed_ClearButton && _Parent.DrawClearButton && !_Parent.Text.IsNullOrEmpty() )
                        {
                            _IsPushed_ClearButton = false;

                            var pt = WinApi.GetMousePos( m.HWnd, Control.MousePosition );
                            var rc = GetClearButtonRect( m.HWnd );

                            var inClearButton = rc.Contains( pt.x, pt.y );
                            if ( inClearButton )
                            {               
                                base.WndProc( ref m );

                                _Parent.Clear();
                                WinApi.InvalidateRect( m.HWnd );

                                m.Result = IntPtr.Zero;
                                return;
                            }
                        }
                        break;
                }
                base.WndProc( ref m );
            }

            private bool _LastMouseMove_InClearButton;
            private bool _IsPushed_ClearButton;
            private void DrawPlaceHolderText( IntPtr hWnd )
            {
                using ( var gr = Graphics.FromHwnd( hWnd ) )
                {
                    gr.DrawString( _Parent.PlaceHolderText, _Parent.Font, Brushes.Silver, 1, 1 );
                }
            }
            private void DrawClearButton( IntPtr hWnd )
            {
                var pt = WinApi.GetMousePos( hWnd, Control.MousePosition );
                var rc = GetClearButtonRect( hWnd );

                _LastMouseMove_InClearButton = rc.Contains( pt.x, pt.y );
                if ( _LastMouseMove_InClearButton && _IsPushed_ClearButton ) rc.Offset( 1, 1 );
                var br = _LastMouseMove_InClearButton ? Brushes.Black : Brushes.Silver;

                //DrawClearButton_Routine( handle, br, rc );
                using ( var gr  = Graphics.FromHwnd( hWnd ) )
                using ( var pen = new Pen( br, 2 ) )
                {
                    gr.DrawLine( pen, rc.X, rc.Y     , rc.Right, rc.Bottom );
                    gr.DrawLine( pen, rc.X, rc.Bottom, rc.Right, rc.Y );
                }
            }
            //private void DrawClearButton_Routine( IntPtr handle, Brush br, in RectangleF rc )
            //{
            //    using ( var gr  = Graphics.FromHwnd( handle ) )
            //    using ( var pen = new Pen( br, 2 ) )
            //    {
            //        gr.DrawLine( pen, rc.X, rc.Y     , rc.Right, rc.Bottom );
            //        gr.DrawLine( pen, rc.X, rc.Bottom, rc.Right, rc.Y );
            //    }
            //}
            private static RectangleF GetClearButtonRect( IntPtr hWnd )
            {
                var suc = WinApi.GetClientRect( hWnd, out var rc ); Debug.Assert( suc );

                var rect  = new RectangleF( rc.left + (rc.Width - rc.Height + 4), rc.top + 4, rc.Height - 8, rc.Height - 8 );
                return (rect);
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            WinApi.EnumChildWindows( this.Handle, EnumWindowsProc_Routine, IntPtr.Zero );
        }
        private bool EnumWindowsProc_Routine( IntPtr hWnd, IntPtr lParam )
        {
            EditNativeWindow._AssignHandle_( this, hWnd );

            return (true);
        }
        #endregion

        protected virtual bool Fill_ItemsValues( StringCollection itemsValues ) => false;
        protected override void CreateHandle()
        {
            base.CreateHandle();
            if ( Fill_ItemsValues( _ItemsValues ) )
            {
                Refill();
                SetCurrentValueEmpty();
            }
        }
        protected override void OnResize( EventArgs e )
        {
            base.OnResize( e );

            this.Invalidate();
        }
        protected override void OnSelectedValueChanged( EventArgs e )
        {
            if ( !_SkipOnSelectedValueChanged )
            {
                base.OnSelectedValueChanged( e );
            }
        }


        private DropdownNativeWindow _DropdownNativeWindow;
        protected override void OnDrawItem( DrawItemEventArgs e )
        {
            e.DrawBackground();
            e.DrawFocusRectangle();

            var itemText = this.Items[ e.Index ]?.ToString();
            if ( !itemText.IsNullOrEmpty() )
            {                
                var gr = e.Graphics;

                #region [.init DropdownNativeWindow.]
                var hDC = gr.GetHdc();
                var hWnd = WinApi.WindowFromDC( hDC ); Debug.Assert( hWnd != IntPtr.Zero );
                gr.ReleaseHdc( hDC );

                if ( _DropdownNativeWindow == null )
                {
                    _DropdownNativeWindow = DropdownNativeWindow._AssignHandle_( this, hWnd, _Resources_.undo, this.Items.Count );
                }
                else if ( _DropdownNativeWindow.Handle != hWnd )
                {
                    _DropdownNativeWindow.ReleaseHandle();
                    _DropdownNativeWindow.AssignHandle( hWnd );
                }
                #endregion

                var itemBounds      = e.Bounds;
                var isMarkAsDeleted = _DropdownNativeWindow.IsMarkAsDeleted( e.Index );
                var foreColor       = isMarkAsDeleted ? (e.State.HasFlag( DrawItemState.Selected ) ? ControlPaint.Dark( e.ForeColor, 0.001f ) : Color.Silver) : e.ForeColor;
                PaintDropdownItem( gr, foreColor, e.Font, itemBounds, isMarkAsDeleted, itemText );
                #region comm.
                /*
                using ( var br   = new SolidBrush( isMarkAsDeleted ? ControlPaint.Dark( e.ForeColor, 0.001f ) : e.ForeColor ) )
                using ( var font = new Font( e.Font, isMarkAsDeleted ? FontStyle.Strikeout : FontStyle.Regular ) )
                {
                    gr.DrawString( itemText, font, br, itemBounds );
                }
                */ 
                #endregion

                if ( e.State.HasFlag( DrawItemState.Selected ) )
                {
                    _DropdownNativeWindow.DrawClearButton( itemBounds, e.Index, e.ForeColor, e.BackColor, e.Font );
                }
                else
                {
                    WinApi.InvalidateRect( hWnd, itemBounds );
                }
            }
        }
        private void PaintDropdownItem( Graphics gr, Color foreColor, Font font, in Rectangle itemBounds, bool isMarkAsDeleted, int itemIndex )
                  => PaintDropdownItem( gr, foreColor, font, itemBounds, isMarkAsDeleted, this.Items[ itemIndex ]?.ToString() );
        private void PaintDropdownItem( Graphics gr, Color foreColor, Font font, in Rectangle itemBounds, bool isMarkAsDeleted, string itemText )
        {
            using ( var br    = new SolidBrush( foreColor /*isMarkAsDeleted ? ControlPaint.Dark( foreColor, 0.001f ) : foreColor*/ ) )
            using ( var nfont = new Font( font, isMarkAsDeleted ? FontStyle.Strikeout : FontStyle.Regular ) )
            {
                gr.DrawString( itemText, nfont, br, itemBounds );
            }
        }
        protected override void OnDropDown( EventArgs e )
        {
            base.OnDropDown( e );
            _DropdownNativeWindow?.SetItemsCount( this.Items.Count );
        }
        protected override void OnDropDownClosed( EventArgs e )
        {
            base.OnDropDownClosed( e );
            if ( _DropdownNativeWindow != null )
            {
                _DropdownNativeWindow.ResetItemBounds();
                
                var removedItemIndecies = _DropdownNativeWindow.GetMarkAsDeletedItemIndexes().ToList();
                if ( removedItemIndecies.Any() )
                {
                    var selectedText = this.Text;

                    foreach ( var itemIndex in removedItemIndecies )
                    {
                        var itemText = this.Items[ itemIndex ]?.ToString();
                        Remove( itemText );

                        Debug.WriteLine( $"RemoveItemFromDropdownList: {itemIndex} => '{itemText}'" );
                    }

                    this.Text = selectedText;
                }
            }
        }
        //private void RemoveItemFromDropdownList( int itemIndex )
        //{
        //    var selectedText = this.Text;
        //    var itemText = this.Items[ itemIndex ]?.ToString();
        //    Remove( itemText );
        //    this.Text = selectedText;
        //
        //    Debug.WriteLine( $"RemoveItemFromDropdownList: {itemIndex} => '{itemText}'" );
        //}

        #region [.DropdownNativeWindow.]
        /// <summary>
        /// 
        /// </summary>
        private sealed class DropdownNativeWindow : NativeWindow
        {
            private ComboBoxEx _Parent;
            public DropdownNativeWindow( ComboBoxEx parent ) => _Parent  = parent;
            public static DropdownNativeWindow _AssignHandle_( ComboBoxEx parent, IntPtr hWnd, Image undoImg, int itemCount )
            {
                var nw = new DropdownNativeWindow( parent ) { _UndoImg = undoImg };
                nw.AssignHandle( hWnd );
                nw.SetItemsCount( itemCount );
                return (nw);
            }

            private Rectangle _ItemBounds;
            private int       _ItemIndex;
            private bool[]    _ItemIndex2DelState;
            private Color     _ForeColor;
            private Color     _BackColor;
            private Font      _Font;
            private Image     _UndoImg;
            public void DrawClearButton( in Rectangle itemBounds, int itemIndex, Color foreColor, Color backColor, Font font )
            {
                _ItemBounds = itemBounds;
                _ItemIndex  = itemIndex;
                _ForeColor  = foreColor;
                _BackColor  = backColor;
                _Font       = font;
                DrawClearButton( this.Handle );
            }
            public void ResetItemBounds() => _ItemBounds = Rectangle.Empty;
            public void SetItemsCount( int itemCount ) => _ItemIndex2DelState = new bool[ itemCount ];
            public bool IsMarkAsDeleted( int itemIndex ) => _ItemIndex2DelState[ itemIndex ];
            public IEnumerable< int > GetMarkAsDeletedItemIndexes()
            {
                for ( var i = _ItemIndex2DelState.Length - 1; 0 <= i; i-- )
                {
                    if ( _ItemIndex2DelState[ i ] )
                    {
                        yield return (i);
                    }
                }
            }

            protected override void WndProc( ref Message m )
            {
                int x, y;
                RectangleF rc;
                switch ( m.Msg )
                {
                    case WinApi.WM_PAINT:
                        base.WndProc( ref m );
                        DrawClearButton( m.HWnd );
                        m.Result = IntPtr.Zero;
                        return;

                    case WinApi.WM_MOUSEMOVE:
                        {                            
                            (x, y) = WinApi.GET_XY_LPARAM( m.LParam );
                            rc = GetClearButtonRect();

                            var inClearButton = rc.Contains( x, y );
                            if ( inClearButton != _LastMouseMove_InClearButton )
                            {
                                _LastMouseMove_InClearButton = inClearButton;
                                DrawClearButton( m.HWnd, rc, mouseCursorInButton: inClearButton );
                            }
                            if ( inClearButton )
                            {
                                WinApi.SetCursor( Cursors.Arrow.Handle );
                            }
                        }
                        break;

                    case WinApi.WM_LBUTTONDOWN:
                        {                            
                            (x, y) = WinApi.GET_XY_LPARAM( m.LParam );
                            rc = GetClearButtonRect();

                            _IsPushed_ClearButton = rc.Contains( x, y );
                            if ( _IsPushed_ClearButton )
                            {
                                if ( !_Parent.Focused ) _Parent.Focus();

                                base.WndProc( ref m );

                                DrawClearButton( m.HWnd, rc, mouseCursorInButton: true );
                                m.Result = IntPtr.Zero;
                                return;
                            }
                        }
                        break;

                    case WinApi.WM_LBUTTONUP:
                        if ( _IsPushed_ClearButton)
                        {
                            _IsPushed_ClearButton = false;

                            (x, y) = WinApi.GET_XY_LPARAM( m.LParam );
                            rc = GetClearButtonRect();

                            var inClearButton = rc.Contains( x, y );
                            if ( inClearButton )
                            {
                                //_Parent._SkipOnSelectedValueChanged = true;
                                //{
                                    //base.WndProc( ref m );                                    
                                    var isMarkAsDeleted = (_ItemIndex2DelState[ _ItemIndex ] = !_ItemIndex2DelState[ _ItemIndex ]);
                                    Debug.WriteLine( $"WM_LBUTTONUP, is_deleted: {isMarkAsDeleted}" );
                                    //------_Parent.DroppedDown = false;
                                    //------_Parent.RemoveItemFromDropdownList( _ItemIndex );                                    
                                //}
                                //_Parent._SkipOnSelectedValueChanged = false;
                                
                                using ( var gr    = Graphics.FromHwnd( m.HWnd ) )
                                using ( var brush = new SolidBrush( _BackColor ) )
                                {
                                    gr.FillRectangle( brush, _ItemBounds );
                                    var foreColor = isMarkAsDeleted ? ControlPaint.Dark( _ForeColor, 0.001f ) : _ForeColor;
                                    _Parent.PaintDropdownItem( gr, foreColor, _Font, _ItemBounds, isMarkAsDeleted, _ItemIndex );
                                    DrawClearButton( gr, rc, mouseCursorInButton: true );
                                }

                                m.Result = IntPtr.Zero;
                                return;
                            }
                        }
                        break;
                }
                base.WndProc( ref m );
            }

            private bool _LastMouseMove_InClearButton;
            private bool _IsPushed_ClearButton;
            private void DrawClearButton( IntPtr hWnd, bool eraseBackground = true )
            {                
                var pt = WinApi.GetMousePos( hWnd, Control.MousePosition );
                var rc = GetClearButtonRect();
                var mouseCursorInButton = rc.Contains( pt.x, pt.y ); 

                DrawClearButton( hWnd, rc, mouseCursorInButton, eraseBackground );
            }
            private void DrawClearButton( IntPtr hWnd, in RectangleF buttonRect, bool mouseCursorInButton, bool eraseBackground = true )
            {
                using ( var gr = Graphics.FromHwnd( hWnd ) )
                {
                    DrawClearButton( gr, buttonRect, mouseCursorInButton, eraseBackground );
                }
            }
            private void DrawClearButton( Graphics gr, RectangleF buttonRect, bool mouseCursorInButton, bool eraseBackground = true )
            {
                _LastMouseMove_InClearButton = mouseCursorInButton;
                if ( _LastMouseMove_InClearButton && _IsPushed_ClearButton ) buttonRect.Offset( 1, 1 );

                if ( eraseBackground ) EraseClearButton( gr, buttonRect );
                if ( _ItemIndex < 0 || _ItemIndex2DelState.Length <= _ItemIndex ) return;

                //var brush = _LastMouseMove_InClearButton ? Brushes.Black : Brushes.Silver;
                using ( var brush = new SolidBrush( _LastMouseMove_InClearButton ? _ForeColor : ControlPaint.Dark( _ForeColor, 0.001f ) ) )
                using ( var pen = new Pen( brush, 2 ) )
                {
                    var is_deleted = _ItemIndex2DelState[ _ItemIndex ];
                    if ( is_deleted )
                    {
                        //gr.DrawArc( pen, buttonRect, 60, -200 );
                        buttonRect.Inflate( 1, 1 );
                        gr.FillRectangle( Brushes.White, buttonRect );
                        gr.DrawImage( _UndoImg, buttonRect );
                    }
                    else
                    {
                        buttonRect.Inflate( -1, -1 );
                        gr.DrawLine( pen, buttonRect.X, buttonRect.Y, buttonRect.Right, buttonRect.Bottom );
                        gr.DrawLine( pen, buttonRect.X, buttonRect.Bottom, buttonRect.Right, buttonRect.Y );
                    }
                }
            }
            private void EraseClearButton( Graphics gr, RectangleF buttonRect )
            {
                buttonRect.Inflate( 2, 2 );
                using var brush = new SolidBrush( _BackColor );
                {
                    gr.FillRectangle( brush, buttonRect );
                }
            }
            private RectangleF GetClearButtonRect()
            {
                const int PAD = 3; //4;
                return (new RectangleF( _ItemBounds.Left + (_ItemBounds.Width - _ItemBounds.Height + PAD), _ItemBounds.Top + PAD, _ItemBounds.Height - 2*PAD, _ItemBounds.Height - 2*PAD ));
            }
        }
        #endregion

        [DefaultValue(false), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool   SkipAddToList   { get; set; }
        public string PlaceHolderText { get; set; }
        public bool   DrawClearButton { get; set; } = true;

        public void   Refill( bool selectFirstItem = false )
        {
            _SkipOnSelectedValueChanged = true;
            {
                this.Items.Clear();

                CutToMaxCount();

                foreach ( var val in _ItemsValues.Cast< string >() )
                {
                    //if ( !val.IsNullOrWhiteSpace() )
                    this.Items.Add( val );
                }
                if ( selectFirstItem && (0 < this.Items.Count) )
                {
                    this.SelectedIndex = 0;
                }
            }
            _SkipOnSelectedValueChanged = false;
        }
        public void   Refill( IEnumerable< string > strings )
        {
            _ItemsValues.Clear();
            if ( strings != null )
            {
                foreach ( var val in strings )
                {
                    _ItemsValues.Add( val );
                }
            }
            Refill();
        }
        public void   AddToHead( string val, bool raiseOnSelectedValueChanged = false )
        {
            if ( SkipAddToList || val.IsNullOrWhiteSpace() )
            {
                return;
            }

            _SkipOnSelectedValueChanged = true;
            {
                if ( _ItemsValues.Contains( val ) )
                {
                    _ItemsValues.Remove( val );
                }
                _ItemsValues.Insert( 0, val );

                var idx = this.Items.IndexOf( val );
                if ( idx != -1 )
                {
                    this.Items.RemoveAt( idx );
                }
                this.Items.Insert( 0, val );
                this.SelectedIndex = 0;
            }
            _SkipOnSelectedValueChanged = false;

            if ( raiseOnSelectedValueChanged )
            {
                base.OnSelectedValueChanged( EventArgs.Empty );
            }
        }
        public void   Remove( string val )
        {
            if ( val.IsNullOrWhiteSpace() )
            {
                return;
            }

            _SkipOnSelectedValueChanged = true;
            {
                if ( _ItemsValues.Contains( val ) )
                {
                    _ItemsValues.Remove( val );
                }

                var idx = this.Items.IndexOf( val );
                if ( idx != -1 )
                {
                    this.Items.RemoveAt( idx );
                }
            }
            _SkipOnSelectedValueChanged = false;
        }
        public void   Remove( IEnumerable< string > vals )
        {
            foreach ( var val in vals )
            {
                Remove( val );
            }
        }
        public string GetHeadValue() => ((0 < this.Items.Count) ? ((string) this.Items[ 0 ]) : null);
        public string GetCurrentValue() => this.Text.Trim();
        public void   SetCurrentValue( string val ) => this.Text = val?.Trim();
        public void   SetCurrentValue( IEnumerable< string > vals, string separator ) => this.Text = string.Join( separator, vals );
        public void   SetCurrentValueEmpty() { this.SelectedIndex = -1; this.Text = string.Empty; }
        public void   Clear()
        {
            this.Text = null;
            ClearButtonClick?.Invoke( this, EventArgs.Empty );
        }
        //public (int cnt_1, int cnt_2) GetCounts() => (_ItemsValues.Count, this.Items.Count);

        public void WTF( StringCollection sc_4_fill, Func< StringCollection > set_sc_IfNullAction )
        {
            if ( sc_4_fill == null ) sc_4_fill = set_sc_IfNullAction();

            var cnt_1 = _ItemsValues.Count;
            //(int cnt_1, int cnt_2) = (_ItemsValues.Count, this.Items.Count);
            var cnt_0 = sc_4_fill.Count;
            if (/* cnt_1 != cnt_2 ||*/ cnt_0 != cnt_1 )
            {
                sc_4_fill.Clear();
                foreach ( var val in _ItemsValues.Cast< string >() )
                {
                    sc_4_fill.Add( val );
                }
            }
        }

        private void CutToMaxCount()
        {
            while ( _ItemsValuesMaxCount < _ItemsValues.Count )
            {
                _ItemsValues.RemoveAt( _ItemsValues.Count - 1 );
            }
            
            while ( _ItemsValuesMaxCount < this.Items.Count )
            {
                this.Items.RemoveAt( this.Items.Count - 1 );
            }            
        }
        private void ContextMenu_Opening( object sender, CancelEventArgs e ) => e.Cancel = (this.Items.Count == 0);

        protected virtual void Before_ClearMenuItemClick() { }
        private void SortMenuItem_Click( object sender, EventArgs e )
        {
            Before_ClearMenuItemClick();

            var lst = new List< string >( _ItemsValues.Count );
            foreach ( var s in _ItemsValues.Cast< string >() )
            {
                lst.Add( s );
            }
            lst.Sort( (x, y) => string.Compare( x, y, StringComparison.InvariantCultureIgnoreCase ) );
            Refill( lst );
        }
        private void ClearMenuItem_Click( object sender, EventArgs e )
        {
            Before_ClearMenuItemClick();

            var val = GetCurrentValue();// GetHeadValue();

            _SkipOnSelectedValueChanged = true;
            {
                _ItemsValues.Clear();
                this.Items.Clear();
            }
            _SkipOnSelectedValueChanged = false;

            AddToHead( val );

            if ( Fill_ItemsValues( _ItemsValues ) )
            {
                Refill();
                SetCurrentValueEmpty();
            }

            OnClearMenuItemClick?.Invoke( sender, e );
        }
    }
}
