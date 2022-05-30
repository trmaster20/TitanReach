using System;
using System.Collections.Generic;
using System.Text;

namespace TRShared.Data.Definitions
{
    public class PickableDef
    {
        public int ObjectID;
        public int ItemID;
        public int RespawnTime;
        public int PicksBeforeDespawn;
        public float BaseDifficulty;
        public int Exp;
        public int LevelReq;


        [Definition("PickableDef")]
        public static void Load(DTWrapper dt)
        {
            DataManager.PickableDefinitions = new PickableDef[dt.Count];
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                PickableDef def = new PickableDef()
                {
                    ObjectID = dt.Int(0),
                    ItemID = dt.Int(1),
                    RespawnTime = dt.Int(2),
                    PicksBeforeDespawn = dt.Int(3),
                    Exp = dt.Int(4),
                    LevelReq = dt.Int(5)
                };

                DataManager.PickableDefinitions[i] = def;

                DataManager.OnAllDefinitionsLoaded += (p1, p2) =>
                {
                    foreach (PickableDef min in DataManager.PickableDefinitions)
                    {
                        foreach (ObjectDef deff in DataManager.ObjectDefinitions)
                        {
                            if (deff.ID == min.ObjectID)
                            {
                                deff.PickableDef = min;
                                break;
                            }
                        }
                    }
                };
            }
            dt.Destroy();
            Logger.Log("Loaded " + DataManager.PickableDefinitions.Length + " PickableDefs");
        }
    }
}
