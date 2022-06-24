using System;
using System.Runtime.InteropServices;

namespace m3u8.download.manager.ipc
{
    /// <summary>
    /// 
    /// </summary>
    internal static class ProcessCreator
    {
        public static bool CreateAsBreakawayFromJob( string commandLine )
        {
            var si = new STARTUPINFO() { cb = Marshal.SizeOf(typeof(STARTUPINFO)) };

            var r = CreateProcess( null, //Process.GetCurrentProcess().MainModule.FileName, 
                                   commandLine,
                                   IntPtr.Zero,
                                   IntPtr.Zero,
                                   false, 
                                   CREATE_BREAKAWAY_FROM_JOB, 
                                   IntPtr.Zero, 
                                   null, //Environment.CurrentDirectory, 
                                   ref si, 
                                   out var processInformation );
            return (r);
        }


        private const uint CREATE_BREAKAWAY_FROM_JOB = 0x01000000;

        #region comm
        /*[StructLayout(LayoutKind.Sequential)] private struct SECURITY_ATTRIBUTES
        {
            public int    nLength;
            public IntPtr lpSecurityDescriptor;
            public int    bInheritHandle;
        }*/ 
        #endregion

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION 
        {
           public IntPtr hProcess;
           public IntPtr hThread;
           public int    dwProcessId;
           public int    dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        private struct STARTUPINFO
        {
             public int    cb;
             public string lpReserved;
             public string lpDesktop;
             public string lpTitle;
             public int    dwX;
             public int    dwY;
             public int    dwXSize;
             public int    dwYSize;
             public int    dwXCountChars;
             public int    dwYCountChars;
             public int    dwFillAttribute;
             public int    dwFlags;
             public short  wShowWindow;
             public short  cbReserved2;
             public IntPtr lpReserved2;
             public IntPtr hStdInput;
             public IntPtr hStdOutput;
             public IntPtr hStdError;
        }

        [DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Auto)]
        private static extern bool CreateProcess( string lpApplicationName,
                                                  string lpCommandLine,
                                                  IntPtr lpProcessAttributes, //ref SECURITY_ATTRIBUTES lpProcessAttributes,
                                                  IntPtr lpThreadAttributes, //ref SECURITY_ATTRIBUTES lpThreadAttributes,
                                                  bool bInheritHandles,
                                                  uint dwCreationFlags,
                                                  IntPtr lpEnvironment,
                                                  string lpCurrentDirectory,
                                                  [In] ref STARTUPINFO lpStartupInfo,
                                                  out PROCESS_INFORMATION lpProcessInformation );
    }
}
