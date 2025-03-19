namespace System.Windows.Forms.Taskbar
{
    /// <summary>A Serch Connector folder in the Shell Namespace</summary>
    public sealed class ShellSearchConnector : ShellSearchCollection
    {
        internal ShellSearchConnector() => CoreHelpers.ThrowIfNotWin7();
        internal ShellSearchConnector( IShellItem2 shellItem ) : this() => _NativeShellItem = shellItem;

        /// <summary>Indicates whether this feature is supported on the current platform.</summary>
        public new static bool IsPlatformSupported =>
                // We need Windows 7 onwards ...
                CoreHelpers.RunningOnWin7;
    }
}