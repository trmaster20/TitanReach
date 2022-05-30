using System.Collections.Generic;
using TRShared;

namespace TitanReach_Server.Model
{
    public class Inventory
    {

        public Inventory(Player pl)
        {
            this.player = pl;
        }

        public Player player;
        public InvItem[] items = new InvItem[30];

        public void AddItem(Item item, bool sound = true)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null && item.Definition.MaxStackSize > 1 && item.ID == items[i].Item.ID) // stack
                {
                    if (item.Amount <= 0)
                        return;
                    items[i].Item.Amount += item.Amount;
                    player.NetworkActions.PlayPickupSound(item.ID, false);
                    EventManager.RaiseItemAdd(item, player);
                    Events.OnItemAdd.Invoke(new EventManager.ItemAdd() { player = player, item = item });
                    return;
                }
                if (items[i] == null)
                {
                    items[i] = new InvItem(item);
                    items[i].Item.Amount = item.Amount;
                    if (sound)
                        player.NetworkActions.PlayPickupSound(item.ID, false);
                    EventManager.RaiseItemAdd(item, player);
                    return;
                }
            }
            player.Error("<color=red>Your inventory is full. " + item.Definition.ItemName + " has dropped</color>");
            DropItem(item, true);

        }


        public static bool AddItemV(Item item, InvItem[] itemz)
        {

            for (int i = 0; i < itemz.Length; i++)
            {
                if (itemz[i] != null && item.Definition.MaxStackSize > 1 && item.ID == itemz[i].Item.ID) // stack
                {

                    itemz[i].Item.Amount += item.Amount;
                    return true;
                }
                if (itemz[i] == null)
                {
                    itemz[i] = new InvItem(item);
                    itemz[i].Item.Amount = item.Amount;
                    return true;
                }
            }

            return false;

        }


        public void SpawnGroundItem(Item ite, Vector3 pos, int despawn = 90000) {
            if (ite == null)
                return;

            var gi = new GroundItem(ite.ID, player.MapID);


            if (pos != null) {
                gi.transform = new Transform(pos, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                gi.transform.position.Y = DataManager.GetHeight((int)pos.X, (int)pos.Z, player.Map.Land) + 1f;
            }


            gi.Item.UID = ite.UID;
            gi.Item.Amount = ite.Amount;
            player.Map.GroundItems.Add(gi);
            lock (player.Map.Players)
            {
                foreach (Player pl in player.Map.Players)
                {
                    pl.Viewport.groundItemsInView.Add(gi);
                    pl.NetworkActions.AddGroundItem(gi);
                }
            }

            Server.Instance.Delay(despawn, (timer, arg) =>
            {
                if (gi != null && player.Map.GroundItems.Contains(gi))
                {

                    player.Map.GroundItems.Remove(gi);
                    lock (player.Map.Players)
                    {
                        foreach (Player pl in player.Map.Players)
                        {


                            pl.Viewport.groundItemsInView.Remove(gi);
                            pl.NetworkActions.RemoveGroundItem(gi.groundItemUID);

                        }
                    }
                }
            });
        }

        public void DropItem(Item ite, bool dontRemove = false) // don't remove from inventory. used when adding item to inventory when its full. (drops to floor)
        {
            if (ite == null)
                return;

            var gi = new GroundItem(ite.ID, player.MapID);
            gi.transform = new Transform(new Vector3(player.transform.position.X, DataManager.GetHeight((int)player.transform.position.X, (int)player.transform.position.Z, player.Map.Land) + 0.5f, player.transform.position.Z), null, null);
            gi.Item.Amount = ite.Amount;
            if (!dontRemove)
            {
                InvItem it = GetItem(ite.ID);
                if (it == null)
                  return;
                gi.Item.Amount = it.Item.Amount;
            
                RemoveItem(it);
                player.NetworkActions.SendInventory();
            }
            player.NetworkActions.PlayPickupSound(ite.ID, false);
            gi.Item.UID = ite.UID;
            EventManager.RaiseItemDrop(gi.Item, player);
            player.Map.AddItemToGround(gi, player);

            /*
            Server.Instance.groundItems.Add(gi);
            lock (Server.Instance.players)
            {
                foreach (Player pl in Server.Instance.players)
                {
                    pl.Viewport.groundItemsInView.Add(gi);
                    pl.NetworkActions.AddGroundItem(gi);
                }
            }

            Server.Instance.Delay(120000, (timer, arg) =>
            {
                if (gi != null && Server.Instance.groundItems.Contains(gi))
                {
                    lock (Server.Instance.players)
                    {
                        foreach (Player pl in Server.Instance.players)
                        {
                            pl.NetworkActions.RemoveGroundItem(gi.groundItemUID);
                            pl.Viewport.groundItemsInView.Remove(gi);
                        }
                    }
                    Server.Instance.groundItems.Remove(gi);
                }
            });
            */
        }

        public void RemoveItem(InvItem item)
        {
            RemoveItem(item.Item, item.Item.Amount);
        }

        public bool HasItem(ushort id)
        {
            foreach (InvItem ite in items)
            {
                if (ite == null)
                    continue;
                if (ite.Item.Definition.ItemID == id)
                    return true;
            }
            return false;
        }

        public bool HasItem(Item item)
        {
            int id = item.ID;
            foreach (InvItem ite in items)
            {
                if (ite == null)
                    continue;
                if (ite.Item.Definition.ItemID == id)
                    return true;
            }
            return false;

        }

        public bool HasItem(ushort id, int count)
        {
            int countc = 0;
            foreach (InvItem ite in items)
            {
                if (ite == null)
                    continue;
                if (ite.Item.Definition.ItemID == id)
                {

                    if (ite.Item.Definition.MaxStackSize > 1)
                        return ite.Item.Amount >= count;
                    countc++;
                    continue;
                }
            }
            return countc >= count;
        }



        public static bool HasItemV(ushort id, int count, InvItem[] itemz)
        {
            int countc = 0;
            foreach (InvItem ite in itemz)
            {
                if (ite == null)
                    continue;
                if (ite.Item.Definition.ItemID == id)
                {

                    if (ite.Item.Definition.MaxStackSize > 1)
                        return ite.Item.Amount >= count;
                    countc++;
                    continue;
                }
            }
            return countc >= count;
        }

        public int CountItem(ushort id)
        {
            int countc = 0;
            foreach (InvItem ite in items)
            {
                if (ite == null)
                    continue;
                if (ite.Item.Definition.ItemID == id)
                {

                    if (ite.Item.Definition.MaxStackSize > 1)
                        return ite.Item.Amount;
                    countc++;
                    continue;
                }
            }
            return countc;
        }

        public InvItem GetItem(ushort id)
        {
            foreach (InvItem ite in items)
            {
                if (ite == null)
                    continue;
                if (ite.Item.Definition.ItemID == id)
                {
                    return ite;
                }
            }
            return null;
        }

        public void RemoveItemID(ushort id)
        {
            RemoveItem(id, 1);
        }

        public void RemoveItem(ushort id, int amount, bool quest) {
            RemoveItem(id, amount);
            player.NetworkActions.SendInventory();
            player.NetworkActions.ItemGain(id, -amount);
        }


        public static void RemoveItemV(ushort id, int count, InvItem[] itemz)
        {

            InvItem it = null;
            foreach (InvItem ite in itemz)
            {
                if (ite == null)
                    continue;
                if (ite.Item.Definition.ItemID == id)
                {
                    it = ite;
                    break;
                }
            }
            if (it != null)
            {
                if (count > 1 && it.Item.Definition.MaxStackSize <= 1)
                {
                    for (int i = 0; i < count; i++)
                    {
                        RemoveItemV(it.Item, 1, itemz);
                    }
                }
                else
                    RemoveItemV(it.Item, count, itemz);
            }

        }

        public static void RemoveItemV(Item it, int co, InvItem[] itemz)
        {
            List<InvItem> temp = new List<InvItem>();
            int idx = -1;
            for (int i = 0; i < itemz.Length; i++)
            {
                if (itemz[i] == null)
                    continue;
                if (it.Definition.MaxStackSize > 1)
                {
                    if (itemz[i].Item.ID == it.ID)
                    {
                        itemz[i].Item.Amount -= co;
                        if (itemz[i].Item.Amount >= 1)
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
                else if (itemz[i].Item.ID == it.ID)
                {
                    idx = i;
                    break;
                }
            }
            if (idx != -1)
            {
                itemz[idx] = null;
            }
            //// rebalance inventory
            for (int i = 0; i < itemz.Length; i++)
            {
                if (itemz[i] == null)
                    continue;
                temp.Add(itemz[i]);
                itemz[i] = null;
            }
            int count = 0;
            foreach (InvItem ite in temp)
            {
                itemz[count] = ite;
                count++;
            }
        }

        public void RemoveItem(int id, int count)
        {

            InvItem it = null;
            foreach (InvItem ite in items)
            {
                if (ite == null)
                    continue;
                if (ite.Item.Definition.ItemID == id)
                {
                    it = ite;
                    break;
                }
            }
            if (it != null)
            {
                if (count > 1 && it.Item.Definition.MaxStackSize <= 1)
                {
                    for (int i = 0; i < count; i++)
                    {
                        RemoveItem(it.Item, 1);
                    }
                }
                else
                    RemoveItem(it.Item, count);
            }

        }


        public void RemoveItem(Item it, int co)
        {
            
            var uid = it.UID;
            List<InvItem> temp = new List<InvItem>();
            int idx = -1;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                    continue;
                if (it.Definition.MaxStackSize > 1)
                {
                    if (items[i].Item.ID == it.ID)
                    {
                        items[i].Item.Amount -= co;
                        EventManager.RaiseItemRemove(items[i].Item, player);
                        if (items[i].Item.Amount >= 1)
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
                else if (items[i].Item.ID == it.ID)
                {
                    idx = i;
                    break;
                }
            }
            if (idx != -1)
            {
                EventManager.RaiseItemRemove(items[idx].Item, player);
                items[idx] = null;
            }
            // rebalance inventory
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                    continue;
                temp.Add(items[i]);
                items[i] = null;
            }
            int count = 0;
            foreach (InvItem ite in temp)
            {
                items[count] = ite;
                count++;
            }
        }

        public void AddItem(ushort id)
        {

            AddItem(new InvItem(id).Item, false);
        }



        public void AddItem(ushort id, bool reward)
        {
            AddItem(id);
            player.NetworkActions.ItemGain(id, 1);
            player.NetworkActions.SendInventory();
        }

        public void AddItem(ushort id, int amount, bool reward)
        {
            if (DataManager.ItemDefinitions[id] == null)
                return;
            if (!reward)
                AddItem(id, amount, false, false);
            else
            {
                AddItem(id, amount, true, false);
                player.NetworkActions.ItemGain(id, amount);
                player.NetworkActions.PlayPickupSound(16, true);
                player.NetworkActions.SendInventory();
            }
        }

        public void AddItem(ushort id, int count, bool sound = true, bool donttouch = false)
        {
            if(count == 0) {
                return;
            }


            InvItem ite = new InvItem(id);


            if (ite.Item.Definition.MaxStackSize > 1 )
                ite.Item.Amount = count;
            else if (count > 1)
                for (int i = 0; i < count - 1; i++)
                    AddItem(id);



            AddItem(ite.Item, sound);
        }


        public bool HasRoom(int count)
        {
            return Count() + count <= 30;
        }

        public int FreeSpace()
        {
            return 30 - Count();
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
