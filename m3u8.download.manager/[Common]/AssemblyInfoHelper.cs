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
        public static string VENDOR_NAME = "zamgi";
        public static string APP_NAME    => AssemblyInfoHelper.AssemblyTitle;

        static AssemblyInfoHelper()
        {
            var commonAppDataFolder   = Environment.GetFolderPath( Environment.SpecialFolder.CommonApplicationData );
            var vendorFolderPath      = Path.Combine( commonAppDataFolder, AssemblyInfoHelper.VENDOR_NAME );
            var appFolderPath         = Path.Combine( vendorFolderPath   , AssemblyInfoHelper.APP_NAME    );

            AppDataFolder = appFolderPath;
        }
        /// <summary>
        /// Application data folder
        /// </summary>
        public static string AppDataFolder { get; } //"C:\ProgramData\zamgi\m3u8.download.manager"

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
#if AVALONIA
        private static Assembly Get_AVALONIA_Assembly() => Assembly.GetAssembly( typeof(Avalonia.AvaloniaObject/*Application*/) );
        public static string AVALONIA_AssemblyVersion => Get_AVALONIA_Assembly().GetName().Version.ToString();
        public static string AVALONIA_AssemblyLastWriteTime
        {
            get
            {
                try
                {
                    return (File.GetLastWriteTime( Get_AVALONIA_Assembly().Location ).ToString( "dd.MM.yyyy HH:mm" ));
                }
                catch (Exception ex )
                {
                    Debug.WriteLine(ex );
                    return (null);
                }
            }
        }
#endif
    }
}
