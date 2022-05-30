using System;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using static TitanReach_Server.Database;

namespace TitanReach_Server.Network.Incoming
{
    class FRIENDS : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.FRIENDS;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {

            int subType = packet.ReadByte();

            switch (subType)
            {
                //INVITE
                case 0:
                    string name = packet.ReadString(packet.ReadByte());
                    p.FriendsManager.InviteFriend(name);
                    break;

                //REMOVE
                case 2:
                    uint uID = packet.ReadUInt32();
                    p.FriendsManager.RemoveFriend(uID);
                    break;
                //ACCEPT
                case 4:
                    uID = packet.ReadUInt32();
                    p.FriendsManager.AcceptInvite(uID);
                    break;

                //DECLINE
                case 5:
                    uID = packet.ReadUInt32();
                    p.FriendsManager.RemoveFriend(uID);
                    break;

                //IGNORE
                case 6:
                    name = packet.ReadString(packet.ReadByte());
                    p.FriendsManager.IgnorePlayer(name);
                    break;
            }


        }
    }
}
