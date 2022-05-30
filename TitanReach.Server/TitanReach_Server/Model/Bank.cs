using System.Collections.Generic;
using TRShared;

namespace TitanReach_Server.Model
{
    public class Bank
    {

        public Bank(Player pl)
        {
            this.player = pl;
        }

        public Player player;
        public Item[] items = new Item[short.MaxValue];

        public void AddItem(Item item)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null && item.ID == items[i].ID) // autostack since its a bank
                {
                    items[i].Amount += item.Amount;
                    return;
                }
                if (items[i] == null)
                {
                    // Server.Log("new item created in bank");
                    items[i] = item;
                    return;
                }
            }
            Server.Log("Unable to place: " + item.Definition.ItemName);

            //player.NetworkActions.SendMessage("<color=red>Your inventory is full. " + item.Definition.ItemName + " has dropped</color>");
            // DropItem(item);

        }


        public void RemoveItem(InvItem item)
        {
            RemoveItem(item.Item, item.Item.Amount);
        }



        public bool HasItem(int id, int count)
        {
            foreach (Item ite in items)
            {
                if (ite == null)
                    continue;
                if (ite.Definition.ItemID == id)
                {
                    return ite.Amount >= count;


                }
            }
            return false;
        }

        public Item GetItem(int id)
        {
            foreach (Item ite in items)
            {
                if (ite == null)
                    continue;
                if (ite.Definition.ItemID == id)
                {
                    return ite;
                }
            }
            return null;
        }

        public void RemoveItemID(int id)
        {
            RemoveItemID(id, 1);
        }

        public void RemoveItemID(int id, int count)
        {

            Item it = null;
            foreach (Item ite in items)
            {
                if (ite == null)
                    continue;
                if (ite.Definition.ItemID == id)
                {
                    it = ite;
                    break;
                }
            }
            if (it != null)
                RemoveItem(it, count);
            //else
            //    Server.Log("null item");


        }


        public void RemoveItem(Item it, int c)
        {
            var uid = it.UID;
            List<Item> temp = new List<Item>();
            int idx = -1;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                    continue;

                if (items[i].ID == it.ID)
                {

                    items[i].Amount -= c;

                    if (items[i].Amount >= 1)
                    {


                        return;
                    }
                    else
                    {
                        idx = i;
                        break;
                    }
                }


            }
            if (idx != -1)
            {
                items[idx] = null;
            }
            // rebalance bank
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                    continue;
                temp.Add(items[i]);
                items[i] = null;
            }
            int count = 0;
            foreach (Item ite in temp)
            {
                items[count] = ite;
                count++;
            }
        }

        public void AddItem(ushort id)
        {

            AddItem(new Item(id));
        }

        public void AddItem(ushort id, int count)
        {
            if (DataManager.ItemDefinitions[id] == null)
                return;
            Item ite = new Item(id);

            ite.Amount = count;

            AddItem(ite);
        }


        public bool HasRoom(int count)
        {
            return Count() + count <= items.Length;
        }



        public int Count()
        {
            int count = 0;
            foreach (var item in items)
            {
                if (item != null)
                    count++;
            }
            return count;
        }
    }
}
