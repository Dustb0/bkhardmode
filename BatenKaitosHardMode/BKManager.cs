using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatenKaitosHardMode
{
    class BKManager : MemoryManager
    {
        const long BATEN_KAITOS_NOA_GAME_ID = 1196114501L;
        const long CHAR_DATA_OFFSET = 68; // Memory-offset between char data
        const long CHAR_MAP_HP_START = 0x080239CDA;
        const long CHAR_MAP_MAXHP_START = 0x080239CDA;

        private long battleFlags;
        private long enemyID;
        private int battlerCount;

        #region " Base Process Handling "

        public bool IsBKNARunning()
        {
            long gameID = Read32(checked(4294901760L + DOLPHIN_5_OFFSET1));
            return gameID == BATEN_KAITOS_NOA_GAME_ID;
        }

        #endregion

        #region " Map Functions "

        public long GetCharMapHP(int index)
        {
            return Read16(CHAR_MAP_HP_START + (index * CHAR_DATA_OFFSET));
        }

        public long GetCharMapMaxHP(int index)
        {
            return Read16(CHAR_MAP_MAXHP_START + (index * CHAR_DATA_OFFSET));
        }

        public void SetCharMapHP(int index, int value)
        {
            Write16(CHAR_MAP_HP_START + (index * CHAR_DATA_OFFSET), value);
        }

        public void SetCharMapMaxHP(int index, int value)
        {
            Write16(CHAR_MAP_MAXHP_START + (index * CHAR_DATA_OFFSET), value);
        }

        #endregion

        #region " Battle Functions "

        public void ReadBattleFlags()
        {
            battlerCount = Read16(checked(battleFlags + 217510L));
            battleFlags = checked(Read32(4302497412L + DOLPHIN_5_OFFSET1) + 6442385408L - DOLPHIN_5_OFFSET2);
            enemyID = Read16(checked(4297339562L + DOLPHIN_5_OFFSET1));
        }

        public bool IsInBattle()
        {
            // ??
            if (battleFlags == checked(6442385408L - DOLPHIN_5_OFFSET2) || Read32(checked(battleFlags + 32L)) == -17958194)
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

        public long GetCharBattleHP(int index)
        {
            long battleOffset = index * 24088;
            return Read16(checked(battleFlags + 94686L + battleOffset));
        }

        public long GetCharBattleMaxHP(int index)
        {
            long battleOffset = (int)index * 24088;
            int baseHp = Read16(checked(battleFlags + 94634L + battleOffset));

            // Check for equipment bonus
            int equipment = Read16(0x100239D04 + DOLPHIN_5_OFFSET2 + index * CHAR_DATA_OFFSET);
            int equipBonus = 0;
            if (equipment > 0)
            {
                equipBonus = Read8(0x1001AC8EE + 108 * equipment);
            }

            // Check for deluxe pastry bonus
            int deluxepastryHP = Read16(0x100239CD + DOLPHIN_5_OFFSET2 + index * CHAR_DATA_OFFSET);

            return (long)Math.Floor((baseHp + deluxepastryHP) * (1 + equipBonus * 0.01));
        }

        public void SetCharBattleHP(int index, int value)
        {
            long battleOffset = index * 24088;
            long address = battleFlags + 94686L + battleOffset;
            Write16(checked(address), value);
        }

        public void SetCharBattleMaxHP(int index, int value)
        {
            long battleOffset = index * 24088;
            long address = battleFlags + 94634L + battleOffset;
            Write16(checked(address), value);
        }

        #endregion

    }
}
