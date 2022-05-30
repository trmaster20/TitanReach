using System;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using TitanReach_Server.Utilities;
using TRShared;
using TRShared.Data.Definitions;
using TRShared.Data.Enums;

namespace TitanReach_Server.Network.Incoming
{
    class ADMIN_MODE : IncomingPacketHandler
    {
        public int GetID()
        {
            return Packets.ADMIN_MODE;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            if (p.Rank != Rank.ADMIN)
                return;
            byte subtype = packet.ReadByte();

            switch (subtype) {
                // Case 1 Not being used currently
                case 1:

                    p.SetPosition(Locations.PLAYER_SPAWN_POINT);
                    break;

                case 2:

                    p.SetPosition(new Vector3(p.transform.position.X, DataManager.GetHeight((int)p.transform.position.X, (int)p.transform.position.Z, p.Map.Land) + 1f, p.transform.position.Z));
                    break;

                case 3:

                    p.NetworkActions.SendMessage("Cur Location: X=" + (int)p.transform.position.X + " Z=" + (int)p.transform.position.Z + " Y=" + (int)p.transform.position.Y);
                    break;

                case 4:

                    foreach (Npc n in p.Map.Npcs) {
                        // n.NpcSpawnDefinition.RespawnTime = int.MaxValue;
                        n.Damage(1000, p, DamageType.MAGIC);
                    }
                    break;

                case 5:

                    p.HealMax();
                    break;

                case 6:

                    p.ToggleAdminMode();
                    break;

                case 7:

                    p.NetworkActions.SendBank();
                    break;

                case 8:

                    p.Invincible();
                    break;

                case 9:

                    p.MaxAllSkills();
                    break;

                case 10:

                    p.ResetAllSkills();
                    break;

                case 11:

                    p.SetSkill(packet.ReadByte(), packet.ReadByte());
                    break;

                case 12:

                    p.AdminChangeMap(packet.ReadByte());
                    break;

                case 13:

                    byte location = packet.ReadByte();
                    byte mapID = packet.ReadByte();

                    switch (location)
                    {
                        case 1:
                            Vector3 position = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
                            Server.Log(position);
                            p.SetPosition(position);
                            break;

                        case 2:
                            p.TeleportTo(mapID, packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
                            break;
                    }

                    break;
            }
        }
    }
}
