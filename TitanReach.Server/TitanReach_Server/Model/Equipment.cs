
using TitanReach_Server.Utilities;
using TRShared.Data.Definitions;
using TRShared.Data.Enums;

namespace TitanReach_Server.Model
{
    public class Equipment
    {

        public Player player;
        private TotalBonus totalBonus;
        public Item[] EquippedItems = new Item[11];

        public bool IsUnequipped()
        {
            for (int i = 0; i < EquippedItems.Length; i++)
            {
                Item eq = EquippedItems[i];
                if (eq != null)
                    return false;
            }
            return true;
        }

        public Item RIGHT_HAND
        {
            get
            {
                return EquippedItems[10];
            }
            set
            {
                EquippedItems[10] = value;
            }
        }
        public Item LEFT_HAND
        {
            get
            {
                return EquippedItems[9];
            }
            set
            {
                EquippedItems[9] = value;
            }
        }
        public Item HEAD
        {
            get
            {
                return EquippedItems[1];
            }
            set
            {
                EquippedItems[1] = value;
            }
        }
        public Item CHEST
        {
            get
            {
                return EquippedItems[2];
            }
            set
            {
                EquippedItems[2] = value;
            }
        }
        public Item LEGS
        {
            get
            {
                return EquippedItems[3];
            }
            set
            {
                EquippedItems[3] = value;
            }
        }
        public Item HANDS
        {
            get
            {
                return EquippedItems[4];
            }
            set
            {
                EquippedItems[4] = value;
            }
        }
        public Item BOOTS
        {
            get
            {
                return EquippedItems[5];
            }
            set
            {
                EquippedItems[5] = value;
            }
        }
        public Item NECK
        {
            get
            {
                return EquippedItems[6];
            }
            set
            {
                EquippedItems[6] = value;
            }
        }
        public Item BACK
        {
            get
            {
                return EquippedItems[7];
            }
            set
            {
                EquippedItems[7] = value;
            }
        }
        public Item LEFT_RING
        {
            get
            {
                return EquippedItems[8];
            }
            set
            {
                EquippedItems[8] = value;
            }
        }
        //public Item RIGHT_RING {
        //    get {
        //        return EquippedItems[8];
        //    }
        //    set {
        //        EquippedItems[8] = value;
        //    }
        //}



        public Equipment(Player p)
        {
            this.player = p;
            totalBonus = new TotalBonus();
        }

        private class TotalBonus
        {
            public int MelleAB = 0;  //Accuracy Bonus
            public int MellePB = 0;  //Power Bonus
            public int MelleDB = 0;  //Defence Bonus
            public int RangedAB = 0;
            public int RangedPB = 0;
            public int RangedDB = 0;
            public int MagicAB = 0;
            public int MagicPB = 0;
            public int MagicDB = 0;
            public bool NeedsResetting = true;

            public TotalBonus()
            {

            }
            public void ResetBonuses()
            {
                MelleAB = 0;
                MellePB = 0;
                MelleDB = 0;
                RangedAB = 0;
                RangedPB = 0;
                RangedDB = 0;
                MagicAB = 0;
                MagicPB = 0;
                MagicDB = 0;
            }
            public void AddBonusDefToTotal(ItemBonusDef def)
            {
                MelleAB += def.MeleeAB;
                MellePB += def.MeleePB;
                MelleDB += def.MeleeDB;
                RangedAB += def.RangedAB;
                RangedPB += def.RangedPB;
                RangedDB += def.RangedDB;
                MagicAB += def.MagicAB;
                MagicPB += def.MagicPB;
                MagicDB += def.MagicDB;
            }
        }

        private void ResetTotalStats() //Updates totalStats with all equipped items
        {
            totalBonus.ResetBonuses();
            foreach (Item item in EquippedItems)
            {
                if (item == null) continue;
                if (item.Definition.BonusDef == null)
                {
                 //   Server.Log("there is no bonus for this item");
                    continue;
                }
                totalBonus.AddBonusDefToTotal(item.Definition.BonusDef);
            }
        }

        private void CheckReset() //Checks if totalBonus needs updating
        {
            if (totalBonus.NeedsResetting == true)
            {
                ResetTotalStats();
                totalBonus.NeedsResetting = false;
            }
        }

        public int GetAccuracyBonus(DamageType damageType)
        {
            CheckReset();

            if (damageType == DamageType.MELEE) return totalBonus.MelleAB;
            else if (damageType == DamageType.RANGED) return totalBonus.RangedAB;
            else if (damageType == DamageType.MAGIC) return totalBonus.MagicAB;
            else
            {
                Server.Error("Invalid Syle Selected");
                return 0;
            }
        }

        public int GetPowerBonus(DamageType damageType)
        {
            CheckReset();

            if (damageType == DamageType.MELEE) return totalBonus.MellePB;
            else if (damageType == DamageType.RANGED) return totalBonus.RangedPB;
            else if (damageType == DamageType.MAGIC) return totalBonus.MagicPB;
            else
            {
                Server.Error("Invalid Syle Selected");
                return 0;
            }
        }

        public int GetDefenceBonus(DamageType damageType)
        {
            CheckReset();

            if (damageType == DamageType.MELEE) return totalBonus.MelleDB;
            else if (damageType == DamageType.RANGED) return totalBonus.RangedDB;
            else if (damageType == DamageType.MAGIC) return totalBonus.MagicDB;
            else
            {
                Server.Error("Invalid Syle Selected");
                return 0;
            }
        }

        public void Delete(int slot)
        {
            totalBonus.NeedsResetting = true;
            EquippedItems[slot] = null;
        }

        public void Unequip(int slot)
        {
            totalBonus.NeedsResetting = true;

            if (EquippedItems[slot] != null)
            {
              //  Server.Log("added unequipped item to inv: " + EquippedItems[slot]);
                player.Inventory.AddItem(EquippedItems[slot]);
            }
            EquippedItems[slot] = null;


            if (slot == 9 || slot == 10)
            {
                player.NetworkActions.PlaySound("item_unequip");
            }
            else
            {
                player.NetworkActions.PlaySound("armor_unequip");
            }
        }


        public void Equip(Item item)
        {
            totalBonus.NeedsResetting = true;

            int slot = (int)item.Definition.EquippedItemPosition;
            // Server.Log("Equipping slot " + slot);
            /*if(item.Definition.EquippedItemPosition == Definitions.ItemDefinition.EquippedItemPositions.Head)
                slot = 2;
            if (item.Definition.EquippedItemPosition == Definitions.ItemDefinition.EquippedItemPositions.Chest)
                slot = 3;
            if (item.Definition.EquippedItemPosition == Definitions.ItemDefinition.EquippedItemPositions.Legs)
                slot = 4;
            if (item.Definition.EquippedItemPosition == Definitions.ItemDefinition.EquippedItemPositions.Boots)
                slot = 5;
            if (item.Definition.EquippedItemPosition == Definitions.ItemDefinition.EquippedItemPositions.Wrist)
                slot = 10;
            if (item.Definition.EquippedItemPosition == Definitions.ItemDefinition.EquippedItemPositions.Neck)
                slot = 6;
            if (item.Definition.EquippedItemPosition == Definitions.ItemDefinition.EquippedItemPositions.Back)
                slot = 7;*/


            //if (item.Definition.EquippedItemPosition == Definitions.ItemDefinition.EquippedItemPositions.Ring)
            //{
            //    if (EquippedItems[8] == null)
            //        slot = 8;
            //    else if(EquippedItems[9] == null)
            //    {
            //        slot = 9;
            //    } else { 
            //        slot = 8;
            //    }
            //}

            /*  if (item.Definition.EquippedItemPosition == Definitions.ItemDefinition.EquippedItemPositions.RightHand)
              {
                  slot = 0;
                  if (EquippedItems[1] != null && EquippedItems[1].Definition.EquippedItemPosition == Definitions.ItemDefinition.EquippedItemPositions.BothHands) // unequip 2 hander to wield  a 1 hand
                      Unequip(1);
              }*/

            /* if (item.Definition.EquippedItemPosition == Definitions.ItemDefinition.EquippedItemPositions.LeftHand)
             {
                 //slot = 1;
                 if (EquippedItems[0] != null && EquippedItems[0].Definition.EquippedItemPosition == Definitions.ItemDefinition.EquippedItemPositions.BothHands) // unequip 2 hander to wield  a 1 hand
                     Unequip(0);
             }*/


            /*if (item.Definition.EquippedItemPosition == Definitions.ItemDefinition.EquippedItemPositions.AnyHand)
            {
                if (EquippedItems[0] == null)
                    slot = 0;
                else if (EquippedItems[1] == null)
                {
                    slot = 1;
                }
                else
                {
                    slot = 0;
                }
            }
               
            if (item.Definition.EquippedItemPosition == Definitions.ItemDefinition.EquippedItemPositions.BothHands)
            {
                slot = 0;
                Unequip(1);
            }*/


            Item ite = new Item(item.ID);
            ite.Amount = item.Amount;
            //ite.UID = item.UID;

            player.Inventory.RemoveItem(item, item.Amount);
            if (EquippedItems[slot] != null) // put old worn item into inventory (need to do full inventory checks here)
            {
                Unequip(slot);
            }
            else
            {

            }
            if (ite == null)
            {
                Server.Log("ite is null...");
            }
            EquippedItems[slot] = ite;
            if (slot == 9 || slot == 10)
            {
                player.NetworkActions.PlaySound("item_equip");
            }
            else
            {
                player.NetworkActions.PlaySound("armor_equip");
            }

            player.NetworkActions.SendInventory();
            player.NetworkActions.SendLocalPlayerEquipmentUpdate();






            /* Item ite = new Item();
             ite.ID = 1; // redo when item system in place.
             slot = ite;


                 GameObject wep = GameObject.Find("Weapons");
                 GameObject weapon = null;

                 Transform hand = Utility.FindChildObjectsWithTag(player.Game_Object.transform, "RightHandWeaponSlot")[0].transform;
                 weapon = GameObject.Instantiate(wep.transform.Find(name).gameObject);
                 if (weapon == null)
                     Debug.Log("null weapon");
                 if (hand == null)
                     Debug.Log("null hand");


                 //   DisableChildren(hand.transform);
                 weapon.SetActive(true);

             // BELOW CODE IS TEMPORARY! once we get correct voxel weapons/equipment, and correcwt positioning we will set one static coords here.
                 weapon.transform.SetParent(hand);
                 weapon.transform.localPosition = new Vector3(-0.0163f, -0.0202f, 0.0051f);
                 weapon.transform.localEulerAngles = new Vector3(166.344f, -17.67798f, 23.75899f);
                 weapon.transform.localScale = new Vector3(0.001513898f, 0.002537166f, 0.002867232f);
 */




        }
    }
}
