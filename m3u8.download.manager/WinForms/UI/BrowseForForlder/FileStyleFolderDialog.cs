using System.Runtime.InteropServices;
using System.Text;

namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    public static class FileStyleFolderDialog
    {
        /// <summary>
        /// 
        /// </summary>
        private static class WinApi
        {
            /// <summary>
            /// 
            /// </summary>
            [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214E6-0000-0000-C000-000000000046")]
            public interface IShellFolder
            {
                /// <summary>
                /// Translates a file object's or folder's display name into an item identifier list.
                /// </summary>
                [PreserveSig] int ParseDisplayName( IntPtr hwnd, IntPtr pbc, [MarshalAs(UnmanagedType.LPWStr)] string displayName, ref uint pchEaten, out IntPtr pidl, ref SFGAO attributes );

                // Allows a client to determine the contents of a folder by creating an item
                // identifier enumeration object and returning its IEnumIDList interface.
                // Return value: error code, if any
                [PreserveSig] int EnumObjects( IntPtr hwnd, SHCONTF grfFlags, [MarshalAs(UnmanagedType.Interface)] out IEnumIDList ppenumIDList );

                /// <summary>
                /// Retrieves a handler, typically the Shell folder object that implements IShellFolder for a particular item. Optional parameters that control the construction of the handler are passed in the bind context.
                /// </summary>
                /// <param name="pidl">The address of an ITEMIDLIST  structure (PIDL) that identifies the subfolder. This value can refer to an item at any level below the parent folder in the namespace hierarchy. The structure contains one or more SHITEMID structures, followed by a terminating NULL.</param>
                /// <param name="pbc">A pointer to an IBindCtx  interface on a bind context object that can be used to pass parameters to the construction of the handler. If this parameter is not used, set it to NULL. Because support for this parameter is optional for folder object implementations, some folders may not support the use of bind contexts.Information that can be provided in the bind context includes a BIND_OPTS structure that includes a grfMode member that indicates the access mode when binding to a stream handler. Other parameters can be set and discovered using IBindCtx::RegisterObjectParam and IBindCtx::GetObjectParam.</param>
                /// <param name="riid">The identifier of the interface to return. This may be IID_IShellFolder, IID_IStream or any other interface that identifies a particular handler.</param>
                /// <param name="ppv">When this method returns, contains the address of a pointer to the requested interface. If an error occurs, a NULL pointer is returned at this address.</param>
                /// <returns>If the method succeeds, it returns 0. Otherwise, it returns an HRESULT error code.</returns>
                [PreserveSig] int BindToObject( IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv );

                // Requests a pointer to an object's storage interface. 
                // Return value: error code, if any
                [PreserveSig] int BindToStorage( IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv );

                // Determines the relative order of two file objects or folders, given their
                // item identifier lists. Return value: If this method is successful, the
                // CODE field of the HRESULT contains one of the following values (the code
                // can be retrived using the helper function GetHResultCode): Negative A
                // negative return value indicates that the first item should precede
                // the second (pidl1 < pidl2). 

                // Positive A positive return value indicates that the first item should
                // follow the second (pidl1 > pidl2).  Zero A return value of zero
                // indicates that the two items are the same (pidl1 = pidl2). 
                [PreserveSig] int CompareIDs( IntPtr lParam, IntPtr pidl1, IntPtr pidl2 );

                // Requests an object that can be used to obtain information from or interact
                // with a folder object.
                // Return value: error code, if any
                [PreserveSig] int CreateViewObject( IntPtr hwndOwner, Guid riid, out IntPtr ppv );

                // Retrieves the attributes of one or more file objects or subfolders. 
                // Return value: error code, if any
                [PreserveSig] int GetAttributesOf( uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, ref SFGAO rgfInOut );

                // Retrieves an OLE interface that can be used to carry out actions on the
                // specified file objects or folders.
                // Return value: error code, if any
                [PreserveSig] int GetUIObjectOf( IntPtr hwndOwner, uint cidl, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, ref Guid riid, IntPtr rgfReserved, out IntPtr ppv );

                // Retrieves the display name for the specified file object or subfolder. 
                // Return value: error code, if any
                [PreserveSig] int GetDisplayNameOf( IntPtr pidl, SHGDNF uFlags, ref STRRET/*IntPtr*/ lpName );

                // Sets the display name of a file object or subfolder, changing the item
                // identifier in the process.
                // Return value: error code, if any
                [PreserveSig] int SetNameOf( IntPtr hwnd, IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName, SHGDNF uFlags, out IntPtr ppidlOut );
            }

            /// <summary>
            /// 
            /// </summary>
            [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214F2-0000-0000-C000-000000000046")]
            public interface IEnumIDList
            {
                // Retrieves the specified number of item identifiers in the enumeration 
                // sequence and advances the current position by the number of items retrieved
                [PreserveSig] int Next( int celt, out IntPtr rgelt, out int pceltFetched );

                // Skips over the specified number of elements in the enumeration sequence
                [PreserveSig] int Skip( int celt );

                // Returns to the beginning of the enumeration sequence
                [PreserveSig] int Reset();

                // Creates a new item enumeration object with the same contents and state as the current one
                [PreserveSig] int Clone( out IEnumIDList ppenum );
            }

            /// <summary>
            /// 
            /// </summary>
            [Flags] public enum SHCONTF : uint
            {
                CHECKING_FOR_CHILDREN = 0x10,
                FOLDERS               = 0x20,
                NONFOLDERS            = 0x40,
                INCLUDEHIDDEN         = 0x80,
                INIT_ON_FIRST_NEXT    = 0x100,
                NETPRINTERSRCH        = 0x200,
                SHAREABLE             = 0x400,
                STORAGE               = 0x800,
                NAVIGATION_ENUM       = 0x1000,
                FASTITEMS             = 0x2000,
                FLATLIST              = 0x4000,
                ENABLE_ASYNC          = 0x8000,
                INCLUDESUPERHIDDEN    = 0x10000
            }

            /// <summary>
            /// 
            /// </summary>
            [Flags] public enum SHGDNF
            {
                NORMAL        = 0,
                INFOLDER      = 0x1,
                FOREDITING    = 0x1000,
                FORADDRESSBAR = 0x4000,
                FORPARSING    = 0x8000
            };

            /// <summary>
            /// 
            /// </summary>
            [Flags] public enum SFGAO : uint
            {
                CANCOPY = 0x00000001,
                CANMOVE = 0x00000002,
                CANLINK = 0x00000004,
                STORAGE = 0x00000008,
                CANRENAME = 0x00000010,
                CANDELETE = 0x00000020,
                HASPROPSHEET = 0x00000040,
                DROPTARGET = 0x00000100,
                CAPABILITYMASK = 0x00000177,
                SYSTEM = 0x00001000,
                ENCRYPTED = 0x00002000,
                ISSLOW = 0x00004000,
                GHOSTED = 0x00008000,
                LINK = 0x00010000,
                SHARE = 0x00020000,
                READONLY = 0x00040000,
                HIDDEN = 0x00080000,
                DISPLAYATTRMASK = 0x000FC000,
                NONENUMERATED = 0x00100000,
                NEWCONTENT = 0x00200000,
                //CANMONIKER =
                //HASSTORAGE =
                STREAM = 0x00400000,
                STORAGEANCESTOR = 0x00800000,
                VALIDATE = 0x01000000,
                REMOVABLE = 0x02000000,
                COMPRESSED = 0x04000000,
                BROWSABLE = 0x08000000,
                FILESYSANCESTOR = 0x10000000,
                FOLDER = 0x20000000,
                FILESYSTEM = 0x40000000,
                STORAGECAPMASK = 0x70C50008,
                HASSUBFOLDER = 0x80000000,
                CONTENTSMASK = 0x80000000,
                PKEYSFGAOMASK = 0x81044000,
            }

            /// <summary>
            /// 
            /// </summary>
            [StructLayout(LayoutKind.Explicit)]
            unsafe public struct STRRET
            {
                [FieldOffset(0)]
                public uint uType;
                [FieldOffset(4)]
                public IntPtr pOleStr;
                [FieldOffset(4)]
                public IntPtr pStr;
                [FieldOffset(4)]
                public uint uOffset;
                [FieldOffset(4)]
                public fixed char cStr[ MAX_PATH /*260*/ ];
                //public IntPtr cStr;
            }
            private const int MAX_PATH = 260;


            private const string SHELL32_DLL = "shell32.dll";
            [DllImport(SHELL32_DLL)] public static extern int SHGetSpecialFolderLocation( IntPtr hwnd, int csidl, out IntPtr ppidl );
            [DllImport(SHELL32_DLL)] public static extern int SHGetSpecialFolderLocation( IntPtr hwnd, Environment.SpecialFolder specialFolder, out IntPtr ppidl );
            [DllImport(SHELL32_DLL)] private static extern int SHParseDisplayName( [MarshalAs(UnmanagedType.LPWStr)] string pszName, IntPtr pbc, ref IntPtr ppidl, SFGAO sfgaoIn, out SFGAO psfgaoOut );
            [DllImport(SHELL32_DLL)] private static extern int SHCreateItemFromParsingName( [MarshalAs(UnmanagedType.LPWStr)] string pszPath, IntPtr pbc, ref Guid riid, out IShellItem shellItem );
            [DllImport(SHELL32_DLL)] private static extern int SHBindToParent( IntPtr pidl, ref Guid riid, out IShellFolder shellFolder, out IntPtr ppidlLast );
            [DllImport(SHELL32_DLL)] private static extern int SHCreateShellItem( IntPtr pidlParent, IShellFolder psfParent, IntPtr pidl, out IShellItem shellItem );

            public static int SHParseDisplayName( string pszName, ref IntPtr ppidl, SFGAO sfgaoIn ) => SHParseDisplayName( pszName, IntPtr.Zero, ref ppidl, sfgaoIn, out var _ );
            public static int SHCreateShellItem( IShellFolder psfParent, IntPtr pidl, out IShellItem shellItem ) => SHCreateShellItem( IntPtr.Zero, psfParent, pidl, out shellItem );
            public static int SHCreateItemFromParsingName( string pszPath, out IShellItem shellItem ) => SHCreateItemFromParsingName( pszPath, IntPtr.Zero, out shellItem );
            public static int SHCreateItemFromParsingName( string pszPath, IntPtr pbc, out IShellItem shellItem )
            {
                var IShellItem_IIDGuid = Guid.Parse( IIDGuid.IShellItem );
                return SHCreateItemFromParsingName( pszPath, pbc, ref IShellItem_IIDGuid, out shellItem );
            }
            public static int SHBindToParent( IntPtr pidl, out IShellFolder shellFolder, out IntPtr ppidlLast )
            {
                var IShellFolder_IIDGuid = Guid.Parse( IIDGuid.IShellFolder );
                return SHBindToParent( pidl, ref IShellFolder_IIDGuid, out shellFolder, out ppidlLast );
            }

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

        public static bool TryShowFileStyleFolderDialog( this IWin32Window hWnd, string selectedPath, string descr, out string outSelectedPath )
        {
            var t =  Show( hWnd.Handle, selectedPath, descr );
            if ( !t.suc || t.isMyComputer )
            {
                outSelectedPath = default;
                return (false);
            }
            outSelectedPath = t.selectedPath; 
            return (true);
        }
        public static (bool suc, bool isMyComputer, string selectedPath) ShowFileStyleFolderDialog( this IWin32Window hWnd, string selectedPath, string descr ) => Show( hWnd.Handle, selectedPath, descr );
        public static (bool suc, bool isMyComputer, string selectedPath) Show( IWin32Window hWnd, string selectedPath, string descr ) => Show( hWnd.Handle, selectedPath, descr );
        public static (bool suc, bool isMyComputer, string selectedPath) Show( IntPtr hWnd, string selectedPath, string descr )
        {
            var dialog = default(IFileDialog);
            try
            {
                dialog = new FileOpenDialogImpl();

                dialog.SetOptions( FOS.PICKFOLDERS | FOS.FORCEFILESYSTEM /*| FOS.ALLNONSTORAGEITEMS*/ );
                if ( !string.IsNullOrEmpty( descr ) ) dialog.SetTitle( descr );

                if ( WinApi.SHCreateItemFromParsingName( selectedPath, out var ppv ) == 0 )
                {
                    dialog.SetFolder( ppv );
                    Marshal.ReleaseComObject( ppv );
                }

                var result = (suc: false, isMyComputer: false, selectedPath: default(string));
                var res = dialog.Show( hWnd );
                if ( res == 0 )
                {
                    if ( dialog.GetResult( out var ppsi ) == 0 )
                    {
                        result.selectedPath = ppsi.GetDisplayName( SIGDN.FILESYSPATH );
                        if ( result.selectedPath == null )
                        {
                            var suc = WinApi.SHGetSpecialFolderLocation( hWnd, Environment.SpecialFolder.MyComputer, out var pidl );
                            if ( suc == 0 )
                            {
                                suc = WinApi.SHBindToParent( pidl, out var shellFolder_4_MyComputer, out var child );
                                if ( suc == 0 )
                                {
                                    suc = WinApi.SHCreateShellItem( shellFolder_4_MyComputer, child, out var shellItem_4_MyComputer );
                                    if ( suc == 0 )
                                    {
                                        var comp_res = ppsi.Compare( shellItem_4_MyComputer, SICHINTF.CANONICAL, out var piOrder );
                                        if ( comp_res == 0 )
                                        {
                                            result.suc          = true;
                                            result.isMyComputer = true;
                                        }
                                        Marshal.ReleaseComObject( shellItem_4_MyComputer );
                                    }
                                }
                                Marshal.ReleaseComObject( shellFolder_4_MyComputer );
                                WinApi.CoTaskMemFree_IfNotZero( pidl );
                            }
                        }
                        else
                        {
                            result.suc = true;
                        }
                        Marshal.ReleaseComObject( ppsi );
                    }
                }
                return (result);
            }
            finally
            {
                if ( dialog != null ) Marshal.FinalReleaseComObject( dialog );
            }
        }

        private static string GetDisplayName( this IShellItem shellItem, SIGDN sigdn )
        {
            var suc = shellItem.GetDisplayName( sigdn, out var hglobal );
            var displayName = (suc == 0) ? Marshal.PtrToStringAuto( hglobal ) : null;
            WinApi.CoTaskMemFree_IfNotZero( hglobal );
            return (displayName);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class IIDGuid
    {
        public const string IModalWindow    = "b4db1657-70d7-485e-8e3e-6fcb5a5c1802";
        public const string IFileDialog     = "42f85136-db7e-439c-85f1-e4075d135fc8";
        public const string IFileOpenDialog = "d57c7288-d4ad-4768-be02-9d969532d960";
        public const string IShellItem      = "43826d1e-e718-42ee-bc55-a1e261c37bfe";
        public const string IShellFolder    = "000214E6-0000-0000-C000-000000000046";
    }
    /// <summary>
    /// 
    /// </summary>
    internal static class CLSIDGuid
    {
        public const string FileOpenDialog = "DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7";
    }

    /// <summary>
    /// 
    /// </summary>
    [ComImport, Guid(IIDGuid.IModalWindow), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IModalWindow
    {
        [PreserveSig] int Show([In] IntPtr parent);
    }

    /// <summary>
    /// 
    /// </summary>
    [ComImport, Guid(IIDGuid.IFileDialog), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFileDialog : IModalWindow
    {
        // Defined on IModalWindow - repeated here due to requirements of COM interop layer
        // --------------------------------------------------------------------------------
        [PreserveSig] int Show([In] IntPtr parent);

        [PreserveSig] int SetFileTypes( [In] uint cFileTypes,/* [size_is][in] */ /*COMDLG_FILTERSPEC* rgFilterSpec*/ IntPtr rgFilterSpec );
        [PreserveSig] int SetFileTypeIndex( [In] uint iFileType );
        [PreserveSig] int GetFileTypeIndex( [Out] out uint piFileType);
        [PreserveSig] int Advise( [In] /*IFileDialogEvents *pfde*/IntPtr pfde, [Out] /*DWORD* */out uint pdwCookie);
        [PreserveSig] int Unadvise( [In] /*DWORD*/uint dwCookie );
        [PreserveSig] int SetOptions([In] FOS fos);
        [PreserveSig] int GetOptions( [Out] /*FILEOPENDIALOGOPTIONS *pfos*/IntPtr pfos );
        [PreserveSig] int SetDefaultFolder( [In] /*IShellItem *psi*/IntPtr psi );
        [PreserveSig] int SetFolder([In] /*IShellItem *psi*//*IntPtr*/IShellItem psi );
        [PreserveSig] int GetFolder([Out]/*IShellItem **ppsi*/out IShellItem/*IntPtr*/ ppsi );
        [PreserveSig] int GetCurrentSelection([Out] /*IShellItem **ppsi*/out IntPtr ppsi);
        [PreserveSig] int SetFileName(/* [string][in] */ [In][MarshalAs(UnmanagedType.LPWStr)] string pszName);
        [PreserveSig] int GetFileName(/* [string][out] */ /*LPWSTR *pszName*/[Out] StringBuilder pszName/*[MarshalAs(UnmanagedType.LPWStr)] out string pszName*/);
        [PreserveSig] int SetTitle(/* [string][in] */ [MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
        [PreserveSig] int SetOkButtonLabel(/* [string][in] */ [MarshalAs(UnmanagedType.LPWStr)] string pszText);
        [PreserveSig] int SetFileNameLabel(/* [string][in] */ [MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
        [PreserveSig] int GetResult([Out] /*IShellItem **ppsi*/out IShellItem/*IntPtr*/ ppsi );
        [PreserveSig] int AddPlace([In] /*IShellItem *psi*/IntPtr psi, [In] FDAP fdap );
        [PreserveSig] int SetDefaultExtension(/* [string][in] */ [MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
        [PreserveSig] int Close([In] /*HRESULT*/int hr );
        [PreserveSig] int SetClientGuid([In] ref Guid/*REFGUID*/ guid);
        [PreserveSig] int ClearClientData();
        [PreserveSig] int SetFilter([In] /*IShellItemFilter *pFilter*/IntPtr pFilter );
    }    

    /// <summary>
    /// 
    /// </summary>
    [ComImport, Guid(IIDGuid.IShellItem), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IShellItem
    {
        [PreserveSig] int BindToHandler( [In] /*IBindCtx *pbc*/IntPtr pbc, [In] ref Guid /*REFGUID*/ bhid, [In] ref Guid /*REFIID*/ riid, [Out] out IntPtr ppv );        
        [PreserveSig] int GetParent(/* [out] */ [Out] out IShellItem ppsi);
        [PreserveSig] int GetDisplayName( [In] SIGDN sigdnName, /*LPWSTR *ppszName*/[Out] out IntPtr pszName );
        [PreserveSig] int GetAttributes( [In] /*SFGAOF*/uint sfgaoMask, [Out] /*SFGAOF* */out uint psfgaoAttribs );
        [PreserveSig] int Compare( [In] IShellItem psi, [In] SICHINTF hint, [Out] out int piOrder );
        
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags] internal enum FOS : uint
    {
        OVERWRITEPROMPT          = 0x2,
        STRICTFILETYPES          = 0x4,
        NOCHANGEDIR              = 0x8,
        PICKFOLDERS              = 0x20,
        FORCEFILESYSTEM          = 0x40,
        ALLNONSTORAGEITEMS       = 0x80,
        NOVALIDATE               = 0x100,
        ALLOWMULTISELECT         = 0x200,
        PATHMUSTEXIST            = 0x800,
        FILEMUSTEXIST            = 0x1000,
        CREATEPROMPT             = 0x2000,
        SHAREAWARE               = 0x4000,
        NOREADONLYRETURN         = 0x8000,
        NOTESTFILECREATE         = 0x10000,
        HIDEMRUPLACES            = 0x20000,
        HIDEPINNEDPLACES         = 0x40000,
        NODEREFERENCELINKS       = 0x100000,
        OKBUTTONNEEDSINTERACTION = 0x200000,
        DONTADDTORECENT          = 0x2000000,
        FORCESHOWHIDDEN          = 0x10000000,
        DEFAULTNOMINIMODE        = 0x20000000,
        FORCEPREVIEWPANEON       = 0x40000000,
        SUPPORTSTREAMABLEITEMS   = 0x80000000
    }

    /// <summary>
    /// 
    /// </summary>
    internal enum SICHINTF : uint
    {
        DISPLAY                       = 0,
        CANONICAL                     = 0x10000000,
        TEST_FILESYSPATH_IF_NOT_EQUAL = 0x20000000,
        ALLFIELDS                     = 0x80000000,
    }

    /// <summary>
    /// 
    /// </summary>
    internal enum SIGDN : uint
    {
        NORMALDISPLAY               = 0,
        PARENTRELATIVEPARSING       = 0x80018001,
        DESKTOPABSOLUTEPARSING      = 0x80028000,
        PARENTRELATIVEEDITING       = 0x80031001,
        DESKTOPABSOLUTEEDITING      = 0x8004c000,
        FILESYSPATH                 = 0x80058000,
        URL                         = 0x80068000,
        PARENTRELATIVEFORADDRESSBAR = 0x8007c001,
        PARENTRELATIVE              = 0x80080001,
        PARENTRELATIVEFORUI         = 0x80094001
    }

    /// <summary>
    /// 
    /// </summary>
    internal enum FDAP : uint
    {
        BOTTOM = 0,
        TOP    = 1
    }

    /// <summary>
    /// 
    /// </summary>
    [ComImport, Guid(IIDGuid.IFileOpenDialog), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFileOpenDialog : IFileDialog
    {
        //// Defined on IModalWindow - repeated here due to requirements of COM interop layer
        //// --------------------------------------------------------------------------------
        //[PreserveSig] int Show([In] IntPtr parent);
        //[PreserveSig] int SetOptions([In] FOS fos);
    }


    // ---------------------------------------------------
    /// <summary>
    /// .NET classes representing runtime callable wrappers
    /// </summary>
    [ComImport, ClassInterface(ClassInterfaceType.None), TypeLibType(TypeLibTypeFlags.FCanCreate), Guid(CLSIDGuid.FileOpenDialog)]
    internal class FileOpenDialogRCW
    {
    }

    // ---------------------------------------------------------
    /// <summary>
    /// Coclass interfaces - designed to "look like" the object 
    /// in the API, so that the 'new' operator can be used in a 
    /// straightforward way. Behind the scenes, the C# compiler
    /// morphs all 'new CoClass()' calls to 'new CoClassWrapper()'
    /// </summary>
    [ComImport, Guid(IIDGuid.IFileOpenDialog), CoClass(typeof(FileOpenDialogRCW))]
    internal interface FileOpenDialogImpl : IFileOpenDialog
    {
    }
}
