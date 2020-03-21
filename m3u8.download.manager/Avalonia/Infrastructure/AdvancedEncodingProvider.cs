using System;
using System.Collections.Generic;
using System.Text;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class AdvancedEncodingProvider : EncodingProvider
    {
        private Dictionary< int   , Encoding > _Dict_1 = new Dictionary< int   , Encoding >();
        private Dictionary< string, Encoding > _Dict_2 = new Dictionary< string, Encoding >( StringComparer.InvariantCultureIgnoreCase );
        public AdvancedEncodingProvider( EncodingInfo[] encodingInfos )
        {
            foreach ( var ei in encodingInfos )
            {
                var encoding = Encoding.GetEncoding( ei.CodePage );
                _Dict_1.Add( ei.CodePage, encoding );
                _Dict_2.Add( ei.Name    , encoding );
            }

            if ( !_Dict_2.ContainsKey( "utf8" ) && _Dict_2.TryGetValue( "utf-8", out var _encoding ) )
            {
                _Dict_2.Add( "utf8", _encoding );
            }
        }
        public override Encoding GetEncoding( string name ) => (_Dict_2.TryGetValue( name, out var encoding ) ? encoding : default);
        public override Encoding GetEncoding( int codepage ) => (_Dict_1.TryGetValue( codepage, out var encoding ) ? encoding : default);

        public static void Init()
        {
            Encoding.RegisterProvider( CodePagesEncodingProvider.Instance );
            Encoding.RegisterProvider( new AdvancedEncodingProvider( Encoding.GetEncodings() ) );
        }
    }
}
