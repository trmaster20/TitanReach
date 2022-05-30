using System;
using System.Collections.Generic;
using System.Text;

namespace TRShared.Data.Definitions
{
    public class PetDef
    {

        public int PetID = -1;
        public int TypeID = -1;        //used for different skins of same pet -> same type id different variation id
        public int VariationID = -1;   //used for different skins of same pet -> same type id different variation id
        public string Name = "";
        public string Path = "";
        public int[] NPCDropIds;
        public int Rarity;
       
        [Definition("PetDef")]
        public static void Load(DTWrapper dt)
        {

            DataManager.PetDefinitions = new PetDef[dt.Count];
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                PetDef def = new PetDef()
                {
                    PetID = dt.Int(0),
                    TypeID = dt.Int(1),
                    VariationID = dt.Int(2),
                    Name = dt.String(3),
                    Path = dt.String(4),
                    
                };

                if (dt.String(5) != null)
                {
                    def.NPCDropIds = Formula.StringArrayToInt(dt.String(5).Split(','));
                }
                if(dt.Int(6) != null)
                {
                    def.Rarity = dt.Int(6);
                }

                DataManager.PetDefinitions[def.PetID] = def;
            }
            dt.Destroy();

 
            Logger.Log("Loaded " + DataManager.PetDefinitions.Length + " PetDefs");
        }

    }
}
