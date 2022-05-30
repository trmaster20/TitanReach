using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TRShared.Data.Definitions
{
    public class TeleportLocationDef
    {
        public int ID;
        public string Name;
        public float X;
        public float Z;
        public float Radius;

        [Definition("TeleportLocations")]
        public static void Load(DTWrapper dt)
        {
            Logger.Log("Starting");
            DataManager.TeleportLocations = new TeleportLocationDef[dt.Count];
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                TeleportLocationDef def = new TeleportLocationDef()
                {
                    ID = dt.Int(0),
                    Name = dt.String(1),
                    X = dt.Float(2),
                    Z = dt.Float(3),
                    Radius = dt.Float(4)
                };
                DataManager.TeleportLocations[i] = def;
            }
            dt.Destroy();
            Logger.Log("Loaded " + DataManager.TeleportLocations.Length + " TeleportLocations");
        }
    }
}
