using System.Globalization;
using System.Text;

namespace System.Windows.Forms.Taskbar;

/// <summary>
/// 
/// </summary>
public static class CoreHelpers
{
    //
    // Summary:
    //     Determines if the application is running on Vista
    public static bool RunningOnVista => Environment.OSVersion.Version.Major >= 6;

    //
    // Summary:
    //     Determines if the application is running on Windows 7
    public static bool RunningOnWin7 => (Environment.OSVersion.Platform == PlatformID.Win32NT) && (0 <= Environment.OSVersion.Version.CompareTo( new Version( 6, 1 ) ));

    //
    // Summary:
    //     Determines if the application is running on XP
    public static bool RunningOnXP => (Environment.OSVersion.Platform == PlatformID.Win32NT) && (5 <= Environment.OSVersion.Version.Major);

    //
    // Summary:
    //     Get a string resource given a resource Id
    //
    // Parameters:
    //   resourceId:
    //     The resource Id
    //
    // Returns:
    //     The string resource corresponding to the given resource Id. Returns null if the
    //     resource id is invalid or the string cannot be retrieved for any other reason.
    public static string GetStringResource( string resourceId )
    {
        if ( string.IsNullOrEmpty( resourceId ) )
        {
            return string.Empty;
        }

        resourceId = resourceId.Replace( "shell32,dll", "shell32.dll" );
        var array = resourceId.Split( ',' );
        var instanceHandle = CoreNativeMethods.LoadLibrary( Environment.ExpandEnvironmentVariables( array[ 0 ].Replace( "@", string.Empty ) ) );
        array[ 1 ] = array[ 1 ].Replace( "-", string.Empty );
        var id = int.Parse( array[ 1 ], CultureInfo.InvariantCulture );
        var buf = new StringBuilder( 255 );
        if ( CoreNativeMethods.LoadString( instanceHandle, id, buf, 255 ) == 0 )
        {
            return (null);
        }
        return (buf.ToString());
    }

    //
    // Summary:
    //     Throws PlatformNotSupportedException if the application is not running on Windows
    //     Vista
    public static void ThrowIfNotVista()
    {
        if ( !RunningOnVista )
        {
            throw new PlatformNotSupportedException( "LocalizedMessages.CoreHelpersRunningOnVista" );
        }
    }

    //
    // Summary:
    //     Throws PlatformNotSupportedException if the application is not running on Windows
    //     7
    public static void ThrowIfNotWin7()
    {
        if ( !RunningOnWin7 )
        {
            throw new PlatformNotSupportedException( "LocalizedMessages.CoreHelpersRunningOn7" );
        }
    }

    //
    // Summary:
    //     Throws PlatformNotSupportedException if the application is not running on Windows
    //     XP
    public static void ThrowIfNotXP()
    {
        if ( !RunningOnXP )
        {
            throw new PlatformNotSupportedException( "LocalizedMessages.CoreHelpersRunningOnXp" );
        }
    }
}
