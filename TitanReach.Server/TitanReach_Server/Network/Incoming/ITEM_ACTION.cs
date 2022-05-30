using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using TitanReach.Crypto;
using TitanReach.Crypto.Contracts;
using TitanReach.Server;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using TitanReach_Server.Skills;
using TRShared;
using TRShared.Data.Definitions;
using TRShared.Data.Enums;
using static TitanReach_Server.Database;

namespace TitanReach_Server.Network.Incoming
{
    class ITEM_ACTION : IncomingPacketHandler
    {

        public int GetID()
        {
            return Packets.ITEM;
        }

        ushort[] logs = { 249, 250, 251, 252, 253 };
        public async void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {

            int subType = packet.ReadByte();
            switch (subType) {
                case 1:
                    ushort id1 = packet.ReadUInt16();
                    ushort id2 = packet.ReadUInt16();
                    if (p.Inventory.HasItem(id1, 1) && p.Inventory.HasItem(id2, 1)) {
                        EventManager.RaiseUseItemOnItem(p, p.Inventory.GetItem(id1).Item, p.Inventory.GetItem(id2).Item);
                    }
                    break;

                case 2:
                    int slot = packet.ReadByte();
                    int uid = (int)packet.ReadUInt32();
                    ushort id = packet.ReadUInt16();
                    string action = packet.ReadString(packet.ReadByte());
                    if (p.Busy)
                        return;

                    if (action == "Light Fire") {
                        bool hasFlint = p.Inventory.HasItem(79);
                        if (hasFlint) {
                            for (int i = 0; i < logs.Length; i++) {
                                if (p.Inventory.HasItem(logs[i])) {
                                    Survival.LightFire(p, p.Inventory.GetItem(79).Item, p.Inventory.GetItem(logs[i]).Item);
                                    return;
                                }
                            }
                        } else {
                            p.Error("You need a flint to light this");
                            return;
                        }
                    }



                    if (action == "Craft" && id == 377) {
                        p.NetworkActions.OpenCraftingMenu(0);
                    }


                    if (action != "Unequip") {
                        if (action != "Drop") {
                            if (DataManager.ItemDefinitions[id].Options == null || !DataManager.ItemDefinitions[id].Options.Contains(action))
                                return;
                        }
                        if (!p.Inventory.HasItem(id, 1))
                            return;
                    }

                    if (action.Equals("Equip")) {

                        var item = p.Inventory.items[slot];
                        if (item != null && item.Item.Definition.ItemID == id && item.Item.Definition.EquippedItemPosition != ItemDefinition.EquippedItemPositions.NOT_EQUIPPABLE) {

                            if (item.Item.Definition.StatReqs.Count > 0) {
                                foreach (var stat in item.Item.Definition.StatReqs) {
                                    if (p.Skills.GetMaxLevel(stat.Stat) < stat.MinLv) {
                                        p.Error("This item requires level " + stat.MinLv + " " + Enum.GetName(typeof(Stats.SKILLS), stat.Stat));
                                        return;
                                    }
                                }
                            }


                            //  Server.Log("Equipping " + item.Item.Definition.ItemName);

                            if (p.Equipment.LEFT_HAND != null && p.Equipment.LEFT_HAND.ID == item.Item.ID && item.Item.Definition.IsStackable && (p.Equipment.LEFT_HAND.Amount + item.Item.Amount) <= item.Item.Definition.MaxStackSize) // stack items
                            {
                                p.Equipment.LEFT_HAND.Amount += item.Item.Amount;
                                p.Inventory.RemoveItem(item);
                                p.NetworkActions.SendInventory();
                                p.NetworkActions.SendEquipment();
                            } else p.Equipment.Equip(item.Item);
                        }
                    } else if (action.Equals("Unequip")) {

                        for (int i = 0; i < p.Equipment.EquippedItems.Length; i++) {
                            var itm = p.Equipment.EquippedItems[i];
                            if (itm != null) {
                                if (itm.ID == id) {
                                    if (p.Inventory.FreeSpace() < 1 && (!DataManager.ItemDefinitions[id].IsStackable || !p.Inventory.HasItem(id))) {
                                        p.Error("Your Inventory is full - please make room");
                                        return;
                                    }
                                    Server.Log("unequipp slot: " + i);
                                    p.Equipment.Unequip(i);
                                    p.NetworkActions.SendInventory();
                                    p.NetworkActions.SendLocalPlayerEquipmentUpdate();

                                    break;
                                }
                            }
                        }



                    } else if (action.Equals("Drop")) {
                        if (!p.Inventory.GetItem(id).Item.Definition.Tradable)
                        {
                            p.Error("This item cannot be dropped");
                            return;
                        }
                        p.Inventory.DropItem(p.Inventory.GetItem(id).Item);


                    }  else if (action.Equals("Eat")) {

                        if (DataManager.ItemDefinitions[id].EatibleID > -1) {

                            var def = DataManager.EatibleDefinitions[DataManager.ItemDefinitions[id].EatibleID];
                            if (def != null) {
                                if (Environment.TickCount - p.LastFood > def.Delay) {
                                    p.NetworkActions.PlaySound("eat");
                                    p.NetworkActions.SendAnimation(Actions.Eat, 900, -1, def.FoodID);
                                    p.Inventory.RemoveItemID(def.FoodID);
                                    p.LastFood = Environment.TickCount;
                                    p.LastFoodDelayTime = def.Delay;
                                    p.Heal(def.HealAmount);

                                    switch (def.EffectID) {
                                        case 0: // No Effect
                                            break;

                                        case 1: // Health Potion - heal for 1 quater of max hp, rounded up
                                                // p.Heal( (int) MathF.Ceiling (p.Skills.GetMaxLevel((int) Stats.SKILLS.Vitality) / 4.0f));
                                            p.ApplyBuff(Buff.Heal(0, 50, 10000));
                                            break;

                                        case 2: // Minor Healing Potion
                                            p.Heal((int)MathF.Ceiling((p.Skills.GetMaxLevel((int)Stats.SKILLS.Vitality) / 16.0f) * 10.0f));
                                            break;
                                        case 3: // Attack Potion
                                            p.NetworkActions.SendMessage("Buffing Dexterity 7 Levels");
                                            p.BuffStat((int)Stats.SKILLS.Dexterity, 7);
                                            break;
                                        case 4: // Strength Potion
                                            p.NetworkActions.SendMessage("Buffing strength 7 Levels");
                                            p.BuffStat((int)Stats.SKILLS.Strength, 7);
                                            break;
                                        case 5: // Defence Potion
                                            p.NetworkActions.SendMessage("Buffing Defence 7 Levels");
                                            p.BuffStat((int)Stats.SKILLS.Defence, 7);
                                            break;
                                        case 6: // Ranging Potion
                                            p.NetworkActions.SendMessage("Buffing Ranged 7 Levels");
                                            p.BuffStat((int)Stats.SKILLS.Ranged, 7);

                                            break;
                                        case 7: // Super Strength Potion
                                            p.NetworkActions.SendMessage("Buffing strength 12 Levels");
                                            p.BuffStat((int)Stats.SKILLS.Strength, 12);

                                            break;
                                        case 8: // Super Healing Potion
                                            p.Heal((int)MathF.Ceiling((p.Skills.GetMaxLevel((int)Stats.SKILLS.Vitality) / 8.0f) * 10.0f));
                                            break;

                                    }
                                    if (def.LeftOverID > 0)
                                    {
                                        p.Inventory.AddItem(new Item(def.LeftOverID)); //null check stopped working so did id > 0
                                    }

                                    p.NetworkActions.SendInventory();
                                    return;
                                }
                            }

                        }


                    } else {
                        p.NetworkActions.SendMessage("This action is not yet implemented");
                    }
                    break;
            }
        }
        public bool UsedItems(int id1, int id2, int item1, int item2) {
            if ((id1 == item1 && id2 == item2) || (id1 == item2 && id2 == item1))
                return true;
            return false;
        }

    }
}
