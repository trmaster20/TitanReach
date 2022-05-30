using System;
using System.Collections.Generic;
using System.Text;

namespace TRShared.Data.Definitions
{
    public class MiningDef
    {
        public int RockID;
        public int OreID;
        public int RespawnTime;
        public int Exp;
        public int LevelReq;
        public float BaseChance;
        public float LevelScale;
        public ushort RareItemID;
        public float RareItemChance;

        [Definition("MiningDef")]
        public static void Load(DTWrapper dt)
        {
            DataManager.MiningDefinitions = new MiningDef[dt.Count];
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                MiningDef def = new MiningDef()
                {
                    RockID = dt.Int(0),
                    OreID = dt.Int(1),
                    RespawnTime = dt.Int(2),
                    BaseChance = dt.Float(3),
                    LevelScale = dt.Float(4),
                    Exp = dt.Int(5),
                    LevelReq = dt.Int(6),
                    RareItemID = dt.Ushort(7),
                    RareItemChance = dt.Float(8)
                };
                DataManager.MiningDefinitions[i] = def;
            }
            dt.Destroy();
            DataManager.OnAllDefinitionsLoaded += (sender, obj) => {                
                foreach (MiningDef min in DataManager.MiningDefinitions)
                {
                    foreach (ObjectDef deff in DataManager.ObjectDefinitions)
                    {
                        if (deff.ID == min.RockID)
                        {
                            deff.MiningDef = min;
                            break;
                        }
                    }
                }
            };

            Logger.Log("Loaded " + DataManager.MiningDefinitions.Length + " MiningDefs");
        }

    }
}
