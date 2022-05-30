using System;
using System.Collections.Generic;
using System.Text;

namespace TRShared.Data
{
    public static class LevelXPEvaluater
    {
        public static int[] LEVEL_EXP = null;

        static LevelXPEvaluater()
        {
            GenerateLevelExp();
        }

        //public static void Load()
        //{
        //    GenerateLevelExp();
        //}

        public static int ConvertExpToLevel(int exp)
        {
            //if (exp < LEVEL_EXP[0])
            //   return 1;
            if (exp >= LEVEL_EXP[99])
                return 100;
            for (int i = 0; i < LEVEL_EXP.Length - 1; i++)
            {
                if (exp >= LEVEL_EXP[i])
                    continue;
                return i - 1;
            }
            return 1;
        }

        public static int ConvertLevelToExp(int level)
        {
            if (level <= 1)
                return 0;
            if (level >= 100)
                return LEVEL_EXP[99];

            return LEVEL_EXP[level - 1];
        }

        public static void GenerateLevelExp()
        {
            var xpcount = 0.0;
            var initialIncrement = 100;
            double increment = initialIncrement;
            double prevXP = 0;
            LEVEL_EXP = new int[102];
            LEVEL_EXP[0] = 0;
            // Console.WriteLine("Level: 1" + ": " + xpcount);
            for (int level = 1; level <= 99; level++)
            {

                xpcount += increment;
                LEVEL_EXP[level + 1] = (int)xpcount;
                // Console.WriteLine("Level: " + (level + 1) + ": " + xpcount);
                increment = increment * 1.097183210;
                prevXP = xpcount;
            }
        }
    }
}
