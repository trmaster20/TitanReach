using System.Collections.Generic;
using System.Linq;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using TRShared;
using TRShared.Data.Definitions;

namespace TitanReach_Server.Network.Incoming {
    class SHOP_ACTION : IncomingPacketHandler {

        public int GetID() {
            return Packets.SHOP;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet) {

            int subType = packet.ReadByte();
            if (p.CurrentInteractedShop == -1)
                return;

            ShopDef def = DataManager.ShopDefinitions[p.CurrentInteractedShop];


            switch (subType) {
                case 0:

                    int slot = packet.ReadInt16();
                    int amount = (int)packet.ReadUInt32();

                    if (!CheckShopRange(p)) {
                        p.Msg("you are not in range of this shop");
                        return;
                    }

                    if (slot < 0 || slot > def.Items.Count)
                        return;

                    if (amount <= 0)
                        return;

                    ShopDef.ShopItem ite = def.Items[slot];
                    if (ite != null) {
                       //  Server.Log("Our Slot/Amount " + slot + " / " + amount + " Found Amount " + ite.Amount);


                        if (ite.Amount >= amount) {

             
                            var id = ite.ItemID;

                            if (!DataManager.ItemDefinitions[id].IsStackable && p.Inventory.FreeSpace() < amount)
                                amount = p.Inventory.FreeSpace();
                     

                            int totalPrice = amount * (int)(DataManager.ItemDefinitions[id].Value * def.BuyMultiplier);


                            if (p.Inventory.CountItem(16) < totalPrice) {
                                p.Error("You cannot afford this");
                                return;
                            }
                         

                            bool canFit = true;
                            if (p.Inventory.FreeSpace() == 0) {
                                canFit = false;
                                if (DataManager.ItemDefinitions[id].IsStackable && p.Inventory.HasItem(id)) {
                                    canFit = true;
                                }
                            }
                      
                            if (!canFit) {
                                p.Error("You don't have enough inventory space");
                                return;
                            }
                    
                            //p.Vault.RemoveItemID(ite.ItemID, amount);

                            ite.Amount -= amount;
                            if (ite.Amount <= 0 && def.General && !ite.OriginalItem) {
                                def.Items.Remove(ite);
                            }
                     
                            p.Inventory.AddItem(id, amount);
                            p.NetworkActions.ItemGain(id, amount);
                            p.Inventory.RemoveItem(16, totalPrice);
                            p.NetworkActions.PlayPickupSound(16, false);
                            // p.NetworkActions.SendShop(def);
                            UpdateShopToPlayers(p.Map, def);
                            p.NetworkActions.SendInventory();
                        
                        }
                    }
                    break;

                case 1:


                    slot = packet.ReadInt16();
                    amount = (int)packet.ReadUInt32();

                    if (!CheckShopRange(p)) {
                        p.Msg("you are not in range of this shop");
                        return;
                    }

                    if (slot < 0 || slot > p.Inventory.items.Length)
                        return;
                    if (amount <= 0)
                        return;
                    InvItem invItem = p.Inventory.items[slot];
                    if (invItem != null) {
                        bool token = invItem.Item.Definition.IsToken;
                        int nonTokenID = invItem.Item.Definition.NonTokenID;
                     //   Server.Log("Our Slot/Amount " + slot + " / " + amount + " Found Amount " + ite.Item.Amount);
                        if (p.Inventory.HasItem(invItem.Item.ID, 1)) {
                            if (invItem.Item.ID == 16 || !invItem.Item.Definition.Tradable) {
                                p.Error("You cannot sell this item");
                                return;
                            }
                            bool found = false;

                            foreach (var it in def.Items) {
                                if (it.ItemID == invItem.Item.Definition.NonTokenID) {
                                    found = true;
                                    break;
                                }
                            }
                            if (def.General)
                                found = true;

                            if (!found)
                                return;
                  
                            int cnt = p.Inventory.CountItem(invItem.Item.ID);
                            if (amount > cnt)
                                amount = cnt;
                        
                            var id = invItem.Item.Definition.NonTokenID;
                            var val = (int)(invItem.Item.Definition.Value * def.SellMultiplier) * amount;
                            p.Inventory.RemoveItem(invItem.Item.ID, amount);
                            found = false;
                            foreach (var it in def.Items) {
                                if (it.ItemID == invItem.Item.Definition.NonTokenID) {
                                    it.Amount += amount;
                                    found = true;
                                    break;
                                }
                            }
                       
                            if (!found) {
                                var ingred = new ShopDef.ShopItem();
                                ingred.ItemID = (ushort)id;
                                ingred.Amount = amount;
                                ingred.OriginalItem = false;
                                def.Items.Add(ingred);
                            }

                            if (val != 0) {
                                p.Inventory.AddItem(16, val);
                                p.NetworkActions.ItemGain(16, val);
                                p.NetworkActions.PlayPickupSound(16, true);
                            }
                            UpdateShopToPlayers(p.Map, def);
                            p.NetworkActions.SendInventory();
                        }
                    }
                    break;

                case 2:
                    p.CurrentInteractedShop = -1;
                    break;
            }
        }


        private bool CheckShopRange(Player p) {
            bool inRange = false;
            foreach (Npc npc in p.Map.Npcs) {
                foreach (KeyValuePair<int, List<NPCShopRegister>> keyValuePair in Server.Instance.ScriptManager.ACTION_ShopNpc) {
                    foreach (NPCShopRegister nPCShopRegister in keyValuePair.Value) {
                        if (nPCShopRegister.ID == p.CurrentInteractedShop) {
                            if (npc.ID == keyValuePair.Key && Formula.InRadius(p.transform.position, npc.Transform.position, 3)) {
                                inRange = true;
                                break;
                            }
                        }
                    }
                }
            }
            return inRange;
        }

        public static void UpdateShopToPlayers(Map map, ShopDef sd)
        {
            lock (map.Players)
            {
                foreach (Player p in map.Players)
                {
                    if (p.CurrentInteractedShop == sd.ID)
                    {
                        p.NetworkActions.SendShop(sd);
                    }
                }
            }
        }
    }
}
