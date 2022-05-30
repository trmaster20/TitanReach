using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using TRShared.Data.Structs;

namespace TRShared.Data.Definitions
{
    public class ShopDef
    {

        public List<ShopItem> Items;
        public bool General = false;
        public float SellMultiplier;
        public float BuyMultiplier;
        public bool CanSell;
        public int ID;
        public string ShopName;

        public class ShopItem
        {
            public ushort ItemID;
            public int Amount; // current shop stock
            public int OriginalAmount; // default shop stock
            public bool OriginalItem = false; // is this a permanent item in a shop (for example, pots in a general store?) - YES it is. its an item that belongs in the shop. for example a fishing shop, if it has carp with 0 stock, this is still an original item
            public int RespawnTime; // time to restock this item if amount is lower than original amount
            public int LastRespawnCheck = Environment.TickCount; // ignore this
        }

        [Definition("ShopDef")]
        public static void Load(DTWrapper dt)
        {
            DataManager.ShopDefinitions = new ShopDef[dt.Count];
            for (int i = 0; i < dt.Count; i++)
            {
                dt.Row = i;
                ShopDef def = new ShopDef()
                {
                    ShopName = dt.String(0),
                    ID = dt.Int(1),
                    SellMultiplier = dt.Float(3),
                    BuyMultiplier = dt.Float(4),
                    CanSell = dt.Int(6) == 1,
                    General = dt.Int(6) == 1
                };
                string[] arr = dt.String(2).Split('|');
                def.Items = new List<ShopDef.ShopItem>();
                foreach (string s in arr)
                {
                    ShopItem ing = new ShopItem();
                    var ss = s.Split(',');
                    ing.ItemID = ushort.Parse(ss[0].Split('=')[0]);
                    ing.Amount = int.Parse(ss[0].Split('=')[1]);
                    ing.OriginalAmount = int.Parse(ss[0].Split('=')[1]);
                    ing.RespawnTime = int.Parse(ss[1]);
                    ing.OriginalItem = true;
                    def.Items.Add(ing);
                }

                DataManager.ShopDefinitions[i] = def;
            }
            dt.Destroy();
            Logger.Log("Loaded " + DataManager.ShopDefinitions.Length + " ShopDefs");
        }
    }
}
