using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;

namespace TitanReach_Server.Network.Incoming
{
    class SKILL_ACTION_TRIGGER : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.SKILL_ACTION_TRIGGER;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            packet.ReadByte();
            p.NetworkActions.SendSkillActionTrigger(packet.ReadUInt16());
        }

    }
}
