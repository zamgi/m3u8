using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class SHBrowser : CommonDialog
    {
        /// <summary>
        /// 
        /// </summary>
        [Flags] public enum BrowseInfoFlag : uint
        {
            // Browsing for directory.
            BIF_RETURNONLYFSDIRS =  0x0001,  // For finding a folder to start document searching
            BIF_DONTGOBELOWDOMAIN = 0x0002,  // For starting the Find Computer
            BIF_STATUSTEXT        = 0x0004,   // Top of the dialog has 2 lines of text for BROWSEINFO.lpszTitle and one line if
                                        // this flag is set.  Passing the message BFFM_SETSTATUSTEXTA to the hwnd can set the
                                        // rest of the text.  This is not used with BIF_USENEWUI and BROWSEINFO.lpszTitle gets
                                        // all three lines of text.
            BIF_RETURNFSANCESTORS = 0x0008,
            BIF_EDITBOX           = 0x0010,   // Add an editbox to the dialog
            BIF_VALIDATE          = 0x0020,   // insist on valid result (or CANCEL)

            BIF_NEWDIALOGSTYLE    = 0x0040,   // Use the new dialog layout with the ability to resize
                                        // Caller needs to call OleInitialize() before using this API

            BIF_USENEWUI          = (BIF_NEWDIALOGSTYLE | BIF_EDITBOX),

            BIF_BROWSEINCLUDEURLS = 0x0080,   // Allow URLs to be displayed or entered. (Requires BIF_USENEWUI)
            BIF_UAHINT            = 0x0100,   // Add a UA hint to the dialog, in place of the edit box. May not be combined with BIF_EDITBOX
            BIF_NONEWFOLDERBUTTON = 0x0200,   // Do not add the "New Folder" button to the dialog.  Only applicable with BIF_NEWDIALOGSTYLE.
            BIF_NOTRANSLATETARGETS= 0x0400,   // don't traverse target as shortcut

            BIF_BROWSEFORCOMPUTER = 0x1000,  // Browsing for Computers.
            BIF_BROWSEFORPRINTER  = 0x2000,  // Browsing for Printers
            BIF_BROWSEINCLUDEFILES= 0x4000,  // Browsing for Everything
            BIF_SHAREABLE         = 0x8000  // sharable resources displayed (remote shares, requires BIF_USENEWUI)
        }

        /// <summary>
        /// 
        /// </summary>
        private static class WinApi
        {
            /// <summary>
            /// 
            /// </summary>
            [Flags] public enum SFGAO : uint
            {
                SFGAO_CANCOPY = 0x00000001,
                SFGAO_CANMOVE = 0x00000002,
                SFGAO_CANLINK = 0x00000004,
                SFGAO_STORAGE = 0x00000008,
                SFGAO_CANRENAME = 0x00000010,
                SFGAO_CANDELETE = 0x00000020,
                SFGAO_HASPROPSHEET = 0x00000040,
                SFGAO_DROPTARGET = 0x00000100,
                SFGAO_CAPABILITYMASK = 0x00000177,
                SFGAO_SYSTEM = 0x00001000,
                SFGAO_ENCRYPTED = 0x00002000,
                SFGAO_ISSLOW = 0x00004000,
                SFGAO_GHOSTED = 0x00008000,
                SFGAO_LINK = 0x00010000,
                SFGAO_SHARE = 0x00020000,
                SFGAO_READONLY = 0x00040000,
                SFGAO_HIDDEN = 0x00080000,
                SFGAO_DISPLAYATTRMASK = 0x000FC000,
                SFGAO_NONENUMERATED = 0x00100000,
                SFGAO_NEWCONTENT = 0x00200000,
                //SFGAO_CANMONIKER = ,
                //SFGAO_HASSTORAGE = ,
                SFGAO_STREAM = 0x00400000,
                SFGAO_STORAGEANCESTOR = 0x00800000,
                SFGAO_VALIDATE = 0x01000000,
                SFGAO_REMOVABLE = 0x02000000,
                SFGAO_COMPRESSED = 0x04000000,
                SFGAO_BROWSABLE = 0x08000000,
                SFGAO_FILESYSANCESTOR = 0x10000000,
                SFGAO_FOLDER = 0x20000000,
                SFGAO_FILESYSTEM = 0x40000000,
                SFGAO_STORAGECAPMASK = 0x70C50008,
                SFGAO_HASSUBFOLDER = 0x80000000,
                SFGAO_CONTENTSMASK = 0x80000000,
                SFGAO_PKEYSFGAOMASK = 0x81044000,
            }

            private const string SHELL32_DLL = "shell32.dll";
            [DllImport(SHELL32_DLL, CharSet=CharSet.Auto)] public static extern IntPtr SHBrowseForFolder( [In] BROWSEINFO lpbi );
            [DllImport(SHELL32_DLL, CharSet=CharSet.Auto)] private static extern bool SHGetPathFromIDList( IntPtr pidl, IntPtr pszPath );
            public static bool SHGetPathFromIDList( IntPtr pidl, out string path )
            {
                var pszPath = Marshal.AllocHGlobal( MAX_PATH * Marshal.SystemDefaultCharSize );
                try
                {
                    var success = SHGetPathFromIDList( pidl, pszPath );
                    if ( success )
                    {
                        path = Marshal.PtrToStringAuto( pszPath, MAX_PATH * Marshal.SystemDefaultCharSize );
                        var idx = path.IndexOf( '\0' );
                        if ( idx != -1 )
                        {
                            path = path.Substring( 0, idx );
                        }
                        return (true);
                    }
                    else
                    {
                        path = null;
                        return (false);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal( pszPath );
                }     
            }

            [DllImport(SHELL32_DLL)] public static extern int SHGetSpecialFolderLocation( IntPtr hwnd, int csidl, ref IntPtr ppidl );
            [DllImport(SHELL32_DLL)] public static extern int SHParseDisplayName( [MarshalAs(UnmanagedType.LPWStr)] string pszName, IntPtr pbc, ref IntPtr ppidl, SFGAO sfgaoIn, out SFGAO psfgaoOut );
            [DllImport("shlwapi.dll", CharSet=CharSet.Unicode, EntryPoint="PathMatchSpecW")][return:MarshalAs(UnmanagedType.Bool)] public static extern bool IsPathMatch( string pszFile, string pszSpec );

            private const int WM_USER  = 0x0400;
            public  const int MAX_PATH = 260;

            /// <summary>
            /// message from browser
            /// </summary>
            public static class BFFM
            {
                public const int BFFM_INITIALIZED       = 1;
                public const int BFFM_SELCHANGED        = 2;
                public const int BFFM_VALIDATEFAILEDA   = 3;   // lParam:szPath ret:1(cont),0(EndDialog)
                public const int BFFM_VALIDATEFAILEDW   = 4;   // lParam:wzPath ret:1(cont),0(EndDialog)
                public const int BFFM_IUNKNOWN          = 5;   // provides IUnknown to client. lParam: IUnknown*

                // messages to browser 2
                public const int BFFM_SETSTATUSTEXTA    = (WM_USER + 100);
                public const int BFFM_ENABLEOK          = (WM_USER + 101);
                public const int BFFM_SETSELECTIONA     = (WM_USER + 102);
                public const int BFFM_SETSELECTIONW     = (WM_USER + 103);
                public const int BFFM_SETSTATUSTEXTW    = (WM_USER + 104);
                public const int BFFM_SETOKTEXT         = (WM_USER + 105); // Unicode only
                public const int BFFM_SETEXPANDED       = (WM_USER + 106); // Unicode only
            }

            /// <summary>
            /// 
            /// </summary>
            public delegate int BrowseCallbackProc( IntPtr hwnd, int msg, IntPtr lParam, IntPtr lpData );

            /// <summary>
            /// 
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
            public sealed class BROWSEINFO
            {
                public IntPtr             hwndOwner;
                public IntPtr             pidlRoot;
                public IntPtr             pszDisplayName;
                public string             lpszTitle;
                public int                ulFlags;
                public BrowseCallbackProc lpfn;
                public IntPtr             lParam;
                public int                iImage;
            }

            private const string USER32_DLL = "user32.dll";
            [DllImport(USER32_DLL, CharSet=CharSet.Auto)] public static extern IntPtr SendMessage( HandleRef hWnd, int msg, int wParam, int lParam );
            [DllImport(USER32_DLL, CharSet=CharSet.Auto)] public static extern IntPtr SendMessage( HandleRef hWnd, int msg, int wParam, string lParam );
            [DllImport(USER32_DLL, SetLastError=true, CharSet=CharSet.Auto)][return:MarshalAs(UnmanagedType.Bool)] public static extern bool SetWindowText( IntPtr hwnd, string lpString );

            private const string OLE32_DLL = "ole32.dll";
            [DllImport(OLE32_DLL)] private static extern IntPtr CoTaskMemAlloc( long cb );
            [DllImport(OLE32_DLL)] private static extern void CoTaskMemFree( IntPtr pv );

            public static void CoTaskMemFree_IfNotZero( IntPtr pv )
            {
                if ( pv != IntPtr.Zero ) CoTaskMemFree( pv );
            }
            public static void CoTaskMemFree_IfNotZero( ref IntPtr pv )
            {
                if ( pv != IntPtr.Zero ) { CoTaskMemFree( pv ); pv = IntPtr.Zero; }
            }
        }

        #region comm.
        //public new event EventHandler HelpRequest
        //{
        //    add    => base.HelpRequest += value;
        //    remove => base.HelpRequest -= value;
        //} 
        #endregion

        #region [.field's.]
        private string                    _DescriptionText;
        private string                    _SelectedPath;        
        private bool                      _SelectOnlyFiles;
        private Environment.SpecialFolder _RootFolder;
        private string                    _RootFolderPath;
        //private bool                      _SelectedPathNeedsCheck;
        #endregion

        #region [.ctor().]
        public SHBrowser()
        {
            Flags = BrowseInfoFlag.BIF_USENEWUI;
            Reset();
        } 
        #endregion

        #region [.prop's.]
        public string Description
        {
            get => _DescriptionText;
            set => _DescriptionText = value ?? string.Empty;
        }
        public string DialogTitle { get; set; }
        public Environment.SpecialFolder? RootFolder
        {
            get => ((_RootFolderPath == null) ? _RootFolder : (Environment.SpecialFolder?) null);
            set
            {
                if ( !value.HasValue ) throw (new ArgumentNullException());
                
                var t = value.Value;
                if ( !Enum.IsDefined( typeof(Environment.SpecialFolder), t ) )
                {
                    throw (new InvalidEnumArgumentException( nameof(value), (int) t, typeof(Environment.SpecialFolder) ));
                }
                _RootFolder     = t;
                _RootFolderPath = null;
            }
        }
        public string RootFolderPath
        {
            get => (_RootFolderPath ?? Environment.GetFolderPath( _RootFolder ));
            set => _RootFolderPath = value;
        }
        public string SelectedPath
        {
            get
            {
                #region comm.
                //if ( !_SelectedPath.IsNullOrEmpty() && _SelectedPathNeedsCheck )
                //{
                //    /*
                //    var di = new DirectoryInfo( _SelectedPath );
                //    var ac = di.GetAccessControl( AccessControlSections.Access );
                //    ac.AddAccessRule( new FileSystemAccessRule( new SecurityIdentifier( WellKnownSidType.WorldSid, null ), 
                //                                                FileSystemRights.ListDirectory, 
                //                                                InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, 
                //                                                PropagationFlags.NoPropagateInherit, 
                //                                                AccessControlType.Allow ) );
                //    di.SetAccessControl( ac );
                //    //*/

                //    #region comm. prev.
                //    //(new FileIOPermission( FileIOPermissionAccess.PathDiscovery, _SelectedPath )).Demand();
                //    #endregion
                //}
                #endregion
                return (_SelectedPath);
            }
            set
            {
                _SelectedPath = value ?? string.Empty;
                //_SelectedPathNeedsCheck = false;
            }
        }
        public BrowseInfoFlag Flags { get; set; }
        public bool ShowFiles
        {
            get => ((Flags & BrowseInfoFlag.BIF_BROWSEINCLUDEFILES) == BrowseInfoFlag.BIF_BROWSEINCLUDEFILES);
            set
            {
                if ( value )
                {
                    Flags |= BrowseInfoFlag.BIF_BROWSEINCLUDEFILES;
                }
                else if ( ShowFiles )
                {
                    Flags ^= BrowseInfoFlag.BIF_BROWSEINCLUDEFILES;
                }
            }
        }
        public bool SelectOnlyFiles
        {
            get => (_SelectOnlyFiles & ShowFiles);
            set
            {
                if ( value )
                {
                    _SelectOnlyFiles = true;
                    Flags |= BrowseInfoFlag.BIF_BROWSEINCLUDEFILES;
                }
                else
                {
                    _SelectOnlyFiles = false;
                }
            }
        }
        public string SearchPattern { get; set; }
        #endregion

        private bool CheckSelectedPath( IntPtr pidl ) => CheckSelectedPath( pidl, out var _ );
        private bool CheckSelectedPath( IntPtr pidl, out string path )
        {
            var success = WinApi.SHGetPathFromIDList( pidl, out path );
            if ( success && this.SelectOnlyFiles && this.ShowFiles )
            {
                success &= File.Exists( path );
            }
            if ( success && (this.SearchPattern != null) )
            {
                success &= WinApi.IsPathMatch( path, this.SearchPattern );
            }
            if ( success && (this._RootFolderPath != null) )
            {
                success &= path.StartsWith( this._RootFolderPath, StringComparison.InvariantCultureIgnoreCase );
            }
            return (success);
        }
        private int FolderBrowserDialog_BrowseCallbackProc( IntPtr hwnd, int msg, IntPtr lParam, IntPtr lpData )
        {
            switch ( msg )
            {
                case WinApi.BFFM.BFFM_INITIALIZED:
                {
                    if ( !string.IsNullOrEmpty( this.DialogTitle ) )
                    {
                        WinApi.SetWindowText( hwnd, this.DialogTitle );
                    }
                    else if ( this.SelectOnlyFiles )
                    {
                        WinApi.SetWindowText( hwnd, "Select only files" );
                    }

                    if ( !string.IsNullOrEmpty( _SelectedPath ) )
                    {
                        WinApi.SendMessage( new HandleRef( null, hwnd ), WinApi.BFFM.BFFM_SETSELECTIONW, 1, _SelectedPath );                        
                    }
                }
                break;

                case WinApi.BFFM.BFFM_SELCHANGED:
                {
                    IntPtr pidl = lParam;
                    if ( pidl != IntPtr.Zero )
                    {
                        var success = CheckSelectedPath( pidl );
                        
                        WinApi.SendMessage( new HandleRef( null, hwnd ), WinApi.BFFM.BFFM_ENABLEOK, 0, (success ? 1 : 0) );
                    }
                }
                break;
            }
            return (0);
        }

        public override void Reset()
        {
            _RootFolder      = Environment.SpecialFolder.Desktop;
            _DescriptionText = string.Empty;
            _SelectedPath    = string.Empty;
            //_SelectedPathNeedsCheck = false;
        }
        protected override bool RunDialog( IntPtr hWndOwner )
        {
            if ( Control.CheckForIllegalCrossThreadCalls && (Application.OleRequired() != ApartmentState.STA) )
            {
                throw (new ThreadStateException( "Application.OleRequired() != ApartmentState.STA (Thread must be STA)" ));
            }

            #region [.obtain TEMIDLIST for root folder.]            
            var pidlRoot = IntPtr.Zero;
            if ( _RootFolderPath == null )
            {
                WinApi.SHGetSpecialFolderLocation( hWndOwner, (int) _RootFolder, ref pidlRoot );
            }
            else
            {
                WinApi.SHParseDisplayName( _RootFolderPath, IntPtr.Zero, ref pidlRoot, WinApi.SFGAO.SFGAO_FILESYSTEM | WinApi.SFGAO.SFGAO_FOLDER, out var psfgaoOut );
            }
            if ( pidlRoot == IntPtr.Zero )
            {
                WinApi.SHGetSpecialFolderLocation( hWndOwner, 0, ref pidlRoot );
                if ( pidlRoot == IntPtr.Zero )
                {
                    throw (new InvalidOperationException( $"Can't obtain TEMIDLIST for root folder: '{this.RootFolderPath}'." ));
                }
            }
            #endregion
            
            var hglobal = IntPtr.Zero;
            try
            {
                hglobal = Marshal.AllocHGlobal( WinApi.MAX_PATH * Marshal.SystemDefaultCharSize );

                for (; ; )
                {
                    var pidl = IntPtr.Zero;
                    try
                    {                                        
                        var lpbi = new WinApi.BROWSEINFO()
                        {
                            pidlRoot       = pidlRoot,
                            hwndOwner      = hWndOwner,
                            pszDisplayName = hglobal,
                            lpszTitle      = _DescriptionText,
                            ulFlags        = Convert.ToInt32( Flags ),
                            lpfn           = new WinApi.BrowseCallbackProc( FolderBrowserDialog_BrowseCallbackProc ),
                            lParam         = IntPtr.Zero,
                            iImage         = 0,
                        };
                        pidl = WinApi.SHBrowseForFolder( lpbi );
                        if ( pidl != IntPtr.Zero )
                        {
                            var success = CheckSelectedPath( pidl, out var path );                            
                            if ( success )
                            {
                                _SelectedPath = path;
                                //_SelectedPathNeedsCheck = true;
                                return (true);
                            }
                        }
                        else
                        {
                            return (false);
                        }
                    }
                    finally
                    {
                        WinApi.CoTaskMemFree_IfNotZero( pidl );
                    }
                }
            }
            finally
            {
                WinApi.CoTaskMemFree_IfNotZero( pidlRoot );
                if ( hglobal != IntPtr.Zero )
                {
                    Marshal.FreeHGlobal( hglobal );
                }
            }
        }


        public static bool TrySelectPath( IWin32Window owner, string selectedPath, string description, out string outSelectedPath )
        {
            using ( var d = new SHBrowser() { SelectedPath = selectedPath, Description = description } )
            {
                if ( d.ShowDialog( owner ) == DialogResult.OK )
                {
                    outSelectedPath = d.SelectedPath;
                    return (true);
                }
            }
            outSelectedPath = default;
            return (false);
        }
    }
}
