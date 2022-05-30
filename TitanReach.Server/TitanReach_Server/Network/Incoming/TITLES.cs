using System;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;

namespace TitanReach_Server.Network.Incoming
{
    class TITLES : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.TITLE;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            int subType = packet.ReadByte();

            switch (subType)
            {
                case 2:
                    uint titleID = packet.ReadByte();
                    p.TitleManager.SetActiveTitle(titleID);
                    break;
                case 3:
                    p.TitleManager.DeselectTitle();
                    break;
            }
        }
    }
}
