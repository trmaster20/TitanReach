using System;
using System.Collections.Generic;
using System.Text;

namespace TRShared.Data.Definitions
{
    public class WoodcuttingDef
    {
        public int TreeID;
        public int LogID;
        public int RespawnTime;
        public float BaseSize;
        public float BaseDifficulty;
        public int Exp;
        public int LevelReq;


        [Definition("WoodcuttingDef")]
        public static void Load(DTWrapper dt)
        {
            DataManager.WoodcuttingDefinitions = new WoodcuttingDef[dt.Count];
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                WoodcuttingDef def = new WoodcuttingDef()
                {
                    TreeID = dt.Int(0),
                    LogID = dt.Int(1),
                    RespawnTime = dt.Int(2),
                    BaseSize = dt.Float(3),
                    BaseDifficulty = dt.Float(4),
                    Exp = dt.Int(5),
                    LevelReq = dt.Int(6)
                };
                DataManager.WoodcuttingDefinitions[i] = def;

                DataManager.OnAllDefinitionsLoaded += (sender, obj) => {

 
                };
            }
            dt.Destroy();
            foreach (WoodcuttingDef min in DataManager.WoodcuttingDefinitions)
            {
                foreach (ObjectDef deff in DataManager.ObjectDefinitions)
                {
               
                    if (deff.ID == min.TreeID)
                    {
                        deff.WoodcuttingDef = min;
                        break;
                    }
                }
            }
            Logger.Log("Loaded " + DataManager.WoodcuttingDefinitions.Length + " WoodcuttingDefs");
        }

    }
}
