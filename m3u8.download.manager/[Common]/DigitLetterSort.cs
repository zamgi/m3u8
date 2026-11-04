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
                                        //return (Decimal.GetBits( d ).Max());
                                        return (int.MaxValue);
                                    }
                                    else if ( d <= int.MinValue )
                                    {
                                        //return (Decimal.GetBits( d ).Min());
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
    }
}
