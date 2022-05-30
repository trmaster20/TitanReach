using System;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using TitanReach_Server.Utilities;

namespace TitanReach_Server.Network.Incoming
{
    class PARTY : IncomingPacketHandler
    {
        private const byte INVITE = 0;
        private const byte ACCEPT = 1;
        private const byte DECLINE = 2;
        private const byte LEAVE = 5;
        private const byte KICK = 7;
        private const byte PROMOTE = 8;
        private const byte PVP_TOGGLE = 9;
        private const byte PVP_STATE = 10;

        public const int PARTY_TIMEOUT = 30000;

        public int GetID()
        {
            return Packets.PARTY;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            byte subtype = packet.ReadByte();

            switch (subtype) {
                case INVITE:
                    Server.Log("got invite");
                    uint puid = packet.ReadUInt32();
                    Player toInvite = Server.GetPlayerByUID(puid);
                    if (toInvite == null) {
                        p.Msg("You cannot invite that player");
                        return;
                    }
                    Server.Instance.PM.InvitePlayer(p, toInvite);
                    break;

                case ACCEPT:
                    int partyID = packet.ReadInt32();
                    Server.Instance.PM.AcceptInvite(partyID, p);
                    break;

                case DECLINE:
                    partyID = packet.ReadInt32();
                    Server.Instance.PM.DeclineInvite(partyID, p);
                    break;

                case LEAVE:
                    if (p.HasParty())
                        Server.Instance.PM.RemovePartyMember(p.Party.ID, p);
                    break;

                case KICK:
                    puid = packet.ReadUInt32();
                    Player toKick = Server.GetPlayerByUID(puid);

                    if (p.HasParty())
                        if(p.Party.IsOwner(p))
                            Server.Instance.PM.RemovePartyMember(p.Party.ID, toKick);
                    break;

                case PVP_TOGGLE:
                    if (p.HasParty())
                        p.Party.TogglePvp();
                    break;

                case PROMOTE:
                    puid = packet.ReadUInt32();
                    Player toPromote = Server.GetPlayerByUID(puid);
                    p.Party.ChangeOwner(toPromote);
                    break;

            }
        }
    }
}
