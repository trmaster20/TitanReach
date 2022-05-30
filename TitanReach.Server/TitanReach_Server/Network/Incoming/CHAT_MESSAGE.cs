using System;
using System.Linq;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using TitanReach_Server.Utilities;
using TRShared.Data.Enums;

namespace TitanReach_Server.Network.Incoming
{
    class CHAT_MESSAGE : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.CHAT_MESSAGE;
        }

        public const int LOCAL_CHAT_RADIUS = 40;


        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            MsgType type = (MsgType)packet.ReadInt32();
            string msg = packet.ReadString(packet.ReadByte());

            //check for command
            if (type == MsgType.Command) {
                Server.Instance.ProcessChatCommand(p, msg);
                return;
            }

            //check for mute
            if (p.Muted) {
                p.Error("You are muted and cannot speak");
                return;
            }

            switch (type) {
                case MsgType.Local:
                    SendLocalMessage(p, msg, type);
                    break;
                case MsgType.Party:
                    SendPartyMessage(p, msg, type);
                    break;
                case MsgType.Whisper:
                    SendPrivateMessage(p, msg, type);
                    break;
                case MsgType.World:
                    SendGlobalMessage(p, msg, type);
                    break;
            }

            if (type == MsgType.World || type == MsgType.Local)
            {
                string disc = "(W" + Server.SERVER_World + ") " + (type == MsgType.World ? "[W]" : "[L]") + " [" + p.Name + "]: " + msg;
                Server.DiscordChatMessages.Add(disc);
            }

        }

        private void SendGlobalMessage(Player p1, string msg, MsgType type)
        {
            
            foreach (Player p2 in Server.Instance.AllPlayers) {
                if (p1.Equals(p2)) {
                    continue;
                }

                if (p1.FriendsManager.IsIgnored(p2)) {
                    continue;
                }

                p2.NetworkActions.SendMessage(msg, type, (int)p1.UID, p1.Name, p1.Rank);
            }
        }

        private void SendLocalMessage(Player p1, string msg, MsgType type) {
            bool alone = true;
            foreach(Player p2 in p1.Map.Players) {
                if(!p2.Equals(p1) && Formula.InRadius(p1, p2, LOCAL_CHAT_RADIUS)){
                    if(alone) alone = false;
                    if (p1.FriendsManager.IsIgnored(p2))
                        continue;
                    p2.NetworkActions.SendMessage(msg, type, (int)p1.UID, p1.Name, p1.Rank);
                }
            }

        //    if (alone) {
       //         p1.NetworkActions.SendMessage("Your feeble voice can not reach any ears");
         //   }
        }

        private void SendPartyMessage(Player p1, string msg, MsgType type) {
            if (p1.HasParty()) {
                p1.Party.SendMessage(p1, msg, type);
            } else {
                p1.NetworkActions.SendMessage("You are not in a party");
            }
        }

        private void SendPrivateMessage(Player p, string msg, MsgType type) {
            string[] args = msg.Split(new[] { ',' }, 2);
            if (args.Length == 2) {
                Player other = Server.Instance.AllPlayers.Where(pl => pl.Name.ToLower().Equals(args[0].ToLower())).SingleOrDefault();
                if (other != null && !p.FriendsManager.IsIgnored(other)) {
                    p.NetworkActions.SendMessage(p.Name + " " + args[1], type, (int)other.UID, other.Name, other.Rank);
                    other.NetworkActions.SendMessage(p.Name + " " + args[1], type, (int)p.UID, p.Name, p.Rank);
                } else {
                    p.NetworkActions.SendMessage("Cannot send a message to: " + args[0]);
                }
            } else {
                p.NetworkActions.SendMessage("Invalid format: [User] [message]");
            }
        }
    }
}
