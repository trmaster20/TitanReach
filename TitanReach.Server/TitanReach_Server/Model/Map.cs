using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TitanReach.Server.Model.ServerObjects;
using TRShared;
using TRShared.Data.Definitions;
using TRShared.Data.Enums;

namespace TitanReach_Server.Model
{
    public class Map
    {

        public int MapID;
        public int LandID;

        public List<Player> Players = new List<Player>();
        public List<Npc> Npcs = new List<Npc>();
        public List<Obj> Objs = new List<Obj>();
        public List<GroundItem> GroundItems = new List<GroundItem>();
        public List<Projectile> Projectiles = new List<Projectile>();
        public DateTime START = DateTime.Now;
        public static int TIME_MULTIPLIER = 6;
        public static Vector3 STARTING_TIME = new Vector3(6, 30, 0);

        public Vector3 GetServerTime()
        {
            TimeSpan elapsed = (DateTime.Now - START) * TIME_MULTIPLIER;

            return new Vector3(elapsed.Hours + STARTING_TIME.X, elapsed.Minutes + STARTING_TIME.Y, elapsed.Seconds);
        }

        public bool PvpEnabled = false;

        public LandDef Land => DataManager.LandDefinitions[LandID];
        static bool thing = false;
        public void Load() {
            Server.Log("Loading Map: " + MapID + " using LandID: " + LandID);
            Land.LoadDefs();
            SpawnNPCs();
            SpawnObjects();
        }

        public void AddItemToGround(GroundItem groundItem, Player owner = null) {
            if(groundItem.Item.Amount < 1)
            {
                Server.Error("Item dropped with 0 ammount Map.cs");
            }
            if (owner != null) {
                groundItem.ownerUID = owner.UID;
                if (Formula.InRadius(owner.transform.position, groundItem.transform.position, Viewport.VIEW_RANGE)) {
                    owner.Viewport.groundItemsInView.Add(groundItem);
                    owner.NetworkActions.AddGroundItem(groundItem);
                }
            }
            GroundItems.Add(groundItem);

            Server.Instance.Delay(120000, (timer, arg) => {
                if (groundItem != null && GroundItems.Contains(groundItem)) {
                    lock (Players) {
                        foreach (Player pl in Players) {
                            pl.Viewport.groundItemsInView.Remove(groundItem);
                            pl.NetworkActions.RemoveGroundItem(groundItem.groundItemUID);

                        }
                    }
                    GroundItems.Remove(groundItem);
                }
            });
            // 30 second timer before the world see it
            if (owner != null) {
                Server.Instance.Delay(30000, (timer, arg) => {
                    if (groundItem != null && !groundItem.Taken) {
                        groundItem.ownerUID = 0;
                        lock (Players) {
                            foreach (Player pl in Players) {
                                if (pl == null)
                                    return;
                                if (pl.UID == owner.UID)
                                    continue;
                                if (pl.MapID != groundItem.MapID)
                                    continue;
                               
                                if (Formula.InRadius(pl.transform.position, groundItem.transform.position, Viewport.VIEW_RANGE)) {
                                    pl.Viewport.groundItemsInView.Add(groundItem);
                                    pl.NetworkActions.AddGroundItem(groundItem);
                                }
                            }
                        }
                    }


                });
            } else {
                lock (Players) {
                    foreach (Player pl in Players) {
                        if (Formula.InRadius(pl.transform.position, groundItem.transform.position, Viewport.VIEW_RANGE)) {
                            pl.Viewport.groundItemsInView.Add(groundItem);
                            pl.NetworkActions.AddGroundItem(groundItem);
                        }
                    }
                }
            }

        }

        public Obj SpawnGroundObject(int objectID, Vector3 pos, ObjectLocationDef def = null) {
            Obj obj = new Obj(objectID); // bonfire
            if (def != null) {
                obj.LocationDef = def;
                obj.transform.position = new Vector3(obj.LocationDef.Position);
                obj.transform.rotation = new Vector3(obj.LocationDef.Rotation);
                obj.transform.scale = new Vector3(obj.LocationDef.Scale);
            } else {
                obj.transform.position = pos;
                obj.transform.scale = new Vector3(1, 1, 1);
            }
            obj.MapID = MapID;
            Objs.Add(obj);
            lock (Players) {
                foreach (Player p in Players) {
                    p.Viewport.objectsInView.Add(obj);
                    p.NetworkActions.SyncObject(obj);
                }
            }

            return obj;
        }

        private void SpawnNPCs() {
            int count = 0;
            foreach (var def in Land.NpcSpawnDefinitions) {
                if (def.LandID == Land.LandID)
                {
                    for (int r = 0; r < def.Amount; r++)
                    {
                        Npc npc = new Npc(def.ID, Land.NpcSpawnDefinitions.IndexOf(def), MapID);
                        Npcs.Add(npc);
                        count++;
                    }
                }
            }
            Server.Log("[Map #" + MapID + "]: Spawned " + count + " Npcs");
        }

        private void SpawnObjects() {
            int count = 0;
            foreach (ObjectLocationDef def in Land.ObjectLocationsDefinitions) {
                Obj obj;
                if (DataManager.ObjectDefinitions[def.ObjectID].HasAttribute(ObjectAttribute.Portal))
                {
                    obj = new ObjectPortal(def.ObjectID);
                }
                else if (DataManager.ObjectDefinitions[def.ObjectID].HasAttribute(ObjectAttribute.MultiFish))
                {
                    obj = new ObjectFishingSpot(def.ObjectID);
                }
                else { obj = new Obj(def.ObjectID); }
                obj.transform.position = new Vector3(def.Position);
                obj.transform.rotation = new Vector3(def.Rotation);
                obj.transform.scale = new Vector3(def.Scale);
                obj.LocationDef = def;
                obj.MapID = MapID;
                if (def.MetaData != null)
                {
                    Server.Log(def.MetaData);
                    obj.SetMetaData(def.MetaData);
                }
                Objs.Add(obj);
                count++;
            }
            Server.Log("[Map #" + MapID + "]: Spawned " + count + " Objects");

        }

        public override bool Equals(object obj) {
            return obj is Map map &&
                   MapID == map.MapID;
        }

        public override int GetHashCode() {
            return HashCode.Combine(MapID);
        }
    }
}
