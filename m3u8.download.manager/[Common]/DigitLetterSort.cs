using System.Collections.Generic;
using System.Linq;
#if DEBUG
using System.Text;
#endif

namespace System.Collections.DigitLetterSort
{
    /// <summary>
    /// Часть строки - цифры или не-цифры(буквы)
    /// </summary>
    internal sealed class PartOfString 
    {
        /// <summary>
        /// Тип части строки
        /// </summary>
        private enum PartOfStringTypeEnum
        {
            Unkonwn = 0,
            Letters,  //не-цифры(буквы)
            Digits,   //цифры
            EOF       //конец строки
        }

        /// <summary>
        ///
        /// </summary>
        public sealed class Comparer : IComparer< PartOfString >
        {
            public int Compare( PartOfString x, PartOfString y ) => x.GetIntRelation2Self( y );
        }

        public PartOfString( string value ) : this( value, 0 ) { }
        public PartOfString( string value, int startIndex )
        {
            if ( string.IsNullOrEmpty( value ) || (value.Length <= startIndex) )
            {
                this.PosType = PartOfStringTypeEnum.EOF;
                return;    
            }

            var length = 0;
            if ( char.IsDigit( value, startIndex ) )
            {
                this.PosType = PartOfStringTypeEnum.Digits;
                var s = new string( value.Skip( startIndex ).TakeWhile( char.IsDigit ).ToArray() );
                this.Digits = decimal.TryParse( s, out var d ) ? d : decimal.MaxValue;
                length = s.Length;
            }
            else
            {
                this.PosType = PartOfStringTypeEnum.Letters;
                this.Letters = new string( value.Skip( startIndex ).TakeWhile( ch => !char.IsDigit( ch ) ).ToArray());
                length = this.Letters.Length;
            }

            this.Next = new PartOfString( value, startIndex + length );
        }

        private PartOfStringTypeEnum PosType { get; set; }
        private decimal      Digits  { get; set; }
        private string       Letters { get; set; }
        private PartOfString Next    { get; set; }

        /// <summary>
        /// Метод - сравнивает (на больше, меньше, равно) два объекта типа 'PartOfString'
        /// </summary>
        /// <param name="one">PartOfString</param>
        /// <param name="two">PartOfString</param>
        /// <returns>результат сравнения: 0, 1, -1</returns>
        private static int GetIntRelations( PartOfString one, PartOfString two )
        {       
            while ( (one.PosType != PartOfStringTypeEnum.EOF) && (two.PosType != PartOfStringTypeEnum.EOF) )
            {
                switch ( one.PosType )
                {
                    case PartOfStringTypeEnum.Digits:
                        switch ( two.PosType )
                        {
                            case PartOfStringTypeEnum.Digits:
                                var d = one.Digits - two.Digits;
                                if ( d != 0 )
                                {
                                    if ( int.MaxValue <= d )
                                    {
                                        //return (Decimal.GetBits( _r ).Max());
                                        return (int.MaxValue);
                                    }
                                    else if ( d <= int.MinValue )
                                    {
                                        //return (Decimal.GetBits( _r ).Min());
                                        return (int.MinValue);
                                    }
                                    return ((int) d);
                                }
                                break;

                            case PartOfStringTypeEnum.Letters:
                                return (-1);
                        }
                        break;

                    case PartOfStringTypeEnum.Letters:
                        switch ( two.PosType )
                        {
                            case PartOfStringTypeEnum.Digits:
                                return (1);

                            case PartOfStringTypeEnum.Letters:
                                var d = string.Compare( one.Letters, two.Letters, true );
                                if ( d != 0 )
                                    return (d);
                                break;
                        }
                        break;
                }

                one = one.Next;
                two = two.Next;
            }

            if ( (one.PosType == PartOfStringTypeEnum.EOF) && (two.PosType == PartOfStringTypeEnum.EOF) )
                return (0);
            if ( one.PosType == PartOfStringTypeEnum.EOF )
                return (-1);
            return (1);
        }
        private int GetIntRelation2Self( PartOfString other ) => GetIntRelations( this, other );
#if DEBUG
        public override string ToString()
        {
            var buf = new StringBuilder();
            for ( var v = this; v.PosType != PartOfStringTypeEnum.EOF; v = v.Next )
            {
                buf.Append( (v.PosType == PartOfStringTypeEnum.Digits) ? v.Digits.ToString() : v.Letters );
            }
            return (buf.ToString());
        }
#endif
    }

    /// <summary>
    /// Методы-расширения для цифро-буквенной (или буквенно-цифровой) сортировки
    /// </summary>
    internal static class _DigitLetterSort_Extensions
    {
        public static PartOfString GetExistsOrAddNewValue( this Dictionary< string, PartOfString > dict, string key )
            => dict.TryGetValue( key, out var partOfString ) ? partOfString : dict.AddNew( key, new PartOfString( key ) );
        private static PartOfString AddNew( this Dictionary< string, PartOfString > dict, string key, PartOfString value )
        {
            dict.Add( key, value );
            return (value);
        }

        /*public static IEnumerable< FileViewItem > DigitLetterSortByFullNameAsc( this IEnumerable< FileViewItem > fvis )
            => fvis.OrderBy< FileViewItem, PartOfString >( fvi => new PartOfString( fvi.FullName ), new PartOfString.Comparer() );

        public static IEnumerable< FileViewItem > DigitLetterSortByFullNameDesc( this IEnumerable< FileViewItem > fvis )
            => fvis.OrderByDescending< FileViewItem, PartOfString >( fvi => new PartOfString( fvi.FullName ), new PartOfString.Comparer() );

        public static IEnumerable< FileViewItem > DigitLetterSortByNameAsc( this IEnumerable< FileViewItem > fvis )
            => fvis.OrderBy< FileViewItem, PartOfString >( fvi => new PartOfString( fvi.Name ), new PartOfString.Comparer() );

        public static IEnumerable< FileViewItem > DigitLetterSortByNameDesc( this IEnumerable< FileViewItem > fvis )
            => fvis.OrderByDescending< FileViewItem, PartOfString >( fvi => new PartOfString( fvi.Name ), new PartOfString.Comparer() );

        */

        /*/// <summary>
        /// Digit-Letter sort by short name of IEnumerable&lt; string &gt; 
        /// </summary>
        /// <returns>Sorted IEnumerable&lt; string &gt;</returns>
        public static IEnumerable< string > DigitLetterSort( this IEnumerable< string > values )
            => values.OrderBy< string, PartOfString >( value => new PartOfString( value ), new PartOfString.Comparer() );

        /// <summary>
        /// Digit-Letter sort by short name of [FileSystemInfo] 
        /// </summary>
        /// <returns>Sorted IEnumerable&lt; FileSystemInfo &gt;</returns>
        public static IEnumerable< FileSystemInfo > DigitLetterSortByShortName( this IEnumerable< FileSystemInfo > fileSystemInfos )
            => fileSystemInfos.OrderBy< FileSystemInfo, PartOfString >( fsInfo => new PartOfString( fsInfo.Name ), new PartOfString.Comparer() );

        /// <summary>
        /// Digit-Letter sort by full name of [FileSystemInfo] 
        /// </summary>
        /// <returns>Sorted IEnumerable&lt; FileSystemInfo &gt;</returns>
        public static IEnumerable< FileSystemInfo > DigitLetterSortByFullName( this IEnumerable< FileSystemInfo > fileSystemInfos )
            => fileSystemInfos.OrderBy< FileSystemInfo, PartOfString >( fsInfo => new PartOfString( fsInfo.FullName ), new PartOfString.Comparer() );

        /// <summary>
        /// Digit-Letter sort by short name of [FileSystemInfo] with using [Dictionary] for faster sort with more [FileSystemInfo] (prevent repeatedly create similar obejcts. may be quicker than without him)
        /// </summary>
        /// <returns>Sorted IEnumerable&lt; FileSystemInfo &gt;</returns>
        public static IEnumerable< FileSystemInfo > DigitLetterSortByShortNameAtDictionary( this IEnumerable< FileSystemInfo > fileSystemInfos )
        {
            var helpDictionary = new Dictionary< string, PartOfString >();// FileSystemInfos.Count() );
            var result = fileSystemInfos.OrderBy< FileSystemInfo, PartOfString >
            ( 
                fsInfo => helpDictionary.GetExistsOrAddNewValue( fsInfo.Name ),
                new PartOfString.Comparer()
            );
            return (result);
        }
        /// <summary>
        /// Digit-Letter sort by full name of [FileSystemInfo] with using [Dictionary] for faster sort with more [FileSystemInfo] (prevent repeatedly create similar obejcts. may be quicker than without him)
        /// </summary>
        /// <returns>Sorted IEnumerable&lt; FileSystemInfo &gt;</returns>
        public static IEnumerable< FileSystemInfo > DigitLetterSortByFullNameAtDictionary( this IEnumerable< FileSystemInfo > fileSystemInfos )
        {
            var helpDictionary = new Dictionary< string, PartOfString >();// FileSystemInfos.Count() );
            var result = fileSystemInfos.OrderBy< FileSystemInfo, PartOfString >
            ( 
                fsInfo => helpDictionary.GetExistsOrAddNewValue( fsInfo.FullName ),
                new PartOfString.Comparer()
            );
            return (result);
        }
        */
    }
}
