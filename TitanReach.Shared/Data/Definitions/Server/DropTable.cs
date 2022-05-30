using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TRShared.Data.Enums;

namespace TRShared.Data.Definitions
{
    public class DropDef
    {
        public int ID;
        public string TableName;
        public List<ItemDrop> Drops;
        public List<ItemDrop> GuaranteedDrops;
        public int TotalWeight;

        [Definition("NpcDrops")]
        public static void Load(DTWrapper dt)
        {
            
            DataManager.DropTables = new DropDef[dt.Count];
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                DropDef def = new DropDef()
                {
                   ID = dt.Int(0),
                   TableName = dt.String(1)
                };
                def.Drops = new List<ItemDrop>();
                def.GuaranteedDrops = new List<ItemDrop>();
                int totalWeight = 0;

                string[] drops = dt.String(2).Split('|');

                foreach (string s in drops)
                {
                    int count = s.Count(x => x == '=');
                    ItemDrop drop = new ItemDrop();

                    if (count == 0 && s.Length > 0)     //example 347
                    {
                        drop.ItemID = ushort.Parse(s);
                        drop.Amount = 1;
                        drop.Weight = 0;
                        def.GuaranteedDrops.Add(drop);
                    }
                    if (count == 1)  //example 216=5,10
                    {
                        drop.ItemID = ushort.Parse(s.Split('=')[0]);
                        if (s.Split('=')[1].Contains(','))
                        {
                            var ss = s.Split('=')[1].Split(',');
                            drop.AmountMin = int.Parse(ss[0]);
                            drop.AmountMax = int.Parse(ss[1]);
                        }
                        else drop.Amount = int.Parse(s.Split('=')[1]);

                        drop.Weight = 0;
                        def.GuaranteedDrops.Add(drop);
                    }
                    if (count == 2)           //example 216=5=10
                    {
                        drop.ItemID = ushort.Parse(s.Split('=')[0]);
                        drop.Amount = int.Parse(s.Split('=')[1]);
                        int weight = int.Parse(s.Split('=')[2]);
                        drop.DBWeight = weight;
                        drop.Weight = weight + totalWeight;
                        totalWeight += weight;
                        def.Drops.Add(drop);
                    }
                }
                def.TotalWeight = totalWeight;
                DataManager.DropTables[i] = def;
                //def.PrintDropTable();

            }
            dt.Destroy();
            Logger.Log("Loaded " + DataManager.DropTables.Length + " DropDefs");
        }


        public void PrintDropTable()
        {
            Logger.Log("-----------------------------------------Printing [" + TableName + "] Drop Table--------------------------------------------");
            Logger.Log("Info For Drop Table [" + TableName + "] ID: [" + ID + "]                                " + TableName);
            if (GuaranteedDrops.Count > 0)
            {
                Logger.Log("Guaranteed Drops:");
                foreach (ItemDrop drop in GuaranteedDrops)
                {
                    Logger.Log("ItemName: [" + DataManager.ItemDefinitions[drop.ItemID].ItemName + "] Item ID: [" + drop.ItemID + "] Ammount: [" + drop.Amount + "]");
                }
            }
            else
            {
                Logger.Log("No Guaranted Drops:");
            }
            if (Drops.Count > 0)
            {
                Logger.Log("Random Drops:    TotalWeight: [" + TotalWeight + "]");
                foreach (ItemDrop drop in Drops)
                {
                    float chance = ((float)drop.DBWeight / (float)TotalWeight) * 100.0f;
                    if (DataManager.ItemDefinitions[drop.ItemID] != null)
                    {
                        Logger.Log("ItemName: [" + DataManager.ItemDefinitions[drop.ItemID].ItemName + "] Item ID: [" + drop.ItemID + "] Ammount: [" + drop.Amount + "] Weight: [" + drop.DBWeight + "] Chance : [" + chance + "%]");

                    }
                    else
                    {
                        Logger.Log("Null Drop:");
                    }
                }
            }
            else
            {
                Logger.Log("No Random Drops:");
            }



        }

    }
}
