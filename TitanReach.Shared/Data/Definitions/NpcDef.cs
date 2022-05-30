using System;
using System.Collections.Generic;
using System.Text;

namespace TRShared.Data.Definitions
{
    public class NpcDef
    {

        public int NpcID = -1;
        public string Name = "";
        public string Path = "";
        public string Title = null;
        public bool Friendly;
        public bool Aggressive = false;
        public int MaxHealth;
        public float MoveSpeed = 1.0f;
        public int[] Equipment = new int[0];
        public int[] Appearance = new int[0];
        public int CombatLevel = 1;
        public bool IsHumanModel = false;
        public bool CanWearEquipment = true;
        public int DefaultStance = 0;
        public object Untyped_NpcModel = null;
        public string WardrobeCollection = "";
        public bool isMale = true;
        public int fakeWeapon = -1;
        public float InteractionRadius = 2;

        public int AttackLevel = 1;  //create struct for these maybe
        public int StrengthLevel = 1;
        public int DefenceLevel = 1;
        public int AttackBonus = 1;
        public int StrengthBonus = 1;
        public int DefenceBonus = 1;

        public int DropTableID = 0;
        public int AiType = 0;

        public float AttackRadius = 3.0f;
        public float AttackSpeed = 1.0f;

        [Definition("NpcDef")]
        public static void Load(DTWrapper dt)
        {
            DataManager.NpcDefinitions = new NpcDef[dt.Count + 1];
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                NpcDef def = new NpcDef()
                {
                    NpcID = dt.Int(0),
                    Name = dt.String(1),
                    Path = dt.String(2),
                    Title = dt.String(3),
                    Friendly = dt.Int(4) == 1,
                    Aggressive = dt.Int(5) == 1,
                    MaxHealth = dt.Int(6),
                    MoveSpeed = dt.Float(9),
                    WardrobeCollection = dt.String(11),
                    fakeWeapon = dt.Int(14),
                    // InteractionRadius = dt.Float(15),
                    DropTableID = dt.Int(10),
                    AiType = dt.Int(13),
                    AttackRadius = dt.Float(16),
                    AttackSpeed = dt.Float(17)
                };
                if (dt.String(12) != null)
                {
                    def.CanWearEquipment = true;
                    def.Equipment = Formula.StringArrayToInt(dt.String(12).Split(','));
                }
                if (dt.String(7) != null)
                {
                    int[] lv = Formula.StringArrayToInt(dt.String(7).Split(','));
                    if (lv.Length == 3)
                    {
                        def.AttackLevel = lv[0];
                        def.StrengthLevel = lv[1];
                        def.DefenceLevel = lv[2];
                        def.CombatLevel = Formula.CombatLevel(lv[0], lv[1], lv[2], def.MaxHealth / 10 + 8);
                    }
                }
                if (dt.String(8) != null)
                {
                    int[] lv = Formula.StringArrayToInt(dt.String(8).Split(','));

                    if (lv.Length == 3)
                    {
                        def.AttackBonus = lv[0];
                        def.StrengthBonus = lv[1];
                        def.DefenceBonus = lv[2];
                        def.CombatLevel = Formula.CombatLevel(lv[0], lv[1], lv[2], def.MaxHealth / 10 + 8);
                    }
                }
                // def.Untyped_NpcModel = (GameObject)Resources.Load(def.Path, typeof(GameObject));
                DataManager.NpcDefinitions[def.NpcID] = def;
            }
            dt.Destroy();
           
            Logger.Log("123 Loaded " + DataManager.NpcDefinitions.Length + " NpcDefs");
        }

    }
}
