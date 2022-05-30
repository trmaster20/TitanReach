using TitanReach_Server.Model;
using TitanReach_Server.Utilities;
using TRShared.Data.Enums;

namespace TitanReach_Server.Skills
{
    class Forging : Skill
    {
        static readonly Recipe[] AnvilRecipes = {

            // itemID, ingredientID, ingredientCount, levelReq, xpReward
            // itemID, itemCount, ingredientID, ingredientCount, levelReq, xpReward
                            
            // Copper
            new Recipe(409, 4, 1, 1, 7 * 3), //pickaxe
            new Recipe(407, 4, 1, 1, 7 * 3), // dagger
            new Recipe(405, 4, 1, 2, 7 * 3), // axe
            new Recipe(208, 10, 4, 1, 2, 7 * 3),  //arrow heads x 10
            new Recipe(425, 4, 2, 3, 14 * 3), // gloves
            new Recipe(410, 4, 2, 4, 14 * 3), // longsword
            new Recipe(426 , 4, 2, 5, 14 * 3), // boots
          //  new Recipe(406, 4, 2, 6, 14 * 3), //battle axe
            new Recipe(423, 4, 2, 7, 14 * 3), // helmet
            new Recipe(424 , 4, 3, 8, 21 * 3), // legs
            new Recipe(408, 4, 3, 9, 21 * 3), // greatsword
            new Recipe(442 , 4, 4, 10, 28 * 3), // chest

            // Iron
            new Recipe(415, 5, 1, 16, 7 * 6), //pickaxe
            new Recipe(411, 5, 1, 16, 7 * 6), // axe
            new Recipe(413, 5, 1, 17, 7 * 6), // dagger
            new Recipe(209, 10, 5, 1, 17, 7 * 6),  //arrow heads x 10
            new Recipe(430, 5, 2, 18, 14 * 6), // gloves
            new Recipe(416, 5, 2, 19, 14 * 6), // longsword
            new Recipe(431 , 5, 2, 20, 14 * 6), // boots
           // new Recipe(412, 5, 2, 21, 14 * 6), //battle axe
            new Recipe(427, 5, 2, 22, 14 * 6), // helmet
            new Recipe(429 , 5, 3, 23, 21 * 6), // legs
            new Recipe(414, 5, 3, 24, 21 * 6), // greatsword
            new Recipe(428 , 5, 4, 25, 28 * 6), // chest

            // Steel
            new Recipe(421, 9, 1, 31, 7 * 9), //pickaxe
            new Recipe(417, 9, 1, 31, 7 * 9), // axe
            new Recipe(419, 9, 1, 32, 7 * 9), // dagger
            new Recipe(210, 10, 9, 1, 32, 7 * 9),  //arrow heads x 10
            new Recipe(435, 9, 2, 33, 14 * 9), // gloves
            new Recipe(422, 9, 2, 34, 14 * 9), // longsword
            new Recipe(436 , 9, 2, 35, 14 * 9), // boots
           // new Recipe(418, 9, 2, 36, 14 * 9), //battle axe
            new Recipe(432, 9, 2, 37, 14 * 9), // helmet
            new Recipe(434 , 9, 3, 38, 14 * 9), // legs
            new Recipe(420, 9, 3, 39, 21 * 9), // greatsword
            new Recipe(433 , 9, 4, 40, 28 * 9), // chest

             // DarkSteel
            new Recipe(573, 556, 1, 31, 10 * 3), //pickaxe
            new Recipe(569, 556, 1, 31, 10 * 3), // axe
            new Recipe(571, 556, 1, 32, 10 * 3), // dagger
            //new Recipe(210, 10, 556, 1, 32, 10 * 3),  //arrow heads x 10
            new Recipe(543, 556, 2, 33, 20 * 3), // gloves
            new Recipe(574, 556, 2, 34, 20 * 3), // longsword
            new Recipe(544 , 556, 2, 35, 20 * 3), // boots
           // new Recipe(570, 9, 2, 36, 20 * 3), //battle axe
            new Recipe(540, 556, 2, 37, 20 * 3), // helmet
            new Recipe(542 , 556, 3, 38, 30 * 3), // legs
            new Recipe(572, 556, 3, 39, 30 * 3), // greatsword
            new Recipe(541 , 556, 4, 40, 40 * 3), // chest


             // verbatus
            new Recipe(566, 11, 1, 40, 15 * 3), //pickaxe
            new Recipe(562, 11, 1, 41, 15 * 3), // axe
            new Recipe(564, 11, 1, 42, 15 * 3), // dagger
            new Recipe(211, 10, 11, 1, 42, 15 * 3),  //arrow heads x 10
            new Recipe(548, 11, 2, 43, 30 * 3), // gloves
            new Recipe(568, 11, 2, 44, 30 * 3), // longsword
            new Recipe(549 , 11, 2, 45, 30 * 3), // boots
           // new Recipe(418, 9, 2, 36, 20 * 3), //battle axe
            new Recipe(545, 11, 2, 47, 30 * 3), // helmet
            new Recipe(547 , 11, 3, 48, 45 * 3), // legs
            new Recipe(565, 11, 3, 49, 45 * 3), // greatsword
            new Recipe(546 , 11, 4, 50, 60 * 3), // chest


           //// Coballt

            new Recipe(561, 12, 1, 50, 20 * 3), //pickaxe
            new Recipe(557, 12, 1, 50, 20 * 3), // axe
            new Recipe(559, 12, 1, 50, 20 * 3), // dagger
            new Recipe(212, 10, 12, 1, 50, 100 * 9), //arrow
            new Recipe(536, 12, 5, 50, 500 * 9), //sword

        };

        static readonly CraftingObjectData Anvil = new CraftingObjectData(ObjectInteractTypes.Anvil,
                                                                          Actions.HammerTable,
                                                                          Stats.SKILLS.Forging,
                                                                          AnvilRecipes,
                                                                          HandSetting.ingredient_1,
                                                                          HandSetting.tool,
                                                                          "Anvil",
                                                                          "Forging",
                                                                          "Anvil",
                                                                          "gainbar",
                                                                          2000,
                                                                          130); // hammer id

        static Forging()
        {
            Skill.Objects.Add(ObjectInteractTypes.Anvil, Anvil);
        }

        public static void Init()
        {

        }
    }
}
