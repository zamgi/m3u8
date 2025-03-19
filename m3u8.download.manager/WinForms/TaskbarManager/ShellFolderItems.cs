using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.Taskbar
{
    internal class ShellFolderItems : IEnumerator< ShellObject >
    {
        private readonly ShellContainer _NativeShellFolder;
        private ShellObject _CurrentItem;
        private IEnumIDList _NativeEnumIdList;

        internal ShellFolderItems( ShellContainer nativeShellFolder )
        {
            _NativeShellFolder = nativeShellFolder;

            var hr = nativeShellFolder.NativeShellFolder.EnumObjects(
                IntPtr.Zero,
                ShellNativeMethods.ShellFolderEnumerationOptions.Folders | ShellNativeMethods.ShellFolderEnumerationOptions.NonFolders,
                out _NativeEnumIdList );

            if ( !CoreErrorHelper.Succeeded( hr ) )
            {
                if ( hr == HResult.Canceled )
                {
                    throw (new FileNotFoundException());
                }
                else
                {
                    throw (new ShellException( hr ));
                }
            }
        }

        public ShellObject Current => _CurrentItem;

        object IEnumerator.Current => _CurrentItem;

        public void Dispose()
        {
            if ( _NativeEnumIdList != null )
            {
                Marshal.ReleaseComObject( _NativeEnumIdList );
                _NativeEnumIdList = null;
            }
        }

        public bool MoveNext()
        {
            if ( _NativeEnumIdList == null ) return (false);

            uint itemsRequested = 1;
            var hr = _NativeEnumIdList.Next( itemsRequested, out var item, out var numItemsReturned );

            if ( numItemsReturned < itemsRequested || hr != HResult.Ok ) return (false);

            _CurrentItem = ShellObjectFactory.Create( item, _NativeShellFolder );

            return (true);
        }

        public void Reset()
        {
            if ( _NativeEnumIdList != null )
            {
                _NativeEnumIdList.Reset();
            }
        }
    }
}