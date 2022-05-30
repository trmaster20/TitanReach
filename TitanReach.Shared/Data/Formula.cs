using System;
using System.Collections.Generic;
using System.Text;
using TRShared.Data.Enums;

namespace TRShared.Data
{
    public class Formula
    {

        public static Random rand = new Random(Guid.NewGuid().GetHashCode());


        public static int CombatLevel(int dex, int str, int def, int hp)
        {
            int melleeLevel = (int)(((dex + str + def + hp) - 18.0) * (99.0 / (396.0 - 18.0)) + 1.0);
            int combatLevel = 1;
            if (melleeLevel > combatLevel) combatLevel = melleeLevel;
            if (combatLevel > 99.9) return 100;
            return combatLevel;
        }


        public static int[] StringArrayToInt(string[] p_stringArray)
        {
            int[] intArray = new int[p_stringArray.Length];
            for (int i = 0; i < p_stringArray.Length; i++)
            {
                intArray[i] = int.Parse(p_stringArray[i]);
            }
            return intArray;
        }

        public static DamageType EvaluateDamageType(WeaponCategory wc)
        {
            if (wc == WeaponCategory.GENERICARROW) return DamageType.RANGED;
            if (wc == WeaponCategory.GENERICSPELL) return DamageType.MAGIC;

            {
                return DamageType.MELEE;
            }
        }



    }
}
