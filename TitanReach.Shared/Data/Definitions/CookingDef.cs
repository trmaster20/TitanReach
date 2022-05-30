using System;
using System.Collections.Generic;
using System.Text;
using TRShared.Data.Structs;

namespace TRShared.Data.Definitions
{
    public class CookingDef
    {

        public Ingredient[] Recipe;
        public ushort ResultItemID;
        public int ResultAmount;
        public int Exp;
        public int LevelReq;
        public ushort BurntResultID;
        public string Category;

        [Definition("CookingDef")]
        public static void Load(DTWrapper dt)
        {
            DataManager.CookingDefinitions = new CookingDef[dt.Count];
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                CookingDef def = new CookingDef()
                {
                    Recipe = Ingredient.Prepare(dt.String(1)),
                    ResultItemID = dt.Ushort(2),
                    ResultAmount = dt.Int(3),
                    Exp = dt.Int(4),
                    LevelReq = dt.Int(5),
                    BurntResultID = dt.Ushort(6),
                    Category = dt.String(7)
                };

                DataManager.CookingDefinitions[i] = def;
            }
            dt.Destroy();
            Logger.Log("Loaded " + DataManager.CookingDefinitions.Length + " CookingDefs");
        }



    }
}
