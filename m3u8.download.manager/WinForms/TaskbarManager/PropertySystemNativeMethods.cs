using System.Runtime.InteropServices;

namespace System.Windows.Forms.Taskbar
{
    /// <summary>
    /// 
    /// </summary>
    internal static class PropertySystemNativeMethods
    {
        /// <summary>
        /// 
        /// </summary>
        internal enum RelativeDescriptionType
        {
            General,
            Date,
            Size,
            Count,
            Revision,
            Length,
            Duration,
            Speed,
            Rate,
            Rating,
            Priority
        }

        private const string PROPSYS_DLL = "propsys.dll";

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int PSGetNameFromPropertyKey( ref PropertyKey propkey, [Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszCanonicalName );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern HResult PSGetPropertyDescription( ref PropertyKey propkey, ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out IPropertyDescription ppv );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int PSGetPropertyDescriptionListFromString( [In, MarshalAs(UnmanagedType.LPWStr)] string pszPropList, [In] ref Guid riid, out IPropertyDescriptionList ppv );

        [DllImport(PROPSYS_DLL, CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int PSGetPropertyKeyFromName( [In, MarshalAs(UnmanagedType.LPWStr)] string pszCanonicalName, out PropertyKey propkey );
    }
}