using System;
using System.Collections.Generic;
using System.Text;
using TitanReach_Server.Model;
//using System.Text.Json;
using TitanReach_Server;
using Newtonsoft.Json;

namespace TitanReach.Server.Model.ServerObjects
{
    public class ObjectPortal : Obj
    {
        public int LandID; //where the player is teleported to
        public Vector3 SpawnPosition;

        public ObjectPortal(int id) : base(id) { }

        public override void SetMetaData(string json) //override
        {
            MetaPortalData metaData = JsonConvert.DeserializeObject<MetaPortalData>(json);
            //MetaPortalData metaData = JsonSerializer.Deserialize<MetaPortalData>(json);
            LandID = metaData.LandID;
            SpawnPosition = ToTVec3(metaData.SpawnPosition);
        }

        public override void Update1000ms()
        {

        }

        public Vector3 ToTVec3(System.Numerics.Vector3 sysVec3)
        {
            return new Vector3(sysVec3.X, sysVec3.Y, sysVec3.Z);
        }

    }

    [System.Serializable]
    public class MetaPortalData
    {
        public int LandID;
        public System.Numerics.Vector3 SpawnPosition;

        public MetaPortalData(int LandID, System.Numerics.Vector3 SpawnPosition)
        {
            this.LandID = LandID;
            //SpawnPosition = ToSysVec3(spawnPosition);
            this.SpawnPosition = SpawnPosition;
        }


        public System.Numerics.Vector3 ToSysVec3(Vector3 uVec3)
        {
            return new System.Numerics.Vector3(uVec3.X, uVec3.Y, uVec3.Z);
        }
    }
}
