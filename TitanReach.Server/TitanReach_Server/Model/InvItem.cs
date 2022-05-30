namespace TitanReach_Server.Model
{
    public class InvItem
    {

        public InvItem(ushort id)
        {
            Item = new Item(id);
        }

        public InvItem(Item item)
        {
            Item = item;
        }

        public Item Item;
    }
}
