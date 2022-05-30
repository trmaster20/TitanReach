using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TRShared.Data.Definitions
{
    public class FishingDef
    {

        public int ObjectID;
        public ushort FishID;
        public int Exp;
        public float Difficulty;
        public float Size;
        public int LevelReq;
        public ushort BaitID = 0;
        public ushort ToolID = 0;

        [Definition("FishingDef")]
        public static void Load(DTWrapper dt)
        {
            
            DataManager.FishingDefinitions = new FishingDef[dt.Count];
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                FishingDef def = new FishingDef()
                {
                    ObjectID = dt.Int(0),
                    FishID = dt.Ushort(1),
                    Difficulty = dt.Float(2),
                    Exp = dt.Int(5),
                    LevelReq = dt.Int(6),
                    ToolID =  dt.Ushort(7),
                    BaitID =  dt.Ushort(8)
                };

                DataManager.FishingDefinitions[i] = def;
            }
            DataManager.OnAllDefinitionsLoaded += (sender, obj) => {
                foreach (FishingDef min in DataManager.FishingDefinitions)
                {
                    foreach (ObjectDef deff in DataManager.ObjectDefinitions)
                    {
                        if (deff.ID == min.ObjectID)
                        {
                            deff.FishDef = min;
                            break;
                        }
                    }
                }
            };
            dt.Destroy();
            Logger.Log("Loaded " + DataManager.FishingDefinitions.Length + " FishingDefs");
            
        }
    }
}
