using Avalonia;
using Avalonia.Platform;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static class PlatformHelper
    {
        public static OperatingSystemType GetOperatingSystemType() => AvaloniaLocator.Current.GetService< IRuntimePlatform >().GetRuntimeInfo().OperatingSystem;
        public static bool IsWinNT() => (GetOperatingSystemType() == OperatingSystemType.WinNT);
    }
}
