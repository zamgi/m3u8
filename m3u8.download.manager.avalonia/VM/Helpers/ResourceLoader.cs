using System;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Platform;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ResourceLoader
    {
        private string       _SelfAssemblyName;
        private IAssetLoader _AssetLoader;
        public ResourceLoader()
        {
            _SelfAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            _AssetLoader      = AvaloniaLocator.Current.GetService< IAssetLoader >();
        }

        public Stream GetResource( string resourcePath )
        {
            const string RESOURCE_LOCATION_PREFIX = "/Resources/";

            if ( resourcePath.IsNullOrEmpty() ) throw (new ArgumentNullException(  nameof(resourcePath) ));

            if ( resourcePath[ 0 ] != '/' )
            {
                resourcePath = '/' + resourcePath;
            }
            if ( !resourcePath.StartsWith( RESOURCE_LOCATION_PREFIX, StringComparison.Ordinal ) )
            {
                resourcePath = RESOURCE_LOCATION_PREFIX + resourcePath.TrimStart( '/' );
            }

            var stream = _AssetLoader.Open( new Uri( $"avares://{_SelfAssemblyName}{resourcePath}" ) );
            return (stream);
        }

        public static Stream _GetResource_( string resourcePath ) => (new ResourceLoader()).GetResource( resourcePath );
    }
}
