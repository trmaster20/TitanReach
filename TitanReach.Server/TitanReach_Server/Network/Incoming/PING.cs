using System;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;

namespace TitanReach_Server.Network.Incoming
{
    class PING : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.PING;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            p.NetworkActions.Ping();
            p.LastPing = Environment.TickCount;
        }
    }
}
