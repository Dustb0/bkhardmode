using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatenKaitosHardMode
{
    public enum PartyMembers
    {
        Kalas = 0
    }

    class MemoryManager
    {
        protected const long DOLPHIN_5_OFFSET1 = (long)int.MinValue;
        protected const long DOLPHIN_5_OFFSET2 = 2147483648L;

        private Process dolphinProcess;
        private IntPtr dolphinProcessHandle;
        private int hProcess = 0;

        #region " Base Process Handling "

        public MemoryManager()
        {
            // Grab dolphin process
            FetchDolphin();
        }

        private void FetchDolphin()
        {
            dolphinProcess = Process.GetProcessesByName("Dolphin").FirstOrDefault();

            if (dolphinProcess != null)
            {
                dolphinProcessHandle = (IntPtr) Kernel32Wrapper.OpenProcess(127231, 0, dolphinProcess.Id);
                hProcess = (int)dolphinProcessHandle;
            }
        }

        public bool IsDolphinRunning()
        {
            FetchDolphin();
            return dolphinProcess != null && dolphinProcessHandle != IntPtr.Zero;
        }

        #endregion

        #region " Memory Reading "

        protected int Read32(long Address)
        {
            long lpBaseAddress = Address;

            int nsize = 4;
            int num1 = 0;
            ref int local1 = ref num1;

            int nSize = nsize;
            int num2 = 0;
            ref int local2 = ref num2;

            Kernel32Wrapper.ReadProcessMemory(hProcess, lpBaseAddress, ref local1, nSize, ref local2);
            byte[] bytes = BitConverter.GetBytes(num1);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        protected int Read16(long Address)
        {
            long lpBaseAddress = Address;
            int num1 = 0;
            ref int local1 = ref num1;
            
            int nSize = 2;
            int num2 = 0;
            ref int local2 = ref num2;

            Kernel32Wrapper.ReadProcessMemory(hProcess, lpBaseAddress, ref local1, nSize, ref local2);
            byte[] bytes = BitConverter.GetBytes(num1);
            Array.Reverse(bytes);
            return checked((int)Math.Round(unchecked((double)BitConverter.ToInt32(bytes, 0) / 65536.0)));
        }

        protected int Read8(long Address)
        {
            int buffer = 0;
            int numOfBytesWritten = 0;
            Kernel32Wrapper.ReadProcessMemory(hProcess, Address, ref buffer, 1, ref numOfBytesWritten);
            return buffer;
        }

        #endregion

        #region " Memory Writing "

        protected void Write32(long Address, int value, int nsize = 4)
        {
            long lpBaseAddress = Address;

            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);

            int num1 = BitConverter.ToInt32(bytes, 0);
            ref int local1 = ref num1;

            int nSize = nsize;
            int num2 = 0;
            ref int local2 = ref num2;

            Kernel32Wrapper.WriteProcessMemory(hProcess, lpBaseAddress, ref num1, nSize, ref local2);
        }

        protected void Write16(long Address, int value)
        {
            int numOfBytesWritten = 0;
            byte[] bytes = BitConverter.GetBytes(value);
            value = bytes[0] * 256 + bytes[1];
            Kernel32Wrapper.WriteProcessMemory(hProcess, Address, ref value, 2, ref numOfBytesWritten);
        }

        protected void Write8(long Address, int value)
        {
            int numOfBytesWritten = 0;
            byte[] bytes = BitConverter.GetBytes(value);
            value = bytes[0] * 8 + bytes[1];
            Kernel32Wrapper.WriteProcessMemory(hProcess, Address, ref value, 1, ref numOfBytesWritten);
        }

        #endregion

    }
}