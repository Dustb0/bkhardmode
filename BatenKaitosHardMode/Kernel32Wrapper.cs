using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BatenKaitosHardMode
{
    class Kernel32Wrapper
    {
        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int OpenProcess(int dwDesiredAccess, int bInheritHandle, int dwProcessId);

        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int ReadProcessMemory(int hProcess, long lpBaseAddress, ref int lpBuffer, int nSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int WriteProcessMemory(int hProcess, long lpBaseAddress, ref int lpBuffer, int nSize, ref int lpNumberOfBytesWritten);
    }
}
