using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using m3u8.download.manager.Properties;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static class ObjAsDict_JsonSerializer
    {
        public static string ToJSON( Settings settings ) => ToJSON( settings, settings.Properties.Cast< SettingsProperty >().Select( p => p.Name ) );
        public static string ToJSON< T >( T obj, IEnumerable< string > serializePropNames )
        {
            var props = typeof(T).GetProperties();
            var props_set = new HashSet< PropertyInfo >( props.Length );
            var hs = serializePropNames.ToHashSet();

            foreach ( var prop in props )
            {
                if ( hs.Contains( prop.Name ) )
                {
                    props_set.Add( prop );
                }
            }
            return (obj.ToJSON( props_set ));
        }
        private static string ToJSON< T >( this T obj, IReadOnlyCollection< PropertyInfo > props )
        {
            var pd = new Dictionary< string, object >( props.Count );
            
            foreach ( var prop in props )
            {
                var pt = prop.PropertyType;
                if ( pt.IsPrimitive || pt.IsValueType || pt.IsEnum || ( pt == typeof(string)) )
                {
                    pd[ prop.Name ] = prop.GetValue( obj )?.ToString() ?? "NULL";
                }
                else if ( (pt.Name == "StringCollection") && (pt.Namespace == "System.Collections.Specialized") )
                {
                    var sc = (StringCollection) prop.GetValue( obj );
                    if ( sc != null )
                    {
                        pd[ prop.Name ] = string.Join( "; ", sc.Cast<string>() );
                    }
                }
                else
                {
                    Debug.Assert( false, $"Unknown props-type: '{prop.Name}'" );
                    Debug.WriteLine( $"{prop.Name}, IsValueType: {pt.IsValueType}, val: {prop.GetValue( obj )}"  );
                }
                //---pd[ prop.Name ] = prop.GetValue( obj )?.ToString() ?? "NULL";
            }
            var json = pd.ToJSON();
            return (json);
        }
    }
}
