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

    class MemoryManipulator
    {
        const long BATEN_KAITOS_NOA_GAME_ID = 1196114501L;
        const long DOLPHIN_5_OFFSET1 = (long)int.MinValue;
        const long DOLPHIN_5_OFFSET2 = 2147483648L;

        // Addresses
        private readonly long[] PARTY_HP_ADDRESSES = { 2149817558L };
        private readonly long[] ENEMY_HP_ADDRESSES = { 2156529616L, 2156553704L };

        private Process dolphinProcess;
        private IntPtr dolphinProcessHandle;
        private int hProcess = 0;
        private long battleFlags;
        private long enemyID;
        private int battlerCount;

        public MemoryManipulator()
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

        #region " Memory Reading "

        private int Read32(long Address, int nsize = 4)
        {
            long lpBaseAddress = Address;

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

        private int Read16(long Address, int nsize = 2)
        {
            long lpBaseAddress = Address;
            int num1 = 0;
            ref int local1 = ref num1;
            
            int nSize = nsize;
            int num2 = 0;
            ref int local2 = ref num2;

            Kernel32Wrapper.ReadProcessMemory(hProcess, lpBaseAddress, ref local1, nSize, ref local2);
            byte[] bytes = BitConverter.GetBytes(num1);
            Array.Reverse(bytes);
            return checked((int)Math.Round(unchecked((double)BitConverter.ToInt32(bytes, 0) / 65536.0)));
        }

        public int Read8(long Address)
        {
            int buffer = 0;
            int numOfBytesWritten = 0;
            Kernel32Wrapper.ReadProcessMemory(hProcess, Address, ref buffer, 1, ref numOfBytesWritten);
            return buffer;
        }


        private void Write32(long Address, int value, int nsize = 4)
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

        private void Write16(long Address, int value)
        {
            int numOfBytesWritten = 0;
            byte[] bytes = BitConverter.GetBytes(value);
            value = bytes[0] * 256 + bytes[1];
            Kernel32Wrapper.WriteProcessMemory(hProcess, Address, ref value, 2, ref numOfBytesWritten);
        }


        #endregion

        #region " Game Functions "

        public bool IsDolphinRunning()
        {
            FetchDolphin();
            return dolphinProcess != null && dolphinProcessHandle != IntPtr.Zero;
        }

        public bool IsBKNARunning()
        {
            long gameID = Read32(checked(4294901760L + DOLPHIN_5_OFFSET1));
            return gameID == BATEN_KAITOS_NOA_GAME_ID;
        }

        public void ReadBattleFlags()
        {
            battlerCount = Read16(checked(battleFlags + 217510L), 2);
            battleFlags = checked(Read32(4302497412L + DOLPHIN_5_OFFSET1, 4) + 6442385408L - DOLPHIN_5_OFFSET2);
            enemyID = Read16(checked(4297339562L + DOLPHIN_5_OFFSET1), 2);
        }

        public bool IsInBattle()
        {
            // ??
            if (battleFlags == checked(6442385408L - DOLPHIN_5_OFFSET2) || Read32(checked(battleFlags + 32L), 4) == -17958194)
            {
                return false;
            }

            // Check matching battles (shadow wizard & ice queen)
            if (enemyID == 164 || enemyID == 86)
            {
                return false;
            }

            return true;
        }

        public int GetBattlerCount()
        {
            return battlerCount;
        }

        public bool AreCharsBattleReady()
        {
            return (battlerCount >= 1 && battlerCount <= 3);
        }

        public string GetEnemyID()
        {
            return enemyID.ToString();
        }

        public long GetPartyMapHP(PartyMembers index)
        {
            return Read32(checked(PARTY_HP_ADDRESSES[(int) index]));
        }

        public long GetPartyMapMaxHP(int index)
        {
            return Read16(0x080239CDA + (index * 68));
        }

        public long GetPartyBattleHP(int index)
        {
            long battleOffset = index * 24088;
            return Read16(checked(battleFlags + 94686L + battleOffset));
        }

        public long GetPartyBattleMaxHP(int index)
        {
            long battleOffset = (int)index * 24088;
            int baseHp =  Read16(checked(battleFlags + 94634L + battleOffset));
            
            // Check for equipment bonus
            int equipment = Read16(0x100239D04 + index * 68);
            int equipBonus = 0;
            if (equipment > 0)
            {
                equipBonus = Read8(0x1001AC8EE + 108 * equipment);
            }

            // Check for deluxe pastry bonus
            int deluxepastryHP = Read16(0x100239CD + index * 68);

            return (long)Math.Floor((baseHp + deluxepastryHP) * (1 + equipBonus * 0.01));
        }

        public void SetPartyMapHP(PartyMembers index, int value)
        {
            if (PARTY_HP_ADDRESSES.Length > (int) index)
            {
                Write32(checked(PARTY_HP_ADDRESSES[(int)index]), value);
            }
        }

        public void SetPartyBattleHP(int index, int value)
        {
            long battleOffset = index * 24088;
            long address = battleFlags + 94686L + battleOffset;
            Write16(checked(address), value);
        }

        public void SetPartyBattleMaxHP(int index, int value)
        {
            long battleOffset = index * 24088;
            long address = battleFlags + 94634L + battleOffset;
            Write16(checked(address), value);
        }

        public long GetEnemyHP(int index)
        {
            return Read32(checked(ENEMY_HP_ADDRESSES[index]));
        }

        public void SetEnemyHP(int index, int value)
        {
            Write32(checked(ENEMY_HP_ADDRESSES[index]), value);
        }

        #endregion

    }
}