using System.Collections.Generic;
using TitanReach_Server.Model;
using TitanReach_Server.Utilities;
using TRShared.Data.Enums;

namespace TitanReach_Server.Skills
{
    class Metalurgy : Skill
    {
        static readonly Recipe[] FurnaceRecepies = {

            // itemID, ingredientID, ingredientCount, levelReq, xpReward
            new Recipe(4,  1, 2, 1, 25), // Copper
            new Recipe(5,  6, 2, 15, 35),  // Iron
            new Recipe(9,  new List<ItemPlusQuantity>{new ItemPlusQuantity(6,1), new ItemPlusQuantity(78,2)}, 30, 45), // Steel
            new Recipe(556,  new List<ItemPlusQuantity>{new ItemPlusQuantity(6,1), new ItemPlusQuantity(78,2), new ItemPlusQuantity(555,1)}, 30, 60), // DarkSteel

            new Recipe(11, new List<ItemPlusQuantity>{new ItemPlusQuantity(74,2), new ItemPlusQuantity(78,3)}, 40, 80), // Cadent
            new Recipe(12, new List<ItemPlusQuantity>{new ItemPlusQuantity(75,2), new ItemPlusQuantity(78,3)}, 50, 100), // Cobalt
         //   new Recipe(13, new List<ItemPlusQuantity>{new ItemPlusQuantity(76,2), new ItemPlusQuantity(78,4)}, 75, 120), // Platinum
     //       new Recipe(14, new List<ItemPlusQuantity>{new ItemPlusQuantity(77,3), new ItemPlusQuantity(78,8)}, 90, 150), // Titanite
            
            
            new Recipe(499, new List<ItemPlusQuantity>{new ItemPlusQuantity(4, 1), new ItemPlusQuantity(241, 1)}, 5, 100), // EmeraldRing
            new Recipe(496, new List<ItemPlusQuantity>{new ItemPlusQuantity(4, 1), new ItemPlusQuantity(241, 1)}, 10, 200), // EmeraldAmulet
            new Recipe(500, new List<ItemPlusQuantity>{new ItemPlusQuantity(5, 1), new ItemPlusQuantity(243, 1)}, 20, 300), // RubyRing
            new Recipe(497, new List<ItemPlusQuantity>{new ItemPlusQuantity(5, 1), new ItemPlusQuantity(243, 1)}, 25, 400), // RubyAmulet
            new Recipe(501, new List<ItemPlusQuantity>{new ItemPlusQuantity(9, 1), new ItemPlusQuantity(242, 1)}, 35, 500), // SapphireRing
            new Recipe(498, new List<ItemPlusQuantity>{new ItemPlusQuantity(9, 1), new ItemPlusQuantity(242, 1)}, 40, 600), // SapphireAmulet
            new Recipe(582, new List<ItemPlusQuantity>{new ItemPlusQuantity(9, 1), new ItemPlusQuantity(244, 1)}, 45, 700), // DiamondRing
            new Recipe(581, new List<ItemPlusQuantity>{new ItemPlusQuantity(9, 1), new ItemPlusQuantity(244, 1)}, 50, 800), // DiamondAmulet

         };

        static readonly CraftingObjectData Funrnace = new CraftingObjectData(ObjectInteractTypes.Smelt,
                                                                            Actions.GatherKneeling,
                                                                            Stats.SKILLS.Metallurgy,
                                                                            FurnaceRecepies,
                                                                            HandSetting.empty,
                                                                            HandSetting.ingredient_1,
                                                                            "Furnace",
                                                                            "Smelting",
                                                                            "furnace",
                                                                            "gainbar",
                                                                            2000);

        static Metalurgy()
        {
            Skill.Objects.Add(ObjectInteractTypes.Smelt, Funrnace);
        }
    }
}
