using System;
using System.Collections.Generic;
using System.Text;
using TRShared.Data.Enums;

namespace TRShared.Data.Definitions
{
    public class ProjectileDefinition
    {

        public int ProjectileID = -1;
        public string ProjectileName = "";
        public int Damage = 0;
        public string DamageType = "";
        public string Path = "";
        public int category = -1;
        public float speed;
        public float time;
        public float radius;
        public int ItemID = -1;

        public int TriggerBuffOnCast = -1;
        public int TriggerBuffOnHit = -1;
        public WeaponCategory weaponCategory = WeaponCategory.GENERICARROW;
        public DamageType damageType = TRShared.Data.Enums.DamageType.RANGED;

        [Definition("ProjectileDef")]
        public static void Load(DTWrapper dt)
        {
            DataManager.ProjectileDefinitions = new ProjectileDefinition[dt.Count];
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                ProjectileDefinition def = new ProjectileDefinition()
                {
                    ProjectileID = dt.Int(0),
                    ProjectileName = dt.String(1),
                    ItemID = dt.Int(2),
                    Damage = dt.Int(3),
                    DamageType = dt.String(4),
                    Path = dt.String(5),
                    speed = dt.Float(6),
                    time = dt.Float(7),
                    radius = dt.Float(8),
                    category = dt.Int(9),
                    TriggerBuffOnCast = dt.Int(11),
                    TriggerBuffOnHit = dt.Int(10),
                    weaponCategory = (WeaponCategory)dt.Int(9),
                    damageType = Formula.EvaluateDamageType((WeaponCategory)dt.Int(9))
                };

                DataManager.ProjectileDefinitions[i] = def;

            }



            dt.Destroy();
            Logger.Log("Loaded " + DataManager.ProjectileDefinitions.Length + " ProjectileDefs");

            DataManager.OnAllDefinitionsLoaded += (sender, obj) => {
                foreach (var itm in DataManager.ItemDefinitions)
                {
                    if (itm == null)
                        continue;
                    foreach (var proj in DataManager.ProjectileDefinitions)
                    {
                        if (proj == null)
                            continue;
                        if (itm.ItemID == proj.ItemID)
                        {
                            itm.ProjectileID = proj.ProjectileID;
                            break;
                        }
                    }
                }
            };
        }
    }
}
