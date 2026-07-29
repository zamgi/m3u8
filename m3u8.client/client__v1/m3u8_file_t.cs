using System;
using System.Collections.Generic;
using System.Linq;

using m3u8.client__v1;
using m3u8.infrastructure;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.client__v1
{
    /// <summary>
    /// 
    /// </summary>
    public struct m3u8_part_ts
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly struct Comparer: IComparer< m3u8_part_ts >
        {
            public static Comparer Inst { get; } = new Comparer();
            public int Compare( m3u8_part_ts x, m3u8_part_ts y ) => (x.OrderNumber - y.OrderNumber);
        }

        public m3u8_part_ts( string relativeUrlName, int orderNumber ) => (RelativeUrlName, OrderNumber) = (relativeUrlName, orderNumber);

        public string RelativeUrlName { get; }
        public int    OrderNumber     { get; }

        public byte[] Bytes { get; private set; }
        public void SetBytes( byte[] bytes ) => Bytes = bytes;

        public Exception Error { get; private set; }
        public void SetError( Exception error ) => Error = error;
#if DEBUG
        public override string ToString() => $"{OrderNumber}, '{RelativeUrlName}'";
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public struct m3u8_file_t
    {
        public IReadOnlyList< m3u8_part_ts > Parts { get; private set; }
        public Uri BaseAddress { get; private set; }
        public string RawText { get; private set; }

        public static m3u8_file_t Parse( string content, Uri baseAddress )
        {
            var lines = from row in content.Split( ['\r', '\n'], StringSplitOptions.RemoveEmptyEntries )
                        let line = row.Trim()
                        where (!line.IsNullOrEmpty() && !line.StartsWith( "#" ))
                        select line
                        ;
            var parts = lines.Select( (line, i) => new m3u8_part_ts( line, i ) );
            var o = new m3u8_file_t()
            {
                Parts       = parts.ToList().AsReadOnly(),
                BaseAddress = baseAddress,
                RawText     = content,
            };
            return (o);
        }
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
        [M(O.AggressiveInlining)] internal static Uri GetPartUrl( this in m3u8_part_ts part, Uri baseAddress ) => baseAddress.GetPartUrl( part.RelativeUrlName );
    }
}