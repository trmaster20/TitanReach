using System;
using TitanReach_Server.Model;
using TRShared;
using TRShared.Data.Definitions;
using TRShared.Data.Enums;
using TRShared.Data.Structs;
using static TitanReach_Server.Model.Player;

namespace TitanReach_Server.Skills
{
    class Arcana
    {

        public static void CastSpell(Player p, int spellId, bool cheats = false)
        {

            if (spellId < 0 || spellId >= DataManager.SpellDefinitions.Length)
            {
                p.NetworkActions.SendMessage("This isnt a valid spell");
                return;
            }

            SpellDef spellDef = DataManager.SpellDefinitions[spellId];
            if (!cheats)
            {
                if (p.Skills.GetCurLevel((int)Stats.SKILLS.Arcana) < spellDef.LevelRequirement)
                {
                    p.NetworkActions.SendMessage("You dont have the Arcana level for this spell");
                    return;
                }

                foreach (Ingredient ing in spellDef.Ingredients)
                {
                    if (!p.Inventory.HasItem((ushort)ing.ItemID, ing.Amount))
                    {
                        p.NetworkActions.SendMessage("You dont have the required ingredients for this spell");
                        return;
                    }
                }
            }
            p.NetworkActions.SendAnimationType(1, (UInt16)(30 + spellId), true);

            // ----- Spell is valid and will be cast after this point ----- \\
            p.BusyDelay(BusyType.FULL_LOCK, "Teleporting", 1500, (timer, arg) =>
            {
                foreach (Ingredient ing in spellDef.Ingredients)
                {
                    p.Inventory.RemoveItem(ing.ItemID, ing.Amount);
                }

                p.NetworkActions.SendInventory();

                p.Skills.AddExp(Stats.SKILLS.Arcana, spellDef.XP);

                if (spellDef.SpellType == SpellType.Teleport)
                {
                    TeleportLocationDef teleLoc = DataManager.TeleportLocations[0];
                    if (spellId == 0)  //coincidence that the spellID matches the teleLocID for now~
                    {
                        teleLoc = DataManager.TeleportLocations[0];
                    }
                    else if (spellId == 1)
                    {
                        teleLoc = DataManager.TeleportLocations[1];
                    }
                    else if (spellId == 2)
                    {
                        teleLoc = DataManager.TeleportLocations[2];
                    }

                    float radius = teleLoc.Radius * (float)TRShared.Data.Formula.rand.NextDouble();
                    float angle = 3.141592f * 2 * (float)TRShared.Data.Formula.rand.NextDouble();

                    float newX = teleLoc.X + teleLoc.Radius * MathF.Sin(angle);
                    float newZ = teleLoc.Z + teleLoc.Radius * MathF.Cos(angle);

                    p.TeleportTo(0, newX, DataManager.GetHeight((int)newX, (int)newZ, p.Map.Land), newZ);
                }
            });

        }
    }
}
