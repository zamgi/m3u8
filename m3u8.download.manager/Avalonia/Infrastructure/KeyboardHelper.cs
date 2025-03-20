using System.Runtime.InteropServices;

//using Avalonia.Platform;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static class KeyboardHelper
    {
        public static bool? IsShiftButtonPushed()
        {
            switch ( PlatformHelper.GetOperatingSystemType() )
            {
                case PlatformHelper.OperatingSystemType_CUSTOM_.WinNT: return (WinNT.IsShiftButtonPushed());
                //case OperatingSystemType.WinNT: return (WinNT.IsShiftButtonPushed());

                default:
                    return (null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        unsafe private static class WinNT
        {
            private const string USER32_DLL = "user32.dll";

            [DllImport(USER32_DLL)] private static extern bool GetKeyboardState( /*byte[]*/byte* lpKeyState );
            public static bool IsShiftButtonPushed()
            {
                const int VK_SHIFT  = 0x10;
                const int VK_LSHIFT = 0xA0;
                const int VK_RSHIFT = 0xA1;

                var keyState = stackalloc /*new*/ byte[ 256 ];
                if ( GetKeyboardState( keyState ) )
                {
                    return (1 < keyState[ VK_SHIFT ] || 1 < keyState[ VK_LSHIFT ] || 1 < keyState[ VK_RSHIFT ]);
                }
                return (false);
            }
        }
    }

}
