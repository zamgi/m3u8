using System;
using System.Diagnostics;
using System.Runtime;
using System.Windows.Input;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class CollectGarbageCommand : ICommand
    {
#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore
        public bool CanExecute( object parameter ) => true;

        public void Execute( object parameter ) => Collect_Garbage( out _ );

        public static void Collect_Garbage( out long totalMemoryBytes )
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            totalMemoryBytes = GC.GetTotalMemory( forceFullCollection: true );

            Debug.WriteLine( $"Collect Garbage. Total Memory: {(totalMemoryBytes / (1024.0 * 1024)):N2} MB." );
        }
    }
}
