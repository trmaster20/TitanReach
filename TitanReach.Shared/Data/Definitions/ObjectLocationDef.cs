using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TRShared.Data.Definitions
{
    public class ObjectLocationDef
    {

        public int ObjectID;
        public int LandID;
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
        public string MetaData;

        [Definition("ObjectLocations")]
        public static void Load(DTWrapper dt)
        {
            DataManager.ObjectLocations = new ObjectLocationDef[dt.Count];
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                ObjectLocationDef def = new ObjectLocationDef()
                {
                    ObjectID = dt.Int(0),
                    Position = new Vector3(dt.Float(2), dt.Float(3), dt.Float(4)),
                    Rotation = new Vector3(dt.Float(5), dt.Float(6), dt.Float(7)),
                    Scale = new Vector3(dt.Float(8), dt.Float(9), dt.Float(10)),
                    LandID = dt.Int(11),
                    MetaData = dt.String(12)
                };
                DataManager.ObjectLocations[i] = def;
            }
            dt.Destroy();
            Logger.Log("Loaded " + DataManager.ObjectLocations.Length + " ObjectLocDefs");
        }
    }
}
