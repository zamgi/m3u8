using System;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class OutputFileNamePatternProcessor
    {
        private char   _PatternChar;
        private string _Last_OutputFileName_As_Pattern;
        private int    _Last_OutputFileName_Num;
        public OutputFileNamePatternProcessor( char patternChar = '*' ) => _PatternChar = patternChar;

        public string Process( string outputFileName )
        {
            if ( outputFileName.EqualIgnoreCase( Get_Patterned_Last_OutputFileName() ) )
            {
                _Last_OutputFileName_Num++;
                return (outputFileName);
            }

            return Process_Internal( outputFileName, increase_pattern_num: true );
        }
        private string Process_Internal( string outputFileName, bool increase_pattern_num )
        {
            if ( outputFileName.IsNullOrEmpty() ) return (outputFileName);
            //--------------------------------------------------------------------------------------//

            var pattern_stars_pos = (before: outputFileName.IndexOf    ( _PatternChar ),
                                     after : outputFileName.LastIndexOf( _PatternChar )); //always exists if exists 'before'
            if ( pattern_stars_pos.before == -1 ) 
            {
                _Last_OutputFileName_As_Pattern = null;
                _Last_OutputFileName_Num        = 1;
                return (outputFileName);
            }

            if ( !outputFileName.EqualIgnoreCase( _Last_OutputFileName_As_Pattern ) )
            {
                _Last_OutputFileName_As_Pattern = outputFileName;
                _Last_OutputFileName_Num        = 1;
            }

            var star_cnt = (pattern_stars_pos.after - pattern_stars_pos.before + 1);
            
            var n = _Last_OutputFileName_Num.ToString();
            var d = star_cnt - n.Length;
            if ( 0 < d ) n = new string( '0', d ) + n;

            var pattern_parts = (before_stars: outputFileName.Substring( 0, pattern_stars_pos.before ),
                                 after_stars : outputFileName.Substring( pattern_stars_pos.after + 1 ));
            var name_by_pattern = pattern_parts.before_stars + n + pattern_parts.after_stars;
            if ( increase_pattern_num )
            {
                _Last_OutputFileName_Num++;
            }

            return (name_by_pattern);
        }

        public string Get_Patterned_Last_OutputFileName() => Process_Internal( _Last_OutputFileName_As_Pattern, increase_pattern_num: false );
        public string Last_OutputFileName_As_Pattern => _Last_OutputFileName_As_Pattern;
        public char PatternChar => _PatternChar;
    }
}
