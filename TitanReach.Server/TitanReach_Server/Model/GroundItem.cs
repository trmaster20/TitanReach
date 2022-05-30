namespace TitanReach_Server.Model
{
    public class GroundItem
    {

        public static int GROUND_ITEM_TOTAL_COUNT = 1;
        public Transform transform;
        public int groundItemUID;
        public bool Taken = false;
        public uint ownerUID = 0;
        public int MapID = 0;

        public GroundItem(ushort id, int MapID)
        {
            Item = new Item(id);
            this.MapID = MapID;
            GROUND_ITEM_TOTAL_COUNT++;
            groundItemUID = GROUND_ITEM_TOTAL_COUNT;
        }

        public GroundItem(Item item, int MapID)
        {
            Item = item;
            this.MapID = MapID;
            GROUND_ITEM_TOTAL_COUNT++;
            groundItemUID = GROUND_ITEM_TOTAL_COUNT;
        }

        public Item Item;


    }
}
