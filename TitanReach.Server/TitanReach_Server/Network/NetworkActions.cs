using ENet;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using TitanReach_Server.Quests;
using TitanReach_Server.Utilities;
using TRShared;
using TRShared.Data.Definitions;
using TRShared.Data.Enums;
using static TitanReach_Server.Database;
using static TitanReach_Server.Model.Npc;
using static TitanReach_Server.QuestManager;

namespace TitanReach_Server.Network
{
    public class NetworkActions
    {

        public Peer peer;
        public Player player;
        public NetworkActions(Peer pe, Player p)
        {
            this.peer = pe;
            this.player = p;
        }

        public void SyncAllPets() {
            MessageBuffer buffer = CreatePacket(Packets.PET);
            buffer.WriteByte(0);
           // buffer.WriteByte((byte)Server.Instance.AllPets.Count);
            buffer.WriteByte((byte)TRShared.DataManager.PetDefinitions.Length);

            foreach (var petDef in TRShared.DataManager.PetDefinitions) {
                buffer.WriteInt32(petDef.PetID);
                buffer.WriteInt32(petDef.Name.Length);
                buffer.WriteString(petDef.Name);
            }
            SendPacket(buffer, PacketFlags.Reliable);
        }

        public void SyncPlayerPets() {
            MessageBuffer buffer = CreatePacket(Packets.PET);
            buffer.WriteByte(1);
            buffer.WriteByte((byte)player.PetManager.TotalPets());

            for(int i=0; i < player.PetManager.UnlockedPets.Count; i++)
            {
                int pet = player.PetManager.UnlockedPets[i];
                buffer.WriteInt32(pet);
            }
            SendPacket(buffer, PacketFlags.Reliable);
        }

        public void SetPet(int petID, Player owner)
        {
            MessageBuffer buffer = CreatePacket(Packets.PET);
            buffer.WriteByte(2);
            buffer.WriteUInt32(owner.UID);
            buffer.WriteInt32(petID);
            SendPacket(buffer, PacketFlags.Reliable);
        }

        public void DestroyPet(Player owner) {
            MessageBuffer buffer = CreatePacket(Packets.PET);
            buffer.WriteByte(3);
            buffer.WriteUInt32(owner.UID);
            SendPacket(buffer, PacketFlags.Reliable);
        }

        public void SyncAllTitles() {

                MessageBuffer buffer = CreatePacket(Packets.TITLE);
                buffer.WriteByte(0);
                buffer.WriteByte((byte)Server.Instance.AllTitles.Count);

                foreach (Title title in Server.Instance.AllTitles.Values)
                {
                    buffer.WriteByte((byte)title.ID);
                    buffer.WriteByte((byte)title.Display.Length);
                    buffer.WriteString(title.Display);
                    buffer.WriteByte((byte)title.Type);
                }
                SendPacket(buffer, PacketFlags.Reliable);

        }

        public void SyncPlayerTitles()
        {
            MessageBuffer buffer = CreatePacket(Packets.TITLE);
            buffer.WriteByte(1);
            buffer.WriteByte((byte)player.TitleManager.TotalTitles());

            foreach(Title title in player.TitleManager.UnlockedTitles)
            {
            
                buffer.WriteByte((byte)title.ID);
            }
            SendPacket(buffer, PacketFlags.Reliable);
        }

        public void DeselectTitle(Player owner)
        {
            MessageBuffer buffer = CreatePacket(Packets.TITLE);
            buffer.WriteByte(3);
            buffer.WriteUInt32(owner.UID);
            SendPacket(buffer, PacketFlags.Reliable);
        }

        public void SetTitle(Title title, Player owner)
        {
            MessageBuffer buffer = CreatePacket(Packets.TITLE);
            buffer.WriteByte(2);
            buffer.WriteUInt32(owner.UID);
            buffer.WriteByte((byte)title.ID);
            SendPacket(buffer, PacketFlags.Reliable);
        }

        public void SendFriendInvite(Friend friend)
        {
            MessageBuffer buffer = CreatePacket(Packets.FRIENDS);
            buffer.WriteByte(0);
            buffer.WriteUInt32(friend.UID);
            buffer.WriteByte((byte)friend.Name.Length);
            buffer.WriteString(friend.Name);
            buffer.WriteByte((byte)friend.FriendType);
            buffer.WriteByte((byte)friend.InviteStatus);
            buffer.WriteUInt32((byte)friend.InitiatedBy);
            SendPacket(buffer, PacketFlags.Reliable);
        }

        internal void FriendRemoved(Friend friend) {
            MessageBuffer buffer = CreatePacket(Packets.FRIENDS);
            buffer.WriteByte(2);
            buffer.WriteUInt32(friend.UID);
            SendPacket(buffer, PacketFlags.Reliable);
        }

        public void SyncFriend(Friend friend, bool online) {
            MessageBuffer buffer = CreatePacket(Packets.FRIENDS);
            buffer.WriteByte(3);
            buffer.WriteUInt32(friend.UID);
            buffer.WriteByte((byte)friend.Name.Length);
            buffer.WriteString(friend.Name);
            buffer.WriteByte((byte)friend.FriendType);
            buffer.WriteByte((byte)friend.InviteStatus);
            buffer.WriteUInt32((byte)friend.InitiatedBy);
            buffer.WriteBoolean(online);
            SendPacket(buffer, PacketFlags.Reliable);
        }

        public void AddFriend(Friend friend, bool online) {
            MessageBuffer buffer = CreatePacket(Packets.FRIENDS);
            buffer.WriteByte(1);
            buffer.WriteUInt32(friend.UID);
            buffer.WriteByte((byte)friend.Name.Length);
            buffer.WriteString(friend.Name);
            buffer.WriteByte((byte)friend.FriendType);
            buffer.WriteByte((byte)friend.InviteStatus);
            buffer.WriteUInt32((byte)friend.InitiatedBy);
            buffer.WriteBoolean(online);
            SendPacket(buffer, PacketFlags.Reliable);
        }

        public void SyncAllExp(Player playerToUpdate)
        {
            for (int i = 0; i < Enum.GetValues(typeof(Stats.SKILLS)).Length; i++)
            {
                SyncExp(playerToUpdate, i);
            }
        }

        public void AddToHotbar(int id)
        {
            var buf = CreatePacket(Packets.ADD_ITEM_TO_HOTBAR);
            buf.WriteByte(0);
            buf.WriteUInt32((uint)id);
            SendPacket(buf, PacketFlags.Reliable);
        }

        public void SyncHealth(Player playerToUpdate)
        {
            var buffer = CreatePacket(Packets.STATS);
            buffer.WriteByte(0); //Update Health
            buffer.WriteUInt32(playerToUpdate.UID);
            buffer.WriteInt32(playerToUpdate.CurrentHealth);
            buffer.WriteInt32(playerToUpdate.GetMaxHealth());
            SendPacket(buffer, PacketFlags.Reliable);
        }

        public void SyncMana(Player playerToUpdate)
        {
            //Not yet needed but is here 
            var buffer = CreatePacket(Packets.STATS);
            buffer.WriteByte(1); //Update Health
            //buffer.WriteUInt32(playerToUpdate.UID);
            //buffer.WriteInt32(playerToUpdate.CurHealth);
            //buffer.WriteInt32(playerToUpdate.GetMaxHealth());
            //SendPacket(buffer, PacketFlags.Reliable);
        }

        public void SyncExp(Player playerToUpdate,int skill)
        {
            var buf = CreatePacket(Packets.SYNC_EXP);
            buf.WriteByte(0);
            buf.WriteUInt32(playerToUpdate.UID);
            buf.WriteByte((byte)skill);
            buf.WriteByte((byte)playerToUpdate.Skills.GetCurLevel(skill));
            buf.WriteByte((byte)playerToUpdate.Skills.GetMaxLevel(skill));
            buf.WriteByte((byte)playerToUpdate.Skills.GetCombatLevel());

            buf.WriteUInt32((uint)playerToUpdate.Skills.GetExp(skill));
                
            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void UpdateBuff(int buffID, int duration, float[] arg)
        {
            lock (player.Map.Players)
            {
                foreach (Player p in player.Map.Players)
                {
                    var buf = CreatePacket(Packets.UPDATE_BUFF);
                    buf.WriteByte((byte)(int)DataManager.BuffDefinitions[buffID].SpellType); // type ID
                    buf.WriteUInt16((ushort)buffID); // buff ID
                    buf.WriteUInt32((uint)duration); // duration in milliseconds
                    buf.WriteUInt32((uint)player.UID);

                    buf.WriteByte((byte)arg.Length);
                    foreach (float f in arg)
                        buf.WriteFloat(f);

                    p.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
                }
            }
        }

        public void ClearBuffs()
        {
            lock (player.Map.Players)
            {
                foreach (Player p in player.Map.Players)
                {
                    var buf = CreatePacket(Packets.UPDATE_BUFF);
                    buf.WriteByte(Byte.MaxValue); // type ID
                    buf.WriteUInt32(player.UID);
                    p.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
                }
            }
        }

        public void SendCharacterScreen()
        {
            MessageBuffer buf = CreatePacket(Packets.CHARACTER_CREATOR);
            buf.WriteByte(0);
            SendPacket(buf, PacketFlags.Reliable);
        }

        public void UpdateQuestInfo(Quest q, QuestInfo qInfo, QuestState state)
        {
            try
            {
                MessageBuffer buf = CreatePacket(Packets.QUEST);

                buf.WriteByte(1);
                buf.WriteByte((byte)q.ID);
                buf.WriteInt32(state.Number);


                if (!AddQuestInfo(buf, qInfo, state)) {
                    SendPacket(buf, PacketFlags.Reliable);
                }
            } catch(Exception e)
            {
                Server.Error(e);
            }
        }

        public void UpdateQuest(Quest q, QuestInfo qInfo, QuestState state, bool completed) {

            try
            {
                MessageBuffer buf = CreateLargePacket(Packets.QUEST);

                buf.WriteByte(0);
                buf.WriteByte((byte)q.ID);
                buf.WriteByte((byte)q.Name.Length);
                buf.WriteString(q.Name);
                buf.WriteInt32(state.Number);
                buf.WriteBoolean(completed);
          
                bool broke = AddQuestInfo(buf, qInfo, state);
                if(!broke)
                    player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
            } catch(Exception e)
            {
                Server.Error("Error @ UpdateQuest: " + e.Message + "- " + e.StackTrace);
            }
        }

        private bool AddQuestInfo(MessageBuffer buf, QuestInfo qInfo, QuestState state) {

            //write tasks
            List<List<QuestStep>> taskOptions = state.GetTaskOptions();
            buf.WriteByte((byte)taskOptions.Count);
            foreach (List<QuestStep> option in taskOptions) {
                buf.WriteByte((byte)option.Count);
                foreach (QuestStep step in option) {
                    if(step == null || step.Text == null || step.Text.Length < 1 || step.Text.Length > 300)
                        continue;
                    

                    buf.WriteInt16((short)step.Text.Length);
                    try
                    {
                        buf.WriteString(step.Text);
                    } catch(Exception e)
                    {
                        Server.Log("=== DEBUG LOG === QuestPacketCount " + player.questCountDebug);
                        player.Disconnect("Error with Quest", false, false);
                        return true;
                    }
                    buf.WriteBoolean(step.complete);
                }
            }

            if (qInfo == null) {
                buf.WriteByte(0);
            } else {
                buf.WriteByte(1);

                //write hints
                buf.WriteInt32(qInfo.HintText.Count);
                for (int i = 0; i < qInfo.HintText.Count; i++) {
                    buf.WriteInt16((short)qInfo.HintText[i].Length);
                    buf.WriteString(qInfo.HintText[i]);
                }

                //write journal text
                buf.WriteInt16((short)qInfo.JournalText.Length);
                buf.WriteString(qInfo.JournalText);
            }
            return false;
        }

        public void CompleteQuest(int questid)
        {

            var buf = CreatePacket(Packets.QUEST);
            buf.WriteByte((byte)2);
            buf.WriteByte((byte)questid);

            SendPacket(buf, PacketFlags.Reliable);
        }

        /* 
        public void UpdateIndicator(bool add, Indicator indi)
        {

            var buf = CreatePacket(Packets.INDICATOR_UPDATE);
            buf.WriteByte((byte)(add ? 1 : 0));
            buf.WriteByte((byte)indi.IconType);
            buf.WriteByte((byte)indi.Type);
            buf.WriteByte((byte)indi.QuestID);
            if (indi.Type == IndicatorType.POSITION)
            {
                buf.WriteFloat(indi.x);
                buf.WriteFloat(indi.y);
                buf.WriteFloat(indi.z);
            }
            else
            {
                buf.WriteUInt32((uint)indi.ObjOrNpcID);
            }

            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);

        }*/

        public void UpdateIndicators()
        {
            // foreach (Indicator indi in player.QuestManager.Indicators)
            //  UpdateIndicator(true, indi);
        }

        public void SendEmote(int emoteID) {
            MessageBuffer buf = CreatePacket(Packets.EMOTE);
            buf.WriteByte(0);
            buf.WriteByte((byte)emoteID);
            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void SendLocalPlayerEquipmentUpdate()   //send one players equipment to everyone else
        {
            lock (player.Viewport.PlayersInView)
            {
                foreach (Player p in player.Viewport.PlayersInView)
                {
                    MessageBuffer buf = CreatePacket(Packets.EQUIP_UPDATE);
                    buf.WriteByte(0);
                    buf.WriteUInt32(player.UID);
                    buf.WriteByte((byte)player.Equipment.EquippedItems.Length);
                    for (int i = 0; i < player.Equipment.EquippedItems.Length; i++)
                    {

                        buf.WriteByte((byte)i);
                        buf.WriteUInt16(player.Equipment.EquippedItems[i] == null ? UInt16.MaxValue : (ushort)player.Equipment.EquippedItems[i].Definition.ItemID);
                        buf.WriteUInt32(player.Equipment.EquippedItems[i] == null ? UInt32.MaxValue : (uint)player.Equipment.EquippedItems[i].UID);
                        buf.WriteUInt32(player.Equipment.EquippedItems[i] == null ? UInt32.MaxValue : (uint)player.Equipment.EquippedItems[i].Amount);
                    }
                    p.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
                }
            }

        }

        public void SendLocalPlayerAreaEquipment()  //send everyone elses equipment to one player
        {
            lock (player.Viewport.PlayersInView)
            {
                foreach (Player p in player.Viewport.PlayersInView)
                {
                    MessageBuffer buf = CreatePacket(Packets.EQUIP_UPDATE);
                    buf.WriteByte(0);
                    buf.WriteUInt32(p.UID);
                    buf.WriteByte((byte)p.Equipment.EquippedItems.Length);
                    for (int i = 0; i < p.Equipment.EquippedItems.Length; i++)
                    {

                        buf.WriteByte((byte)i);
                        buf.WriteUInt16(p.Equipment.EquippedItems[i] == null ? UInt16.MaxValue : (ushort)p.Equipment.EquippedItems[i].Definition.ItemID);
                        buf.WriteUInt32(p.Equipment.EquippedItems[i] == null ? UInt32.MaxValue : (uint)p.Equipment.EquippedItems[i].UID);
                        buf.WriteUInt32(p.Equipment.EquippedItems[i] == null ? UInt32.MaxValue : (uint)p.Equipment.EquippedItems[i].Amount);

                    }
                    SendPacket(buf, PacketFlags.Reliable);
                }
            }

        }

        public void SendEquipment()
        {
            var buf = CreatePacket(Packets.EQUIP_UPDATE);
            buf.WriteByte(0);
            buf.WriteUInt32(player.UID);
            buf.WriteByte((byte)player.Equipment.EquippedItems.Length);
            for (int i = 0; i < player.Equipment.EquippedItems.Length; i++)
            {

                buf.WriteByte((byte)i);
                buf.WriteUInt16(player.Equipment.EquippedItems[i] == null ? UInt16.MaxValue : (ushort)player.Equipment.EquippedItems[i].Definition.ItemID);
                buf.WriteUInt32(player.Equipment.EquippedItems[i] == null ? UInt32.MaxValue : (uint)player.Equipment.EquippedItems[i].UID);
                buf.WriteUInt32(player.Equipment.EquippedItems[i] == null ? UInt32.MaxValue : (uint)player.Equipment.EquippedItems[i].Amount);
            }
            SendPacket(buf, PacketFlags.Reliable);


        }

        public void UpdateNpc(Npc npc)
        {
            if (player.Viewport.NpcsInView.Contains(npc))
            {

                MessageBuffer buf = CreatePacket(Packets.NPC);
                buf.WriteByte(10);
                buf.WriteUInt32((uint)npc.UID);
                buf.WriteByte((byte)(npc.DeSpawned ? 1 : 0));
                buf.WriteUInt32((uint)npc.CurrentHealth);
                buf.WriteFloat(npc.Transform.position.X);
                buf.WriteFloat(npc.Transform.position.Y);
                buf.WriteFloat(npc.Transform.position.Z);

                buf.WriteDouble(npc.Scale);
                SendPacket(buf, PacketFlags.Reliable);

            }
        }

        public void SendAnimation(Actions animation, int time, int left_id = -1, int right_id = -1)
        {
            lock (player.Viewport.PlayersInView)
            {
                foreach (Player pl in player.Viewport.PlayersInView)
                {
                    if (pl != null)
                    {
                        MessageBuffer buf = CreatePacket(Packets.ANIMATION);
                        buf.WriteByte(0);
                        buf.WriteUInt32(player.UID);
                        buf.WriteUInt16((ushort)animation);
                        buf.WriteUInt16((ushort)time);
                        buf.WriteUInt16((ushort)left_id);
                        buf.WriteUInt16((ushort)right_id);
                        pl.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
                    }
                }
            }
        }

        public void UpdateObject(Obj obj)
        {
            if (player.Viewport.objectsInView.Contains(obj))
            {
                MessageBuffer buf = CreatePacket(Packets.OBJECT);
                buf.WriteByte(2);
                buf.WriteUInt32((uint)obj.UID);
                if (obj.NeedsRemove)
                    buf.WriteByte((byte)(2));
                else
                    buf.WriteByte((byte)(obj.Depleted ? 1 : 0));

                buf.WriteByte((byte)obj.State);
                SendPacket(buf, PacketFlags.Reliable);

            }
        }

        public void SendProjectile(ProjectileDefinition def, Vector3 velocity, Player pl)
        {
            MessageBuffer buf = CreatePacket(Packets.PROJECTILE);
            buf.WriteByte(0);
            buf.WriteUInt32(pl.UID);
            buf.WriteByte((byte)def.ProjectileID);

            buf.WriteFloat(velocity.X);
            buf.WriteFloat(velocity.Y);
            buf.WriteFloat(velocity.Z);

            SendPacket(buf, PacketFlags.Reliable);
        }

        public void SendNPCProjectile(ProjectileDefinition def, Vector3 velocity, Npc npc)
        {
            MessageBuffer buf = CreatePacket(Packets.PROJECTILE);
            buf.WriteByte(1);
            buf.WriteUInt32((uint)npc.UID);
            buf.WriteByte((byte)def.ProjectileID);

            buf.WriteFloat(velocity.X);
            buf.WriteFloat(velocity.Y);
            buf.WriteFloat(velocity.Z);

            SendPacket(buf, PacketFlags.Reliable);
        }

        public void SendNpcTarget(Vector3 pos, int totalTime, Npc npc)
        {
            MessageBuffer buf = CreatePacket(Packets.NPC);
            buf.WriteByte(3);
            buf.WriteUInt32((uint)npc.UID);
            buf.WriteFloat(pos.X);
            buf.WriteFloat(pos.Y);
            buf.WriteFloat(pos.Z);
            buf.WriteUInt32((uint)totalTime);
            SendPacket(buf, PacketFlags.Reliable | PacketFlags.Unsequenced);
        }

        public void SendNpcTargetLock(int type, int npcUID, uint playerID = 0, float angle = 0)
        {
            MessageBuffer buf = CreatePacket(Packets.NPC);
            buf.WriteByte((byte) (type));
           // buf.WriteUInt32((uint)type);
            buf.WriteUInt32((uint)npcUID);
            if (type == 5) buf.WriteUInt32(playerID);
            if (type == 6) buf.WriteFloat(angle);

            SendPacket(buf, PacketFlags.Reliable);
        }

        public void SyncObjects()
        {
            MessageBuffer buf = CreateLargePacket(Packets.OBJECT);
            buf.WriteByte(1);
            buf.WriteUInt32((uint)player.Viewport.objectsInView.Count);

            foreach (Obj obj in player.Viewport.objectsInView)
            {
                buf.WriteUInt32((uint)obj.ID);
                buf.WriteUInt32((uint)obj.UID);
                buf.WriteByte((byte)obj.State);
                buf.WriteFloat(obj.transform.position.X);
                buf.WriteFloat(obj.transform.position.Y);
                buf.WriteFloat(obj.transform.position.Z);
                buf.WriteFloat(obj.transform.rotation.X);
                buf.WriteFloat(obj.transform.rotation.Y);
                buf.WriteFloat(obj.transform.rotation.Z);
                buf.WriteFloat(obj.transform.scale.X);
                buf.WriteFloat(obj.transform.scale.Y);
                buf.WriteFloat(obj.transform.scale.Z);
            }

            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);

        }

        public void SendGameReady()
        {
            var buf = CreatePacket(Packets.GAME_READY);
            buf.WriteByte(0);
   
            byte hour = 0;
            byte minute = 0;
            if(player.MapID == 0)
            {
                var time = player.Map.GetServerTime();
                hour = (byte)time.X;
                minute = (byte)time.Y;
            }
            buf.WriteByte(hour);
            buf.WriteByte(minute);
            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);

        }

        public void SyncGroundItems()
        {
            MessageBuffer buf = CreateLargePacket(Packets.GROUND_ITEM);
            buf.WriteByte(0);
            buf.WriteUInt32((uint)player.Viewport.groundItemsInView.Count);

            foreach (GroundItem obj in player.Viewport.groundItemsInView)
            {
                buf.WriteUInt32((uint)obj.Item.ID);
                buf.WriteUInt32((uint)obj.groundItemUID);
                buf.WriteUInt32((uint)obj.Item.Amount);
                buf.WriteFloat(obj.transform.position.X);
                buf.WriteFloat(obj.transform.position.Y);
                buf.WriteFloat(obj.transform.position.Z);
            }

            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);

        }

        internal void SendFollow(Player pl) {
            if (pl.UID == player.UID) {
                player.Msg("You cannot follow yourself");
                return;
            }

            MessageBuffer buf = CreatePacket(Packets.MOVE);

            buf.WriteByte(5);
            buf.WriteUInt32(pl.UID);
            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void AddGroundItem(GroundItem gi)
        {
            MessageBuffer buf = CreatePacket(Packets.GROUND_ITEM);
            buf.WriteByte(1);

            buf.WriteUInt32((uint)gi.Item.ID);
            buf.WriteUInt32((uint)gi.groundItemUID);
            buf.WriteUInt32((uint)gi.Item.Amount);
            buf.WriteFloat(gi.transform.position.X);
            buf.WriteFloat(gi.transform.position.Y);
            buf.WriteFloat(gi.transform.position.Z);

            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void RemoveGroundItem(int uid)
        {
            MessageBuffer buf = CreatePacket(Packets.GROUND_ITEM);
            buf.WriteByte(2);
            buf.WriteUInt32((uint)uid);

            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void SpellTargetGround(Vector3 position, int spell)
        {

            MessageBuffer buf = CreatePacket(Packets.SPELL_TARGET_GROUND);
            buf.WriteByte(0);
            buf.WriteUInt32((uint)spell);
            buf.WriteFloat(position.X);
            buf.WriteFloat(position.Y);
            buf.WriteFloat(position.Z);
            SendPacket(buf, PacketFlags.Reliable);
        }

        public void SyncObject(Obj obj)
        {
            MessageBuffer buf = CreatePacket(Packets.OBJECT);
            buf.WriteByte(0);
            buf.WriteUInt32((uint)obj.ID);
            buf.WriteUInt32((uint)obj.UID);
            buf.WriteByte((byte)obj.State);
            buf.WriteFloat(obj.transform.position.X);
            buf.WriteFloat(obj.transform.position.Y);
            buf.WriteFloat(obj.transform.position.Z);
            buf.WriteFloat(obj.transform.rotation.X);
            buf.WriteFloat(obj.transform.rotation.Y);
            buf.WriteFloat(obj.transform.rotation.Z);
            buf.WriteFloat(obj.transform.scale.X);
            buf.WriteFloat(obj.transform.scale.Y);
            buf.WriteFloat(obj.transform.scale.Z);

            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);

        }

        public void SendBusyState(bool busy, int duration = 0, string name = "none")
        {
            MessageBuffer buf = CreatePacket(Packets.BUSY_FLAG);
            buf.WriteByte(0);
            buf.WriteByte((byte)(busy ? 1 : 0));
            buf.WriteUInt32((uint)duration);
            buf.WriteByte((byte)name.Length);
            buf.WriteString(name);
            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);

        }

        public void EndDialog(uint uid, string text)
        {
            var buf = CreatePacket(Packets.DIALOG_ACTION);
            buf.WriteByte((byte)1);
            buf.WriteUInt32(uid);
            buf.WriteUInt16((ushort)text.Length);
            buf.WriteString(text);
            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void SendInvite(Player sender, int partyID) {
            MessageBuffer buf = CreatePacket(Packets.PARTY);

            buf.WriteByte(0); //send invite
            buf.WriteInt32(partyID);
            buf.WriteByte((byte) sender.Name.Length);
            buf.WriteString(sender.Name);

            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void SendOtherPlayerTradeAccept()
        {
            MessageBuffer buf = CreatePacket(Packets.TRADE);
            buf.WriteByte(4);
            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void SendTradeUnAccept() {
            MessageBuffer buf = CreatePacket(Packets.TRADE);
            buf.WriteByte(7);
            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void UpdateParty(List<Player> members) {
            MessageBuffer buf = CreatePacket(Packets.PARTY);

            buf.WriteByte(3); //send party update

            buf.WriteInt32(members.Count);
            foreach(Player p in members) {
                buf.WriteUInt32(p.UID);
                buf.WriteByte((byte)p.Name.Length);
                buf.WriteString(p.Name);
                buf.WriteByte((byte)(p.Party.IsOwner(p) ? 1 : 0));
                buf.WriteByte((byte)p.Skills.GetCombatLevel());
                buf.WriteUInt16((ushort)p.CurrentHealth);
                buf.WriteUInt16((ushort)p.GetMaxHealth());
                // int healthPercent = (int)(((float)p.CurrentHealth / (float)p.GetMaxHealth()) * 100f);
                // buf.WriteByte((byte)healthPercent);
            }

            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void SendHitSplat(Player player, Npc npc, int damage, int type)
        {
            MessageBuffer buf = CreatePacket(Packets.DAMAGE);

            buf.WriteByte(3); //send party info update
            buf.WriteByte(player == null ? (byte)0 : (byte)1);
            buf.WriteUInt32(player != null ? player.UID : (uint)npc.UID);
            buf.WriteUInt16((ushort)damage);
            buf.WriteByte((byte)type);
            this.player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void UpdatePartyInfo(Player updatedPlayer) {
            MessageBuffer buf = CreatePacket(Packets.PARTY);

            buf.WriteByte(4); //send party info update
            buf.WriteUInt32(updatedPlayer.UID);
            buf.WriteByte(Convert.ToByte(updatedPlayer.Skills.GetCombatLevel()));
            buf.WriteUInt16((ushort)updatedPlayer.CurrentHealth);
            //int healthPercent = (int) (((float)updatedPlayer.CurrentHealth / (float)updatedPlayer.GetMaxHealth()) * 100f);
          //  buf.WriteByte((byte)healthPercent);

            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void UpdatePartyBuffs(Player updatedPlayer, Buff buff) {
            MessageBuffer buf = CreatePacket(Packets.PARTY);

            buf.WriteByte(8); //send party buff update
            buf.WriteUInt32(updatedPlayer.UID);

            int buffID = buff.BuffID;
            float[] arg = buff.ArgArray;

            buf.WriteByte((byte)(int)DataManager.BuffDefinitions[buffID].SpellType); // type ID
            buf.WriteUInt16((ushort)buffID); // buff ID
            buf.WriteUInt32((uint)buff.Duration); // duration in milliseconds

            buf.WriteByte((byte)arg.Length);
            foreach (float f in arg)
                buf.WriteFloat(f);

            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void UpdatePartyPvpState(bool state) {
            MessageBuffer buf = CreatePacket(Packets.PARTY);

            buf.WriteByte(10); //send pvp state
            buf.WriteBoolean(state);

            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void StartTrade(Player target)
        {
            MessageBuffer buf = CreatePacket(Packets.TRADE);
            buf.WriteByte(6);
            buf.WriteUInt32(target.UID);
            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void DisbandParty() {
            MessageBuffer buf = CreatePacket(Packets.PARTY);
            buf.WriteByte(6); //send party disband
            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void TradeUpdateTheirItems(List<Item> items)
        {
            MessageBuffer buf = CreatePacket(Packets.TRADE);
            buf.WriteByte(2);
            buf.WriteByte((byte)items.Count);
            foreach (var itm in items)
            {
                buf.WriteUInt16(itm.ID);
                buf.WriteUInt32((uint)itm.Amount);
            }
            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void SendLogout(string reason = null)
        {
            try
            {
                if (peer.State != PeerState.Connected)
                    return;

                MessageBuffer buf = CreatePacket(Packets.LOGOUT_REQUEST);
                buf.WriteByte(0);
                if (reason != null)
                {
                    buf.WriteInt16((short)reason.Length);
                    buf.WriteString(reason);
                }
                else buf.WriteByte(0);

                player.NetworkActions.SendPacket(buf, PacketFlags.Reliable, true);
            }
            catch (Exception e)
            {
                Server.Error(e);
            }
        }

        public void TradeUpdateItem(ushort id, int amount)
        {
            MessageBuffer buf = CreatePacket(Packets.TRADE);
            buf.WriteByte(3);
            buf.WriteUInt16(id);
            buf.WriteUInt32((uint)amount);
            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void TradeRequest(Player sender)
        {
            MessageBuffer buf = CreatePacket(Packets.TRADE);
            buf.WriteByte(0);

            buf.WriteUInt32(sender.UID);
            buf.WriteByte((byte)sender.Name.Length);
            buf.WriteString(sender.Name);
            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void EndTrade()
        {
            MessageBuffer buf = CreatePacket(Packets.TRADE);
            buf.WriteByte(5);
            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void SendDialog(string title, string text, string summary, bool player, params string[] options)
        {
            int count = options.Length;

            MessageBuffer buf = CreateLargePacket(Packets.DIALOG_ACTION);
            buf.WriteByte(0); // 1 for close?
            buf.WriteByte(player ? (byte)0 : (byte)1);
            buf.WriteByte((byte)title.Length);
            buf.WriteString(title);
            if(summary == null)
            {
                buf.WriteByte((byte)0);
            } else
            {
                buf.WriteByte((byte)summary.Length);
                buf.WriteString(summary);
            }

            buf.WriteUInt16((ushort)text.Length);
            buf.WriteString(text);
            buf.WriteByte((byte)count);
            foreach (var str in options)
            {

                buf.WriteUInt16((ushort)str.Length);
                buf.WriteString(str);
            }

            SendPacket(buf, PacketFlags.Reliable);

        }

        public void SyncNpc(Npc npc, bool remove = false)
        {
            MessageBuffer buf = CreatePacket(Packets.NPC);
            buf.WriteByte(remove ? (byte)1 : (byte)0);
            CreateNPCBuffer(buf, npc);

            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void SyncNpcs()
        {
            MessageBuffer buf = CreateLargePacket(Packets.NPC);
            buf.WriteByte(2);

            List<Npc> toSync = player.Viewport.NpcsInView.Where(n => !n.Dead && !n.DeSpawned).ToList();
            buf.WriteInt32(toSync.Count);
            foreach (Npc npc in toSync){
                CreateNPCBuffer(buf, npc);
            }

            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        private void CreateNPCBuffer(MessageBuffer buf, Npc npc) {
            buf.WriteUInt32((uint)npc.ID);
            buf.WriteUInt32((uint)npc.UID);
            buf.WriteFloat(npc.Transform.position.X);
            buf.WriteFloat(npc.Transform.position.Y);
            buf.WriteFloat(npc.Transform.position.Z);
            buf.WriteFloat(npc.SpawnDefinition.Direction);
            buf.WriteBoolean(npc.SpawnDefinition.CanMove);
            buf.WriteUInt32((uint)npc.SpawnDefinition.IdleAnimation);

            buf.WriteUInt32((uint)npc.CurrentHealth);
            buf.WriteDouble(npc.Scale);
        }

        public void SendInventory()
        {
            MessageBuffer buf = CreatePacket(Packets.INVENTORY_SYNC);
            buf.WriteByte(0);

            int count = 0;
            for (int i = 0; i < player.Inventory.items.Length; i++)
            {
                if (player.Inventory.items[i] == null)
                    continue;
                count++;
            }

            buf.WriteByte((byte)count);

            for (int i = 0; i < player.Inventory.items.Length; i++)
            {
                var item = player.Inventory.items[i];
                if (item == null)
                    continue;
                buf.WriteByte((byte)i); // slot
                buf.WriteUInt16((ushort)item.Item.Definition.ItemID);
                buf.WriteUInt32((uint)item.Item.UID);
                buf.WriteUInt32((uint)item.Item.Amount);
            }

            SendPacket(buf, PacketFlags.Reliable);
        }

        public void StopInteracting()
        {
            MessageBuffer buf = CreatePacket(Packets.BUSY_FLAG);
            buf.WriteByte(1);
            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void ItemGain(int id, int amount)
        {
            if (id == 16)
            {
                Server.Instance.Delay(200, (timer, arg) =>
                {
                    PlayPickupSound(id, true);

                    //if (id == 16)
                    //    PlaySound("gold coins");
                    //else if (id == 0)
                    //    PlaySound("receive log");
                    //else if (id == 1 || id == 6)
                    //    PlaySound("receive ore");
                    //else
                    //{
                    //    PlaySound("gen_invadd");
                    //}
                    MessageBuffer buf = CreatePacket(Packets.ITEM);
                    buf.WriteByte(0);
                    buf.WriteUInt32((uint)id);
                    buf.WriteUInt32((uint)amount);
                    SendPacket(buf, PacketFlags.Reliable);

                });
            }
            else
            {
                PlayPickupSound(id, true);

                MessageBuffer buf = CreatePacket(Packets.ITEM);
                buf.WriteByte(0);
                buf.WriteUInt32((uint)id);
                buf.WriteUInt32((uint)amount);
                //SendMessage("<color=#EECA96>Obtained <color=#FFD056>" + (amount > 1 ? (amount + "x [") : "[") + DataManager.ItemDefinitions[id].ItemName + "]");
                SendPacket(buf, PacketFlags.Reliable);
            }


        }

        public void SyncVaultItem(bool add, Item item)
        {
            MessageBuffer buf = CreatePacket(Packets.VAULT);
            buf.WriteByte(1); // sync item
            buf.WriteByte((byte)(add ? 1 : 0));
            buf.WriteUInt16((ushort)item.Definition.ItemID);
            buf.WriteUInt32((uint)item.Amount);
            SendPacket(buf, PacketFlags.Reliable);
        }


        public void SendCombatStance()
        {
            MessageBuffer buf = CreatePacket(Packets.COMBAT_STANCE);
            buf.WriteByte(0); 
            buf.WriteByte((byte)player.Stance);
            SendPacket(buf, PacketFlags.Reliable);
        }
        public void SyncVault()
        {
            MessageBuffer buf = CreateLargePacket(Packets.VAULT);
            buf.WriteByte(0); // sync all items
            ushort count = 0;
            for (int i = 0; i < player.Vault.items.Length; i++)
            {
                if (player.Vault.items[i] == null)
                    continue;
                count++;
            }

            buf.WriteUInt16(count);

            for (ushort i = 0; i < player.Vault.items.Length; i++)
            {
                var item = player.Vault.items[i];
                if (item == null)
                    continue;
                buf.WriteUInt16(i); // slot
                buf.WriteUInt16((ushort)item.Definition.ItemID);
                buf.WriteUInt32((uint)item.Amount);
                // Server.Log(item.Definition.ItemID);
            }

            SendPacket(buf, PacketFlags.Reliable);
        }

        public void SendLoginResult(int res, string error = null)
        {
            MessageBuffer buf = CreatePacket(Packets.LOGIN);
            buf.WriteByte((byte)res);
            if(error != null && (res == 1 || res == 2))
            {
                buf.WriteInt16((short)error.Length);
                buf.WriteString(error);
            }

            else if (res == byte.MaxValue)
            {
                buf.WriteInt16((short)error.Length);
                buf.WriteString(error);
            } else { 
            
                buf.WriteUInt32(player.UID);
               // Server.Log("Sending char data size: " + player.charData.Length);
                buf.WriteByte((byte)(player.charData == null ? 0 : player.charData.Length));
                if(player.charData != null && player.charData.Length > 0)
                {
                    foreach(var acc in player.charData)
                    {
                        if(acc != null)
                        {
                            buf.WriteUInt32((uint)acc.characterId);
                            buf.WriteInt32(acc.map == null ? 0 : (int)acc.map);
                            buf.WriteByte((byte)acc.combatLevel);
                            buf.WriteInt16((byte)acc.totalLevel);
                            buf.WriteByte((byte)acc.username.Length);
                            buf.WriteString(acc.username);
                            buf.WriteInt32(acc.clan == null ? 0 : (int)acc.clan);
                            buf.WriteInt32(0);

                            
                            if(acc.appearance == null || acc.appearance.Count < 9 ) //why was this set to 11
                            {
                                acc.appearance = new List<AppearanceData>();
                                for(int i=0; i < 9; i++)
                                {
                                    acc.appearance.Add(new AppearanceData() { slotId = i, clothingId = 0, slotColor1 = 0 });
                                }
                            }
                            List<AppearanceData> d = acc.appearance.OrderBy(e => e.slotId).ToList();

                            buf.WriteByte((byte)d[0].clothingId);
                            buf.WriteByte((byte)d[1].clothingId);
                            buf.WriteByte((byte)d[2].clothingId);
                            buf.WriteByte((byte)d[2].slotColor1);
                            buf.WriteByte((byte)d[3].clothingId);
                            buf.WriteByte((byte)d[3].slotColor1);
                            buf.WriteByte((byte)d[4].clothingId);
                            buf.WriteByte((byte)d[4].slotColor1);
                            buf.WriteByte((byte)d[5].clothingId);
                            buf.WriteByte((byte)d[5].slotColor1);
                            buf.WriteByte((byte)d[6].clothingId);
                            buf.WriteByte((byte)d[6].slotColor1);
                            buf.WriteByte((byte)d[7].clothingId);
                            buf.WriteByte((byte)d[7].slotColor1);
                            buf.WriteByte((byte)d[8].clothingId);
                            buf.WriteByte((byte)d[8].slotColor1);

                            buf.WriteByte((byte)acc.equipment.Count);
                            foreach(Database.Equipment equip in acc.equipment)
                            {
                                buf.WriteByte((byte)equip.slotId);
                                buf.WriteInt32(equip.equipmentItemId);
                            }

                          //  buf.WriteByte(acc.equipment)
                            /*  if (acc.title != null)
                              {

                                  string title = Server.Instance.AllTitles[(int)acc.title].Display;
                                  if (title != null)
                                  {
                                      buf.WriteByte((byte)title.Length);
                                      buf.WriteString(title);
                                  }
                              }
                              else
                              {
                                  buf.WriteByte(0);
                              }*/


                        }
                    }
                  //  buf.WriteUInt32()
                }
                    
            }

            SendPacket(buf, PacketFlags.Reliable);
        }

        public void Ping()
        {
            MessageBuffer buf = CreatePacket(Packets.PING);
            buf.WriteByte(0);
            SendPacket(buf, PacketFlags.Reliable);
        }

        public void RemovePlayer(uint uid)
        {
            MessageBuffer buf = CreatePacket(Packets.SPAWN_PLAYER);
            buf.WriteByte(1);
            buf.WriteUInt32(uid);
            SendPacket(buf, PacketFlags.Reliable);
        }


        public void SendRank(Rank rank)
        {
            MessageBuffer buf = CreatePacket(Packets.PLAYER_RANK);
            buf.WriteByte(0);
            buf.WriteByte((byte)rank);
            SendPacket(buf, PacketFlags.Reliable);
        }

        public void SendJump(int jump)
        {

            lock (player.Viewport.PlayersInView)
            {
                foreach (Player p in player.Viewport.PlayersInView)
                {
                    if (p == player)
                        continue;

                    MessageBuffer buf = CreatePacket(Packets.MOVE);
                    buf.WriteByte(1);
                    buf.WriteUInt32(player.UID);
                    buf.WriteInt32(jump);
                    p.NetworkActions.SendPacket(buf, PacketFlags.Reliable);

                }
            }
        }
        public void SendRoll(int action)
        {
            lock (player.Viewport.PlayersInView)
            {
                foreach (Player p in player.Viewport.PlayersInView)
                {
                    if (p == player)
                        continue;

                    MessageBuffer buf = CreatePacket(Packets.MOVE);
                    buf.WriteByte(2);
                    buf.WriteUInt32(player.UID);
                    buf.WriteInt32(action);
                    p.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
                }
            }
        }
        public void SendSwim(int swim, bool trigger)
        {
            lock (player.Viewport.PlayersInView)
            {
                foreach (Player p in player.Viewport.PlayersInView)
                {
                    if (p == player)
                        continue;

                    MessageBuffer buf = CreatePacket(Packets.MOVE);
                    buf.WriteByte(3);
                    buf.WriteUInt32(player.UID);
                    buf.WriteInt32(swim);
                    buf.WriteBoolean(trigger);
                    p.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
                }
            }
        }


        public void SendAnimation(uint state, uint shift, uint direction, float DirV, float DirH)
        {
            lock (player.Viewport.PlayersInView)
            {
                foreach (Player p in player.Viewport.PlayersInView)
                {
                    if (p == player)
                        continue;

                    MessageBuffer buf = CreatePacket(Packets.ANIMATION);
                    buf.WriteByte(0);
                    buf.WriteUInt32(player.UID);
                    buf.WriteUInt32(state);
                    buf.WriteUInt32(shift);
                    buf.WriteUInt32(direction);
                    buf.WriteFloat(DirV);
                    buf.WriteFloat(DirH);

                    p.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
                }
            }
        }

        public void SendAttackAnimation(ushort shortToSend, bool freeze)
        {
            lock (player.Viewport.PlayersInView)
            {
                foreach (Player p in player.Viewport.PlayersInView)
                {
                    if (p == player)
                        continue;

                    MessageBuffer buf = CreatePacket(Packets.ANIMATION);
                    buf.WriteByte(4);
                    buf.WriteUInt32(player.UID);
                    buf.WriteUInt16(shortToSend);
                    buf.WriteBoolean(freeze);
                    p.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
                }
            }
        }

        public void SendBowPull(bool bowPull)
        {
            lock (player.Viewport.PlayersInView)
            {
                foreach (Player p in player.Viewport.PlayersInView)
                {
                    if (p == player)
                        continue;

                    MessageBuffer buf = CreatePacket(Packets.ANIMATION);
                    buf.WriteByte(5);
                    buf.WriteUInt32(player.UID);
                    buf.WriteBoolean(bowPull);
                    p.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
                }
            }
        }

        public void SendAnimationType(int subtype, ushort shortToSend, bool includePlayer = false) {
            lock (player.Viewport.PlayersInView) {
                foreach (Player p in player.Viewport.PlayersInView) {
                    if (p == player && !includePlayer)
                        continue;

                    MessageBuffer buf = CreatePacket(Packets.ANIMATION);
                    buf.WriteByte((byte)subtype);
                    buf.WriteUInt32(player.UID);
                    buf.WriteUInt16(shortToSend);
                    p.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
                }
            }
        }

        public void SendSkillActionTrigger(ushort shortToSend) {
            lock (player.Viewport.PlayersInView) {
                foreach (Player p in player.Viewport.PlayersInView) {
                    if (p == player)
                        continue;

                    MessageBuffer buf = CreatePacket(Packets.SKILL_ACTION_TRIGGER);
                    buf.WriteByte(0);
                    buf.WriteUInt32(player.UID);
                    buf.WriteUInt16(shortToSend);
                    p.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
                }

            }
        }

        public void SendTeleportAnimation(ushort shortToSend)
        {
            lock (player.Viewport.PlayersInView)
            {
                foreach (Player p in player.Viewport.PlayersInView)
                {
                    if (p == player)
                        continue;

                    MessageBuffer buf = CreatePacket(Packets.ANIMATION);
                    buf.WriteByte(1);
                    buf.WriteUInt32(player.UID);
                    buf.WriteUInt16(shortToSend);
                    p.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
                }
            }
        }

        public void SendBlankToPlayersInView(ushort packetToSendTo)
        {
            lock (player.Viewport.PlayersInView)
            {
                foreach (Player p in player.Viewport.PlayersInView)
                {
                    if (p == player)
                        continue;

                    MessageBuffer buf = CreatePacket(packetToSendTo);
                    buf.WriteUInt32(player.UID);
                    p.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
                }
            }
        }

        public void SendAdminMode(bool mode)
        {
            MessageBuffer buf = CreatePacket(Packets.ADMIN_MODE);
            buf.WriteByte(0);
            buf.WriteByte(mode ? (byte)1 : (byte)0);
            SendPacket(buf, PacketFlags.Reliable);
        }

        public void SendNpcAnimation(int state, Npc npc)
        {
            MessageBuffer buf = CreatePacket(Packets.NPC);
            buf.WriteByte(9);
            buf.WriteUInt32((uint)npc.UID);
            buf.WriteUInt32((uint)state);
            SendPacket(buf, PacketFlags.Reliable | PacketFlags.Unsequenced);
        }
        public void SendSpecialAttack(Vector3 position, int type)
        {
            MessageBuffer buf = CreatePacket(Packets.PROJECTILE);
            buf.WriteByte(2);
            buf.WriteFloat(position.X);
            buf.WriteFloat(position.Y);
            buf.WriteFloat(position.Z);
            buf.WriteUInt32((uint)type);
            SendPacket(buf, PacketFlags.Reliable | PacketFlags.Unsequenced);
        }

        public void SendNpcPosition(Vector3 pos, Npc npc)
        {
            MessageBuffer buf = CreatePacket(Packets.NPC);
            buf.WriteByte(8);
            buf.WriteUInt32((uint)npc.UID);
            buf.WriteFloat(pos.X);
            buf.WriteFloat(pos.Y);
            buf.WriteFloat(pos.Z);
            SendPacket(buf, PacketFlags.Reliable);
        }

        public void SendNPCDeAgro(Npc npc)
        {
            MessageBuffer buf = CreatePacket(Packets.NPC);
            buf.WriteByte(7);
            buf.WriteUInt32((uint)npc.UID);
            SendPacket(buf, PacketFlags.Reliable);
        }

        public void SendMessage(string msg, MsgType type = MsgType.Server, int puid = -1, string user = "", Rank rank = Rank.SERVER)
        {
            MessageBuffer buf = CreatePacket(Packets.CHAT_MESSAGE);
            buf.WriteByte(0);
            buf.WriteInt32((int)type);
            buf.WriteInt32(puid);
            buf.WriteByte((byte)rank);
            buf.WriteByte((byte)msg.Length);
            buf.WriteString(msg);
            buf.WriteByte((byte)user.Length);
            buf.WriteString(user);
            SendPacket(buf, PacketFlags.Reliable);
        }


        public void SendBank() {
            MessageBuffer buf = CreatePacket(Packets.VAULT);
            buf.WriteByte(5);

            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void SendShop(ShopDef shop)
        {
            MessageBuffer buf = CreatePacket(Packets.SHOP);
            buf.WriteByte(0); // open shop / refresh items.
            buf.WriteUInt32((uint)shop.ID);
            buf.WriteByte((byte)(shop.General ? 1 : 0));
            buf.WriteByte((byte)(shop.CanSell ? 1 : 0));
            buf.WriteFloat(shop.BuyMultiplier);
            buf.WriteFloat(shop.SellMultiplier);
            buf.WriteInt16((short)shop.ShopName.Length);
            buf.WriteString(shop.ShopName);
            buf.WriteUInt16((ushort)shop.Items.Count);
            foreach (var ing in shop.Items)
            {
                buf.WriteUInt16((ushort)ing.ItemID);
                buf.WriteUInt32((uint)ing.Amount);
            }

            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void SendMovement(Vector3 pos, float roty, Vector3 velocity, byte type, bool force)
        {
            lock (player.Viewport.PlayersInView)
            {
                foreach (Player p in player.Viewport.PlayersInView)
                {
                    if (p == player)
                        continue;

                    MessageBuffer buf = CreatePacket(Packets.MOVE);
                    buf.WriteByte(0);
                    buf.WriteUInt32(player.UID);
                    buf.WriteByte(type);

                    buf.WriteFloat(pos.X);
                    buf.WriteFloat(pos.Y);
                    buf.WriteFloat(pos.Z);

                    buf.WriteFloat(roty);
                    buf.WriteFloat(velocity.X);
                    buf.WriteFloat(velocity.Y);
                    buf.WriteFloat(velocity.Z);
                    buf.WriteBoolean(force);

                    p.NetworkActions.SendPacket(buf, PacketFlags.None);

                }
            }

        }

        public void SendDeath(bool start)
        {
            lock (player.Viewport.PlayersInView)
            {
                foreach (Player p in player.Viewport.PlayersInView)
                {
                    MessageBuffer buf = CreatePacket(Packets.SEND_DEATH);
                    buf.WriteByte(start ? (byte)0 : (byte)1);
                    buf.WriteUInt32(player.UID);
                    p.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
                }
            }
        }

        public void SendLocation()
        {
            MessageBuffer buf = CreatePacket(Packets.MOVE);
            buf.WriteByte(4);
            buf.WriteUInt32(player.UID);
            buf.WriteFloat(player.transform.position.X);
            buf.WriteFloat(player.transform.position.Y);
            buf.WriteFloat(player.transform.position.Z);
            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void SendAppearanceRequest()
        {
            MessageBuffer buf = CreatePacket(Packets.APPEARANCE_CREATED);
            buf.WriteByte(0);
            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void SendLocation(Player target)
        {
            MessageBuffer buf = CreatePacket(Packets.MOVE);
            buf.WriteByte(4);
            buf.WriteUInt32(target.UID);
            buf.WriteFloat(target.transform.position.X);
            buf.WriteFloat(target.transform.position.Y);
            buf.WriteFloat(target.transform.position.Z);
            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
        }

        public void PlaySound(string sound)
        {
            MessageBuffer buf = CreatePacket(Packets.PLAY_SOUND);
            buf.WriteByte(0);
            buf.WriteByte((byte)sound.Length);
            buf.WriteString(sound);
            SendPacket(buf, PacketFlags.Reliable);
        }

        public void PlayPickupSound(int id, bool pickup)
        {
            MessageBuffer buf = CreatePacket(Packets.PLAY_SOUND);
            buf.WriteByte(1);
            buf.WriteInt32(id);
            buf.WriteBoolean(pickup);
            SendPacket(buf, PacketFlags.Reliable);
        }

        public void OpenCraftingMenu(int id)
        {
            MessageBuffer buf = CreatePacket(Packets.OPEN_CRAFTING_MENU);
            buf.WriteByte((byte)id);
            SendPacket(buf, PacketFlags.Reliable);
        }

        public void PlayerDamaged(uint uid, int amount)
        {
            MessageBuffer buf = CreatePacket(Packets.DAMAGE);
            buf.WriteByte(0);
            buf.WriteUInt32(uid);
            buf.WriteUInt32((uint)amount);
            SendPacket(buf, PacketFlags.Reliable);
        }

        public void SyncPlayers()
        {
            lock (player.Viewport.PlayersInView)
            {
                foreach (Player p in player.Viewport.PlayersInView)
                {
                    if (p == player)
                        continue;

                    MessageBuffer buf = CreatePacket(Packets.SYNC_POS);
                    buf.WriteByte(0);
                    buf.WriteUInt32(p.UID);
                    buf.WriteFloat(p.transform.position.X);
                    buf.WriteFloat(p.ServerHeight());
                    buf.WriteFloat(p.transform.position.Z);
                    buf.WriteFloat(p.transform.rotation.X);
                    buf.WriteFloat(p.transform.rotation.Y);
                    buf.WriteFloat(p.transform.rotation.Z);

                    player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);

                    player.NetworkActions.SendLocalPlayerEquipmentUpdate();
                    player.NetworkActions.SyncAllExp(p);
                }
            }
        }

        public void PlayerCustomUpdate()
        {
            lock (player.Viewport.PlayersInView)
            {
                foreach (Player p in player.Viewport.PlayersInView)
                { // send everyone on server your spawn data
                    MessageBuffer buf = CreatePacket(Packets.PLAYER_CUSTOM_UPDATE);
                    buf.WriteByte(0);
                    buf.WriteUInt32(player.UID);
                    buf.WriteByte((byte)player.Name.Length);
                    buf.WriteString(player.Name);
                    buf.WriteByte((byte)player.Skills.GetCombatLevel());
                    buf.WriteUInt16((ushort)player.Skills.GetSkillTotal());

                    List<AppearanceData> d = player.appearanceData.OrderBy(e => e.slotId).ToList();

                    buf.WriteByte((byte)d[0].clothingId);
                    buf.WriteByte((byte)d[1].clothingId);
                    buf.WriteByte((byte)d[2].clothingId);
                    buf.WriteByte((byte)d[2].slotColor1);
                    buf.WriteByte((byte)d[3].clothingId);
                    buf.WriteByte((byte)d[3].slotColor1);
                    buf.WriteByte((byte)d[4].clothingId);
                    buf.WriteByte((byte)d[4].slotColor1);
                    buf.WriteByte((byte)d[5].clothingId);
                    buf.WriteByte((byte)d[5].slotColor1);
                    buf.WriteByte((byte)d[6].clothingId);
                    buf.WriteByte((byte)d[6].slotColor1);
                    buf.WriteByte((byte)d[7].clothingId);
                    buf.WriteByte((byte)d[7].slotColor1);
                    buf.WriteByte((byte)d[8].clothingId);
                    buf.WriteByte((byte)d[8].slotColor1);



                    //buf.WriteByte((byte)(player.isMale ? 1 : 0));
                    //buf.WriteByte((byte)player.shirtIndex);
                    //buf.WriteByte((byte)player.pantsIndex);                      // THESE ARE NOT IN THE COORECT ORDER
                    //buf.WriteByte((byte)player.shoeIndex);
                    //buf.WriteByte((byte)player.hairIndex);
                    //buf.WriteByte((byte)player.facialHairIndex);
                    //buf.WriteByte((byte)player.skinColourIndex);
                    //buf.WriteByte((byte)player.hairColourIndex);
                    //buf.WriteByte((byte)player.faceialHairColourIndex);
                    //buf.WriteByte((byte)player.pantsColourIndex);
                    //buf.WriteByte((byte)player.shoesColourIndex);
                    //buf.WriteByte((byte)player.shirtColourIndex);

                    p.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
                }
            }
  
        }

        public void PlayerChangeGender()
        {
            lock (player.Viewport.PlayersInView)
            {
                foreach (Player p in player.Viewport.PlayersInView)
                { // send everyone on server your spawn data
                    MessageBuffer buf = CreatePacket(Packets.APPEARANCE_CREATED);
                    buf.WriteByte(1);
                    buf.WriteUInt32(player.UID);
                    buf.WriteBoolean(player.isMale);


                    p.NetworkActions.SendPacket(buf, PacketFlags.Reliable);
                }
            }
        }

        public void ChangeMap(int MapID, int LandID, Vector3 Position, Action onLoad)
        {

            MessageBuffer buf = CreatePacket(Packets.MAP_CHANGE);
            buf.WriteByte(0);
            buf.WriteInt32(MapID);
            buf.WriteInt32(LandID);
            buf.WriteFloat(Position.X);
            buf.WriteFloat(Position.Y);
            buf.WriteFloat(Position.Z);
            if (MapID != 0)
            {
                buf.WriteByte((byte)0);
                buf.WriteByte((byte)0);
            }
            else
            {
                var chippy = Server.Instance.Maps[MapID].GetServerTime();
                buf.WriteByte((byte)chippy.X);
                buf.WriteByte((byte)chippy.Y);
            }
            SendPacket(buf, PacketFlags.Reliable);
            player.OnMapChange = onLoad;


        }

        public void SpawnPlayer(Player p)
        {
            MessageBuffer buf = CreatePacket(Packets.SPAWN_PLAYER);
            buf.WriteByte(0); //spawn type
            buf.WriteUInt32(p.UID);
            buf.WriteByte((byte)p.Name.Length);
            buf.WriteString(p.Name);
            buf.WriteUInt16((ushort)p.Skills.GetSkillTotal());
            buf.WriteByte((byte)p.Rank);
            if (p.clan != null && p.clan.Length > 0 && p.clan != "NULL")
            {
                buf.WriteByte((byte)p.clan.Length);
                buf.WriteString(p.clan);
            }
            else
            {
                buf.WriteByte(0);
            }
            int titleID = p.TitleManager.CurrentTitle != null ? p.TitleManager.CurrentTitle.ID : 0;
            buf.WriteInt16((short)titleID);


            List<AppearanceData> d = p.appearanceData.OrderBy(e => e.slotId).ToList();

            buf.WriteByte((byte)d[0].clothingId);
            buf.WriteByte((byte)d[1].clothingId);
            buf.WriteByte((byte)d[2].clothingId);
            buf.WriteByte((byte)d[2].slotColor1);
            buf.WriteByte((byte)d[3].clothingId);
            buf.WriteByte((byte)d[3].slotColor1);
            buf.WriteByte((byte)d[4].clothingId);
            buf.WriteByte((byte)d[4].slotColor1);
            buf.WriteByte((byte)d[5].clothingId);
            buf.WriteByte((byte)d[5].slotColor1);
            buf.WriteByte((byte)d[6].clothingId);
            buf.WriteByte((byte)d[6].slotColor1);
            buf.WriteByte((byte)d[7].clothingId);
            buf.WriteByte((byte)d[7].slotColor1);
            buf.WriteByte((byte)d[8].clothingId);
            buf.WriteByte((byte)d[8].slotColor1);
            buf.WriteByte((byte)p.Skills.GetCombatLevel());

           // Server.Log("POS: " + p.transform.position.X + ", " + p.transform.position.Y + ", " + p.transform.position.Z);
            buf.WriteFloat(p.transform.position.X);
            buf.WriteFloat(p.transform.position.Y);
            buf.WriteFloat(p.transform.position.Z);
            buf.WriteFloat(p.transform.rotation.X);
            buf.WriteFloat(p.transform.rotation.Y);
            buf.WriteFloat(p.transform.rotation.Z);
            buf.WriteFloat(p.transform.scale.X);
            buf.WriteFloat(p.transform.scale.Y);
            buf.WriteFloat(p.transform.scale.Z);

            player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);

            SyncHealth(p);

            SyncAllExp(p);
        }
        // this needs to be split up so only 
        public void SpawnPlayer(bool skip = false)
        {
            lock (player.Viewport.PlayersInView)
            {
                foreach (Player p in player.Viewport.PlayersInView)
                { // send everyone on server your spawn data
                    if(skip && p == player) {
                        continue;
                    }
                
                    MessageBuffer buf = CreatePacket(Packets.SPAWN_PLAYER);
                    buf.WriteByte(0); //spawn type
                    buf.WriteUInt32(player.UID);
                    buf.WriteByte((byte)player.Name.Length);
                    buf.WriteString(player.Name);
                    buf.WriteUInt16((ushort)player.Skills.GetSkillTotal());
                    buf.WriteByte((byte)player.Rank);
                    if (player.clan != null && player.clan.Length > 0 && player.clan != "NULL")
                    {
                        buf.WriteByte((byte)player.clan.Length);
                        buf.WriteString(player.clan);
                    }
                    else
                    {
                        buf.WriteByte(0);
                    }

                    int titleID = player.TitleManager.CurrentTitle != null ? player.TitleManager.CurrentTitle.ID : 0;
                    buf.WriteInt16((short)titleID);

                    List<AppearanceData> d = player.appearanceData.OrderBy(e => e.slotId).ToList();

                    buf.WriteByte((byte)d[0].clothingId);
                    buf.WriteByte((byte)d[1].clothingId);
                    buf.WriteByte((byte)d[2].clothingId);
                    buf.WriteByte((byte)d[2].slotColor1);
                    buf.WriteByte((byte)d[3].clothingId);
                    buf.WriteByte((byte)d[3].slotColor1);
                    buf.WriteByte((byte)d[4].clothingId);
                    buf.WriteByte((byte)d[4].slotColor1);
                    buf.WriteByte((byte)d[5].clothingId);
                    buf.WriteByte((byte)d[5].slotColor1);
                    buf.WriteByte((byte)d[6].clothingId);
                    buf.WriteByte((byte)d[6].slotColor1);
                    buf.WriteByte((byte)d[7].clothingId);
                    buf.WriteByte((byte)d[7].slotColor1);
                    buf.WriteByte((byte)d[8].clothingId);
                    buf.WriteByte((byte)d[8].slotColor1);


                    buf.WriteByte((byte)player.Skills.GetCombatLevel());

                    buf.WriteFloat(player.transform.position.X);
                    buf.WriteFloat(player.transform.position.Y);
                    buf.WriteFloat(player.transform.position.Z);
                    buf.WriteFloat(player.transform.rotation.X);
                    buf.WriteFloat(player.transform.rotation.Y);
                    buf.WriteFloat(player.transform.rotation.Z);
                    buf.WriteFloat(player.transform.scale.X);
                    buf.WriteFloat(player.transform.scale.Y);
                    buf.WriteFloat(player.transform.scale.Z);

                    p.NetworkActions.SyncHealth(player);
                    p.NetworkActions.SendPacket(buf, PacketFlags.Reliable);

                    if (!skip)
                    {
                        p.NetworkActions.SyncAllExp(player);
                    }
                    //Server.Log("Sent SpawnPlayer()" + player.Name + " to " + p.Name);
                }
            }
            lock (player.Viewport.PlayersInView)
            {
                foreach (Player p in player.Viewport.PlayersInView)
                { // send you everyone on servers spawn data
                    if (p == player)
                        continue;
                    MessageBuffer buf = CreatePacket(Packets.SPAWN_PLAYER);
                    buf.WriteByte(0); //spawn type
                    buf.WriteUInt32(p.UID);
                    buf.WriteByte((byte)p.Name.Length);
                    buf.WriteString(p.Name);
                    buf.WriteUInt16((ushort)p.Skills.GetSkillTotal());
                    buf.WriteByte((byte)p.Rank);
                    if (p.clan != null && p.clan.Length > 0 && p.clan != "NULL")
                    {
                        buf.WriteByte((byte)p.clan.Length);
                        buf.WriteString(p.clan);
                    }
                    else
                    {
                        buf.WriteByte(0);
                    }
                    int titleID = p.TitleManager.CurrentTitle != null ? p.TitleManager.CurrentTitle.ID : 0;
                    buf.WriteInt16((short)titleID);


                    List<AppearanceData> d = p.appearanceData.OrderBy(e => e.slotId).ToList();

                    buf.WriteByte((byte)d[0].clothingId);
                    buf.WriteByte((byte)d[1].clothingId);
                    buf.WriteByte((byte)d[2].clothingId);
                    buf.WriteByte((byte)d[2].slotColor1);
                    buf.WriteByte((byte)d[3].clothingId);
                    buf.WriteByte((byte)d[3].slotColor1);
                    buf.WriteByte((byte)d[4].clothingId);
                    buf.WriteByte((byte)d[4].slotColor1);
                    buf.WriteByte((byte)d[5].clothingId);
                    buf.WriteByte((byte)d[5].slotColor1);
                    buf.WriteByte((byte)d[6].clothingId);
                    buf.WriteByte((byte)d[6].slotColor1);
                    buf.WriteByte((byte)d[7].clothingId);
                    buf.WriteByte((byte)d[7].slotColor1);
                    buf.WriteByte((byte)d[8].clothingId);
                    buf.WriteByte((byte)d[8].slotColor1);
                    buf.WriteByte((byte)p.Skills.GetCombatLevel());
                    buf.WriteFloat(p.transform.position.X);
                    buf.WriteFloat(p.transform.position.Y);
                    buf.WriteFloat(p.transform.position.Z);
                    buf.WriteFloat(p.transform.rotation.X);
                    buf.WriteFloat(p.transform.rotation.Y);
                    buf.WriteFloat(p.transform.rotation.Z);
                    buf.WriteFloat(p.transform.scale.X);
                    buf.WriteFloat(p.transform.scale.Y);
                    buf.WriteFloat(p.transform.scale.Z);

                    player.NetworkActions.SendPacket(buf, PacketFlags.Reliable);

                    SyncHealth(p);

                    SyncAllExp(p);
                }
            }
        }


        public static int SMALL_PACKET_COUNT = 0;

        public static int LARGE_PACKET_COUNT = 0;

        public MessageBuffer CreatePacket(ushort id)
        {
            MessageBuffer buf = new MessageBuffer(new byte[SMALL_PACKET_MAX_SIZE], true);

            buf.WriteUInt16(id);
            buf.WriteUInt16(Packets.NULL_LENGTH);

            return buf;
        }

        public MessageBuffer CreateLargePacket(ushort id)
        {
            MessageBuffer buf = new MessageBuffer(new byte[LARGE_PACKET_MAX_SIZE], true);

            buf.WriteUInt16(id);
            buf.WriteUInt16(Packets.NULL_LENGTH);

            return buf;
        }
        public static int SMALL_PACKET_MAX_SIZE = 2048;
        public static int LARGE_PACKET_MAX_SIZE = 100384;

        public void SendPacket(MessageBuffer buf, PacketFlags flag, bool instant = false)
        {

            Message msg = new Message();
            msg.buf = buf;
            msg.flag = flag;
            msg.peer = peer;
            if (buf.Size == SMALL_PACKET_MAX_SIZE)
                msg.small = true;
            else msg.small = false;
            if (!instant)
            {
                Server.Instance.network.OutgoingPackets.Enqueue(msg);
            }
            else
                WritePacket(msg);

        }

        public struct Message
        {
            public MessageBuffer buf;
            public PacketFlags flag;
            public bool small;
            public Peer peer;
        }

        static int count = 0;

        public static void WritePacket(Message message)
        {
            Packet data = default(Packet);
            int pos = (int)message.buf.Position;
            message.buf.Position = 2;
            message.buf.WriteUInt16((ushort)pos);
            data.Create(message.buf.Stream.ToArray(), pos, message.flag);//PacketFlags.Reliable);
            
            if (message.small)
            {
                if (data.Length > SMALL_PACKET_COUNT)
                    SMALL_PACKET_COUNT = data.Length;
            }
            else
            {
                if (data.Length > LARGE_PACKET_COUNT)
                    LARGE_PACKET_COUNT = data.Length;
            }
            if (count >= 20)
                count = 1;
            if (message.peer.State == PeerState.Connected)
                message.peer.Send(0, ref data);//message.flag == PacketFlags.Reliable ? (byte)0 : (byte)count, ref data);
            count++;
            try
            {
                message.buf.Dispose();
                message.buf = null;
            } catch(Exception e)
            {
                Server.Error(e);
            }
            //data.Dispose();
            //player.PacketsSent++;
        }
    }
}
