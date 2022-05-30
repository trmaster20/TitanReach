using System.Collections.Generic;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using TitanReach_Server.Utilities;
using TRShared;
using TRShared.Data.Enums;

namespace TitanReach_Server.Network.Incoming
{
    class VAULT : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.VAULT;
        }

        public bool BankNear(Player p)  {
            if (p.Rank == Rank.ADMIN)
                return true;

            foreach (Npc n in p.Viewport.NpcsInView)
                if (Server.Instance.ScriptManager.ACTION_BankNpc.ContainsKey(n.ID) && Formula.InRadius(p.transform.position, n.Transform.position, (n.Definition.InteractionRadius + 0.5f)))
                    return true;

            foreach (Obj o in p.Viewport.objectsInView)
                if (o.Definition.HasAttribute(ObjectAttribute.Bank) && Formula.InRadius(p.transform.position, o.transform.position, 4))
                    return true;

            return false;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {


            int type = packet.ReadByte();
            if (type == 2) // withdraw
            {
                int slot = packet.ReadInt16();
                int amount = (int)packet.ReadUInt32();
                bool token = packet.ReadByte() == 1;
                if (slot < 0 || slot > p.Vault.items.Length)
                    return;
                if (amount <= 0)
                    return;

               if (!BankNear(p))
                    return;

                var ite = p.Vault.items[slot];
                if (ite != null)
                {

                    if (ite.Definition.IsStackable || !ite.Definition.Droppable || !ite.Definition.Tradable)
                        token = false;

                        // Server.Log("Our Slot/Amount " + slot + " / " + amount + " Found Amount " + ite.Amount);
                        if (ite.Amount >= amount)
                    {
                        if (!ite.Definition.IsStackable && !token)
                        {
                            if (amount > p.Inventory.FreeSpace())
                                amount = p.Inventory.FreeSpace();
                        }
                        else
                        {
                            if (token)
                            {
                                if (!p.Inventory.HasItem((ushort)ite.Definition.TokenID) && p.Inventory.FreeSpace() <= 0)
                                    amount = 0;
                            } else
                            {
                                if (!p.Inventory.HasItem((ushort)ite.Definition.TokenID) && p.Inventory.FreeSpace() <= 0)
                                    amount = 0;
                            }
                        }

                        if (amount <= 0)
                        {
                            p.Error("Your Inventory is full!");
                            return;
                        }

                        var id = ite.ID;
                        p.Vault.RemoveItemID(ite.ID, amount);
                        p.Inventory.AddItem((ushort)(token ? DataManager.ItemDefinitions[id].TokenID : id), amount, true, false);
                        p.NetworkActions.SyncVaultItem(false, new Item(id, amount));
                        p.NetworkActions.SendInventory();
                    }
                }
            }
            else if (type == 3) // deposit
            {
                int slot = packet.ReadInt16();
                int amount = (int)packet.ReadUInt32();
                if (slot < 0 || slot > p.Inventory.items.Length)
                    return;
                if (amount <= 0)
                    return;
                if (!BankNear(p))
                    return;


                var ite = p.Inventory.items[slot];
                if (ite == null)
                    return;
                if (!p.Inventory.HasItem(ite.Item.ID, amount))
                    return;

                    DepositItem(p, ite, amount);
                p.NetworkActions.SyncVaultItem(true, new Item((ushort)(ite.Item.Definition.IsToken ? ite.Item.Definition.NonTokenID : ite.Item.ID), amount));
                p.NetworkActions.SendInventory();
            }
            else if (type == 4) // deposit inv
            {
                if (!BankNear(p))
                    return;
                List<InvItem> temp = new List<InvItem>();
                temp.AddRange(p.Inventory.items);
                foreach (InvItem ite in temp)
                {
                    if (ite != null)
                    {
                        int amount = ite.Item.Amount;
                        DepositItem(p, ite, amount);
                    }
                }
                p.NetworkActions.SendInventory();
                p.NetworkActions.SyncVault();

            }
        }

        public void DepositItem(Player p, InvItem ite, int amount)
        {
            if (!BankNear(p))
                return;
            if (ite != null)
            {
                if (p.Inventory.HasItem(ite.Item.ID, amount))
                {
                    var id = ite.Item.ID;
                    p.Inventory.RemoveItem(ite.Item.ID, amount);
                    p.Vault.AddItem((ushort)(ite.Item.Definition.IsToken ? ite.Item.Definition.NonTokenID : id), amount);
                }

            }
        }
    }
}
