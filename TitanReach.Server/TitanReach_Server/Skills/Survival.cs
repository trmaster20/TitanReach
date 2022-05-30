using System;
using TitanReach_Server.Model;
using TitanReach_Server.Utilities;
using TRShared;
using TRShared.Data.Definitions;
using TRShared.Data.Enums;
using static TitanReach_Server.Model.Player;
using Vector3 = TitanReach_Server.Model.Vector3;
using static TRShared.DataManager.ItemID;

namespace TitanReach_Server.Skills
{
    class Survival : Skill
    {
        static readonly Recipe[] WorkbenchRecepies = {
            // itemID, ingredientID, ingredientCount, levelReq, xpReward
            // itemID, itemCount, ingredientID, ingredientCount, levelReq, xpReward    
            // itemID, itemCount, ingredientList, levelReq, xpReward    
       
            new Recipe(ARROW_SHAFT, 10, LOGS, 1, 1, 23),
            // needs eucaliptis
            new Recipe(ARROW_SHAFT, 15, WILLOW_LOGS, 1, 1, 30),
            new Recipe(ARROW_SHAFT, 20, ASPEN_LOGS, 1, 1, 35),
            new Recipe(ARROW_SHAFT, 25, OAK_LOGS, 1, 1, 40),



            new Recipe(SHORTBOW_UNSTRUNG, LOGS, 1, 1, 23), // normal
            new Recipe(LONGBOW_UNSTRUNG, LOGS, 1, 5, 30),

            new Recipe(WILLOW_SHORTBOW_UNSTRUNG, WILLOW_LOGS, 1, 10, 35), // willow
            new Recipe(WILLOW_LONGBOW_UNSTRUNG, WILLOW_LOGS, 1, 15, 40),

            new Recipe(ASPEN_SHORTBOW_UNSTRUNG, ASPEN_LOGS, 1, 30, 50), // aspen
            new Recipe(ASPEN_LONGBOW_UNSTRUNG, ASPEN_LOGS, 1, 35, 60),

            new Recipe(OAK_SHORTBOW_UNSTRUNG, OAK_LOGS, 1, 40, 50), // oak
            new Recipe(OAK_LONGBOW_UNSTRUNG, OAK_LOGS, 1, 55, 60),
         };

        static readonly CraftingObjectData Workbench = new CraftingObjectData(ObjectInteractTypes.Workbench,
                                                                            Actions.Saw,
                                                                            Stats.SKILLS.Survival,
                                                                            WorkbenchRecepies,
                                                                            HandSetting.ingredient_1,
                                                                            HandSetting.tool,
                                                                            "Workbench",
                                                                            "Fletching",
                                                                            "woodsaw",
                                                                            "inv_arrows_add",
                                                                            2000,
                                                                            219);

        static readonly Recipe[] TanningRecepies = {

            // itemID, ingredientID, ingredientCount, levelReq, xpReward
            // itemID, itemCount, ingredientID, ingredientCount, levelReq, xpReward    
            // itemID, itemCount, ingredientList, levelReq, xpReward    
       
            new Recipe(366, 371, 1, 1, 30),  // leather Roll
            new Recipe(366, 485, 1, 1, 40),  // leather Roll from deer hide
            new Recipe(367, 372, 1, 10, 50), // wolf roll
            new Recipe(484, 483, 1, 10, 60), // bear roll


         };

        static readonly CraftingObjectData TanningRack = new CraftingObjectData(ObjectInteractTypes.TanningRack,
                                                                                Actions.Saw,
                                                                                Stats.SKILLS.Artisan,
                                                                                TanningRecepies,
                                                                                HandSetting.ingredient_1,
                                                                                HandSetting.empty,
                                                                                "Tanning Rack",
                                                                                "Tanning",
                                                                                "leather_stitching",
                                                                                "inv_leather_add",
                                                                                2000);

        static Survival()
        {
            Skill.Objects.Add(ObjectInteractTypes.Workbench, Workbench);
            Skill.Objects.Add(ObjectInteractTypes.TanningRack, TanningRack);

            EventManager.UseItemOnItemEvent += UseItemOnItem;
            EventManager.ObjectInteractionEvent += PickingInteract;
        }

        public static void Init()
        {

        }

        private static void PickingInteract(Player player, Obj obj, int type, int opt, bool active)
        {

            if (obj.Definition.HasAttribute(ObjectAttribute.Spinning))
            {
                var count = player.Inventory.CountItem(184);

                if (count <= 0)
                {
                    player.Error("You don't have any Flax to spin");
                    return;
                }

                player.Interacting = true;
                player.NetworkActions.SendAnimation(Actions.Chisel, 1500);

                Server.Instance.LoopedDelay(count, 1500, (timer, arg) =>
                {
                    player.LastActionTimer = timer;
                    bool hasItem = player.Inventory.HasItem(184);
                    if (player.Interacting && hasItem)
                    {

                        player.Inventory.RemoveItemID(184);
                        player.Inventory.AddItem(215);
                        player.NetworkActions.ItemGain(215, 1);
                        player.Skills.AddExp(Stats.SKILLS.Artisan, 15);
                        player.NetworkActions.SendInventory();

                        if (player.Inventory.HasItem(184))
                            player.NetworkActions.SendAnimation(Actions.Chisel, 1500);
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

                player.BusyDelay(Player.BusyType.FULL_LOCK, "Spinning", 1501 * count, (timer, arg) =>
                {
                    if (player.Interacting)
                    {
                        player.Interacting = false;
                        player.StopInteracting();
                    }
                });

            }

            if (!obj.Definition.HasAttribute(ObjectAttribute.Pickable))
                return;
            if (!obj.Depleted)
            {


                if (obj.PickableRemaining == -1)
                    obj.PickableRemaining = obj.Definition.PickableDef.PicksBeforeDespawn;

                int animID = 8;
                if (obj.Definition.Name.Contains("Tree"))
                    animID = 22;

                player.NetworkActions.SendAnimation((Actions)animID, 1400);

                ushort itm = (ushort)obj.Definition.PickableDef.ItemID;
                player.NetworkActions.PlaySound("flax");
                player.BusyDelay(Player.BusyType.FULL_LOCK, "Foraging", 1400, (timer, arg) =>
                {
                    if (!player.Busy)
                        return;
                    timer.Stop();
                    player.StopInteracting();

                    //player.Msg("You pick a " + DataManager.ItemDefinitions[itm].ItemName);
                    player.Inventory.AddItem(itm, true);
                    player.Skills.AddExp(Stats.SKILLS.Survival, 15);

                    if (obj != null && !obj.NeedsRemove)
                    {
                        obj.PickableRemaining--;
                        if (obj.Definition.HasAttribute(ObjectAttribute.Depletable))
                        {
                            if (obj.PickableRemaining <= 0)
                            {
                                int objID = obj.ID;
                                var pos = new Vector3(obj.transform.position.X, obj.transform.position.Y, obj.transform.position.Z);
                                Server.Instance.Delay(obj.Definition.PickableDef.RespawnTime, (timer, arg) => obj.Map.SpawnGroundObject(objID, pos, obj.LocationDef));
                                obj.Remove();
                            }
                        }
                    }

                });

            }
        }

        public static void LightFire(Player player, Item item1, Item item2)
        {
            float placementOffset = 1.0f;
            float xoffset = placementOffset * (float)Math.Sin((player.rotation / 360.0f) * 2 * 3.1415f);
            float yoffset = placementOffset * (float)Math.Cos((player.rotation / 360.0f) * 2 * 3.1415f);
            float xpos = player.transform.position.X + xoffset;
            float ypos = player.transform.position.Z + yoffset;   
           // var pos = new Vector3(xpos, player.transform.position.Y, ypos);
            var pos = new Vector3(xpos, player.ServerHeight() + 0.1f, ypos); //old campfire pos check like 20 lines down
            foreach (Vector3 vec in Server.Instance.ActiveFires)
            {
                if (Formula.InRadius(pos, vec, 1))
                {
                    player.Error("Cannot light a fire next to an existing fire");
                    return;
                }
            }

            player.Msg("You attempt to light the logs");
            player.NetworkActions.SendAnimation(Actions.GatherKneeling, 2000, 79, item2.ID);
            player.NetworkActions.PlaySound("flintfire");

            player.BusyDelay(BusyType.FULL_LOCK, "Lighting", 2000, (timer, arg) =>
            {
                if (!player.Busy)
                    return;
                if (player.Inventory.HasItem(item1) && player.Inventory.HasItem(item2))
                {
                    EventManager.RaiseLightFire(player, item2.ID);
                    player.Msg("You successfully light the logs");
                    player.Skills.AddExp(Stats.SKILLS.Survival, LightLogExp[Formula.GetIndex(item2.ID, LightableLogs)]);
                    player.Inventory.RemoveItemID(item2.ID);
                    player.NetworkActions.SendInventory();

                    //var pos = new Vector3(xpos, player.transform.position.Y, ypos);
                    var pos = new Vector3(xpos, player.ServerHeight() + 0.1f, ypos); //old campfire pos check like 20 lines up
                    Server.Instance.ActiveFires.Add(pos);
                    var bonfire = player.Map.SpawnGroundObject(DataManager.ObjectID.CAMPFIRE, pos);

                    if (player.ACTION_OnSpawnObject.ContainsKey(40)) {
                        player.ACTION_OnSpawnObject[40](player);
                    }

                    Server.Instance.Delay(120000, (timer, arg) =>
                    {
                        Server.Instance.ActiveFires.Remove(pos);
                        bonfire?.Remove();
                    });
                }
                else player.Error("You don't have logs to do this.");

            });
        }

        public static ushort[] LightableLogs = { LOGS, WILLOW_LOGS, ASPEN_LOGS, OAK_LOGS};
        public static int[] LightLogExp = { 30, 50, 80, 100};
        public static ushort[] StrungBows = { SHORTBOW, LONGBOW, WILLOW_SHORTBOW, WILLOW_LONGBOW, ASPEN_SHORTBOW, ASPEN_LONGBOW, OAK_SHORTBOW, OAK_LONGBOW};
        public static ushort[] UnstrungBows = { SHORTBOW_UNSTRUNG, LONGBOW_UNSTRUNG, WILLOW_SHORTBOW_UNSTRUNG, WILLOW_LONGBOW_UNSTRUNG, ASPEN_SHORTBOW_UNSTRUNG, ASPEN_LONGBOW_UNSTRUNG, OAK_SHORTBOW_UNSTRUNG, OAK_LONGBOW_UNSTRUNG };
        public static int[] StringingBowXP = { 20, 25, 30, 35, 40, 45, 50, 55};
        public static int[] StringingBowLevelReq = { 1, 5, 10, 15, 30, 35, 40, 45 };
        public static ushort[] ArrowHeads = { COPPER_ARROWHEAD, IRON_ARROWHEAD, STEEL_ARROWHEAD, VERTIUM_ARROWHEAD, COBALT_ARROWHEAD, PLATINUM_ARROWHEAD, TITANITE_ARROWHEAD };
        public static ushort[] Arrows = { COPPER_ARROW, IRON_ARROW, STEEL_ARROW, VERTIUM_ARROW, COBALT_ARROW, PLATINUM_ARROW, TITANITE_ARROW };

        public static int[] ArrowHeadsXP = { 25, 30, 50, 60, 80, 100, 125 };

        private static void UseItemOnItem(Player player, Item item1, Item item2)
        {
            if (player.Busy)
                return;
            foreach (ushort id in LightableLogs)
                if (UsedItems(item1, item2, id, FLINT))
                { // flint on logs
                    if (item1.ID == FLINT)
                        LightFire(player, item1, item2);
                    else
                        LightFire(player, item2, item1);
                    return;
                }

            if (UsedItems(item1, item2, DataManager.ItemID.FEATHER, DataManager.ItemID.ARROW_SHAFT))
            {

                //   player.NetworkActions.SendAnimation(8, -1, 216);
                int amt = Math.Min(player.Inventory.CountItem(item1.ID), player.Inventory.CountItem(item2.ID));
                if (amt > 10)
                    amt = 10;


                if (player.Inventory.HasItem(item1) && player.Inventory.HasItem(item2))
                {
                    player.Skills.AddExp(Stats.SKILLS.Survival, 40);
                    player.Inventory.RemoveItem(DataManager.ItemID.FEATHER, amt);
                    player.Inventory.RemoveItem(DataManager.ItemID.ARROW_SHAFT, amt);
                    player.Inventory.AddItem(DataManager.ItemID.HEADLESS_ARROW, amt);
                    player.NetworkActions.ItemGain(DataManager.ItemID.HEADLESS_ARROW, amt);
                    player.NetworkActions.SendInventory();
                }
                else player.Error("You dont have the required items to do this");


            }

            //if (UsedItems(item1, item2, 208, 217)) // arrow shaft and copper arrowtips
            //{


            //    int amt = Math.Min(player.Inventory.CountItem(item1.ID), player.Inventory.CountItem(item2.ID));
            //    if (amt > 10)
            //        amt = 10;


            //    if (player.Inventory.HasItem(item1) && player.Inventory.HasItem(item2))
            //    {
            //        player.Skills.AddExp(Stats.SKILLS.Survival, 40);
            //        player.Inventory.RemoveItem(217, amt);
            //        player.Inventory.RemoveItem(208, amt);
            //        player.Inventory.AddItem(201, amt);
            //        player.NetworkActions.ItemGain(201, amt);
            //        player.NetworkActions.SendInventory();
            //    }
            //    else player.Error("You dont have the required items to do this");


            //}

            for (int i = 0; i < ArrowHeads.Length; i++)
            {
                if (UsedItems(item1, item2, ArrowHeads[i], HEADLESS_ARROW)) // stringing bows
                {
                    int amt = Math.Min(player.Inventory.CountItem(item1.ID), player.Inventory.CountItem(item2.ID));
                    if (amt > 10)
                        amt = 10;


                    if (player.Inventory.HasItem(item1) && player.Inventory.HasItem(item2) && amt == 10)
                    {
                        player.Skills.AddExp(Stats.SKILLS.Survival, ArrowHeadsXP[i]);
                        player.Inventory.RemoveItem(HEADLESS_ARROW, amt);
                        player.Inventory.RemoveItem(ArrowHeads[i], amt);
                        player.Inventory.AddItem(Arrows[i], amt);
                        player.NetworkActions.ItemGain(Arrows[i], amt);
                        player.NetworkActions.SendInventory();
                    }
                    else player.Error("You dont have the required items to do this");
                }
            }


            for (int i = 0; i < UnstrungBows.Length; i++)
            {


                if (UsedItems(item1, item2, UnstrungBows[i], BOW_STRING)) // stringing bows
                {
                    if (StringingBowLevelReq[i] > player.Skills.GetCurLevel((int)Stats.SKILLS.Survival))
                    {
                        player.Msg("you need level " + StringingBowLevelReq[i] + "  surival to string this");
                        return;
                    }

                    player.NetworkActions.SendAnimation(Actions.UseItem, 1500, UnstrungBows[i], BOW_STRING);
                    player.NetworkActions.PlaySound("lighting");


                    player.BusyDelay(BusyType.FULL_LOCK, "Fletching", 1500, (timer, arg) =>
                    {
                        if (player.Inventory.HasItem(item1) && player.Inventory.HasItem(item2))
                        {
                            player.Skills.AddExp(Stats.SKILLS.Survival, StringingBowXP[i]);
                            player.Inventory.RemoveItemID(UnstrungBows[i]);
                            player.Inventory.RemoveItemID(BOW_STRING);
                            player.Inventory.AddItem(StrungBows[i]);
                            player.NetworkActions.ItemGain(StrungBows[i], 1);
                            player.NetworkActions.SendInventory();
                        }
                        else player.Error("You dont have the required items to do this");
                    });
                    return;
                }
            }
        }
    }
}
