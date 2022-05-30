using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TRShared.Data.Enums;
using System.Linq;

namespace TRShared.Data.Definitions
{


    public class BuffDef
    {

        public int ID;
        public string Name;
        public string Description;
        public string SpellPath;
        public bool Stackable;
        public int TickRate = -1;

        public object Untyped_Sprite;
        public object Untyped_SpellObject;
        public BuffType SpellType;

        [Definition("BuffDef")]
        public static void Load(DTWrapper dt)
        {
            DataManager.BuffDefinitions = new BuffDef[dt.Count];
            for (int idx = 0; idx < dt.Count; idx++)
            {
                dt.Row = idx;
                BuffDef def = new BuffDef()
                {
                    ID = dt.Int(0),
                    Name = dt.String(1),
                    SpellType = (BuffType)dt.Int(2),
                    SpellPath = dt.String(4),
                    Description = dt.String(5),
                    Stackable = dt.Int(6) == 1,
                    Untyped_Sprite = dt.String(3),
                    TickRate = dt.Int(7)
                    
                };
                DataManager.BuffDefinitions[idx] = def;
            }
            dt.Destroy();
            Logger.Log("Loaded " + DataManager.BuffDefinitions.Length + " BuffDefs");
        }
        

    }
}
