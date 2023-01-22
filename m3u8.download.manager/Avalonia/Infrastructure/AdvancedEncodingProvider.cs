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

            const string UTF8  = "utf8";
            const string UTF_8 = "utf-8";
            if ( !_Dict_2.ContainsKey( UTF8 ) && _Dict_2.TryGetValue( UTF_8, out var utf8_encoding ) )
            {
                _Dict_2.Add( UTF8, utf8_encoding );
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
