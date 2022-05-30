using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TRShared.Data.Definitions
{
    public class EatibleDef
    {
        public ushort FoodID;
        public int HealAmount;
        public int Delay;
        public ushort LeftOverID;   //once eaten, what should this item turn into e.g. 4dose->3dose
        public int EffectID;

        [Definition("EatibleDef")]
        public static void Load(DTWrapper dt)
        {
            DataManager.EatibleDefinitions = new EatibleDef[dt.Count];
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                EatibleDef def = new EatibleDef()
                {
                    FoodID = dt.Ushort(1),
                    HealAmount = dt.Int(2),
                    Delay = dt.Int(3),
                    LeftOverID = dt.Ushort(4),
                    EffectID = dt.Int(5)
                };
                DataManager.EatibleDefinitions[i] = def;
                DataManager.ItemDefinitions[def.FoodID].EatibleID = i;
            }
            dt.Destroy();
            Logger.Log("Loaded " + DataManager.EatibleDefinitions.Length + " EatibleDefs");
            
        }
    }
}
