using System.IO;
using System.Reflection;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static class AssemblyInfoHelper
    {
        public static string AssemblyTitle
        {
            get
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
                return (Path.GetFileNameWithoutExtension( Assembly.GetExecutingAssembly().CodeBase )); 
            }
        }
        public static string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string AssemblyDescription
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof(AssemblyDescriptionAttribute), false );
                if ( 0 < attributes.Length )
                {
                    return ((AssemblyDescriptionAttribute) attributes[ 0 ]).Description; 
                }
                return (string.Empty);
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
                return (string.Empty); 
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
                return (string.Empty); 
            }
        }
        public static string AssemblyCompany
        {
            get
            {
                var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof(AssemblyCompanyAttribute), false );
                if ( 0 < attributes.Length )
                {
                    return ((AssemblyCompanyAttribute) attributes[ 0 ]).Company;
                }
                return (string.Empty); 
            }
        }
        public static string AssemblyLastWriteTime => File.GetLastWriteTime( Assembly.GetExecutingAssembly().Location ).ToString( "dd.MM.yyyy HH:mm" );
    }
}
