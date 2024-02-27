using System;
//using System.Drawing;
//using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace m3u8.download.manager
{
    /// <summary>
    /// Specifies ellipsis format and alignment.
    /// </summary>
    [Flags] internal enum EllipsisFormat
    {
        /// <summary>
        /// Text is not modified.
        /// </summary>
        None = 0,
        /// <summary>
        /// Text is trimmed at the end of the string. An ellipsis (...) is drawn in place of remaining text.
        /// </summary>
        End = 1,
        /// <summary>
        /// Text is trimmed at the begining of the string. An ellipsis (...) is drawn in place of remaining text. 
        /// </summary>
        Start = 2,
        /// <summary>
        /// Text is trimmed in the middle of the string. An ellipsis (...) is drawn in place of remaining text.
        /// </summary>
        Middle = 3,
        /// <summary>
        /// Preserve as much as possible of the drive and filename information. Must be combined with alignment information.
        /// </summary>
        Path = 4,
        /// <summary>
        /// Text is trimmed at a word boundary. Must be combined with alignment information.
        /// </summary>
        Word = 8
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class Ellipsis
    {
        /// <summary>
        /// string used as a place holder for trimmed text.
        /// </summary>
        public const string ELLIPSIS_CHARS = "...";
        public const int    DEFAULT_MAX_LENGTH = 150;

        private static Regex _PrevWordRegex;
        private static Regex _NextWordRegex;

        private static Regex PrevWordRegex
        {
            get
            {
                if ( _PrevWordRegex == null )
                {
                    lock( typeof(Ellipsis) )
                    {
                        if ( _PrevWordRegex == null )
                        {
                            _PrevWordRegex = new Regex( @"\W*\w*$" );
                        }
                    }
                }
                return (_PrevWordRegex);
            }
        }
        private static Regex NextWordRegex
        {
            get
            {
                if ( _NextWordRegex == null )
                {
                    lock( typeof(Ellipsis) )
                    {
                        if ( _NextWordRegex == null )
                        {
                            _NextWordRegex = new Regex( @"\w*\W*" );
                        }
                    }
                }
                return (_NextWordRegex);
            }
        }

        /// <summary>
        /// Truncates a text string to fit within a given control width by replacing trimmed text with ellipses. 
        /// </summary>
        /// <param name="text">string to be trimmed.</param>
        /// <param name="ctrl">text must fit within ctrl width.
        ///	The ctrl's Font is used to measure the text string.</param>
        /// <param name="options">Format and alignment of ellipsis.</param>
        /// <returns>This function returns text trimmed to the specified witdh.</returns>
        public static string Compact( string text, int maxLength, EllipsisFormat options )
        {
            if ( text.IsNullOrEmpty() || text.Length <= maxLength )
                return (text);

            // no aligment information
            if ( (EllipsisFormat.Middle & options) == 0 )
                return (text);

            var pre  = string.Empty;
            var mid  = text;
            var post = string.Empty;

            var isPath = (EllipsisFormat.Path & options) != 0;

            // split path string into <drive><directory><filename>
            if ( isPath )
            {
                pre  = Path.GetPathRoot( text );
                mid  = Path.GetDirectoryName( text ).Substring( pre.Length );
                post = Path.GetFileName( text );
            }

            var len = 0;
            var seg = mid.Length;
            var fit = string.Empty;

            // find the longest string that fits into 
            // the control boundaries using bisection method
            while ( seg > 1 )
            {
                seg -= seg / 2;

                int left  = len + seg;
                int right = mid.Length;

                if ( left > right )
                    continue;

                if ( (EllipsisFormat.Middle & options) == EllipsisFormat.Middle )
                {
                    right -= left / 2;
                    left  -= left / 2;
                }
                else if ( (EllipsisFormat.Start & options) != 0 )
                {
                    right -= left;
                    left   = 0;
                }

                // trim at a word boundary using regular expressions
                if ( (EllipsisFormat.Word & options) != 0 )
                {
                    if ( (EllipsisFormat.End & options) != 0 )
                    {
                        left -= PrevWordRegex.Match( mid, 0, left ).Length;
                    }
                    if ( (EllipsisFormat.Start & options) != 0 )
                    {
                        right += NextWordRegex.Match( mid, right ).Length;
                    }
                }

                // build and measure a candidate string with ellipsis
                var tst = mid.Substring( 0, left ) + ELLIPSIS_CHARS + mid.Substring( right );

                // restore path with <drive> and <filename>
                if ( isPath )
                {
                    tst = Path.Combine( Path.Combine( pre, tst ), post );
                }
                var sz = tst.Length;

                // candidate string fits into control boundaries, try a longer string
                // stop when seg <= 1
                if ( sz <= maxLength )
                {
                    len += seg;
                    fit  = tst;
                }
            }

            if ( len == 0 ) // string can't fit into control
            {
                // "path" mode is off, just return ellipsis characters
                if ( !isPath )
                    return (ELLIPSIS_CHARS);

                // <drive> and <directory> are empty, return <filename>
                if ( pre.IsNullOrEmpty() && mid.IsNullOrEmpty() )
                    return (post);

                // measure "C:\...\filename.ext"
                fit = Path.Combine( Path.Combine( pre, ELLIPSIS_CHARS ), post );

                // if still not fit then return "...\filename.ext"
                if ( fit.Length > maxLength )
                    fit = Path.Combine( ELLIPSIS_CHARS, post );
            }
            return (fit);
            
        }

        public static string MinimizePath( string path, int maxLength = DEFAULT_MAX_LENGTH ) => Compact( path, maxLength, EllipsisFormat.Path | EllipsisFormat.Middle );


        /*
        /// <summary>
        /// Truncates a text string to fit within a given control width by replacing trimmed text with ellipses. 
        /// </summary>
        /// <param name="text">string to be trimmed.</param>
        /// <param name="ctrl">text must fit within ctrl width.
        ///	The ctrl's Font is used to measure the text string.</param>
        /// <param name="options">Format and alignment of ellipsis.</param>
        /// <returns>This function returns text trimmed to the specified witdh.</returns>
        public static string Compact( string text, Control ctrl, EllipsisFormat options )
        {
            if ( ctrl == null ) throw (new ArgumentNullException( nameof(ctrl) ));

            if ( text.IsNullOrEmpty() )
            {
                return (text);
            }

            // no aligment information
            if ( (EllipsisFormat.Middle & options) == 0 )
            {
                return (text);
            }

            using ( Graphics dc = ctrl.CreateGraphics() )
            {
                var sz = TextRenderer.MeasureText( dc, text, ctrl.Font );

                // control is large enough to display the whole text
                if ( sz.Width <= ctrl.Width )
                    return (text);

                var pre  = string.Empty;
                var mid  = text;
                var post = string.Empty;

                var isPath = (EllipsisFormat.Path & options) != 0;

                // split path string into <drive><directory><filename>
                if ( isPath )
                {
                    pre  = Path.GetPathRoot( text );
                    mid  = Path.GetDirectoryName( text ).Substring( pre.Length );
                    post = Path.GetFileName( text );
                }

                var len = 0;
                var seg = mid.Length;
                var fit = string.Empty;

                // find the longest string that fits into 
                // the control boundaries using bisection method
                while ( seg > 1 )
                {
                    seg -= seg / 2;

                    var left  = len + seg;
                    var right = mid.Length;

                    if ( left > right )
                        continue;

                    if ( (EllipsisFormat.Middle & options) == EllipsisFormat.Middle )
                    {
                        right -= left / 2;
                        left  -= left / 2;
                    }
                    else if ( (EllipsisFormat.Start & options) != 0 )
                    {
                        right -= left;
                        left   = 0;
                    }

                    // trim at a word boundary using regular expressions
                    if ( (EllipsisFormat.Word & options) != 0 )
                    {
                        if ( (EllipsisFormat.End & options) != 0 )
                        {
                            left -= PrevWordRegex.Match( mid, 0, left ).Length;
                        }
                        if ( (EllipsisFormat.Start & options) != 0 )
                        {
                            right += NextWordRegex.Match( mid, right ).Length;
                        }
                    }

                    // build and measure a candidate string with ellipsis
                    var tst = mid.Substring( 0, left ) + ELLIPSIS_CHARS + mid.Substring( right );

                    // restore path with <drive> and <filename>
                    if ( isPath )
                    {
                        tst = Path.Combine( Path.Combine( pre, tst ), post );
                    }
                    sz = TextRenderer.MeasureText( dc, tst, ctrl.Font );

                    // candidate string fits into control boundaries, try a longer string
                    // stop when seg <= 1
                    if ( sz.Width <= ctrl.Width )
                    {
                        len += seg;
                        fit  = tst;
                    }
                }

                if ( len == 0 ) // string can't fit into control
                {
                    // "path" mode is off, just return ellipsis characters
                    if ( !isPath )
                        return (ELLIPSIS_CHARS);

                    // <drive> and <directory> are empty, return <filename>
                    if ( pre.IsNullOrEmpty() && mid.IsNullOrEmpty() )
                        return (post);

                    // measure "C:\...\filename.ext"
                    fit = Path.Combine( Path.Combine( pre, ELLIPSIS_CHARS ), post );

                    sz = TextRenderer.MeasureText( dc, fit, ctrl.Font );

                    // if still not fit then return "...\filename.ext"
                    if ( sz.Width > ctrl.Width )
                        fit = Path.Combine( ELLIPSIS_CHARS, post );
                }
                return (fit);
            }
        }         
        //*/
    }
}
