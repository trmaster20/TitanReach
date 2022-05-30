using System;
using System.Diagnostics;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;

namespace TitanReach_Server.Network.Incoming
{
    class LOGOUT : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.LOGOUT_REQUEST;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
           p.Disconnect("", true);
        }
    }
}
