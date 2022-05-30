using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;

namespace TitanReach_Server.Network.Incoming
{
    class NULL_LENGTH : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.NULL_LENGTH;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {

        }
    }
}
