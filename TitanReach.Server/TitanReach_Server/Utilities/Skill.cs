using System;
using System.Collections.Generic;
using TitanReach_Server.Model;
using TRShared.Data.Enums;
using static TitanReach_Server.Model.Player;

namespace TitanReach_Server.Utilities
{
    class Skill
    {

        protected class CraftingObjectData
        {
            public ObjectInteractTypes type;
            public Actions actionID;

            public Stats.SKILLS skill;
            private readonly Recipe[] recipes;
            public HandSetting leftHand;
            public HandSetting rightHand;

            public string name;
            public string verb;
            public string craftSound;
            public string gainSound;
            public int craftingTime;
            public int toolId;


            public CraftingObjectData(ObjectInteractTypes type, Actions actionID, Stats.SKILLS skill, Recipe[] recepies, HandSetting leftHand, HandSetting rightHand, string name, string verb, string craftSound, string gainSound, int craftingTime, int toolId = 0)
            {
                this.type = type;
                this.actionID = actionID;
                this.skill = skill;
                this.recipes = recepies;
                this.name = name;
                this.verb = verb;
                this.craftSound = craftSound;
                this.gainSound = gainSound;
                this.craftingTime = craftingTime;
                this.leftHand = leftHand;
                this.rightHand = rightHand;
                this.toolId = toolId;
            }

            public bool RecepieCheck(Player player, ushort itemId, int idx, out Recipe recepie)
            {
                recepie = default;
                bool levelError = false;
                bool matError = false;
                for (int i = 0; i < recipes.Length; i++)
                {
                    //  Server.Log("recipes[i].item.itemIDitem " + recipes[i].item.itemID + " - " + itemId);
                    if (recipes[i].item.itemID == itemId)
                    {
                        //    Server.Log("got heem");
                        if (idx != -1 && recipes[i].item.itemID != (ushort)idx)
                        {
                            continue;
                        }


                        levelError = false;
                        matError = false;
                        recepie = recipes[i];
                        // Server.Log("Found recipe idx " + i);
                        if (recepie.Equals(default))
                        {
                            player.Error("Not a valid crafting target");
                            return false;
                        }
                        if (recepie.ingredients != null)
                            foreach (ItemPlusQuantity ingredient in recepie.ingredients)
                            {
                                if (!player.Inventory.HasItem(ingredient.itemID, ingredient.itemQuantity))
                                {
                                    matError = true;
                                }
                            }
                        if (player.Skills.GetCurLevel((int)skill) < recepie.levelReq)
                            levelError = true;

                        if (!levelError && !matError)
                        {
                            break;
                        }

                    }
                }

                if (matError)
                {
                    player.Error("You don't have the required materials to craft this");
                    return false;
                }
                if (levelError)
                {
                    player.Error("You don't have a high enough " + skill.ToString().ToLower() + " level for this");
                    return false;

                }
                return true;
            }

            public void CraftItem(Player player, int option, int idx = -1)
            {

                // -- Verify if the crafting can occur -- \\
                // TODO - needs another check to see if there is space for the new item
              //  Server.Log("Player.BUsy: " + player.Busy);
                if (player.Busy)
                    return;

                if (Environment.TickCount - player.LastCraftTime < craftingTime - 1)
                {// so client does not try to spam actions and expect to get results
                    Server.Log("shady stuff here");
                    return;
                }

                ushort itemId = (ushort)option;
                Recipe recepie = default;

                //  Server.Log("recipe length " + recipes.Length);

                if (!RecepieCheck(player, itemId, idx, out Recipe rec))
                    return;
                recepie = rec;
                // --- crafting action is valid and will be preformed after this point -- \\

                int leftObjID = DetermineHeldObject(leftHand, recepie);
                int rightObjID = DetermineHeldObject(rightHand, recepie);

                player.NetworkActions.SendAnimation(actionID, craftingTime, left_id: leftObjID, right_id: rightObjID);
                player.NetworkActions.PlaySound(craftSound);
                player.Interacting = true;
                player.LastCraftTime = Environment.TickCount;
             //   player.NetworkActions.
                player.BusyDelay(BusyType.FULL_LOCK, verb, craftingTime, (timer, arg) =>
                {
                    bool breakk = false;
                    if (!player.Interacting)
                        breakk = true;

                    if (Environment.TickCount - player.LastCraftTime < craftingTime - 1)
                        breakk = true;

                    if(breakk || !RecepieCheck(player, itemId, idx, out Recipe rec))
                    {
                        player.SetBusy(false);
                        timer.Stop();
                        return;
                    }

                    if (recepie.ingredients != null)
                        foreach (ItemPlusQuantity ingredient in recepie.ingredients)
                        {
                            player.Inventory.RemoveItem(ingredient.itemID, ingredient.itemQuantity);
                        }


                 //   Server.Log("Item " + recepie.item.itemID);
                    player.Skills.AddExp((int)skill, recepie.xpReward);
                    player.Inventory.AddItem(recepie.item.itemID, recepie.item.itemQuantity);
                    player.NetworkActions.ItemGain(recepie.item.itemID, recepie.item.itemQuantity);
                    player.NetworkActions.SendInventory();
                    player.NetworkActions.PlaySound(gainSound);
                    player.Interacting = false;
                });
            }

            private int DetermineHeldObject(HandSetting hs, Recipe recepie)
            {
                int itemId = -1;

                switch (hs)
                {
                    case HandSetting.empty:
                        itemId = -1;
                        break;
                    case HandSetting.ingredient_1:
                        if (recepie.ingredients != null && recepie.ingredients.Count > 0)
                            itemId = recepie.ingredients[0].itemID;
                        break;
                    case HandSetting.ingredient_2:
                        if (recepie.ingredients != null && recepie.ingredients.Count > 1)
                            if (recepie.ingredients.Count > 0)
                                itemId = recepie.ingredients[1].itemID;
                        break;
                    case HandSetting.item:
                        itemId = recepie.item.itemID;
                        break;
                    case HandSetting.tool:
                        itemId = toolId;
                        break;
                }
                return itemId;
            }
        }

        protected enum HandSetting
        {
            empty = 0,
            ingredient_1 = 1,
            ingredient_2 = 2,
            item = 3,
            tool = 4
        }

        protected struct ItemPlusQuantity
        {
            public ushort itemID;
            public int itemQuantity;

            public ItemPlusQuantity(ushort itemID, int itemQuantity)
            {
                this.itemID = itemID;
                this.itemQuantity = itemQuantity;
            }
        }

        protected struct Recipe
        {
            public ItemPlusQuantity item;
            public List<ItemPlusQuantity> ingredients;
            public int levelReq;
            public int xpReward;

            public Recipe(ushort itemID, int itemCount, ushort ingredientID, int ingredientCount, int levelReq, int xpReward)
            {
                this.item = new ItemPlusQuantity(itemID, itemCount);
                this.ingredients = new List<ItemPlusQuantity> { new ItemPlusQuantity(ingredientID, ingredientCount) };
                this.levelReq = levelReq;
                this.xpReward = xpReward;
            }

            public Recipe(ushort itemID, ushort ingredientID, int ingredientCount, int levelReq, int xpReward)
            {
                this.item = new ItemPlusQuantity(itemID, 1);
                this.ingredients = new List<ItemPlusQuantity> { new ItemPlusQuantity(ingredientID, ingredientCount) };
                this.levelReq = levelReq;
                this.xpReward = xpReward;
            }

            public Recipe(ushort itemID, int itemCount, List<ItemPlusQuantity> ingredients, int levelReq, int xpReward)
            {
                this.item = new ItemPlusQuantity(itemID, itemCount);
                this.ingredients = ingredients;
                this.levelReq = levelReq;
                this.xpReward = xpReward;
            }

            public Recipe(ushort itemID, List<ItemPlusQuantity> ingredients, int levelReq, int xpReward)
            {
                this.item = new ItemPlusQuantity(itemID, 1);
                this.ingredients = ingredients;
                this.levelReq = levelReq;
                this.xpReward = xpReward;
            }

        }

        protected static Dictionary<ObjectInteractTypes, CraftingObjectData> Objects;//contains all the object types (Loom, anivil etc)

        static Skill()
        {
            Objects = new Dictionary<ObjectInteractTypes, CraftingObjectData>();

        }

        public static bool CraftItem(ObjectInteractTypes type, Player player, int ItemId, int idx = -1)
        {
            if (Objects.ContainsKey(type))
            {
                Objects[type].CraftItem(player, ItemId, idx);
                return true;
            }

            return false;
        }

        public static bool UsedItems(int id1, int id2, int item1, int item2)
        {
            if ((id1 == item1 && id2 == item2) || (id1 == item2 && id2 == item1))
                return true;
            return false;
        }

        public static bool UsedItems(Item id1, Item id2, int item1, int item2)
        {
            if ((id1.ID == item1 && id2.ID == item2) || (id1.ID == item2 && id2.ID == item1))
                return true;
            return false;
        }
    }
}
