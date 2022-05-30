using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanReach_Server.Utilities;
using TRShared.Data.Enums;

namespace TitanReach_Server.Model {
    public class Party {
        private const int MAX_PARTY_SIZE = 5;

        public int ID { get; private set; }
        private Player owner;
        private List<Player> members;
        private List<Player> invitedMembers;
        public bool PvpEnabled { get; private set; }

        public Party(int iD, Player owner) {
            ID = iD;
            this.owner = owner;
            members = new List<Player> {
                owner
            };
            invitedMembers = new List<Player>();
            PvpEnabled = false;
        }

        public bool AddMember(Player p) {

            if(members.Count == MAX_PARTY_SIZE ||
                p.HasParty() ||
                invitedMembers.Where(invited => invited.UID == p.UID).SingleOrDefault() == null) {

                return false;
            }

            invitedMembers.Remove(p);
            members.Add(p);
            p.Party = this;
            p.PartyInvites.Clear();
            UpdateParty();

            foreach (Player player in GetOthers(p)) {
                player.Msg(p.Name + " has joined the party.");
            }

            return true;
        }

        public bool AddInvite(Player p) {
            if (members.Count == MAX_PARTY_SIZE) {
                return false;
            }

            if (p.HasParty()) {
                return false;
            }

            invitedMembers.Add(p);
            p.PartyInvites[ID] = Environment.TickCount;
            return true;
        }

        public bool RemoveMember(Player p) {
            if (members.Count > 1) {
                if (owner.UID == p.UID) {
                    UpdateOwner();
                }
            }

            if (members.Remove(p)) {
                p.Party = null;
                foreach(Player player in members) {
                    player.Msg(p.Name + " has left the party.");
                }
                UpdateParty();
                p.NetworkActions.DisbandParty();
                return true;
            }

            return false;
        }

        public void UpdateParty() {
            foreach (Player player in members) {
                player.NetworkActions.UpdateParty(members);
            }
        }

        public void UpdatePartyInfo(Player p) {
            foreach (Player player in members) {
                player.NetworkActions.UpdatePartyInfo(p);
            }
        }

        public void UpdatePartyBuffs(Player p, Buff buff) {
            foreach (Player player in members) {
                player.NetworkActions.UpdatePartyBuffs(p, buff);
            }
        }

        internal List<Player> GetOthers(Player p) {
            return members.Where(member => member.UID != p.UID).ToList();
        }

        public bool RemoveInvite(Player p) {
            return invitedMembers.Remove(p);
        }

        public bool IsOwner(Player p) {
            if (p == null || owner == null)
                return false;
            return p.UID == owner.UID;
        }

        private void UpdateOwner() {
            List<Player> nonOwners = members.Where(p => !IsOwner(p)).ToList();

            if(nonOwners.Count() > 0) {
                owner = nonOwners[0];
            }
        }

        public bool ChangeOwner(Player p) {
            if(IsOwner(p)) {
                return false;
            }

            var pl = members.Where(pl => pl.UID == p.UID).SingleOrDefault();
            if(pl != null) {
                owner = p;
                UpdateParty();
                return true;
            }

            return false;
        }

        public void TogglePvp() {
            PvpEnabled = !PvpEnabled;
            foreach(Player player in members) {
                player.NetworkActions.UpdatePartyPvpState(PvpEnabled);
            }
        }

        public void SendMessage(Player p, string msg, MsgType type) {
            foreach(Player o in GetOthers(p)) {
                if (p.FriendsManager.IsIgnored(o))
                    return;
                o.NetworkActions.SendMessage(msg, type, (int)p.UID, p.Name, p.Rank);
            }
        }

        public int MemberCount() {
            return members.Count;
        }

        public int InviteCount() {
            return invitedMembers.Count;
        }

        public void DestroyParty() {
            foreach(Player p in members) {
                p.Party = null;
                p.Msg("The party has disbanded.");
                p.NetworkActions.DisbandParty();
            }

            members.Clear();
            owner = null;
        }

        public override bool Equals(object obj) {
            return obj is Party party &&
                   ID == party.ID;
        }

        public override int GetHashCode() {
            return HashCode.Combine(ID);
        }
    }
}
