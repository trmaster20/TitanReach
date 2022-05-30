using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;

namespace TitanReach_Server.Network.Incoming
{
    class ANIMATION_UPDATE : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.ANIMATION;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            int subType = packet.ReadByte();
            switch (subType) {
                case 0:

                    break;

                case 1:
                    p.NetworkActions.SendTeleportAnimation(packet.ReadUInt16());
                    break;

                case 2:
                    uint stateID = packet.ReadUInt32();
                    uint shift = packet.ReadUInt32();
                    uint direction = packet.ReadUInt32();
                    float DirV = (uint)packet.ReadFloat();
                    float DirH = (uint)packet.ReadFloat();

                    p.NetworkActions.SendAnimation(stateID, shift, direction, DirV, DirH);
                    break;

                case 3:
                    p.NetworkActions.SendAnimationType(3, packet.ReadUInt16());
                    break;

                case 4:
                    p.NetworkActions.SendAttackAnimation(packet.ReadUInt16(), packet.ReadBoolean());
                    break;

                case 5:
                    p.NetworkActions.SendBowPull(packet.ReadBoolean());
                    break;
                case 6:
                    p.NetworkActions.SendAnimationType(6, packet.ReadUInt16());
                    break;

            }


        }
    }
}
