using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using TRShared;
using TRShared.Data.Definitions;

namespace TitanReach_Server.Network.Incoming
{
    class GROUND_ITEM : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.GROUND_ITEM;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            byte subtype = packet.ReadByte();

            GroundItem gi;
            uint uid = packet.ReadUInt32();
            int id = (int)packet.ReadUInt32();
            ItemDefinition def = DataManager.ItemDefinitions[id];
            gi = p.GetGroundItemByUID(uid);

            if (gi != null)
            {
                if (gi.Taken)
                    return;

                if (gi.ownerUID != 0 && gi.ownerUID != p.UID)
                {
                    Server.Suss(p, "Tried picking up an item he should have never been able to see");
                    return;
                }

                if (!Formula.InRadius(p.transform.position, gi.transform.position, 6))
                    return;

                if (p.Inventory.FreeSpace() < 1 && (!DataManager.ItemDefinitions[gi.Item.ID].IsStackable || !p.Inventory.HasItem(gi.Item.ID)))
                {
                    p.Error("Your Inventory is full!");
                    return;
                }

                InvItem ig = new InvItem(gi.Item.ID);
                ig.Item.Amount = gi.Item.Amount;
                p.Inventory.AddItem(ig.Item);

                ig.Item.UID = gi.Item.UID;
                p.NetworkActions.SendInventory();
                p.NetworkActions.ItemGain(ig.Item.ID, ig.Item.Amount);
                lock (p.Viewport.PlayersInView)
                {
                    p.Viewport.PlayersInView.ForEach(pl => pl?.NetworkActions.RemoveGroundItem(gi.groundItemUID));
                }
                p.NetworkActions.RemoveGroundItem(gi.groundItemUID);
                p.Viewport.groundItemsInView.Remove(gi);
                p.Map.GroundItems.Remove(gi);
                gi.Taken = true;
                gi = null;
            }
        }
    }
}
