using System.Collections.Generic;
using System.Linq;

namespace TitanReach_Server.Model
{
    public class Viewport
    {

        public static int VIEW_RANGE = 130;

        public Player player;
        public List<Npc> NpcsInView = new List<Npc>();
        public List<Obj> objectsInView = new List<Obj>();
        public List<GroundItem> groundItemsInView = new List<GroundItem>();
        public List<Player> PlayersInView = new List<Player>();
        public Viewport(Player p)
        {
            this.player = p;
        }

        public void FullUpdate()
        {
            UpdateSurroundingNpcs();
            objectsInView.AddRange(player.Map.Objs);
            // NpcsInView.AddRange(Server.Instance.npcs);
            groundItemsInView.AddRange(Server.Instance.Maps[player.MapID].GroundItems.Where(itm => itm.ownerUID == 0 || itm.ownerUID == player.UID));
          //  groundItemsInView.AddRange(Server.Instance.groundItems);
        }


        public void UpdateSurroundingNpcs()
        {

            // not active right now
            for (int i = 0; i < NpcsInView.Count; i++)
            {
                var n = NpcsInView[i];
                if (n != null)
                {
                    if (!Formula.InRange(n.Transform.position, player.transform.position, VIEW_RANGE))
                    {
                        // Server.Log("removing npc");

                        //  Server.Log(n.Transform.position);
                        //   Server.Log(player.transform.position);
                        // Server.Log(VIEW_RANGE);


                        player.NetworkActions.SyncNpc(n, true);
                        NpcsInView.Remove(n);
                    }
                }
            }

            for (int i = 0; i < player.Map.Npcs.Count; i++)
            {
                var n = player.Map.Npcs[i];
                if (n != null)
                {
                    if (n.DeSpawned || n.Dead)
                        continue;
                    if (Formula.InRange(player.transform.position, n.Transform.position, VIEW_RANGE))
                    {
                        if (!NpcsInView.Contains(n))
                        {
                            //Server.Log("Adding " + n.Definition.Name);
                            NpcsInView.Add(n);
                            player.NetworkActions.SyncNpc(n);
                        }

                    }
                }
            }


        }
    }
}
