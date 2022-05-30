using System;
using System.Linq;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using TitanReach_Server.Utilities;

namespace TitanReach_Server.Network.Incoming
{


    class CHANGE_MAP : IncomingPacketHandler
    {
        public int GetID()
        {
            return Packets.MAP_CHANGE;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            int subtype = packet.ReadByte();
            switch (subtype)
            {
                case 1: // return signal from client saying map has loaded.

                    p.OnMapChange?.Invoke();
                    p.OnMapChange = null;
                    break;
            }

        }

    }

}

