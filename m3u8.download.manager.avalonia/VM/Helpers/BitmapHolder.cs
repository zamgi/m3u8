using System.Collections.Generic;
using Avalonia.Media.Imaging;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class BitmapHolder
    {
        #region [.fields.]
        private IList< Bitmap > _Bitmaps;
        private int             _CurrentPos;
        #endregion

        #region [.ctor().]
        private BitmapHolder( IList< Bitmap > bitmaps ) => (_Bitmaps, _CurrentPos) = (bitmaps, -1);
        #endregion

        #region [.public methods.]
        public void Reset() => _CurrentPos = -1;
        
        public Bitmap Current
        {
            get
            {
                if ( (_CurrentPos < 0) || (_Bitmaps.Count <= _CurrentPos) )
                {
                    _CurrentPos = 0;
                }
                return (_Bitmaps[ _CurrentPos ]);
            }
        }
        public Bitmap Next()
        {
            _CurrentPos++;
            return (this.Current);
        }
        #endregion

        #region [.Singletons.]
        private static volatile BitmapHolder _IndicatorI;
        public static BitmapHolder IndicatorI
        {
            get
            {
                
                if ( _IndicatorI == null )
                {
                    lock ( typeof(BitmapHolder) )
                    {
                       var resourceLoader = new ResourceLoader();
                        var bitmaps = new List< Bitmap >( 8 );
                        for ( var i = 1; i <= 8; i++ )
                        {
                            var bitmap = new Bitmap( resourceLoader.GetResource( $"/Resources/roller/i{i}.ico" ) );
                            bitmaps.Add( bitmap );
                        }

                        _IndicatorI = new BitmapHolder( bitmaps );
                    }
                }
                return (_IndicatorI);
            }
        }
        #endregion
    }
}
