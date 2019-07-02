using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Xunit;

namespace m3u8.client.tests
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class m3u8_client_tests
    {
        private HttpClient _HttpClient;
        private Uri        _M3u8Url;

        public m3u8_client_tests()
        {
            const string SETTINGS_JSON_FILE_NAME = "settings.json";

            IConfigurationRoot configuration;
            try
            {
                configuration = new ConfigurationBuilder()
                                .AddJsonFile( SETTINGS_JSON_FILE_NAME )
                                .Build();
            }
            catch ( FileNotFoundException )
            {
                Debug.WriteLine( $"'{SETTINGS_JSON_FILE_NAME}' not found in project folder." );
                return;
            }

            var m3u8_url = configuration[ "m3u8_url" ];
            if ( m3u8_url.IsNullOrWhiteSpace() )
            {
                Debug.WriteLine( $"'{nameof(m3u8_url)}' not configured in '{SETTINGS_JSON_FILE_NAME}'." );
                return;
            }
            if ( !Uri.TryCreate( m3u8_url, UriKind.RelativeOrAbsolute, out _M3u8Url ) )
            {
                Debug.WriteLine( $"Wrong value of '{nameof( m3u8_url )}' in '{SETTINGS_JSON_FILE_NAME}'." );
                return;
            }

            _HttpClient = new HttpClient();
        }

        #region [.Tests.]
        private static void _Assert_( in m3u8_file_t m3u8_file  )
        {
            Assert.NotNull( m3u8_file.BaseAddress );
            Assert.True   ( !m3u8_file.RawText.IsNullOrWhiteSpace() );
            Assert.NotNull( m3u8_file.Parts );
            Assert.True   ( 0 < m3u8_file.Parts.Count );
        }
        private static void _Assert_( in m3u8_part_ts m3u8_part )
        {
            Assert.Null   ( m3u8_part.Error );            
            Assert.NotNull( m3u8_part.Bytes );
            Assert.True   ( 0 < m3u8_part.Bytes.Length );
        }

        [Fact]
        public async Task DownloadFile()
        {
            using ( var mc = new m3u8_client( _HttpClient, default ) )
            {
                var m3u8_file = await mc.DownloadFile( _M3u8Url );

                _Assert_( in m3u8_file );
            }
        }

        [Fact]
        public async Task DownloadPart()
        {
            using ( var mc = new m3u8_client( _HttpClient, default ) )
            {
                var m3u8_file = await mc.DownloadFile( _M3u8Url );

                _Assert_( in m3u8_file );

                var m3u8_part = m3u8_file.Parts.First();
                var downloaded_m3u8_part = await mc.DownloadPart( m3u8_part, m3u8_file.BaseAddress );

                _Assert_( in downloaded_m3u8_part );
            }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class Extensions
    {
        public static bool IsNullOrEmpty( this string s ) => string.IsNullOrEmpty( s );
        public static bool IsNullOrWhiteSpace( this string s ) => string.IsNullOrWhiteSpace( s );
    }
}
