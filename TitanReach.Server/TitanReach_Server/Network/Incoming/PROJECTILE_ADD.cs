using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using TRShared;

namespace TitanReach_Server.Network.Incoming
{
    class PROJECTILE_ADD : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.PROJECTILE;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            byte subType = packet.ReadByte();

            int projectileID = (int)packet.ReadUInt32(); //not the uid
            int playerUID = (int)packet.ReadUInt32();
            Vector3 vel = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
            uint projID = packet.ReadUInt32();

            Projectile arrow = new Projectile(projectileID, p, projID) {
                direction = vel,
                position = p.transform.position - new Vector3(0, 1, 0),
                Targets = null
            };

            p.Map.Projectiles.Add(arrow);

            var ite = p.Equipment.EquippedItems[9];
            if (ite != null)
            {
                ite.Amount--;
                if (ite.Amount < 1)
                {
                    p.Equipment.Delete(9);
                    p.NetworkActions.SendEquipment();
                    p.NetworkActions.SendLocalPlayerEquipmentUpdate();
                }
                else
                {
                    p.NetworkActions.SendEquipment();
                }
            }

            lock (p.Map.Players)
            {
                p.Map.Players.ForEach(x => { if (x != p) x?.NetworkActions.SendProjectile(DataManager.ProjectileDefinitions[projectileID], vel, p); });
            }

        }
    }
}
