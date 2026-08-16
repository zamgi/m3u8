using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using m3u8.download.manager.Properties;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static partial class Extensions
    {
        [M(O.AggressiveInlining)] public static bool IsNullOrEmpty( this string s ) => string.IsNullOrEmpty( s );
        [M(O.AggressiveInlining)] public static bool IsNullOrWhiteSpace( this string s ) => string.IsNullOrWhiteSpace( s );
        [M(O.AggressiveInlining)] public static bool HasFirstCharNotDot( this string s ) => (s != null) && (0 < s.Length) && (s[ 0 ] != '.');
        [M(O.AggressiveInlining)] public static string GetValueIfNotNullOrWhiteSpaceOrDefault( this string s, string defVal ) => (s.IsNullOrWhiteSpace() ? defVal : s);
        [M(O.AggressiveInlining)] public static bool AnyEx< T >( this IEnumerable< T > seq ) => (seq != null) && seq.Any();
        [M(O.AggressiveInlining)] public static bool AnyEx< T >( this IList< T > seq ) => (seq != null) && (0 < seq.Count);
        [M(O.AggressiveInlining)] public static bool AnyEx< T >( this ICollection< T > seq ) => (seq != null) && (0 < seq.Count);
        [M(O.AggressiveInlining)] public static bool AnyEx< T >( this T[] seq ) => (seq != null) && (0 < seq.Length);
        [M(O.AggressiveInlining)] public static bool AnyEx_< T >( this IReadOnlyList< T > seq ) => (seq != null) && (0 < seq.Count);
        [M(O.AggressiveInlining)] public static bool AnyEx_< T >( this IReadOnlyCollection< T > seq ) => (seq != null) && (0 < seq.Count);
        [M(O.AggressiveInlining)] public static T? Try2Enum< T >( this string s ) where T : struct => (Enum.TryParse< T >( s, true, out var t ) ? t : null);
        [M(O.AggressiveInlining)] public static bool EqualIgnoreCase( this string s1, string s2 ) => (string.Compare( s1, s2, true ) == 0);
        [M(O.AggressiveInlining)] public static bool ContainsIgnoreCase( this string s1, string s2 ) => ((s1 != null) && (s1.IndexOf( s2, StringComparison.InvariantCultureIgnoreCase ) != -1));
        [M(O.AggressiveInlining)] public static bool StartsWith_Ex( this string s1, string s2, StringComparison sc = StringComparison.OrdinalIgnoreCase ) => (s1 != null) && s1.StartsWith( s2, sc );
        [M(O.AggressiveInlining)] public static bool EndsWith_Ex( this string s1, string s2, StringComparison sc = StringComparison.OrdinalIgnoreCase ) => (s1 != null) && s1.EndsWith( s2, sc );
        public static string Cut( this string s, int max_len ) => (s != null) ? ((max_len < s.Length) ? ((3 < max_len) ? $"{s.Substring( 0, max_len - 3 )}..." : s.Substring( 0, max_len )) : s) : s;
       
        public static void Remove< T >( this HashSet< T > hs, IEnumerable< T > seq )
        {
            if ( seq != null )
            {
                foreach ( var t in seq )
                {
                    hs.Remove( t );
                }
            }
        }
        public static void RemoveAllExcept< T >( this HashSet< T > hs, IEnumerable< T > seq )
        {
            var seq_hs = seq?.ToHashSet();
            if ( seq_hs.AnyEx() )
            {
                var exists = hs.ToArrayEx();
                foreach ( var t in exists )
                {
                    if ( !seq_hs.Contains( t ) )
                    {
                        hs.Remove( t );
                    }
                }
            }
            else
            {
                hs.Clear();
            }
        }
        public static void Add< T >( this HashSet< T > hs, IEnumerable< T > seq )
        {
            if ( seq != null )
            {
                foreach ( var t in seq )
                {
                    hs.Add( t );
                }
            }
        }
        //public static void RemoveIfNotNull< T >( this HashSet< T > hs, T t ) { if ( t != null ) hs.Remove( t ); }
        public static void AddIfNotNull< T >( this HashSet< T > hs, T t )
        {
            if ( t != null )
            {
                hs.Add( t );
            }
        }

        public static List< X > SelectToList< T, X >( this IReadOnlyList< T > seq, Func< T, X > func )
        {
            var lst = new List< X >( seq.Count );
            foreach ( var t in seq )
            {
                lst.Add( func( t ) );
            }
            return (lst);
        }
        public static T[] ToArrayEx< T >( this IReadOnlyList< T > seq )
        {
            var array = new T[ seq.Count ];
            for ( var i = seq.Count - 1; 0 <= i; i-- )
            {
                array[ i ] = seq[ i ];
            }
            return (array);
        }
        public static T[] ToArrayEx< T >( this IReadOnlyCollection< T > seq )
        {
            var array = new T[ seq.Count ];
            var i = 0;
            foreach ( var t in seq )
            {
                array[ i++ ] = t;
            }
            return (array);
        }
        public static List< T > ToList< T >( this IEnumerable< T > seq, int capacity )
        {
            var list = new List< T >( capacity );
            list.AddRange( seq );
            return (list);
        }
        public static T[] ReverseEx< T >( this T[] arr )
        {
            Array.Reverse( arr );
            return (arr);
        }
        //public static T[] ToArray< T >( this IEnumerable< T > seq, int size )
        //{
        //    var array = new T[ size ];
        //    var i = 0;
        //    foreach ( var t in seq )
        //    {
        //        array[ i++ ] = t;
        //    }
        //    return (array);
        //}
        public static void Replace< T >( this List< T > lst, IEnumerable< T > seq )
        {
            lst.Clear();
            if ( seq != null )
            {
                lst.AddRange( seq );
            }
        }
        public static void ForEach< T >( this IEnumerable< T > seq, Action < T > action )
        {
            if ( seq != null )
            {
                foreach ( var t in seq )
                {
                    action( t );
                }
            }
        }

        /// <summary>
        /// Copy user settings from previous application version if necessary
        /// </summary>
        [M(O.AggressiveInlining)] public static void UpgradeIfNeed( this Settings settings )
        {
            // Copy user settings from previous application version if necessary
            if ( !settings._IsUpgradedInThisVersion )
            {
                settings.Upgrade();
                settings._IsUpgradedInThisVersion = true;
                settings.MaxCrossDownloadInstance = settings.MaxCrossDownloadInstanceSaved;
                settings.SaveNoThrow();
            }
        }
        [M(O.AggressiveInlining)] public static void SaveNoThrow( this Settings settings )
        {
            try
            {
                settings.Save();
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        public static IEnumerable< string > GetNameCleanerExcludesWords( this Settings settings ) => settings.NameCleanerExcludesWords?.Cast< string >();
        public static void ResetNameCleanerExcludesWords( this Settings settings, IReadOnlyCollection< string > excludesWords )
        {
            if ( settings.NameCleanerExcludesWords == null )
            {
                settings.NameCleanerExcludesWords = new StringCollection();
            }
            else
            {
                settings.NameCleanerExcludesWords.Clear();
            }
            settings.NameCleanerExcludesWords.AddRange( excludesWords.ToArrayEx() );
        }

        [M(O.AggressiveInlining)] public static string TrimIfLongest( this string s, int maxLength ) => ((maxLength < s.Length) ? (s.Substring( 0, maxLength ) + "..." ) : s);

        public static string ToJSON< T >( this T t )
        {
            var ser = new DataContractJsonSerializer( typeof(T) );

            using ( var ms = new MemoryStream() )
            {                
                ser.WriteObject( ms, t );
                var json = Encoding.UTF8.GetString( ms.GetBuffer(), 0, (int) ms.Position );
                return (json);
            }
        }
        public static T FromJSON< T >( string json )
        {
            var ser = new DataContractJsonSerializer( typeof(T) );

            using ( var ms = new MemoryStream( Encoding.UTF8.GetBytes( json ) ) )
            {
                var t = (T) ser.ReadObject( ms );
                return (t);
            }
        }

        [M(O.AggressiveInlining)] public static void Cancel_NoThrow( this CancellationTokenSource cts )
        {
            try
            {
                cts.Cancel();
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        [M(O.AggressiveInlining)] public static void Set_NoThrow( this ManualResetEventSlim evt )
        {
            try
            {
                evt.Set();
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        [M(O.AggressiveInlining)] public static bool Reset_NoThrow( this ManualResetEventSlim evt )
        {
            try
            {
                evt.Reset();
                return (true);
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
            return (false);
        }
        [M(O.AggressiveInlining)] public static void Dispose_NoThrow( this IDisposable disposable )
        {
            try
            {
                disposable.Dispose();
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }

        [M(O.AggressiveInlining)] public static ConfiguredTaskAwaitable< T > CAX< T >( this Task< T > task ) => task.ConfigureAwait( false );
        [M(O.AggressiveInlining)] public static ConfiguredTaskAwaitable CAX( this Task task ) => task.ConfigureAwait( false );
#if NETCOREAPP
        [M(O.AggressiveInlining)] public static ConfiguredValueTaskAwaitable< T > CAX< T >( this in ValueTask< T > task ) => task.ConfigureAwait( false );
        [M(O.AggressiveInlining)] public static ConfiguredValueTaskAwaitable CAX( this in ValueTask task ) => task.ConfigureAwait( false );
#endif
        [M(O.AggressiveInlining)] public static void Invoke( this SynchronizationContext ctx, Action action ) => ctx.Send( _ => action(), null );

        public static string ToText( this IDictionary< string, string > requestHeaders, string separator = ": ", StringBuilder buf = null )
        {
            if ( requestHeaders.AnyEx() )
            {
                if ( buf == null ) buf = new StringBuilder(); else buf.Clear();
                foreach ( var p in requestHeaders )
                {
                    if ( buf.Length != 0 ) buf.Append( /*"\\r\\n "*/"; " ).AppendLine();
                    buf.Append( p.Key ).Append( separator ).Append( p.Value );
                }
                return (buf.ToString());
            }
            return (null);
        }
    }
}
