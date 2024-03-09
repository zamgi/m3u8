using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using m3u8.download.manager.Properties;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static class AssemblyInfoHelper
    {
#if NETCOREAPP
        public static string FrameWorkName => Resources.NET_CORE;
#else
        public static string FrameWorkName => Resources.NET_FW;
#endif
        public static string AssemblyTitle
        {
            get
            {
                try
                {
                    var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof(AssemblyTitleAttribute), false );
                    if ( 0 < attributes.Length )
                    {
                        var titleAttribute = (AssemblyTitleAttribute) attributes[ 0 ];
                        if ( !string.IsNullOrEmpty( titleAttribute.Title ) )
                        {
                            return (titleAttribute.Title);
                        }
                    }
                    return (Path.GetFileNameWithoutExtension( Assembly.GetExecutingAssembly().Location )); 
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                    return (null);
                }
            }
        }
        public static string AssemblyCopyright
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof(AssemblyCopyrightAttribute), false );
                if ( 0 < attributes.Length )
                {
                    return ((AssemblyCopyrightAttribute) attributes[ 0 ]).Copyright;
                }
                return (null); 
            }
        }
        public static string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string AssemblyLastWriteTime
        {
            get
            {
                try
                {
                    return (File.GetLastWriteTime( Assembly.GetExecutingAssembly().Location ).ToString( "dd.MM.yyyy HH:mm" ));
                }
                catch (Exception ex )
                {
                    Debug.WriteLine(ex );
                    return (null);
                }
            }
        }
        #region comm.
        /*
        public static string AssemblyCompany
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof(AssemblyCompanyAttribute), false );
                if ( 0 < attributes.Length )
                {
                    return ((AssemblyCompanyAttribute) attributes[ 0 ]).Company;
                }
                return (null); 
            }
        }
        public static string AssemblyDescription
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof(AssemblyDescriptionAttribute), false );
                if ( 0 < attributes.Length )
                {
                    return ((AssemblyDescriptionAttribute) attributes[ 0 ]).Description; 
                }
                return (null);
            }
        }
        public static string AssemblyProduct
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof(AssemblyProductAttribute), false );
                if ( 0 < attributes.Length )
                {
                    return ((AssemblyProductAttribute) attributes[ 0 ]).Product;
                }
                return (null); 
            }
        }
        //*/
        #endregion
    }
}
