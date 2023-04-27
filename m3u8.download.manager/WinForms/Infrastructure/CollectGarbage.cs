using System;
using System.Diagnostics;
using System.Runtime;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static class CollectGarbage
    {
        public static void Collect_Garbage( out long totalMemoryBytes )
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            totalMemoryBytes = GC.GetTotalMemory( forceFullCollection: true );

            Debug.WriteLine( $"Collect Garbage. Total Memory: {(totalMemoryBytes / (1024.0 * 1024)):N2} MB." );
        }
        public static void GetTotalMemory( out long totalMemoryBytes ) => totalMemoryBytes = GC.GetTotalMemory( forceFullCollection: false );
    }
}
