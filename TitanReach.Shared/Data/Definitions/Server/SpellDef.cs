using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TRShared.Data.Enums;
using TRShared.Data.Structs;

namespace TRShared.Data.Definitions
{
    public class SpellDef
    {

        public int ID;
        public string Name;
        public SpellType SpellType;
        public Ingredient[] Ingredients;
        public int LevelRequirement;
        public int XP;

        [Definition("SpellDef")]
        public static void Load(DTWrapper dt)
        {
            DataManager.SpellDefinitions = new SpellDef[dt.Count];
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                SpellDef def = new SpellDef()
                {
                    ID = dt.Int(0),
                    Name = dt.String(1),
                    Ingredients = Ingredient.Prepare(dt.String(3)),
                    SpellType = SpellType.Teleport,
                    LevelRequirement = dt.Int(4),
                    XP = dt.Int(5)
                };
                DataManager.SpellDefinitions[i] = def;
            }
            dt.Destroy();
            Logger.Log("Loaded " + DataManager.SpellDefinitions.Length + " SpellDefs");
        }
    }
}
