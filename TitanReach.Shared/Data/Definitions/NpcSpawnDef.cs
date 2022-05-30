using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using TRShared.Data.Structs;

namespace TRShared.Data.Definitions
{



    public class NpcSpawnDef
    {
        public string nameRef = "Unknown";
        public int ID; // npc id
        public int LandID = 0;
        public Vector3 Position;
        public float Direction = 0;
        public int WanderType;
        public float RangeWidth;
        public float RangeHeight;
        public float RangeRotation;
        public bool SpawnInCenter;
        public bool ForceStationary;
        //public float X;
        //public float Y;
        //public float Z;
        //public float Radius;
        public int Amount; // amount to spawn
        public int RespawnTime = 2500;
        public bool SpawnedAtRuntime = false;
        public bool CanMove = true;
        public string tempSpawnName;
        public int IdleAnimation = 0;

        [Definition("NpcSpawns")]
        public static void Load(DTWrapper dt)
        {
            DataManager.NpcSpawnDefinitions = new List<NpcSpawnDef>();
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;

                var row = dt.Row;
                int amount = 1;
                string name = dt.String(0);
                int npcid = dt.Int(1);
                int land = dt.Int(2);

                float xx = dt.Float(3);
                float yy = dt.Float(4);
                float zz = dt.Float(5);
                float dir = dt.Float(6);
                int boundsType = dt.Int(7);
                float rangeWidth = dt.Float(8);
                float rangeHeight = dt.Float(9);
                float rangeRotation = dt.Float(10);
                int spawnInCenter = dt.Int(11);
                int forceStationary = dt.Int(12);
                int idleAnimation = dt.Int(13);

                // why is any of this here is not used   2/06

                //float x = xx;
                //float z = zz;
                //
                //if (!DataManager.NpcDefinitions[npcid].Friendly) 
                //{
                //    x += Formula.rand.Next(-dist, dist);
                //    z += Formula.rand.Next(-dist, dist);
                //}
                //
                //float y = DataManager.GetHeight((int)x, (int)z, 0) + 0.7f; // change 0 to the loaded LandID
                //if (y == float.MaxValue)
                //{
                //    Logger.Log("BROKEN COORDS: ID: " + npcid + " - " + xx + " - " + zz);
                //}
                //if (yy != -1) y = yy;

                NpcSpawnDef npcSpawn = new NpcSpawnDef();
                npcSpawn.nameRef = name;
                npcSpawn.ID = npcid;
                npcSpawn.LandID = land;

                //npcSpawn.X = xx;
                //npcSpawn.Z = zz;
                //npcSpawn.Y = yy;
                //npcSpawn.Radius = dist;
                npcSpawn.Position = new Vector3(xx, yy, zz);
                npcSpawn.Direction = dir;

                npcSpawn.WanderType = boundsType;
                npcSpawn.RangeWidth = rangeWidth;
                npcSpawn.RangeHeight = rangeHeight;
                npcSpawn.RangeRotation = rangeRotation;
                npcSpawn.SpawnInCenter = spawnInCenter == 1;
                npcSpawn.ForceStationary = forceStationary == 1;

                npcSpawn.Amount = amount;

                //npcSpawn.CanMove = dist != 0;
                npcSpawn.CanMove = (rangeWidth == 0 && rangeHeight == 0) || npcSpawn.ForceStationary;
                npcSpawn.IdleAnimation = idleAnimation;

                DataManager.NpcSpawnDefinitions.Add(npcSpawn);
            }
            dt.Destroy();
            Logger.Log("Loaded " + DataManager.ForgingDefinitions.Length + " NPCSpawnDefs");
        }
    }
}
