using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using TitanReach_Server.Utilities;
using TRShared;
using TRShared.Data.Enums;


namespace TitanReach.Server.Model.ServerObjects
{
    class ObjectFishingSpot : Obj
    {
        public int TotalActiveSpots;
        public int CycleTime;
        public List<Vector3> FishingSpots;
        public List<int> ActiveIndexes;

        public ObjectFishingSpot(int id) : base(id) { }

        public override void SetMetaData(string json) //override
        {
            MetaFishingSpotData metaData = JsonConvert.DeserializeObject<MetaFishingSpotData>(json);
            TotalActiveSpots = metaData.TotalActiveSpots;
            CycleTime = metaData.CycleTime;
            FishingSpots = ToTVec3List(metaData.FishingSpots);
            ActiveIndexes = new List<int>();
        }

        public override void Update1000ms() {
            TitanReach_Server.Server.Log(CycleTime + " ct");
            ActiveIndexes.Clear();
            while(ActiveIndexes.Count < TotalActiveSpots)
            {
                int nextEntry = (int) (TRShared.Data.Formula.rand.NextDouble() * FishingSpots.Count);
                if (!ActiveIndexes.Contains(nextEntry)) ActiveIndexes.Add(nextEntry);
            }
            //UpdateSpawns();
        }


        public Vector3 ToTVec3(System.Numerics.Vector3 sysVec3)
        {
            return new Vector3(sysVec3.X, sysVec3.Y, sysVec3.Z);
        }

        public List<Vector3> ToTVec3List(List<System.Numerics.Vector3> fishingSpots)
        {
            List<Vector3> UVec3s = new List<Vector3>();
            foreach (System.Numerics.Vector3 vec in fishingSpots)
            {
                UVec3s.Add(ToTVec3(vec));
            }
            return UVec3s;
        }

        //public MessageBuffer UpdateSpawns()
        //{

        //}

        public override MessageBuffer WriteBuffer(MessageBuffer buf)
        {
            buf.WriteUInt32((uint)ID);
            buf.WriteUInt32((uint)UID);
            buf.WriteByte((byte)State);
            buf.WriteFloat(transform.position.X);
            buf.WriteFloat(transform.position.Y);
            buf.WriteFloat(transform.position.Z);
            buf.WriteFloat(transform.rotation.X);
            buf.WriteFloat(transform.rotation.Y);
            buf.WriteFloat(transform.rotation.Z);
            buf.WriteFloat(transform.scale.X);
            buf.WriteFloat(transform.scale.Y);
            buf.WriteFloat(transform.scale.Z);

            buf.WriteInt32(FishingSpots.Count);
            foreach (Vector3 pos in FishingSpots)
            {
                buf.WriteFloat(pos.X);
                buf.WriteFloat(pos.Y);
                buf.WriteFloat(pos.Z);
            }
            return buf;
        }

    }

    [System.Serializable]
    public class MetaFishingSpotData
    {
        public int TotalActiveSpots;
        public int CycleTime;
        public List<System.Numerics.Vector3> FishingSpots;

        public MetaFishingSpotData(int totalActiveSpots, int cycleTime, List<Vector3> fishingSpots)
        {
            TotalActiveSpots = totalActiveSpots;
            CycleTime = cycleTime;
            FishingSpots = ToSysVec3List(fishingSpots);

        }

        public System.Numerics.Vector3 ToSysVec3(Vector3 tVec3)
        {
            return new System.Numerics.Vector3(tVec3.X, tVec3.Y, tVec3.Z);
        }

        public List<System.Numerics.Vector3> ToSysVec3List(List<Vector3> fishingSpots)
        {
            List<System.Numerics.Vector3> SysVec3s = new List<System.Numerics.Vector3>();
            foreach (Vector3 vec in fishingSpots)
            {
                SysVec3s.Add(ToSysVec3(vec));
            }
            return SysVec3s;
        }
    }
}
