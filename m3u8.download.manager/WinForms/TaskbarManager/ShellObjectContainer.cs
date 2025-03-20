using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.Taskbar
{
    /// <summary>
    /// Represents the base class for all types of Shell "containers". Any class deriving from this class can contain other ShellObjects
    /// (e.g. ShellFolder, FileSystemKnownFolder, ShellLibrary, etc)
    /// </summary>
    public abstract class ShellContainer : ShellObject, IEnumerable<ShellObject>, IDisposable
    {
        private IShellFolder _DesktopFolderEnumeration;
        private IShellFolder _NativeShellFolder;

        internal ShellContainer() { }
        internal ShellContainer( IShellItem2 shellItem ) : base( shellItem ) { }

        internal IShellFolder NativeShellFolder
        {
            get
            {
                if ( _NativeShellFolder == null )
                {
                    var guid    = new Guid( ShellIIDGuid.IShellFolder );
                    var handler = new Guid( ShellBHIDGuid.ShellFolderObject );

                    var hr = NativeShellItem.BindToHandler(
                        IntPtr.Zero, ref handler, ref guid, out _NativeShellFolder );

                    if ( CoreErrorHelper.Failed( hr ) )
                    {
                        var str = ShellHelper.GetParsingName( NativeShellItem );
                        if ( str != null && str != Environment.GetFolderPath( Environment.SpecialFolder.Desktop ) )
                        {
                            throw (new ShellException( hr ));
                        }
                    }
                }

                return (_NativeShellFolder);
            }
        }

        /// <summary>Enumerates through contents of the ShellObjectContainer</summary>
        /// <returns>Enumerated contents</returns>
        public IEnumerator<ShellObject> GetEnumerator()
        {
            if ( NativeShellFolder == null )
            {
                if ( _DesktopFolderEnumeration == null )
                {
                    ShellNativeMethods.SHGetDesktopFolder( out _DesktopFolderEnumeration );
                }

                _NativeShellFolder = _DesktopFolderEnumeration;
            }

            return (new ShellFolderItems( this ));
        }

        IEnumerator IEnumerable.GetEnumerator() => new ShellFolderItems( this );

        /// <summary>Release resources</summary>
        /// <param name="disposing"><B>True</B> indicates that this is being called from Dispose(), rather than the finalizer.</param>
        protected override void Dispose( bool disposing )
        {
            if ( _NativeShellFolder != null )
            {
                Marshal.ReleaseComObject( _NativeShellFolder );
                _NativeShellFolder = null;
            }

            if ( _DesktopFolderEnumeration != null )
            {
                Marshal.ReleaseComObject( _DesktopFolderEnumeration );
                _DesktopFolderEnumeration = null;
            }

            base.Dispose( disposing );
        }
    }
}