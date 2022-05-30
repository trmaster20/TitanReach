using System;
using TitanReach_Server.Model;
using static TitanReach_Server.EventManager;

namespace TitanReach_Server
{

    public class TREvent<T>
    {
        public event EventHandler<T> Event;
        public void Invoke(T param) => Event?.Invoke(this, param);
    }

    public class Events
    {
        public static readonly TREvent<PlayerArg> OnInventoryChange = new();
        public static readonly TREvent<ItemAdd> OnItemAdd = new();


    }
   
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    public class EventManager
    {
        public struct PlayerArg { public Player player; }
        public struct ItemAdd { public Player player; public Item  item; }



        /**
        * Delegates
        */
        public delegate void Flag(Player player); // generic delegate for player events with no further args
        public delegate void ChangedItem(Item item, Player player);
        public delegate void ItemOnItem(Player player, Item item1, Item item2);
        public delegate void ObjectInteraction(Player player, Obj obj, int type, int opt, bool active);
        public delegate void LightFire(Player player, int logID);



        /**
        * Events
        */
        public static event Flag InventoryChangeFlag;
        public static event ChangedItem InventoryAddEvent;
        public static event ChangedItem InventoryRemoveEvent;
        public static event ChangedItem InventoryDropEvent;
        public static event ItemOnItem UseItemOnItemEvent;
        public static event LightFire LightFireEvent;
        public static event ObjectInteraction ObjectInteractionEvent;


        /**
         * Raised Methods
         */
        public static void RaiseObjectInteractionEvent(Player player, Obj obj, int type, int opt, bool active = false)
        {
            ObjectInteractionEvent?.Invoke(player, obj, type, opt, active);
        }
        public static void RaiseUseItemOnItem(Player player, Item item1, Item item2)
        {
            UseItemOnItemEvent?.Invoke(player, item1, item2);
        }
        public static void RaiseItemAdd(Item item, Player player)
        {
            RaiseInventoryChanged(player);
            InventoryAddEvent?.Invoke(item, player);
        }

        public static void RaiseItemRemove(Item item, Player player)
        {
            RaiseInventoryChanged(player);
            InventoryRemoveEvent?.Invoke(item, player);
        }

        public static void RaiseLightFire(Player player, int logID)
        {
            LightFireEvent?.Invoke(player, logID);
        }

        public static void RaiseItemDrop(Item item, Player player)
        {
            RaiseInventoryChanged(player);
            InventoryDropEvent?.Invoke(item, player);
        }

        public static void RaiseInventoryChanged(Player player)
        {
            //RaiseInventoryChanged(player);
            //Events.OnInventoryChange.Invoke(new PlayerArg() { player = player });
            InventoryChangeFlag?.Invoke(player);
        }
    }
}
