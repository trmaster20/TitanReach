using TitanReach_Server.Model;
using TitanReach_Server.Utilities;
using TRShared.Data.Enums;

namespace TitanReach_Server.Skills
{
    class Artisan : Skill
    {


        static readonly Recipe[] LeatherRecipes = {

            // itemID, ingredientID, ingredientCount, levelReq, xpReward
            // itemID, itemCount, ingredientID, ingredientCount, levelReq, xpReward
                            
            // Leather
            new Recipe(458, 366, 1, 1, 20),
            new Recipe(459, 366, 1, 5, 20),
            new Recipe(455, 366, 2, 8, 40),
            new Recipe(457, 366, 3, 12, 60),
            new Recipe(456, 366, 4, 16, 80),

            // Wolf
            new Recipe(463, 367, 1, 15, 50),
            new Recipe(464, 367, 1, 20, 50),
            new Recipe(460, 367, 2, 25, 100),
            new Recipe(462, 367, 3, 30, 150),
            new Recipe(461, 367, 4, 34, 200),

            // Bear
            new Recipe(393, 484, 1, 33, 50),
            new Recipe(394, 484, 1, 36, 50),
            new Recipe(390, 484, 2, 39, 100),
            new Recipe(392, 484, 3, 42, 150),
            new Recipe(391, 484, 4, 45, 200),

         };

        static readonly CraftingObjectData Loom = new CraftingObjectData(ObjectInteractTypes.Leather,
                                                                         Actions.UseItem,
                                                                         Stats.SKILLS.Artisan,
                                                                         LeatherRecipes,
                                                                         HandSetting.ingredient_1,
                                                                         HandSetting.tool,
                                                                         "Artisan",
                                                                         "Sewing",
                                                                         "leather_stitching",
                                                                         "inv_cloth_add",
                                                                         2000,
                                                                         377); // hammer id

        static Artisan()
        {
            EventManager.UseItemOnItemEvent += UseItemOnItem;
            Skill.Objects.Add(ObjectInteractTypes.Leather, Loom);
        }
        public static void Init()
        {
            
        }


        //public static void CraftItem(Player player, int option)
        //{

        //    CraftingObejctData obj = Loom;
        //    // -- Verify if the crafting can occur -- \\
        //    // TODO - needs another check to see if there is space for the new item

        //    int itemId = option;
        //    Recepie recepie = default;

        //    for (int i = 0; i < Recipes.Length; i++)
        //    {
        //        if (Recipes[i].item.itemID == itemId)
        //        {
        //            recepie = Recipes[i];
        //            break;
        //        }
        //    }

        //    if (recepie.Equals(default))
        //    {
        //        player.Error("Not a valid crafting ID");
        //        return;
        //    }

        //    foreach (ItemPlusQuantity ingredient in recepie.ingredients)
        //    {
        //        if (!player.Inventory.HasItem(ingredient.itemID, ingredient.itemQuantity))
        //        {
        //            player.Error("You don't have the required materials to craft this");
        //            return;
        //        }           
        //    } 

        //    if (player.Skills.GetCurLevel((int)obj.skill) < recepie.levelReq)
        //    {
        //        player.Error("You don't have a higher enough " + obj.name + " level for this");
        //        return;
        //    }

        //    // --- crafting action is valid and will be preformed after this point -- \\

        //    int leftObjID = DetermineHeldObejct(obj.leftHand, obj, recepie);
        //    int rightObjID = DetermineHeldObejct(obj.rightHand, obj, recepie);

        //    player.NetworkActions.SendAnimation(obj.actionID, obj.craftingTime, left_id: leftObjID, right_id: rightObjID);
        //    player.NetworkActions.PlaySound(obj.craftSound);

        //    player.BusyDelay(BusyType.FULL_LOCK, obj.verb, obj.craftingTime, (timer) =>
        //    {

        //        foreach (ItemPlusQuantity ingredient in recepie.ingredients)
        //        {
        //            player.Inventory.RemoveItem(ingredient.itemID, ingredient.itemQuantity);
        //        }

        //        player.Skills.AddExp((int) obj.skill, recepie.xpReward);
        //        player.Inventory.AddItem(recepie.item.itemID, recepie.item.itemQuantity);
        //        player.NetworkActions.ItemGain(recepie.item.itemID, recepie.item.itemQuantity);
        //        player.NetworkActions.SendInventory();
        //        player.NetworkActions.PlaySound(obj.gainSound);
        //    });
        //}

        //private static int DetermineHeldObejct(HandSetting hs, CraftingObejctData obj, Recepie recepie)
        //{
        //    int itemId = -1;

        //    switch (hs)
        //    {
        //        case HandSetting.empty:
        //            itemId = -1;
        //            break;
        //        case HandSetting.ingredient_1:
        //            itemId = recepie.ingredients[0].itemID;
        //            break;
        //        case HandSetting.ingredient_2:
        //            itemId = recepie.ingredients[1].itemID;
        //            break;
        //        case HandSetting.item:
        //            itemId = recepie.item.itemID;
        //            break;
        //        case HandSetting.tool:
        //            itemId = obj.toolId;
        //            break;
        //    }
        //    return itemId;
        //}


        private static void UseItemOnItem(Player player, Item item1, Item item2)
        {
            //366 leather roll
            //371 cowhide
            //376 thread
            //377 neeedle


            /* if (UsedItems(item1, item2, 371, 376)) // cowhide - thread
             {
                 //player.Msg("You craft the leather");
                 player.NetworkActions.SendAnimation(Utilities.Actions.UseItem, 1500, 371, 376);

                 player.NetworkActions.PlaySound("lighting");

                 player.BusyDelay(BusyType.FULL_LOCK, "Fletching", 1500, (timer) =>
                 {
                     if (player.Inventory.HasItem(item1) && player.Inventory.HasItem(item2))
                     {
                         if (player.Inventory.HasItem(371) && player.Inventory.HasItem(376))
                         {
                             player.Skills.AddExp(Stats.SKILLS.Artisan, 400);
                             player.Inventory.RemoveItem(371, 1);
                             player.Inventory.RemoveItem(376, 1);
                             player.Inventory.AddItem(366, 1);
                             player.NetworkActions.ItemGain(366, 1);
                             player.NetworkActions.SendInventory();
                         }
                         else
                         {
                             player.Error("You need a needle to do this");
                         }
                     }
                     else player.Error("You dont have the required items to do this");

                 });
             }

             if (UsedItems(item1, item2, 371, 377)) // craft leather?
             {
                 //player.Msg("You craft the leather");
                 player.NetworkActions.SendAnimation(Utilities.Actions.GatherKneeling, 1500, 371, 376);
                 player.NetworkActions.PlaySound("lighting");

                 player.BusyDelay(BusyType.FULL_LOCK, "Fletching", 1500, (timer) =>
                 {
                     if (player.Inventory.HasItem(item1) && player.Inventory.HasItem(item2))
                     {
                         if (player.Inventory.HasItem(372) && player.Inventory.HasItem(376))
                         {
                             player.Skills.AddExp(Stats.SKILLS.Artisan, 400);
                             player.Inventory.RemoveItem(372, 1);
                             player.Inventory.RemoveItem(376, 1);
                             player.Inventory.AddItem(367, 1);
                             player.NetworkActions.ItemGain(367, 1);
                             player.NetworkActions.SendInventory();
                         }
                         else
                         {
                             player.Error("You need a needle to do this");
                         }
                     }
                     else player.Error("You dont have the required items to do this");

                 });
             }*/

        }
    }
}
