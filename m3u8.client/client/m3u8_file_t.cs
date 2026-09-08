using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using m3u8.client;
using m3u8.infrastructure;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.client
{
    /// <summary>
    /// 
    /// </summary>
    public /*struct*/sealed class m3u8_part_ts : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly struct Comparer: IComparer< m3u8_part_ts >
        {
            public static Comparer Inst { get; } = new Comparer();
            public int Compare( m3u8_part_ts x, m3u8_part_ts y ) => (x.OrderNumber - y.OrderNumber);
        }

        //public m3u8_part_ts( string relativeUrlName, int orderNumber ) : this() => (RelativeUrlName, OrderNumber) = (relativeUrlName, orderNumber);
        private m3u8_part_ts() : this( null, -1, null ) { Prev = this; TotalContentLength = 0; }
        public static m3u8_part_ts Zero { get; } = new m3u8_part_ts();
        public m3u8_part_ts( string relativeUrlName, int orderNumber, m3u8_part_ts prev )
        {
            RelativeUrlName = relativeUrlName;
            OrderNumber     = orderNumber;
            Prev            = prev;
        }
        public void Dispose()
        {
            if ( _Holder != null )
            {
                _Holder.Dispose();
                _Holder = null;
            }
        }

        public m3u8_part_ts Prev            { get; }
        public string       RelativeUrlName { get; }
        public int          OrderNumber     { get; }

        private IObjectHolder< Stream > _Holder;
        public Stream Stream { get; private set; }
        public void SetStreamHolder( IObjectHolder< Stream > holder )
        {
            _Holder = holder;
            Stream  = holder.Value;
            Stream.SetLength( 0 );
        }

        public Exception Error { get; private set; }
        public void SetError( Exception error ) => Error = error;

        public long? TotalContentLength { get; private set; }
        public void SetTotalContentLength( long? totalContentLength ) => TotalContentLength = totalContentLength;
        
        public bool TryCalcTotalContentLengthBefore( int beforePrevPartOrderNumber, out long totalContentLengthBefore )
        {
            totalContentLengthBefore = 0;
            for ( var part = this.Prev; (part != null) && (part != m3u8_part_ts.Zero/*part.Prev*/); part = part.Prev )
            {
                var totalContentLength = part.TotalContentLength;
                if ( !totalContentLength.HasValue )
                {
                    return (false);
                }
                totalContentLengthBefore += totalContentLength.Value;
                if ( part.OrderNumber == beforePrevPartOrderNumber )
                {
                    break;
                }
            }
            return (totalContentLengthBefore != 0/*true*/);
        }

#if DEBUG
        public override string ToString() => $"{OrderNumber}, '{RelativeUrlName}'" +
                                             ((Error != null) ? $", Error: {Error}" : null) + 
                                             ((Stream != null) ? $", Stream: {Stream.Length}"   : null);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public readonly struct m3u8_file_t
    {
        public IReadOnlyList< m3u8_part_ts > Parts { get; init; }
        public Uri    BaseAddress { get; init; }
        public string RawText     { get; init; }

        public static m3u8_file_t Parse( string content, Uri baseAddress )
        {
            var rawRows = content.Split( [ '\r', '\n' ], StringSplitOptions.RemoveEmptyEntries );
            var lines = from row in rawRows
                        let line = row.Trim()
                        where (!line.IsNullOrEmpty() && !line.StartsWith( "#" ))
                        select line
                        ;
            //var parts = lines.Select( (line, i) => new m3u8_part_ts( line, i ) );
            var parts = new List< m3u8_part_ts >( rawRows.Length );
            var i = 0;
            var prev = m3u8_part_ts.Zero; //default(m3u8_part_ts);
            foreach ( var line in lines )
            {
                var part = new m3u8_part_ts( line, i++, prev );
                parts.Add( part );
                prev = part;
            }
            var o = new m3u8_file_t()
            {
                //Parts       = parts.ToList( rawRows.Length ).AsReadOnly(),
                Parts       = parts.AsReadOnly(),
                BaseAddress = baseAddress,
                RawText     = content,
            };
            return (o);
        }
        public static m3u8_file_t From( in m3u8_file_t mf, IReadOnlyList< m3u8_part_ts > new_parts ) => new m3u8_file_t()
        {
            Parts       = new_parts,
            BaseAddress = mf.BaseAddress,
            RawText     = mf.RawText,
        };
#if DEBUG
        public override string ToString() => $"Parts: {Parts?.Count.ToString() ?? "-"}";
#endif
    }
}

namespace m3u8.infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    internal static partial class Extensions
    {
        [M(O.AggressiveInlining)] internal static Uri GetPartUrl( this /*in*/ m3u8_part_ts part, Uri baseAddress ) => baseAddress.GetPartUrl( part.RelativeUrlName );
        //internal static List< T > ToList< T >( this IEnumerable< T > source, int capacity )
        //{
        //    var lst = new List< T >( capacity );
        //    lst.AddRange( source );
        //    return (lst);
        //}
    }
}
