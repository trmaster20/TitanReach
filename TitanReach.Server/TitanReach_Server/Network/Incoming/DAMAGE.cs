using System;
using System.Linq;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using TitanReach_Server.Utilities;
using TRShared.Data.Enums;

namespace TitanReach_Server.Network.Incoming
{


    class DAMAGE : IncomingPacketHandler
    {
        public int GetID()
        {
            return Packets.DAMAGE;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            if (Environment.TickCount - p.LastFood < p.LastFoodDelayTime)
                return;

            int subtype = packet.ReadByte();
            switch (subtype) {
                case 1:
                    Player player = null;
                    uint puid = packet.ReadUInt32();


                    player = p.Map.Players.FirstOrDefault(x => x.UID == puid);
                    bool isProjectile = packet.ReadBoolean();


                    if (player == null) return;
                    if (player.Dead) return;

                    if (!CheckPvp(player, p)) return;

                    if (isProjectile) {
                        uint projId = packet.ReadUInt32();
                        foreach (Projectile projectile in p.Map.Projectiles) {
                            if (projId == projectile.projID) {


                                if (p.LastAttackedPlayer != null && p.LastAttackedPlayer == player && Environment.TickCount - p.LastAttackedPlayerTime < 500)
                                    return;
                                p.LastAttackedPlayer = player;
                                p.LastAttackedPlayerTime = Environment.TickCount;
                                DamageCalculator.PlayerProjAttackPlayer(p, projectile, player);
                            }
                        }
                    } else {
                        WeaponCategory weaponCategory;

                        int time = 750;
                        if (p.Equipment.EquippedItems[10] == null) {
                            weaponCategory = WeaponCategory.UNARMED;
                        } else {
                            weaponCategory = (WeaponCategory)p.Equipment.RIGHT_HAND.Definition.WeaponType;
                            time = (int)(p.Equipment.RIGHT_HAND.Definition.WeaponSpeed * 0.9f);
                        }
                        if (p.LastPlayerAttackTimes.ContainsKey(player.UID) && Environment.TickCount - p.LastPlayerAttackTimes[player.UID] < time)
                            return;
                        p.LastPlayerAttackTimes[player.UID] = Environment.TickCount;
                        DamageCalculator.PlayerAttackPlayer(p, player, weaponCategory);
                    }

                    p.NetworkActions.PlaySound("sword_impact");
                    break;

                case 2:
                    Npc npc = p.GetNpcByUID(packet.ReadUInt32());
                    isProjectile = packet.ReadBoolean();
                    if (npc == null) return;

                    if (npc.Definition.Friendly) return;

                    if (npc.Invulnerable) return;

                    if (npc.Dead) return;
                    if (isProjectile) {

                        uint projId = packet.ReadUInt32();
                        foreach (Projectile projectile in npc.Map.Projectiles) {
                            if (projId == projectile.projID && !projectile.Used) {

                                DamageCalculator.PlayerProjAttackNPC(p, projectile, npc);
                                projectile.Used = true;
                            }
                            p.PlayerTracking.TrackAttacking("Damaged " + npc.Definition.Name + "(ranged attack)");
                        }
                    } else {
                        int time = 750;
                        WeaponCategory weaponCategory;

                        if (p.Equipment.EquippedItems[10] == null) {
                            weaponCategory = WeaponCategory.UNARMED;
                        } else {
                            weaponCategory = (WeaponCategory)p.Equipment.RIGHT_HAND.Definition.WeaponType;
                            time = (int)(p.Equipment.RIGHT_HAND.Definition.WeaponSpeed * 0.9f);
                        }
                        if (p.LastNpcAttackTimes.ContainsKey(npc.UID) && Environment.TickCount - p.LastNpcAttackTimes[npc.UID] < time)
                            return;
                        p.LastNpcAttackTimes[npc.UID] = Environment.TickCount;
                        DamageCalculator.PlayerAttackNPC(p, npc, weaponCategory);
                        p.PlayerTracking.TrackAttacking("Damaged " + npc.Definition.Name + "(melee attack)");
                    }
                    break;

            }
            
        }
        private bool CheckPvp(Player p1, Player p2) {
            if (!p1.OnSameMap(p2)) {
                return false;
            }

            //if(Dynny.HasHotSinglesNearby()) return false;
            return p1.Map.PvpEnabled && CheckPartyPvp(p1, p2);
        }

        private bool CheckPartyPvp(Player p1, Player p2) {
            if (p1.HasParty() && p2.HasParty()) {
                return !(p1.Party.Equals(p2.Party) && !p1.Party.PvpEnabled);
            }
            return true;
        }
    }
}
