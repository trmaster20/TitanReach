using System;
using System.Collections.Generic;
using System.Text;
using TRShared.Data.Structs;

namespace TRShared.Data.Definitions
{
    public class ForgingDef
    {
        public Ingredient[] Recipe;
        public int ResultItemID;
        public int ResultAmount;
        public int Exp;
        public int LevelReq;
        public int BurntResultID = -1;


        [Definition("ForgingDef")]
        public static void Load(DTWrapper dt)
        {
            DataManager.ForgingDefinitions = new ForgingDef[dt.Count];
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                ForgingDef def = new ForgingDef()
                {
                    Recipe = Ingredient.Prepare(dt.String(1)),
                    ResultItemID = dt.Int(2),
                    ResultAmount = dt.Int(3),
                    Exp = dt.Int(4),
                    LevelReq = dt.Int(5),
                    BurntResultID = dt.Int(6)
                };
              
                DataManager.ForgingDefinitions[i] = def;
            }
            dt.Destroy();
            Logger.Log("Loaded " + DataManager.ForgingDefinitions.Length + " ForgingDefs");
        }
    }
}
