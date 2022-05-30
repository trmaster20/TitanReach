using System;
using TitanReach_Server.Model;
using TitanReach_Server.Utilities;
using TRShared;
using TRShared.Data.Definitions;
using TRShared.Data.Enums;
using TRShared.Data.Structs;
using static TitanReach_Server.Model.Player;

namespace TitanReach_Server.Skills
{
    class Cooking : Skill
    {
        static Cooking()
        {
            EventManager.ObjectInteractionEvent += EventManager_ObjectInteractionEvent;
        }

        public static bool HasIngredients(Player player, CookingDef def)
        {
            bool hasRaw = true;
            foreach (Ingredient ing in def.Recipe)
            {
                if (!player.Inventory.HasItem((ushort)ing.ItemID, ing.Amount))
                {
                    hasRaw = false;
                    break;
                }
            }
            return hasRaw;

        }

        private static void EventManager_ObjectInteractionEvent(Player p, Obj obj, int type, int opt, bool active)
        {


            if (obj == null || obj.NeedsRemove)
                return;

            if (!obj.Definition.HasAttribute(ObjectAttribute.Cooking))
                return;

            CookingDef def = null;


            foreach (CookingDef de in DataManager.CookingDefinitions)
            {
                if (de.ResultItemID == opt)
                {
                    def = de;
                    break;
                }
            }
            if (def == null)
            {
                p.Error("Error invalid food");
                return;
            }


            if (!HasIngredients(p, def))
                return;
            if (Environment.TickCount - p.LastCraftTime < 2000 - 1) // so client does not try to spam actions and expect to get results
                return;

            int levelDiff = p.Skills.GetCurLevel((int)Stats.SKILLS.Cooking) - def.LevelReq;
            if (levelDiff < 0)
            {
                p.Error("You need a Cooking level of " + def.LevelReq + " to Cook this");
                return;
            }


            //  p.NetworkActions.SendMessage("<color=white>You attempt to cook the " + DataManager.ItemDefinitions[def.Recipe[0].ItemID].ItemName + "</color>");
            p.NetworkActions.PlaySound("cooking wolf meat");
            p.NetworkActions.SendAnimation(Actions.UseItem, 2000, right_id: def.Recipe[0].ItemID);
            p.LastCraftTime = Environment.TickCount;
            p.BusyDelay(BusyType.FULL_LOCK, "Cooking", 2000, (timer, arg) =>
            {
                if (!HasIngredients(p, def))
                {
                    p.Error("You don't have enough raw food");
                    return;
                }

                p.Inventory.RemoveItemID((ushort)def.Recipe[0].ItemID);

                float burnChance = Math.Max(0, 40 - (2 * levelDiff));
                burnChance += 5;

                if (TRShared.Data.Formula.rand.Next(0, 100) < burnChance)
                {
                    if (def.BurntResultID != 0)
                    {
                        p.Inventory.AddItem(def.BurntResultID, 1);
                        p.NetworkActions.ItemGain(def.BurntResultID, 1);
                    }
                    p.Error("You burn the " + DataManager.ItemDefinitions[def.ResultItemID].ItemName);
                }
                else
                {
                    p.Inventory.AddItem(def.ResultItemID, def.ResultAmount);
                    p.NetworkActions.ItemGain(def.ResultItemID, def.ResultAmount);
                        //  p.NetworkActions.SendMessage("<color=white>You cook the " + DataManager.ItemDefinitions[def.ResultItemID].ItemName + " </color>");
                        p.Skills.AddExp((int)Stats.SKILLS.Cooking, def.Exp);
                }
                p.NetworkActions.SendInventory();

            });

        }
    }
}
