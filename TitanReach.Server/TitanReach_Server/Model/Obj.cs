using System;
using TitanReach_Server.Network.Assets.Core.Network;
using TRShared;
using TRShared.Data.Definitions;

namespace TitanReach_Server.Model
{
    public class Obj
    {
        public int MapID = 0;
        public Map Map => Server.Instance.Maps[MapID];
        static int OBJ_COUNTER = 5000;
        public int UID;
        public int ID;
        public bool Depleted = false;
        public int DeathTime = -1;
        public bool NeedsRemove = false;
        public int PickableRemaining = -1;
        public ObjectLocationDef LocationDef;
        public int State = 0;



        public Transform transform;
        public ObjectDef Definition
        {
            get
            {
                return DataManager.ObjectDefinitions[ID];
            }
        }


        public Obj(int id)
        {
            ID = id;
            UID = OBJ_COUNTER;
            OBJ_COUNTER++;
            transform = new Transform(Vector3.Zero(), Vector3.Zero(), Vector3.Zero());
        }

        public virtual string GetMetaDataJson() => "";
        public virtual void SetMetaData(string json) { }

        public virtual void Update1000ms() {
        }

        public void Respawn()
        {
            //  Server.Log("Respawning Object");
            Depleted = false;
            DeathTime = -1;

            foreach (Player pla in Map.Players)
            {
                pla.Viewport.objectsInView.Add(this);
                pla.NetworkActions.UpdateObject(this);

            }
        }

        public void Remove()
        {
            Map.Objs.Remove(this);
            NeedsRemove = true;
            foreach (Player pla in Map.Players)
            {
                pla.NetworkActions.UpdateObject(this);
                pla.Viewport.objectsInView.Remove(this);
            }
        }

        public void Deplete(int time = 15000)
        {
            //  Server.Log("Depleting " + Definition.Name + "[" + UID + "]");

            Depleted = true;
            DeathTime = Environment.TickCount;
            Server.Instance.Delay(time, (timer, arg) =>
            {
                Respawn();
            });
            foreach (Player pla in Map.Players)
            {
                pla.NetworkActions.UpdateObject(this);
                pla.Viewport.objectsInView.Remove(this);

            }

        }

        public virtual MessageBuffer WriteBuffer(MessageBuffer buf)
        {
            buf.WriteUInt32((uint) ID);
            buf.WriteUInt32((uint) UID);
            buf.WriteByte((byte) State);
            buf.WriteFloat(transform.position.X);
            buf.WriteFloat(transform.position.Y);
            buf.WriteFloat(transform.position.Z);
            buf.WriteFloat(transform.rotation.X);
            buf.WriteFloat(transform.rotation.Y);
            buf.WriteFloat(transform.rotation.Z);
            buf.WriteFloat(transform.scale.X);
            buf.WriteFloat(transform.scale.Y);
            buf.WriteFloat(transform.scale.Z);

            return buf;
        }
    }
}
