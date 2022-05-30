using TitanReach_Server.Model;
using TitanReach_Server.Utilities;
using TRShared;
using TRShared.Data.Enums;

namespace TitanReach_Server.Skills
{
    class Fishing : Skill
    {

        static Fishing()
        {
            EventManager.ObjectInteractionEvent += FishInteract;
        }

        public static bool BaitToolCheck(Player player, Obj obj)
        {
            bool baitFound = false;
            bool toolFound = false;
            if (player.Equipment.EquippedItems[10] != null && player.Equipment.EquippedItems[10].ID == obj.Definition.FishDef.ToolID)
            {
                toolFound = true;
            }
            else
            {
                player.Error("You need an equipped " + DataManager.ItemDefinitions[obj.Definition.FishDef.ToolID].ItemName + " to fish here");
            }

            if (obj.Definition.FishDef.BaitID == 0 || player.Inventory.HasItem(obj.Definition.FishDef.BaitID))
            {
                baitFound = true;
            }
            else
            {
                if (toolFound && obj.Definition.FishDef.BaitID != 0)
                    player.Error("You don't have any " + DataManager.ItemDefinitions[obj.Definition.FishDef.BaitID].ItemName + " to fish here");


            }



            if (!baitFound || !toolFound)
            {
                player.StopInteracting();

                return false;
            }
            else
            {
                return true;
            }
        }


        private static void FishInteract(Player player, Obj obj, int type, int opt, bool active)
        {
            if (!obj.Definition.HasAttribute(ObjectAttribute.Fishable))
                return;

            if (player.Busy)
                return;

            if (player.Skills.GetCurLevel((int)Stats.SKILLS.Fishing) < obj.Definition.FishDef.LevelReq)
            {

                player.Error("This requires level " + obj.Definition.FishDef.LevelReq + " fishing to catch");
                player.StopInteracting();
                return;
            }

            if (!BaitToolCheck(player, obj))
                return;

            if (!player.Interacting)
            {
                player.Interacting = true;
                player.BusyDelay(Player.BusyType.FULL_LOCK, "Fishing", 30000, (timer, arg) =>
                {
                    if (player.Interacting)
                    {
                        player.Interacting = false;
                        player.StopInteracting();
                    }
                });

                Server.Instance.LoopedDelay(30, 1000, (timer, arg) =>
                {

                    if (!player.Interacting || !Formula.InRadius(player.transform.position, obj.transform.position, obj.Definition.InteractableRadius + 1))
                    {
                        timer.Stop();
                        player.StopInteracting();
                        return;
                    }
                    player.LastActionTimer = timer;
                    if (player.Interacting)
                    {

                        if (!BaitToolCheck(player, obj))
                            return;
                        if (TRShared.Data.Formula.rand.NextDouble() < 0.27)
                        {
                            player.NetworkActions.PlaySound("fish");
                            if (obj.Definition.FishDef.ToolID == 535)
                            {
                                player.NetworkActions.PlaySound("spear_fishing");
                            }
                            player.Skills.AddExp(Stats.SKILLS.Fishing, obj.Definition.FishDef.Exp);
                            player.Inventory.AddItem(obj.Definition.FishDef.FishID);

                            player.NetworkActions.ItemGain(obj.Definition.FishDef.FishID, 1);
                            player.Inventory.RemoveItem(obj.Definition.FishDef.BaitID, 1);
                            player.NetworkActions.SendInventory();
                                //  player.Msg("You catch a " + DataManager.ItemDefinitions[obj.Definition.FishDef.FishID].ItemName + "!");

                            }
                    }
                    else
                    {
                        timer.Stop();

                    }

                });
            }

        }
    }
}
