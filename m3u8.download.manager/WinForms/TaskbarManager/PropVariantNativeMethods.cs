using System.Runtime.InteropServices;

using _FILETIME_ = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace System.Windows.Forms.Taskbar
{
    internal static class PropVariantNativeMethods
    {
        private const string PROPSYS_DLL  = "propsys.dll";
        private const string OLE32_DLL    = "ole32.dll";
        private const string OLEAUT32_DLL = "OleAut32.dll";

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void InitPropVariantFromBooleanVector( [In, MarshalAs(UnmanagedType.LPArray)] bool[] prgf, uint cElems, [Out] PropVariant ppropvar );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void InitPropVariantFromDoubleVector( [In, Out] double[] prgn, uint cElems, [Out] PropVariant propvar );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void InitPropVariantFromFileTime( [In] ref _FILETIME_ pftIn, [Out] PropVariant ppropvar );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void InitPropVariantFromFileTimeVector( [In, Out] _FILETIME_[] prgft, uint cElems, [Out] PropVariant ppropvar );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void InitPropVariantFromInt16Vector( [In, Out] short[] prgn, uint cElems, [Out] PropVariant ppropvar );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void InitPropVariantFromInt32Vector( [In, Out] int[] prgn, uint cElems, [Out] PropVariant propVar );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void InitPropVariantFromInt64Vector( [In, Out] long[] prgn, uint cElems, [Out] PropVariant ppropvar );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void InitPropVariantFromPropVariantVectorElem( [In] PropVariant propvarIn, uint iElem, [Out] PropVariant ppropvar );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void InitPropVariantFromStringVector( [In, Out] string[] prgsz, uint cElems, [Out] PropVariant ppropvar );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void InitPropVariantFromUInt16Vector( [In, Out] ushort[] prgn, uint cElems, [Out] PropVariant ppropvar );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void InitPropVariantFromUInt32Vector( [In, Out] uint[] prgn, uint cElems, [Out] PropVariant ppropvar );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void InitPropVariantFromUInt64Vector( [In, Out] ulong[] prgn, uint cElems, [Out] PropVariant ppropvar );

        [DllImport(OLE32_DLL, PreserveSig=false)] // returns hresult
        internal static extern void PropVariantClear( [In, Out] PropVariant pvar );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void PropVariantGetBooleanElem( [In] PropVariant propVar, [In] uint iElem, [Out, MarshalAs( UnmanagedType.Bool)] out bool pfVal );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void PropVariantGetDoubleElem( [In] PropVariant propVar, [In] uint iElem, [Out] out double pnVal );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true)]
        [return: MarshalAs( UnmanagedType.I4)]
        internal static extern int PropVariantGetElementCount( [In] PropVariant propVar );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void PropVariantGetFileTimeElem( [In] PropVariant propVar, [In] uint iElem, [Out, MarshalAs( UnmanagedType.Struct)] out _FILETIME_ pftVal );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void PropVariantGetInt16Elem( [In] PropVariant propVar, [In] uint iElem, [Out] out short pnVal );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void PropVariantGetInt32Elem( [In] PropVariant propVar, [In] uint iElem, [Out] out int pnVal );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void PropVariantGetInt64Elem( [In] PropVariant propVar, [In] uint iElem, [Out] out long pnVal );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void PropVariantGetStringElem( [In] PropVariant propVar, [In] uint iElem, [MarshalAs(UnmanagedType.LPWStr)] ref string ppszVal );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void PropVariantGetUInt16Elem( [In] PropVariant propVar, [In] uint iElem, [Out] out ushort pnVal );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void PropVariantGetUInt32Elem( [In] PropVariant propVar, [In] uint iElem, [Out] out uint pnVal );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true, PreserveSig=false)]
        internal static extern void PropVariantGetUInt64Elem( [In] PropVariant propVar, [In] uint iElem, [Out] out ulong pnVal );

        [DllImport(OLEAUT32_DLL, PreserveSig=false)] // returns hresult
        internal static extern IntPtr SafeArrayAccessData( IntPtr psa );

        [DllImport(OLEAUT32_DLL, PreserveSig = true)] // psa is actually returned, not hresult
        internal static extern IntPtr SafeArrayCreateVector( ushort vt, int lowerBound, uint cElems );

        [DllImport(OLEAUT32_DLL, PreserveSig = true)] // retuns uint32
        internal static extern uint SafeArrayGetDim( IntPtr psa );

        // This decl for SafeArrayGetElement is only valid for cDims==1!
        [DllImport(OLEAUT32_DLL, PreserveSig=false)] // returns hresult
        [return: MarshalAs( UnmanagedType.IUnknown)]
        internal static extern object SafeArrayGetElement( IntPtr psa, ref int rgIndices );

        [DllImport(OLEAUT32_DLL, PreserveSig=false)] // returns hresult
        internal static extern int SafeArrayGetLBound( IntPtr psa, uint nDim );

        [DllImport(OLEAUT32_DLL, PreserveSig=false)] // returns hresult
        internal static extern int SafeArrayGetUBound( IntPtr psa, uint nDim );

        [DllImport(OLEAUT32_DLL, PreserveSig=false)] // returns hresult
        internal static extern void SafeArrayUnaccessData( IntPtr psa );
    }
}