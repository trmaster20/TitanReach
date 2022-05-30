using System;
using TitanReach_Server.Model;

namespace TitanReach_Server
{
    public class HealthEngine
    {
        int LastPrint = Environment.TickCount;
        public void Ping()
        {

            if (Environment.TickCount - LastPrint > 60000)
            {

                Server.Debug("------- World " + Server.SERVER_World + " Stats -------");

                //Server.Log("Online Players: " + Server.Instance.players.Count + " - Npcs " + Server.Instance.npcs.Count);

                lock (Server.Instance.AllPlayers)
                {
                    for (int i = 0; i < Server.Instance.AllPlayers.Count; i++)
                    {
                        Player p = Server.Instance.AllPlayers[i];
                        Server.Log("[" + p.UID + "] " + p.Name + " (" + (int)p.transform.position.X + ", " + (int)p.transform.position.Y + ", " + (int)p.transform.position.Z + ") Recv/Sent: " + p.PacketsRecv + "/" + p.PacketsSent);
                    }
                }

                LastPrint = Environment.TickCount;

            }

        }
    }
}

