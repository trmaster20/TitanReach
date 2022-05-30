using System;
using System.Collections.Generic;
using System.Text;
using TRShared.Data.Structs;

namespace TRShared.Data.Definitions
{
    public class ItemDefinition
    {
        public object Untyped_ItemIcon;
        public ushort ItemID;
        public string ItemName;
        public string ItemDescription;
        public bool IsToken = false;
        public int Value;
        public string[] Options;
        public bool Tradable = true;
        public int MaxStackSize = 1;
        public string ItemCreatorStr;
        public string ItemMaterial;
        public int BonusArmor;
        public int BonusDamage;
        public int BonusHealth;
        public string ItemModelName = "";
        public string ItemObjectName = "";
        public bool CanMine = false;
        public bool CanCut = false;
        public int WeaponType;
        public int WeaponTypeOneHand = 9; // TODO add this in db eventually
        public int WeaponSpeed = 2000;
        public string ItemObject;
        public bool IsMagicWeapon = false;
        public int MagicType; // 1=ice, 2=fire, 3=lightning
        public bool Legacy = false;
        public int SoundType;
        public bool Droppable = true;
        public string UmaFemaleRecepie;
        public string UmaMaleRecepie;
        public int EatibleID = -1;
        public int ProjectileID = -1;
        public ItemBonusDef BonusDef = null;
        public List<StatReq> StatReqs = new List<StatReq>();
        public EquippedItemPositions EquippedItemPosition = EquippedItemPositions.NOT_EQUIPPABLE;

        public int NonTokenID => IsToken ? ItemID - (ushort.MaxValue / 2) : ItemID;
        public int TokenID => !IsToken ? ItemID + (ushort.MaxValue / 2) : ItemID;
        public string UmaSlotName => UmaSlotNames[(int)EquippedItemPosition];
        public enum EquippedItemPositions { NOT_EQUIPPABLE = 0, Head = 1, Chest = 2, Legs = 3, Hands = 4, Boots = 5, Neck = 6, Back = 7, Ring = 8, LeftHand = 9, RightHand = 10, AnyHand = 11, BothHands = 12 };
        public enum EquippedBlendShapes { CHEST = 0, LEGS = 1, FEET = 2 };

        public bool IsStackable => MaxStackSize > 1;

        public bool IsFishingTool => ItemID == 160 || ItemID == 161 || ItemID == 435; // remove

        public static string[] UmaSlotNames = { "",
            "Helmet", "Chest", "Legs", "Hands", "Feet", "Necklace", "Back",
            "Finger", "Left Hand", "Right Hand", "One Hand", "Two Hand"
        };

        private static ushort tokenOffset = ushort.MaxValue / 2;

        [Definition("ItemDef")]
        public static void Load(DTWrapper dt)
        {

            int tokens = 0;
            int count = 0;
            DataManager.ItemDefinitions = new ItemDefinition[ushort.MaxValue];
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                ItemDefinition def = new ItemDefinition()
                {
                    ItemID = dt.Ushort(0),
                    ItemName = dt.String(1),
                    ItemDescription = dt.String(2),
                    Options = dt.String(3) != null ? dt.String(3).Split(',') : null,
                    Droppable = dt.Int(4) == 1,
                    Tradable = dt.Int(5) == 1,
                    WeaponType = dt.Int(17),
                    WeaponSpeed = dt.Int(18),
                    MaxStackSize = dt.Int(6),
                    EquippedItemPosition = (EquippedItemPositions)dt.Int(7),
                    BonusArmor = dt.Int(8),
                    BonusDamage = dt.Int(9),
                    BonusHealth = dt.Int(10),
                    UmaFemaleRecepie = dt.String(24),
                    UmaMaleRecepie = dt.String(25),
                    ItemModelName = dt.String(12),
                    ItemObjectName = dt.String(13),
                    CanMine = dt.Int(14) == 1,
                    CanCut = dt.Int(15) == 1,
                    ItemCreatorStr = dt.String(16),
                    ItemMaterial = dt.String(19),
                    Untyped_ItemIcon = dt.String(11),
                    Legacy = dt.Int(20) == 1,
                    Value = dt.Int(21),
                    SoundType = dt.Int(23),
                    ItemObject = dt.String(13)
                };
                count++;



                if(dt.String(22) != null)
                {
                    foreach(string s in dt.String(22).Split(','))
                    {
                        var r = s.Split('=');
                        def.StatReqs.Add(new StatReq() { Stat = int.Parse(r[0]), MinLv = int.Parse(r[1]) });
                    }
                }

                if (def.MaxStackSize == 0)
                    def.MaxStackSize = int.MaxValue;
                if (def.WeaponType == 6) {                 
                    def.IsMagicWeapon = true;
                    if (def.ItemID == 130)
                        def.MagicType = 1;
                    if (def.ItemID == 131)
                        def.MagicType = 2;
                }

                DataManager.ItemDefinitions[def.ItemID] = def;
                if (def.MaxStackSize == 1 && def.Tradable)
                {
                    ItemDefinition token = new ItemDefinition();
                    token.ItemID = (ushort)(tokenOffset + def.ItemID);
                    token.IsToken = true;
                    token.ItemName = def.ItemName;
                    token.ItemDescription = def.ItemDescription;
                    token.MaxStackSize = int.MaxValue;
                    token.Value = def.Value;
                    // token.ItemIcon = def.ItemIcon;
                    DataManager.ItemDefinitions[token.ItemID] = token;
                    tokens++;
                }

            }
            dt.Destroy();



            Logger.Log("Loaded " + count + " ItemDefs " + tokens + " Tokens");
        }
    }
}
