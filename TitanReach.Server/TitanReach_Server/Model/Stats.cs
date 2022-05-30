using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace TitanReach_Server.Model
{
    public class Stats
    {

        

        public enum SKILLS { Dexterity, Strength, Defence, Vitality, Ranged, Sorcery, Occultism, Mining, Fishing, Woodcutting, Cooking, Forging, Metallurgy, Arcana, Survival, Alchemy, Slayer, Artisan };
        public static Dictionary<string, int> SkillNameToId = new Dictionary<string, int>();


        public Player player;

        static Stats()
        {
            GenerateLevelExp();
            for (int i = 0; i < Enum.GetValues(typeof(SKILLS)).Length; i++)
            {
                SkillNameToId.Add(Enum.GetName(typeof(SKILLS), i), i);
            }
        }

        public Stats(Player p) {
            this.player = p;

            skill_exp = new int[Enum.GetValues(typeof(SKILLS)).Length];
            current_level = new int[Enum.GetValues(typeof(SKILLS)).Length];
            max_level = new int[Enum.GetValues(typeof(SKILLS)).Length];

            for (int i = 0; i < Enum.GetValues(typeof(SKILLS)).Length; i++) {
                skill_exp[i] = 0;
                current_level[i] = 1;
                max_level[i] = 1;
            }
        }

        private int[] skill_exp;
        private int[] current_level;
        private int[] max_level;

        public int GetCombatLevel()
        {
            return Formula.CombatLevel(GetMaxLevel(0), GetMaxLevel(1), GetMaxLevel(2), GetMaxLevel(3), GetMaxLevel(4), GetMaxLevel(4));
        }

        public int GetCurLevel(int skill)
        {
            return current_level[skill];
        }

        public int GetSkillTotal()
        {
            int total = 0;
            for (int i = 0; i < max_level.Length; i++)
                total += max_level[i];

            return total;
        }

        public void SetCurLevel(int skill, int val)
        {
            current_level[skill] = val;
        }

        public int GetMaxLevel(int skill)
        {
            return max_level[skill];
        }

        public int GetExp(int skill)
        {
            return skill_exp[skill];
        }

        public void SetLevelAndExp(int skill, int lvl)
        {
            skill_exp[skill] = ConvertLevelToExp(lvl);
            current_level[skill] = lvl;
            max_level[skill] = lvl;
            //  if (skill == (int)SKILLS.Vitality)
            // {
            //    player.CurHealth = player.GetMaxHealth();
            // }
            player.NetworkActions.SyncExp(player, skill);

        }

        public void SetExpAndLevel(int skill, int exp)
        {
            skill_exp[skill] = exp;
            current_level[skill] = ConvertExpToLevel(exp);
            max_level[skill] = ConvertExpToLevel(exp);
        }

        public void AddExp(SKILLS skill, int exp)
        {
            AddExp((int)skill, exp);
        }

        public void AddExp(int skill, int exp)
        {
            int oldCombatLevel = GetCombatLevel();

            int offset = GetCurLevel(skill) - GetMaxLevel(skill);
            if (offset < 0)
                offset = 0;
            exp = (int) Math.Round(exp * 0.65);
            int oldLv = ConvertExpToLevel(skill_exp[skill]);
            skill_exp[skill] += exp;
            int newlv = ConvertExpToLevel(skill_exp[skill]);
            if (newlv > oldLv)
            {
                current_level[skill] = newlv + offset;
                max_level[skill] = newlv;
                var en = (SKILLS)skill;

                if (skill == (int)SKILLS.Vitality)
                {
                    player.CurrentHealth += 10;
                }
                player.NetworkActions.PlaySound("level_up");
                player.NetworkActions.SendMessage("<color=green>Your " + en.ToString() + " level has increased to " + newlv + "!</color>");

            }
            player.NetworkActions.SyncExp(player, skill);

            if (oldCombatLevel != GetCombatLevel()) {
                if (player.HasParty()) {
                    player.Party.UpdatePartyInfo(player);
                }

            }
        }

        public static int[] LEVEL_EXP = null;

        public int ConvertExpToLevel(int exp)
        {
            if (exp >= LEVEL_EXP[99])
            {
                return 100;
            }
            for (int i = 0; i < LEVEL_EXP.Length; i++)
            {
                if (exp >= LEVEL_EXP[i])
                    continue;
                return i;
            }
            return 1;
        }

        public int ConvertLevelToExp(int level)
        {
            if (level <= 1)
                return 0;
            if (level >= 100)
                return LEVEL_EXP[99];

            return LEVEL_EXP[level - 1];
        }

        public static int ConvertExpToLevelTest(int exp)
        {
            if (exp >= LEVEL_EXP[99])
            {
                return 100;
            }
            for (int i = 0; i < LEVEL_EXP.Length; i++)
            {
                if (exp >= LEVEL_EXP[i])
                    continue;
                return i;
            }
            return 1;
        }
        public static void LevelExpUnitTest()
        {
            Test(10000000, 100);
            Test(10000001, 100);
            Test(9999999, 99);
            Test(9114158, 99);
            Test(9114156, 98);
            Test(9000000, 98);
            Test(8306778, 98);
            Test(8306779, 98);
            Test(8306777, 97);
            Test(99, 1);
            Test(100, 2);
            Test(101, 2);
            Test(95818, 50);
            Test(95817, 49);

        }

        public static void Test(int xp, int expectedLevel)
        {
            int level = ConvertExpToLevelTest(xp);
            string successString = level == expectedLevel ? "Pass" : "Fail";
            Server.Log(successString + "! Testing XP: " + xp + "  expected lvl: " + expectedLevel + "  got level: " + level);
        }
        public static void GenerateLevelExp()
        {
            double xpcount = 0.0;
            double initialIncrement = 100;
            double multiplyer = 1.097183204;
            double increment = initialIncrement;
            int maxXP = 10000000;
            LEVEL_EXP = new int[100];
            LEVEL_EXP[0] = 0;
            for (int level = 1; level < 100; level++) {
                xpcount += increment;
                LEVEL_EXP[level] = (int)xpcount;
                increment = increment * multiplyer;
                //Server.Log(level + " " + xpcount);
            }
            if (LEVEL_EXP[99] != maxXP) Server.Error("Level 100 is not set correctly");
        }
    }
}
