using System;
using System.Collections.Generic;
using TitanReach_Server.Model;

namespace TitanReach_Server {
    public class PartyManager {
        private int avaialblePartyID = 0;
        private List<Party> parties;

        public PartyManager() {
            parties = new List<Party>();
        }

        public void CreateNewParty(Player owner) {
            Party p = new Party(avaialblePartyID, owner);
            avaialblePartyID++;
            parties.Add(p);
            owner.Party = p;
        }

        public void InvitePlayer(Player p, Player toInvite) {
            if (toInvite.UID == p.UID) {
                p.Msg("You cannot invite yourself to a party");
                return;
            }

            //check whether the other player is already in a party
            if (toInvite.HasParty()) {
                p.Msg(toInvite.Name + " is already in a party");
                return;
            }

            if (!p.HasParty()) {
                Server.Instance.PM.CreateNewParty(p);
            }

            //add to list of invites
            if (p.Party.AddInvite(toInvite)) {
                toInvite.NetworkActions.SendInvite(p, p.Party.ID);
                p.Msg("Sent a party invite to " + toInvite.Name);
                return;
            }

            p.Msg("Could not invite " + toInvite.Name);
        }

        public void AcceptInvite(int partyID, Player p) {
            AddPartyMember(partyID, p);
        }

        public void DeclineInvite(int partyID, Player p) {
            p.PartyInvites.Remove(partyID);

            Party party = GetParty(partyID);
            if (party == null) {
                return;
            }
            
            party.RemoveInvite(p);

            if (party.InviteCount() == 0 && party.MemberCount() == 1) {
                DestroyParty(party);
            }
        }

        public void RemovePartyMember(int partyID, Player member) {
            Party party = GetParty(partyID);
            if(party == null) {
                return;
            }

            if (party.RemoveMember(member)) {

                if(party.MemberCount() < 2) {
                    DestroyParty(party);
                }
            }
        }

        private void AddPartyMember(int partyID, Player member) {
            Party party = GetParty(partyID);
            if(party == null) {
                member.Msg("That party no longer exists");
                return;
            }

            party.AddMember(member);
        }

        private Party GetParty(int partyID) {
            return parties.Find( p => p.ID == partyID);
        }

        private void DestroyParty(Party p) {
            p.DestroyParty();
            parties.Remove(p);
        }
    }
}
