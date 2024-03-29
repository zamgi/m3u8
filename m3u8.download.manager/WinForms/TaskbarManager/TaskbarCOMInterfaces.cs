﻿using System.Runtime.InteropServices;

namespace System.Windows.Forms.Taskbar
{
    [ComImport]
    [Guid("6332DEBF-87B5-4670-90C0-5E57B408A49E" )]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICustomDestinationList
    {
        void SetAppID(
            [MarshalAs(UnmanagedType.LPWStr)] string pszAppID );

        [PreserveSig]
        HResult BeginList(
            out uint cMaxSlots,
            ref Guid riid,
            [Out(), MarshalAs(UnmanagedType.Interface)] out object ppvObject );

        [PreserveSig]
        HResult AppendCategory(
            [MarshalAs(UnmanagedType.LPWStr)] string pszCategory,
            [MarshalAs(UnmanagedType.Interface)] IObjectArray poa );

        void AppendKnownCategory(
            [MarshalAs( UnmanagedType.I4 )] KnownDestinationCategory category );

        [PreserveSig]
        HResult AddUserTasks(
            [MarshalAs(UnmanagedType.Interface)] IObjectArray poa );

        void CommitList();

        void GetRemovedDestinations(
            ref Guid riid,
            [Out(), MarshalAs(UnmanagedType.Interface)] out object ppvObject );

        void DeleteList(
            [MarshalAs(UnmanagedType.LPWStr)] string pszAppID );

        void AbortList();
    }

    [ComImport()]
    [Guid("92CA9DCD-5622-4BBA-A805-5E9F541BD8C9" )]
    [InterfaceTypeAttribute( ComInterfaceType.InterfaceIsIUnknown )]
    internal interface IObjectArray
    {
        void GetCount( out uint cObjects );

        void GetAt(
            uint iIndex,
            ref Guid riid,
            [Out(), MarshalAs(UnmanagedType.Interface)] out object ppvObject );
    }

    [ComImport()]
    [Guid("5632B1A4-E38A-400A-928A-D4CD63230295" )]
    [InterfaceTypeAttribute( ComInterfaceType.InterfaceIsIUnknown )]
    internal interface IObjectCollection
    {
        // IObjectArray
        [PreserveSig]
        void GetCount( out uint cObjects );

        [PreserveSig]
        void GetAt(
            uint iIndex,
            ref Guid riid,
            [Out(), MarshalAs(UnmanagedType.Interface)] out object ppvObject );

        // IObjectCollection
        void AddObject(
            [MarshalAs(UnmanagedType.Interface)] object pvObject );

        void AddFromArray(
            [MarshalAs(UnmanagedType.Interface)] IObjectArray poaSource );

        void RemoveObject( uint uiIndex );

        void Clear();
    }

    [ComImport()]
    [Guid("c43dc798-95d1-4bea-9030-bb99e2983a1a" )]
    [InterfaceTypeAttribute( ComInterfaceType.InterfaceIsIUnknown )]
    internal interface ITaskbarList4
    {
        // ITaskbarList
        [PreserveSig]
        void HrInit();

        [PreserveSig]
        void AddTab( IntPtr hwnd );

        [PreserveSig]
        void DeleteTab( IntPtr hwnd );

        [PreserveSig]
        void ActivateTab( IntPtr hwnd );

        [PreserveSig]
        void SetActiveAlt( IntPtr hwnd );

        // ITaskbarList2
        [PreserveSig]
        void MarkFullscreenWindow(
            IntPtr hwnd,
            [MarshalAs( UnmanagedType.Bool )] bool fFullscreen );

        // ITaskbarList3
        [PreserveSig]
        void SetProgressValue( IntPtr hwnd, ulong ullCompleted, ulong ullTotal );

        [PreserveSig]
        void SetProgressState( IntPtr hwnd, TaskbarProgressBarStatus tbpFlags );

        [PreserveSig]
        void RegisterTab( IntPtr hwndTab, IntPtr hwndMDI );

        [PreserveSig]
        void UnregisterTab( IntPtr hwndTab );

        [PreserveSig]
        void SetTabOrder( IntPtr hwndTab, IntPtr hwndInsertBefore );

        [PreserveSig]
        void SetTabActive( IntPtr hwndTab, IntPtr hwndInsertBefore, uint dwReserved );

        [PreserveSig]
        HResult ThumbBarAddButtons(
            IntPtr hwnd,
            uint cButtons,
            [MarshalAs(UnmanagedType.LPArray)] ThumbButton[] pButtons );

        [PreserveSig]
        HResult ThumbBarUpdateButtons(
            IntPtr hwnd,
            uint cButtons,
            [MarshalAs(UnmanagedType.LPArray)] ThumbButton[] pButtons );

        [PreserveSig]
        void ThumbBarSetImageList( IntPtr hwnd, IntPtr himl );

        [PreserveSig]
        void SetOverlayIcon(
          IntPtr hwnd,
          IntPtr hIcon,
          [MarshalAs(UnmanagedType.LPWStr)] string pszDescription );

        [PreserveSig]
        void SetThumbnailTooltip(
            IntPtr hwnd,
            [MarshalAs(UnmanagedType.LPWStr)] string pszTip );

        [PreserveSig]
        void SetThumbnailClip(
            IntPtr hwnd,
            IntPtr prcClip );

        // ITaskbarList4
        void SetTabProperties( IntPtr hwndTab, SetTabPropertiesOption stpFlags );
    }

    [Guid("77F10CF0-3DB5-4966-B520-B7C54FD35ED6" )]
    [ClassInterface(ClassInterfaceType.None )]
    [ComImport()]
    internal class CDestinationList { }

    [Guid("2D3468C1-36A7-43B6-AC24-D3F02FD9607A" )]
    [ClassInterface(ClassInterfaceType.None )]
    [ComImport()]
    internal class CEnumerableObjectCollection { }

    [Guid("56FDF344-FD6D-11d0-958A-006097C9A090" )]
    [ClassInterface(ClassInterfaceType.None )]
    [ComImport()]
    internal class CTaskbarList { }
}
