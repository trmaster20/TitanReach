using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;

namespace TitanReach_Server.Network.Incoming
{
    class DIALOG_ACTION : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.DIALOG_ACTION;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            p.questCountDebug++;
            int subType = packet.ReadByte();
            switch (subType) {
                case 0:
                    int option = packet.ReadByte();
                    p.CurrentDialogOption = option;
                    break;

                case 1:
                    if (p.ChatTokenSource != null) {
                        p.ChatTokenSource.Cancel();
                    }
                    break;
            }

        }
    }
}
