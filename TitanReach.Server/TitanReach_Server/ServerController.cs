using System;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Incoming;
using TRShared;
using TRShared.Data.Definitions;

namespace TitanReach_Server
{
    public class ServerController
    {

        public int LastNpcTick = Environment.TickCount;
        public int LastObjTick = Environment.TickCount;
        public void StartManager()
        {
            // System.Threading.Thread.CurrentThread.Name = "SvrCtrlThread";
            Server.Log("Started ServerController Manager");


            Server.Instance.LoopedDelay(25, (timer, arg) =>
            {
                UpdateProjectiles();
            });

            Server.Instance.LoopedDelay(200, (timer, arg) =>
            {
                UpdateNpcs();
                UpdatePlayers();
            });

            Server.Instance.LoopedDelay(500, (timer, arg) =>
            {
                RespawnObjects();
                RestockShops();
                RegenHealth();
                TRShared.Data.Formula.rand = new Random(Environment.TickCount);
            });

            Server.Instance.LoopedDelay(45000, (timer, arg) => //set to 30000
            {
                UpdatePlayerStats();
            });

            Server.Instance.LoopedDelay(1000, (timer, arg) =>
            {
                UpdateServerObejcts();
                CheckPlayerTimeouts();
            });

        }

        public void CheckPlayerTimeouts()
        {
            for (int i = 0; i < Server.Instance.AllPlayers.Count; i++)
            {
                Player p = Server.Instance.AllPlayers[i];
                if (p != null && p.LastPing != -1)
                {
                    if (Environment.TickCount - p.LastPing > 75000)
                    {
                        Server.Instance.RemovePlayer(p.NetworkActions.peer.ID, "Timed Out");
                    }
                }
            }
        }

        public void RegenHealth()
        {
            lock (Server.Instance.AllPlayers)
            {
                foreach (Player p in Server.Instance.AllPlayers)
                {
                    if (p == null)
                        continue;
                    if (Environment.TickCount - p.LastHeal > 30000)
                    {
                        p.Heal(1);
                        p.LastHeal = Environment.TickCount;
                    }
                }
            }
        }

        public void RestockShops()
        {
            foreach (Map map in Server.Instance.Maps.Values) {
                if (map == null) {
                    Server.Error("Map is null");
                    continue;
                }
                foreach (ShopDef def in DataManager.ShopDefinitions) {
                    bool changed = false;
                    foreach (ShopDef.ShopItem ing in def.Items) {
                        if (ing.Amount < ing.OriginalAmount && Environment.TickCount - ing.LastRespawnCheck > ing.RespawnTime) {
                            ing.LastRespawnCheck = Environment.TickCount;
                            ing.Amount++;
                            changed = true;
                        }
                    }
                    if (changed) {
                        SHOP_ACTION.UpdateShopToPlayers(map, def);
                    }
                }
            }
        }


        private void RespawnObjects()
        {
            /*foreach (Obj n in Server.Instance.objects)
            {
                if (n.Depleted)
                {
                    if (Environment.TickCount - n.DeathTime >= n.Definition.RespawnTime)
                    {
                        n.Respawn();
                    }
                }
            }*/
        }

        private void UpdateProjectiles()
        {
            foreach (Map map in Server.Instance.Maps.Values) {
                if (map == null) {
                    Server.Error("Map is null");
                    continue;
                }

                for (int i = map.Projectiles.Count - 1; i >= 0; i--) {
                    if (map.Projectiles[i].remove) map.Projectiles.RemoveAt(i);
                }

                foreach (Projectile projectile in map.Projectiles) {
                    try {
                        projectile.Update();

                    } catch (Exception e) {
                        Server.Error("Error " + e.Message + " \n" + e.StackTrace);
                    }
                }
            }
        }

        private void UpdateServerObejcts()
        {

            foreach (Map map in Server.Instance.Maps.Values)
            {
                if (map == null)
                {
                    // Server.Error("Map is null");
                    continue;
                }
                foreach (var obj in map.Objs)
                {
                    obj.Update1000ms();

                }
                //for(int i = 0; i < map.Objs.Count; i++)
                //{

                //}

            }
        }

        private void UpdateNpcs() {
            foreach (Map map in Server.Instance.Maps.Values) {
                if (map == null) {
                   // Server.Error("Map is null");
                    continue;
                }
                for (int i = 0; i < map.Npcs.Count; i++) {
                    Npc npc = map.Npcs[i];

                    if (npc != null) {
                        try {
                            npc.Update();
                        } catch (Exception e) {
                            Server.Error("Error " + e.Message + " \n" + e.StackTrace);
                        }
                    }
                }
            }
        }

        private void UpdatePlayers()
        {

            lock (Server.Instance.AllPlayers)
            {
                foreach (Player p in Server.Instance.AllPlayers)
                {
                    try
                    {
                        p.Update();

                    }
                    catch (Exception e)
                    {
                        Server.Error("Error " + e.Message + " \n" + e.StackTrace);
                    }

                }
            }
        }

        private void UpdatePlayerStats()
        {
            lock (Server.Instance.AllPlayers)
            {
                foreach (Player p in Server.Instance.AllPlayers)
                {
                    try
                    {
                        p.UpdateStats();

                    }
                    catch (Exception e)
                    {
                        Server.Error("Error " + e.Message + " \n" + e.StackTrace);
                    }
                }
            }
        }



        //float Lerp(float firstFloat, float secondFloat, float ratio)
        //{
        //    return firstFloat * (1 - ratio) + secondFloat * ratio;
        //}

        //Vector3 Lerp(Vector3 firstVector, Vector3 secondVector, float ratio)
        //{
        //    float retX = Lerp(firstVector.X, secondVector.X, ratio);
        //    float retY = Lerp(firstVector.Y, secondVector.Y, ratio);
        //    float retZ = Lerp(firstVector.Z, secondVector.Z, ratio);

        //    return new Vector3(retX, retY, retZ);
        //}
    }
}
