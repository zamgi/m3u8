namespace m3u8.download.manager.ui
{
    ///// <summary>
    ///// 
    ///// </summary>
    //internal interface IOutputFileNamePatternProcessor
    //{
    //    string Process( string outputFileName );
    //    bool HasPatternChar( string outputFileName );
    //    bool IsEqualPattern( string outputFileName );
    //    bool HasLast_OutputFileName_As_Pattern { get; }
    //    bool TryGet_Patterned_Last_OutputFileName( out (string Patterned_Last_OutputFileName, string Last_OutputFileName_As_Pattern, int Last_OutputFileName_Num) t );
    //    string Get_Patterned_Last_OutputFileName();
    //    void Set_Last_OutputFileName_Num( int num );
    //}

    /// <summary>
    /// 
    /// </summary>
    internal sealed class OutputFileNamePatternProcessor //: IOutputFileNamePatternProcessor
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
                if ( _Last_OutputFileName_As_Pattern != null ) _Last_OutputFileName_Num = 1;
                _Last_OutputFileName_As_Pattern = outputFileName;                
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

        public bool HasPatternChar( string outputFileName ) => (outputFileName.IndexOf( _PatternChar ) != -1);
        public bool IsEqualPattern( string outputFileName ) => outputFileName.EqualIgnoreCase( _Last_OutputFileName_As_Pattern );
        public bool HasLast_OutputFileName_As_Pattern => (_Last_OutputFileName_As_Pattern != null);

        public bool TryGet_Patterned_Last_OutputFileName( out (string Patterned_Last_OutputFileName, string Last_OutputFileName_As_Pattern, int Last_OutputFileName_Num) t )
        {
            t = (Patterned_Last_OutputFileName : Get_Patterned_Last_OutputFileName(),
                 Last_OutputFileName_As_Pattern: _Last_OutputFileName_As_Pattern,
                 Last_OutputFileName_Num       : _Last_OutputFileName_Num);

            return (!t.Patterned_Last_OutputFileName.IsNullOrEmpty());
        }
        public string Get_Patterned_Last_OutputFileName() => Process_Internal( _Last_OutputFileName_As_Pattern, increase_pattern_num: false );
        /*public string Get_Patterned_OutputFileName( string pattern )
        {
            if ( pattern.IsNullOrEmpty() ) return (pattern);
            //--------------------------------------------------------------------------------------//

            var pattern_stars_pos = (before: pattern.IndexOf    ( _PatternChar ),
                                     after : pattern.LastIndexOf( _PatternChar )); //always exists if exists 'before'
            if ( pattern_stars_pos.before == -1 ) 
            {
                return (pattern);
            }

            var star_cnt = (pattern_stars_pos.after - pattern_stars_pos.before + 1);
            
            var n = _Last_OutputFileName_Num.ToString();
            var d = star_cnt - n.Length;
            if ( 0 < d ) n = new string( '0', d ) + n;

            var pattern_parts = (before_stars: pattern.Substring( 0, pattern_stars_pos.before ),
                                 after_stars : pattern.Substring( pattern_stars_pos.after + 1 ));
            var name_by_pattern = pattern_parts.before_stars + n + pattern_parts.after_stars;

            return (name_by_pattern);
        }*/
        public void Set_Last_OutputFileName_Num( int num ) => _Last_OutputFileName_Num = num;

        public string Last_OutputFileName_As_Pattern => _Last_OutputFileName_As_Pattern;
        public int    Last_OutputFileName_Num => _Last_OutputFileName_Num;
        public char   PatternChar => _PatternChar;
    }
}
