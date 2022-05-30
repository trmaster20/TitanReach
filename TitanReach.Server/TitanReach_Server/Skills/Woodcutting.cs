using System;
using TitanReach_Server.Model;
using TitanReach_Server.Utilities;
using TRShared.Data.Enums;

namespace TitanReach_Server.Skills
{
    class Woodcutting : Skill
    {

        static Woodcutting()
        {
            EventManager.ObjectInteractionEvent += WoodcutInteract;
        }

        public static bool ToolCheck(Player player, Obj obj)
        {
            if (player.Equipment.EquippedItems[10] == null || !player.Equipment.RIGHT_HAND.Definition.CanCut)
            {
                player.Error("You need an axe to cut this");
                return false;
            }
            return true;
        }

        private static void WoodcutInteract(Player player, Obj obj, int type, int opt, bool active)
        {
            if (!obj.Definition.HasAttribute(ObjectAttribute.Chopable))
                return;

            if (player.Busy || obj.Depleted)
                return;
            if (player.Equipment.EquippedItems[10] != null && Environment.TickCount - player.LastActiveAction < (player.Equipment.EquippedItems[10].Definition.WeaponSpeed * 0.8))
                return;
            if (!ToolCheck(player, obj))
                return;
            var def = obj.Definition.WoodcuttingDef;
            if (def == null)
                return;
            int pWCLevel = player.Skills.GetCurLevel((int)Stats.SKILLS.Woodcutting);
            if (pWCLevel < def.LevelReq)
            {

                player.Error("This tree requires level " + def.LevelReq + " woodcutting to cut");
                player.StopInteracting();
                return;
            }

            if(active)
            {
                HandleCut(player, obj, active);
                player.LastActiveAction = Environment.TickCount;
                return;
            }


            if (!active)
            {
                if (!player.Interacting)
                    player.Interacting = true;

                player.BusyDelay(Player.BusyType.FULL_LOCK, "Woodcutting", 30000, (timer, arg) =>
                {
                    if (player.Interacting)
                    {
                        player.Interacting = false;
                        player.StopInteracting();
                    }
                });

                Server.Instance.LoopedDelay(30, 1000, (timer, arg) =>
                {
                    player.LastActionTimer = timer;
                    var def = obj.Definition.WoodcuttingDef;
                    if (!player.Interacting || !player.Busy || def == null || !Formula.InRadius(player.transform.position, obj.transform.position, obj.Definition.InteractableRadius + 1))
                    {
                        timer.Stop();
                        player.StopInteracting();
                        return;
                    }

                    if (player.Interacting)
                    {
                        if (!ToolCheck(player, obj))
                            return;
                        HandleCut(player, obj, active);
                    }
                    else
                    {
                        timer.Stop();
                    }

                });
            }
        }

        public static void HandleCut(Player player, Obj obj, bool active)
        {
            var def = obj.Definition.WoodcuttingDef;
            int axeID = player.Equipment.RIGHT_HAND.Definition.ItemID;
            float successChance = def.BaseSize + MathF.Sqrt(player.Skills.GetCurLevel((int)Stats.SKILLS.Woodcutting) - def.LevelReq) * def.BaseDifficulty;
            if (axeID == 411) //iron
                successChance = 1 - (1 - successChance) * 0.85f;
            if (axeID == 417 || axeID == 573) //steel
                successChance = 1 - (1 - successChance) * 0.75f;
            if (axeID == 566) //verbatius
                successChance = 1 - (1 - successChance) * 0.70f;
            if (axeID == 561) //verbatius
                successChance = 1 - (1 - successChance) * 0.65f;

            if (player.rand.NextDouble() < (successChance * (active ? 2f : 1f)))
            {
                player.Inventory.AddItem((ushort)def.LogID);
                player.NetworkActions.ItemGain(def.LogID, 1);


                player.NetworkActions.SendInventory();
                player.Skills.AddExp((int)Stats.SKILLS.Woodcutting, def.Exp);
                if (obj.Definition.HasAttribute(ObjectAttribute.Depletable))
                {
                    if (player.rand.Next(0, 100) < (active ? 33 : 15))
                    {
                        obj.Deplete(def.RespawnTime);
                        player.StopInteracting();
                    }
                }
            }
        }

    }
}
