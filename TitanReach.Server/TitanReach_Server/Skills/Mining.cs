using System;
using TitanReach_Server.Model;
using TitanReach_Server.Utilities;
using TRShared.Data.Enums;

namespace TitanReach_Server.Skills
{
    class Mining : Skill
    {

        static Mining()
        {
            EventManager.ObjectInteractionEvent += MiningInteract;
        }

        public static bool ToolCheck(Player player, Obj obj)
        {
            if (player.Equipment.EquippedItems[10] == null || !player.Equipment.RIGHT_HAND.Definition.CanMine)
            {
                player.Error("You need a pickaxe to mine this");
                return false;
            }

            return true;
        }

        private static void MiningInteract(Player player, Obj obj, int type, int opt, bool active)
        {
            if (!obj.Definition.HasAttribute(ObjectAttribute.Mineable))
                return;
            if (player.Busy)
                return;


            if (!obj.Depleted)
            {

                if (!ToolCheck(player, obj))
                {

                    return;
                }


                if (!player.Interacting)
                {
                    player.Interacting = true;
                }
                player.BusyDelay(Player.BusyType.FULL_LOCK, "Mining", 30000, (timer, arg) =>
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
                    var def = obj.Definition.MiningDef;
                    if (!player.Interacting || !player.Busy || def == null || !Formula.InRadius(player.transform.position, obj.transform.position, obj.Definition.InteractableRadius + 1))
                    {
                        timer.Stop();
                        player.StopInteracting();
                        return;
                    }
                    int pMiningLevel = player.Skills.GetCurLevel((int)Stats.SKILLS.Mining);
                    if (pMiningLevel < def.LevelReq)
                    {

                        player.Error("This rock requires level " + def.LevelReq + " mining to mine");
                        player.StopInteracting();
                        return;
                    }

                    if (player.Interacting)
                    {

                        if (!ToolCheck(player, obj))
                            return;

                        int pickID = player.Equipment.RIGHT_HAND.Definition.ItemID;
                        float successChance = def.BaseChance + MathF.Sqrt(pMiningLevel - def.LevelReq) * def.LevelScale;
                        if (pickID == 415) //iron
                            {
                            successChance = 1 - (1 - successChance) * 0.90f;
                        }
                        if (pickID == 421 || pickID == 573){ //steel
                            successChance = 1 - (1 - successChance) * 0.85f;
                        }
                        if (pickID == 566){ //verbatius
                            successChance = 1 - (1 - successChance) * 0.82f;
                        }
                        if (pickID == 561){ //cobalt
                            successChance = 1 - (1 - successChance) * 0.80f;
                        }
                        if (player.rand.NextDouble() < successChance)
                        {
                                //player.NetworkActions.SendMessage("<color=white>You mine some " + DataManager.ItemDefinitions[def.OreID].ItemName + "</color>");
                                if (player.rand.NextDouble() < def.RareItemChance)
                            {
                                player.Inventory.AddItem(def.RareItemID);
                                player.NetworkActions.ItemGain(def.RareItemID, 1);
                            }
                            else
                            {
                                player.Inventory.AddItem((ushort)def.OreID);
                                player.NetworkActions.ItemGain(def.OreID, 1);
                            }

                            player.NetworkActions.SendInventory();
                            player.Skills.AddExp((int)Stats.SKILLS.Mining, def.Exp);
                            if (obj.Definition.HasAttribute(ObjectAttribute.Depletable))
                            {
                                if (player.rand.Next(0, 100) < 18)
                                {
                                    obj.Deplete(def.RespawnTime);
                                    player.StopInteracting();
                                }
                            }
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
