using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;

namespace TitanReach_Server.Network.Incoming
{
    class SYNC_POS : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.SYNC_POS;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            p.transform.position = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
        }
    }
}
