using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TRShared.Data.Definitions
{
    public class ItemBonusDef
    {
        public int ItemID = -1;
        public string ItemName = "NoName";
        public int MeleeAB = 0;  //Accuracy Bonus
        public int MeleePB = 0;  //Power Bonus
        public int MeleeDB = 0;  //Defence Bonus
        public int RangedAB = 0;
        public int RangedPB = 0;
        public int RangedDB = 0;
        public int MagicAB = 0;
        public int MagicPB = 0;
        public int MagicDB = 0;
        public string AttackModifiers;
        public string DefenceModifiers;

        [Definition("Bonus")]
        public static void Load(DTWrapper dt)
        {
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                ItemBonusDef def = new ItemBonusDef()
                {
                    ItemID = dt.Int(0),
                    ItemName = dt.String(1),
                    MeleeAB = dt.Int(2),
                    MeleePB = dt.Int(3),
                    MeleeDB = dt.Int(4),
                    RangedAB = dt.Int(5),
                    RangedPB = dt.Int(6),
                    RangedDB = dt.Int(7),
                    MagicAB = dt.Int(8),
                    MagicPB = dt.Int(9),
                    MagicDB = dt.Int(10)
                };
                DataManager.ItemDefinitions[def.ItemID].BonusDef = def;
            }
          
            Logger.Log("Loaded " + dt.Count + " ItemBonusDefs");
            dt.Destroy();
        }
    }
}
