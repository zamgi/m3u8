﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.Taskbar
{
    internal class ShellFolderItems : IEnumerator< ShellObject >
    {
        private readonly ShellContainer nativeShellFolder;
        private ShellObject currentItem;
        private IEnumIDList nativeEnumIdList;

        internal ShellFolderItems( ShellContainer nativeShellFolder )
        {
            this.nativeShellFolder = nativeShellFolder;

            var hr = nativeShellFolder.NativeShellFolder.EnumObjects(
                IntPtr.Zero,
                ShellNativeMethods.ShellFolderEnumerationOptions.Folders | ShellNativeMethods.ShellFolderEnumerationOptions.NonFolders,
                out nativeEnumIdList );

            if ( !CoreErrorHelper.Succeeded( hr ) )
            {
                if ( hr == HResult.Canceled )
                {
                    throw new FileNotFoundException();
                }
                else
                {
                    throw new ShellException( hr );
                }
            }
        }

        public ShellObject Current => currentItem;

        object IEnumerator.Current => currentItem;

        public void Dispose()
        {
            if ( nativeEnumIdList != null )
            {
                Marshal.ReleaseComObject( nativeEnumIdList );
                nativeEnumIdList = null;
            }
        }

        public bool MoveNext()
        {
            if ( nativeEnumIdList == null ) return (false);

            uint itemsRequested = 1;
            var hr = nativeEnumIdList.Next( itemsRequested, out var item, out var numItemsReturned );

            if ( numItemsReturned < itemsRequested || hr != HResult.Ok ) return (false);

            currentItem = ShellObjectFactory.Create( item, nativeShellFolder );

            return (true);
        }

        public void Reset()
        {
            if ( nativeEnumIdList != null )
            {
                nativeEnumIdList.Reset();
            }
        }
    }
}