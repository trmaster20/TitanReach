using TitanReach_Server.Model;
using TitanReach_Server.Utilities;
using TRShared.Data.Enums;

namespace TitanReach_Server.Skills
{
    internal class Alchemy : Skill
    {
        private static readonly Recipe[] CauldronRecipes = {
            // itemID, ingredientID, ingredientCount, levelReq, xpReward
            // itemID, itemCount, ingredientID, ingredientCount, levelReq, xpReward

            new Recipe(386, 385, 1, 1, 25), // HealthPot
            new Recipe(503, 502, 1, 5, 35), // attackPot
            new Recipe(508, 507, 1, 10, 45), // strength
            new Recipe(381, 380, 1, 20, 50), // regen
            new Recipe(513, 512, 1, 30, 60), // defence
            new Recipe(518, 517, 1, 40, 70), // ranging
            new Recipe(528, 527, 1, 50, 80), // superhealth
            new Recipe(523, 522, 1, 60, 90), // super strength
         };

        private static readonly Recipe[] FillableRecipes = {
            // itemID, ingredientID, ingredientCount, levelReq, xpReward
            // itemID, itemCount, ingredientID, ingredientCount, levelReq, xpReward

            new Recipe(196, 195, 1, 0, 0) // bucket to water
         };

        private static readonly CraftingObjectData Cauldron = new CraftingObjectData(ObjectInteractTypes.Cauldron,
                                                                            Actions.DigShovel,
                                                                            Stats.SKILLS.Alchemy,
                                                                            CauldronRecipes,
                                                                            HandSetting.ingredient_1,
                                                                            HandSetting.empty,
                                                                            "Cauldron",
                                                                            "Brewing",
                                                                            "cauldron",
                                                                            "inv_potion_add",
                                                                            2000);

        static Alchemy()
        {
            Skill.Objects.Add(ObjectInteractTypes.Cauldron, Cauldron);
            EventManager.ObjectInteractionEvent += UseObject;
            EventManager.UseItemOnItemEvent += EventManager_UseItemOnItemEvent;
        }

        private static readonly Recipe[] AlchemyRecipes = {
            // itemID, ingredientID, ingredientCount, levelReq, xpReward
            // itemID, itemCount, ingredientID, ingredientCount, levelReq, xpReward

            //new Recipe(380, 176, 1, 1, 20), // unfinished health potion
            //new Recipe(385, 173, 1, 1, 20) // unfinished mana pot

            new Recipe(385, 176, 1, 1, 10), // minor HealthPot
            new Recipe(502, 174, 1, 5, 20), // attackPot
            new Recipe(507, 175, 1, 10, 25), // strength
            new Recipe(380, 173, 1, 20, 30), // regen
            new Recipe(512, 177, 1, 30, 35), // defence
            new Recipe(517, 465, 1, 40, 40), // ranging
            new Recipe(527, 178, 1, 50, 45), // superhealth
            new Recipe(522, 164, 1, 60, 50), // super strength
         };

        private static void EventManager_UseItemOnItemEvent(Player player, Item item1, Item item2)
        {
            if (player.Busy)
                return;
            if (player.Interacting)
                return;

            foreach (var rec in AlchemyRecipes)
            {
                if (Skill.UsedItems(item1.ID, item2.ID, rec.ingredients[0].itemID, 379))
                {
                    if (rec.levelReq > player.Skills.GetCurLevel((int)Stats.SKILLS.Alchemy))
                    {
                        player.Msg("You need level " + rec.levelReq + " alchemy to do this");
                        return;
                    }
                    player.NetworkActions.PlaySound("inv_potion_drop");
                    player.Skills.AddExp(Stats.SKILLS.Alchemy, rec.xpReward);
                    player.Inventory.RemoveItem(rec.ingredients[0].itemID, rec.ingredients[0].itemQuantity);
                    player.Inventory.RemoveItem(379, rec.item.itemQuantity);
                    player.Inventory.AddItem(rec.item.itemID, rec.item.itemQuantity);
                    player.NetworkActions.ItemGain(rec.item.itemID, rec.item.itemQuantity);
                    player.NetworkActions.SendInventory();
                }
            }
            //if(Skill.UsedItems(item1.ID, item2.ID, 378, ))
        }

        //old ID, new ID
        public static ushort[][] Fillables =
        {
            new ushort[]{191,192 },
            new ushort[]{193,194 },
            new ushort[]{195,196 },
            new ushort[]{197,198 },
            new ushort[]{378,379 }
        };

        public static int GetNextFillable(Player player)
        {
            for (int i = 0; i < Fillables.Length; i++)
            {
                if (player.Inventory.HasItem(Fillables[i][0]))
                    return i;
            }
            return -1;
        }

        private static void UseObject(Player player, Obj obj, int type, int opt, bool active)
        {
            // Server.Log("use object in alchemy");
            if (obj.Definition.HasAttribute(ObjectAttribute.Fillable))
            {
                int idx = GetNextFillable(player);
                if (idx == -1)
                {
                    player.Error("You don't have anything to fill");
                    return;
                }

                int count = 0;
                for (int i = 0; i < Fillables.Length; i++)
                    count += player.Inventory.CountItem(Fillables[i][0]);

                if (count <= 0)
                {
                    player.Error("You don't have anything to fill");
                    return;
                }

                player.Interacting = true;
                player.NetworkActions.SendAnimation(Actions.GatherKneeling, 2000, Fillables[idx][0]);

                player.LastActionTimer = Server.Instance.LoopedDelay(count, 2000, (timer, arg) =>
                {
                   
                    if(player.LastActionTimer != timer)
                    {
                        timer.Stop();
                        return;
                    }
                    int itm = GetNextFillable(player);

                    bool hasItem = player.Inventory.HasItem(Fillables[itm][0]);
                    if (player.Interacting && hasItem && Formula.InRadius(player.transform.position, obj.transform.position, obj.Definition.InteractableRadius + 1))
                    {
                        player.Inventory.RemoveItemID(Fillables[itm][0]);
                        player.Inventory.AddItem(Fillables[itm][1]);
                        player.NetworkActions.ItemGain(Fillables[itm][1], 1);
                        player.NetworkActions.SendInventory();
                        //player.Msg("You fill the " + DataManager.ItemDefinitions[Fillables[itm][0]].ItemName);
                        itm = GetNextFillable(player);
                        if (itm != -1 && player.Inventory.HasItem(Fillables[itm][0]))
                            player.NetworkActions.SendAnimation(Actions.GatherKneeling, 2000, Fillables[itm][0]);
                    }
                    else
                    {
                        timer.Stop();
                        player.Interacting = false;
                        player.StopInteracting();
                    }

                    if (!player.Interacting || !hasItem)
                    {
                        timer.Stop();
                        player.Interacting = false;
                        player.StopInteracting();
                        return;
                    }
                });

                player.BusyDelay(Player.BusyType.FULL_LOCK, "Filling", 2001 * count, (timer, arg) =>
                {
                    if (player.Interacting)
                    {
                        player.Interacting = false;
                        player.StopInteracting();
                    }
                });
            }
        }

        public static void Init()
        {
        }
    }
}