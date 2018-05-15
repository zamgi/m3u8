using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class BitmapHolder
    {
        private Bitmap[] _Bitmaps;
        private int      _CurrentPos;

        public BitmapHolder( IEnumerable<Icon> icons )
        {
            _Bitmaps    = icons.Select( icon => icon.ToBitmap() ).ToArray();
            _CurrentPos = -1;
        }

        public void Reset() => _CurrentPos = -1;
        
        public Bitmap Current
        {
            get
            {
                if ( _CurrentPos < 0 || _Bitmaps.Length <= _CurrentPos )
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

        #region [.Singletons.]
        private static BitmapHolder _IndicatorI;
        public static BitmapHolder IndicatorI
        {
            get
            {
                if ( _IndicatorI == null )
                {
                    lock (typeof(BitmapHolder))
                    {
                        _IndicatorI = new BitmapHolder
                        (
                            new[]
                            {
                                Properties.Resources.i1,
                                Properties.Resources.i2,
                                Properties.Resources.i3,
                                Properties.Resources.i4,
                                Properties.Resources.i5,
                                Properties.Resources.i6,
                                Properties.Resources.i7,
                                Properties.Resources.i8,
                            }
                        );
                    }
                }
                return (_IndicatorI);
            }
        }
        #endregion
    }
}
