using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TRShared.Data.Definitions
{
    public class LandDef
    {
        public int LandID;
        public string Name = "";
        public string ScenePath;

        public List<NpcSpawnDef> NpcSpawnDefinitions = new List<NpcSpawnDef>();
        public List<ObjectLocationDef> ObjectLocationsDefinitions = new List<ObjectLocationDef>();

        private bool _loaded = false;
        public int HEIGHT_MAP_RESOLUTION = 10000;
        public int MAP_RESOLUTION = 1024;
        public int MAP_MESH_SIZE = 1024;
        public int MAP_MESH_HEIGHT = 1024;
        public float HEIGHTMAP_OFFSET = 250;

        public bool POSITIVE_CHUNKS = true;

        public float[][] HEIGHT_MAP;

        public void LoadDefs()
        {
            if (_loaded)
                return;
            Logger.Log("Loading LandID : " + LandID);
            List<NpcSpawnDef> npcDefs = DataManager.NpcSpawnDefinitions.Where(n => n.LandID == LandID).ToList();
            if (npcDefs != null)
                NpcSpawnDefinitions.AddRange(npcDefs);

            List<ObjectLocationDef> objDefs = DataManager.ObjectLocations.Where(o => o.LandID == LandID).ToList();
            if (objDefs != null)
                ObjectLocationsDefinitions.AddRange(objDefs);


            _loaded = true;
        }

        public LandDef()
        {
            HEIGHT_MAP = new float[HEIGHT_MAP_RESOLUTION + 1][];
        }

        [Definition("LandDef")]
        public static void Load(DTWrapper dt)
        {
            DataManager.LandDefinitions = new LandDef[dt.Count];
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                LandDef def = new LandDef()
                {
                    LandID = dt.Int(0),
                    Name = dt.String(1),
                    HEIGHT_MAP_RESOLUTION = dt.Int(2),
                    MAP_RESOLUTION = dt.Int(3),
                    MAP_MESH_SIZE = dt.Int(4),
                    MAP_MESH_HEIGHT = dt.Int(5),
                    HEIGHTMAP_OFFSET = dt.Float(6),
                    POSITIVE_CHUNKS = dt.Int(7) == 1,
                    ScenePath = dt.String(8)
                };
                DataManager.LandDefinitions[i] = def;
            }
            dt.Destroy();
            Logger.Log("Loaded " + DataManager.LandDefinitions.Length + " LandDefs");
            if(DataManager.IS_SERVER)
                DataManager.LoadHeightMap();
        }
    }
}
