using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;

namespace TitanReach_Server.Network.Incoming
{
    class COMBAT_STANCE : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.COMBAT_STANCE;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            packet.ReadByte();
            p.Stance = packet.ReadByte();
            p.NetworkActions.SendCombatStance();
        }
    }
}
